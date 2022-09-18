﻿using System.Net.Http.Headers;
using DiscordBot.MusicPlayer.DownloadHandlers;

namespace DiscordBot.MusicPlayer.Containers.Matroska;

internal sealed class HttpSegmentedStream : Stream
{
    private readonly IDownloadUrlHandler _downloadUrlHandler;
    private readonly HttpClient _httpClient;
    private Stream? _httpStream;
    private bool _positionChanged;

    private HttpSegmentedStream(IDownloadUrlHandler downloadUrlHandler, HttpClient httpClient, long initialPos, int bufferSize)
    {
        _downloadUrlHandler = downloadUrlHandler;
        _httpClient = httpClient;
        BufferSize = bufferSize;
        Position = initialPos;
    }

    public int BufferSize { get; set; }
    public HttpCompletionOption CompletionOption { get; set; } = HttpCompletionOption.ResponseHeadersRead;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position { get; set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient.Dispose();
            _httpStream?.Dispose();
        }

        base.Dispose(disposing);
    }

    //The smaller the size is the faster it will initially loaded
    //The larger the size is the faster it will be when reading all the url data
    public static ValueTask<HttpSegmentedStream> Create(IDownloadUrlHandler downloadUrlHandler, long initialPos = 0, int bufferSize = 9_898_989)
    {
        var httpClient = new HttpClient();
        return ValueTask.FromResult(new HttpSegmentedStream(downloadUrlHandler, httpClient, initialPos, bufferSize));
    }

    public override void Flush() => throw new NotImplementedException();

    public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_httpStream is null || _positionChanged)
        {
            await ReadNextChunk(cancellationToken);
            _positionChanged = false;
        }

        int bytesLeftToRead;
        var totalRead = 0;

        do
        {
            var read = await _httpStream!.ReadAsync(buffer, cancellationToken);

            Position += read;

            bytesLeftToRead = buffer.Length - read;
            totalRead += read;

            buffer = buffer[read..];

            if (bytesLeftToRead > 0)
                await ReadNextChunk(cancellationToken);
        } while (bytesLeftToRead > 0);

        return totalRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => await ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken);

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:

                if (offset == Position) return Position;

                Position = offset;
                break;

            case SeekOrigin.Current:

                var pos = offset + Position;

                if (pos == Position) return Position;

                Position = pos;
                break;

            case SeekOrigin.End:
                throw new NotSupportedException();

            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }

        _positionChanged = true;
        return Position;
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    private async Task ReadNextChunk(CancellationToken cancellationToken)
    {
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, await _downloadUrlHandler.GetUrl());
        httpRequestMessage.Headers.Range = new RangeHeaderValue(Position, Position + BufferSize - 1);


        var response = await _httpClient.SendAsync(httpRequestMessage, CompletionOption, cancellationToken);

        if (_httpStream is not null)
            await _httpStream.DisposeAsync();

        _httpStream = await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}