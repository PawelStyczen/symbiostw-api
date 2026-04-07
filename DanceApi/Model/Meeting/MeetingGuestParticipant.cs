namespace DanceApi.Model;

public class MeetingGuestParticipant
{
    public int Id { get; set; }

    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; }

    public int GuestUserId { get; set; }
    public GuestUser GuestUser { get; set; }
}