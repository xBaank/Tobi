namespace DiscordBot.MusicPlayer.Exceptions;

using System;

public class InvalidTimeException : Exception
{
    public InvalidTimeException(string message) : base(message)
    {
    }
}
