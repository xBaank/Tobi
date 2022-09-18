namespace DiscordBot.Proxies;

using System;
using System.IO;
using System.Threading.Tasks;

public interface IVoiceConnection
{
    public bool IsConnected { get; }

    public Stream GetStream();
    public void Disconnect();

    public event Func<Task> VoiceDisconnected;
}
