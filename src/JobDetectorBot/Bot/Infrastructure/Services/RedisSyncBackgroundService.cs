using Bot.Domain.DataAccess.Model;
using Bot.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Bot.Infrastructure
{
    public class RedisSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<RedisSyncBackgroundService> _logger;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);

        public RedisSyncBackgroundService(
            IServiceProvider services,
            ILogger<RedisSyncBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
                    var userCacheService = scope.ServiceProvider.GetRequiredService<IUserCacheService>();
                    var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<RedisSyncBackgroundService>>();

                    var endpoints = redis.GetEndPoints();
                    if (endpoints.Length == 0)
                    {
                        logger.LogWarning("Не найдены конечные точки Redis");
                        continue;
                    }

                    // Все ключи пользователей из Redis
                    var server = redis.GetServer(redis.GetEndPoints().First());
                    var userKeys = server.Keys(pattern: "юзер:*").ToArray();

                    logger.LogInformation($"Найдено {userKeys.Length} пользователей в Redis для синхронизации");

                    foreach (var key in userKeys)
                    {
                        try
                        {
                            var redisValue = await cache.GetStringAsync(key.ToString());
                            if (string.IsNullOrEmpty(redisValue)) continue;

                            var user = JsonSerializer.Deserialize<User>(redisValue);
                            if (user == null) continue;

                            await userCacheService.SyncToDatabaseAsync(user);

                            logger.LogDebug($"Синхронизирован пользователь {user.TelegramId}");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Ошибка синхронизации пользователя по ключу {key}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в основном цикле синхронизации");
                }

                await Task.Delay(_syncInterval, stoppingToken);
            }
        }
    }
}