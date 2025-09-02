using System.Security.Claims;
using System.Linq; // <-- needed for Where/Any
using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Controllers;

[AllowAnonymous]
[Route("api/Public/[controller]")]
[ApiController]
public class PublicMeetingController : ControllerBase
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMapper _mapper;

    public PublicMeetingController(IMeetingRepository meetingRepository, IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicMeetings()
    {
        var meetings = await _meetingRepository.GetAllMeetingsAsync();

        var publicMeetings = meetings
            .Where(m =>
                // only visible meetings
                m.IsVisible &&
                // instructor must exist and not be soft-deleted
                m.Instructor != null && !m.Instructor.IsDeleted &&
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
            meeting.Instructor == null ||
            meeting.Instructor.IsDeleted)
        {
            return NotFound();
        }

        var meetingDto = _mapper.Map<MeetingDto>(meeting);
        return Ok(meetingDto);
    }

    [Authorize]
    [HttpPost("add-participant")]
    public async Task<IActionResult> AddParticipantToMeeting([FromBody] AddParticipantDto addParticipantDto)
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var meeting = await _meetingRepository.GetMeetingByIdAsync(addParticipantDto.MeetingId);
        if (meeting == null ||
            meeting.Instructor == null ||
            meeting.Instructor.IsDeleted)
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

        // keep only meetings with active (non-deleted) instructor
        var filtered = meetings
            .Where(m => m.Instructor != null && !m.Instructor.IsDeleted)
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

        // keep only meetings with active (non-deleted) instructor
        var filtered = upcomingMeetings
            .Where(m => m.Instructor != null && !m.Instructor.IsDeleted)
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

        // visible, future, instructor active, and not "Indywidualne spotkanie | wolne miejsce"
        var baseQuery = all
            .Where(m => m.IsVisible &&
                        m.Date >= nowUtc &&
                        m.Instructor != null &&
                        !m.Instructor.IsDeleted &&
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
    
}