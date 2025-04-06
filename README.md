# SmartApiResponseCache

`SmartApiResponseCache` is a flexible and efficient middleware for .NET APIs that implements HTTP response caching. It stores successful responses (2XX status codes) based on session and request data, improving API performance by avoiding repeated calls to the same endpoints. The cache can be stored in-memory or in a customizable storage solution like Redis.

## Features

- Caches successful responses (2XX status codes).
- Customizable cache duration per endpoint.
- Cache invalidation and disabling options per endpoint.
- Supports in-memory caching (with the ability to integrate other storage systems such as Redis).

## Installation

1. Install the NuGet package via the package manager:

    ```
    dotnet add package SmartApiResponseCache
    ```

2. Or by using the NuGet CLI:

    ```
    nuget install SmartApiResponseCache
    ```

## Quick Start

### Step 1: Add Cache Middleware to Your API

In your `Startup.cs` (or `Program.cs` if using .NET 6+), you will need to add the middleware to your service collection and configure the cache options.

#### 1.1 Configure Cache Options

You can customize the cache duration and enable/disable the cache via `SmartCacheOptions`.

In appsettings json using `IOptions<SmartCacheOptions>` file like:
```json
  "SmartCacheOptions": {
    "DefaultCacheDurationSeconds": 10,
    "IsCacheEnabled":  true
  }
```

Or directly like:
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSmartResponseMemoryCache(options =>
    {
        // Set the default cache duration (in seconds)
        options.DefaultCacheDurationSeconds = 10;

        // Enable or disable the cache globally
        options.IsCacheEnabled = true;
    });
}

//minimal api
builder.Services.AddSmartResponseMemoryCache();
//Or also can do
/*
builder.Services.AddSmartApiResponseCache(
    options => builder.Configuration.GetSection(SmartCacheOptions.SectionKey).Bind(options)
    );
*/
```

#### 1.2 Add the Middleware to Your Request Pipeline

In your `Configure` method, add the middleware to the pipeline using `UseSmartApiResponseCache()`:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Add other middlewares like routing, authentication, etc.
    
    app.UseSmartApiResponseCache();
}

//minimal api
app.UseSmartApiResponseCache();
```

### Step 2: Customizing the Cache Store

By default, the middleware uses in-memory caching. However, you can customize the cache storage by implementing the `ISmartCacheService` interface.

```csharp
public interface ISmartCacheService
{
    Task<string> GenerateCacheKeyAsync(HttpContext context);
    bool TryGetCachedResponse<T>(string cacheKey, out T cachedResponse, out int cachedStatusCode);
    void CacheResponse<T>(string cacheKey, T response, TimeSpan duration, int statusCode);
}
```
If you want to customize, don't use the extension method AddSmartResponseMemoryCache() to add into Services and use your own.

#### 2.1 Create Your Custom Cache Service

For example, if you want to use Redis as a storage solution, implement the `ISmartCacheService` interface to interact with Redis:

```csharp
public class RedisSmartCacheService : ISmartCacheService
{
    private readonly IConnectionMultiplexer _redisConnection;
    
    public RedisSmartCacheService(IConnectionMultiplexer redisConnection)
    {
        _redisConnection = redisConnection;
    }

    public async Task<string> GenerateCacheKeyAsync(HttpContext context)
    {
        // Implement your key generation logic here
    }

    public bool TryGetCachedResponse<T>(string cacheKey, out T cachedResponse, out int cachedStatusCode)
    {
        // Implement your cache retrieval logic here (e.g., using Redis)
    }

    public void CacheResponse<T>(string cacheKey, T response, TimeSpan duration, int statusCode)
    {
        // Implement your cache storage logic here (e.g., saving to Redis)
    }
}
```

#### 2.2 Register Your Custom Cache Service

Replace the default cache service with your custom implementation in the `ConfigureServices` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSmartResponseMemoryCache(options =>
    {
        options.DefaultCacheDurationSeconds = 10;
        options.IsCacheEnabled = true;
    });

    // Register custom cache service (e.g., Redis)
    services.AddSingleton<ISmartCacheService, RedisSmartCacheService>();
}
```

### Step 3.1: (Optional) Customize Cache Duration Per Endpoint

You can control the cache duration for individual endpoints by using the `SmartCacheAttribute` on your controller actions:

```csharp
[SmartCache(DurationInSeconds = 30)]
public IActionResult GetProducts()
{
    // Your action logic here
}

//minimal api
app.Mapget("/weatherforecast", async () =>
{
    ...
})
.WithSmartCacheSeconds(10);
```

This sets a custom cache duration of 30 seconds for the `GetProducts` action. If not specified, the global default cache duration will be used.

### Step 3.2: (Optional) Disable Caching for Specific Endpoints

If you want to disable caching for specific endpoints, you can use the `NoSmartCacheAttribute`:

```csharp
[NoSmartCache]
public IActionResult GetNonCachedData()
{
    // Your action logic here
}

//minimal api
app.Mapget("/weatherforecast", async () =>
{
    ...
})
.WithoutSmartCache(10);
```

### Step 3.3: (Optional) Enable Caching for Specific Endpoints

If you has default disable caching, then can enabled for specific endpoints, you can use the `EnabledSmartCacheAttribute`:

```csharp
[EnabledSmartCacheAttribute]
public IActionResult GetCachedData()
{
    // Your action logic here
}

//minimal api
app.Mapget("/weatherforecast", async () =>
{
    ...
})
.WithSmartCache();
```

This also if you use `[SmartCacheAttribute]` or `.WithoutSmartCache(10)` will enabled the cache for the endpoint only with the seconds you request.

## Configuration Options

### `SmartCacheOptions`

This class contains the options for configuring the cache middleware:

- **`DefaultCacheDurationSeconds`**: The default duration (in seconds) for cache entries. Default is `5`.
- **`IsCacheEnabled`**: A flag that indicates whether caching is enabled globally. Default is `true`.

Example:

```csharp
public class SmartCacheOptions
{
    public static string SectionKey = nameof(SmartCacheOptions);
    public int DefaultCacheDurationSeconds { get; set; } = 5;
    public bool IsCacheEnabled { get; set; } = true;
}
```

### `ISmartCacheService`

Implement this interface to create your custom cache store. This allows you to use various caching systems like Redis, SQL, etc.

- **`GenerateCacheKeyAsync`**: Generates a unique cache key for the current HTTP request.
- **`TryGetCachedResponse<T>`**: Attempts to retrieve a cached response by key.
- **`CacheResponse`**: Caches a response with a specified duration and status code.

## Troubleshooting

- Ensure that the cache service is correctly registered in the DI container (`AddSmartResponseMemoryCache()`).
- Check that the response body stream is properly handled when caching (read and reset the stream position).
- Verify that cache retrieval logic (e.g., Redis or memory cache) is working as expected.

## License

This project is licensed under the MIT License.
