using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class GuestUserProfile
{
    public int Id { get; set; }

    [Required]
    public int GuestUserId { get; set; }

    public GuestUser GuestUser { get; set; }

    public bool AllowNewsletter { get; set; } = true;

    public bool AllowSmsMarketing { get; set; } = false;

    public bool IsPendingApproval { get; set; } = false;

    [MaxLength(2000)]
    public string? Bio { get; set; }
}
