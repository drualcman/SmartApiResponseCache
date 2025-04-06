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
        string cacheKey = await CacheService.GenerateCacheKeyAsync(context);
        var endpoint = context.GetEndpoint();
        if(!endpoint.ShouldCacheable(Options))
        {
            await Next(context);
        }
        else
        {
            if(CacheService.TryGetCachedResponse(cacheKey, out string cachedResponse, out int cachedStatusCode))
            {
                Logger?.LogInformation($"Cache hit for {cacheKey}");
                context.Response.StatusCode = cachedStatusCode;
                if(cachedStatusCode != StatusCodes.Status204NoContent && cachedStatusCode != StatusCodes.Status205ResetContent)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(cachedResponse);
                }
            }
            else
            {
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;
                await Next(context);

                if(context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    var smartCacheAttribute = endpoint?.Metadata.GetMetadata<SmartCacheAttribute>();
                    TimeSpan cacheDuration = smartCacheAttribute != null
                        ? TimeSpan.FromSeconds(smartCacheAttribute.DurationInSeconds)
                        : TimeSpan.FromSeconds(Options.DefaultCacheDurationSeconds);

                    if(context.Response.StatusCode == StatusCodes.Status204NoContent || context.Response.StatusCode == StatusCodes.Status205ResetContent)
                    {
                        CacheService.CacheResponse(cacheKey, "", cacheDuration, context.Response.StatusCode);
                    }
                    else
                    {
                        responseBody.Seek(0, SeekOrigin.Begin);
                        string responseBodyString = await new StreamReader(responseBody).ReadToEndAsync();
                        CacheService.CacheResponse(cacheKey, responseBodyString, cacheDuration, context.Response.StatusCode);
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
            }
        }
    }
}