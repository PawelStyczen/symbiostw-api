using DanceApi.Model;

namespace DanceApi.Interface;

public interface ILeadNotificationService
{
    Task<bool> SendNewLeadNotificationAsync(Lead lead, CancellationToken cancellationToken = default);
}
