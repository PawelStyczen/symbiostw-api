using AutoMapper;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Helper;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DanceApi.Dto.DanceApi.Dto;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/Admin/[controller]")]
[ApiController]
public class AdminMeetingController : ControllerBase
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMapper _mapper;
    private readonly ITypeOfMeetingRepository _typeOfMeetingRepository;
    private readonly AppDbContext _context;
    private readonly IGuestRegistrationNotificationService _guestRegistrationNotificationService;
    private readonly ILogger<AdminMeetingController> _logger;
    private readonly IAdminNoteService _adminNoteService;
    private readonly IAuditLogService _auditLogService;

    public AdminMeetingController(
        IMeetingRepository meetingRepository,
        ITypeOfMeetingRepository typeOfMeetingRepository,
        IMapper mapper,
        AppDbContext context,
        IGuestRegistrationNotificationService guestRegistrationNotificationService,
        ILogger<AdminMeetingController> logger,
        IAdminNoteService adminNoteService,
        IAuditLogService auditLogService)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
        _typeOfMeetingRepository = typeOfMeetingRepository;
        _context = context;
        _guestRegistrationNotificationService = guestRegistrationNotificationService;
        _logger = logger;
        _adminNoteService = adminNoteService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMeetings(
        [FromQuery] int? locationId = null, 
        [FromQuery] int? typeOfMeetingId = null,
        [FromQuery] bool? isVisible = null)
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User?.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }

        List<Meeting> meetings;

        if (userRole == "Admin")
        {
            meetings = (await _meetingRepository.GetAllMeetingsAsync()).ToList();
        }
        else if (userRole == "Instructor")
        {
            meetings = await _meetingRepository.GetMeetingsByInstructorIdAsync(userId);
        }
        else
        {
            return Forbid("Access denied.");
        }

        //  filters
        if (locationId.HasValue)
        {
            meetings = meetings.Where(m => m.LocationId == locationId.Value).ToList();
        }
        if (typeOfMeetingId.HasValue)
        {
            meetings = meetings.Where(m => m.TypeOfMeetingId == typeOfMeetingId.Value).ToList();
        }
        if (isVisible.HasValue)
        {
            meetings = meetings.Where(m => m.IsVisible == isVisible.Value).ToList();
        }

        var meetingDtos = _mapper.Map<IEnumerable<MeetingDto>>(meetings);
        return Ok(meetingDtos);
    }

    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeetingById(int id)
    {
        var adminUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            return Unauthorized("User is not authenticated.");
        }

        var meeting = await _meetingRepository.GetMeetingByIdAsync(id);
        if (meeting == null)
        {
            return NotFound();
        }

        var meetingDto = _mapper.Map<MeetingDetailsDto>(meeting);
        meetingDto.Notes = await _adminNoteService.GetNotesForTargetAsync(
            meeting.TypeOfMeeting != null && meeting.TypeOfMeeting.IsEvent
                ? AdminNoteTargetType.Event
                : AdminNoteTargetType.Meeting,
            meeting.Id.ToString(),
            adminUserId);

        return Ok(meetingDto);
    }

    [SwaggerIgnore]
    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMeeting([FromForm] CreateMeetingDto createMeetingDto, [FromForm] IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User?.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

      
            if (userRole == "Instructor")
            {
                createMeetingDto.InstructorId = userId; 
                createMeetingDto.IsGuestInstructor = false;
            }

            if (string.IsNullOrEmpty(createMeetingDto.InstructorId))
            {
                return BadRequest("InstructorId is required.");
            }

            var typeOfMeeting = await _typeOfMeetingRepository.GetTypeOfMeetingByIdAsync(createMeetingDto.TypeOfMeetingId);
            if (typeOfMeeting == null)
            {
                return NotFound("Invalid TypeOfMeetingId.");
            }

            

            var meeting = _mapper.Map<Meeting>(createMeetingDto);
            if (createMeetingDto.IsGuestInstructor)
            {
                if (!int.TryParse(createMeetingDto.InstructorId, out var parsedGuestInstructorId))
                {
                    return BadRequest("Guest instructor id must be a valid integer.");
                }

                meeting.InstructorId = null;
                meeting.GuestInstructorId = parsedGuestInstructorId;
            }
            else
            {
                meeting.GuestInstructorId = null;
            }
            meeting.Price = typeOfMeeting.Price;
         

            var createdMeeting = await _meetingRepository.CreateMeetingAsync(meeting, userId);
            var createdMeetingDto = _mapper.Map<MeetingDto>(createdMeeting);

            return CreatedAtAction(nameof(GetMeetingById), new { id = createdMeetingDto.Id }, createdMeetingDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [SwaggerIgnore]
    [Authorize]
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMeeting(int id, [FromForm] CreateMeetingDto updateMeetingDto, [FromForm] IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingMeeting = await _meetingRepository.GetMeetingByIdAsync(id);
            if (existingMeeting == null)
            {
                return NotFound($"Meeting with ID {id} not found.");
            }

            if (existingMeeting.TypeOfMeetingId != updateMeetingDto.TypeOfMeetingId)
            {
                var typeOfMeeting = await _typeOfMeetingRepository.GetTypeOfMeetingByIdAsync(updateMeetingDto.TypeOfMeetingId);
                if (typeOfMeeting == null)
                {
                    return NotFound("Invalid TypeOfMeetingId.");
                }
                existingMeeting.Price = typeOfMeeting.Price; 
            }

       
            _mapper.Map(updateMeetingDto, existingMeeting);

           
       

            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User?.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authorized.");
            }

            if (userRole == "Instructor")
            {
                updateMeetingDto.InstructorId = userId;
                updateMeetingDto.IsGuestInstructor = false;
            }

            if (updateMeetingDto.IsGuestInstructor)
            {
                if (!int.TryParse(updateMeetingDto.InstructorId, out var parsedGuestInstructorId))
                {
                    return BadRequest("Guest instructor id must be a valid integer.");
                }

                existingMeeting.InstructorId = null;
                existingMeeting.GuestInstructorId = parsedGuestInstructorId;
            }
            else
            {
                existingMeeting.InstructorId = updateMeetingDto.InstructorId;
                existingMeeting.GuestInstructorId = null;
            }

            var isUpdated = await _meetingRepository.UpdateMeetingAsync(existingMeeting, userId);
            if (!isUpdated)
            {
                return StatusCode(500, "Failed to update the meeting.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteMeeting(int id)
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authorized.");
        }

        var result = await _meetingRepository.SoftDeleteMeetingAsync(id, userId);
        if (!result)
        {
            return NotFound("Meeting not found.");
        }

        return NoContent();
    }

   
    
    
    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetMeetingParticipants(int id)
    {
        var meeting = await _meetingRepository.GetMeetingByIdAsync(id);
        if (meeting == null)
        {
            return NotFound($"Meeting with ID {id} not found.");
        }

        var participants = meeting.MeetingParticipants
            .Select(mp => new ParticipantDto
            {
                Id = mp.User.Id,
                IsGuest = false,
                Name = mp.User.Name,
                Surname = mp.User.Surname,
                Email = mp.User.Email,
                PhoneNumber = mp.User.PhoneNumber,
                HasPaid = mp.HasPaid,
                RegistrationStatus = ParticipantRegistrationStatus.Accepted.ToString(),
                CreatedFromPublicRequest = false,
                RequestedAt = null
            })
            .Concat(meeting.MeetingGuestParticipants.Select(mgp => new ParticipantDto
            {
                Id = mgp.GuestUser.Id.ToString(),
                IsGuest = true,
                Name = mgp.GuestUser.Name,
                Surname = mgp.GuestUser.Surname,
                Email = mgp.GuestUser.Email,
                PhoneNumber = mgp.GuestUser.PhoneNumber,
                HasPaid = mgp.HasPaid,
                RegistrationStatus = mgp.RegistrationStatus.ToString(),
                CreatedFromPublicRequest = mgp.CreatedFromPublicRequest,
                RequestedAt = mgp.RequestedAt
            }))
            .ToList();

        return Ok(participants);
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpGet("unpaid-participants")]
    public async Task<IActionResult> GetUnpaidParticipantsByMeeting()
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User?.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }

        IQueryable<Meeting> meetingsQuery = _context.Meetings
            .AsNoTracking()
            .Where(m => !m.IsDeleted);

        if (userRole == "Instructor")
        {
            meetingsQuery = meetingsQuery.Where(m => m.InstructorId == userId);
        }
        else if (userRole != "Admin")
        {
            return Forbid("Access denied.");
        }

        var meetings = await meetingsQuery
            .Where(m =>
                m.MeetingParticipants.Any(mp => !mp.HasPaid && !mp.User.IsDeleted) ||
                m.MeetingGuestParticipants.Any(mgp =>
                    !mgp.HasPaid &&
                    mgp.RegistrationStatus == ParticipantRegistrationStatus.Accepted &&
                    !mgp.GuestUser.IsDeleted))
            .Include(m => m.Location)
            .Include(m => m.TypeOfMeeting)
            .Include(m => m.Instructor)
            .Include(m => m.GuestInstructor)
            .Include(m => m.MeetingParticipants)
            .ThenInclude(mp => mp.User)
            .Include(m => m.MeetingGuestParticipants)
            .ThenInclude(mgp => mgp.GuestUser)
            .OrderBy(m => m.Date)
            .ToListAsync();

        var response = meetings
            .Select(meeting =>
            {
                var unpaidParticipants = meeting.MeetingParticipants
                    .Where(mp => !mp.HasPaid && !mp.User.IsDeleted)
                    .Select(mp => new ParticipantDto
                    {
                        Id = mp.User.Id,
                        IsGuest = false,
                        Name = mp.User.Name,
                        Surname = mp.User.Surname,
                        Email = mp.User.Email,
                        PhoneNumber = mp.User.PhoneNumber,
                        HasPaid = mp.HasPaid,
                        RegistrationStatus = ParticipantRegistrationStatus.Accepted.ToString(),
                        CreatedFromPublicRequest = false,
                        RequestedAt = null
                    })
                    .Concat(meeting.MeetingGuestParticipants
                        .Where(mgp =>
                            !mgp.HasPaid &&
                            mgp.RegistrationStatus == ParticipantRegistrationStatus.Accepted &&
                            !mgp.GuestUser.IsDeleted)
                        .Select(mgp => new ParticipantDto
                        {
                            Id = mgp.GuestUser.Id.ToString(),
                            IsGuest = true,
                            Name = mgp.GuestUser.Name,
                            Surname = mgp.GuestUser.Surname,
                            Email = mgp.GuestUser.Email,
                            PhoneNumber = mgp.GuestUser.PhoneNumber,
                            HasPaid = mgp.HasPaid,
                            RegistrationStatus = mgp.RegistrationStatus.ToString(),
                            CreatedFromPublicRequest = mgp.CreatedFromPublicRequest,
                            RequestedAt = mgp.RequestedAt
                        }))
                    .OrderBy(p => p.Surname)
                    .ThenBy(p => p.Name)
                    .ToList();

                return new UnpaidMeetingParticipantsDto
                {
                    Meeting = MapUnpaidMeetingSummary(meeting),
                    Participants = unpaidParticipants
                };
            })
            .Where(item => item.Participants.Count > 0)
            .ToList();

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending-guest-registrations")]
    public async Task<IActionResult> GetPendingGuestRegistrations()
    {
        var pendingRegistrations = await GetGuestRegistrationsByStatusAsync(ParticipantRegistrationStatus.Pending);

        return Ok(pendingRegistrations);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("rejected-guest-registrations")]
    public async Task<IActionResult> GetRejectedGuestRegistrations()
    {
        var rejectedRegistrations = await GetGuestRegistrationsByStatusAsync(ParticipantRegistrationStatus.Rejected);

        return Ok(rejectedRegistrations);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("accepted-guest-registrations")]
    public async Task<IActionResult> GetAcceptedGuestRegistrations()
    {
        var acceptedRegistrations = await GetGuestRegistrationsByStatusAsync(ParticipantRegistrationStatus.Accepted);

        return Ok(acceptedRegistrations);
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpGet("user-meetings/{userId}")]
    public async Task<IActionResult> GetMeetingsByUserId(string userId, [FromQuery] bool isGuest = false)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("UserId is required.");
        }

        IEnumerable<Meeting> meetings;
        int? guestUserId = null;

        if (isGuest)
        {
            if (!int.TryParse(userId, out var parsedGuestUserId))
            {
                return BadRequest("Guest user id must be a valid integer.");
            }

            guestUserId = parsedGuestUserId;
            meetings = await _meetingRepository.GetMeetingsByGuestUserIdAsync(parsedGuestUserId);
        }
        else
        {
            meetings = await _meetingRepository.GetMeetingsByUserIdAsync(userId);
        }

        var meetingList = meetings.ToList();
        var meetingDtos = _mapper.Map<List<MeetingDto>>(meetingList);

        foreach (var meetingDto in meetingDtos)
        {
            var meeting = meetingList.First(m => m.Id == meetingDto.Id);
            meetingDto.ParticipantHasPaid = isGuest
                ? meeting.MeetingGuestParticipants.FirstOrDefault(p => p.GuestUserId == guestUserId)?.HasPaid
                : meeting.MeetingParticipants.FirstOrDefault(p => p.UserId == userId)?.HasPaid;
        }

        return Ok(meetingDtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("user-meetings/{userId}/unpaid")]
    public async Task<IActionResult> GetUnpaidMeetingsByUserId(string userId, [FromQuery] bool isGuest = false)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("UserId is required.");
        }

        List<UnpaidMeetingSummaryDto> response;

        if (isGuest)
        {
            if (!int.TryParse(userId, out var guestUserId))
            {
                return BadRequest("Guest user id must be a valid integer.");
            }

            var meetings = await _context.Meetings
                .AsNoTracking()
                .Where(m => !m.IsDeleted)
                .Where(m => m.MeetingGuestParticipants.Any(mgp =>
                    mgp.GuestUserId == guestUserId &&
                    !mgp.HasPaid &&
                    mgp.RegistrationStatus == ParticipantRegistrationStatus.Accepted &&
                    !mgp.GuestUser.IsDeleted))
                .Include(m => m.Location)
                .Include(m => m.TypeOfMeeting)
                .Include(m => m.Instructor)
                .Include(m => m.GuestInstructor)
                .OrderBy(m => m.Date)
                .ToListAsync();

            response = meetings
                .Select(MapUnpaidMeetingSummary)
                .ToList();
        }
        else
        {
            var meetings = await _context.Meetings
                .AsNoTracking()
                .Where(m => !m.IsDeleted)
                .Where(m => m.MeetingParticipants.Any(mp =>
                    mp.UserId == userId &&
                    !mp.HasPaid &&
                    !mp.User.IsDeleted))
                .Include(m => m.Location)
                .Include(m => m.TypeOfMeeting)
                .Include(m => m.Instructor)
                .Include(m => m.GuestInstructor)
                .OrderBy(m => m.Date)
                .ToListAsync();

            response = meetings
                .Select(MapUnpaidMeetingSummary)
                .ToList();
        }

        return Ok(response);
    }

    private async Task<List<PendingGuestRegistrationDto>> GetGuestRegistrationsByStatusAsync(
        ParticipantRegistrationStatus status)
    {
        return await _context.MeetingGuestParticipants
            .Include(mgp => mgp.GuestUser)
            .ThenInclude(gu => gu.GuestUserProfile)
            .Include(mgp => mgp.GuestUser)
            .ThenInclude(gu => gu.GuestInstructorProfile)
            .Include(mgp => mgp.Meeting)
            .ThenInclude(m => m.TypeOfMeeting)
            .Include(mgp => mgp.Meeting)
            .ThenInclude(m => m.Location)
            .Where(mgp => mgp.RegistrationStatus == status)
            .Where(mgp => mgp.GuestUser != null &&
                          !mgp.GuestUser.IsDeleted &&
                          mgp.GuestUser.GuestUserProfile != null &&
                          mgp.GuestUser.GuestInstructorProfile == null)
            .OrderByDescending(mgp => mgp.RequestedAt ?? DateTime.MinValue)
            .ThenBy(mgp => mgp.Meeting.Date)
            .Select(mgp => new PendingGuestRegistrationDto
            {
                MeetingId = mgp.MeetingId,
                MeetingDate = mgp.Meeting.Date,
                TypeOfMeetingName = mgp.Meeting.TypeOfMeeting.Name,
                LocationName = mgp.Meeting.Location.Name,
                LocationCity = mgp.Meeting.Location.City,
                GuestUserId = mgp.GuestUserId,
                Name = mgp.GuestUser.Name,
                Surname = mgp.GuestUser.Surname,
                Email = mgp.GuestUser.Email,
                PhoneNumber = mgp.GuestUser.PhoneNumber,
                AllowNewsletter = mgp.GuestUser.GuestUserProfile != null && mgp.GuestUser.GuestUserProfile.AllowNewsletter,
                AllowSmsMarketing = mgp.GuestUser.GuestUserProfile != null && mgp.GuestUser.GuestUserProfile.AllowSmsMarketing,
                RequestedAt = mgp.RequestedAt,
                RegistrationStatus = mgp.RegistrationStatus.ToString()
            })
            .ToListAsync();
    }

    private static UnpaidMeetingSummaryDto MapUnpaidMeetingSummary(Meeting meeting)
    {
        return new UnpaidMeetingSummaryDto
        {
            Id = meeting.Id,
            Date = meeting.Date,
            Duration = meeting.Duration,
            TypeOfMeetingName = meeting.TypeOfMeeting.Name,
            LocationName = meeting.Location.Name,
            LocationCity = meeting.Location.City,
            InstructorName = meeting.Instructor != null
                ? $"{meeting.Instructor.Name} {meeting.Instructor.Surname}"
                : meeting.GuestInstructor != null
                    ? $"{meeting.GuestInstructor.Name} {meeting.GuestInstructor.Surname}"
                    : string.Empty,
            IsGuestInstructor = meeting.GuestInstructorId.HasValue,
            Price = meeting.Price
        };
    }
    
    
    [Authorize(Roles = "Admin,Instructor")]
    [HttpPost("add-participant")]
    public async Task<IActionResult> AddParticipantToMeeting([FromBody] AddParticipantDto addParticipantDto)
    {
        if (string.IsNullOrEmpty(addParticipantDto.UserId))
            return BadRequest("UserId is required.");

        var meeting = await _meetingRepository.GetMeetingByIdAsync(addParticipantDto.MeetingId);
        if (meeting == null)
            return NotFound($"Meeting with ID {addParticipantDto.MeetingId} not found.");

        var isAlreadyParticipant = meeting.MeetingParticipants
            .Any(mp => mp.UserId == addParticipantDto.UserId);
        if (isAlreadyParticipant)
            return BadRequest("User is already a participant.");

        var result = await _meetingRepository.AddParticipantToMeetingAsync(
            addParticipantDto.MeetingId,
            addParticipantDto.UserId
        );

        if (!result)
            return StatusCode(500, "Failed to add participant.");

        return NoContent();
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPost("add-participants-bulk")]
    public async Task<IActionResult> AddParticipantsBulk([FromBody] BulkParticipantsDto dto)
    {
        var meeting = await _meetingRepository.GetMeetingByIdAsync(dto.MeetingId);
        if (meeting == null)
            return NotFound($"Meeting with ID {dto.MeetingId} not found.");

        if (dto.Participants == null || dto.Participants.Count == 0)
            return BadRequest("At least one participant is required.");

        var response = await ProcessBulkParticipantsAsync(meeting, dto.Participants, "add");
        return Ok(response);
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPost("add-guest-participant")]
    public async Task<IActionResult> AddGuestParticipantToMeeting([FromBody] AddGuestParticipantDto addGuestParticipantDto)
    {
        if (addGuestParticipantDto.GuestUserId <= 0)
            return BadRequest("GuestUserId is required.");

        var meeting = await _meetingRepository.GetMeetingByIdAsync(addGuestParticipantDto.MeetingId);
        if (meeting == null)
            return NotFound($"Meeting with ID {addGuestParticipantDto.MeetingId} not found.");

        var isAlreadyParticipant = meeting.MeetingGuestParticipants
            .Any(mgp => mgp.GuestUserId == addGuestParticipantDto.GuestUserId);
        if (isAlreadyParticipant)
            return BadRequest("Guest user is already a participant.");

        var result = await _meetingRepository.AddGuestParticipantToMeetingAsync(
            addGuestParticipantDto.MeetingId,
            addGuestParticipantDto.GuestUserId
        );

        if (!result)
            return StatusCode(500, "Failed to add guest participant.");

        var participant = await _context.MeetingGuestParticipants
            .FirstOrDefaultAsync(mgp =>
                mgp.MeetingId == addGuestParticipantDto.MeetingId &&
                mgp.GuestUserId == addGuestParticipantDto.GuestUserId);

        if (participant != null)
        {
            await _auditLogService.WriteAsync(new AuditWriteRequest
            {
                TargetType = AuditLogTargetType.MeetingGuestParticipant,
                TargetId = participant.Id.ToString(),
                ActionType = AuditLogActionType.Created,
                SourceType = AuditLogSourceType.AdminPanel,
                Actor = AuditActorInfo.FromPrincipal(User),
                Changes = new AuditChangeSetBuilder()
                    .AddCreated("meetingId", participant.MeetingId)
                    .AddCreated("guestUserId", participant.GuestUserId)
                    .AddCreated("registrationStatus", participant.RegistrationStatus)
                    .AddCreated("createdFromPublicRequest", participant.CreatedFromPublicRequest)
                    .Build(),
                Reason = "Existing guest user added to meeting from admin panel."
            });
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPost("{meetingId}/create-guest-participant")]
    public async Task<IActionResult> CreateGuestUserAndAddToMeeting(
        int meetingId,
        [FromBody] GuestUserCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
        if (meeting == null)
            return NotFound($"Meeting with ID {meetingId} not found.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var guestUser = new GuestUser
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                IsDeleted = false,
                GuestUserProfile = new GuestUserProfile
                {
                    AllowNewsletter = dto.AllowNewsletter,
                    AllowSmsMarketing = dto.AllowSmsMarketing,
                    IsPendingApproval = false,
                    Bio = dto.Bio
                }
            };

            _context.GuestUsers.Add(guestUser);
            await _context.SaveChangesAsync();

            var added = await _meetingRepository.AddGuestParticipantToMeetingAsync(meetingId, guestUser.Id);
            if (!added)
            {
                await transaction.RollbackAsync();
                return BadRequest("Failed to add the newly created guest user to the meeting.");
            }

            await transaction.CommitAsync();

            var createdGuestUser = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .FirstAsync(g => g.Id == guestUser.Id);

            var createdParticipant = await _context.MeetingGuestParticipants
                .FirstOrDefaultAsync(mgp => mgp.MeetingId == meetingId && mgp.GuestUserId == guestUser.Id);

            await _auditLogService.WriteAsync(new AuditWriteRequest
            {
                TargetType = AuditLogTargetType.GuestUser,
                TargetId = createdGuestUser.Id.ToString(),
                ActionType = AuditLogActionType.Created,
                SourceType = AuditLogSourceType.AdminPanel,
                Actor = AuditActorInfo.FromPrincipal(User),
                Changes = new AuditChangeSetBuilder()
                    .AddCreated("name", createdGuestUser.Name)
                    .AddCreated("surname", createdGuestUser.Surname)
                    .AddCreated("email", createdGuestUser.Email)
                    .AddCreated("phoneNumber", createdGuestUser.PhoneNumber)
                    .AddCreated("allowNewsletter", createdGuestUser.GuestUserProfile?.AllowNewsletter)
                    .AddCreated("allowSmsMarketing", createdGuestUser.GuestUserProfile?.AllowSmsMarketing)
                    .Build(),
                Reason = "Guest user created and attached to meeting from admin panel."
            });

            if (createdParticipant != null)
            {
                await _auditLogService.WriteAsync(new AuditWriteRequest
                {
                    TargetType = AuditLogTargetType.MeetingGuestParticipant,
                    TargetId = createdParticipant.Id.ToString(),
                    ActionType = AuditLogActionType.Created,
                    SourceType = AuditLogSourceType.AdminPanel,
                    Actor = AuditActorInfo.FromPrincipal(User),
                    Changes = new AuditChangeSetBuilder()
                        .AddCreated("meetingId", createdParticipant.MeetingId)
                        .AddCreated("guestUserId", createdParticipant.GuestUserId)
                        .AddCreated("registrationStatus", createdParticipant.RegistrationStatus)
                        .AddCreated("createdFromPublicRequest", createdParticipant.CreatedFromPublicRequest)
                        .Build(),
                    Reason = "Guest participant created from admin panel."
                });
            }

            return Ok(new
            {
                MeetingId = meetingId,
                GuestUser = _mapper.Map<GuestUserDto>(createdGuestUser)
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    [Authorize(Roles = "Admin,Instructor")]
    [HttpPost("remove-participant")]
    public async Task<IActionResult> RemoveParticipantFromMeeting([FromBody] RemoveParticipantDto removeParticipantDto)
    {
        if (string.IsNullOrEmpty(removeParticipantDto.UserId))
        {
            return BadRequest("UserId is required.");
        }

        var meeting = await _meetingRepository.GetMeetingByIdAsync(removeParticipantDto.MeetingId);
        if (meeting == null)
        {
            return NotFound($"Meeting with ID {removeParticipantDto.MeetingId} not found.");
        }

        var participant = meeting.MeetingParticipants
            .FirstOrDefault(mp => mp.UserId == removeParticipantDto.UserId);

        if (participant == null)
        {
            return NotFound("User is not a participant of this meeting.");
        }

        var result = await _meetingRepository.RemoveParticipantFromMeetingAsync(
            removeParticipantDto.MeetingId,
            removeParticipantDto.UserId
        );

        if (!result)
        {
            return StatusCode(500, "Failed to remove participant.");
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPost("remove-participants-bulk")]
    public async Task<IActionResult> RemoveParticipantsBulk([FromBody] BulkParticipantsDto dto)
    {
        var meeting = await _meetingRepository.GetMeetingByIdAsync(dto.MeetingId);
        if (meeting == null)
            return NotFound($"Meeting with ID {dto.MeetingId} not found.");

        if (dto.Participants == null || dto.Participants.Count == 0)
            return BadRequest("At least one participant is required.");

        var response = await ProcessBulkParticipantsAsync(meeting, dto.Participants, "remove");
        return Ok(response);
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPut("{meetingId}/participants/payment-status")]
    public async Task<IActionResult> UpdateParticipantPaymentStatus(
        int meetingId,
        [FromBody] UpdateParticipantPaymentStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
            return BadRequest("Participant id is required.");

        if (dto.IsGuest)
        {
            if (!int.TryParse(dto.Id, out var guestUserId))
                return BadRequest("Guest participant id must be a valid integer.");

            var participant = await _context.MeetingGuestParticipants
                .FirstOrDefaultAsync(mgp => mgp.MeetingId == meetingId && mgp.GuestUserId == guestUserId);

            if (participant == null)
                return NotFound("Guest participant not found for this meeting.");

            participant.HasPaid = dto.HasPaid;
        }
        else
        {
            var participant = await _context.MeetingParticipants
                .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId && mp.UserId == dto.Id);

            if (participant == null)
                return NotFound("Participant not found for this meeting.");

            participant.HasPaid = dto.HasPaid;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPut("{meetingId}/participants/registration-status")]
    public async Task<IActionResult> UpdateParticipantRegistrationStatus(
        int meetingId,
        [FromBody] UpdateParticipantRegistrationStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
            return BadRequest("Participant id is required.");

        if (!dto.IsGuest)
            return BadRequest("Registration status can only be updated for guest participants.");

        if (!int.TryParse(dto.Id, out var guestUserId))
            return BadRequest("Guest participant id must be a valid integer.");

        if (!Enum.TryParse<ParticipantRegistrationStatus>(dto.Status, true, out var status))
            return BadRequest("Status must be Pending, Accepted, or Rejected.");

        var participant = await _context.MeetingGuestParticipants
            .Include(mgp => mgp.GuestUser)
            .ThenInclude(gu => gu.GuestUserProfile)
            .FirstOrDefaultAsync(mgp => mgp.MeetingId == meetingId && mgp.GuestUserId == guestUserId);

        if (participant == null)
            return NotFound("Guest participant not found for this meeting.");

        var previousStatus = participant.RegistrationStatus;
        var previousPendingApproval = participant.GuestUser.GuestUserProfile?.IsPendingApproval;
        participant.RegistrationStatus = status;

        if (status == ParticipantRegistrationStatus.Accepted && participant.GuestUser.GuestUserProfile != null)
        {
            participant.GuestUser.GuestUserProfile.IsPendingApproval = false;
        }

        await _context.SaveChangesAsync();

        var auditChanges = new AuditChangeSetBuilder()
            .Add("registrationStatus", previousStatus, participant.RegistrationStatus)
            .Add("isPendingApproval", previousPendingApproval, participant.GuestUser.GuestUserProfile?.IsPendingApproval);

        await _auditLogService.WriteAsync(new AuditWriteRequest
        {
            TargetType = AuditLogTargetType.MeetingGuestParticipant,
            TargetId = participant.Id.ToString(),
            ActionType = status switch
            {
                ParticipantRegistrationStatus.Accepted => AuditLogActionType.Approved,
                ParticipantRegistrationStatus.Rejected => AuditLogActionType.Rejected,
                _ => AuditLogActionType.StatusChanged
            },
            SourceType = AuditLogSourceType.AdminPanel,
            Actor = AuditActorInfo.FromPrincipal(User),
            Changes = auditChanges.Build(),
            Reason = "Guest participant registration status updated from admin panel."
        });

        var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
        if (meeting == null)
            return NotFound($"Meeting with ID {meetingId} not found.");

        bool emailSent = status switch
        {
            ParticipantRegistrationStatus.Accepted =>
                await _guestRegistrationNotificationService.SendAcceptedNotificationAsync(participant.GuestUser, meeting),
            ParticipantRegistrationStatus.Rejected =>
                await _guestRegistrationNotificationService.SendRejectedNotificationAsync(participant.GuestUser, meeting),
            _ => true
        };

        if (!emailSent && status != ParticipantRegistrationStatus.Pending)
        {
            _logger.LogWarning(
                "Participant status updated but notification email was not sent. GuestUserId: {GuestUserId}, MeetingId: {MeetingId}, Status: {Status}",
                participant.GuestUserId,
                meetingId,
                status);
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPost("remove-guest-participant")]
    public async Task<IActionResult> RemoveGuestParticipantFromMeeting([FromBody] RemoveGuestParticipantDto removeGuestParticipantDto)
    {
        if (removeGuestParticipantDto.GuestUserId <= 0)
        {
            return BadRequest("GuestUserId is required.");
        }

        var meeting = await _meetingRepository.GetMeetingByIdAsync(removeGuestParticipantDto.MeetingId);
        if (meeting == null)
        {
            return NotFound($"Meeting with ID {removeGuestParticipantDto.MeetingId} not found.");
        }

        var participant = meeting.MeetingGuestParticipants
            .FirstOrDefault(mgp => mgp.GuestUserId == removeGuestParticipantDto.GuestUserId);

        if (participant == null)
        {
            return NotFound("Guest user is not a participant of this meeting.");
        }

        var result = await _meetingRepository.RemoveGuestParticipantFromMeetingAsync(
            removeGuestParticipantDto.MeetingId,
            removeGuestParticipantDto.GuestUserId
        );

        if (!result)
        {
            return StatusCode(500, "Failed to remove guest participant.");
        }

        return NoContent();
    }

    private async Task<BulkParticipantOperationResponseDto> ProcessBulkParticipantsAsync(
        Meeting meeting,
        List<BulkParticipantItemDto> participants,
        string action)
    {
        var response = new BulkParticipantOperationResponseDto
        {
            MeetingId = meeting.Id,
            Action = action
        };

        var regularIds = participants
            .Where(p => !p.IsGuest && !string.IsNullOrWhiteSpace(p.Id))
            .Select(p => p.Id)
            .Distinct()
            .ToList();

        var parsedGuestIds = new Dictionary<string, int>();
        foreach (var participant in participants.Where(p => p.IsGuest && !string.IsNullOrWhiteSpace(p.Id)))
        {
            if (!parsedGuestIds.ContainsKey(participant.Id) && int.TryParse(participant.Id, out var guestId))
            {
                parsedGuestIds[participant.Id] = guestId;
            }
        }

        var existingRegularUsers = await _context.Users
            .Where(u => regularIds.Contains(u.Id) && !u.IsDeleted)
            .Select(u => u.Id)
            .ToHashSetAsync();

        var existingGuestUsers = await _context.GuestUsers
            .Where(g => parsedGuestIds.Values.Contains(g.Id) && !g.IsDeleted)
            .Select(g => g.Id)
            .ToHashSetAsync();

        var regularParticipantIds = meeting.MeetingParticipants
            .Select(mp => mp.UserId)
            .ToHashSet();

        var guestParticipantIds = meeting.MeetingGuestParticipants
            .Select(mgp => mgp.GuestUserId)
            .ToHashSet();

        foreach (var participant in participants)
        {
            if (string.IsNullOrWhiteSpace(participant.Id))
            {
                response.Results.Add(new BulkParticipantOperationItemDto
                {
                    Id = participant.Id,
                    IsGuest = participant.IsGuest,
                    Status = "failed",
                    Message = "Participant id is required."
                });
                response.FailedCount++;
                continue;
            }

            if (participant.IsGuest)
            {
                if (!parsedGuestIds.TryGetValue(participant.Id, out var guestId))
                {
                    response.Results.Add(new BulkParticipantOperationItemDto
                    {
                        Id = participant.Id,
                        IsGuest = true,
                        Status = "failed",
                        Message = "Guest participant id must be a valid integer."
                    });
                    response.FailedCount++;
                    continue;
                }

                if (!existingGuestUsers.Contains(guestId))
                {
                    response.Results.Add(new BulkParticipantOperationItemDto
                    {
                        Id = participant.Id,
                        IsGuest = true,
                        Status = "failed",
                        Message = "Guest user not found."
                    });
                    response.FailedCount++;
                    continue;
                }

                if (action == "add")
                {
                    if (guestParticipantIds.Contains(guestId))
                    {
                        response.Results.Add(new BulkParticipantOperationItemDto
                        {
                            Id = participant.Id,
                            IsGuest = true,
                            Status = "skipped",
                            Message = "Guest user is already a participant."
                        });
                        response.SkippedCount++;
                        continue;
                    }

                    var entity = new MeetingGuestParticipant
                    {
                        MeetingId = meeting.Id,
                        GuestUserId = guestId
                    };

                    _context.MeetingGuestParticipants.Add(entity);
                    meeting.MeetingGuestParticipants.Add(entity);
                    guestParticipantIds.Add(guestId);
                }
                else
                {
                    var existingParticipant = meeting.MeetingGuestParticipants
                        .FirstOrDefault(mgp => mgp.GuestUserId == guestId);

                    if (existingParticipant == null)
                    {
                        response.Results.Add(new BulkParticipantOperationItemDto
                        {
                            Id = participant.Id,
                            IsGuest = true,
                            Status = "skipped",
                            Message = "Guest user is not a participant."
                        });
                        response.SkippedCount++;
                        continue;
                    }

                    _context.MeetingGuestParticipants.Remove(existingParticipant);
                    meeting.MeetingGuestParticipants.Remove(existingParticipant);
                    guestParticipantIds.Remove(guestId);
                }
            }
            else
            {
                if (!existingRegularUsers.Contains(participant.Id))
                {
                    response.Results.Add(new BulkParticipantOperationItemDto
                    {
                        Id = participant.Id,
                        IsGuest = false,
                        Status = "failed",
                        Message = "User not found."
                    });
                    response.FailedCount++;
                    continue;
                }

                if (action == "add")
                {
                    if (regularParticipantIds.Contains(participant.Id))
                    {
                        response.Results.Add(new BulkParticipantOperationItemDto
                        {
                            Id = participant.Id,
                            IsGuest = false,
                            Status = "skipped",
                            Message = "User is already a participant."
                        });
                        response.SkippedCount++;
                        continue;
                    }

                    var entity = new MeetingParticipant
                    {
                        MeetingId = meeting.Id,
                        UserId = participant.Id
                    };

                    _context.MeetingParticipants.Add(entity);
                    meeting.MeetingParticipants.Add(entity);
                    regularParticipantIds.Add(participant.Id);
                }
                else
                {
                    var existingParticipant = meeting.MeetingParticipants
                        .FirstOrDefault(mp => mp.UserId == participant.Id);

                    if (existingParticipant == null)
                    {
                        response.Results.Add(new BulkParticipantOperationItemDto
                        {
                            Id = participant.Id,
                            IsGuest = false,
                            Status = "skipped",
                            Message = "User is not a participant."
                        });
                        response.SkippedCount++;
                        continue;
                    }

                    _context.MeetingParticipants.Remove(existingParticipant);
                    meeting.MeetingParticipants.Remove(existingParticipant);
                    regularParticipantIds.Remove(participant.Id);
                }
            }

            response.Results.Add(new BulkParticipantOperationItemDto
            {
                Id = participant.Id,
                IsGuest = participant.IsGuest,
                Status = "success",
                Message = action == "add" ? "Participant processed successfully." : "Participant removed successfully."
            });
            response.SucceededCount++;
        }

        if (response.SucceededCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        return response;
    }
 
    [Authorize(Roles = "Admin")]
    [HttpPost("copy-month")]
    public async Task<IActionResult> CopyMonth([FromBody] CopyMonthDto dto)
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _meetingRepository.CopyMeetingsMonthAsync(
            dto.SourceYear,
            dto.SourceMonth,
            dto.TargetYear,
            dto.TargetMonth,
            userId
        );

        return Ok(new { copied = result });
    }
    
 

    [Authorize(Roles = "Admin")]
    [HttpPost("copy-week")]
    public async Task<IActionResult> CopyWeek([FromBody] CopyWeekDto dto)
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (dto.SourceYear == dto.TargetYear && dto.SourceWeek == dto.TargetWeek)
            return BadRequest("Source and target week cannot be the same.");

        var copied = await _meetingRepository.CopyWeekAsync(
            dto.SourceYear, dto.SourceWeek, dto.TargetYear, dto.TargetWeek, userId
        );

        return Ok(new { copied });
    }
    
    
}
