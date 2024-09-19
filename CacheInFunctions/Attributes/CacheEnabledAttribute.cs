namespace CacheInFunctions.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CacheEnabledAttribute : Attribute
    {
        public int ExpirationInSeconds { get; set; }

        public CacheEnabledAttribute() { }
        
        public CacheEnabledAttribute(int expirationInSeconds) 
        {
            ExpirationInSeconds = expirationInSeconds;
        }
    }
}
