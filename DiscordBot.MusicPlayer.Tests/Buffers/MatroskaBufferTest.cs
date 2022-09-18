namespace DiscordBot.MusicPlayer.Tests.Buffers;

using MusicPlayer.Buffers;

public class MatroskaBufferTest : PlayBufferTest
{
    public MatroskaBufferTest() : base(MatroskaPlayerBuffer.Create)
    {
    }
}
