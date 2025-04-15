namespace SmartApiResponseCache.Middleware.Handlers;
internal class HeadersContextHandler(IOptions<SmartCacheOptions> options) : IHeaderKeyGenerator
{
    public string AddHeaders(HttpContext context)
    {
        StringBuilder keyBuilder = new();
        bool isCaseSensitive = options.Value.IsQueryStringCaseSensitive;
        var excludedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Date", "Set-Cookie", "Cache-Control", "Expires", "Pragma", "Last-Modified"
            };

        if(options?.Value?.ExcludedHeaders != null)
        {
            foreach(var header in options.Value.ExcludedHeaders)
            {
                excludedHeaders.Add(header);
            }
        }
        Dictionary<string, string[]> headerMap = context.Response.Headers
            .Where(h => !excludedHeaders.Contains(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToArray());

        List<string> headers = new List<string>();
        foreach(KeyValuePair<string, string[]> header in headerMap)
        {
            string headerKey = isCaseSensitive
                ? header.Key
                : header.Key.ToLowerInvariant();

            string headerValue = isCaseSensitive
                ? string.Join(",", header.Value)
                : string.Join(",", header.Value).ToLowerInvariant();
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
