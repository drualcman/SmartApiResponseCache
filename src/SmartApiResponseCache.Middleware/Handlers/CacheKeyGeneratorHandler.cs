namespace SmartApiResponseCache.Middleware.Handlers;

internal class CacheKeyGeneratorHandler(IHeaderKeyGenerator headerKeyGenerator,
    IUserKeyGenerator userKeyGenerator, IOptions<SmartCacheOptions> options) : ICacheKeyGenerator
{
    public async Task<string> GenerateKey(HttpContext context)
    {
        StringBuilder keyBuilder = new(userKeyGenerator.CreateUserKey(context));
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Method.ToUpperInvariant());
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Scheme.ToLowerInvariant()); // http o https
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Host.ToString().ToLowerInvariant()); // api.com
        keyBuilder.Append("|");
        keyBuilder.Append(context.Request.Path.ToString().ToLowerInvariant()); // /endpoint/1
        keyBuilder.Append(headerKeyGenerator.AddHeaders(context));
        keyBuilder.Append("|");
        string queryString = context.Request.QueryString.ToString();
        bool isCaseSensitive = context.ShouldUseCaseSensitiveQuery(options.Value);
        if(!isCaseSensitive)
            queryString = queryString.ToLowerInvariant();
        keyBuilder.Append(queryString);
        if(context.Request.Method == HttpMethods.Post ||
           context.Request.Method == HttpMethods.Put ||
           context.Request.Method == HttpMethods.Patch)
        {
            context.Request.EnableBuffering();
            using StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            keyBuilder.Append("|");
            keyBuilder.Append(body);
        }
        return keyBuilder.ToString();
    }
}
