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
        public bool IsGuestInstructor { get; set; }
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
        public bool? ParticipantHasPaid { get; set; }
    }

    public class MeetingDetailsDto : MeetingDto
    {
        public List<AdminNoteReadDto> Notes { get; set; } = new();
    }


    public class CreateMeetingDto
    {
       
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public int LocationId { get; set; }
        public string InstructorId { get; set; }
        public bool IsGuestInstructor { get; set; }
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

    public class AddGuestParticipantDto
    {
        public int MeetingId { get; set; }
        public int GuestUserId { get; set; }
    }
    
    public class RemoveParticipantDto
    {
        public int MeetingId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class RemoveGuestParticipantDto
    {
        public int MeetingId { get; set; }
        public int GuestUserId { get; set; }
    }
    
    public class ParticipantDto
    {
        public string Id { get; set; }
        public bool IsGuest { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool HasPaid { get; set; }
        public string RegistrationStatus { get; set; } = string.Empty;
        public bool CreatedFromPublicRequest { get; set; }
        public DateTime? RequestedAt { get; set; }
    }

    public class PendingGuestRegistrationDto
    {
        public int MeetingId { get; set; }
        public DateTime MeetingDate { get; set; }
        public string TypeOfMeetingName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string LocationCity { get; set; } = string.Empty;
        public int GuestUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool AllowNewsletter { get; set; }
        public bool AllowSmsMarketing { get; set; }
        public DateTime? RequestedAt { get; set; }
        public string RegistrationStatus { get; set; } = string.Empty;
    }

    public class UnpaidMeetingParticipantsDto
    {
        public UnpaidMeetingSummaryDto Meeting { get; set; } = null!;
        public List<ParticipantDto> Participants { get; set; } = new();
    }

    public class UnpaidMeetingSummaryDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public string TypeOfMeetingName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string LocationCity { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public bool IsGuestInstructor { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateParticipantPaymentStatusDto
    {
        public string Id { get; set; } = string.Empty;
        public bool IsGuest { get; set; }
        public bool HasPaid { get; set; }
    }

    public class UpdateParticipantRegistrationStatusDto
    {
        public string Id { get; set; } = string.Empty;
        public bool IsGuest { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PublicGuestMeetingRegistrationDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool AllowNewsletter { get; set; } = true;

        public bool AllowSmsMarketing { get; set; } = false;
    }

    public class BulkParticipantItemDto
    {
        public string Id { get; set; } = string.Empty;
        public bool IsGuest { get; set; }
    }

    public class BulkParticipantsDto
    {
        public int MeetingId { get; set; }
        public List<BulkParticipantItemDto> Participants { get; set; } = new();
    }

    public class BulkParticipantOperationItemDto
    {
        public string Id { get; set; } = string.Empty;
        public bool IsGuest { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class BulkParticipantOperationResponseDto
    {
        public int MeetingId { get; set; }
        public string Action { get; set; } = string.Empty;
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<BulkParticipantOperationItemDto> Results { get; set; } = new();
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
