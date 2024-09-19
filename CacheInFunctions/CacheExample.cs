using CacheInFunctions.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CacheInFunctions
{
    public class CacheExample
    {
        private readonly ILogger<CacheExample> _logger;
        public CacheExample(ILogger<CacheExample> logger)
        {
            _logger = logger;
        }

        [Function("CacheExample")]
        [CacheEnabled(ExpirationInSeconds =30)]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            req.HttpContext.Request.Query.TryGetValue("name", out var name);
            return new OkObjectResult($"Welcome to Azure Functions! {name}");
        }
    }
}
