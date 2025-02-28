using Microsoft.EntityFrameworkCore;

namespace Bot
{

    public class BotUserRepository
    {
        private readonly BotDbContext _context;

        public BotUserRepository(BotDbContext context)
        {
            _context = context;
        }

        public async Task AddOrUpdateUserAsync(BotUser user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);
            if (existingUser != null)
            {
                existingUser.FirstName = user.FirstName;
                existingUser.Age = user.Age;
                existingUser.Phone = user.Phone;
            }
            else
            {
                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<BotUser?> GetUserAsync(long userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
