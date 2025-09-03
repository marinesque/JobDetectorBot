using Bot.Domain.Enums;
using Telegram.Bot.Types;

namespace Bot.Application.Interfaces
{
    public interface IUserStateManager
    {
        Task<UserState> DetermineNextStateAsync(Bot.Domain.DataAccess.Model.User user, Message message);
        Task UpdateUserStateAsync(Bot.Domain.DataAccess.Model.User user, UserState newState);
    }
}
