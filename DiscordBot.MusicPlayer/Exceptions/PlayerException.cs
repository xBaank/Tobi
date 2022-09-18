namespace DiscordBot.MusicPlayer.Exceptions;

using System;

public class MusicPlayerException : Exception
{
    public MusicPlayerException(string? message) : base(message)
    {
    }
}
