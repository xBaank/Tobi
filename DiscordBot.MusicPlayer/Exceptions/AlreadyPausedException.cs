namespace DiscordBot.MusicPlayer.Exceptions;

using System;

public class AlreadyPausedException : Exception
{
    public AlreadyPausedException(string? message) : base(message)
    {
    }
}
