namespace DiscordBot.Proxies.Dsharp;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext;

[ExcludeFromCodeCoverage]
public class TransmitSinkOpusProxy : Stream
{
    private readonly VoiceTransmitSink _voiceTransmitSink;

    public TransmitSinkOpusProxy(VoiceTransmitSink voiceNextConnection) => _voiceTransmitSink = voiceNextConnection;

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotImplementedException();

    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override void Flush() => _voiceTransmitSink.FlushAsync().GetAwaiter().GetResult();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => _voiceTransmitSink.WriteOpusPacketAsync(new ReadOnlyMemory<byte>(buffer, offset, count)).GetAwaiter().GetResult();

    public override async Task FlushAsync(CancellationToken cancellationToken) => await _voiceTransmitSink.FlushAsync(cancellationToken);

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => await _voiceTransmitSink.WriteOpusPacketAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new()) => await _voiceTransmitSink.WriteOpusPacketAsync(buffer, cancellationToken);
}
