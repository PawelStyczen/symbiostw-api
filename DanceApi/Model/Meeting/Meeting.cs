using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;
public enum SkillLevel
{
        Beginner = 0,
        Intermediate  = 1,
        Advanced = 2
}
public class Meeting : VisibleHighlightBaseEntity
{
      
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int Duration { get; set; } 
        
        // ⬇️ Optional level: null = open to all
        public SkillLevel? Level { get; set; }
        
        [Required]
        public int LocationId { get; set; } 
        public Location Location { get; set; } 
        public string? InstructorId { get; set; }
        public User? Instructor { get; set; }
        
        public int? GuestInstructorId { get; set; }
        public GuestUser? GuestInstructor { get; set; }

        [Required]
        public int TypeOfMeetingId { get; set; }
        public TypeOfMeeting TypeOfMeeting { get; set; }

        public decimal Price { get; set; } 
       
        
 

        public ICollection<MeetingParticipant> MeetingParticipants { get; set; }
                = new List<MeetingParticipant>();

        public ICollection<MeetingGuestParticipant> MeetingGuestParticipants { get; set; }
                = new List<MeetingGuestParticipant>();
    
}