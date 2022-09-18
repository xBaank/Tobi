namespace DiscordBot.Tests.Controllers;

using System;
using System.IO;
using System.Threading.Tasks;
using DiscordBot.Controllers;
using Exceptions;
using Moq;
using MusicPlayer.Controllers;
using Proxies;
using Proxies.Channels;
using Xunit;
using static Moq.It;

public class MusicControllerTest
{
    //Test music controller
    private readonly MusicController _musicController;
    private readonly Mock<IPlayer> _playerMock = new();
    private readonly Mock<IMusicTextChannel> _textChannelMock = new();
    private readonly Mock<IVoiceChannel> _voiceChannelMock = new();
    private readonly Mock<IVoiceConnection> _voiceConnectionMock = new();

    public MusicControllerTest() => _musicController = new MusicController(_playerMock.Object)
    {
        TextChannel = _textChannelMock.Object,
    };

    [Fact]
    public async Task ShouldConnectToVoiceChannelTest()
    {
        var stream = new Mock<Stream>();
        _voiceChannelMock.Setup(i => i.ConnectToVoiceChannel()).ReturnsAsync(_voiceConnectionMock.Object);
        _voiceConnectionMock.Setup(i => i.GetStream()).Returns(stream.Object);

        await _musicController.SetVoiceChannel(_voiceChannelMock.Object);

        _voiceConnectionMock.Verify(i => i.GetStream());
        _voiceChannelMock.Verify(i => i.ConnectToVoiceChannel());
    }

    [Fact]
    public async Task ShouldNotConnectToVoiceChannelTest()
    {
        Stream? stream = null;
        Exception? exception = null;

        _voiceChannelMock.Setup(i => i.ConnectToVoiceChannel()).ReturnsAsync(_voiceConnectionMock.Object);
        _voiceConnectionMock.Setup(i => i.GetStream()).Returns(stream!);

        try
        {
            await _musicController.SetVoiceChannel(_voiceChannelMock.Object);
        }
        catch (Exception e)
        {
            exception = e;
        }

        Assert.IsType<VoiceConnectionException>(exception);
        _voiceConnectionMock.Verify(i => i.GetStream());
        _voiceChannelMock.Verify(i => i.ConnectToVoiceChannel());
    }

    [Fact]
    public async Task ShouldPlay()
    {
        var stream = new Mock<Stream>();
        _voiceChannelMock.Setup(i => i.ConnectToVoiceChannel()).ReturnsAsync(_voiceConnectionMock.Object);
        _voiceConnectionMock.Setup(i => i.GetStream()).Returns(stream.Object);
        _voiceConnectionMock.SetupGet(i => i.IsConnected).Returns(true);

        await _musicController.SetVoiceChannel(_voiceChannelMock.Object);

        await _musicController.Play("pepe");

        _playerMock.Verify(i => i.Add("pepe"));
    }

    [Fact]
    public async Task ShouldNotPlayEmptyQuery()
    {
        _playerMock.SetupGet(i => i.HasNextSong).Returns(true);

        await _musicController.Play(string.Empty);

        _playerMock.Verify(i => i.Play(), Times.Never);
        _textChannelMock.Verify(i => i.SendInfo(IsAny<string>()));
    }

    [Fact]
    public async Task ShouldNotPlayEmptyQueue()
    {
        _playerMock.SetupGet(i => i.HasNextSong).Returns(false);
        _playerMock.SetupGet(i => i.HasFinished).Returns(true);

        await _musicController.Play("pepe");

        _playerMock.Verify(i => i.Play(), Times.Never);
    }

    [Fact]
    public async Task ShouldNotPlayAlreadyPlaying()
    {
        _playerMock.SetupGet(i => i.IsPlaying).Returns(true);

        await _musicController.Play("asd");

        _playerMock.Verify(i => i.Play(), Times.Never);
    }

    [Fact]
    public async Task ShouldPause()
    {
        await _musicController.Pause();

        _playerMock.Verify(i => i.Pause());
    }

    [Fact]
    public async Task ShouldLoop()
    {
        await _musicController.Loop();

        _playerMock.Verify(i => i.Loop());
    }

    [Fact]
    public async Task ShouldResume()
    {
        var stream = new Mock<Stream>();
        _voiceChannelMock.Setup(i => i.ConnectToVoiceChannel()).ReturnsAsync(_voiceConnectionMock.Object);
        _voiceConnectionMock.Setup(i => i.GetStream()).Returns(stream.Object);
        _voiceConnectionMock.Setup(i => i.IsConnected).Returns(true);
        await _musicController.SetVoiceChannel(_voiceChannelMock.Object);
        
        await _musicController.Resume();

        _playerMock.Verify(i => i.Resume());
    }

    [Fact]
    public async Task ShouldStop()
    {
        await _musicController.Stop();

        _playerMock.Verify(i => i.Stop());
    }

    [Fact]
    public async Task ShouldSkip()
    {
        await _musicController.Skip();

        _playerMock.Verify(i => i.Skip());
    }

    [Fact]
    public async Task ShouldSeek()
    {
        await _musicController.Seek("0:0:5");

        _playerMock.Verify(i => i.Seek(5000));
    }

    [Fact]
    public async Task ShouldNotSeekIncorrectFormat()
    {
        await _musicController.Seek("50asd");

        _playerMock.Verify(i => i.Seek(50), Times.Never);
        _textChannelMock.Verify(i => i.SendInfo(IsAny<string>()));
    }
}
