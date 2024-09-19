/* *
 * Move this class to Function host
 *  
 * */
namespace CacheInFunctions.Services
{
    public interface CacheService<V>
    {
        public Task<V> GetCachedValueAsync(string key);
        public Task SetCacheValueAsync(string key, V value, TimeSpan expiration);
    }
}
