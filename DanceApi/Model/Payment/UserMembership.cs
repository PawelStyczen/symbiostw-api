using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class UserMembership : BaseEntity
{

    [Required]
    public string UserId { get; set; } 
    public User User { get; set; } 

    [Required]
    public int MembershipPlanId { get; set; } 
    public MembershipPlan MembershipPlan { get; set; } 

    [Required]
    public DateTime StartDate { get; set; } 

    [Required]
    public DateTime EndDate { get; set; } 
}