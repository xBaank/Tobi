namespace DiscordBot.MusicPlayer.Notifications;

using MediatR;
using Tracks;

/// <summary>
///     Notification used when a song finish playing
/// </summary>
public record AddNotification(MediaInfo? Result) : IResultNotification<MediaInfo?>, INotification;
