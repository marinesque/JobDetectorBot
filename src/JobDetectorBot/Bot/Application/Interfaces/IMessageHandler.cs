using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot
{
    public interface IMessageHandler
    {
        /// <summary>
        /// Обрабатывает входящее сообщение от пользователя.
        /// </summary>
        /// <param name="client">Клиент Telegram Bot API.</param>
        /// <param name="update">Входящее обновление (сообщение).</param>
        /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task HandleMessageAsync(ITelegramBotClient cient, Update update, CancellationToken cancellationToken);
    }
}