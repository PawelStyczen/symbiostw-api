using DanceApi.Model;

namespace DanceApi.Interface;

public interface INotificationChannelSender
{
    NotificationChannel Channel { get; }

    string ProviderName { get; }

    Task<NotificationDispatchResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
