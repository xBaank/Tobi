using System.Buffers;
using DiscordBot.MusicPlayer.DownloadHandlers;
using DiscordBot.MusicPlayer.Extensions;
using FFmpeg.AutoGen;

namespace DiscordBot.MusicPlayer.Buffers;

using static ffmpeg;
using static String;

internal class FFmpegPlayerBuffer : IPlayerBuffer
{
    private const int Tlserror = -10054;
    private readonly object _lock = new();
    private readonly long _totalTime;

    private unsafe AVFormatContext* _formatContext;
    private bool _hasMore;
    private long _lastTime;
    private unsafe AVPacket* _packet;
    private string _url;

    private unsafe FFmpegPlayerBuffer(string url)
    {
        _url = url;
        _formatContext = avformat_alloc_context();
        _packet = av_packet_alloc();
        _formatContext->max_index_size = 100000;
        _formatContext->max_probe_packets = 1;
        _formatContext->probesize = 100000;
        _formatContext->format_probesize = 100000;

        fixed (AVPacket** packet = &_packet)
        fixed (AVFormatContext** formatContext = &_formatContext)
        {
            if (avformat_open_input(formatContext, url, null, null) >= 0)
            {
                _totalTime = _formatContext->duration / 1000;
                return;
            }

            av_packet_free(packet);
            throw new Exception($"Could not open input {url}");
        }
    }

    public TimeSpan CurrentTime => TimeSpan.FromMilliseconds(_lastTime);

    public TimeSpan TotalTime => TimeSpan.FromMilliseconds(_totalTime);


    public Task WriteFile(Stream stream, CancellationToken cancellationToken) => Task.Run(() => WriteFileSync(stream, cancellationToken), cancellationToken);

    public ValueTask<bool> Seek(long timeStamp) => ValueTask.FromResult(SeekTo(timeStamp) >= 0);

    public void Dispose()
    {
        _lastTime = 0;
        _url = Empty;
        _hasMore = false;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            Dispose();
            return default;
        }
        catch (Exception e)
        {
            return new ValueTask(Task.FromException(e));
        }
    }

    private unsafe void WriteFileSync(Stream stream, CancellationToken token)
    {
        if (stream is null) throw new ArgumentNullException(nameof(stream));

        do
        {
            lock (_lock)
            {
                var code = av_read_frame(_formatContext, _packet);

                _hasMore = code >= 0;

                if (_hasMore)
                {
                    _lastTime = _packet->dts;
                }

                //tls error aka connection error
                var isTlsError = code == Tlserror;

                while (isTlsError)
                {
                    fixed (AVFormatContext** formatContext = &_formatContext)
                    {
                        avformat_close_input(formatContext);
                        avformat_open_input(formatContext, _url, null, null); //reopen the connection
                    }

                    isTlsError = SeekTo(_lastTime) == Tlserror;
                    _hasMore = !isTlsError;
                }

                var size = _packet->size;

                if (size == 0)
                {
                    av_packet_unref(_packet);
                    continue;
                }

                var buffer = ArrayPool<byte>.Shared.Rent(size);

                for (var i = 0; i < size; i++)
                {
                    buffer[i] = _packet->data[i];
                }

                av_packet_unref(_packet);

                stream.WriteSharedAsync(buffer, size, token).GetAwaiter().GetResult();
            }
        } while (_hasMore);
    }

    private unsafe void ReleaseUnmanagedResources()
    {
        fixed (AVPacket** packet = &_packet)
        fixed (AVFormatContext** formatContext = &_formatContext)
        {
            av_packet_free(packet);
            avformat_close_input(formatContext);
        }
    }

    ~FFmpegPlayerBuffer() => ReleaseUnmanagedResources();

    public static async ValueTask<IPlayerBuffer> Create(IDownloadUrlHandler downloadUrlHandler, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return new FFmpegPlayerBuffer(await downloadUrlHandler.GetUrl());
    }

    private unsafe int SeekTo(long timeStamp)
    {
        lock (_lock)
        {
            if (timeStamp > _formatContext->duration / 1000 || timeStamp < 0)
                return -1;

            var success = -1;


            for (var i = 0; i < _formatContext->nb_streams; i++)
            {
                success = av_seek_frame(_formatContext, i, timeStamp, 1);
            }

            if (success < 0)
                return success;

            long lastTimeStamp = 0;
            while (lastTimeStamp < timeStamp)
            {
                success = av_read_frame(_formatContext, _packet);
                if (_packet->dts < 0 || success < 0) return success;

                lastTimeStamp = _packet->dts;
            }

            _lastTime = timeStamp;

            return success;
        }
    }
}