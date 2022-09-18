namespace DiscordBot.MusicPlayer.Extensions;

public static class QueueExtensions
{
    public static async Task EnqueueRange<T>(this Queue<T> queue, IAsyncEnumerable<T> items)
    {
        await foreach (var item in items)
        {
            queue.Enqueue(item);
        }
    }
}
