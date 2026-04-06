using System.ComponentModel.DataAnnotations;
using DanceApi.Model;

namespace DanceApi.Dto;

public class EventDto
{
    public class CreateEventDto
    {
        // TYPE (event meta)
        [Required]
        public string Name { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public decimal Price { get; set; }
        
        public bool IsVisible { get; set; } = true;

        // MEETING (termin)
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        public string InstructorId { get; set; } = null!;

        public SkillLevel? Level { get; set; }
        public int? MaxParticipants { get; set; }
    }
    
    public class UpdateEventDto
    {
        // TYPE
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }

     
        public bool IsVisible { get; set; }

        // MEETING
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public int LocationId { get; set; }
        public string InstructorId { get; set; }
        public SkillLevel? Level { get; set; }
    }
}