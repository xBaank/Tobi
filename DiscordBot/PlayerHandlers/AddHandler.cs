namespace DiscordBot.PlayerHandlers;

using System.Threading;
using System.Threading.Tasks;
using Controllers;
using MediatR;
using MusicPlayer.Notifications;

public class AddHandler : INotificationHandler<AddNotification>
{
    private readonly IMusicController _serverController;

    public AddHandler(IMusicController serverController) => _serverController = serverController;

    public async Task Handle(AddNotification notification, CancellationToken cancellationToken)
    {
        if (_serverController.TextChannel is null)
            return;

        if (!notification.Result.HasValue)
        {
            await _serverController.TextChannel.SendInfo("Could not add song.");
            return;
        }

        await _serverController.TextChannel.SendAddedMediaInfo(notification.Result.Value);
    }
}
