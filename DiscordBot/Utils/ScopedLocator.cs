using System.Collections.Concurrent;
using System.Collections.Generic;
using DiscordBot.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Utils;

public static class ScopedLocator
{
    private static readonly ConcurrentDictionary<ulong,IServiceScope> ScopedProviders = new();
    
    public static IServiceScope AddScopedProvider(ulong key, IServiceScope scopedProvider) => ScopedProviders.GetOrAdd(key, scopedProvider);
    public static IServiceScope? TryGetScopedProvider(ulong key) => ScopedProviders.TryGetValue(key, out var musicController) ? musicController : null;
}