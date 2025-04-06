namespace SmartApiResponseCache.Middleware.Helpers;
internal static class HttpContextHelper
{
    public static StringBuilder GenerateKey(this HttpContext context)
    {
        StringBuilder keyBuilder = new StringBuilder();
        keyBuilder.Append(CreateUserKey(context));
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Path.ToString().ToLowerInvariant());
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.QueryString.ToString().ToLowerInvariant());
        return keyBuilder;
    }

    private static StringBuilder CreateUserKey(HttpContext context)
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
        return userKey;
    }
}
