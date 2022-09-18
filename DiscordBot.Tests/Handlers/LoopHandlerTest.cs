namespace DiscordBot.Tests.Handlers;

using System;
using System.Threading.Tasks;
using DiscordBot.Controllers;
using Moq;
using MusicPlayer.Notifications;
using MusicPlayer.Tracks;
using PlayerHandlers;
using Proxies.Channels;
using Xunit;
using static Moq.It;

public class LoopHandlerTest
{
    private readonly LoopHandler _handler;
    private readonly Mock<IMusicController> _musicControllerMock = new();
    private readonly Mock<IMusicTextChannel> _textChannelMock = new();

    public LoopHandlerTest() => _handler = new LoopHandler(_musicControllerMock.Object);

    [Fact]
    public async Task ShouldHandleCorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new LoopNotification(true);

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendLoop(true), Times.Once);
    }
}
