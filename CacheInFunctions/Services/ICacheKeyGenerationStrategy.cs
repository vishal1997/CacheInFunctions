
namespace CacheInFunctions.Services
{
    public interface ICacheKeyGenerationStrategy
    {
        string GenerateKey(string methodName, object parameter);
    }
}
