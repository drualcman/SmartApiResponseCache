namespace SmartApiResponseCache.Middleware.Interfaces;
public interface ISmartCacheService
{
    Task<string> GenerateCacheKeyAsync(HttpContext context);
    bool TryGetCachedResponse(string cacheKey, out CachedResponseEntry cachedEntry);
    void CacheResponse(string cacheKey, byte[] response, TimeSpan duration, int statusCode, string contentType, IHeaderDictionary headers);
}
