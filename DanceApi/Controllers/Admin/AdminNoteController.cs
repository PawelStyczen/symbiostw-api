using AutoMapper;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Controllers.Admin;

[Route("api/Admin/Notes")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminNoteController : BaseController
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AdminNoteController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotes(
        [FromQuery] AdminNoteTargetType targetType,
        [FromQuery] string targetId)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!TryNormalizeTargetId(targetType, targetId, out var normalizedTargetId, out var error))
        {
            return BadRequest(error);
        }

        var notes = await _context.AdminNotes
            .Include(note => note.CreatedBy)
            .Include(note => note.UpdatedBy)
            .Where(note =>
                note.CreatedById == userId &&
                note.TargetType == targetType &&
                note.TargetId == normalizedTargetId)
            .OrderByDescending(note => note.UpdatedDate ?? note.CreatedDate)
            .ToListAsync();

        return Ok(_mapper.Map<List<AdminNoteReadDto>>(notes));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetNoteById(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var note = await _context.AdminNotes
            .Include(item => item.CreatedBy)
            .Include(item => item.UpdatedBy)
            .FirstOrDefaultAsync(item => item.Id == id && item.CreatedById == userId);

        if (note == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AdminNoteReadDto>(note));
    }

    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] AdminNoteCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!TryNormalizeTargetId(dto.TargetType, dto.TargetId, out var normalizedTargetId, out var error))
        {
            return BadRequest(error);
        }

        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return BadRequest("Content is required.");
        }

        if (!await TargetExistsAsync(dto.TargetType, normalizedTargetId))
        {
            return NotFound("Target object not found.");
        }

        var note = new AdminNote
        {
            TargetType = dto.TargetType,
            TargetId = normalizedTargetId,
            Content = dto.Content.Trim(),
            CreatedById = userId,
            CreatedDate = DateTime.UtcNow
        };

        _context.AdminNotes.Add(note);
        await _context.SaveChangesAsync();
        await _context.Entry(note).Reference(item => item.CreatedBy).LoadAsync();

        var result = _mapper.Map<AdminNoteReadDto>(note);
        return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] AdminNoteUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var note = await _context.AdminNotes
            .Include(item => item.CreatedBy)
            .Include(item => item.UpdatedBy)
            .FirstOrDefaultAsync(item => item.Id == id && item.CreatedById == userId);

        if (note == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return BadRequest("Content is required.");
        }

        note.Content = dto.Content.Trim();
        note.UpdatedById = userId;
        note.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<AdminNoteReadDto>(note));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var note = await _context.AdminNotes
            .FirstOrDefaultAsync(item => item.Id == id && item.CreatedById == userId);

        if (note == null)
        {
            return NotFound();
        }

        note.IsDeleted = true;
        note.DeletedById = userId;
        note.DeletedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TryNormalizeTargetId(
        AdminNoteTargetType targetType,
        string targetId,
        out string normalizedTargetId,
        out string? error)
    {
        normalizedTargetId = targetId.Trim();
        error = null;

        if (!Enum.IsDefined(targetType))
        {
            error = "Unsupported target type.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(normalizedTargetId))
        {
            error = "TargetId is required.";
            return false;
        }

        if (targetType == AdminNoteTargetType.Meeting || targetType == AdminNoteTargetType.Event)
        {
            if (!int.TryParse(normalizedTargetId, out var parsedId))
            {
                error = "TargetId must be a valid integer for meetings and events.";
                return false;
            }

            normalizedTargetId = parsedId.ToString();
        }
        else if (targetType == AdminNoteTargetType.User && int.TryParse(normalizedTargetId, out var parsedGuestUserId))
        {
            normalizedTargetId = parsedGuestUserId.ToString();
        }

        return true;
    }

    private async Task<bool> TargetExistsAsync(AdminNoteTargetType targetType, string normalizedTargetId)
    {
        return targetType switch
        {
            AdminNoteTargetType.User => await UserExistsAsync(normalizedTargetId),
            AdminNoteTargetType.Meeting => await MeetingExistsAsync(normalizedTargetId, requireEvent: false),
            AdminNoteTargetType.Event => await MeetingExistsAsync(normalizedTargetId, requireEvent: true),
            _ => false
        };
    }

    private async Task<bool> UserExistsAsync(string targetId)
    {
        if (await _context.Users.AnyAsync(user => user.Id == targetId && !user.IsDeleted))
        {
            return true;
        }

        return int.TryParse(targetId, out var guestUserId) &&
               await _context.GuestUsers.AnyAsync(user => user.Id == guestUserId && !user.IsDeleted);
    }

    private async Task<bool> MeetingExistsAsync(string targetId, bool requireEvent)
    {
        if (!int.TryParse(targetId, out var meetingId))
        {
            return false;
        }

        return await _context.Meetings
            .AnyAsync(meeting =>
                meeting.Id == meetingId &&
                (!requireEvent || (meeting.TypeOfMeeting != null && meeting.TypeOfMeeting.IsEvent)));
    }
}
