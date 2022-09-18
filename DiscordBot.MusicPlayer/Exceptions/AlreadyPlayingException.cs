namespace DiscordBot.MusicPlayer.Exceptions;

using System;

public class AlreadyPlayingException : Exception
{
    public AlreadyPlayingException(string? message) : base(message)
    {
    }
}
