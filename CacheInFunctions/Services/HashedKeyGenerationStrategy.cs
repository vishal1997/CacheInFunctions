namespace CacheInFunctions.Services
{
    using System.Security.Cryptography;
    using System.Text;

    public class HashedKeyGenerationStrategy : ICacheKeyGenerationStrategy
    {
        public string GenerateKey(string methodName, object parameter)
        {
            string paramKey = parameter.ToString() ?? "null";
            string fullKey = $"{methodName}:{paramKey}";

            // Generate a hash of the key
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fullKey));
                return $"{methodName}:{Convert.ToBase64String(hashBytes)}";
            }
        }
    }

}
