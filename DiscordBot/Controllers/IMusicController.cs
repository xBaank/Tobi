using System.Threading.Tasks;
using DiscordBot.Proxies.Channels;

namespace DiscordBot.Controllers;

public interface IMusicController
{
    IMusicTextChannel? TextChannel { get; set; }

    Task SetVoiceChannel(IVoiceChannel? voiceChannel);

    Task Pause();

    Task Disconnect();

    Task Resume();

    Task Seek(string? timeStamp);

    Task Skip();

    Task Stop();

    Task Loop();

    Task Time();

    Task Play(string query);
}