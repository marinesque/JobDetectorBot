using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Domain.Enums;
using Bot.Domain.Request.VacancySearch;
using Bot.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Bot.Domain.DataAccess.Model.User;

namespace Bot.Application.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private static readonly string _botImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "BotImages");
        private readonly UserRepository _userRepository;
        private readonly CriteriaStepRepository _criteriaStepRepository;
        private readonly BotDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUserCacheService _userCacheService;
        private readonly IVacancySearchService _vacancyService;

        private List<CriteriaStep> _criteriaSteps;

        static MessageHandler()
        {
            if (!Directory.Exists(_botImagesPath))
            {
                Directory.CreateDirectory(_botImagesPath);
            }
        }

        public MessageHandler(
            ILogger<MessageHandler> logger,
            UserRepository userRepository,
            CriteriaStepRepository criteriaStepRepository,
            BotDbContext context,
            IServiceScopeFactory serviceScopeFactory,
            IUserCacheService userCacheService,
            IVacancySearchService vacancySearchService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _context = context;
            _criteriaStepRepository = criteriaStepRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _userCacheService = userCacheService;
            _vacancyService = vacancySearchService;
        }

        //private List<CriteriaStep> GetCriteriaSteps() => _criteriaStepsActualizer.GetCriteriaSteps();
        private async Task LoadCriteriaStepsAsync() => this._criteriaSteps = await _criteriaStepRepository.GetAllCriteriaStepsAsync();


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

                var user = await _userCacheService.GetUserAsync(userId);

                if (user == null)
                {
                    user = await _userRepository.GetUserAsync(userId);
                }

                if (user == null)
                {
                    user = new Domain.DataAccess.Model.User
                    {
                        TelegramId = userId,
                        UserCriteriaStepValues = new List<UserCriteriaStepValue>(),
                        LastUpdated = DateTime.UtcNow
                    };

                    await _userCacheService.SetUserAsync(user);
                    await _userCacheService.SyncToDatabaseAsync(user);
                    //await _userRepository.AddOrUpdateUserAsync(user);

                    await client.SendMessage(
                        chatId: chatId,
                        text: "Команда Безработных.NET приветствует Вас!" +
                              "Давайте найдем вакансию Вашей мечты вместе. Приступим!)",
                        cancellationToken: cancellationToken);

                    await ShowMainMenu(client, chatId, cancellationToken);

                    return;
                }

                if (user.State != UserState.None && DateTime.UtcNow - user.LastUpdated > TimeSpan.FromMinutes(10))
                {
                    await _userCacheService.SyncToDatabaseAsync(user);
                    await CancelScenario(client, message, user, cancellationToken);
                    return;
                }

                switch (user.State)
                {
                    case UserState.AwaitingCriteria:
                    case UserState.AwaitingCriteriaEdit:
                    case UserState.AwaitingCustomValue:
                    case UserState.SearchingVacancies:
                        await HandleCriteriaInput(client, message, user, cancellationToken);
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

        private async Task HandleDefaultInput(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (message.Text is not { } messageText)
                return;

            switch (messageText)
            {
                case "Начать новый поиск":
                    await StartScenario(client, message, user, cancellationToken);
                    break;
                case "Искать вакансии":
                    await SearchVacancies(client, message, user, cancellationToken);
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

        private async Task CancelScenario(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            var output = user.State switch
            {
                UserState.AwaitingCriteria or UserState.AwaitingCriteriaEdit or UserState.AwaitingCustomValue
                    => "Ввод критериев завершен.",
                UserState.SearchingVacancies
                    => "Поиск вакансий завершен.",
                _ => string.Empty
            };

            await ResetUserStateAsync(user);

            await client.SendMessage(
                chatId: message.Chat.Id,
                text: output,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);

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

        // Метод для конвертации значения в нужный тип
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
                if (user.State == UserState.SearchingVacancies)
                    user.CurrentCriteriaStepValueIndex -= 1;
                else user.CurrentCriteriaStepValueIndex = Math.Max(0, user.CurrentCriteriaStepValueIndex - 4);
                user.LastUpdated = DateTime.UtcNow;
                await _userCacheService.SetUserAsync(user);
                //await _userRepository.AddOrUpdateUserAsync(user);
                if (user.State == UserState.SearchingVacancies)
                    await SearchVacancies(client, message, user, cancellationToken);
                else await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
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
                if (user.State == UserState.SearchingVacancies)
                    await SearchVacancies(client, message, user, cancellationToken);
                else await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
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

        private async Task ResetUserStateAsync(Domain.DataAccess.Model.User user)
        {
            // Обнуляем состояние юзера
            user.State = UserState.None;
            user.CurrentCriteriaStep = 0;
            user.CurrentCriteriaStepValueIndex = 0;
            user.LastUpdated = DateTime.UtcNow;
            user.IsSingle = false;
            await _userCacheService.SetUserAsync(user);
            await _userCacheService.SyncToDatabaseAsync(user);
            //await _userRepository.AddOrUpdateUserAsync(user);
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

        public async Task HandleCallbackQueryAsync(ITelegramBotClient client, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (_criteriaSteps == null || !_criteriaSteps.Any())
            {
                await LoadCriteriaStepsAsync();
            }

            if (callbackQuery.Data.StartsWith("edit:"))
            {
                await client.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

                var criteriaName = callbackQuery.Data.Split(':')[1];
                var stepIndex = _criteriaSteps.FindIndex(cs => cs.Name == criteriaName);

                if (stepIndex >= 0)
                {
                    var user = await _userCacheService.GetUserAsync(callbackQuery.From.Id);
                    if (user == null) user = await _userRepository.GetUserAsync(callbackQuery.Message.Chat.Id);

                    user.State = UserState.AwaitingCriteriaEdit;
                    user.CurrentCriteriaStep = stepIndex;
                    user.CurrentCriteriaStepValueIndex = 0;
                    user.IsSingle = true;
                    user.LastUpdated = DateTime.UtcNow;
                    await _userCacheService.SetUserAsync(user);
                    //await _userRepository.AddOrUpdateUserAsync(user);

                    await SendStepMessage(client, callbackQuery.Message.Chat.Id, user, cancellationToken);
                }
            }
        }

        public async Task SearchVacancies(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            try
            {
                if (user.UserCriteriaStepValues == null || !user.UserCriteriaStepValues.Any())
                {
                    await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: "⚠️ У вас нет сохраненных критериев поиска.\n\n" +
                             "Пожалуйста, сначала установите критерии через меню \"Мои критерии поиска\".",
                        replyMarkup: new ReplyKeyboardMarkup(new[]
                        {
                            new[] { new KeyboardButton("Мои критерии поиска") },
                            new[] { new KeyboardButton("Вернуться в меню") }
                        })
                        {
                            ResizeKeyboard = true
                        },
                        cancellationToken: cancellationToken);
                    return;
                }

                try
                {
                    // "печатает..."
                    await client.SendChatAction(
                        chatId: message.Chat.Id,
                        action: ChatAction.Typing,
                        cancellationToken: cancellationToken);

                    // Если это первый запрос или нужно обновить результаты
                    if (user.CurrentCriteriaStepValueIndex <= 1 || user.State != UserState.SearchingVacancies)
                    {
                        var request = await CreateRequestFromUser(user);
                        var hasResults = await _vacancyService.SearchAndCacheVacancies(user.TelegramId, request);

                        if (!hasResults)
                        {
                            await client.SendMessage(
                            chatId: message.Chat.Id,
                            text: "😕 По вашим критериям не найдено подходящих вакансий.\n\n" +
                                  "Попробуйте изменить параметры поиска.",
                            replyMarkup: new ReplyKeyboardMarkup(new[]
                            {
                                new[] { new KeyboardButton("Мои критерии поиска") },
                                new[] { new KeyboardButton("Вернуться в меню") }
                            })
                            {
                                ResizeKeyboard = true
                            },
                            cancellationToken: cancellationToken);
                            return;
                        }

                        // Сбрасываем индекс на первую страницу при новом поиске
                        user.CurrentCriteriaStepValueIndex = 1;
                    }

                    user.State = UserState.SearchingVacancies;
                    user.IsSingle = false;
                    user.LastUpdated = DateTime.UtcNow;
                    await _userCacheService.SetUserAsync(user);

                    var page = user.CurrentCriteriaStepValueIndex;
                    var vacancies = await _vacancyService.GetVacanciesPage(user.TelegramId, page);

                    if (!vacancies.Any())
                    {
                        await client.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Вакансии не найдены для этой страницы.",
                            cancellationToken: cancellationToken);
                        return;
                    }

                    foreach (var vacancy in vacancies)
                    {
                        var messageText =
                            $"<b>💼 Должность:</b> {vacancy.Title}\n" +
                            $"<b>🏢 Компания:</b> {vacancy.CompanyName}\n" +
                            $"<b>🌍 Местоположение:</b> {vacancy.Location}\n" +
                            $"<b>💰 Зарплата:</b> {"Неизвестно" ?? "Не указана"}\n" +
                            $"<b>📅 График:</b> {vacancy.Schedule}\n" +
                            $"<b>🕒 Формат работы:</b> {vacancy.WorkFormat}\n" +
                            $"🔗 <a href=\"{vacancy.Url}\">Ссылка на вакансию</a>";

                        await client.SendMessage(
                            chatId: message.Chat.Id,
                            text: messageText,
                            parseMode: ParseMode.Html,
                            cancellationToken: cancellationToken);

                        // Задержечка
                        await Task.Delay(300, cancellationToken);
                    }

                    var keyboard = new List<KeyboardButton[]>();

                    // Имитируем пагинацию
                    var nextPageVacancies = await _vacancyService.GetVacanciesPage(user.TelegramId, page + 1);
                    bool hasNext = nextPageVacancies.Any();

                    bool hasPrevious = page > 1;

                    if (hasPrevious && hasNext)
                    {
                        keyboard.Add(new[] { new KeyboardButton("Назад"), new KeyboardButton("Далее") });
                    }
                    else if (hasPrevious)
                    {
                        keyboard.Add(new[] { new KeyboardButton("Назад") });
                    }
                    else if (hasNext)
                    {
                        keyboard.Add(new[] { new KeyboardButton("Далее") });
                    }

                    keyboard.Add(new[] { new KeyboardButton("Вернуться в меню") });

                    await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: $"Продолжить поиск?",
                        replyMarkup: new ReplyKeyboardMarkup(keyboard) { ResizeKeyboard = true },
                        cancellationToken: cancellationToken);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка при поиске вакансий для пользователя {user.TelegramId}!");

                    await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: "⚠️ Произошла ошибка при поиске вакансий. Пожалуйста, попробуйте позже.",
                        replyMarkup: new ReplyKeyboardMarkup(new[]
                        {
                            new[] { new KeyboardButton("Вернуться в меню") }
                        })
                        {
                            ResizeKeyboard = true
                        },
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске вакансий!");
            }
        }

        private async Task<UserCriteriaRequest> CreateRequestFromUser(User user)
        {
            if (_criteriaSteps == null || !_criteriaSteps.Any())
            {
                await LoadCriteriaStepsAsync();
            }

            if (user?.UserCriteriaStepValues == null)
            {
                return new UserCriteriaRequest
                {
                    UserId = user?.TelegramId ?? 0,
                    RequestDate = DateTime.UtcNow,
                    UserCriteria = new List<UserCriteriaItem>()
                };
            }

            var userCriteria = new List<UserCriteriaItem>();

            foreach (var userValue in user.UserCriteriaStepValues)
            {
                var criteriaStep = _criteriaSteps.FirstOrDefault(cs => cs.Id == userValue.CriteriaStepId);
                if (criteriaStep == null) continue;

                string value;
                bool isCustom;

                if (!string.IsNullOrEmpty(userValue.CustomValue))
                {
                    value = userValue.CustomValue;
                    isCustom = true;
                }
                else if (userValue.CriteriaStepValue != null)
                {
                    value = userValue.CriteriaStepValue.Value;
                    isCustom = false;
                }
                else
                {
                    continue;
                }

                userCriteria.Add(new UserCriteriaItem
                {
                    Name = criteriaStep.Name,
                    Id = value,
                    IsCustom = isCustom,
                    IsMapped = criteriaStep.IsMapped,
                    MainDictionary = criteriaStep.MainDictionary
                });
            }

            return new UserCriteriaRequest
            {
                UserId = user.TelegramId,
                RequestDate = DateTime.UtcNow,
                UserCriteria = userCriteria
            };
        }
    }
}

