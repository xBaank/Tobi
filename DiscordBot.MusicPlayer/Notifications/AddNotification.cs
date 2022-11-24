using DiscordBot.MusicPlayer.Tracks.Inmutable;

namespace DiscordBot.MusicPlayer.Notifications;

using MediatR;

/// <summary>
///     Notification used when a song finish playing
/// </summary>
public record AddNotification(MediaInfo? Result) : IResultNotification<MediaInfo?>, INotification;
