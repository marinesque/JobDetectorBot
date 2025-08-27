using Bot.Domain.DataAccess.Repositories;
using Bot.Domain.Enums;
using Bot.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Application.Strategies
{
    public class NoneStateStrategy : IUserStateStrategy
    {
        private readonly ILogger<NoneStateStrategy> _logger;
        private readonly IUserCacheService _userCacheService;
        private readonly CriteriaStepRepository _criteriaStepRepository;
        private List<Domain.DataAccess.Model.CriteriaStep> _criteriaSteps;

        public NoneStateStrategy(
            ILogger<NoneStateStrategy> logger,
            IUserCacheService userCacheService,
            CriteriaStepRepository criteriaStepRepository)
        {
            _logger = logger;
            _userCacheService = userCacheService;
            _criteriaStepRepository = criteriaStepRepository;
        }

        public bool CanHandle(UserState state) => state == UserState.None;

        public async Task HandleAsync(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
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

            user.State = UserState.AwaitingCriteria;
            user.CurrentCriteriaStep = 0;
            user.CurrentCriteriaStepValueIndex = 0;
            user.LastUpdated = DateTime.UtcNow;
            await _userCacheService.SetUserAsync(user);
            await _userCacheService.SyncToDatabaseAsync(user);

            await SendStepMessage(client, message.Chat.Id, user, cancellationToken);
        }

        private async Task SearchVacancies(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            // Реализация будет в отдельной стратегии
            user.State = UserState.SearchingVacancies;
            user.LastUpdated = DateTime.UtcNow;
            await _userCacheService.SetUserAsync(user);

            // Здесь будет вызов сервиса поиска вакансий
            await client.SendMessage(
                chatId: message.Chat.Id,
                text: "Поиск вакансий запущен...",
                cancellationToken: cancellationToken);
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

        private async Task HandleSubscription(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            await client.SendMessage(
                chatId: message.Chat.Id,
                text: "Функция подписки пока недоступна.",
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

        private async Task SendStepMessage(ITelegramBotClient client, long chatId, Domain.DataAccess.Model.User user, CancellationToken cancellationToken)
        {
            await client.SendMessage(
                chatId: chatId,
                text: "Выберите критерий:",
                cancellationToken: cancellationToken);
        }

        private async Task LoadCriteriaStepsAsync()
        {
            _criteriaSteps = await _criteriaStepRepository.GetAllCriteriaStepsAsync();
        }
    }
}