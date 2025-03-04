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

        // Получить пользователя по Telegram ID
        public async Task<User> GetUserAsync(long telegramId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        // Добавить или обновить пользователя
        public async Task AddOrUpdateUserAsync(User user)
        {
            var existingUser = await _context.Users
                .Include(u => u.Criteria)
                .FirstOrDefaultAsync(u => u.TelegramId == user.TelegramId);

            if (existingUser == null)
            {
                // Если пользователь не существует, добавляем его
                _context.Users.Add(user);
            }
            else
            {
                // Если пользователь существует, обновляем его данные
                _context.Entry(existingUser).CurrentValues.SetValues(user);

                // Обновляем критерии, если они есть
                if (user.Criteria != null)
                {
                    if (existingUser.Criteria == null)
                    {
                        existingUser.Criteria = user.Criteria;
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