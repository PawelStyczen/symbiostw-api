using DanceApi.Model;

namespace DanceApi.Interface;

public interface IGuestRegistrationNotificationService
{
    Task<bool> SendPendingApprovalNotificationAsync(GuestUser guestUser, Meeting meeting, CancellationToken cancellationToken = default);
    Task<bool> SendAcceptedNotificationAsync(GuestUser guestUser, Meeting meeting, CancellationToken cancellationToken = default);
    Task<bool> SendRejectedNotificationAsync(GuestUser guestUser, Meeting meeting, CancellationToken cancellationToken = default);
}
