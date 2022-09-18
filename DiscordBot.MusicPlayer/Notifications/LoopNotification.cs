namespace DiscordBot.MusicPlayer.Notifications;

using MediatR;

public record LoopNotification(bool IsLooping) : INotification;
