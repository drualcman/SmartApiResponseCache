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
        Endpoint endpoint = context.GetEndpoint();
        if(endpoint == null)
        {
            Logger?.LogInformation("No endpoint found for request. Skipping caching.");
            await Next(context);
        }
        else
        {
            string endpointName = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            if(!endpoint.ShouldCacheable(Options) || !endpoint.IsHttpMethodValidForEndpoint(context.Request.Method))
            {
                Logger?.LogDebug($"No cacheable Endpoint: {endpointName}");
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
                        Logger?.LogDebug($"Cache hit for {cacheKey} (Endpoint: {endpointName})");
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
                    Logger?.LogDebug($"Cache miss for {cacheKey} (Endpoint: {endpointName})");
                    Stream originalBodyStream = context.Response.Body;
                    using MemoryStream responseBody = new MemoryStream();
                    context.Response.Body = responseBody;
                    await Next(context);
                    if(context.Response.StatusCode >= 200 && context.Response.StatusCode < 300 &&
                        context.Response.Body.CanRead && context.Response.Body.CanSeek)
                    {
                        string contentType = context.Response.ContentType ?? string.Empty;
                        SmartCacheAttribute smartCacheAttribute = endpoint?.Metadata.GetMetadata<SmartCacheAttribute>();
                        TimeSpan cacheDuration = smartCacheAttribute != null
                            ? TimeSpan.FromSeconds(smartCacheAttribute.DurationInSeconds)
                            : TimeSpan.FromSeconds(Options.DefaultCacheDurationSeconds);
                        byte[] responseBodyBytes = responseBody.ToArray();
                        CacheService.CacheResponse(cacheKey, responseBodyBytes, cacheDuration,
                            context.Response.StatusCode, contentType, context.Response.Headers);
                        context.Response.ContentLength = responseBodyBytes.Length;
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
                else
                    Logger?.LogInformation($"Response has already started. Skipping writing to body.");
            }
        }
    }
}