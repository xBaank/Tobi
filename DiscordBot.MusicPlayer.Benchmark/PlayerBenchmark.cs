namespace DiscordBot.MusicPlayer.Benchmark;

using BenchmarkDotNet.Attributes;
using Controllers;
using Factories;
using MediatR;
using Services;
using YoutubeExplode;

[MemoryDiagnoser]
public class PlayerBenchmark
{
    private const string Query = "DVRST - Dream Space";

    [Benchmark]
    public async Task PlayFromQuery()
    {
        var youtubeService = new YoutubeService(new YoutubeClient());
        var player = new Player(youtubeService, new EmptyMediator(), PlayerBufferFactories.CreateMatroska);
        await player.SetStream(new EmptyStream());
        await player.Add(Query);
        await player.Play();
        await player.WaitFinishPlaying();
    }
}

public class EmptyMediator : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new()) => throw new NotImplementedException();

    public Task<object?> Send(object request, CancellationToken cancellationToken = new()) => throw new NotImplementedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = new()) => throw new NotImplementedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = new()) => throw new NotImplementedException();

    public Task Publish(object notification, CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new()) where TNotification : INotification => Task.CompletedTask;
}

public class EmptyStream : Stream
{
    public override bool CanRead { get; }

    public override bool CanSeek { get; }

    public override bool CanWrite => true;

    public override long Length { get; }

    public override long Position { get; set; }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
