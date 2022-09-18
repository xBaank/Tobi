﻿using System.Buffers;
using DiscordBot.MusicPlayer.Containers.Matroska;
using DiscordBot.MusicPlayer.Containers.Matroska.EBML;
using DiscordBot.MusicPlayer.Containers.Matroska.Elements;
using DiscordBot.MusicPlayer.Containers.Matroska.Extensions.EbmlExtensions;
using DiscordBot.MusicPlayer.Containers.Matroska.Extensions.MatroskaExtensions;
using DiscordBot.MusicPlayer.Containers.Matroska.Parsers;
using DiscordBot.MusicPlayer.Containers.Matroska.Types;
using DiscordBot.MusicPlayer.DownloadHandlers;
using DiscordBot.MusicPlayer.Extensions;

namespace DiscordBot.MusicPlayer.Buffers;

using static ElementTypes;

internal class MatroskaPlayerBuffer : IPlayerBuffer
{
    private readonly EbmlReader _ebmlReader;

    private readonly Stream _inputStream;
    private List<AudioTrack>? _audioTracks;
    private List<CuePoint>? _cuePoints;
    private IMemoryOwner<byte> _memoryOwner = MemoryPool<byte>.Shared.Rent(1024);

    private long _seekTime;
    private CancellationTokenSource _seekToken;

    private MatroskaPlayerBuffer(Stream stream)
    {
        _inputStream = stream;
        _ebmlReader = new EbmlReader(stream);
        _seekToken = new CancellationTokenSource();
    }

    public void Dispose()
    {
        _ebmlReader.Dispose();
        _inputStream.Dispose();
        _memoryOwner.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _ebmlReader.DisposeAsync();
        await _inputStream.DisposeAsync();
        _memoryOwner.Dispose();
    }

    public TimeSpan CurrentTime { get; private set; } = TimeSpan.Zero;

    public TimeSpan TotalTime { get; private set; }

    public async Task WriteFile(Stream stream, CancellationToken cancellationToken)
    {
        if (_cuePoints is null)
            throw new Exception("No cues found");

        if (_audioTracks is null)
            throw new Exception("No audio tracks found");

        var cuePointToSeekFrom = _cuePoints.OrderByDescending(i => i.Time).First(i => i.Time <= _seekTime);
        var cues = _cuePoints.Where(i => i.Time >= cuePointToSeekFrom.Time);
        var clusters = cues.Select(cuePoint => new {Cuetrack = cuePoint.TrackPositions.First(i => i.Track == _audioTracks[0].Number), CueTime = cuePoint.Time});

        _seekToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            foreach (var cue in clusters)
                await WriteCluster(stream, cue.Cuetrack.ClusterPosition, _seekToken.Token);
        }
        catch (OperationCanceledException)
        {
            //If only seek was canceled, we play again from the requested time
            //Maybe recursion is not the best way to do this ?
            if (!cancellationToken.IsCancellationRequested)
                await WriteFile(stream, cancellationToken);
            else
                throw;
        }
    }

    public ValueTask<bool> Seek(long timeStamp)
    {
        if (timeStamp > TotalTime.TotalMilliseconds || timeStamp < 0)
            return ValueTask.FromResult(false);

        _seekTime = timeStamp;
        _seekToken.Cancel();
        return ValueTask.FromResult(true);
    }

    private async ValueTask WriteCluster(Stream stream, long pos, CancellationToken cancellationToken)
    {
        var cluster = await _ebmlReader.Read(pos, cancellationToken).PipeAsync(i => i.As(Cluster)) ?? throw new Exception("Cluster not found");

        TimeSpan? time = null;

        await foreach (var matroskaElement in _ebmlReader.ReadAll(cluster.Size, cancellationToken))
        {
            //TODO use timescale here
            if (matroskaElement.Id == Timestamp.Id)
            {
                time = await _ebmlReader.TryReadUlong(matroskaElement, cancellationToken)
                    .PipeAsync(i => i ?? throw new Exception("Cluster time not found"))
                    .PipeAsync(i => TimeSpan.FromMilliseconds(i));

                continue;
            }

            await WriteBlock(stream, matroskaElement, time ?? throw new Exception("TimeStamp was not the first element on the cluster"), cancellationToken);
        }
    }

    private async ValueTask WriteBlock(Stream stream, MatroskaElement matroskaElement, TimeSpan time, CancellationToken cancellationToken)
    {
        if (matroskaElement.Id == SimpleBlock.Id)
        {
            var size = (int) matroskaElement.Size;

            if (_memoryOwner.Memory.Length < size)
            {
                _memoryOwner.Dispose();
                _memoryOwner = MemoryPool<byte>.Shared.Rent(size);
            }

            var memory = _memoryOwner.Memory[..size];

            var block = await _ebmlReader.GetSimpleBlock(memory, cancellationToken);
            CurrentTime = time + TimeSpan.FromMilliseconds(block.Timestamp);

            if (CurrentTime.TotalMilliseconds < _seekTime)
                return;

            foreach (var frame in block.GetFrames())
                await stream.WriteAsync(frame, cancellationToken);

            return;
        }

        //TODO handle blocks

        await _ebmlReader.Skip(matroskaElement.Size, cancellationToken);
    }

    private async Task LoadFileInfo(CancellationToken token)
    {
        var headerParser = new EbmlHeaderParser(_ebmlReader);

        var isValid = await headerParser.TryGetDocType(token)
            .PipeAsync(i => i ?? throw new Exception("Couldn't parse docType"))
            .PipeAsync(i => i is "webm" or "matroska");

        if (!isValid)
            throw new Exception("Invalid DocType file");

        var segmentparser = await SegmentParser.CreateAsync(_ebmlReader, token);

        _cuePoints = await segmentparser.GetCuePoints(token);
        _audioTracks = await segmentparser.GetAudioTracks(token);
        TotalTime = await segmentparser.GetDuration(token);
    }


    public static async ValueTask<IPlayerBuffer> Create(IDownloadUrlHandler downloadUrlHandler, CancellationToken token = default)
    {
        var stream = await HttpSegmentedStream.Create(downloadUrlHandler, bufferSize: 1024 * 10);
        stream.CompletionOption = HttpCompletionOption.ResponseContentRead;

        var playerBuffer = new MatroskaPlayerBuffer(stream);
        await playerBuffer.LoadFileInfo(token);

        stream.BufferSize = 9_898_989;
        stream.CompletionOption = HttpCompletionOption.ResponseHeadersRead;
        return playerBuffer;
    }
}