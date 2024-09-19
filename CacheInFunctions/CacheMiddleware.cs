using System.Reflection;
using CacheInFunctions.Attributes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace CacheInFunctions
{
    public class CacheMiddleware : IFunctionsWorkerMiddleware
    {

        private readonly CacheInterceptor _cacheInterceptor;

        public CacheMiddleware(CacheInterceptor cacheInterceptor)
        {
            _cacheInterceptor = cacheInterceptor;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var functionName = context.FunctionDefinition.Name;

            var functionType = typeof(CacheExample);

            var methodInfo = GetMethodByFunctionName(functionType, functionName);

            // Check if the method has the CacheEnabled attribute
            var cacheEnabledAttribute = methodInfo.GetCustomAttribute<CacheEnabledAttribute>();
            if (cacheEnabledAttribute != null)
            {
                // Extract method parameters
                var parameters = context.BindingContext.BindingData["Query"];
                // Use CacheInterceptor to handle caching
                var result = await _cacheInterceptor.ExecuteWithCacheAsync(
                    () => ExecuteNextMiddleware(context, next),  // Execute the actual function if not cached
                    methodInfo,
                    parameters
                );

                // If a cached result exists, set it as the function result
                context.GetInvocationResult();
                return;
            }

            await next(context);
        }

        private async Task<InvocationResult> ExecuteNextMiddleware(FunctionContext context, FunctionExecutionDelegate next)
        {
            await next(context);

            // Extract the result of the function execution
            return context.GetInvocationResult();
        }

        private MethodInfo GetMethodByFunctionName(Type functionType, string functionName)
        {
            return functionType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.GetCustomAttribute<FunctionAttribute>()?.Name == functionName);
        }
    }
}
