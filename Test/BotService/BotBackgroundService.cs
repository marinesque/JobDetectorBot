using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace BotService
{
    public class BotBackgroundService : BackgroundService
    {
        private readonly ILogger<BotBackgroundService> _logger;
        private readonly BotOptions _botOptions;
        private readonly IMessageHandler _messageHandler;

        public BotBackgroundService(
            ILogger<BotBackgroundService> logger,
            IOptions<BotOptions> botOptions,
            IMessageHandler messageHandler)
        {
            _logger = logger;
            _botOptions = botOptions.Value;
            _messageHandler = messageHandler;
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
                    updateHandler: (client, update, token) => _messageHandler.HandleMessageAsync(client, update, token),
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

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            _logger.LogError(exception, $"ErrorHandler: Ошибка в ходе работы бота ({exception.Message})");
            await Task.CompletedTask;
        }

        /*private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message is not { } message) return;

            Console.WriteLine($"UpdateHandler : Username: {message.Chat.Username ?? "anonymous"}  |  " +
                $"Text:{message.Text ?? "not_text"}  |  " +
                $"Chat.ID:{message.Chat.Id}");

            var chatId = message.Chat.Id;

            Message sentMessage = await client.SendMessage(chatId, "Добрый вечер!");

            await Task.CompletedTask;
        }*/
    }
}

