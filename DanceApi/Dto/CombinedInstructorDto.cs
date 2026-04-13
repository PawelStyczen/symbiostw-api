namespace DanceApi.Dto;

public class CombinedInstructorDto
{
    public string Id { get; set; }
    public bool IsGuest { get; set; }
    public string InstructorType { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsDeleted { get; set; }
    public string? Bio { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Specialization { get; set; }
    public string? FacebookLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? TikTokLink { get; set; }
    public string? ImageUrl { get; set; }
}
