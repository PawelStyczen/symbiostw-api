using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DanceApi.Model;

public enum AuditLogTargetType
{
    User = 1,
    GuestUser = 2,
    MeetingGuestParticipant = 3,
    Meeting = 4
}

public enum AuditLogActionType
{
    Created = 1,
    Updated = 2,
    StatusChanged = 3,
    Approved = 4,
    Rejected = 5
}

public enum AuditLogSourceType
{
    AdminPanel = 1,
    PublicRequest = 2,
    UserPanel = 3,
    System = 4
}

public class AuditFieldChange
{
    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}

public class AuditActorInfo
{
    public string? UserId { get; set; }

    public string? DisplayName { get; set; }

    public string? Identifier { get; set; }

    public static AuditActorInfo FromPrincipal(ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal?.FindFirstValue(ClaimTypes.Email);
        var role = principal?.FindFirstValue(ClaimTypes.Role);

        return new AuditActorInfo
        {
            UserId = userId,
            DisplayName = email ?? role ?? userId ?? "Authenticated user",
            Identifier = email ?? userId
        };
    }

    public static AuditActorInfo PublicRequest(string? identifier)
    {
        return new AuditActorInfo
        {
            DisplayName = "Public request",
            Identifier = identifier
        };
    }

    public static AuditActorInfo System(string? identifier = null)
    {
        return new AuditActorInfo
        {
            DisplayName = "System",
            Identifier = identifier
        };
    }
}

public class AuditWriteRequest
{
    public AuditLogTargetType TargetType { get; set; }

    public string TargetId { get; set; } = string.Empty;

    public AuditLogActionType ActionType { get; set; }

    public AuditLogSourceType SourceType { get; set; }

    public AuditActorInfo? Actor { get; set; }

    public IReadOnlyDictionary<string, AuditFieldChange> Changes { get; set; } =
        new Dictionary<string, AuditFieldChange>();

    public string? Reason { get; set; }
}

public class AuditLog
{
    public long Id { get; set; }

    public AuditLogTargetType TargetType { get; set; }

    [MaxLength(450)]
    public string TargetId { get; set; } = string.Empty;

    public int Version { get; set; }

    public AuditLogActionType ActionType { get; set; }

    public AuditLogSourceType SourceType { get; set; }

    [MaxLength(450)]
    public string? ActorUserId { get; set; }

    [MaxLength(200)]
    public string? ActorDisplayName { get; set; }

    [MaxLength(256)]
    public string? ActorIdentifier { get; set; }

    public DateTime ChangedAtUtc { get; set; }

    public string ChangesJson { get; set; } = "{}";

    [MaxLength(4000)]
    public string? Reason { get; set; }
}
