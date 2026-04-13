using AutoMapper;
using DanceApi.Dto;
using DanceApi.Model;

namespace DanceApi.Helper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Base -> BaseReadDto (only for entities that derive from BaseEntity)
        CreateMap<BaseEntity, BaseReadDto>()
            .ForMember(d => d.CreatedById,  o => o.MapFrom(s => s.CreatedById))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy != null ? s.CreatedBy.Name : "Unknown"))
            .ForMember(d => d.CreatedDate,  o => o.MapFrom(s => s.CreatedDate))
            .ForMember(d => d.UpdatedById,  o => o.MapFrom(s => s.UpdatedById))
            .ForMember(d => d.UpdatedByName, o => o.MapFrom(s => s.UpdatedBy != null ? s.UpdatedBy.Name : "Unknown"))
            .ForMember(d => d.UpdatedDate,  o => o.MapFrom(s => s.UpdatedDate));

        // Location (derives from BaseEntity)
        CreateMap<Location, LocationReadDto>()
            .IncludeBase<BaseEntity, BaseReadDto>();
        CreateMap<LocationCreateDto, Location>();
        CreateMap<LocationUpdateDto, Location>();

        // TypeOfMeeting (derives from BaseEntity)
        CreateMap<TypeOfMeeting, TypeOfMeetingReadDto>()
            .IncludeBase<BaseEntity, BaseReadDto>();
        CreateMap<TypeOfMeetingCreateDto, TypeOfMeeting>();
        CreateMap<TypeOfMeetingUpdateDto, TypeOfMeeting>();

        // Meeting (derives from BaseEntity)
        CreateMap<Meeting, MeetingDto>()
            .IncludeBase<BaseEntity, BaseReadDto>()
            .ForMember(d => d.LocationName, o => o.MapFrom(s => s.Location.Name))
            .ForMember(d => d.LocationCity, o => o.MapFrom(s => s.Location.City))
            .ForMember(d => d.LocationStreet, o => o.MapFrom(s => s.Location.Street))
            .ForMember(d => d.LocationDescription, o => o.MapFrom(s => s.Location.Description))
            .ForMember(d => d.InstructorId, o => o.MapFrom(s =>
                s.InstructorId ?? (s.GuestInstructorId.HasValue ? s.GuestInstructorId.Value.ToString() : string.Empty)))
            .ForMember(d => d.InstructorName, o => o.MapFrom(s =>
                s.Instructor != null
                    ? $"{s.Instructor.Name} {s.Instructor.Surname}"
                    : s.GuestInstructor != null
                        ? $"{s.GuestInstructor.Name} {s.GuestInstructor.Surname}"
                        : string.Empty))
            .ForMember(d => d.IsGuestInstructor, o => o.MapFrom(s => s.GuestInstructorId.HasValue))
            .ForMember(d => d.TypeOfMeetingName, o => o.MapFrom(s => s.TypeOfMeeting.Name))
            .ForMember(d => d.IsIndividual, o => o.MapFrom(s => s.TypeOfMeeting.IsIndividual))
            .ForMember(d => d.IsSolo, o => o.MapFrom(s => s.TypeOfMeeting.IsSolo))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.TypeOfMeeting.ImageUrl))
            .ForMember(d => d.IsEvent, o => o.MapFrom(s => s.TypeOfMeeting.IsEvent))
            .ForMember(d => d.Level, o => o.MapFrom(s => s.Level))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.TypeOfMeeting.Description))
            .ForMember(d => d.ShortDescription, o => o.MapFrom(s => s.TypeOfMeeting.ShortDescription));
        
        

        CreateMap<Meeting, PublicEventDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.TypeOfMeeting.Name))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.TypeOfMeeting.Description))
            .ForMember(d => d.ShortDescription, o => o.MapFrom(s => s.TypeOfMeeting.ShortDescription))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.TypeOfMeeting.ImageUrl))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price))
            .ForMember(d => d.LocationName, o => o.MapFrom(s => s.Location.Name))
            .ForMember(d => d.InstructorId, o => o.MapFrom(s =>
                s.InstructorId ?? (s.GuestInstructorId.HasValue ? s.GuestInstructorId.Value.ToString() : string.Empty)))
            .ForMember(d => d.InstructorName, o => o.MapFrom(s =>
                s.Instructor != null
                    ? $"{s.Instructor.Name} {s.Instructor.Surname}"
                    : s.GuestInstructor != null
                        ? $"{s.GuestInstructor.Name} {s.GuestInstructor.Surname}"
                        : string.Empty))
            .ForMember(d => d.IsGuestInstructor, o => o.MapFrom(s => s.GuestInstructorId.HasValue))
            .ForMember(d => d.IsEvent, o => o.MapFrom(_ => true));
 
        CreateMap<CreateMeetingDto, Meeting>();

        // ❗️User does NOT derive from BaseEntity — DO NOT IncludeBase here
        CreateMap<User, InstructorDto>()
            // basic user fields
            .ForMember(d => d.Id,     o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Name,   o => o.MapFrom(s => s.Name))
            .ForMember(d => d.Surname,o => o.MapFrom(s => s.Surname))
            .ForMember(d => d.Email,  o => o.MapFrom(s => s.Email))
            // if User has City/Street, map; otherwise set to null
           
            .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
            // from InstructorProfile (may be null)
            .ForMember(d => d.Bio,              o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.Bio : null))
            .ForMember(d => d.ExperienceYears,  o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.ExperienceYears : 0))
            .ForMember(d => d.Specialization,   o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.Specialization : null))
            .ForMember(d => d.FacebookLink, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.FacebookLink : null))
            .ForMember(d => d.InstagramLink, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.InstagramLink : null))
            .ForMember(d => d.TikTokLink, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.TikTokLink : null))
            .ForMember(d => d.ImageUrl,         o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.ImageUrl : null))
            // InstructorDto inherits BaseReadDto, but User doesn't have those audit fields -> ignore them
            .ForMember(d => d.CreatedById,   o => o.Ignore())
            .ForMember(d => d.CreatedByName, o => o.Ignore())
            .ForMember(d => d.CreatedDate,   o => o.Ignore())
            .ForMember(d => d.UpdatedById,   o => o.Ignore())
            .ForMember(d => d.UpdatedByName, o => o.Ignore())
            .ForMember(d => d.UpdatedDate,   o => o.Ignore());

        // PublicInstructorDto (also from User) – same idea, no IncludeBase
        CreateMap<User, PublicInstructorDto>()
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.InstructorProfile.Bio))
            .ForMember(d => d.ExperienceYears, o => o.MapFrom(s => s.InstructorProfile.ExperienceYears))
            .ForMember(d => d.Specialization, o => o.MapFrom(s => s.InstructorProfile.Specialization))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.InstructorProfile.ImageUrl))
            .ForMember(d => d.FacebookLink,
                o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.FacebookLink : null))
            .ForMember(d => d.InstagramLink,
                o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.InstagramLink : null))
            .ForMember(d => d.TikTokLink,
                o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.TikTokLink : null));

        CreateMap<User, CombinedInstructorDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.IsGuest, o => o.MapFrom(_ => false))
            .ForMember(d => d.InstructorType, o => o.MapFrom(_ => "regular"))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.Bio : null))
            .ForMember(d => d.ExperienceYears, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.ExperienceYears : null))
            .ForMember(d => d.Specialization, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.Specialization : null))
            .ForMember(d => d.FacebookLink, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.FacebookLink : null))
            .ForMember(d => d.InstagramLink, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.InstagramLink : null))
            .ForMember(d => d.TikTokLink, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.TikTokLink : null))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.InstructorProfile != null ? s.InstructorProfile.ImageUrl : null));

        CreateMap<GuestUser, GuestInstructorDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.Surname, o => o.MapFrom(s => s.Surname))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.Bio : null))
            .ForMember(d => d.ExperienceYears, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.ExperienceYears : null))
            .ForMember(d => d.Specialization, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.Specialization : null))
            .ForMember(d => d.FacebookLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.FacebookLink : null))
            .ForMember(d => d.InstagramLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.InstagramLink : null))
            .ForMember(d => d.TikTokLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.TikTokLink : null))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.ImageUrl : null))
            .ForMember(d => d.CreatedById, o => o.Ignore())
            .ForMember(d => d.CreatedByName, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedById, o => o.Ignore())
            .ForMember(d => d.UpdatedByName, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore());
        
        CreateMap<GuestUser, PublicGuestInstructorDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.Surname, o => o.MapFrom(s => s.Surname))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.Bio : string.Empty))
            .ForMember(d => d.Specialization, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.Specialization : string.Empty))
            .ForMember(d => d.ExperienceYears, o => o.MapFrom(s => s.GuestInstructorProfile != null && s.GuestInstructorProfile.ExperienceYears.HasValue
                ? s.GuestInstructorProfile.ExperienceYears.Value
                : 0))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.ImageUrl : string.Empty))
            .ForMember(d => d.FacebookLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.FacebookLink : null))
            .ForMember(d => d.InstagramLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.InstagramLink : null))
            .ForMember(d => d.TikTokLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.TikTokLink : null));

        CreateMap<GuestUser, CombinedInstructorDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.IsGuest, o => o.MapFrom(_ => true))
            .ForMember(d => d.InstructorType, o => o.MapFrom(_ => "guest"))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.Bio : null))
            .ForMember(d => d.ExperienceYears, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.ExperienceYears : null))
            .ForMember(d => d.Specialization, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.Specialization : null))
            .ForMember(d => d.FacebookLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.FacebookLink : null))
            .ForMember(d => d.InstagramLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.InstagramLink : null))
            .ForMember(d => d.TikTokLink, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.TikTokLink : null))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.GuestInstructorProfile != null ? s.GuestInstructorProfile.ImageUrl : null));

        CreateMap<User, CombinedUserDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.IsGuest, o => o.MapFrom(_ => false))
            .ForMember(d => d.UserType, o => o.MapFrom(_ => "regular"))
            .ForMember(d => d.AllowNewsletter, o => o.MapFrom(s => s.UserProfile != null ? s.UserProfile.AllowNewsletter : (bool?)null))
            .ForMember(d => d.AllowSmsMarketing, o => o.MapFrom(s => s.UserProfile != null ? s.UserProfile.AllowSmsMarketing : (bool?)null))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.UserProfile != null ? s.UserProfile.AboutMe : null));

        CreateMap<User, AdminUserDetailsDto>()
            .ForMember(d => d.AllowNewsletter, o => o.MapFrom(s => s.UserProfile != null && s.UserProfile.AllowNewsletter))
            .ForMember(d => d.AllowSmsMarketing, o => o.MapFrom(s => s.UserProfile != null && s.UserProfile.AllowSmsMarketing))
            .ForMember(d => d.Notes, o => o.Ignore());

        CreateMap<GuestUser, GuestUserDto>()
            .ForMember(d => d.AllowNewsletter, o => o.MapFrom(s => s.GuestUserProfile != null && s.GuestUserProfile.AllowNewsletter))
            .ForMember(d => d.AllowSmsMarketing, o => o.MapFrom(s => s.GuestUserProfile != null && s.GuestUserProfile.AllowSmsMarketing))
            .ForMember(d => d.IsPendingApproval, o => o.MapFrom(s => s.GuestUserProfile != null && s.GuestUserProfile.IsPendingApproval))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.GuestUserProfile != null ? s.GuestUserProfile.Bio : null));

        CreateMap<GuestUser, GuestUserDetailsDto>()
            .IncludeBase<GuestUser, GuestUserDto>()
            .ForMember(d => d.Notes, o => o.Ignore());

        CreateMap<GuestUser, CombinedUserDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.IsGuest, o => o.MapFrom(_ => true))
            .ForMember(d => d.UserType, o => o.MapFrom(_ => "guest"))
            .ForMember(d => d.AllowNewsletter, o => o.MapFrom(s => s.GuestUserProfile != null ? s.GuestUserProfile.AllowNewsletter : (bool?)null))
            .ForMember(d => d.AllowSmsMarketing, o => o.MapFrom(s => s.GuestUserProfile != null ? s.GuestUserProfile.AllowSmsMarketing : (bool?)null))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.GuestUserProfile != null ? s.GuestUserProfile.Bio : null));
        
        // NewsArticle (if derives from BaseEntity, you can IncludeBase; otherwise don’t)
        CreateMap<NewsArticle, NewsArticleReadDto>()
            .ForMember(d => d.CreatedById,   o => o.MapFrom(s => s.CreatedById))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy != null ? s.CreatedBy.Name : "Unknown"));
        CreateMap<NewsArticleCreateDto, NewsArticle>();
        CreateMap<NewsArticleUpdateDto, NewsArticle>();

        // NewsComment (derives from BaseEntity)
        CreateMap<NewsComment, NewsCommentReadDto>()
            .IncludeBase<BaseEntity, BaseReadDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.CreatedBy != null ? s.CreatedBy.Name : "Anonymous"))
            .ForMember(d => d.NewsArticleTitle, o => o.MapFrom(s => s.NewsArticle.Title));

        // ContactMessage
        CreateMap<ContactMessageCreateDto, ContactMessage>();
        CreateMap<ContactMessage, ContactMessageReadDto>();

        CreateMap<AdminNote, AdminNoteReadDto>()
            .IncludeBase<BaseEntity, BaseReadDto>();

        CreateMap<Meeting, MeetingDetailsDto>()
            .IncludeBase<Meeting, MeetingDto>()
            .ForMember(d => d.Notes, o => o.Ignore());
    }
}
