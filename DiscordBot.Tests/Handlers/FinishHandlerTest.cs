namespace DiscordBot.Tests.Handlers;

using System;
using System.Threading.Tasks;
using DiscordBot.Controllers;
using Moq;
using MusicPlayer.Controllers;
using MusicPlayer.Notifications;
using MusicPlayer.Tracks;
using PlayerHandlers;
using Proxies.Channels;
using Xunit;
using static Moq.It;

public class FinishHandlerTest
{
    private readonly FinishHandler _handler;
    private readonly Mock<IMusicController> _musicControllerMock = new();
    private readonly Mock<IPlayer> _playerMock = new();
    private readonly Mock<IMusicTextChannel> _textChannelMock = new();

    public FinishHandlerTest() => _handler = new FinishHandler(_musicControllerMock.Object, _playerMock.Object);

    [Fact]
    public async Task ShouldHandleCorrect()
    {
        _playerMock.SetupGet(i => i.HasNextSong).Returns(false);
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new FinishNotification(new Song(null!));

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendInfo(IsAny<string>()));
    }

    [Fact]
    public async Task ShouldHandleIncorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new FinishNotification(new Exception());

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendInfo(IsAny<string>()));
    }
}
