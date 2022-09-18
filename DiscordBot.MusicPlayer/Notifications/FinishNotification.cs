namespace DiscordBot.MusicPlayer.Notifications;

using MediatR;
using Tracks;

/// <summary>
///     Notification used when a song finish playing
/// </summary>
public record FinishNotification(Result Result) : IResultNotification<Result>, INotification;
