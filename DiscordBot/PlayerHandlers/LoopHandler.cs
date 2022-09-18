namespace DiscordBot.PlayerHandlers;

using System.Threading;
using System.Threading.Tasks;
using Controllers;
using MediatR;
using MusicPlayer.Notifications;

public class LoopHandler : INotificationHandler<LoopNotification>
{
    private readonly IMusicController _serverController;

    public LoopHandler(IMusicController serverController) => _serverController = serverController;

    public async Task Handle(LoopNotification notification, CancellationToken cancellationToken)
    {
        if (_serverController.TextChannel is null)
            return;

        await _serverController.TextChannel.SendLoop(notification.IsLooping);
    }
}
