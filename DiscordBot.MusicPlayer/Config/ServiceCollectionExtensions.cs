namespace DiscordBot.MusicPlayer.Config;

using Controllers;
using Factories;
using Microsoft.Extensions.DependencyInjection;
using Services;
using YoutubeExplode;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayer(this IServiceCollection serviceCollection, Action<PlayerConfig> action)
    {
        var playerConfig = new PlayerConfig();
        action(playerConfig);

        serviceCollection
            .AddSingleton(_ =>
            {
                if(playerConfig.Sapisid is not null && playerConfig.Psid is not null)
                    return new YoutubeClient(new CookiesSettings(playerConfig.Sapisid, playerConfig.Psid));

                return new YoutubeClient();
            })
            .AddSingleton<IMusicService, YoutubeService>();


        serviceCollection.Add(new ServiceDescriptor(typeof(PlayerBufferFactory), _ => playerConfig.PlayerBufferFactory, ServiceLifetime.Singleton));
        serviceCollection.Add(new ServiceDescriptor(typeof(IPlayer), typeof(Player), playerConfig.Lifetime));

        return serviceCollection;
    }
}
