using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DanceApi.Controllers.Admin;


   [Authorize]
[Route("api/Admin/Events")]
[ApiController]
public class AdminEventController : BaseController
{
    private readonly ITypeOfMeetingRepository _typeRepo;
    private readonly IMeetingRepository _meetingRepo;
    private readonly IMapper _mapper;


    public AdminEventController(
        ITypeOfMeetingRepository typeRepo,
        IMeetingRepository meetingRepo,
        IMapper mapper)
    {
        _typeRepo = typeRepo;
        _meetingRepo = meetingRepo;
        _mapper = mapper;
    }

    // LIST ALL EVENTS
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var meetings = await _meetingRepo.GetAllMeetingsAsync();

        var events = meetings
            .Where(m =>
                m.TypeOfMeeting != null &&
                m.TypeOfMeeting.IsEvent)
            .OrderByDescending(m => m.Date)
            .ToList();

        var dto = _mapper.Map<IEnumerable<MeetingDto>>(events);
        return Ok(dto);
    }
    
    
    // =========================
    // CREATE EVENT
    // =========================
    [SwaggerIgnore]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateEvent(
        [FromForm] EventDto.CreateEventDto dto,
        [FromForm] IFormFile? image)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // save image (same helper as TypeOfMeeting)
        var imageUrl = image != null
            ? await FileHelper.SaveImageAsync(image)
            : null;

        // 1️⃣ create TypeOfMeeting (event)
        var type = new TypeOfMeeting
        {
            Name = dto.Name,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            Price = dto.Price,
            ImageUrl = imageUrl,
            IsIndividual = false,
            IsSolo = false,
            IsHighlighted = false,
            IsVisible = dto.IsVisible,
            IsEvent = true,
            CreatedById = userId,
            CreatedDate = DateTime.UtcNow
        };

        var createdType = await _typeRepo.CreateTypeOfMeetingAsync(type, userId);

        // 2️⃣ create Meeting (termin)
        var meeting = new Meeting
        {
            Date = dto.Date,
            Duration = dto.Duration,
            LocationId = dto.LocationId,
            InstructorId = dto.InstructorId,
            TypeOfMeetingId = createdType.Id,
            Price = dto.Price, // snapshot
            Level = dto.Level,
            IsHighlighted = false,
            IsVisible = dto.IsVisible
        };

        var createdMeeting = await _meetingRepo.CreateMeetingAsync(meeting, userId);

        return Ok(new
        {
            EventTypeId = createdType.Id,
            MeetingId = createdMeeting.Id
        });
    }

    // =========================
    // UPDATE EVENT
    // =========================
    [SwaggerIgnore]
    [HttpPut("{meetingId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateEvent(
        int meetingId,
        [FromForm] EventDto.UpdateEventDto dto,
        [FromForm] IFormFile? image)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var meeting = await _meetingRepo.GetMeetingByIdAsync(meetingId);
        if (meeting == null || meeting.TypeOfMeeting == null || !meeting.TypeOfMeeting.IsEvent)
            return NotFound("Event not found.");

        var type = meeting.TypeOfMeeting;

        // update TYPE
        type.Name = dto.Name;
        type.Description = dto.Description;
        type.ShortDescription = dto.ShortDescription;
        type.Price = dto.Price;
        type.IsHighlighted = false;
        type.IsVisible = dto.IsVisible;

        if (image != null)
        {
            FileHelper.DeleteImage(type.ImageUrl);
            type.ImageUrl = await FileHelper.SaveImageAsync(image);
        }

        await _typeRepo.UpdateTypeOfMeetingAsync(type, userId);

        // update MEETING
        meeting.Date = dto.Date;
        meeting.Duration = dto.Duration;
        meeting.LocationId = dto.LocationId;
        meeting.InstructorId = dto.InstructorId;
        meeting.Level = dto.Level;
        meeting.Price = dto.Price;

        await _meetingRepo.UpdateMeetingAsync(meeting, userId);

        return NoContent();
    }
    
    [Authorize]
    [HttpDelete("{meetingId}")]
    public async Task<IActionResult> SoftDeleteEvent(int meetingId)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authorized.");

        var result = await _meetingRepo.SoftDeleteEventAsync(meetingId, userId);
        if (!result)
            return NotFound("Event not found or is not an event.");

        return NoContent();
    }
    
}