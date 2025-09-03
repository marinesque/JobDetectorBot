using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Domain.Enums;
using Bot.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Application.Strategies
{
    public class AwaitingCriteriaStrategy : IUserStateStrategy
    {
        private readonly ILogger<AwaitingCriteriaStrategy> _logger;
        private readonly IUserCacheService _userCacheService;
        private readonly CriteriaStepRepository _criteriaStepRepository;
        private readonly BotDbContext _context;
        private List<CriteriaStep> _criteriaSteps;

        public AwaitingCriteriaStrategy(
            ILogger<AwaitingCriteriaStrategy> logger,
            IUserCacheService userCacheService,
            CriteriaStepRepository criteriaStepRepository,
            BotDbContext context)
        {
            _logger = logger;
            _userCacheService = userCacheService;
            _criteriaStepRepository = criteriaStepRepository;
            _context = context;
        }

        public bool CanHandle(UserState state) =>
            state == UserState.AwaitingCriteria ||
            state == UserState.AwaitingCriteriaEdit ||
            state == UserState.AwaitingCustomValue;

        public async Task HandleAsync(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (_criteriaSteps == null || !_criteriaSteps.Any())
            {
                await LoadCriteriaStepsAsync();
            }

            switch (message.Text)
            {
                case "Начать новый поиск":
                    await StartScenario(client, message, user, cancellationToken);
                    break;
                case "Мои критерии поиска":
                    await ShowUserCriteria(client, message, user, cancellationToken);
                    break;
                default:
                    // Обработка неизвестных команд
                    await HandleCriteriaInput(client, message, user, cancellationToken);
                    break;
            }

            //await HandleCriteriaInput(client, message, user, cancellationToken);
        }

        private async Task StartScenario(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (_criteriaSteps == null || !_criteriaSteps.Any())
            {
                await LoadCriteriaStepsAsync();
            }

            user.State = UserState.AwaitingCriteria; // Вводим критерии
            user.CurrentCriteriaStep = 0; // Шаг
            user.CurrentCriteriaStepValueIndex = 0; // Страница ответа
            user.LastUpdated = DateTime.UtcNow;
            await _userCacheService.SetUserAsync(user);
            await _userCacheService.SyncToDatabaseAsync(user);
            //await _userRepository.AddOrUpdateUserAsync(user);

            await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
        }

        private async Task HandleCriteriaInput(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)

        {
            if (_criteriaSteps == null || !_criteriaSteps.Any())
            {
                await LoadCriteriaStepsAsync();
            }

            if (message.Text == "Вернуться в меню")
            {
                user.IsSingle = false;
                user.LastUpdated = DateTime.UtcNow;
                await _userCacheService.SetUserAsync(user);
                //await _userRepository.AddOrUpdateUserAsync(user);
                await CancelScenario(client, message, user, cancellationToken);
                await ShowMainMenu(client, message.Chat.Id, cancellationToken);
                return;
            }

            if (message.Text == "Назад")
            {
                user.CurrentCriteriaStepValueIndex = Math.Max(0, user.CurrentCriteriaStepValueIndex - 4);
                user.LastUpdated = DateTime.UtcNow;
                await _userCacheService.SetUserAsync(user);
                //await _userRepository.AddOrUpdateUserAsync(user);
                await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
                return;
            }

            if (message.Text == "Далее")
            {
                if (user.State == UserState.SearchingVacancies)
                    user.CurrentCriteriaStepValueIndex += 1;
                else user.CurrentCriteriaStepValueIndex += 4;
                user.LastUpdated = DateTime.UtcNow;
                await _userCacheService.SetUserAsync(user);
                //await _userRepository.AddOrUpdateUserAsync(user);
                await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
                return;
            }

            if (message.Text == "Свое значение")
            {
                var currentStep = _criteriaSteps[user.CurrentCriteriaStep];
                if (currentStep.IsCustom)
                {
                    user.State = UserState.AwaitingCustomValue;
                    user.LastUpdated = DateTime.UtcNow;
                    await _userCacheService.SetUserAsync(user);
                    //await _userRepository.AddOrUpdateUserAsync(user);

                    await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Пожалуйста, введите свое значение:",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
                    return;
                }

                await client.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Ввод своего значения недоступен для этого шага.",
                    cancellationToken: cancellationToken);
                await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
                return;
            }

            var step = _criteriaSteps[user.CurrentCriteriaStep];

            var criteriaStep = await _context.CriteriaSteps
                .Include(cs => cs.CriteriaStepValues)
                .FirstOrDefaultAsync(cs => cs.Id == step.Id);

            if (criteriaStep == null)
            {
                await client.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Ошибка: критерий поиска не найден.",
                    cancellationToken: cancellationToken);
                return;
            }

            // Состояние ожидания кастомного значения критерия
            if (user.State == UserState.AwaitingCustomValue)
            {
                //await _userRepository.AddOrUpdateUserCriteriaAsync(
                //    user.Id,
                //    step.Id,
                //    null, // Нет выбранного значения из списка
                //    message.Text); // Сохраняем текст как кастомное значение
                user = await _userCacheService.UpdateUserCriteriaAsync(
                    user.TelegramId,
                    step.Id,
                    null, // Нет выбранного значения из списка
                    message.Text); // Сохраняем текст как кастомное значение

                user.State = UserState.AwaitingCriteria;
                user.LastUpdated = DateTime.UtcNow;
                await _userCacheService.SetUserAsync(user);
                //await _userRepository.AddOrUpdateUserAsync(user);
            }
            else
            {
                var selectedValue = criteriaStep.CriteriaStepValues
                    .FirstOrDefault(csv => csv.Prompt == message.Text);

                if (selectedValue == null && !step.IsCustom)
                {
                    // Если значение не найдено и IsCustom = false, повторяем запрос
                    await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Пожалуйста, выберите значение из предложенных.",
                        cancellationToken: cancellationToken);
                    await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
                    return;
                }

                //await _userRepository.AddOrUpdateUserCriteriaAsync(
                //user.Id,
                //step.Id,
                //selectedValue?.Id);
                user = await _userCacheService.UpdateUserCriteriaAsync(
                    user.TelegramId,
                    step.Id,
                    selectedValue?.Id);
            }

            // Состояние точечной модификации критерия
            if (user.State == UserState.AwaitingCriteriaEdit || user.IsSingle == true)
            {
                await ResetUserStateAsync(user);
                await ShowUserCriteria(client, message, user, cancellationToken);
            }
            else
            {
                user.CurrentCriteriaStep++;
                user.CurrentCriteriaStepValueIndex = 0;
                user.LastUpdated = DateTime.UtcNow;
                await _userCacheService.SetUserAsync(user);
                //await _userRepository.AddOrUpdateUserAsync(user);

                if (user.CurrentCriteriaStep >= _criteriaSteps.Count)
                {
                    await ResetUserStateAsync(user);

                    await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Ввод критериев завершен. Вы можете отредактировать их позже.",
                        cancellationToken: cancellationToken);

                    await ShowMainMenu(client, message.Chat.Id, cancellationToken);
                    return;
                }

                await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
            }
        }

        private async Task SendStepMessage(ITelegramBotClient client, long chatId, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            var step = _criteriaSteps[user.CurrentCriteriaStep];

            var criteriaStep = await _context.CriteriaSteps
                .Include(cs => cs.CriteriaStepValues)
                .FirstOrDefaultAsync(cs => cs.Prompt == step.Prompt);

            if (criteriaStep == null)
            {
                await client.SendMessage(
                    chatId,
                    "Ошибка: шаг сценария не найден.",
                    cancellationToken: cancellationToken);
                return;
            }

            var sortedValues = criteriaStep.CriteriaStepValues
                .OrderBy(csv => csv.OrderBy.HasValue ? 0 : 1) // Сначала значения с заполненным OrderBy
                .ThenBy(csv => csv.OrderBy) // Затем сортируем по OrderBy
                .ThenBy(csv => ConvertValue(csv.Value, criteriaStep.Type)) // Затем по Value, сконвертированному в нужный тип
                .Skip(user.CurrentCriteriaStepValueIndex)
                .Take(4)
                .ToList();

            var replyKeyboardButtonList = new List<KeyboardButton[]>();
            var row = new List<KeyboardButton>();

            foreach (var value in sortedValues)
            {
                row.Add(new KeyboardButton(value.Prompt)); // Используем Prompt для отображения
                if (row.Count == 2)
                {
                    replyKeyboardButtonList.Add(row.ToArray());
                    row.Clear();
                }
            }

            if (row.Any())
            {
                replyKeyboardButtonList.Add(row.ToArray());
            }

            if (criteriaStep.CriteriaStepValues.Count > 4)
            {
                var navigationRow = new List<KeyboardButton>();

                if (user.CurrentCriteriaStepValueIndex > 0)
                {
                    navigationRow.Add(new KeyboardButton("Назад"));
                }

                if (user.CurrentCriteriaStepValueIndex + 4 < criteriaStep.CriteriaStepValues.Count)
                {
                    navigationRow.Add(new KeyboardButton("Далее"));
                }

                if (navigationRow.Any())
                {
                    replyKeyboardButtonList.Add(navigationRow.ToArray());
                }
            }

            if (criteriaStep.IsCustom)
            {
                replyKeyboardButtonList.Add(new[] { new KeyboardButton("Свое значение") });
            }

            replyKeyboardButtonList.Add(new[] { new KeyboardButton("Вернуться в меню") });

            var replyKeyboard = new ReplyKeyboardMarkup(replyKeyboardButtonList)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };

            await client.SendMessage(
                chatId: chatId,
                text: step.Prompt,
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken);
        }

        private object ConvertValue(string value, string type)
        {
            return type.ToLower() switch
            {
                "string" => value,
                "int" => int.TryParse(value, out var intValue) ? intValue : throw new ArgumentException($"Невозможно преобразовать '{value}' в int."),
                "decimal" => decimal.TryParse(value, out var decimalValue) ? decimalValue : throw new ArgumentException($"Невозможно преобразовать '{value}' в decimal."),
                "boolean" => value.ToLower() switch
                {
                    "да" or "true" => true,
                    "нет" or "false" => false,
                    _ => throw new ArgumentException($"Невозможно преобразовать '{value}' в bool.")
                },
                _ => throw new ArgumentException($"Неизвестный тип данных: {type}")
            };
        }

        private async Task CancelScenario(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            await ResetUserStateAsync(user);
            await client.SendMessage(
                chatId: message.Chat.Id,
                text: "Операция отменена.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
            await ShowMainMenu(client, message.Chat.Id, cancellationToken);
        }

        private async Task ShowMainMenu(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("Начать новый поиск") },
                new[] { new KeyboardButton("Искать вакансии") },
                new[] { new KeyboardButton("Мои критерии поиска") },
                new[] { new KeyboardButton("Вернуться в меню") }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };

            await client.SendMessage(
                chatId: chatId,
                text: "Выберите действие:",
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken);
        }

        private async Task LoadCriteriaStepsAsync()
        {
            _criteriaSteps = await _criteriaStepRepository.GetAllCriteriaStepsAsync();
        }

        private async Task ResetUserStateAsync(Domain.DataAccess.Model.User user)
        {
            user.State = UserState.None;
            user.CurrentCriteriaStep = 0;
            user.CurrentCriteriaStepValueIndex = 0;
            user.LastUpdated = DateTime.UtcNow;
            user.IsSingle = false;
            await _userCacheService.SetUserAsync(user);
            await _userCacheService.SyncToDatabaseAsync(user);
        }

        private async Task ShowUserCriteria(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (_criteriaSteps == null || !_criteriaSteps.Any())
            {
                await LoadCriteriaStepsAsync();
            }

            if (user.UserCriteriaStepValues.Count == 0)
            {
                await client.SendMessage(
                    chatId: message.Chat.Id,
                    text: "У вас пока нет сохраненных критериев.",
                    replyMarkup: new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("Вернуться в меню") } })
                    {
                        ResizeKeyboard = true
                    },
                    cancellationToken: cancellationToken);
                return;
            }

            var criteriaTextBuilder = new StringBuilder();

            foreach (var criteriaStep in _criteriaSteps.OrderBy(cs => cs.OrderBy))
            {
                var userValue = user.UserCriteriaStepValues.FirstOrDefault(ucsv => ucsv.CriteriaStepId == criteriaStep.Id);

                if (userValue != null)
                {
                    string valueText;

                    if (userValue.CustomValue != null)
                    {
                        valueText = userValue.CustomValue;
                    }
                    else if (userValue.CriteriaStepValue != null)
                    {
                        valueText = userValue.CriteriaStepValue.Prompt ?? "Не указано";
                    }
                    else
                    {
                        valueText = "Не указано";
                    }

                    criteriaTextBuilder.AppendLine($"{criteriaStep.Prompt}: {valueText}");
                }
            }

            await client.SendMessage(
                chatId: message.Chat.Id,
                text: criteriaTextBuilder.ToString(),
                cancellationToken: cancellationToken);

            var inlineKeyboard = new InlineKeyboardMarkup(
                _criteriaSteps.Select(step =>
                    new[] { InlineKeyboardButton.WithCallbackData(step.Prompt, $"edit:{step.Name}") })
                .ToArray()
            );

            await client.SendMessage(
                chatId: message.Chat.Id,
                text: "Выберите критерий для редактирования:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);

            await client.SendMessage(
                chatId: message.Chat.Id,
                text: "Или вернитесь в меню:",
                replyMarkup: new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("Вернуться в меню") } })
                {
                    ResizeKeyboard = true
                },
                cancellationToken: cancellationToken);
        }
    }
}