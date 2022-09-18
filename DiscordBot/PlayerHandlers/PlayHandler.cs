namespace DiscordBot.PlayerHandlers;

using System.Threading;
using System.Threading.Tasks;
using Controllers;
using MediatR;
using MusicPlayer.Extensions;
using MusicPlayer.Notifications;

public class PlayHandler : INotificationHandler<PlayNotification>
{
    private readonly IMusicController _serverController;

    public PlayHandler(IMusicController serverController) => _serverController = serverController;

    public async Task Handle(PlayNotification notification, CancellationToken cancellationToken) => await notification.Result
        .OnSuccessAsync(async media =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendSongPlaying(media, string.Empty, cancellationToken);
        })
        .OnFailureAsync(async ex =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendInfo(ex.Message);
        });
}
