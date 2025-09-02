using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;


public class Review : VisibleHighlightBaseEntity
{

    [Required]
    [MaxLength(1000)]    
    public string Content { get; set; } 
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; } 
    [Required]
    public int TypeOfMeetingId { get; set; } 
    [Required]
    public string UserId { get; set; } 
    [Required]    
    public TypeOfMeeting TypeOfMeeting { get; set; } 
    [Required]
    public User User { get; set; } 
}