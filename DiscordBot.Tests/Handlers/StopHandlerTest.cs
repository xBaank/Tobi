namespace DiscordBot.Tests.Handlers;

using System;
using System.Threading.Tasks;
using DiscordBot.Controllers;
using Moq;
using MusicPlayer.Notifications;
using MusicPlayer.Tracks;
using MusicPlayer.Tracks.Inmutable;
using PlayerHandlers;
using Proxies.Channels;
using Xunit;
using static Moq.It;

public class StopHandlerTest
{
    private readonly StopHandler _handler;
    private readonly Mock<IMusicController> _musicControllerMock = new();
    private readonly Mock<IMusicTextChannel> _textChannelMock = new();

    public StopHandlerTest() => _handler = new StopHandler(_musicControllerMock.Object);

    [Fact]
    public async Task ShouldHandleCorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new StopNotification(new Song(null!));

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendStop(IsAny<ReadOnlySong>()));
    }

    [Fact]
    public async Task ShouldHandleIncorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new StopNotification(new Exception());

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendInfo(IsAny<string>()));
    }
}
