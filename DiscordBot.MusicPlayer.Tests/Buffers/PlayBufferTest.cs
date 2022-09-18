namespace DiscordBot.MusicPlayer.Tests.Buffers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DownloadHandlers;
using Factories;
using FluentAssertions;
using Moq;
using Xunit;
using static Moq.It;

public abstract class PlayBufferTest
{
    private readonly PlayerBufferFactory _factory;
    private readonly Mock<Stream> _streamMock = new();

    protected PlayBufferTest(PlayerBufferFactory factory) => _factory = factory;

    [Theory]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=rTvfnTkiYGo")]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=hP6VM6YAMIE")]
    public async Task WriteFileTest(Task<IDownloadUrlHandler> downloadUrlHandler)
    {
        var buffer = await _factory(await downloadUrlHandler);

        await buffer.WriteFile(_streamMock.Object);

        _streamMock.Verify(i => i.WriteAsync(IsAny<ReadOnlyMemory<byte>>(), IsAny<CancellationToken>()), Times.AtLeastOnce);
        buffer.CurrentTime.Should().BeCloseTo(buffer.TotalTime, TimeSpan.FromMilliseconds(100));
    }

    [Theory]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=rTvfnTkiYGo")]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=hP6VM6YAMIE")]
    public async Task ShouldSeek(Task<IDownloadUrlHandler> downloadUrlHandler)
    {
        var buffer = await _factory(await downloadUrlHandler);

        var result = await buffer.Seek(100);

        result.Should().BeTrue();

        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=rTvfnTkiYGo")]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=hP6VM6YAMIE")]
    public async Task ShouldNotSeek(Task<IDownloadUrlHandler> downloadUrlHandler)
    {
        var buffer = await _factory(await downloadUrlHandler);

        var result = await buffer.Seek(long.MaxValue);

        result.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=rTvfnTkiYGo")]
    [MemberData(nameof(GetDownloadHandler), "https://www.youtube.com/watch?v=hP6VM6YAMIE")]
    public async Task ShouldNotSeekNegative(Task<IDownloadUrlHandler> downloadUrlHandler)
    {
        var buffer = await _factory(await downloadUrlHandler);

        var result = await buffer.Seek(long.MinValue);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("https://www.google.com/")]
    public async Task ShouldNotLoadFileTest(string url)
    {
        var downloadUrlHandlerMock = new Mock<IDownloadUrlHandler>();
        downloadUrlHandlerMock.Setup(i => i.GetUrl()).Returns(Task.FromResult(url));
        var task = _factory(downloadUrlHandlerMock.Object).AsTask();

        await task.NotThrow<Exception>();

        task.Exception.Should().NotBeNull();
        task.Exception!.InnerException.Should().BeOfType<Exception>();
    }

    public static IEnumerable<object[]> GetDownloadHandler(string url)
    {
        yield return new object[] { Utils.GetDownloadHandler(url) };
    }
}
