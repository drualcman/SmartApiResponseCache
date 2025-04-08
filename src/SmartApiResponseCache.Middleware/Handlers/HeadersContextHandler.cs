namespace SmartApiResponseCache.Middleware.Handlers;
internal class HeadersContextHandler(IOptions<SmartCacheOptions> options) : IHeaderKeyGenerator
{
    public string AddHeaders(HttpContext context)
    {
        StringBuilder keyBuilder = new();
        bool isCaseSensitive = options.Value.IsQueryStringCaseSensitive;
        List<string> headers = new List<string>();
        foreach(KeyValuePair<string, StringValues> header in context.Request.Headers)
        {
            string headerKey = isCaseSensitive
                ? header.Key
                : header.Key.ToLowerInvariant();
            List<string> values = header.Value.ToList();
            string headerValue = isCaseSensitive
                ? string.Join(",", values)
                : string.Join(",", values).ToLowerInvariant();
            headers.Add($"{headerKey}:{headerValue}");
        }
        headers.Sort();
        foreach(string header in headers)
        {
            keyBuilder.Append("|");
            keyBuilder.Append(header);
        }
        return keyBuilder.ToString();
    }
}
