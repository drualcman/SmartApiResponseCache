namespace SmartApiResponseCache.Middleware.Handlers;
internal class TeeStream : Stream
{
    private readonly Stream Original;
    private readonly Stream Copy;

    public TeeStream(Stream original, Stream copy)
    {
        Original = original;
        Copy = copy;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Original.Write(buffer, offset, count);
        Copy.Write(buffer, offset, count);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await Original.WriteAsync(buffer, offset, count, cancellationToken);
        await Copy.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override bool CanRead => Original.CanRead;
    public override bool CanSeek => Original.CanSeek;
    public override bool CanWrite => Original.CanWrite;
    public override long Length => Original.Length;
    public override long Position { get => Original.Position; set => Original.Position = value; }
    public override void Flush() => Original.Flush();
    public override int Read(byte[] buffer, int offset, int count) => Original.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => Original.Seek(offset, origin);
    public override void SetLength(long value) => Original.SetLength(value);
}