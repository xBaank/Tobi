namespace DiscordBot.MusicPlayer.Tests.Controllers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DownloadHandlers;
using Exceptions;
using FluentAssertions;
using MediatR;
using Moq;
using MusicPlayer.Buffers;
using MusicPlayer.Controllers;
using MusicPlayer.Services;
using Notifications;
using Tracks;
using Tracks.Inmutable;
using Xunit;
using static Moq.It;
using static Utils;

public class PlayerTest
{
    private const string VideoId = "r1BRpYgRNdw";
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IMusicService> _musicService = new();
    private readonly Player _player;
    private readonly Mock<IPlayerBuffer> _playerBuffer = new();

    private readonly Mock<Stream> _stream = new();

    public PlayerTest()
    {
        _player = new Player(_musicService.Object, _mediator.Object, (_, _) => ValueTask.FromResult(_playerBuffer.Object));
    }


    [Fact]
    public async Task ShouldAdd()
    {
        var song = GetSongsTest().First();
        var mediaInfo = new MediaInfo(song.Title, song.Author, song.Url);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        _musicService.Setup(i => i.TryGetMediaInfo(VideoId)).ReturnsAsync(mediaInfo);

        await _player.Add(VideoId);

        _player.IsPlaying.Should().BeFalse();
        _player.IsPaused.Should().BeFalse();
        _player.HasFinished.Should().BeTrue();
        _player.IsLooping.Should().BeFalse();
        _player.HasNextSong.Should().BeTrue();
        _player.SongQueue.Should().HaveCount(1);
        _musicService.Verify(i => i.GetSongsByQuery(VideoId), Times.Once());
        _mediator.Verify(i => i.Publish(IsMediaNotification<AddNotification>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotAddWhenServiceReturnsEmpty()
    {
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(Enumerable.Empty<ReadOnlySong>().ToAsyncEnumerable);

        await _player.Add(VideoId);

        _player.IsPaused.Should().BeFalse();
        _player.HasFinished.Should().BeTrue();
        _player.IsLooping.Should().BeFalse();
        _player.HasNextSong.Should().BeFalse();
        _player.SongQueue.Should().HaveCount(0);
        _musicService.Verify(i => i.GetSongsByQuery(VideoId), Times.Once());
        _mediator.Verify(i => i.Publish(IsFaultedNotification<AddNotification>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotPlayWithNoSongs()
    {
        var task = _player.Play();
        await task;

        _player.IsPaused.Should().BeFalse();
        _player.HasFinished.Should().BeTrue();
        _player.IsLooping.Should().BeFalse();
        _player.HasNextSong.Should().BeFalse();
        _player.SongQueue.Should().HaveCount(0);
        _mediator.Verify(i => i.Publish(IsFaultedNotification<PlayNotification, NoSongException>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldPlay()
    {
        await _player.SetStream(_stream.Object);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        _playerBuffer.Setup(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>())).Returns(Task.Delay(3000, _player.Token));

        await _player.Add(VideoId);

        await _player.Play();

        _player.IsPlaying.Should().BeTrue();
        _player.HasFinished.Should().BeFalse();

        await _player.WaitFinishPlaying();

        _player.IsPlaying.Should().BeFalse();
        _player.HasFinished.Should().BeTrue();

        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _playerBuffer.Verify(i => i.DisposeAsync());
        _playerBuffer.Verify(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>()));
        _mediator.Verify(i => i.Publish(IsNotification<PlayNotification>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotStartPlayingWithoutStream()
    {
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        await _player.Add(VideoId);

        await _player.Play();

        _player.IsPlaying.Should().BeFalse();
        _player.HasFinished.Should().BeFalse();
        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _mediator.Verify(i => i.Publish(IsFaultedNotification<PlayNotification, MusicPlayerException>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldAutoPlay()
    {
        await _player.SetStream(_stream.Object);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);

        await _player.Add(VideoId);
        await _player.Add(VideoId);

        await _player.Play();

        await _player.WaitFinishPlaying();

        _player.SongQueue.Should().BeEmpty();
        _player.IsPlaying.Should().BeFalse();
        _player.IsLooping.Should().BeFalse();
        _player.AutoPlay.Should().BeTrue();
        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _playerBuffer.Verify(i => i.WriteFile(IsAny<Stream>(), IsAny<CancellationToken>()));
        _mediator.Verify(i => i.Publish(IsNotification<PlayNotification>(), IsAny<CancellationToken>()), Times.Exactly(2));
    }


    [Fact]
    public async Task ShouldStop()
    {
        await _player.SetStream(_stream.Object);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        _playerBuffer.Setup(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>())).Returns(Task.Delay(10000, _player.Token));

        await _player.Add(VideoId);
        _ = _player.Play();
        await _player.Stop();

        _player.IsPlaying.Should().BeFalse();
        _player.HasNextSong.Should().BeFalse();
        _player.HasFinished.Should().BeTrue();
        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _mediator.Verify(i => i.Publish(IsNotification<StopNotification>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotStop()
    {
        await _player.Stop();

        _player.IsPlaying.Should().BeFalse();
        _player.HasNextSong.Should().BeFalse();
        _player.HasFinished.Should().BeTrue();
        _mediator.Verify(i => i.Publish(IsFaultedNotification<StopNotification, NoSongException>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotPauseAlreadyPaused()
    {
        await _player.SetStream(_stream.Object);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        _playerBuffer.Setup(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>())).Returns(Task.Delay(10000, _player.Token));

        await _player.Add(VideoId);

        _ = _player.Play();

        await _player.Pause();
        await _player.Pause();

        _player.IsPaused.Should().BeTrue();
        _player.HasNextSong.Should().BeFalse();
        _player.HasFinished.Should().BeFalse();
        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _playerBuffer.Verify(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>()));
        _mediator.Verify(i => i.Publish(IsFaultedNotification<PauseNotification, AlreadyPausedException>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldSkip()
    {
        await _player.SetStream(_stream.Object);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        _playerBuffer.Setup(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>())).Returns(Task.Delay(10000, _player.Token));

        await _player.Add(VideoId);
        _ = _player.Play();
        await _player.Skip();

        _player.IsPlaying.Should().BeFalse();
        _player.IsPaused.Should().BeFalse();
        _player.HasNextSong.Should().BeFalse();
        _player.HasFinished.Should().BeTrue();
        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _playerBuffer.Verify(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>()));
        _mediator.Verify(i => i.Publish(IsNotification<SkipNotification>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldLoop()
    {
        _player.AutoPlay = false;
        await _player.SetStream(_stream.Object);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        _playerBuffer.Setup(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _player.Loop();
        await _player.Add(VideoId);
        await _player.Play();
        await _player.WaitFinishPlayingCurrent();

        _player.IsPlaying.Should().BeFalse();
        _player.IsPaused.Should().BeFalse();
        _player.HasNextSong.Should().BeTrue();
        _player.HasFinished.Should().BeTrue();
        _player.IsLooping.Should().BeTrue();
        _player.SongQueue.Should().HaveCount(1);
        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _playerBuffer.Verify(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>()));
        _mediator.Verify(i => i.Publish(IsNotification(true), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotSkipWhitNoSongs()
    {
        await _player.Skip();

        _player.IsPlaying.Should().BeFalse();
        _player.IsPaused.Should().BeFalse();
        _player.HasNextSong.Should().BeFalse();
        _player.HasFinished.Should().BeTrue();
        _mediator.Verify(i => i.Publish(IsFaultedNotification<SkipNotification, NoSongException>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldSeek()
    {
        await _player.SetStream(_stream.Object);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        _playerBuffer.Setup(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>())).Returns(Task.Delay(10000, _player.Token));
        _playerBuffer.Setup(i => i.Seek(10)).ReturnsAsync(true);
        _playerBuffer.SetupGet(i => i.TotalTime).Returns(TimeSpan.MaxValue);

        await _player.Add(VideoId);


        _ = _player.Play();

        await _player.Seek(10);

        _player.IsPlaying.Should().BeTrue();
        _player.IsPaused.Should().BeFalse();
        _player.HasNextSong.Should().BeFalse();
        _player.HasFinished.Should().BeFalse();
        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _playerBuffer.Verify(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>()));
        _playerBuffer.Verify(i => i.Seek(10));
        _mediator.Verify(i => i.Publish(IsNotification<SeekNotification>(), IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotSeekNegative()
    {
        await _player.SetStream(_stream.Object);
        _musicService.Setup(i => i.GetSongsByQuery(VideoId)).Returns(GetSongsTest().ToAsyncEnumerable);
        _playerBuffer.Setup(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>())).Returns(Task.Delay(10000, _player.Token));
        _playerBuffer.SetupGet(i => i.TotalTime).Returns(TimeSpan.MaxValue);

        await _player.Add(VideoId);

        _ = _player.Play();

        await _player.Seek(-10);

        _player.IsPlaying.Should().BeTrue();
        _player.IsPaused.Should().BeFalse();
        _player.HasNextSong.Should().BeFalse();
        _player.HasFinished.Should().BeFalse();
        _musicService.Verify(i => i.GetSongsByQuery(VideoId));
        _playerBuffer.Verify(i => i.WriteFile(_stream.Object, IsAny<CancellationToken>()));
        _playerBuffer.Verify(i => i.Seek(10), Times.Never);
        _mediator.Verify(i => i.Publish(IsFaultedNotification<SeekNotification, InvalidTimeException>(), IsAny<CancellationToken>()), Times.Once());
    }


    private IEnumerable<ReadOnlySong> GetSongsTest()
    {
        var downloadHandlerMock = new Mock<IDownloadUrlHandler>();
        yield return new ReadOnlySong("pepe", "pepe", "pepe", "pepe", TimeSpan.FromSeconds(5), downloadHandlerMock.Object);
    }
}
