using System;
using System.Threading.Tasks;
using DiscordBot.Controllers;
using DiscordBot.Proxies.Dsharp.Channels;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using static DiscordBot.Utils.ScopedLocator;


namespace DiscordBot.Modules;

[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
public class MusicSlashModule : ApplicationCommandModule
{
    private readonly IServiceProvider _serviceProvider;
    private IMusicController _musicController;

    public MusicSlashModule(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;


    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        var scope = TryGetScopedProvider(ctx.Guild.Id);
        if (scope is null)
        {
            scope = _serviceProvider.CreateScope();
            AddScopedProvider(ctx.Guild.Id, scope);
        }

        _musicController = scope.ServiceProvider.GetRequiredService<IMusicController>();
        //Set the channel from where the command was executed
        var channel = new TextChannelDsharpProxy(ctx.Channel);
        _musicController.TextChannel = channel;

        return Task.FromResult(true);
    }

    [SlashCommand("pause", "Pauses the music")]
    public async Task PauseMusic(InteractionContext ctx)
    {
        await _musicController.Pause();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Command executed"));
    }
    
    [ContextMenu(ApplicationCommandType.AutoCompleteRequest, "sdffds")]
    public async Task PlayMusic(ContextMenuContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Command executed"));
    }
}