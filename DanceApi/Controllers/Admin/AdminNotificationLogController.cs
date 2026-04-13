using System.Linq.Expressions;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Controllers.Admin;

[Route("api/Admin/NotificationLogs")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminNotificationLogController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminNotificationLogController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationLogs(
        [FromQuery] int days = 30,
        [FromQuery] string? recipient = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? kind = null,
        [FromQuery] string? status = null)
    {
        if (days <= 0)
            return BadRequest("Days must be greater than 0.");

        var fromUtc = DateTime.UtcNow.AddDays(-days);
        var query = BuildNotificationLogsQuery()
            .Where(log => log.RequestedAtUtc >= fromUtc);

        if (!string.IsNullOrWhiteSpace(recipient))
        {
            var normalizedRecipient = recipient.Trim().ToLowerInvariant();
            query = query.Where(log => log.Recipient.ToLower() == normalizedRecipient);
        }

        if (!string.IsNullOrWhiteSpace(channel))
        {
            if (!Enum.TryParse<NotificationChannel>(channel, true, out var parsedChannel))
                return BadRequest("Channel must be Email or Sms.");

            query = query.Where(log => log.Channel == parsedChannel);
        }

        if (!string.IsNullOrWhiteSpace(kind))
        {
            if (!Enum.TryParse<NotificationKind>(kind, true, out var parsedKind))
                return BadRequest("Invalid notification kind.");

            query = query.Where(log => log.Kind == parsedKind);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<NotificationStatus>(status, true, out var parsedStatus))
                return BadRequest("Status must be Pending, AcceptedByProvider, Failed, or Skipped.");

            query = query.Where(log => log.Status == parsedStatus);
        }

        var logs = await query
            .OrderByDescending(log => log.RequestedAtUtc)
            .Select(MapListItem())
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetNotificationLogById(int id)
    {
        var log = await BuildNotificationLogsQuery()
            .Where(entry => entry.Id == id)
            .Select(MapDetails())
            .FirstOrDefaultAsync();

        if (log == null)
            return NotFound($"Notification log with ID {id} not found.");

        return Ok(log);
    }

    private IQueryable<NotificationLog> BuildNotificationLogsQuery()
    {
        return _context.NotificationLogs
            .AsNoTracking()
            .Include(log => log.GuestUser)
            .Include(log => log.Meeting)
            .ThenInclude(meeting => meeting.TypeOfMeeting);
    }

    private static Expression<Func<NotificationLog, NotificationLogListItemDto>> MapListItem()
    {
        return log => new NotificationLogListItemDto
        {
            Id = log.Id,
            Channel = log.Channel.ToString(),
            Kind = log.Kind.ToString(),
            Recipient = log.Recipient,
            Subject = log.Subject,
            Provider = log.Provider,
            ProviderOperationId = log.ProviderOperationId,
            Status = log.Status.ToString(),
            ErrorMessage = log.ErrorMessage,
            RequestedAtUtc = log.RequestedAtUtc,
            LastUpdatedAtUtc = log.LastUpdatedAtUtc,
            GuestUserId = log.GuestUserId,
            GuestUserName = log.GuestUser != null ? log.GuestUser.Name + " " + log.GuestUser.Surname : null,
            MeetingId = log.MeetingId,
            MeetingName = log.Meeting != null ? log.Meeting.TypeOfMeeting.Name : null,
            MeetingDateUtc = log.Meeting != null ? log.Meeting.Date : null
        };
    }

    private static Expression<Func<NotificationLog, NotificationLogDetailsDto>> MapDetails()
    {
        return log => new NotificationLogDetailsDto
        {
            Id = log.Id,
            Channel = log.Channel.ToString(),
            Kind = log.Kind.ToString(),
            Recipient = log.Recipient,
            Subject = log.Subject,
            Provider = log.Provider,
            ProviderOperationId = log.ProviderOperationId,
            Status = log.Status.ToString(),
            ErrorMessage = log.ErrorMessage,
            RequestedAtUtc = log.RequestedAtUtc,
            LastUpdatedAtUtc = log.LastUpdatedAtUtc,
            GuestUserId = log.GuestUserId,
            GuestUserName = log.GuestUser != null ? log.GuestUser.Name + " " + log.GuestUser.Surname : null,
            MeetingId = log.MeetingId,
            MeetingName = log.Meeting != null ? log.Meeting.TypeOfMeeting.Name : null,
            MeetingDateUtc = log.Meeting != null ? log.Meeting.Date : null,
            PlainTextContent = log.PlainTextContent,
            HtmlContent = log.HtmlContent
        };
    }
}
