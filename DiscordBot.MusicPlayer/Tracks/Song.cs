namespace DiscordBot.MusicPlayer.Tracks;

using Inmutable;

/// <summary>
///     Represents a song with mutable state.
/// </summary>
public class Song
{
    public Song(ReadOnlySong song) => ReadOnlySong = song;

    public ReadOnlySong ReadOnlySong { get; }

    public TimeSpan CurrentTime { get; internal set; }

    public TimeSpan? TotalTime { get; internal set; }

    public PlayState State { get; internal set; } = PlayState.Ready;
}
