using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class GuestUser
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? PhoneNumber { get; set; }

    public bool IsDeleted { get; set; } = false;

    public GuestUserProfile? GuestUserProfile { get; set; }

    public GuestInstructorProfile? GuestInstructorProfile { get; set; }

    public ICollection<MeetingGuestParticipant> MeetingGuestParticipants { get; set; }
        = new List<MeetingGuestParticipant>();
}
