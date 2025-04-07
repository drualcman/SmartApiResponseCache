namespace SmartApiResponseCache.Middleware.Extensions;

public static class SmartCacheEndpointExtensions
{
    public static IEndpointConventionBuilder WithSmartCache(
        this IEndpointConventionBuilder builder,
        int durationInSeconds)
    {
        builder.Add(endpoint =>
        {
            if(!endpoint.Metadata.Any(m => m is EnabledSmartCacheAttribute))
            {
                endpoint.Metadata.Add(new EnabledSmartCacheAttribute());
            }
        });
        return builder;
    }

    public static IEndpointConventionBuilder WithSmartCacheSeconds(
        this IEndpointConventionBuilder builder,
        int durationInSeconds)
    {
        builder.Add(endpoint =>
        {
            if(!endpoint.Metadata.Any(m => m is EnabledSmartCacheAttribute))
            {
                endpoint.Metadata.Add(new EnabledSmartCacheAttribute());
            }
            endpoint.Metadata.Add(new SmartCacheAttribute(durationInSeconds));
        });
        return builder;
    }

    public static IEndpointConventionBuilder WithoutSmartCache(
        this IEndpointConventionBuilder builder)
    {
        builder.Add(endpoint => endpoint.Metadata.Add(new NoSmartCacheAttribute()));
        return builder;
    }

    public static IEndpointConventionBuilder SmartCacheIsCaseSensitive(
        this IEndpointConventionBuilder builder)
    {
        builder.Add(endpoint => endpoint.Metadata.Add(new CaseSensitiveAttribute()));
        return builder;
    }
}
