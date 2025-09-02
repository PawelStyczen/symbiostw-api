using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class MembershipPlan : VisibleHighlightBaseEntity
{

    [Required]
    public string Name { get; set; } 
    
    [Required]
    [MaxLength(200)]
    public string ShortDescription { get; set; } 
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } 

    [Required]
    public decimal Price { get; set; } 

    [Required]
    public int DurationInDays { get; set; } 
    
 
    public ICollection<TypeOfMeeting> AccessibleMeetingTypes { get; set; } = new List<TypeOfMeeting>();
}