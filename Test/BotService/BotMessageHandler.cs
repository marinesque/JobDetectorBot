using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotService
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken);
    }

    public class MessageHandler : IMessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private static readonly string _botImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "BotImages");

        static MessageHandler()
        {
            if (!Directory.Exists(_botImagesPath))
            {
                Directory.CreateDirectory(_botImagesPath);
            }
        }

        public MessageHandler(ILogger<MessageHandler> logger)
        {
            _logger = logger;
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

                // Имя пользователя (если есть)
                var username = message.From.Username ?? "anonymous";

                _logger.LogInformation($"Получено сообщение от {username} (User ID: {userId}, Chat ID: {chatId}): {message.Text}");

                // Текстовое сообщение
                if (message.Text is not null)
                {
                    await client.SendMessage(message.Chat.Id, "Добрый вечер!", cancellationToken: cancellationToken);
                }

                // Документы
                if (message.Document is not null)
                {
                    await SaveFileAsync(client, message.Document.FileId, message.Document.FileName, cancellationToken);
                    await client.SendMessage(message.Chat.Id, $"Документ {message.Document.FileName} сохранен!", cancellationToken: cancellationToken);
                }

                // Картинки и фото
                if (message.Photo is not null)
                {
                    var photo = message.Photo.Last(); // Может быть много фоток
                    await SaveFileAsync(client, photo.FileId, $"photo_{photo.FileId}.jpg", cancellationToken);
                    await client.SendMessage(message.Chat.Id, "Фото сохранено!", cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "В процессе обработки сообщения возникла ошибка");
            }
        }

        private async Task SaveFileAsync(ITelegramBotClient client, string fileId, string fileName, CancellationToken cancellationToken)
        {
            // Получаем информацию о файле
            var file = await client.GetFile(fileId, cancellationToken);

            // Формируем путь для сохранения
            var filePath = Path.Combine(_botImagesPath, fileName);

            // Скачиваем и сохраняем файл
            await using var saveStream = new FileStream(filePath, FileMode.Create);
            await client.DownloadFile(file.FilePath!, saveStream, cancellationToken);

            _logger.LogInformation($"Файл сохранен в {filePath}");
        }
    }
}

