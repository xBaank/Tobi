namespace DiscordBot.PlayerHandlers;

using System.Threading;
using System.Threading.Tasks;
using Controllers;
using MediatR;
using MusicPlayer.Extensions;
using MusicPlayer.Notifications;

public class StopHandler : INotificationHandler<StopNotification>
{
    private readonly IMusicController _serverController;

    public StopHandler(IMusicController serverController) => _serverController = serverController;

    public async Task Handle(StopNotification notification, CancellationToken token) => await notification.Result
        .OnSuccessAsync(async song =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendStop(song.ReadOnlySong);
        })
        .OnFailureAsync(async ex =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendInfo(ex.Message);
        });
}
