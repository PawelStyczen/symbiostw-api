using Microsoft.AspNetCore.Identity;

namespace DanceApi.Model;

public class User : IdentityUser
{

    public string Name { get; set; }
    public string Surname { get; set; }

    public bool IsDeleted { get; set; } = false;
  
    public ICollection<MeetingParticipant> MeetingParticipants { get; set; }
    

    public ICollection<NewsComment> NewsComments { get; set; }
    
  
    public ICollection<UserMembership> UserMemberships { get; set; }
    

    public UserProfile? UserProfile { get; set; } 
    
 
    public InstructorProfile? InstructorProfile { get; set; }
    
    
}

