using Bot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Application.Strategies
{
    public interface IUserStateStrategy
    {
        bool CanHandle(UserState state);
        Task HandleAsync(ITelegramBotClient client, Message message, Domain.DataAccess.Model.User user, CancellationToken cancellationToken);
    }
}