using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class UserProfile
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } 
    
    public User User { get; set; } 
    
    public bool AllowNewsletter { get; set; } = true;
    
    [MaxLength(2000)]
    public string AboutMe { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
}