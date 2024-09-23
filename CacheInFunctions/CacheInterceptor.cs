namespace CacheInFunctions
{
    using System;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CacheInFunctions.Attributes;
    using CacheInFunctions.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Functions.Worker;

    public class CacheInterceptor
    {
        private readonly CacheService<string> _cacheService;
        private readonly ICacheKeyGenerationStrategy _cacheKeyGenerationStrategy;

        public CacheInterceptor(CacheService<string> cacheService, ICacheKeyGenerationStrategy cacheKeyGenerationStrategy)
        {
            _cacheService = cacheService;
            _cacheKeyGenerationStrategy = cacheKeyGenerationStrategy;
        }

        public async Task<InvocationResult> ExecuteWithCacheAsync(Func<Task<InvocationResult>> function, MethodInfo methodInfo, object parameter, FunctionContext context)
        {
            var cacheAttribute = methodInfo.GetCustomAttribute<CacheEnabledAttribute>();
            if (cacheAttribute == null)
            {
                // If there is no CacheEnabledAttribute, execute the function directly
                return await function();
            }

            // Generate a cache key based on method name and parameters
            var cacheKey = _cacheKeyGenerationStrategy.GenerateKey(methodInfo.Name, parameter);

            // Try to get the result from the cache
            var cachedResult = await _cacheService.GetCachedValueAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedResult))
            {
                var deserializedResult = JsonSerializer.Deserialize<JsonElement>(cachedResult);

                if (deserializedResult.ValueKind != JsonValueKind.Undefined)
                {
                    var l = context.GetInvocationResult();
                    l.Value = new OkObjectResult( deserializedResult.GetProperty("Value"));
                    return l;
                }
            }

            InvocationResult result = await function();

            // Cache the result
            var serializedResult = JsonSerializer.Serialize(result.Value);
            await _cacheService.SetCacheValueAsync(cacheKey, serializedResult, TimeSpan.FromSeconds(cacheAttribute.ExpirationInSeconds));

            return result;
        }
    }

}
