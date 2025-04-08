[![Nuget](https://img.shields.io/nuget/v/SmartApiResponseCache?style=for-the-badge)](https://www.nuget.org/packages/SmartApiResponseCache)
[![Nuget](https://img.shields.io/nuget/dt/SmartApiResponseCache?style=for-the-badge)](https://www.nuget.org/packages/SmartApiResponseCache)

# SmartApiResponseCache

`SmartApiResponseCache` is a flexible and efficient middleware for .NET APIs that implements HTTP response caching. It stores successful responses (2XX status codes) based on session and request data, improving API performance by avoiding repeated calls to the same endpoints. The cache can be stored in-memory or in a customizable storage solution like Redis.

## Features

- Caches successful responses (2XX status codes).
- Customizable cache duration per endpoint.
- Customizable cache case sensitive for query strings for a default or per endpoint.
- Cache invalidation and disabling options per endpoint.
- Supports in-memory caching (with the ability to integrate other storage systems such as Redis).
- Customizable to match your project requirements

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
    "IsCacheEnabled":  true,
    "ContentTypes" : [ "application/json", "application/xml", "text/plain" ],
    "IsQueryStringCaseSensitive": true
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
    bool TryGetCachedResponse(string cacheKey, out CachedResponseEntry cachedEntry);
    void CacheResponse(string cacheKey, byte[] response, TimeSpan duration, int statusCode, string contentType, IHeaderDictionary headers);
}
```
If you want to customize, don't use the extension method AddSmartResponseMemoryCache() to add into Services and use your own.

#### 2.1.1 Create Your Custom Cache Service

For example, if you want to use Redis as a storage solution, implement the `ISmartCacheService` interface to interact with Redis:

```csharp
public class RedisSmartCacheService : ISmartCacheService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ICacheKeyGenerator CacheKeyGenerator
    
    public RedisSmartCacheService(IConnectionMultiplexer redisConnection, ICacheKeyGenerator cacheKeyGenerator)
    {
        _redisConnection = redisConnection;
    }

    public async Task<string> GenerateCacheKeyAsync(HttpContext context)
    {
        string keyBuilder = await CacheKeyGenerator.GenerateKey(context);
        string myKey;
        // Implement your key generation logic here
        return myKey;
    }

    public bool TryGetCachedResponse(string cacheKey, CachedResponseEntry cachedEntry)
    {
        // Implement your cache retrieval logic here (e.g., using Redis)
    }

    public void CacheResponse(string cacheKey, byte[] response, TimeSpan duration, int statusCode, string contentType, IHeaderDictionary headers)
    {
        // Implement your cache storage logic here (e.g., saving to Redis)
    }
}
```

You can use the `ICacheKeyGenerator` or not.

#### 2.1.2 Register Your Custom Cache Service

Replace the default service with your custom implementation in the `ConfigureServices` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ISmartCacheService, RedisSmartCacheService>();
}
```

#### 2.2.1 Create Your Custom Cache Key Generator

For example, if you want to change how the key is generated, implement the `ICacheKeyGenerator` interface to interact with:

```csharp
public class CustomCacheKeyGeneratorHandler(IHeaderKeyGenerator headerKeyGenerator,
    IUserKeyGenerator userKeyGenerator, IOptions<SmartCacheOptions> options) : ICacheKeyGenerator
{
    public async Task<string> GenerateKey(HttpContext context)
    {
        // Implement your key generation logic here
    }
}
```

#### 2.2.2 Register Your Custom Cache Key Generator

Replace the default service with your custom implementation in the `CustomCacheKeyGeneratorHandler` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ICacheKeyGenerator, CustomCacheKeyGeneratorHandler>();
}
```

#### 2.3.1 Create Your Custom User Key Generator

For example, if you want to change how the user key is generated, implement the `IUserKeyGenerator` interface to interact with:

```csharp
public class CustomCreateUserKeyHandler : IUserKeyGenerator
{
    public string CreateUserKey(HttpContext context)
    {
        // Implement your key generation logic here
    }
}
```

#### 2.3.2 Register Your Custom Cache Key Generator

Replace the default service with your custom implementation in the `CustomCreateUserKeyHandler` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IUserKeyGenerator, CustomCreateUserKeyHandler>();
}
```

#### 2.4.1 Create Your Custom Header Key Generator

For example, if you want to change how the header key is generated, implement the `IHeaderKeyGenerator` interface to interact with:

```csharp
public class CustomHeadersContextHandlerr : IHeaderKeyGenerator
{
    public string AddHeaders(HttpContext context)
    {
        // Implement your key generation logic here
    }
}
```

#### 2.4.2 Register Your Custom Cache Key Generator

Replace the default service with your custom implementation in the `CustomHeadersContextHandlerr` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IHeaderKeyGenerator, CustomHeadersContextHandlerr>();
}
```

### Step 3.1: (Optional) Enable Caching for Specific Endpoints

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

### Step 3.2: (Optional) Customize Cache Duration Per Endpoint

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

If you use `[SmartCacheAttribute]` or `.WithoutSmartCache(10)` will enabled the cache for the endpoint only with the seconds you request.

### Step 3.3: (Optional) Disable Caching for Specific Endpoints

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
.WithoutSmartCache();
```

### Step 3.4: (Optional) Make Caching Case Sensitive for about the QueryString for Specific Endpoints

If you has default disable caching, then can enabled for specific endpoints, you can use the `EnabledSmartCacheAttribute`:

```csharp
[CaseSensitiveAttribute]
public IActionResult GetCachedData()
{
    // Your action logic here
}

//minimal api
app.Mapget("/weatherforecast", async () =>
{
    ...
})
.SmartCacheIsCaseSensitive();
```

## Configuration Options

### `SmartCacheOptions`

This class contains the options for configuring the cache middleware:

- **`DefaultCacheDurationSeconds`**: The default duration (in seconds) for cache entries. Default is `5`.
- **`IsCacheEnabled`**: A flag that indicates whether caching is enabled globally. Default is `true`.
- **`ContentTypes`**: Array with cacheable ContentTypes. Don't have default value, but if it's not set should use `[ "application/json", "application/xml", "text/plain" ]`.
- **`IsQueryStringCaseSensitive`**: A flag that indicates whether caching is CASE SENSITIVE for the query strings globally. Default is `false`.

Example:

```csharp
public class SmartCacheOptions
{
    public static string SectionKey = nameof(SmartCacheOptions);
    public int DefaultCacheDurationSeconds { get; set; } = 5;
    public bool IsCacheEnabled { get; set; } = true;
    public string[] ContentTypes { get; set; }
    public bool IsQueryStringCaseSensitive { get; set; } = false;
}
```

### `ISmartCacheService`

Implement this interface to create your custom cache store. This allows you to use various caching systems like Redis, SQL, etc.

- **`GenerateCacheKeyAsync`**: Generates a unique cache key for the current HTTP request.
- **`TryGetCachedResponse`**: Attempts to retrieve a cached response by key.
- **`CacheResponse`**: Caches a response with a specified duration and status code.

### `CachedResponseEntry`

Store the data to be cache.

- **`Body`**: HTTP response body bytes.
- **`StatusCode`**: HTTP response status.
- **`ContentType`**: HTTP response ContentType.
- **`Headers`**: HTTP response Headers. When cache is hit should add `x-smartapiresponsecache: HIT`.


## Troubleshooting

- Ensure that the cache service is correctly registered in the DI container (`AddSmartResponseMemoryCache()`).
- Check that the response body stream is properly handled when caching (read and reset the stream position).
- Verify that cache retrieval logic (e.g., Redis or memory cache) is working as expected.

## License

This project is licensed under the MIT License.
