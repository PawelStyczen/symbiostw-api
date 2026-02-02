using System.ComponentModel.DataAnnotations;
using DanceApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Dto
{
    public class MeetingDto : BaseReadDto
    {
        public int Id { get; set; }
    
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string LocationCity { get; set; }
        public string LocationStreet { get; set; }
        public string LocationDescription { get; set; }

        
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }
        public int TypeOfMeetingId { get; set; }
        public string TypeOfMeetingName { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; } 
    
        
        public bool IsIndividual { get; set; }
        
        public bool IsSolo { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsVisible { get; set; }
        
        public bool IsEvent { get; set; }
        
        public SkillLevel? Level { get; set; }
        
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
    }


    public class CreateMeetingDto
    {
       
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public int LocationId { get; set; }
        public string InstructorId { get; set; }
        public int TypeOfMeetingId { get; set; }
     
        public int? MaxParticipants { get; set; }
        
        public bool IsHighlighted { get; set; }
        public bool IsVisible { get; set; }
        
        public SkillLevel? Level { get; set; }
    }

    public class AddParticipantDto
    {
        public int MeetingId { get; set; }
        public string UserId { get; set; } = string.Empty;
        
    }
    
    public class RemoveParticipantDto
    {
        public int MeetingId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
    
    public class ParticipantDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
    }
    
    public class CopyMonthDto
    {
        public int SourceYear { get; set; }
        public int SourceMonth { get; set; }   // 1–12
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }   // 1–12
    }
    
    namespace DanceApi.Dto
    {
        public class CopyWeekDto
        {
            public int SourceYear { get; set; }
            public int SourceWeek { get; set; }   // ISO week 1..53

            public int TargetYear { get; set; }
            public int TargetWeek { get; set; }   // ISO week 1..53
        }
    }
}