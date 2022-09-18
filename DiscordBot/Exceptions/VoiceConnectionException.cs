namespace DiscordBot.Exceptions;

using System;

public class VoiceConnectionException : Exception
{
    public VoiceConnectionException(string message) : base(message)
    {
    }
}
