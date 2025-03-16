using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Domain.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
            ("Введите зарплату:", (criteria, value) => criteria.Salary = decimal.TryParse(value, out var salary) ? salary : (decimal?)null),
            ("Введите опыт работы:", (criteria, value) => criteria.Experience = int.TryParse(value, out var experience) ? experience : (int?)null),
            ("Введите тип занятости:", (criteria, value) => criteria.WorkType = value),
            ("Введите график работы:", (criteria, value) => criteria.Schedule = value),
            ("Доступно ли для людей с инвалидностью? (да/нет):", (criteria, value) => criteria.Disability = value.ToLower() == "да"),
            ("Доступно ли для детей с 14 лет? (да/нет):", (criteria, value) => criteria.ForChildren = value.ToLower() == "да")
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

                var userId = message.From.Id;
                var chatId = message.Chat.Id;
                var username = message.From.Username ?? "anonymous";
                var firstName = message.From.FirstName;

                _logger.LogInformation($"Получено сообщение от {username} (User ID: {userId}, Chat ID: {chatId}): {message.Text}");

                var user = await _userRepository.GetUserAsync(userId);

                // Если пользователь новый
                if (user == null)
                {
                    user = new Domain.DataAccess.Model.User { TelegramId = userId };
                    await _userRepository.AddOrUpdateUserAsync(user);

                    // Отправляем приветственное сообщение
                    await client.SendMessage(
                        chatId: chatId,
                        text: "Команда Безработных.NET приветствует Вас!" +
                              "Давайте найдем вакансию Вашей мечты вместе. Приступим!)",
                        cancellationToken: cancellationToken);

                    // Показываем меню
                    await ShowMainMenu(client, chatId, cancellationToken);
                    return;
                }

                if (user.State != UserState.None && DateTime.UtcNow - user.LastUpdated > TimeSpan.FromMinutes(10))
                {
                    await CancelScenario(client, message, user, cancellationToken);
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

        private async Task ShowMainMenu(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("Поиск вакансий") },
                new[] { new KeyboardButton("Мои критерии поиска") },
                new[] { new KeyboardButton("Подписка") }
            })
            {
                ResizeKeyboard = true, // Маленькие адаптивные кнопочки
                OneTimeKeyboard = false // Скрыть клаву
            };

            await client.SendMessage(
                chatId: chatId,
                text: "Выберите действие:",
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken);
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

            switch (messageText)
            {
                case "Поиск вакансий":
                    await StartScenario(client, message, user, cancellationToken);
                    break;
                case "Мои критерии поиска":
                    await ShowUserCriteria(client, message, user, cancellationToken);
                    break;
                case "Подписка":
                    await HandleSubscription(client, message, user, cancellationToken);
                    break;
                default:
                    // Обработка неизвестных команд
                    await ShowMainMenu(client, message.Chat.Id, cancellationToken);
                    break;
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

            await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
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

            await client.SendMessage(
                chatId: message.Chat.Id,
                text: "Сценарий отменен.",
                replyMarkup: new ReplyKeyboardRemove(), // Скрываем клавиатуру, если закончили
                cancellationToken: cancellationToken);

        }

        /// <summary>
        /// Шаг сценария в виде кнопок.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="chatId"></param>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SendStepMessage(ITelegramBotClient client, long chatId, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            var step = _criteriaSteps[user.CurrentCriteriaStep];

            var replyKeyboard = step.Prompt switch
            {
                string prompt when prompt.StartsWith("Введите регион:") => new ReplyKeyboardMarkup(new[]
                {
            new[] { new KeyboardButton("Москва"), new KeyboardButton("Санкт-Петербург") },
            new[] { new KeyboardButton("Калининград"), new KeyboardButton("Уфа") },
            new[] { new KeyboardButton("Свое значение") },
            new[] { new KeyboardButton("Вернуться в меню") }
        }),
                string prompt when prompt.StartsWith("Введите должность:") => new ReplyKeyboardMarkup(new[]
                {
            new[] { new KeyboardButton("Грузчик"), new KeyboardButton("Директор") },
            new[] { new KeyboardButton("Менеджер"), new KeyboardButton("Разработчик") },
            new[] { new KeyboardButton("Свое значение") },
            new[] { new KeyboardButton("Вернуться в меню") }
        }),
                _ => new ReplyKeyboardMarkup(new[]
                {
            new[] { new KeyboardButton("Вернуться в меню") }
        })
            };

            replyKeyboard.ResizeKeyboard = true;
            replyKeyboard.OneTimeKeyboard = true;

            await client.SendMessage(
                chatId: chatId,
                text: step.Prompt,
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken);
        }

        private async Task HandleCriteriaInput(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (message.Text == "Вернуться в меню")
            {
                await CancelScenario(client, message, user, cancellationToken);
                await ShowMainMenu(client, message.Chat.Id, cancellationToken);
                return;
            }

            if (user.Criteria == null)
            {
                user.Criteria = new Criteria
                {
                    UserId = user.Id
                };
            }

            var step = _criteriaSteps[user.CurrentCriteriaStep];

            if (message.Text == "Свое значение")
            {
                await client.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Введите значение:",
                    cancellationToken: cancellationToken);
                return;
            }

            step.SetValue(user.Criteria, message.Text);

            user.CurrentCriteriaStep++;
            user.LastUpdated = DateTime.UtcNow;
            await _userRepository.AddOrUpdateUserAsync(user);

            if (user.CurrentCriteriaStep >= _criteriaSteps.Count)
            {
                user.State = UserState.None;

                await client.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Ввод критериев завершен. Вы можете отредактировать их позже.",
                    cancellationToken: cancellationToken);

                await ShowMainMenu(client, message.Chat.Id, cancellationToken);

                return;
            }

            await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
        }

        private async Task HandleSubscription(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            await client.SendMessage(
                chatId: message.Chat.Id,
                text: "Функция подписки пока недоступна.",
                cancellationToken: cancellationToken);

            await ShowMainMenu(client, message.Chat.Id, cancellationToken);
        }

        private async Task ShowUserCriteria(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (user.Criteria == null)
            {
                await client.SendMessage(
                    chatId: message.Chat.Id,
                    text: "У вас пока нет сохраненных критериев.",
                    cancellationToken: cancellationToken);
                return;
            }

            var criteriaText = $"Регион: {user.Criteria.Region}\n" +
                               $"Должность: {user.Criteria.Post}\n" +
                               $"Зарплата: {user.Criteria.Salary}\n" +
                               $"Опыт работы: {user.Criteria.Experience}\n" +
                               $"Тип занятости: {user.Criteria.WorkType}\n" +
                               $"График работы: {user.Criteria.Schedule}\n" +
                               $"Для людей с инвалидностью: {(user.Criteria.Disability ? "Да" : "Нет")}\n" +
                               $"Для детей с 14 лет: {(user.Criteria.ForChildren ? "Да" : "Нет")}";

            await client.SendMessage(
                chatId: message.Chat.Id,
                text: criteriaText,
                cancellationToken: cancellationToken);

            await ShowMainMenu(client, message.Chat.Id, cancellationToken);
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
    }
}

