namespace DiscordBot.MusicPlayer.Containers.Matroska.Extensions.MatroskaExtensions;

using Elements;
using Types;

internal static class MatroskaElementUtilsExtensions
{
    public static MatroskaElement? As(this MatroskaElement element, ElementType type) => element.Id == type.Id ? element : null;
}
