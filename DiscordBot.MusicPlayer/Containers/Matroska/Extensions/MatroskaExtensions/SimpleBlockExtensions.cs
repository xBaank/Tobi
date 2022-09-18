namespace DiscordBot.MusicPlayer.Containers.Matroska.Extensions.MatroskaExtensions;

using EBML;
using Elements;
using Types;

//TODO support lacing
internal static class SimpleBlockExtensions
{
    public static IEnumerable<ReadOnlyMemory<byte>> GetFrames(this SimpleBlock block) => block.LacingType switch
    {
        LacingType.NoLacing => new[] { block.FramesData },
        LacingType.Xiph => GetXiphFrames(block),
        LacingType.FixedSize => GetFixedSizeFrames(block),
        LacingType.Ebml => GetEbmlFrames(block),
        _ => throw new ArgumentOutOfRangeException(nameof(block), block.LacingType, null),
    };


    private static IEnumerable<ReadOnlyMemory<byte>> GetXiphFrames(this SimpleBlock block)
    {
        var head = block.FramesData[4..].Span;
        var numberOfFrames = head[0];
        var vint = EbmlUtils.GetVint(true, head[1..]);
        var size = vint.ToLong();
        var sizeOfVint = vint.Length;

        var framesData = head[(sizeOfVint + 1)..];

        throw new NotImplementedException();
    }

    private static IEnumerable<ReadOnlyMemory<byte>> GetFixedSizeFrames(this SimpleBlock block)
    {
        var head = block.FramesData[4..].Span;
        var numberOfFrames = head[0];
        //We don't need to read the size of the frames because they are all the same size
        var sizeOfVint = EbmlUtils.GetVint(true, head[1..]).Length;

        var framesData = head[(sizeOfVint + 1)..];

        throw new NotImplementedException();
    }

    private static IEnumerable<ReadOnlyMemory<byte>> GetEbmlFrames(this SimpleBlock block)
    {
        var head = block.FramesData[4..].Span;
        var numberOfFrames = head[0];
        var vint = EbmlUtils.GetVint(true, head[1..]);
        var size = vint.ToLong();
        var sizeOfVint = vint.Length;

        var framesData = head[(sizeOfVint + 1)..];

        throw new NotImplementedException();
    }
}
