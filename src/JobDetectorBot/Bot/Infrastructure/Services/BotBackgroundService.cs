using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Bot.Infrastructure
{
    public class BotBackgroundService : BackgroundService
    {
        private readonly ILogger<BotBackgroundService> _logger;
        private readonly BotOptions _botOptions;
        private readonly IServiceScopeFactory _scopeFactory;

        public BotBackgroundService(
            ILogger<BotBackgroundService> logger,
            IOptions<BotOptions> botOptions,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _botOptions = botOptions.Value;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bot = new TelegramBotClient(_botOptions.Token);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = []
            };

            try
            {
                await bot.ReceiveAsync(
                    updateHandler: HandleUpdateAsync,
                    errorHandler: ErrorHandler,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Бот-сервис остановлен по cancellationToken.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "В ходе работы бота обнаружена ошибка");
            }

            _logger.LogInformation("Бот-сервис остановлен.");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            // Создаем область (scope) для использования Scoped-сервисов
            using (var scope = _scopeFactory.CreateScope())
            {
                //var criteriaStepsActualizer = scope.ServiceProvider.GetRequiredService<ICriteriaStepsActualize>();
                //await criteriaStepsActualizer.StartAsync(token);
                var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();
                await messageHandler.HandleMessageAsync(client, update, token);
            }
        }

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            _logger.LogError(exception, $"ErrorHandler: Ошибка в ходе работы бота ({exception.Message})");
            await Task.CompletedTask;
        }
    }
}