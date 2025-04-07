namespace SmartApiResponseCache.Middleware.Models;
public class CachedResponseEntry
{
    public byte[] Body { get; set; }
    public int StatusCode { get; set; }
    public string ContentType { get; set; }
    public Dictionary<string, string[]> Headers { get; set; }
}
