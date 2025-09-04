using StackExchange.Redis;

namespace VacancyService.BusinessLogic
{
    public interface IRedisQueryStore
    {
        Task SaveKeyValueAsync(string key, string value);
        Task<string?> GetKeyValueAsync(string key);
		Task<bool> TryGetValueAsync(string key);
	}

    public class RedisQueryStore : IRedisQueryStore
    {
        private readonly IDatabase _db;

        public RedisQueryStore(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SaveKeyValueAsync(string key, string value)
        {
            await _db.StringSetAsync($"key:{key}", value, TimeSpan.FromMinutes(30));
        }

		public async Task<string?> GetKeyValueAsync(string key)
		{
			return await _db.StringGetAsync($"key:{key}");
		}

		public async Task<bool> TryGetValueAsync(string key)
		{
			string value = await _db.StringGetAsync($"key:{key}");
			if (string.IsNullOrEmpty(value))
			{
				return false;
			}
			return true;
		}
	}
}