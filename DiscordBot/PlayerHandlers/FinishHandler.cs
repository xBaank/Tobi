namespace DiscordBot.PlayerHandlers;

using System.Threading;
using System.Threading.Tasks;
using Controllers;
using MediatR;
using MusicPlayer.Controllers;
using MusicPlayer.Extensions;
using MusicPlayer.Notifications;
using Utils;

public class FinishHandler : INotificationHandler<FinishNotification>
{
    private readonly IPlayer _player;
    private readonly IMusicController _serverController;

    public FinishHandler(IMusicController serverController, IPlayer player)
    {
        _serverController = serverController;
        _player = player;
    }

    public async Task Handle(FinishNotification notification, CancellationToken token) => await notification.Result
        .OnSuccessAsync(async _ =>
        {
            if (_player.HasNextSong)
                return;

            await _serverController.TextChannel.IfNotNull(i => i.SendInfo("No more songs on the queue. Leaving the voice channel."));
            await _serverController.Disconnect();
        })
        .OnFailureAsync(async ex =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendInfo(ex.Message);
        });
}
