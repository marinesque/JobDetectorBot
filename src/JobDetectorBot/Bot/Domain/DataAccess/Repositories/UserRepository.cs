using Bot.Domain.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace Bot.Domain.DataAccess.Repositories
{
    public class UserRepository
    {
        private readonly BotDbContext _context;

        public UserRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserAsync(long telegramId)
        {
            return await _context.Users
                .Include(u => u.UserCriteriaStepValues)
                    .ThenInclude(ucsv => ucsv.CriteriaStep)
                .Include(u => u.UserCriteriaStepValues)
                    .ThenInclude(ucsv => ucsv.CriteriaStepValue)
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        public async Task AddOrUpdateUserAsync(User user)
        {
            var existingUser = await _context.Users
                .Include(u => u.UserCriteriaStepValues)
                .FirstOrDefaultAsync(u => u.TelegramId == user.TelegramId);

            if (existingUser == null)
            {
                _context.Users.Add(user);
            }
            else
            {
                _context.Entry(existingUser).CurrentValues.SetValues(user);

                foreach (var userCriteria in user.UserCriteriaStepValues)
                {
                    var existingCriteria = existingUser.UserCriteriaStepValues
                        .FirstOrDefault(ucsv =>
                            ucsv.CriteriaStepId == userCriteria.CriteriaStepId &&
                            ucsv.UserId == existingUser.Id);

                    if (existingCriteria != null)
                    {
                        _context.Entry(existingCriteria).CurrentValues.SetValues(userCriteria);
                    }
                    else
                    {
                        userCriteria.UserId = existingUser.Id;
                        existingUser.UserCriteriaStepValues.Add(userCriteria);
                    }
                }

                foreach (var existingCriteria in existingUser.UserCriteriaStepValues.ToList())
                {
                    if (!user.UserCriteriaStepValues.Any(ucsv =>
                        ucsv.CriteriaStepId == existingCriteria.CriteriaStepId))
                    {
                        _context.UserCriteriaStepValues.Remove(existingCriteria);
                    }
                }

                existingUser.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
        public async Task AddOrUpdateUserCriteriaAsync(long userId, long criteriaStepId, long? criteriaStepValueId = null, string? customValue = null)
        {
            var existingValue = await _context.UserCriteriaStepValues
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CriteriaStepId == criteriaStepId);

            if (existingValue != null)
            {
                existingValue.CriteriaStepValueId = criteriaStepValueId;
                existingValue.CustomValue = customValue;
                existingValue.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserCriteriaStepValues.Add(new UserCriteriaStepValue
                {
                    UserId = userId,
                    CriteriaStepId = criteriaStepId,
                    CriteriaStepValueId = criteriaStepValueId,
                    CustomValue = customValue,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
    }
}