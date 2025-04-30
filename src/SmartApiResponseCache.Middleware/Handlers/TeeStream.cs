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

    public override void Flush()
    {
        Original.Flush();
        Copy.Flush();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await Original.FlushAsync(cancellationToken);
        await Copy.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Copy.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return Copy.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        Original.SetLength(value);
        Copy.SetLength(value);
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

    public override bool CanRead => Copy.CanRead;
    public override bool CanSeek => Copy.CanSeek;
    public override bool CanWrite => Original.CanWrite;
    public override long Length => Copy.Length;

    public override long Position
    {
        get => Copy.Position;
        set
        {
            Copy.Position = value;
        }
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Original.Write(buffer);
        Copy.Write(buffer);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.WhenAll(
            Original.WriteAsync(buffer, cancellationToken).AsTask(),
            Copy.WriteAsync(buffer, cancellationToken).AsTask()));
    }

    public override void WriteByte(byte value)
    {
        Original.WriteByte(value);
        Copy.WriteByte(value);
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            Copy.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await Copy.DisposeAsync();
        await base.DisposeAsync();
    }
}
