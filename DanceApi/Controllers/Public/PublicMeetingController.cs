using System.Security.Claims;
using System.Linq; // <-- needed for Where/Any
using AutoMapper;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Helper;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Controllers;

[AllowAnonymous]
[Route("api/Public/[controller]")]
[ApiController]
public class PublicMeetingController : ControllerBase
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IGuestRegistrationNotificationService _guestRegistrationNotificationService;
    private readonly ILogger<PublicMeetingController> _logger;

    public PublicMeetingController(
        IMeetingRepository meetingRepository,
        IMapper mapper,
        AppDbContext context,
        IAuditLogService auditLogService,
        IGuestRegistrationNotificationService guestRegistrationNotificationService,
        ILogger<PublicMeetingController> logger)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
        _context = context;
        _auditLogService = auditLogService;
        _guestRegistrationNotificationService = guestRegistrationNotificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicMeetings()
    {
        var meetings = await _meetingRepository.GetAllMeetingsAsync();

        var publicMeetings = meetings
            .Where(m =>
                // only visible meetings
                m.IsVisible &&
                // instructor or guest instructor must exist and be active
                HasActiveInstructor(m) &&
                // allow all non-individual OR the specific individual type
                (!m.TypeOfMeeting.IsIndividual ||
                 string.Equals(
                     m.TypeOfMeeting.Name,
                     "Indywidualne spotkanie | wolne miejsce",
                     StringComparison.OrdinalIgnoreCase))
            )
            .ToList();

        var meetingDtos = _mapper.Map<IEnumerable<MeetingDto>>(publicMeetings);
        return Ok(meetingDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeetingById(int id)
    {
        var meeting = await _meetingRepository.GetMeetingByIdAsync(id);

        if (meeting == null ||
            !meeting.IsVisible ||
            !HasActiveInstructor(meeting))
        {
            return NotFound();
        }

        var meetingDto = _mapper.Map<MeetingDto>(meeting);
        return Ok(meetingDto);
    }

    [HttpPost("{id}/guest-registration")]
    public async Task<IActionResult> RegisterGuestForMeeting(int id, [FromBody] PublicGuestMeetingRegistrationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var meeting = await _meetingRepository.GetMeetingByIdAsync(id);
        if (meeting == null || !meeting.IsVisible || !HasActiveInstructor(meeting))
            return NotFound($"Meeting with ID {id} not found.");

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var actor = AuditActorInfo.PublicRequest(normalizedEmail);
            AuditWriteRequest? guestAuditRequest = null;
            MeetingGuestParticipant? createdParticipant = null;

            var guestUser = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .Include(g => g.GuestInstructorProfile)
                .FirstOrDefaultAsync(g =>
                    !g.IsDeleted &&
                    g.GuestUserProfile != null &&
                    g.GuestInstructorProfile == null &&
                    g.Email != null &&
                    g.Email.ToLower() == normalizedEmail);

            if (guestUser == null)
            {
                guestUser = new GuestUser
                {
                    Name = dto.Name,
                    Surname = dto.Surname,
                    Email = normalizedEmail,
                    PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                    IsDeleted = false,
                    GuestUserProfile = new GuestUserProfile
                    {
                        AllowNewsletter = dto.AllowNewsletter,
                        AllowSmsMarketing = dto.AllowSmsMarketing,
                        IsPendingApproval = true
                    }
                };

                _context.GuestUsers.Add(guestUser);
                await _context.SaveChangesAsync();

                guestAuditRequest = new AuditWriteRequest
                {
                    TargetType = AuditLogTargetType.GuestUser,
                    TargetId = guestUser.Id.ToString(),
                    ActionType = AuditLogActionType.Created,
                    SourceType = AuditLogSourceType.PublicRequest,
                    Actor = actor,
                    Changes = new AuditChangeSetBuilder()
                        .AddCreated("name", guestUser.Name)
                        .AddCreated("surname", guestUser.Surname)
                        .AddCreated("email", guestUser.Email)
                        .AddCreated("phoneNumber", guestUser.PhoneNumber)
                        .AddCreated("allowNewsletter", guestUser.GuestUserProfile?.AllowNewsletter)
                        .AddCreated("allowSmsMarketing", guestUser.GuestUserProfile?.AllowSmsMarketing)
                        .Build(),
                    Reason = "Guest user created from public meeting registration."
                };
            }
            else
            {
                var guestChanges = new AuditChangeSetBuilder()
                    .Add("name", guestUser.Name, dto.Name)
                    .Add("surname", guestUser.Surname, dto.Surname)
                    .Add("phoneNumber", guestUser.PhoneNumber, string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim())
                    .Add("allowNewsletter", guestUser.GuestUserProfile?.AllowNewsletter, dto.AllowNewsletter)
                    .Add("allowSmsMarketing", guestUser.GuestUserProfile?.AllowSmsMarketing, dto.AllowSmsMarketing);

                guestUser.Name = dto.Name;
                guestUser.Surname = dto.Surname;
                guestUser.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
                guestUser.GuestUserProfile!.AllowNewsletter = dto.AllowNewsletter;
                guestUser.GuestUserProfile.AllowSmsMarketing = dto.AllowSmsMarketing;

                if (guestChanges.Count > 0)
                {
                    guestAuditRequest = new AuditWriteRequest
                    {
                        TargetType = AuditLogTargetType.GuestUser,
                        TargetId = guestUser.Id.ToString(),
                        ActionType = AuditLogActionType.Updated,
                        SourceType = AuditLogSourceType.PublicRequest,
                        Actor = actor,
                        Changes = guestChanges.Build(),
                        Reason = "Guest user data refreshed from public meeting registration."
                    };
                }
            }

            var existingParticipant = await _context.MeetingGuestParticipants
                .FirstOrDefaultAsync(p => p.MeetingId == id && p.GuestUserId == guestUser.Id);

            if (existingParticipant != null)
            {
                await transaction.RollbackAsync();
                return BadRequest("Registration request already exists for this meeting.");
            }

            createdParticipant = new MeetingGuestParticipant
            {
                MeetingId = id,
                GuestUserId = guestUser.Id,
                HasPaid = false,
                RegistrationStatus = ParticipantRegistrationStatus.Pending,
                CreatedFromPublicRequest = true,
                RequestedAt = DateTime.UtcNow
            };

            _context.MeetingGuestParticipants.Add(createdParticipant);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (guestAuditRequest != null)
                await _auditLogService.WriteAsync(guestAuditRequest);

            if (createdParticipant != null)
            {
                await _auditLogService.WriteAsync(new AuditWriteRequest
                {
                    TargetType = AuditLogTargetType.MeetingGuestParticipant,
                    TargetId = createdParticipant.Id.ToString(),
                    ActionType = AuditLogActionType.Created,
                    SourceType = AuditLogSourceType.PublicRequest,
                    Actor = actor,
                    Changes = new AuditChangeSetBuilder()
                        .AddCreated("meetingId", createdParticipant.MeetingId)
                        .AddCreated("guestUserId", createdParticipant.GuestUserId)
                        .AddCreated("registrationStatus", createdParticipant.RegistrationStatus)
                        .AddCreated("createdFromPublicRequest", createdParticipant.CreatedFromPublicRequest)
                        .AddCreated("requestedAt", createdParticipant.RequestedAt)
                        .Build(),
                    Reason = "Guest participant registration created from public meeting request."
                });
            }

            var emailSent = await _guestRegistrationNotificationService.SendPendingApprovalNotificationAsync(guestUser, meeting);
            if (!emailSent)
            {
                _logger.LogWarning(
                    "Guest registration saved but confirmation email was not sent. GuestUserId: {GuestUserId}, MeetingId: {MeetingId}",
                    guestUser.Id,
                    meeting.Id);
            }

            return Ok(new
            {
                Message = "Registration request submitted.",
                Status = ParticipantRegistrationStatus.Pending.ToString()
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [Authorize]
    [HttpPost("add-participant")]
    public async Task<IActionResult> AddParticipantToMeeting([FromBody] AddParticipantDto addParticipantDto)
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var meeting = await _meetingRepository.GetMeetingByIdAsync(addParticipantDto.MeetingId);
        if (meeting == null ||
            !HasActiveInstructor(meeting))
        {
            return NotFound($"Meeting with ID {addParticipantDto.MeetingId} not found.");
        }

        var isAlreadyParticipant = meeting.MeetingParticipants.Any(mp => mp.UserId == userId);
        if (isAlreadyParticipant) return BadRequest("You are already a participant of this meeting.");

        var result = await _meetingRepository.AddParticipantToMeetingAsync(addParticipantDto.MeetingId, userId);
        if (!result) return BadRequest("Failed to add yourself to the meeting.");

        return NoContent();
    }

    [Authorize]
    [HttpPost("remove-participant")]
    public async Task<IActionResult> RemoveParticipantFromMeeting([FromBody] RemoveParticipantDto removeParticipantDto)
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var meeting = await _meetingRepository.GetMeetingByIdAsync(removeParticipantDto.MeetingId);
        if (meeting == null) return NotFound($"Meeting with ID {removeParticipantDto.MeetingId} not found.");

        var participant = meeting.MeetingParticipants.FirstOrDefault(mp => mp.UserId == userId);
        if (participant == null) return BadRequest("You are not a participant of this meeting.");

        var result = await _meetingRepository.RemoveParticipantFromMeetingAsync(removeParticipantDto.MeetingId, userId);
        if (!result) return BadRequest("Failed to remove yourself from the meeting.");

        return NoContent();
    }

    [Authorize]
    [HttpGet("my-meetings")]
    public async Task<IActionResult> GetUserMeetings()
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var meetings = await _meetingRepository.GetMeetingsByUserIdAsync(userId);

        // keep only meetings with active (non-deleted) instructor or guest instructor
        var filtered = meetings
            .Where(HasActiveInstructor)
            .ToList();

        var meetingDtos = _mapper.Map<IEnumerable<MeetingDto>>(filtered);
        return Ok(meetingDtos);
    }

    [Authorize]
    [HttpGet("my-upcoming-meetings")]
    public async Task<IActionResult> GetUserUpcomingMeetings()
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var upcomingMeetings = await _meetingRepository.GetUpcomingMeetingsByUserIdAsync(userId);

        // keep only meetings with active (non-deleted) instructor or guest instructor
        var filtered = upcomingMeetings
            .Where(HasActiveInstructor)
            .ToList();

        var meetingDtos = _mapper.Map<IEnumerable<MeetingDto>>(filtered);
        return Ok(meetingDtos);
    }

    [HttpGet("highlighted")]
    public async Task<IActionResult> GetHighlightedMeetings([FromQuery] int count = 8)
    {
        if (count <= 0) count = 8;

        var nowUtc = DateTime.UtcNow;
        var all = await _meetingRepository.GetAllMeetingsAsync();

        // visible, future, active instructor/guest instructor, and not "Indywidualne spotkanie | wolne miejsce"
        var baseQuery = all
            .Where(m => m.IsVisible &&
                        m.Date >= nowUtc &&
                        HasActiveInstructor(m) &&
                        !string.Equals(
                            m.TypeOfMeeting?.Name,
                            "Indywidualne spotkanie | wolne miejsce",
                            StringComparison.OrdinalIgnoreCase))
            .ToList();

        var upcoming = baseQuery.OrderBy(m => m.Date).ToList();

        var highlighted = baseQuery
            .Where(m => m.IsHighlighted)
            .OrderBy(m => m.Date)
            .ToList();

        var resultMeetings = new List<DanceApi.Model.Meeting>();
        var seen = new HashSet<int>();

        foreach (var m in upcoming)
        {
            if (resultMeetings.Count >= count) break;
            if (seen.Add(m.Id)) resultMeetings.Add(m);
        }

        foreach (var m in highlighted)
        {
            if (resultMeetings.Count >= count) break;
            if (seen.Add(m.Id)) resultMeetings.Add(m);
        }

        var dto = _mapper.Map<List<MeetingDto>>(resultMeetings);
        return Ok(dto);
    }

    private static bool HasActiveInstructor(DanceApi.Model.Meeting meeting)
    {
        return (meeting.Instructor != null && !meeting.Instructor.IsDeleted)
               || (meeting.GuestInstructor != null && !meeting.GuestInstructor.IsDeleted);
    }
    
}
