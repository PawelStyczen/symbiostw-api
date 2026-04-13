using System.Text.Json;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Controllers.Admin;

[Route("api/Admin/AuditLogs")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminAuditLogController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminAuditLogController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int days = 30,
        [FromQuery] string? targetType = null,
        [FromQuery] string? targetId = null,
        [FromQuery] string? actionType = null,
        [FromQuery] string? sourceType = null)
    {
        if (days <= 0)
            return BadRequest("Days must be greater than 0.");

        var fromUtc = DateTime.UtcNow.AddDays(-days);
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(log => log.ChangedAtUtc >= fromUtc);

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            if (!Enum.TryParse<AuditLogTargetType>(targetType, true, out var parsedTargetType))
                return BadRequest("Invalid target type.");

            query = query.Where(log => log.TargetType == parsedTargetType);
        }

        if (!string.IsNullOrWhiteSpace(targetId))
            query = query.Where(log => log.TargetId == targetId.Trim());

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            if (!Enum.TryParse<AuditLogActionType>(actionType, true, out var parsedActionType))
                return BadRequest("Invalid action type.");

            query = query.Where(log => log.ActionType == parsedActionType);
        }

        if (!string.IsNullOrWhiteSpace(sourceType))
        {
            if (!Enum.TryParse<AuditLogSourceType>(sourceType, true, out var parsedSourceType))
                return BadRequest("Invalid source type.");

            query = query.Where(log => log.SourceType == parsedSourceType);
        }

        var logs = await query
            .OrderByDescending(log => log.ChangedAtUtc)
            .ThenByDescending(log => log.Version)
            .Select(log => new AuditLogListItemDto
            {
                Id = log.Id,
                TargetType = log.TargetType.ToString(),
                TargetId = log.TargetId,
                Version = log.Version,
                ActionType = log.ActionType.ToString(),
                SourceType = log.SourceType.ToString(),
                ActorUserId = log.ActorUserId,
                ActorDisplayName = log.ActorDisplayName,
                ActorIdentifier = log.ActorIdentifier,
                ChangedAtUtc = log.ChangedAtUtc,
                Reason = log.Reason
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetAuditLogById(long id)
    {
        var log = await _context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.Id == id);

        if (log == null)
            return NotFound($"Audit log with ID {id} not found.");

        Dictionary<string, AuditFieldChangeDto> changes;

        try
        {
            changes = JsonSerializer.Deserialize<Dictionary<string, AuditFieldChangeDto>>(log.ChangesJson) ?? new();
        }
        catch
        {
            changes = new Dictionary<string, AuditFieldChangeDto>();
        }

        return Ok(new AuditLogDetailsDto
        {
            Id = log.Id,
            TargetType = log.TargetType.ToString(),
            TargetId = log.TargetId,
            Version = log.Version,
            ActionType = log.ActionType.ToString(),
            SourceType = log.SourceType.ToString(),
            ActorUserId = log.ActorUserId,
            ActorDisplayName = log.ActorDisplayName,
            ActorIdentifier = log.ActorIdentifier,
            ChangedAtUtc = log.ChangedAtUtc,
            Reason = log.Reason,
            Changes = changes
        });
    }
}
