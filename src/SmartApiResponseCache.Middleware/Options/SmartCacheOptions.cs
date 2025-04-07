namespace SmartApiResponseCache.Middleware.Options;
public class SmartCacheOptions
{
    public static string SectionKey = nameof(SmartCacheOptions);
    public int DefaultCacheDurationSeconds { get; set; } = 5;
    public bool IsCacheEnabled { get; set; } = true;
    public string[] ContentTypes { get; set; }
    public bool IsQueryStringCaseSensitive { get; set; } = false;
}

