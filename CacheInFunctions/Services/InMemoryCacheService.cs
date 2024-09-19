using System.Collections.Concurrent;

namespace CacheInFunctions.Services
{
    internal class InMemoryCacheService<V> : CacheService<V>
    {
        private readonly IDictionary<string, V> _cache;

        public InMemoryCacheService()
        {
            _cache = new ConcurrentDictionary<string, V>();
        }

        public Task<V> GetCachedValueAsync(string key)
        {
            _cache.TryGetValue(key, out V value);
            return Task.FromResult(value);
        }

        public Task SetCacheValueAsync(string key, V value, TimeSpan expiration)
        {
            if (value == null)
            {
                throw new InvalidOperationException("Cannot set a null value in Redis.");
            }

            _cache.Add(key, value);
            return Task.CompletedTask;
        }
    }
}
