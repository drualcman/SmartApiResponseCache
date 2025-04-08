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
}
