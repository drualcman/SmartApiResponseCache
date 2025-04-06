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
}
