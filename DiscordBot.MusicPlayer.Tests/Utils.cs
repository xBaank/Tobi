using DiscordBot.MusicPlayer.Tracks.Inmutable;

namespace DiscordBot.MusicPlayer.Tests;

using System;
using System.Threading.Tasks;
using DownloadHandlers;
using Moq;
using Notifications;
using Tracks;
using YoutubeExplode;

public static class Utils
{
    public static async Task<IDownloadUrlHandler> GetDownloadHandler(string url)
    {
        var client = new YoutubeClient();
        var video = await client.Videos.GetAsync(url);
        return new YtDownloadUrlHandler(client, video.Id);
    }

    public static async Task NotThrow<T>(this Task task) where T : Exception
    {
        try
        {
            await task;
        }
        catch (T)
        {
            // ignored
        }
    }

    public static TNotification IsFaultedNotification<TNotification, TException>() where TException : Exception where TNotification : IResultNotification<Result> =>
        It.Is<TNotification>(notification => notification.Result.Exception != null && notification.Result.Exception.GetType() == typeof(TException));

    public static TNotification IsNotification<TNotification>() where TNotification : IResultNotification<Result> =>
        It.Is<TNotification>(notification => notification.Result.Value != null);

    public static LoopNotification IsNotification(bool match) =>
        It.Is<LoopNotification>(notification => notification.IsLooping == match);

    public static TNotification IsFaultedNotification<TNotification>() where TNotification : IResultNotification<MediaInfo?> =>
        It.Is<TNotification>(notification => notification.Result == null);

    public static TNotification IsMediaNotification<TNotification>() where TNotification : IResultNotification<MediaInfo?> =>
        It.Is<TNotification>(notification => notification.Result != null);
}
