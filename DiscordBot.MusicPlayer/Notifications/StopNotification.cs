namespace DiscordBot.MusicPlayer.Notifications;

using MediatR;
using Tracks;

/// <summary>
///     Notification used when player stops.
/// </summary>
public record StopNotification(Result Result) : IResultNotification<Result>, INotification;
