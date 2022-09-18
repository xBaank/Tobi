namespace DiscordBot.Modules;

using System;
using System.Threading.Tasks;
using Controllers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Proxies.Dsharp.Channels;
using static DSharpPlus.CommandsNext.Attributes.ModuleLifespan;

[ModuleLifespan(Scoped)]
public class MusicModule : BaseCommandModule
{
    private readonly IMusicController _musicController;

    public MusicModule(IMusicController musicController) => _musicController = musicController;

    public override Task BeforeExecutionAsync(CommandContext command)
    {
        //Set the channel from where the command was executed
        var channel = new TextChannelDsharpProxy(command.Channel);
        _musicController.TextChannel = channel;
        return Task.CompletedTask;
    }

    [Command("pause")]
    public async Task PauseMusic(CommandContext command) => await _musicController.Pause();

    [Command("skip")]
    public async Task SkipMusic(CommandContext command) => await _musicController.Skip();

    [Command("loop")]
    public async Task LoopMusic(CommandContext command) => await _musicController.Loop();

    [Command("resume")]
    public async Task ResumeMusic(CommandContext command)
    {
        var voiceChannel = command.Member?.VoiceState?.Channel is not null ? new VoiceChannelDsharp(command.Member.VoiceState.Channel) : null;

        try
        {
            await _musicController.SetVoiceChannel(voiceChannel);
            await _musicController.Resume();
        }
        catch (Exception e)
        {
            if (_musicController.TextChannel is not null)
                await _musicController.TextChannel.SendError(e.Message);
        }
    }

    [Command("stop")]
    public async Task StopMusic(CommandContext command) => await _musicController.Stop();

    [Command("seek")]
    public async Task SeekMusic(CommandContext command, string? timeStamp = null) => await _musicController.Seek(timeStamp);
    
    [Command("join")]
    public async Task JoinMusic(CommandContext command)
    {
        var voiceChannel = command.Member?.VoiceState?.Channel is not null ? new VoiceChannelDsharp(command.Member.VoiceState.Channel) : null;

        try
        {
            await _musicController.SetVoiceChannel(voiceChannel);
        }
        catch (Exception e)
        {
            if (_musicController.TextChannel is not null)
                await _musicController.TextChannel.SendError(e.Message);
        }
    }

    [Command("play")]
    public async Task PlayMusic(CommandContext command, [RemainingText] string url)
    {
        var voiceChannel = command.Member?.VoiceState?.Channel is not null ? new VoiceChannelDsharp(command.Member.VoiceState.Channel) : null;

        try
        {
            await _musicController.SetVoiceChannel(voiceChannel);
            await _musicController.Play(url);
        }
        catch (Exception e)
        {
            if (_musicController.TextChannel is not null)
                await _musicController.TextChannel.SendError(e.Message);
        }
    }
}
