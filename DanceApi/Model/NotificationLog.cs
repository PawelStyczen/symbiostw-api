using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public enum NotificationChannel
{
    Email = 0,
    Sms = 1
}

public enum NotificationKind
{
    MeetingRegistrationPendingApproval = 0,
    MeetingRegistrationAccepted = 1,
    MeetingRegistrationRejected = 2
}

public enum NotificationStatus
{
    Pending = 0,
    AcceptedByProvider = 1,
    Failed = 2,
    Skipped = 3
}

public class NotificationLog
{
    public int Id { get; set; }

    public NotificationChannel Channel { get; set; }

    public NotificationKind Kind { get; set; }

    [MaxLength(256)]
    public string Recipient { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Subject { get; set; }

    [MaxLength(100)]
    public string Provider { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? ProviderOperationId { get; set; }

    public string PlainTextContent { get; set; } = string.Empty;

    public string? HtmlContent { get; set; }

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    [MaxLength(4000)]
    public string? ErrorMessage { get; set; }

    public DateTime RequestedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get; set; }

    public int? GuestUserId { get; set; }
    public GuestUser? GuestUser { get; set; }

    public int? MeetingId { get; set; }
    public Meeting? Meeting { get; set; }
}
