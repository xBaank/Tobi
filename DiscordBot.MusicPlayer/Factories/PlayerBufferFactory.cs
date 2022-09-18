namespace DiscordBot.MusicPlayer.Factories;

using Buffers;
using DownloadHandlers;

/// <summary>
///     Factory for creating a playerBufferImplementation.
/// </summary>
public delegate ValueTask<IPlayerBuffer> PlayerBufferFactory(IDownloadUrlHandler downloadUrlHandler, CancellationToken token = default);

public static class PlayerBufferFactories
{
    [Obsolete("Matroska buffer is much better in terms of memory and performance than this. Use this if there is some problem with the matroska buffer.")]
    public static ValueTask<IPlayerBuffer> CreateFFmpeg(IDownloadUrlHandler downloadUrlHandler, CancellationToken token = default) => FFmpegPlayerBuffer.Create(downloadUrlHandler, token);

    public static ValueTask<IPlayerBuffer> CreateMatroska(IDownloadUrlHandler downloadUrlHandler, CancellationToken token = default) => MatroskaPlayerBuffer.Create(downloadUrlHandler, token);
}
