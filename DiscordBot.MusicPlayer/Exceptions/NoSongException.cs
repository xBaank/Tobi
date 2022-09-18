namespace DiscordBot.MusicPlayer.Exceptions;

using System;

public class NoSongException : Exception
{
    public NoSongException(string? message) : base(message)
    {
    }
}
