using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Converted = 3,
    Lost = 4,
    Archived = 5
}

public class Lead : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? PhoneNumber { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string NormalizedEmail { get; set; } = string.Empty;

    public bool AllowsEmailMarketing { get; set; }

    public bool AllowsNewsletterAndSmsMarketing { get; set; }

    [Required]
    [MaxLength(200)]
    public string GroupName { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? AdditionalMessage { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public DateTime SubmittedAtUtc { get; set; }

    public DateTime ConsentCapturedAtUtc { get; set; }

    public int? ConvertedGuestUserId { get; set; }

    public GuestUser? ConvertedGuestUser { get; set; }
}
