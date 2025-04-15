namespace SmartApiResponseCache.Middleware;

internal class SmartCacheMiddleware
{
    private readonly RequestDelegate Next;
    private readonly ISmartCacheService CacheService;
    private readonly SmartCacheOptions Options;
    private readonly ILogger<SmartCacheMiddleware> Logger;

    public SmartCacheMiddleware(RequestDelegate next, ISmartCacheService cacheService, IOptions<SmartCacheOptions> options, ILogger<SmartCacheMiddleware> logger = null)
    {
        Next = next;
        CacheService = cacheService;
        Options = options.Value;
        Logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if(!context.ShouldCacheable(Options))
        {
            Logger?.LogDebug($"Skipping caching. No cacheable Endpoint: '{context.GetEndpointName()}'.");
            await Next(context);
        }
        else
        {
            string cacheKey = await CacheService.GenerateCacheKeyAsync(context);
            bool cacheHitHandled = false;
            try
            {
                if(CacheService.TryGetCachedResponse(cacheKey, out CachedResponseEntry cachedEntry))
                {
                    Logger?.LogDebug($"Cache hit for {cacheKey} Endpoint: '{context.GetEndpointName()}'");
                    context.Response.StatusCode = cachedEntry.StatusCode;
                    if(!context.Response.HasStarted)
                    {
                        if(cachedEntry.Headers != null)
                        {
                            foreach(KeyValuePair<string, string[]> header in cachedEntry.Headers)
                            {
                                context.Response.Headers[header.Key] = new StringValues(header.Value);
                            }
                        }

                        if(cachedEntry.StatusCode != StatusCodes.Status204NoContent &&
                            cachedEntry.StatusCode != StatusCodes.Status205ResetContent)
                            context.Response.ContentType = cachedEntry.ContentType ?? "application/json";
                    }
                    context.Response.Headers["X-SmartApiResponseCache"] = "HIT";
                    if(cachedEntry.StatusCode != StatusCodes.Status204NoContent &&
                        cachedEntry.StatusCode != StatusCodes.Status205ResetContent)
                        await context.Response.Body.WriteAsync(cachedEntry.Body, 0, cachedEntry.Body.Length);
                    cacheHitHandled = true;
                }
            }
            catch(Exception ex)
            {
                Logger?.LogWarning(ex, $"Cache error for key {cacheKey}. Falling back to normal execution.");
            }

            if(!cacheHitHandled && !context.Response.HasStarted)
            {
                Logger?.LogDebug($"Cache miss for {cacheKey} Endpoint: '{context.GetEndpointName()}'");
                Stream originalBodyStream = context.Response.Body;
                using MemoryStream responseBody = new MemoryStream();
                context.Response.Body = new TeeStream(originalBodyStream, responseBody);
                await Next(context);
                if(context.Response.StatusCode >= 200 && context.Response.StatusCode < 300 &&
                    context.Response.Body.CanRead && context.Response.Body.CanSeek)
                {
                    string contentType = context.Response.ContentType ?? string.Empty;
                    Endpoint endpoint = context.GetEndpoint();
                    SmartCacheAttribute smartCacheAttribute = endpoint.Metadata.GetMetadata<SmartCacheAttribute>();
                    TimeSpan cacheDuration = smartCacheAttribute != null
                        ? TimeSpan.FromSeconds(smartCacheAttribute.DurationInSeconds)
                        : TimeSpan.FromSeconds(Options.DefaultCacheDurationSeconds);
                    byte[] responseBodyBytes = responseBody.ToArray();
                    CacheService.CacheResponse(cacheKey, responseBodyBytes, cacheDuration,
                        context.Response.StatusCode, contentType, context.Response.Headers);
                }
            }
            else
                Logger?.LogInformation($"Response has already started. Skipping writing to body.");
        }
    }
}