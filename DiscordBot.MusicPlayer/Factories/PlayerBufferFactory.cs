namespace DiscordBot.MusicPlayer.Factories;

using Buffers;
using DownloadHandlers;

/// <summary>
///     Factory for creating a playerBufferImplementation.
/// </summary>
public delegate ValueTask<IPlayerBuffer> PlayerBufferFactory(IDownloadUrlHandler downloadUrlHandler, CancellationToken token = default);

public static class PlayerBufferFactories
{
    public static ValueTask<IPlayerBuffer> CreateMatroska(IDownloadUrlHandler downloadUrlHandler, CancellationToken token = default) => MatroskaPlayerBuffer.Create(downloadUrlHandler, token);
}
