namespace DiscordBot.Controllers;

using System.Threading.Tasks;
using Proxies.Channels;

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
    Task Play(string query);
}
