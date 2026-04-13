namespace DanceApi.Dto;

public class NotificationLogListItemDto
{
    public int Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? ProviderOperationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
    public int? GuestUserId { get; set; }
    public string? GuestUserName { get; set; }
    public int? MeetingId { get; set; }
    public string? MeetingName { get; set; }
    public DateTime? MeetingDateUtc { get; set; }
}

public class NotificationLogDetailsDto : NotificationLogListItemDto
{
    public string PlainTextContent { get; set; } = string.Empty;
    public string? HtmlContent { get; set; }
}
