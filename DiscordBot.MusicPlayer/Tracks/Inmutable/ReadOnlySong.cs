namespace DiscordBot.MusicPlayer.Tracks.Inmutable;

using DownloadHandlers;

/// <summary>
///     Represents a song with immutable state.
/// </summary>
/// <param name="Title">Song title</param>
/// <param name="Author">Author name</param>
/// <param name="Url">Url to listen</param>
/// <param name="ThumbnailUrl">Thumbnail of the song</param>
/// <param name="Duration">Total duration of the song</param>
public record ReadOnlySong(string Title, string Author, string Url, string? ThumbnailUrl,
    TimeSpan Duration, IDownloadUrlHandler DownloadUrlHandler);
