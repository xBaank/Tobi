﻿using DiscordBot.MusicPlayer.Tracks.Inmutable;

namespace DiscordBot.Tests.Handlers;

using System.Threading.Tasks;
using DiscordBot.Controllers;
using Moq;
using MusicPlayer.Notifications;
using PlayerHandlers;
using Proxies.Channels;
using Xunit;
using static Moq.It;

public class AddHandlerTest
{
    private readonly AddHandler _handler;
    private readonly Mock<IMusicController> _musicControllerMock = new();
    private readonly Mock<IMusicTextChannel> _textChannelMock = new();

    public AddHandlerTest() => _handler = new AddHandler(_musicControllerMock.Object);

    [Fact]
    public async Task ShouldHandleCorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new AddNotification(new MediaInfo());

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendAddedMediaInfo(IsAny<MediaInfo>()));
    }

    [Fact]
    public async Task ShouldHandleIncorrect()
    {
        _musicControllerMock.SetupGet(i => i.TextChannel).Returns(_textChannelMock.Object);
        var notification = new AddNotification(null);

        await _handler.Handle(notification, default);

        _textChannelMock.Verify(i => i.SendInfo(IsAny<string>()));
    }
}
