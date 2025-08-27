using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Domain.Enums;
using Bot.Domain.Request.VacancySearch;
using Bot.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Application.Strategies
{
    public class SearchingVacanciesStrategy : IUserStateStrategy
    {
        private readonly ILogger<SearchingVacanciesStrategy> _logger;
        private readonly IUserCacheService _userCacheService;
        private readonly IVacancySearchService _vacancyService;
        private readonly CriteriaStepRepository _criteriaStepRepository;

        private List<CriteriaStep> _criteriaSteps;

        public SearchingVacanciesStrategy(
            ILogger<SearchingVacanciesStrategy> logger,
            IUserCacheService userCacheService,
            IVacancySearchService vacancyService,
            CriteriaStepRepository criteriaStepRepository)
        {
            _logger = logger;
            _userCacheService = userCacheService;
            _vacancyService = vacancyService;
            _criteriaStepRepository = criteriaStepRepository;
        }
        private async Task LoadCriteriaStepsAsync() => this._criteriaSteps = await _criteriaStepRepository.GetAllCriteriaStepsAsync();

        public bool CanHandle(UserState state) => state == UserState.SearchingVacancies;

        public async Task HandleAsync(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (message.Text == "Вернуться в меню")
            {
                await CancelSearch(client, message, user, cancellationToken);
                return;
            }

            if (message.Text == "Назад" || message.Text == "Далее")
            {
                await HandleNavigation(client, message, user, cancellationToken);
                return;
            }

            await SearchVacancies(client, message, user, cancellationToken);
        }

        private async Task SearchVacancies(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
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

        private async Task<UserCriteriaRequest> CreateRequestFromUser(Bot.Domain.DataAccess.Model.User user)
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

        private async Task HandleNavigation(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            if (message.Text == "Назад")
            {
                user.CurrentCriteriaStepValueIndex = Math.Max(0, user.CurrentCriteriaStepValueIndex - 1);
            }
            else if (message.Text == "Далее")
            {
                user.CurrentCriteriaStepValueIndex++;
            }

            user.LastUpdated = DateTime.UtcNow;
            await _userCacheService.SetUserAsync(user);

            await SearchVacancies(client, message, user, cancellationToken);
        }

        private async Task CancelSearch(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            await ResetUserStateAsync(user);
            await client.SendMessage(
                chatId: message.Chat.Id,
                text: "Поиск вакансий завершен.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
            await ShowMainMenu(client, message.Chat.Id, cancellationToken);
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
    }
}