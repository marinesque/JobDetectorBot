using Bot.Application.Interfaces;
using Bot.Domain.Enums;
using Bot.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bot.Application.Handlers
{
    public class UserStateManager : IUserStateManager
    {
        private readonly IUserCacheService _userCacheService;
        private readonly ILogger<UserStateManager> _logger;

        public UserStateManager(IUserCacheService userCacheService, ILogger<UserStateManager> logger)
        {
            _userCacheService = userCacheService;
            _logger = logger;
        }

        public async Task<UserState> DetermineNextStateAsync(Bot.Domain.DataAccess.Model.User user, Message message)
        {
            return message.Text switch
            {
                "Начать новый поиск" => UserState.AwaitingCriteria,
                "Искать вакансии" => UserState.SearchingVacancies,
                "Мои критерии поиска" => UserState.AwaitingCriteriaEdit,
                "Вернуться в меню" => UserState.None,
                _ => user.State,
            };
        }

        public async Task UpdateUserStateAsync(Bot.Domain.DataAccess.Model.User user, UserState newState)
        {
            user.State = newState;
            user.CurrentCriteriaStep = 0;
            user.CurrentCriteriaStepValueIndex = 0;
            user.IsSingle = false;
            user.LastUpdated = DateTime.UtcNow;
            await _userCacheService.SetUserAsync(user);
        }
    }
}
