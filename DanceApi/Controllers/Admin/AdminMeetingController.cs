using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
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

    public AdminMeetingController(IMeetingRepository meetingRepository,  ITypeOfMeetingRepository typeOfMeetingRepository,  IMapper mapper)
    {
        _meetingRepository = meetingRepository;
        _mapper = mapper;
        _typeOfMeetingRepository = typeOfMeetingRepository;
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
        var meeting = await _meetingRepository.GetMeetingByIdAsync(id);
        if (meeting == null)
        {
            return NotFound();
        }

        var meetingDto = _mapper.Map<MeetingDto>(meeting);
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
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authorized.");
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

        var participants = meeting.MeetingParticipants.Select(mp => new
        {
            mp.User.Id,
            mp.User.Name,
            mp.User.Surname,
            mp.User.Email
        });

        return Ok(participants);
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpGet("user-meetings/{userId}")]
    public async Task<IActionResult> GetMeetingsByUserId(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("UserId is required.");
        }

        var meetings = await _meetingRepository.GetMeetingsByUserIdAsync(userId);
        var meetingDtos = _mapper.Map<IEnumerable<MeetingDto>>(meetings);

        return Ok(meetingDtos);
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