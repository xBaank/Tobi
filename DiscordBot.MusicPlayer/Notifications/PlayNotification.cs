namespace DiscordBot.MusicPlayer.Notifications;

using MediatR;
using Tracks;

/// <summary>
///     Notification used when a song starts playing
/// </summary>
public record PlayNotification(Result Result) : IResultNotification<Result>, INotification;
