using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Domain.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Application.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private static readonly string _botImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "BotImages");
        private readonly UserRepository _userRepository;

        private readonly List<(string Prompt, Action<Criteria, string> SetValue)> _criteriaSteps = new()
        {
            ("Введите регион:", (criteria, value) => criteria.Region = value),
            ("Введите должность:", (criteria, value) => criteria.Post = value),
            ("Введите зарплату (или /skip):", (criteria, value) => criteria.Salary = decimal.TryParse(value, out var salary) ? salary : (decimal?)null),
            ("Введите опыт работы (или /skip):", (criteria, value) => criteria.Experience = int.TryParse(value, out var experience) ? experience : (int?)null),
            ("Введите тип занятости (или /skip):", (criteria, value) => criteria.WorkType = value),
            ("Введите график работы (или /skip):", (criteria, value) => criteria.Schedule = value),
            ("Доступно ли для людей с инвалидностью? (да/нет или /skip):", (criteria, value) => criteria.Disability = value.ToLower() == "да"),
            ("Доступно ли для детей с 14 лет? (да/нет или /skip):", (criteria, value) => criteria.ForChildren = value.ToLower() == "да")
        };

        static MessageHandler()
        {
            if (!Directory.Exists(_botImagesPath))
            {
                Directory.CreateDirectory(_botImagesPath);
            }
        }

        public MessageHandler(ILogger<MessageHandler> logger, UserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task HandleMessageAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is not { } message) return;

                // Уникальный идентификатор пользователя
                var userId = message.From.Id;

                // Уникальный идентификатор чата
                var chatId = message.Chat.Id;

                // Никнейм пользователя (если есть)
                var username = message.From.Username;

                // Имя пользователя
                var firstName = message.From.FirstName;

                _logger.LogInformation($"Получено сообщение от {username ?? "anonymous"} (User ID: {userId}, Chat ID: {chatId}): {message.Text}");

                // Получаем пользователя из базы данных
                var user = await _userRepository.GetUserAsync(userId) ?? new Domain.DataAccess.Model.User { TelegramId = userId };

                // Проверка тайм-аута сценария
                if (user.State != UserState.None && DateTime.UtcNow - user.LastUpdated > TimeSpan.FromMinutes(1))
                {
                    user.State = UserState.None;
                    await _userRepository.AddOrUpdateUserAsync(user);
                    await client.SendMessage(chatId, "Сценарий прерван из-за неактивности.", cancellationToken: cancellationToken);
                    return;
                }

                // Обработка в зависимости от состояния пользователя
                switch (user.State)
                {
                    case UserState.AwaitingCriteria:
                        await HandleCriteriaInput(client, message, user, cancellationToken);
                        break;
                    case UserState.AwaitingCriteriaEdit:
                        await HandleCriteriaEdit(client, message, user, cancellationToken);
                        break;
                    default:
                        await HandleDefaultInput(client, message, user, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "В процессе обработки сообщения возникла ошибка");
            }
        }

        /// <summary>
        /// Сценарий ввода критериев
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task StartScenario(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            user.State = UserState.AwaitingCriteria; // Вводим критерии
            user.CurrentCriteriaStep = 0; // Шаг
            user.LastUpdated = DateTime.UtcNow;
            await _userRepository.AddOrUpdateUserAsync(user);

            await client.SendMessage(message.Chat.Id, _criteriaSteps[user.CurrentCriteriaStep].Prompt, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Окончание сценария
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CancelScenario(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            user.State = UserState.None;
            await _userRepository.AddOrUpdateUserAsync(user);

            await client.SendMessage(message.Chat.Id, "Сценарий отменен.", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Любое рандомное сообщение
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleDefaultInput(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (message.Text is not { } messageText)
                return;

            await (messageText.Split(' ')[0] switch
            {
                "/search" => StartScenario(client, message, user, cancellationToken),
                "/cancel" => CancelScenario(client, message, user, cancellationToken),
                "/done" => HandleCriteriaInput(client, message, user, cancellationToken),
                "/skip" => HandleCriteriaInput(client, message, user, cancellationToken),
                "/edit" => HandleEditCommand(client, message, user, cancellationToken),
                _ => HandleDefaultMessage(client, message, cancellationToken)
            });
        }

        /// <summary>
        /// Обработка команды /edit
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleEditCommand(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            user.State = UserState.AwaitingCriteriaEdit;
            await _userRepository.AddOrUpdateUserAsync(user);

            var criteriaList = string.Join("\n", _criteriaSteps.Select((step, index) => $"{index + 1}. {step.Prompt}"));
            await client.SendMessage(message.Chat.Id, $"Выберите критерий для редактирования:\n{criteriaList}", cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Обработка редактирования критериев
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleCriteriaEdit(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (int.TryParse(message.Text, out var stepNumber) && stepNumber >= 1 && stepNumber <= _criteriaSteps.Count)
            {
                user.CurrentCriteriaStep = stepNumber - 1; // Переходим к выбранному шагу
                await client.SendMessage(message.Chat.Id, _criteriaSteps[user.CurrentCriteriaStep].Prompt, cancellationToken: cancellationToken);
            }
            else
            {
                await client.SendMessage(message.Chat.Id, "Пожалуйста, введите номер критерия для редактирования (от 1 до 8).", cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Обработка ввода критериев
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleCriteriaInput(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            // Завершим ввод критериев последовательно или выборочно через отдельную команду
            if (message.Text == "/done")
            {
                user.State = UserState.None;
                await _userRepository.AddOrUpdateUserAsync(user);
                await client.SendMessage(
                    message.Chat.Id,
                    "Ввод критериев завершен. Вы можете отредактировать их позже с помощью команды /edit.",
                    cancellationToken: cancellationToken);
                return;
            }

            //Пропустить шаг
            if (message.Text == "/skip")
            {
                user.CurrentCriteriaStep++;
                if (user.CurrentCriteriaStep >= _criteriaSteps.Count)
                {
                    user.State = UserState.None;
                    await _userRepository.AddOrUpdateUserAsync(user);
                    await client.SendMessage(
                        message.Chat.Id,
                        "Ввод критериев завершен. Вы можете отредактировать их позже с помощью команды /edit.",
                        cancellationToken: cancellationToken);
                    return;
                }

                await client.SendMessage(
                    message.Chat.Id,
                    _criteriaSteps[user.CurrentCriteriaStep].Prompt,
                    cancellationToken: cancellationToken);
                return;
            }

            // Обрабатываем ввод данных
            var step = _criteriaSteps[user.CurrentCriteriaStep];
            step.SetValue(user.Criteria, message.Text);

            user.CurrentCriteriaStep++;
            await _userRepository.AddOrUpdateUserAsync(user);

            if (user.CurrentCriteriaStep >= _criteriaSteps.Count)
            {
                user.State = UserState.None;
                await client.SendMessage(
                    message.Chat.Id,
                    "Ввод критериев завершен. Вы можете отредактировать их позже с помощью команды /edit.",
                    cancellationToken: cancellationToken);
                return;
            }

            await client.SendMessage(message.Chat.Id, _criteriaSteps[user.CurrentCriteriaStep].Prompt, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Обработка любого рандомного сообщения
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleDefaultMessage(ITelegramBotClient client, Message message, CancellationToken cancellationToken)
        {
            await client.SendMessage(message.Chat.Id, "Добрый вечер!", cancellationToken: cancellationToken);
        }
    }
}

