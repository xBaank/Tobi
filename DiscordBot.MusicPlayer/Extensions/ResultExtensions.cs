namespace DiscordBot.MusicPlayer.Extensions;

using Tracks;

public static class ResultExtensions
{
    public static async ValueTask<Result> OnSuccessAsync(this Result result, Func<Song, ValueTask> action)
    {
        if (result.Value is not null)
        {
            await action(result.Value);
        }
        return result;
    }

    public static async ValueTask<Result> OnSuccessAsync(this ValueTask<Result> task, Func<Song, ValueTask> action)
    {
        var result = await task;
        if (result.Value is not null)
        {
            await action(result.Value);
        }
        return result;
    }

    public static async ValueTask<Result> OnFailureAsync(this Result result, Func<Exception, ValueTask> action)
    {
        if (result.Exception is not null)
        {
            await action(result.Exception);
        }
        return result;
    }

    public static async ValueTask<Result> OnFailureAsync(this ValueTask<Result> task, Func<Exception, ValueTask> action)
    {
        var result = await task;
        if (result.Exception is not null)
        {
            await action(result.Exception);
        }
        return result;
    }

    public static Result OnSuccess(this Result result, Action<Song> action)
    {
        if (result.Value is not null)
            action(result.Value);

        return result;
    }

    public static Result OnFailure(this Result result, Action<Exception> action)
    {
        if (result.Exception is not null)
            action(result.Exception);

        return result;
    }
}
