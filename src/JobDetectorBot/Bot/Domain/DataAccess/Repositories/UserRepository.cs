using Bot.Domain.DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bot.Domain.DataAccess.Repositories
{
    public class UserRepository
    {
        private readonly BotDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            BotDbContext context,
            ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> GetUserAsync(long telegramId)
        {
            try
            {
                _logger.LogInformation($"Получение пользователя с TelegramId: {telegramId}");

                var user = await _context.Users
                    .Include(u => u.UserCriteriaStepValues)
                        .ThenInclude(ucsv => ucsv.CriteriaStep)
                    .Include(u => u.UserCriteriaStepValues)
                        .ThenInclude(ucsv => ucsv.CriteriaStepValue)
                    .FirstOrDefaultAsync(u => u.TelegramId == telegramId);

                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с TelegramId {telegramId} не найден");
                }
                else
                {
                    _logger.LogInformation($"Найден пользователь: Id={user.Id}, TelegramId={user.TelegramId}, CriteriaCount={user.UserCriteriaStepValues?.Count ?? 0}");
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении пользователя с TelegramId {telegramId}");

                throw;
            }
        }

        public async Task AddOrUpdateUserAsync(User user)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation($"Сохранение пользователя: TelegramId={user.TelegramId}");

                var existingUser = await _context.Users
                    .Include(u => u.UserCriteriaStepValues)
                    .FirstOrDefaultAsync(u => u.TelegramId == user.TelegramId);

                if (existingUser == null)
                {
                    _logger.LogInformation("Добавление нового пользователя");

                    await _context.Users.AddAsync(user);
                }
                else
                {
                    _logger.LogInformation($"Обновление существующего пользователя: Id={existingUser.Id}");

                    _context.Entry(existingUser).CurrentValues.SetValues(user);
                    existingUser.LastUpdated = DateTime.UtcNow;

                    foreach (var userCriteria in user.UserCriteriaStepValues)
                    {
                        var existingCriteria = existingUser.UserCriteriaStepValues
                            .FirstOrDefault(ucsv =>
                                ucsv.CriteriaStepId == userCriteria.CriteriaStepId &&
                                ucsv.UserId == existingUser.Id);

                        if (existingCriteria != null)
                        {
                            _logger.LogDebug($"Обновление критерия: CriteriaStepId={userCriteria.CriteriaStepId}");

                            _context.Entry(existingCriteria).CurrentValues.SetValues(userCriteria);
                        }
                        else
                        {
                            _logger.LogDebug($"Добавление нового критерия: CriteriaStepId={userCriteria.CriteriaStepId}");

                            userCriteria.UserId = existingUser.Id;
                            existingUser.UserCriteriaStepValues.Add(userCriteria);
                        }
                    }

                    foreach (var existingCriteria in existingUser.UserCriteriaStepValues.ToList())
                    {
                        if (!user.UserCriteriaStepValues.Any(ucsv =>
                            ucsv.CriteriaStepId == existingCriteria.CriteriaStepId))
                        {
                            _logger.LogDebug($"Удаление критерия: CriteriaStepId={existingCriteria.CriteriaStepId}");

                            _context.UserCriteriaStepValues.Remove(existingCriteria);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"Пользователь успешно сохранен: TelegramId={user.TelegramId}, CriteriaCount={user.UserCriteriaStepValues?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(ex, $"Ошибка при сохранении пользователя: TelegramId={user.TelegramId}");

                throw;
            }
        }

        public async Task AddOrUpdateUserCriteriaAsync(long userId, long criteriaStepId, long? criteriaStepValueId = null, string? customValue = null)
        {
            try
            {
                _logger.LogInformation($"Обновление критериев пользователя: UserId={userId}, CriteriaStepId={criteriaStepId}");

                var existingValue = await _context.UserCriteriaStepValues
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.CriteriaStepId == criteriaStepId);

                if (existingValue != null)
                {
                    _logger.LogDebug("Обновление существующего критерия");

                    existingValue.CriteriaStepValueId = criteriaStepValueId;
                    existingValue.CustomValue = customValue;
                    existingValue.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _logger.LogDebug("Добавление нового критерия");

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

                await _context.SaveChangesAsync();

                _logger.LogInformation("Критерии пользователя успешно обновлены");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении критериев пользователя: UserId={userId}, CriteriaStepId={criteriaStepId}");
                throw;
            }
        }
    }
}