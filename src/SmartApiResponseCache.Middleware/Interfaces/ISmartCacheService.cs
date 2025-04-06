namespace SmartApiResponseCache.Middleware.Interfaces;
public interface ISmartCacheService
{
    Task<string> GenerateCacheKeyAsync(HttpContext context);
    bool TryGetCachedResponse<T>(string cacheKey, out T cachedResponse, out int cachedStatusCode);
    void CacheResponse<T>(string cacheKey, T response, TimeSpan duration, int statusCode);
}
