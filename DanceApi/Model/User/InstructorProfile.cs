using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class InstructorProfile
{
    public int Id { get; set; }
        
    [Required]
    public string UserId { get; set; } 
        
    public User User { get; set; } 
        
    [MaxLength(2000)]
    public string Bio { get; set; }
        
    public int? ExperienceYears { get; set; } 
        
    [MaxLength(200)]
    public string? Specialization { get; set; } 
        
    [MaxLength(500)]
    public string? FacebookLink { get; set; } 
    [MaxLength(500)]
    public string? InstagramLink { get; set; } 
    [MaxLength(500)]
    public string? TikTokLink { get; set; } 
 
    public string? ImageUrl { get; set; } 
}