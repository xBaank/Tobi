namespace DiscordBot.MusicPlayer.Tests.Buffers;

using System.Threading;
using System.Threading.Tasks;
using DownloadHandlers;
using MusicPlayer.Buffers;

public class FFmpegBufferTest : PlayBufferTest
{
    public FFmpegBufferTest() : base(LoadAndCreate)
    {
    }

    private static ValueTask<IPlayerBuffer> LoadAndCreate(IDownloadUrlHandler downloadUrlHandler, CancellationToken token)
    {
        Utils.SetUpFFmpeg();
        return FFmpegPlayerBuffer.Create(downloadUrlHandler, token);
    }
}
