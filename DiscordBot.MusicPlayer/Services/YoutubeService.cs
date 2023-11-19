using DiscordBot.MusicPlayer.DownloadHandlers;
using DiscordBot.MusicPlayer.Tracks.Inmutable;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace DiscordBot.MusicPlayer.Services;

using static String;

public class YoutubeService : IMusicService
{
    private readonly YoutubeClient _ytClient;

    public YoutubeService(YoutubeClient ytClient) => _ytClient = ytClient;

    public async IAsyncEnumerable<ReadOnlySong> GetSongsByQuery(string query)
    {
        if (IsPlaylist(query))
        {
            IAsyncEnumerable<IVideo> videos = _ytClient.Playlists.GetVideosAsync(query);
            await foreach (var video in videos)
            {
                yield return new ReadOnlySong(video.Title, video.Author.ChannelTitle, video.Url,
                    video.Thumbnails.FirstOrDefault()?.Url ?? Empty,
                    video.Duration ?? TimeSpan.Zero, new YtDownloadUrlHandler(_ytClient, video.Id));
            }

            yield break;
        }

        if (IsVideo(query))
        {
            //return song
            var video = await _ytClient.Videos.GetAsync(query);

            yield return new ReadOnlySong(video.Title, video.Author.ChannelTitle, video.Url,
                video.Thumbnails.TryGetWithHighestResolution()?.Url ?? Empty,
                video.Duration ?? TimeSpan.Zero, new YtDownloadUrlHandler(_ytClient, video.Id));

            yield break;
        }

        // ReSharper disable once InvertIf
        if (!IsPlaylist(query))
        {
            var video = await _ytClient.Search.GetVideosAsync(query).FirstOrDefaultAsync();

            if (video is null)
                yield break;

            yield return new ReadOnlySong(video.Title, video.Author.ChannelTitle, video.Url,
                video.Thumbnails.TryGetWithHighestResolution()?.Url ?? Empty,
                video.Duration ?? TimeSpan.Zero, new YtDownloadUrlHandler(_ytClient, video.Id));
        }
    }


    public async Task<MediaInfo?> TryGetMediaInfo(string query)
    {
        MediaInfo mediaInfo;
        if (IsPlaylist(query))
        {
            var playlist = await _ytClient.Playlists.GetAsync(query);
            mediaInfo = new MediaInfo(playlist.Title, playlist.Author?.ChannelTitle ?? Empty, playlist.Url);
            return mediaInfo;
        }

        if (IsVideo(query))
        {
            //return song
            var video = await _ytClient.Videos.GetAsync(query);

            mediaInfo = new MediaInfo(video.Title, video.Author.ChannelTitle, video.Url);

            return mediaInfo;
        }

        // ReSharper disable once InvertIf
        if (!IsPlaylist(query))
        {
            var video = await _ytClient.Search.GetVideosAsync(query).FirstOrDefaultAsync();

            if (video is null)
                return null;

            mediaInfo = new MediaInfo(video.Title, video.Author.ChannelTitle, video.Url);

            return mediaInfo;
        }

        return null;
    }


    private static bool IsPlaylist(string url) => PlaylistId.TryParse(url) is not null && url.Contains("youtube.com");

    private static bool IsVideo(string idOrUrl) => VideoId.TryParse(idOrUrl) is not null;
}