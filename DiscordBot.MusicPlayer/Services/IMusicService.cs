namespace DiscordBot.MusicPlayer.Services;

using Tracks;
using Tracks.Inmutable;

public interface IMusicService
{
    public IAsyncEnumerable<ReadOnlySong> GetSongsByQuery(string query);
    public Task<MediaInfo?> TryGetMediaInfo(string query);
}
