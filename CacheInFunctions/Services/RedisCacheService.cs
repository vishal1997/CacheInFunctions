namespace CacheInFunctions.Services
{
    using System;
    using System.Threading.Tasks;
    using StackExchange.Redis;

    public class RedisCacheService<V> : CacheService<V>
    {
        private readonly IDatabase _cache;
        private readonly ConnectionMultiplexer _redis;

        public RedisCacheService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _cache = _redis.GetDatabase();
        }

        public async Task<V> GetCachedValueAsync(string key)
        {
            RedisValue value = await _cache.StringGetAsync(key);
            if (!value.HasValue)
            {
                return default;
            }
            return (V)Convert.ChangeType(value.ToString(), typeof(V));
        }

        public async Task SetCacheValueAsync(string key, V value, TimeSpan expiration)
        {
            if (value == null)
            {
                throw new InvalidOperationException("Cannot set a null value in Redis.");
            }
            var redisValue = RedisValue.Unbox(value);
            await _cache.StringSetAsync(key, redisValue, expiration);
        }
    }
}
