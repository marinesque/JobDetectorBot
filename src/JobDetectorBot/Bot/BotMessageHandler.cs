using Microsoft.Extensions.Logging;
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
        private readonly BotUserRepository _userRepository;

        static MessageHandler()
        {
            if (!Directory.Exists(_botImagesPath))
            {
                Directory.CreateDirectory(_botImagesPath);
            }
        }

        public MessageHandler(ILogger<MessageHandler> logger, BotUserRepository userRepository)
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

                if (message.Text is not null)
                {
                    if (message.Text.StartsWith("/start"))
                    {
                        await client.SendMessage(message.Chat.Id, "Введите ваш возраст:", cancellationToken: cancellationToken);
                    }
                    else if (int.TryParse(message.Text, out var age))
                    {
                        var user = new BotUser
                        {
                            UserId = userId,
                            FirstName = firstName,
                            Age = age
                        };

                        await _userRepository.AddOrUpdateUserAsync(user);
                        await client.SendMessage(message.Chat.Id, $"Спасибо, {firstName}! Ваш возраст ({age}) сохранен.", cancellationToken: cancellationToken);
                    }
                    else if (message.Text.StartsWith("/phone"))
                    {
                        await client.SendMessage(message.Chat.Id, "Введите ваш телефон:", cancellationToken: cancellationToken);
                    }
                    else if (message.Text.StartsWith("+"))
                    {
                        var user = new BotUser
                        {
                            UserId = userId,
                            FirstName = firstName,
                            Phone = message.Text
                        };

                        await _userRepository.AddOrUpdateUserAsync(user);
                        await client.SendMessage(message.Chat.Id, $"Спасибо, {firstName}! Ваш телефон ({message.Text}) сохранен.", cancellationToken: cancellationToken);
                    }
                }


                //----------------------------------------------------------------------------------------------------//
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

