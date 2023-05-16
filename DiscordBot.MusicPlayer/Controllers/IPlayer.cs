using DiscordBot.MusicPlayer.Exceptions;
using DiscordBot.MusicPlayer.Tracks;
using DiscordBot.MusicPlayer.Tracks.Inmutable;

namespace DiscordBot.MusicPlayer.Controllers;

public interface IPlayer
{
    public bool AutoPlay { get; set; }

    public bool HasNextSong { get; }

    public bool IsPlaying { get; }

    public bool IsPaused { get; }

    public bool IsLooping { get; }

    /// <summary>
    ///     Defines if the player HasFinished playing the current song, by default is false.
    /// </summary>
    public bool HasFinished { get; }

    public Song? CurrentSong { get; }

    public TimeSpan CurrentTime { get; }

    public IEnumerable<ReadOnlySong> SongQueue { get; }

    /// <summary>
    ///     Pauses the currentSong
    /// </summary>
    /// <exception cref="NoSongException"></exception>
    public Task Pause();

    /// <summary>
    ///     Enables looping for tracks.
    /// </summary>
    /// <exception cref="NoSongException"></exception>
    public Task Loop();

    /// <summary>
    ///     Pauses the currentSong and clears queue
    /// </summary>
    /// <exception cref="NoSongException"></exception>
    public Task Stop();

    /// <summary>
    ///     Skips the currentSong, (After skipping the song you should call play again).
    /// </summary>
    /// <exception cref="NoSongException"></exception>
    public Task Skip();

    /// <summary>
    ///     Seek to a specific time in the current song.
    /// </summary>
    /// <param name="timeStamp">time in milliseconds</param>
    /// <returns></returns>
    public Task Seek(long timeStamp);

    /// <summary>
    ///     Adds songs for the specified query
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of songs added</returns>
    public Task Add(string query);

    /// <summary>
    ///     Starts playing the next song in the queue or all songs if autoPlay is enabled.
    ///     Task will complete once the song starts playing and state is changed.
    /// </summary>
    /// <exception cref="AlreadyPlayingException"></exception>
    /// <exception cref="NoSongException"></exception>
    /// <exception cref="MusicPlayerException"></exception>
    public Task Play();

    public Task Resume();

    /// <summary>
    ///     Set's the stream where data will be written to
    /// </summary>
    /// <param name="stream">Writable stream</param>
    public Task SetStream(Stream stream);
}