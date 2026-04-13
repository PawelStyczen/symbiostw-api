namespace DanceApi.Model;

public class NotificationMessage
{
    public NotificationChannel Channel { get; set; }

    public NotificationKind Kind { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string? RecipientDisplayName { get; set; }

    public string? Subject { get; set; }

    public string PlainTextContent { get; set; } = string.Empty;

    public string? HtmlContent { get; set; }

    public int? GuestUserId { get; set; }

    public int? MeetingId { get; set; }
}

public class NotificationDispatchResult
{
    public NotificationStatus Status { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string? ProviderOperationId { get; set; }

    public string? ErrorMessage { get; set; }
}
