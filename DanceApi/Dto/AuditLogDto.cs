namespace DanceApi.Dto;

public class AuditFieldChangeDto
{
    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}

public class AuditLogListItemDto
{
    public long Id { get; set; }

    public string TargetType { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public int Version { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string? ActorUserId { get; set; }

    public string? ActorDisplayName { get; set; }

    public string? ActorIdentifier { get; set; }

    public DateTime ChangedAtUtc { get; set; }

    public string? Reason { get; set; }
}

public class AuditLogDetailsDto : AuditLogListItemDto
{
    public Dictionary<string, AuditFieldChangeDto> Changes { get; set; } = new();
}
