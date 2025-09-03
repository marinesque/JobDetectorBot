using Bot.Domain.DataAccess.Dto;
using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bot.Infrastructure.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserCacheService> _logger;
        private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(30);
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly BotDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public UserCacheService(
            IDistributedCache cache,
            UserRepository userRepository,
            ILogger<UserCacheService> logger,
            BotDbContext context,
            IConnectionMultiplexer redis)
        {
            _cache = cache;
            _userRepository = userRepository;
            _logger = logger;
            _context = context;
            _redis = redis;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public async Task<User> GetUserAsync(long telegramId)
        {

            var key = GetUserKey(telegramId);

            _logger.LogDebug("Попытка получения пользователя {TelegramId} из кэша. Ключ: {CacheKey}", telegramId, key);

            try
            {
                var cachedUser = await _cache.GetStringAsync(key);

                if (cachedUser != null)
                {
                    _logger.LogDebug("Пользователь {TelegramId} найден в кэше", telegramId);
                    var userDto = JsonSerializer.Deserialize<UserCacheDto>(cachedUser, _jsonOptions);

                    if (userDto != null)
                    {
                        _logger.LogInformation("Успешно получен пользователь {TelegramId} из кэша. Критериев: {CriteriaCount}",
                            telegramId, userDto.UserCriteriaStepValues?.Count ?? 0);
                    }

                    return userDto.ToUser();
                }

                _logger.LogDebug("Пользователь {TelegramId} отсутствует в кэше. Загрузка из БД...", telegramId);

                var user = await _userRepository.GetUserAsync(telegramId);

                if (user != null)
                {
                    _logger.LogDebug("Кэширование пользователя {TelegramId} на 5 минут", telegramId);

                    await SetUserAsync(user, TimeSpan.FromMinutes(5));

                    _logger.LogInformation("Пользователь {TelegramId} успешно загружен из БД и закэширован", telegramId);
                }
                else
                {
                    _logger.LogWarning("Пользователь {TelegramId} не найден ни в кэше, ни в БД", telegramId);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении пользователя {telegramId} из кэша");

                return await _userRepository.GetUserAsync(telegramId);
            }
        }

        public async Task SetUserAsync(User user, TimeSpan? expiry = null)
        {
            var key = GetUserKey(user.TelegramId);
            var actualExpiry = expiry ?? _defaultExpiry;

            _logger.LogDebug("Сохранение пользователя {TelegramId} в кэш. Ключ: {CacheKey}, Время жизни: {Expiry}",
                user.TelegramId, key, actualExpiry);

            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = actualExpiry
                };

                // DTO без циклических ссылок
                var userDto = UserCacheDto.FromUser(user);
                var serializedUser = JsonSerializer.Serialize(userDto, _jsonOptions);

                await _cache.SetStringAsync(key, serializedUser, options);

                _logger.LogInformation("Пользователь {TelegramId} успешно сохранен в кэш. Размер данных: {DataSize} байт",
                    user.TelegramId, serializedUser.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении пользователя {TelegramId} в кэш", user.TelegramId);

                throw;
            }
        }

        public async Task RemoveUserAsync(long telegramId)
        {
            var key = GetUserKey(telegramId);

            _logger.LogDebug("Удаление пользователя {TelegramId} из кэша. Ключ: {CacheKey}", telegramId, key);

            try
            {
                await _cache.RemoveAsync(key);

                _logger.LogInformation("Пользователь {TelegramId} успешно удален из кэша", telegramId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя {TelegramId} из кэша", telegramId);

                throw;
            }
        }

        public async Task SyncToDatabaseAsync(User user)
        {
            _logger.LogInformation("Начало синхронизации пользователя {TelegramId} с БД. Критериев: {CriteriaCount}",
                user.TelegramId, user.UserCriteriaStepValues?.Count ?? 0);

            try
            {
                await _userRepository.AddOrUpdateUserAsync(user);
                await SetUserAsync(user); // Обновляем кэш после синхронизации

                _logger.LogInformation("Успешная синхронизация пользователя {TelegramId} с БД. Обновлен кэш", user.TelegramId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при синхронизации пользователя {TelegramId} с БД", user.TelegramId);

                throw;
            }
        }

        public async Task<User> UpdateUserCriteriaAsync(
            long userId,
            long criteriaStepId,
            long? criteriaStepValueId = null,
            string? customValue = null)

        {
            _logger.LogDebug("Обновление критериев пользователя {UserId}. CriteriaStepId: {CriteriaStepId}, ValueId: {ValueId}, CustomValue: {CustomValue}",
                userId, criteriaStepId, criteriaStepValueId, customValue);

            var user = await GetUserAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь {UserId} не найден при попытке обновления критериев", userId);

                return user;
            }

            var criteriaStepValue = criteriaStepValueId.HasValue
                ? await _context.CriteriaStepValues.FindAsync(criteriaStepValueId.Value)
                : null;

            var criteriaStep = await _context.CriteriaSteps.FindAsync(criteriaStepId);

            var existing = user.UserCriteriaStepValues?
                .FirstOrDefault(x => x.CriteriaStepId == criteriaStepId);

            if (existing != null)
            {
                _logger.LogDebug("Обновление существующего критерия {CriteriaStepId} для пользователя {UserId}",
                    criteriaStepId, userId);

                existing.CriteriaStepValueId = criteriaStepValueId;
                existing.CriteriaStepValue = criteriaStepValue;
                existing.CriteriaStep = criteriaStep;
                existing.CustomValue = customValue;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _logger.LogDebug("Добавление нового критерия {CriteriaStepId} для пользователя {UserId}",
                    criteriaStepId, userId);

                user.UserCriteriaStepValues ??= new List<UserCriteriaStepValue>();
                user.UserCriteriaStepValues.Add(new UserCriteriaStepValue
                {
                    CriteriaStepId = criteriaStepId,
                    CriteriaStepValueId = criteriaStepValueId,
                    CriteriaStepValue = criteriaStepValue,
                    CriteriaStep = criteriaStep,
                    CustomValue = customValue,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await SetUserAsync(user);

            _logger.LogInformation("Критерии пользователя {UserId} успешно обновлены. Всего критериев: {CriteriaCount}",
                userId, user.UserCriteriaStepValues?.Count ?? 0);

            return user;
        }

        private static string GetUserKey(long telegramId) => $"user:{telegramId}";

        public async Task ClearCacheAsync()
        {
            try
            {
                var db = _redis.GetDatabase();
                var endpoint = _redis.GetEndPoints().FirstOrDefault();

                if (endpoint == null)
                {
                    _logger.LogError("Нет endpoints для Redis.");
                    return;
                }

                var server = _redis.GetServer(endpoint);

                await db.ExecuteAsync("FLUSHDB");
                _logger.LogInformation("Redis успешно очищен.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка предварительной очистки Redis.");
                throw;
            }
        }
    }
}