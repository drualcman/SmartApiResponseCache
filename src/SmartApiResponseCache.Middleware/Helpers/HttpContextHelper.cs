namespace SmartApiResponseCache.Middleware.Helpers;
internal static class HttpContextHelper
{
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

    public static bool ShouldCacheable(this HttpContext context, SmartCacheOptions options)
    {
        bool result = false;
        Endpoint endpoint = context.GetEndpoint();
        if(endpoint != null)
        {
            if(endpoint.ShouldCacheable(options) && endpoint.IsHttpMethodValidForEndpoint(context.Request.Method))
            {
                string contentType = context.Response.ContentType ?? string.Empty;
                string[] dataContentTypes = options?.ContentTypes?.Any() ?? false ?
                    options.ContentTypes :
                    ["application/json", "application/xml", "text/plain"];
                result = dataContentTypes.Any(type => contentType.StartsWith(type, StringComparison.OrdinalIgnoreCase));
            }
        }
        return result;
    }

    public static string GetEndpointName(this HttpContext context)
    {
        string result = string.Empty;
        Endpoint endpoint = context.GetEndpoint();
        if(endpoint != null)
            result = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        return result;
    }
}
