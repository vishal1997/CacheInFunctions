using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;

namespace CacheInFunctions
{
    public static class FunctionContextExtensions
    {
        public static bool IsHttpFunction(this FunctionContext context)
        {
            return context.FunctionDefinition.InputBindings.Values
                   .Any(binding => binding.Type.EndsWith("HttpTrigger", StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<HttpRequestData> GetHttpRequestDataAsync(this FunctionContext context)
        {
            return await context.GetHttpRequestDataAsync();
        }

        public static HttpResponseData GetHttpResponseData(this FunctionContext context)
        {
            var invocationResult = context.GetInvocationResult();
            return invocationResult.Value as HttpResponseData;
        }

        public static async Task WriteToResponseAsync(this HttpResponseData response, FunctionContext context)
        {
            var httpResponse = response;
            var httpRequest = await context.GetHttpRequestDataAsync();
            var outputBinding = context.BindingContext.BindingData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            outputBinding["$return"] = httpResponse;
        }
    }

}
