namespace DiscordBot.Proxies.Channels;

using System.Threading.Tasks;

public interface ITextChannel
{
    Task SendError(string error);
    Task SendInfo(string message);
}
