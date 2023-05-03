namespace DiscordBot.MusicPlayer.Tests.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MusicPlayer.Services;
using Xunit;
using YoutubeExplode;
using static System.Environment;
public class YoutubeServiceTest
{
    private readonly IMusicService _musicService;

    public YoutubeServiceTest() {
        var sapisid = GetEnvironmentVariable("Sapisid") ?? throw new Exception("Sapisid not found");
        var psid = GetEnvironmentVariable("Psid") ?? throw new Exception("Psid not found");
        var cookiesettings = new CookiesSettings(sapisid, psid);
        _musicService = new YoutubeService(new YoutubeClient(cookiesettings));
    }

    [Fact]
    public async Task GetSongsByQueryReturnsSong()
    {
        var songs = await _musicService.GetSongsByQuery("test").ToListAsync();
        songs.Should().NotBeEmpty();
        songs.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetSongsByIdQueryReturnsSong()
    {
        var songs = await _musicService.GetSongsByQuery("https://www.youtube.com/watch?v=FAucVNRx_mU").ToListAsync();
        songs.Should().NotBeEmpty();
        songs.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetSongsByQueryReturnsSongs()
    {
        var songs = await _musicService.GetSongsByQuery("https://www.youtube.com/watch?v=FAucVNRx_mU&list=PLBATKC8sZfhHzuaJVuSw_mK04Nqn9Dz2i").ToListAsync();

        songs.Should().NotBeEmpty();
        songs.Count.Should().BeGreaterThan(1);
        songs.First().Title.Should().Be("XXXTENTACION - Jocelyn Flores (Audio)");
        songs.First().Author.Should().Be("XXXTENTACION");
        songs.First().Url.Should().Be("https://www.youtube.com/watch?v=FAucVNRx_mU&list=PLBATKC8sZfhHzuaJVuSw_mK04Nqn9Dz2i");
        var url = await songs.First().DownloadUrlHandler.GetUrl();
        url.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetSongsByQueryReturnsMediaInfo()
    {
        var song = await _musicService.TryGetMediaInfo("https://www.youtube.com/watch?v=4M6Mfjz6noc");
        song.Should().NotBeNull();
        song!.Value.Author.Should().Be("𝘙𝘪𝘰𝘮𝘢");
        song.Value.Title.Should().Be("AGGRESSIVE PHONK PLAYLIST #1");
        song.Value.Url.Should().Be("https://www.youtube.com/watch?v=4M6Mfjz6noc");
    }
}
