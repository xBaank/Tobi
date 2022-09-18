namespace DiscordBot.MusicPlayer.Notifications;

using MediatR;
using Tracks;

/// <summary>
///     Notification used when a song pauses.
/// </summary>
public record PauseNotification(Result Result) : IResultNotification<Result>, INotification;
