namespace DiscordBot.Proxies.Channels;

using System.Threading.Tasks;

public interface IVoiceChannel
{
    bool IsEmpty { get; }

    Task<IVoiceConnection> ConnectToVoiceChannel();
}
