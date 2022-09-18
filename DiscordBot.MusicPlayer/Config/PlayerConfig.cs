namespace DiscordBot.MusicPlayer.Config;

using Factories;
using Microsoft.Extensions.DependencyInjection;

public class PlayerConfig
{
    public ServiceLifetime Lifetime = ServiceLifetime.Transient;

    public PlayerBufferFactory PlayerBufferFactory = PlayerBufferFactories.CreateMatroska;

    //Cookie used for youtube age restriction
    public string? Psid;

    //Cookie used for youtube age restriction
    public string? Sapisid;

    public PlayerConfig WithLifeTime(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
        return this;
    }

    public PlayerConfig WithPlayerBufferFactory(PlayerBufferFactory factory)
    {
        PlayerBufferFactory = factory;
        return this;
    }

    public PlayerConfig WithSapisid(string? sapisid)
    {
        Sapisid = sapisid;
        return this;
    }

    public PlayerConfig WithPsid(string? psid)
    {
        Psid = psid;
        return this;
    }
}
