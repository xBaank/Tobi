namespace DiscordBot.Tests.Handlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using DiscordBot.Controllers;
using Moq;
using MusicPlayer.Notifications;
using MusicPlayer.Tracks;
using PlayerHandlers;
using Proxies.Channels;
using Xunit;

public class PlayHandlerTest
{
    private readonly Mock<IMusicController> _musicControllerMock = new();
    private readonly PlayHandler _playHandler;
    private readonly Mock<IMusicTextChannel> _textChannelMock = new();

    public PlayHandlerTest() => _playHandler = new PlayHandler(_musicControllerMock.Object);

    [Fact]
    public async Task ShouldHandleCorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new PlayNotification(new Song(null!));

        await _playHandler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendSongPlaying(It.IsAny<Song>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ShouldHandleIncorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new PlayNotification(new Exception());

        await _playHandler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendInfo(It.IsAny<string>()));
    }
}
