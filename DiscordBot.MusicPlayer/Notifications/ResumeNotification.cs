namespace DiscordBot.MusicPlayer.Notifications;

using MediatR;
using Tracks;

/// <summary>
///     Notification used when a song is resumed
/// </summary>
public record ResumeNotification(Result Result) : IResultNotification<Result>, INotification;
