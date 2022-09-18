namespace DiscordBot.MusicPlayer.Tracks;

/// <summary>
///     Represents metadata of a song, playlist, streaming, etc.
/// </summary>
public readonly record struct MediaInfo(string Title, string Author, string Url);
