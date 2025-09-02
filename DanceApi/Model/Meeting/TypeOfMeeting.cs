using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class TypeOfMeeting : VisibleHighlightBaseEntity
{
    
    [Required]
    public string Name { get; set; }
    
    [MaxLength(2000)]
    public string Description { get; set; }
    
    [MaxLength(200)] 
    public string ShortDescription { get; set; } 
    public decimal Price { get; set; } 
    
    public string? ImageUrl { get; set; }

    public bool IsIndividual { get; set; } = false;
    
    public bool IsSolo { get; set; } = false;
    
    
   
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    //todo Image size validation mechanizm.
    
    
   
    public ICollection<MembershipPlan> MembershipPlans { get; set; } = new List<MembershipPlan>();
}


