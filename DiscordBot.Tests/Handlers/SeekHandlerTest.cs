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

public class SeekHandlerTest
{
    private readonly SeekHandler _handler;
    private readonly Mock<IMusicController> _musicControllerMock = new();
    private readonly Mock<IMusicTextChannel> _textChannelMock = new();

    public SeekHandlerTest() => _handler = new SeekHandler(_musicControllerMock.Object);

    [Fact]
    public async Task ShouldHandleCorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new SeekNotification(new Song(null!), 11);

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendSeek(IsAny<ReadOnlySong>(), 11));
    }

    [Fact]
    public async Task ShouldHandleIncorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new SeekNotification(new Exception(), 0);

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendInfo(IsAny<string>()));
    }
}
