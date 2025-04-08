namespace SmartApiResponseCache.Middleware.Services;

internal class MemorySmartCacheService : ISmartCacheService
{
    private readonly IMemoryCache Cache;
    private readonly SmartCacheOptions Options;
    private readonly ICacheKeyGenerator CacheKeyGenerator;

    public MemorySmartCacheService(IMemoryCache cache, IOptions<SmartCacheOptions> options, ICacheKeyGenerator cacheKeyGenerator)
    {
        Cache = cache;
        Options = options.Value;
        CacheKeyGenerator = cacheKeyGenerator;
    }

    public async Task<string> GenerateCacheKeyAsync(HttpContext context)
    {
        string keyBuilder = await CacheKeyGenerator.GenerateKey(context);
        using SHA256 sha = SHA256.Create();
        byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(keyBuilder));
        string cacheKey = Convert.ToBase64String(hashBytes);
        return cacheKey;
    }

    public bool TryGetCachedResponse(string cacheKey, out CachedResponseEntry cachedEntry)
    {
        cachedEntry = null;
        bool result = false;
        if(Cache.TryGetValue(cacheKey, out string json))
        {
            cachedEntry = JsonSerializer.Deserialize<CachedResponseEntry>(json);
            result = cachedEntry != null;
        }
        return result;
    }

    public void CacheResponse(string cacheKey, byte[] response, TimeSpan duration,
        int statusCode, string contentType, IHeaderDictionary headers)
    {
        if(IsDataContentType(contentType))
        {
            Dictionary<string, string[]> headerMap = headers
                .Where(h => !string.Equals(h.Key, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
           .ToDictionary(h => h.Key, h => h.Value.ToArray());

            CachedResponseEntry entry = new CachedResponseEntry
            {
                Body = response,
                StatusCode = statusCode,
                ContentType = contentType,
                Headers = headerMap
            };
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            };
            Cache.Set(cacheKey, JsonSerializer.Serialize(entry), options);
        }
    }

    private bool IsDataContentType(string contentType)
    {
        string[] dataContentTypes = Options?.ContentTypes?.Any() ?? false
            ? Options.ContentTypes
            : ["application/json", "application/xml", "text/plain"];
        return dataContentTypes.Any(type => contentType.StartsWith(type, StringComparison.OrdinalIgnoreCase));
    }
}