namespace SmartApiResponseCache.Middleware.Helpers;
internal static class EndpointHelper
{
    public static bool ShouldCacheable(this Endpoint endpoint, SmartCacheOptions options)
    {
        bool shouldCacheable = options.IsCacheEnabled;
        if(!options.IsCacheEnabled)
        {
            shouldCacheable = endpoint?.Metadata.GetMetadata<EnabledSmartCacheAttribute>() != null;
        }
        if(endpoint?.Metadata.GetMetadata<NoSmartCacheAttribute>() != null)
        {
            shouldCacheable = false;
        }
        return shouldCacheable;
    }


    public static bool IsHttpMethodValidForEndpoint(this Endpoint endpoint, string method)
    {
        bool found = false;
        foreach(HttpMethodMetadata metadata in endpoint.Metadata.OfType<HttpMethodMetadata>())
        {
            if(metadata.HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
            {
                found = true;
            }
        }
        return found;
    }
}
