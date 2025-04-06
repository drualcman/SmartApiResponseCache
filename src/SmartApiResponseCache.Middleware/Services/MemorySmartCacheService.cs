namespace SmartApiResponseCache.Middleware.Services;

internal class MemorySmartCacheService : ISmartCacheService
{
    private readonly IMemoryCache _cache;

    public MemorySmartCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<string> GenerateCacheKeyAsync(HttpContext context)
    {
        StringBuilder keyBuilder = context.GenerateKey();
        if(context.Request.Method == HttpMethods.Post ||
           context.Request.Method == HttpMethods.Put ||
           context.Request.Method == HttpMethods.Patch)
        {
            context.Request.EnableBuffering();
            using StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            keyBuilder.Append("|");
            keyBuilder.Append(body);
        }
        using SHA256 sha = SHA256.Create();
        byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(keyBuilder.ToString()));
        string cacheKey = Convert.ToBase64String(hashBytes);
        return cacheKey;
    }


    public bool TryGetCachedResponse<T>(string cacheKey, out T cachedResponse, out int cachedStatusCode)
    {
        cachedResponse = default;
        cachedStatusCode = StatusCodes.Status200OK;
        bool result = false;
        if(_cache.TryGetValue(cacheKey, out string response))
        {
            cachedResponse = JsonSerializer.Deserialize<T>(response);
            cachedStatusCode = JsonSerializer.Deserialize<int>(_cache.Get(cacheKey + "_statusCode").ToString());
            result = true;
        }
        return result;
    }

    public void CacheResponse<T>(string cacheKey, T response, TimeSpan duration, int statusCode)
    {
        string responseBody = JsonSerializer.Serialize(response);
        MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration
        };
        _cache.Set(cacheKey, responseBody, options);
        _cache.Set(cacheKey + "_statusCode", statusCode, options);
    }
}