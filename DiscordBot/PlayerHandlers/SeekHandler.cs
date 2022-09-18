namespace DiscordBot.PlayerHandlers;

using System.Threading;
using System.Threading.Tasks;
using Controllers;
using MediatR;
using MusicPlayer.Extensions;
using MusicPlayer.Notifications;

public class SeekHandler : INotificationHandler<SeekNotification>
{
    private readonly IMusicController _serverController;

    public SeekHandler(IMusicController serverController) => _serverController = serverController;

    public async Task Handle(SeekNotification notification, CancellationToken token) => await notification.Result
        .OnSuccessAsync(async song =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendSeek(song.ReadOnlySong, notification.TimeStamp);
        })
        .OnFailureAsync(async ex =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendInfo(ex.Message);
        });
}
