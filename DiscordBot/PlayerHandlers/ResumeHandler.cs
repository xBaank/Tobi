namespace DiscordBot.PlayerHandlers;

using System.Threading;
using System.Threading.Tasks;
using Controllers;
using MediatR;
using MusicPlayer.Extensions;
using MusicPlayer.Notifications;

public class ResumeHandler : INotificationHandler<ResumeNotification>
{
    private readonly IMusicController _serverController;

    public ResumeHandler(IMusicController serverController) => _serverController = serverController;

    public async Task Handle(ResumeNotification notification, CancellationToken token) => await notification.Result
        .OnSuccessAsync(async media =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendResume(media.ReadOnlySong);
        })
        .OnFailureAsync(async ex =>
        {
            if (_serverController.TextChannel is not null)
                await _serverController.TextChannel.SendInfo(ex.Message);
        });
}
