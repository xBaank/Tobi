using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DiscordBot.Exceptions;
using DiscordBot.MusicPlayer.Controllers;
using DiscordBot.Proxies;
using DiscordBot.Proxies.Channels;
using DiscordBot.Utils;
using Nito.AsyncEx;

namespace DiscordBot.Controllers;

using static TimeSpan;

public class MusicController : IMusicController
{
    private readonly IPlayer _musicPlayer;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private IVoiceConnection? _voiceConnection;

    public MusicController(IPlayer musicPlayer) => _musicPlayer = musicPlayer;

    public IMusicTextChannel? TextChannel { get; set; }

    public async Task SetVoiceChannel(IVoiceChannel? voiceChannel)
    {
        using var _ = await _semaphoreSlim.LockAsync();
        if (_voiceConnection?.IsConnected == true) return;

        if (voiceChannel is null)
        {
            await TextChannel.IfNotNull(i => i.SendInfo(":no_entry_sign: User must be in a voice channel"));
            return;
        }

        _voiceConnection = await voiceChannel.ConnectToVoiceChannel();
        await _musicPlayer.SetStream(_voiceConnection?.GetStream() ?? throw new VoiceConnectionException("Couldn't connect to voice channel"));

        _voiceConnection.VoiceDisconnected += OnVoiceConnectionOnVoiceDisconnected;
    }


    public async Task Pause() => await _musicPlayer.Pause();

    public async Task Disconnect()
    {
        using var _ = await _semaphoreSlim.LockAsync();
        if (_voiceConnection is null) return;
        _voiceConnection.VoiceDisconnected -= OnVoiceConnectionOnVoiceDisconnected;
        _voiceConnection?.Disconnect();
    }

    public async Task Resume()
    {
        if (_voiceConnection?.IsConnected == false || _voiceConnection is null)
            return;

        await _musicPlayer.Resume();
    }

    public async Task Seek(string? timeStamp)
    {
        if (string.IsNullOrWhiteSpace(timeStamp))
        {
            await TextChannel.IfNotNull(i => i.SendInfo("No time specified."));
            return;
        }

        var isParsed = TryParseExact(timeStamp, "g", CultureInfo.InvariantCulture, out var time);

        if (isParsed)
        {
            await _musicPlayer.Seek((long) time.TotalMilliseconds);
            return;
        }

        await TextChannel.IfNotNull(i => i.SendInfo("Wrong time format. Should be hh:mm:ss"));
    }

    public async Task Skip() => await _musicPlayer.Skip();

    public async Task Stop()
    {
        await _musicPlayer.Stop();
        await Disconnect();
    }

    public async Task Loop() => await _musicPlayer.Loop();

    public async Task Play(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await TextChannel.IfNotNull(i => i.SendInfo("Query is empty"));
            return;
        }

        if (_voiceConnection?.IsConnected == false || _voiceConnection is null)
            return;

        var addTask = _musicPlayer.Add(query);
        Task? playtask = null;

        if (!_musicPlayer.IsPlaying)
            playtask = _musicPlayer.Play();

        await Task.WhenAll(addTask, playtask ?? Task.CompletedTask);
    }

    public async Task Time()
    {
        var song = _musicPlayer.CurrentSong;

        if (TextChannel is null) return;
        if (song is null || _musicPlayer.HasFinished)
        {
            await TextChannel.SendInfo("No song is playing");
            return;
        }

        await TextChannel.SendInfo($"{_musicPlayer.CurrentTime:hh':'mm':'ss}/{song.TotalTime:hh':'mm':'ss}");
    }

    private async Task OnVoiceConnectionOnVoiceDisconnected()
    {
        using var _ = await _semaphoreSlim.LockAsync();
        if (!_musicPlayer.IsPaused)
            await _musicPlayer.Pause();
    }
}