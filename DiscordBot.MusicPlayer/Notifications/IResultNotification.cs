namespace DiscordBot.MusicPlayer.Notifications;

public interface IResultNotification<out T>
{
    /// <summary>
    ///     Result of the notification.
    /// </summary>
    public T Result { get; }
}
