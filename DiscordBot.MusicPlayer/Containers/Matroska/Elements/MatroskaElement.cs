namespace DiscordBot.MusicPlayer.Containers.Matroska.Elements;

using EBML;

internal readonly record struct MatroskaElement(VInt Id, long Size, long Position);
