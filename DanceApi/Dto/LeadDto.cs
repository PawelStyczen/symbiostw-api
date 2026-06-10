using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DanceApi.Dto;

public class LeadCreateDto
{
    [Required]
    [MaxLength(100)]
    [JsonPropertyName("imie")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [JsonPropertyName("nazwisko")]
    public string Surname { get; set; } = string.Empty;

    [Phone]
    [MaxLength(32)]
    [JsonPropertyName("numerTelefonu")]
    public string? PhoneNumber { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("chceotrzymywacemailmarketing")]
    public bool AllowsEmailMarketing { get; set; }

    [JsonPropertyName("chceOtrzymywacNewsletterISmsMarketing")]
    public bool AllowsNewsletterAndSmsMarketing { get; set; }

    [Required]
    [MaxLength(200)]
    [JsonPropertyName("grupa")]
    public string GroupName { get; set; } = string.Empty;

    [MaxLength(4000)]
    [JsonPropertyName("dodatkowaWiadomosc")]
    public string? AdditionalMessage { get; set; }
}

public class LeadReadDto : BaseReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool AllowsEmailMarketing { get; set; }
    public bool AllowsNewsletterAndSmsMarketing { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? AdditionalMessage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime ConsentCapturedAtUtc { get; set; }
    public int? ConvertedGuestUserId { get; set; }
    public string? ConvertedGuestUserName { get; set; }
    public bool IsDeleted { get; set; }
    public AdminNoteReadDto? LatestNote { get; set; }
}

public class LeadDetailsDto : LeadReadDto
{
    public List<AdminNoteReadDto> Notes { get; set; } = new();
}

public class LeadStatusUpdateDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Note { get; set; }
}

public class LeadConversionResultDto
{
    public LeadReadDto Lead { get; set; } = null!;
    public GuestUserDto GuestUser { get; set; } = null!;
    public bool CreatedGuestUser { get; set; }
}
