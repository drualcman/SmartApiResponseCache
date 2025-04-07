namespace SmartApiResponseCache.Middleware.Helpers;
internal static class HttpContextHelper
{
    public static StringBuilder GenerateKey(this HttpContext context, bool isCaseSensitive)
    {
        StringBuilder keyBuilder = new StringBuilder();
        keyBuilder.Append(CreateUserKey(context));
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Method.ToUpperInvariant());
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Scheme.ToLowerInvariant()); // http o https
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Host.ToString().ToLowerInvariant()); // api.com
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Path.ToString().ToLowerInvariant()); // /endpoint/1
        keyBuilder.Append("|");
        string queryString = context.Request.QueryString.ToString();
        if(!isCaseSensitive)
            queryString = queryString.ToLowerInvariant();
        keyBuilder.Append(queryString);
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

    public static bool ShouldUseCaseSensitiveQuery(this HttpContext context, SmartCacheOptions options)
    {
        Endpoint endpoint = context.GetEndpoint();
        bool result = options.IsQueryStringCaseSensitive;
        CaseSensitiveAttribute cacheAttr = endpoint?.Metadata.GetMetadata<CaseSensitiveAttribute>();
        if(cacheAttr != null)
        {
            result = true;
        }
        return result;
    }
}
