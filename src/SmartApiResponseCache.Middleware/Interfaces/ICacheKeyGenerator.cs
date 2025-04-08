namespace SmartApiResponseCache.Middleware.Interfaces;
public interface ICacheKeyGenerator
{
    Task<string> GenerateKey(HttpContext context);
}
