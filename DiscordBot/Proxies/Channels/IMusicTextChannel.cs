namespace DiscordBot.Proxies.Channels;

using System;
using System.Threading;
using System.Threading.Tasks;
using MusicPlayer.Tracks;
using MusicPlayer.Tracks.Inmutable;

public interface IMusicTextChannel : ITextChannel
{
    public Task SendEmptyQueue() => SendInfo("No more songs in the queue");
    public Task SendEmptyQuery() => SendInfo(":no_entry_sign: Url or query not specified");
    public Task SendPause(ReadOnlySong song) => SendInfo($"Song {song.Title} has been paused");
    public Task SendResume(ReadOnlySong song) => SendInfo($"Song {song.Title} has been resumed");
    public Task SendSkip(ReadOnlySong song) => SendInfo($"Song {song.Title} has been skipped");
    public Task SendStop(ReadOnlySong song) => SendInfo($"Song {song.Title} has been stopped");
    public Task SendSeek(ReadOnlySong song, long timeStamp) => SendInfo($"Seeked {song.Title} to {TimeSpan.FromMilliseconds(timeStamp):c}");
    public Task SendLoop(bool state) => SendInfo($"Loop turned {(state ? "on" : "off")}");
    public Task SendAddedMediaInfo(MediaInfo mediaInfo) => SendInfo($":musical_note: Added {mediaInfo.Title} by {mediaInfo.Author}");
    public Task SendSongPlaying(Song media, string mention, CancellationToken cancellationToken = default);
}
