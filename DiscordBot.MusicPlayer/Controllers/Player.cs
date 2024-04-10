using DiscordBot.MusicPlayer.Buffers;
using DiscordBot.MusicPlayer.Exceptions;
using DiscordBot.MusicPlayer.Extensions;
using DiscordBot.MusicPlayer.Factories;
using DiscordBot.MusicPlayer.Notifications;
using DiscordBot.MusicPlayer.Services;
using DiscordBot.MusicPlayer.Tracks;
using DiscordBot.MusicPlayer.Tracks.Inmutable;
using MediatR;
using Nito.AsyncEx;

namespace DiscordBot.MusicPlayer.Controllers;

public class Player : IPlayer
{
    //important thing, whenever we are changing the state of the song, we need to lock the object to prevent multiple threads from changing the state at the same time.

    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IMediator _mediator;
    private readonly IMusicService _musicService;
    private readonly PlayerBufferFactory _playerBufferFactory;
    private readonly Queue<ReadOnlySong> _songs = new();
    private Task? _autoPlayTask;

    private IPlayerBuffer? _playerBuffer;

    private Task? _playingTask;

    private CancellationTokenSource _playingToken = new();
    private Stream? _stream;

    public Player(IMusicService musicService, IMediator mediator, PlayerBufferFactory playerBufferFactory)
    {
        _musicService = musicService;
        _mediator = mediator;
        _playerBufferFactory = playerBufferFactory;
    }

    public CancellationToken Token => _playingToken.Token;

    public bool AutoPlay { get; set; } = true;

    public bool HasNextSong => _songs.Any();

    public bool IsPlaying => CurrentSong is {State: PlayState.Playing};

    public bool IsPaused => CurrentSong is {State: PlayState.Paused};

    public bool IsLooping { get; private set; }

    public bool HasFinished => CurrentSong is {State: PlayState.Finished} or null;

    public Song? CurrentSong { get; private set; }

    public TimeSpan CurrentTime => _playerBuffer?.CurrentTime ?? CurrentSong?.CurrentTime ?? TimeSpan.Zero;

    public IEnumerable<ReadOnlySong> SongQueue => _songs;


    public async Task Pause()
    {
        using var lockAsync = await _lock.LockAsync();

        if (HasFinished)
        {
            await NotifyPause(new NoSongException("No song playing"));
            return;
        }

        if (CurrentSong is null)
        {
            await NotifyPause(new NoSongException("No song playing"));
            return;
        }

        if (IsPaused)
        {
            await NotifyPause(new AlreadyPausedException($"{CurrentSong!.ReadOnlySong.Title} is already paused"));
            return;
        }

        CurrentSong.CurrentTime = _playerBuffer?.CurrentTime ?? TimeSpan.Zero;
        CurrentSong.State = PlayState.Paused;
        await CancelAndWaitFinishPlaying();

        await NotifyPause(CurrentSong);
    }

    public async Task Loop()
    {
        IsLooping = !IsLooping;
        await NotifyLoop(IsLooping);
    }

    public async Task Stop()
    {
        using var lockAsync = await _lock.LockAsync();

        if (HasFinished)
        {
            await NotifyStop(new NoSongException("No song playing"));
            return;
        }

        if (CurrentSong is null)
        {
            await NotifyStop(new NoSongException("No song playing"));
            return;
        }

        CurrentSong.State = PlayState.Finished;
        IsLooping = false;
        await CancelAndWaitFinishPlaying();
        _songs.Clear();

        await NotifyStop(CurrentSong);
    }

    public async Task Skip()
    {
        using var lockAsync = await _lock.LockAsync();
        if (_playingToken.IsCancellationRequested) return;

        //Can't skip if there is no more songs
        if (HasFinished && !HasNextSong)
        {
            await NotifySkip(new NoSongException("No song playing"));
            return;
        }

        if (HasFinished)
        {
            await NotifySkip(new NoSongException("No song playing"));
            return;
        }

        if (CurrentSong is null)
        {
            await NotifySkip(new NoSongException("No song playing"));
            return;
        }

        if (IsPaused)
        {
            await NotifySkip(new AlreadyPausedException("Can't skip song if paused"));
            return;
        }

        CurrentSong.State = PlayState.Finished;
        await CancelAndWaitFinishPlaying();

        await NotifySkip(CurrentSong);
    }

    public async Task Add(string query)
    {
        using var lockAsync = await _lock.LockAsync();
        var songs = _musicService.GetSongsByQuery(query);
        var songLoadTask = _songs.EnqueueRange(songs);

        var mediaInfoTask = _musicService.TryGetMediaInfo(query);

        var notifyTaskAdd = NotifyAdd(mediaInfoTask);

        await Task.WhenAll(songLoadTask, notifyTaskAdd);
    }

    public async Task Play() => await Play(NotifyPlay);

    public async Task Resume()
    {
        if (!IsPaused)
        {
            await NotifyResume(new AlreadyPlayingException("Song is not paused"));
            return;
        }

        await Play(NotifyResume);
    }

    public async Task SetStream(Stream stream)
    {
        if (_stream is not null)
            await _stream.DisposeAsync();

        _stream = stream;
    }

    public async Task Seek(long timeStamp)
    {
        using var lockAsync = await _lock.LockAsync();
        if (HasFinished || CurrentSong is null)
        {
            await NotifySeek(new NoSongException("No song playing"));
            return;
        }

        if (timeStamp < 0)
        {
            await NotifySeek(new InvalidTimeException("Timestamp can't be less than 0"));
            return;
        }

        if (CurrentSong.TotalTime is null)
        {
            await NotifySeek(new InvalidTimeException("Song not loaded fully yet"));
            return;
        }

        if (CurrentSong.TotalTime?.TotalMilliseconds < timeStamp)
        {
            await NotifySeek(new InvalidTimeException($"{TimeSpan.FromMilliseconds(timeStamp):c} is greater than the duration of {CurrentSong.ReadOnlySong.Title}"));
            return;
        }

        if (_playerBuffer is null)
        {
            CurrentSong.CurrentTime = TimeSpan.FromMilliseconds(timeStamp);
            await NotifySeek(CurrentSong, timeStamp);
            return;
        }

        if (await _playerBuffer.Seek(timeStamp))
        {
            await NotifySeek(CurrentSong, timeStamp);
            return;
        }

        await NotifySeek(new InvalidTimeException($"Can't seek to{TimeSpan.FromMilliseconds(timeStamp):c}"));
    }

    public Task WaitFinishPlaying() => _autoPlayTask ?? _playingTask ?? Task.CompletedTask;

    public Task WaitFinishPlayingCurrent() => _playingTask ?? Task.CompletedTask;

    private async Task Play(Func<Result, Task> notifyDelegate)
    {
        using (await _lock.LockAsync())
        {
            //First check all conditions to play or resume are met
            var error = CheckForSong();

            if (error is not null)
            {
                await notifyDelegate(error);
                return;
            }

            if (_stream is null)
            {
                await notifyDelegate(new MusicPlayerException("Underlying stream is null"));
                return;
            }

            if (HasFinished && !HasNextSong)
            {
                await notifyDelegate(new NoSongException("No song playing"));
                return;
            }

            if (CurrentSong?.State == PlayState.Playing)
            {
                await notifyDelegate(new AlreadyPlayingException($"{CurrentSong.ReadOnlySong.Title} is already playing"));
                return;
            }

            if (CurrentSong != null)
            {
                CurrentSong.State = PlayState.Playing;
                await notifyDelegate(CurrentSong);
            }

            _playingTask = Task.Run(StartPlaying, CancellationToken.None);

            if ((_autoPlayTask is null || _autoPlayTask.IsCompleted) && AutoPlay)
                _autoPlayTask = AutoPlayTask();
        }
    }

    private async Task AutoPlayTask()
    {
        while (AutoPlay)
        {
            //Check before and after
            await WaitFinishPlayingCurrent();

            if (HasFinished && !HasNextSong)
                return;

            //Exit if song was paused
            if (IsPaused)
                return;

            await Play(NotifyPlay);

            await WaitFinishPlayingCurrent();

            //Exit if no more songs to play
            if (HasFinished && !HasNextSong)
                return;

            //Exit if song was paused
            if (IsPaused)
                return;
        }
    }

    private void CheckLoop()
    {
        if (CurrentSong is null) return;

        if (IsLooping)
            _songs.Enqueue(CurrentSong.ReadOnlySong);
    }

    private Exception? CheckForSong()
    {
        if (IsPaused)
            return null;

        Exception? error = null;
        var nextSong = GetNextSong();

        nextSong
            .OnSuccess(song => CurrentSong = song)
            .OnFailure(ex => error = ex);

        return error;
    }

    private Result GetNextSong()
    {
        _songs.TryDequeue(out var currentSong);

        if (currentSong is null) return new NoSongException("No more songs on the queue");

        return new Song(currentSong);
    }

    private async Task StartPlaying()
    {
        if (CurrentSong is null)
            return;

        if (_stream is null)
            return;

        try
        {
            await using (_stream)
            await using (_playerBuffer = await _playerBufferFactory(CurrentSong.ReadOnlySong.DownloadUrlHandler, _playingToken.Token))
            {
                CurrentSong.TotalTime = _playerBuffer.TotalTime;
                await _playerBuffer.Seek((long) CurrentSong.CurrentTime.TotalMilliseconds);
                await _playerBuffer.WriteFile(_stream, _playingToken.Token);
            }
        }
        catch (OperationCanceledException)
        {
            //Three cases, pause, stop or skip
            if (CurrentSong.State == PlayState.Paused)
                return;
        }
        catch (Exception)
        {
            //If there is an exception the song will be excluded from the queue if looping is enabled
            CurrentSong.State = PlayState.Finished;
            await NotifyFinish(new MusicPlayerException("Error while playing song"));
            return;
        }
        finally
        {
            _playingToken = new CancellationTokenSource();
            _playerBuffer = null;
        }

        //We only reach this point if the song has finished playing
        CurrentSong.State = PlayState.Finished;
        CheckLoop();
        await NotifyFinish(CurrentSong);
    }

    private Task CancelAndWaitFinishPlaying()
    {
        _playingToken.Cancel();
        return _playingTask ?? Task.CompletedTask;
    }

    private Task NotifyPlay(Result result) => _mediator.Publish(new PlayNotification(result));

    private Task NotifyLoop(bool isLooping) => _mediator.Publish(new LoopNotification(isLooping));

    private Task NotifyResume(Result result) => _mediator.Publish(new ResumeNotification(result));

    private Task NotifyFinish(Result result) => _mediator.Publish(new FinishNotification(result));

    private Task NotifyPause(Result result) => _mediator.Publish(new PauseNotification(result));

    private Task NotifyStop(Result result) => _mediator.Publish(new StopNotification(result));

    private Task NotifySeek(Result result, long timeStamp = default) => _mediator.Publish(new SeekNotification(result, timeStamp));

    private Task NotifySkip(Result result) => _mediator.Publish(new SkipNotification(result));

    private async Task NotifyAdd(Task<MediaInfo?> mediaInfo) => await _mediator.Publish(new AddNotification(await mediaInfo));
}