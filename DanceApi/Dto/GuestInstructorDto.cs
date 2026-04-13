namespace DanceApi.Dto;

public class GuestInstructorDto : BaseReadDto
{
    public int Id { get; set; }
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

public class GuestInstructorCreateDto
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Specialization { get; set; }

    public string? FacebookLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? TikTokLink { get; set; }
    
    public string? ImageUrl { get; set; }

    public bool ShowOnPublicInstructorsPage { get; set; } = false;
}

public class GuestInstructorUpdateDto
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Specialization { get; set; }

    public string? FacebookLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? TikTokLink { get; set; }

    public string? ImageUrl { get; set; }
    public bool ShowOnPublicInstructorsPage { get; set; }
}
