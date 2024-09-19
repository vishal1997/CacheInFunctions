namespace CacheInFunctions
{
    using System;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CacheInFunctions.Attributes;
    using CacheInFunctions.Services;
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

        public async Task<InvocationResult> ExecuteWithCacheAsync(Func<Task<InvocationResult>> function, MethodInfo methodInfo, object parameter)
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
                var deserializedResult = Deserialize(cachedResult);// JsonSerializer.Deserialize<T>(cachedResult);
                if (deserializedResult != null)
                {
                    return (InvocationResult)deserializedResult;
                }
            }
            //InvocationResult invocationResult = await function();
            // If not in cache, execute the function
            InvocationResult result = await function();

            // Cache the result
            var serializedResult = Serialize(result);// JsonSerializer.Serialize(result);
            await _cacheService.SetCacheValueAsync(cacheKey, serializedResult, TimeSpan.FromSeconds(cacheAttribute.ExpirationInSeconds));

            return result;
        }


        public static string Serialize(InvocationResult obj)
        {
            var serializableObject = new SerializableObject
            {
                TypeName = obj.GetType().AssemblyQualifiedName,
                Data = JsonSerializer.Serialize(obj) 
            };
            return JsonSerializer.Serialize(serializableObject);
        }

        public static object Deserialize(string serializedString)
        {
            var serializableObject = JsonSerializer.Deserialize<SerializableObject>(serializedString);

            if (serializableObject == null || string.IsNullOrEmpty(serializableObject.TypeName))
                throw new InvalidOperationException("Invalid serialized object.");

            Type objectType = Type.GetType(serializableObject.TypeName);
            if (objectType == null)
                throw new InvalidOperationException("Type not found.");

            return JsonSerializer.Deserialize(serializableObject.Data, objectType);
        }
    }

}
