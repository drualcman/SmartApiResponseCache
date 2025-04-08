namespace SmartApiResponseCache.Middleware.Interfaces;
public interface IUserKeyGenerator
{
    string CreateUserKey(HttpContext context);
}
