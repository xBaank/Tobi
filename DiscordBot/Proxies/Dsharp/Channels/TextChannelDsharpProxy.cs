using static DSharpPlus.Entities.DiscordColor;

namespace DiscordBot.Proxies.Dsharp.Channels;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using MusicPlayer.Tracks;
using Proxies.Channels;

[ExcludeFromCodeCoverage]
public class TextChannelDsharpProxy : IMusicTextChannel
{
    private readonly DiscordChannel _channel;

    public TextChannelDsharpProxy(DiscordChannel channel)
    {
        if (channel.Type != ChannelType.Text)
            throw new ArgumentException("Channel must be a text channel");

        _channel = channel;
    }

    public async Task SendError(string error)
    {
        var embedBuilder = new DiscordEmbedBuilder
        {
            Color = DarkGray,
            Title = "Something went wrong",
            Description = error,
        };
        var embed = embedBuilder.Build();
        await _channel.SendMessageAsync(embed);
    }

    public async Task SendInfo(string message)
    {
        var embedBuilder = new DiscordEmbedBuilder
        {
            Color = Blue,
            Title = message,
        };

        var embed = embedBuilder.Build();
        await _channel.SendMessageAsync(embed);
    }

    public async Task SendSongPlaying(Song media, string mention, CancellationToken cancellationToken = default)
    {
        var embed = GetPlayEmbed(media, mention);
        await _channel.SendMessageAsync(embed);
    }

    public async Task SendResume(Song media, CancellationToken cancellationToken = default)
    {
        var embed = GetPlayEmbed(media);
        await _channel.SendMessageAsync(embed);
    }

    private static DiscordEmbed GetPlayEmbed(Song media, string? mention = null)
    {
        var embedBuilder = new DiscordEmbedBuilder
        {
            Color = DarkRed,
            Title = media.ReadOnlySong.Title,
            Url = media.ReadOnlySong.Url,
            Author = new DiscordEmbedBuilder.EmbedAuthor { Name = media.ReadOnlySong.Author, IconUrl = "https://c.tenor.com/dCsO6A_NkR4AAAAd/obito-tobi.gif", Url = media.ReadOnlySong.Url },
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = media.ReadOnlySong.ThumbnailUrl },
        };
        var durationTime = media.ReadOnlySong.Duration.ToString(@"hh\:mm\:ss");
        var currentTime = TimeSpan.Zero.ToString(@"hh\:mm\:ss");
        embedBuilder.AddField("Duration:", $"```{currentTime} / {durationTime}```");

        if (!string.IsNullOrWhiteSpace(mention))
            embedBuilder.AddField("Requested by ", mention, true);

        return embedBuilder.Build();
    }
}
