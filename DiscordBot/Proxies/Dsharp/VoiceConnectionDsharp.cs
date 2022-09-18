namespace DiscordBot.Proxies.Dsharp;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext;

[ExcludeFromCodeCoverage]
public class VoiceConnectionDsharp : IVoiceConnection
{
    private readonly VoiceNextConnection _connection;

    public VoiceConnectionDsharp(VoiceNextConnection connection)
    {
        if (!connection.IsConnected)
            throw new ArgumentException("Connection is not connected");

        _connection = connection;

        _connection.VoiceDisconnected += _ =>
        {
            VoiceDisconnected?.Invoke();
            return Task.CompletedTask;
        };
    }

    public event Func<Task>? VoiceDisconnected;

    public bool IsConnected => _connection.IsConnected;

    public Stream GetStream() => new TransmitSinkOpusProxy(_connection.GetTransmitSink());
    public void Disconnect() => _connection.Disconnect();
}
