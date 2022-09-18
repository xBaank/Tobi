namespace DiscordBot.Proxies.Dsharp.Channels;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Proxies.Channels;

[ExcludeFromCodeCoverage]
public class VoiceChannelDsharp : IVoiceChannel
{
    private readonly DiscordChannel _discordChannel;

    public VoiceChannelDsharp(DiscordChannel discordChannel)
    {
        if (discordChannel.Type != ChannelType.Voice)
            throw new InvalidDataException("Channel is not a voice channel");

        _discordChannel = discordChannel;
    }

    public bool IsEmpty => _discordChannel.Users.All(i => i.IsBot) || !_discordChannel.Users.Any();

    public async Task<IVoiceConnection> ConnectToVoiceChannel() => new VoiceConnectionDsharp(await _discordChannel.ConnectAsync());
}
