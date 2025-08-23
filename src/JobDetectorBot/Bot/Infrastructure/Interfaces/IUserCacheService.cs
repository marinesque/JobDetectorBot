using Bot.Domain.DataAccess.Model;

namespace Bot.Infrastructure.Interfaces
{
    public interface IUserCacheService
    {
        Task<User> GetUserAsync(long telegramId);

        Task SetUserAsync(User user, TimeSpan? expiry = null);

        Task RemoveUserAsync(long telegramId);

        Task SyncToDatabaseAsync(User user);

        Task ClearCacheAsync();

        public Task<User> UpdateUserCriteriaAsync(
        long userId,
        long criteriaStepId,
        long? criteriaStepValueId = null,
        string? customValue = null);
    }
}
