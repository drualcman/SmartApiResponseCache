namespace SmartApiResponseCache.Middleware.Interfaces;
public interface IHeaderKeyGenerator
{
    string AddHeaders(HttpContext context);
}
