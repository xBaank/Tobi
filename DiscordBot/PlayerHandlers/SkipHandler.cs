namespace DiscordBot.PlayerHandlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Controllers;
using MediatR;
using MusicPlayer.Extensions;
using MusicPlayer.Notifications;

public class SkipHandler : INotificationHandler<SkipNotification>
{
    private readonly IMusicController _serverController;

    public SkipHandler(IMusicController serverController) => _serverController = serverController;

    public async Task Handle(SkipNotification notification, CancellationToken token) => await notification.Result
        .OnSuccessAsync(async song =>
        {
            Console.WriteLine("Skip");
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendSkip(song.ReadOnlySong);
        })
        .OnFailureAsync(async ex =>
        {
            Console.WriteLine("Skip failed");
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendInfo(ex.Message);
        });
}
