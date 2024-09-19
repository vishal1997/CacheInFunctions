using CacheInFunctions.Services;
using CacheInFunctions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<CacheService<string>, InMemoryCacheService<string>>();
        services.AddSingleton<ICacheKeyGenerationStrategy, HashedKeyGenerationStrategy>();
    })
    .Build();*/

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication( worker =>
    {
        worker.UseMiddleware<CacheMiddleware>();
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        var redisConnectionString = Environment.GetEnvironmentVariable("RedisConnectionString");

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            if (redisConnectionString.Contains("UseDevelopmentCache"))
            {
                services.AddSingleton<CacheService<string>, InMemoryCacheService<string>>();
            }
            else
            {
                services.AddSingleton<CacheService<string>>(new RedisCacheService<string>(redisConnectionString));
            }
            services.AddSingleton<ICacheKeyGenerationStrategy, HashedKeyGenerationStrategy>();
            services.AddSingleton<CacheInterceptor>();
        }
    })
    .Build();

host.Run();
