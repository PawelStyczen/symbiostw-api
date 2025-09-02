namespace DanceApi.Dto;

public class PublicInstructorDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Bio { get; set; }
    public string Specialization { get; set; }
    public int ExperienceYears { get; set; }
    public string ImageUrl { get; set; }
    public string? FacebookLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? TikTokLink { get; set; }
}