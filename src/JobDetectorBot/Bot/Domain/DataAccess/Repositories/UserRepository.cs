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
                .Include(u => u.Criteria)
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        public async Task AddOrUpdateUserAsync(User user)
        {
            var existingUser = await _context.Users
                .Include(u => u.Criteria)
                .FirstOrDefaultAsync(u => u.TelegramId == user.TelegramId);

            if (existingUser == null)
            {
                _context.Users.Add(user);
            }
            else
            {
                _context.Entry(existingUser).CurrentValues.SetValues(user);

                if (user.Criteria != null)
                {
                    if (existingUser.Criteria == null)
                    {
                        existingUser.Criteria = user.Criteria;
                        existingUser.Criteria.UserId = existingUser.Id;
                    }
                    else
                    {
                        _context.Entry(existingUser.Criteria).CurrentValues.SetValues(user.Criteria);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}