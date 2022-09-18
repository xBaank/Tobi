namespace DiscordBot.MusicPlayer.Tracks;

public readonly struct Result
{
    private Result(Song value) : this() => Value = value;

    private Result(Exception exception) : this() => Exception = exception;

    public Song? Value { get; }

    public Exception? Exception { get; }

    public static implicit operator Result(Song result) => new(result);
    public static implicit operator Result(Exception result) => new(result);
}
