using DanceApi.Model;

namespace DanceApi.Interface;

public interface INotificationDispatcher
{
    Task<bool> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
