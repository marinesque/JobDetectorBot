using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot
{
    public class MessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private static readonly string _botImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "BotImages");
        private readonly BotUserRepository _userRepository;
        private readonly ITelegramBotClient _bot;

        static MessageHandler()
        {
            if (!Directory.Exists(_botImagesPath))
            {
                Directory.CreateDirectory(_botImagesPath);
            }
        }

        public MessageHandler(ILogger<MessageHandler> logger, BotUserRepository userRepository, ITelegramBotClient bot)
        {
            _logger = logger;
            _userRepository = userRepository;
            _bot = bot;
        }

        public async Task HandleMessageAsync(Update update, CancellationToken cancellationToken)
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
                var user = await _userRepository.GetUserAsync(userId) ?? new BotUser { UserId = userId };

                // Проверка тайм-аута сценария
                if (user.State != UserState.None && (DateTime.UtcNow - user.LastUpdated) > TimeSpan.FromMinutes(1))
                {
                    user.State = UserState.None; // Сброс состояния
                    await _userRepository.AddOrUpdateUserAsync(user);
                    await _bot.SendMessage(chatId, "Сценарий прерван из-за неактивности.", cancellationToken: cancellationToken);
                    return;
                }

                // Обработка в зависимости от состояния пользователя
                // Обработка в зависимости от состояния пользователя
                switch (user.State)
                {
                    case UserState.AwaitingAge:
                        await HandleAgeInput(message, user, cancellationToken);
                        break;
                    case UserState.AwaitingPhone:
                        await HandlePhoneInput(message, user, cancellationToken);
                        break;
                    default:
                        await HandleDefaultInput(message, user, cancellationToken);
                        break;
                }

                // Документы
                if (message.Document is not null)
                {
                    await SaveFileAsync(message.Document.FileId, message.Document.FileName, cancellationToken);
                    await this._bot.SendMessage(message.Chat.Id, $"Документ {message.Document.FileName} сохранен!", cancellationToken: cancellationToken);
                }

                // Картинки и фото
                if (message.Photo is not null)
                {
                    var photo = message.Photo.Last(); // Может быть много фоток
                    await SaveFileAsync(photo.FileId, $"photo_{photo.FileId}.jpg", cancellationToken);
                    await this._bot.SendMessage(message.Chat.Id, "Фото сохранено!", cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "В процессе обработки сообщения возникла ошибка");
            }
        }

        private async Task HandleDefaultInput(Message message, BotUser user, CancellationToken cancellationToken)
        {
            if (message.Text is not { } messageText)
                return;

            await (messageText.Split(' ')[0] switch
            {
                "/search " => StartScenario(message, user, cancellationToken),
                "/cancel" => CancelScenario(message, user, cancellationToken),
                _ => HandleDefaultMessage(message, cancellationToken)
            });
        }

        private async Task HandleDefaultMessage(Message message, CancellationToken cancellationToken)
        {
            await _bot.SendMessage(message.Chat.Id, "Добрый вечер!", cancellationToken: cancellationToken);
        }

        private async Task StartScenario(Message message, BotUser user, CancellationToken cancellationToken)
        {
            user.State = UserState.AwaitingName; // Начало сценария
            user.LastUpdated = DateTime.UtcNow; // Обновление времени последней активности
            await _userRepository.AddOrUpdateUserAsync(user);

            await _bot.SendMessage(message.Chat.Id, "Введите ваше имя:", cancellationToken: cancellationToken);
        }

        private async Task CancelScenario(Message message, BotUser user, CancellationToken cancellationToken)
        {
            user.State = UserState.None; // Сброс состояния
            await _userRepository.AddOrUpdateUserAsync(user);

            await _bot.SendMessage(message.Chat.Id, "Сценарий отменен.", cancellationToken: cancellationToken);
        }

        private async Task HandleAgeInput(Message message, BotUser user, CancellationToken cancellationToken)
        {
            if (message.Text == "/cancel")
            {
                await CancelScenario(message, user, cancellationToken);
                return;
            }

            if (int.TryParse(message.Text, out var age))
            {
                user.Age = age;
                user.State = UserState.AwaitingPhone; // Переход к следующему этапу
                user.LastUpdated = DateTime.UtcNow; // Обновление времени последней активности
                await _userRepository.AddOrUpdateUserAsync(user);
                await _bot.SendMessage(
                    message.Chat.Id,
                    "Возраст сохранен!",
                    cancellationToken: cancellationToken);

                await _bot.SendMessage(
                    message.Chat.Id,
                    "Введите ваш телефон:",
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _bot.SendMessage(message.Chat.Id, "Пожалуйста, введите корректный возраст (число):", cancellationToken: cancellationToken);
            }
        }

        private async Task HandlePhoneInput(Message message, BotUser user, CancellationToken cancellationToken)
        {
            if (message.Text == "/cancel")
            {
                await CancelScenario(message, user, cancellationToken);
                return;
            }

            if (message.Text.StartsWith("+") && message.Text.Length > 5) // Простая валидация номера телефона
            {
                user.Phone = message.Text;
                user.State = UserState.None; // Сценарий завершен
                user.LastUpdated = DateTime.UtcNow; // Обновление времени последней активности
                await _userRepository.AddOrUpdateUserAsync(user);
                await _bot.SendMessage(
                    message.Chat.Id,
                    "Номер телефона сохранен!",
                    cancellationToken: cancellationToken);

                await _bot.SendMessage(
                    message.Chat.Id,
                    $"Спасибо, {user.FirstName}! Ваши данные сохранены.",
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _bot.SendMessage(
                    message.Chat.Id,
                    "Пожалуйста, введите корректный телефон (начинается с + и содержит более 5 символов).",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task SaveFileAsync(string fileId, string fileName, CancellationToken cancellationToken)
        {
            var file = await _bot.GetFile(fileId, cancellationToken);
            var filePath = Path.Combine(_botImagesPath, fileName);

            await using var saveStream = new FileStream(filePath, FileMode.Create);
            await _bot.DownloadFile(file.FilePath!, saveStream, cancellationToken);

            _logger.LogInformation($"Файл сохранен в {filePath}");
        }
    }
}

