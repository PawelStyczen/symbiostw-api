namespace DanceApi.Model;

public enum ParticipantRegistrationStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public class MeetingGuestParticipant
{
    public int Id { get; set; }

    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; }

    public int GuestUserId { get; set; }
    public GuestUser GuestUser { get; set; }

    public bool HasPaid { get; set; } = false;

    public ParticipantRegistrationStatus RegistrationStatus { get; set; } = ParticipantRegistrationStatus.Accepted;

    public bool CreatedFromPublicRequest { get; set; } = false;

    public DateTime? RequestedAt { get; set; }
}
