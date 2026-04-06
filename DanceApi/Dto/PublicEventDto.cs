using DanceApi.Model;

namespace DanceApi.Dto;



public class PublicEventDto
{
    public int Id { get; set; }

    public DateTime Date { get; set; }
    public int Duration { get; set; }

    // Event (TypeOfMeeting)
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }

    // Meeting
    public int LocationId { get; set; }
    public string LocationName { get; set; }

    public string InstructorId { get; set; }
    public string InstructorName { get; set; }

    public SkillLevel? Level { get; set; }

    // Flags
    public bool IsEvent { get; set; } = true;
}