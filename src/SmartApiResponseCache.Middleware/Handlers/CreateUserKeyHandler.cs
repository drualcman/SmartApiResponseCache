namespace SmartApiResponseCache.Middleware.Handlers;
internal class CreateUserKeyHandler : IUserKeyGenerator
{
    public string CreateUserKey(HttpContext context)
    {
        StringBuilder userKey = new();
        try
        {
            userKey.Append(context.Session.Id);
        }
        catch
        {
            userKey = new();
        }
        if(userKey.Length == 0)
        {
            userKey.Append(context.User?.Identity?.IsAuthenticated == true
                ? $"{context.User.Identity.Name}_{context.Connection.RemoteIpAddress?.ToString()}"
                : context.Connection.RemoteIpAddress?.ToString());
            userKey.Append($"|{context.Request.Headers["User-Agent"].ToString()}");
        }
        return userKey.ToString();
    }
}
