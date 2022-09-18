namespace DiscordBot.Utils;

using System;
using System.Threading.Tasks;

public static class Utils
{
    public static async ValueTask IfNotNull<TSource>(this TSource? input, Func<TSource, Task> pipe)
    {
        if (input is not null)
            await pipe(input);
    }
}
