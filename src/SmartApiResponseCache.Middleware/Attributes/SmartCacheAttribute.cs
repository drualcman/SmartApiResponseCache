namespace SmartApiResponseCache.Middleware.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SmartCacheAttribute : Attribute
{
    public int DurationInSeconds { get; }

    public SmartCacheAttribute(int durationInSeconds)
    {
        DurationInSeconds = durationInSeconds;
    }
}
