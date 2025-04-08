namespace Microsoft.Extensions.DependencyInjection;

public static partial class DependencyContainer
{
    public static IServiceCollection AddSmartResponseMemoryCache(this IServiceCollection services,
        Action<SmartCacheOptions> options = null)
    {
        if(options == null)
        {
            SmartCacheOptions cache = new();
            services.Configure<SmartCacheOptions>(o =>
            {
                o.DefaultCacheDurationSeconds = cache.DefaultCacheDurationSeconds;
                o.IsCacheEnabled = cache.IsCacheEnabled;
            });
        }
        else
            services.Configure(options);
        services.AddMemoryCache();
        services.AddSingleton<IUserKeyGenerator, CreateUserKeyHandler>();
        services.AddSingleton<IHeaderKeyGenerator, HeadersContextHandler>();
        services.AddSingleton<ICacheKeyGenerator, CacheKeyGeneratorHandler>();
        services.AddSingleton<ISmartCacheService, MemorySmartCacheService>();
        return services;
    }

    public static IApplicationBuilder UseSmartApiResponseCache(this IApplicationBuilder app)
    {
        app.UseMiddleware<SmartCacheMiddleware>();
        return app;
    }
}
