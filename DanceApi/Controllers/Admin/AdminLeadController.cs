using AutoMapper;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Helper;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Controllers.Admin;

[Route("api/Admin/Leads")]
[ApiController]
[Authorize(Roles = "Instructor,Admin")]
public class AdminLeadController : BaseController
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAdminNoteService _adminNoteService;
    private readonly IAuditLogService _auditLogService;

    public AdminLeadController(
        AppDbContext context,
        IMapper mapper,
        IAdminNoteService adminNoteService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _mapper = mapper;
        _adminNoteService = adminNoteService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeads(
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string? group = null,
        [FromQuery] bool includeDeleted = false)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (includeDeleted && !User.IsInRole("Admin"))
            return Forbid();

        IQueryable<Lead> query = includeDeleted
            ? _context.Leads.IgnoreQueryFilters()
            : _context.Leads;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!TryParseLeadStatus(status, out var parsedStatus))
                return BadRequest(BuildUnsupportedStatusMessage());

            query = query.Where(lead => lead.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(group))
        {
            var trimmedGroup = group.Trim();
            query = query.Where(lead => lead.GroupName.Contains(trimmedGroup));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(lead =>
                lead.NormalizedEmail.Contains(normalizedSearch) ||
                lead.Name.ToLower().Contains(normalizedSearch) ||
                lead.Surname.ToLower().Contains(normalizedSearch) ||
                (lead.PhoneNumber != null && lead.PhoneNumber.Contains(search.Trim())));
        }

        var leads = await query
            .AsNoTracking()
            .Include(lead => lead.CreatedBy)
            .Include(lead => lead.UpdatedBy)
            .Include(lead => lead.ConvertedGuestUser)
            .OrderByDescending(lead => lead.CreatedDate)
            .ToListAsync();

        var dto = _mapper.Map<List<LeadReadDto>>(leads);
        await AttachLatestNotesAsync(dto, userId);

        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLeadById(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var lead = await GetLeadDetailsQuery()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (lead == null)
            return NotFound("Lead not found.");

        var dto = _mapper.Map<LeadDetailsDto>(lead);
        dto.Notes = await _adminNoteService.GetNotesForTargetAsync(
            AdminNoteTargetType.Lead,
            lead.Id.ToString(),
            userId);
        dto.LatestNote = dto.Notes.FirstOrDefault();

        return Ok(dto);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateLeadStatus(int id, [FromBody] LeadStatusUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!TryParseLeadStatus(dto.Status, out var newStatus))
            return BadRequest(BuildUnsupportedStatusMessage());

        if (newStatus == LeadStatus.Converted)
            return BadRequest("Use the convert-to-guest-user endpoint to mark a lead as converted.");

        var lead = await _context.Leads.FirstOrDefaultAsync(item => item.Id == id);
        if (lead == null)
            return NotFound("Lead not found.");

        var previousStatus = lead.Status;
        var now = DateTime.UtcNow;

        lead.Status = newStatus;
        lead.UpdatedById = userId;
        lead.UpdatedDate = now;

        if (!string.IsNullOrWhiteSpace(dto.Note))
        {
            _context.AdminNotes.Add(new AdminNote
            {
                TargetType = AdminNoteTargetType.Lead,
                TargetId = lead.Id.ToString(),
                Content = dto.Note.Trim(),
                CreatedById = userId,
                CreatedDate = now
            });
        }

        await _context.SaveChangesAsync();

        await _auditLogService.WriteAsync(new AuditWriteRequest
        {
            TargetType = AuditLogTargetType.Lead,
            TargetId = lead.Id.ToString(),
            ActionType = AuditLogActionType.StatusChanged,
            SourceType = AuditLogSourceType.AdminPanel,
            Actor = AuditActorInfo.FromPrincipal(User),
            Changes = new AuditChangeSetBuilder()
                .Add("status", previousStatus, lead.Status)
                .Build(),
            Reason = "Lead status updated from admin panel."
        });

        return await GetLeadDetailsResultAsync(lead.Id, userId);
    }

    [HttpPost("{id:int}/convert-to-guest-user")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConvertLeadToGuestUser(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var lead = await _context.Leads
            .Include(item => item.ConvertedGuestUser)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (lead == null)
            return NotFound("Lead not found.");

        if (lead.Status == LeadStatus.Converted && lead.ConvertedGuestUserId.HasValue)
        {
            var convertedGuestUser = await _context.GuestUsers
                .Include(guestUser => guestUser.GuestUserProfile)
                .FirstOrDefaultAsync(guestUser => guestUser.Id == lead.ConvertedGuestUserId.Value);

            if (convertedGuestUser != null)
            {
                return Ok(new LeadConversionResultDto
                {
                    Lead = _mapper.Map<LeadReadDto>(lead),
                    GuestUser = _mapper.Map<GuestUserDto>(convertedGuestUser),
                    CreatedGuestUser = false
                });
            }
        }

        var matchingGuestUser = await _context.GuestUsers
            .Include(guestUser => guestUser.GuestUserProfile)
            .Include(guestUser => guestUser.GuestInstructorProfile)
            .FirstOrDefaultAsync(guestUser =>
                !guestUser.IsDeleted &&
                guestUser.Email != null &&
                guestUser.Email.ToLower() == lead.NormalizedEmail);

        if (matchingGuestUser?.GuestInstructorProfile != null)
            return Conflict("A guest instructor already uses this email address.");

        var now = DateTime.UtcNow;
        var createdGuestUser = false;
        var allowNewsletter = lead.AllowsEmailMarketing || lead.AllowsNewsletterAndSmsMarketing;
        var allowSmsMarketing = lead.AllowsNewsletterAndSmsMarketing;
        var guestUserChanges = new AuditChangeSetBuilder();
        var previousLeadStatus = lead.Status;
        var previousConvertedGuestUserId = lead.ConvertedGuestUserId;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        GuestUser guestUser;

        if (matchingGuestUser == null)
        {
            guestUser = new GuestUser
            {
                Name = lead.Name,
                Surname = lead.Surname,
                Email = lead.NormalizedEmail,
                PhoneNumber = lead.PhoneNumber,
                IsDeleted = false,
                GuestUserProfile = new GuestUserProfile
                {
                    AllowNewsletter = allowNewsletter,
                    AllowSmsMarketing = allowSmsMarketing,
                    IsPendingApproval = false,
                    Bio = BuildGuestUserBio(lead)
                }
            };

            _context.GuestUsers.Add(guestUser);
            createdGuestUser = true;
        }
        else
        {
            guestUser = matchingGuestUser;

            guestUserChanges
                .Add("name", guestUser.Name, lead.Name)
                .Add("surname", guestUser.Surname, lead.Surname)
                .Add("phoneNumber", guestUser.PhoneNumber, lead.PhoneNumber)
                .Add("allowNewsletter", guestUser.GuestUserProfile?.AllowNewsletter, allowNewsletter)
                .Add("allowSmsMarketing", guestUser.GuestUserProfile?.AllowSmsMarketing, allowSmsMarketing);

            guestUser.Name = lead.Name;
            guestUser.Surname = lead.Surname;
            guestUser.PhoneNumber = lead.PhoneNumber;

            if (guestUser.GuestUserProfile == null)
            {
                guestUser.GuestUserProfile = new GuestUserProfile
                {
                    GuestUserId = guestUser.Id,
                    IsPendingApproval = false
                };
            }

            guestUser.GuestUserProfile.AllowNewsletter = allowNewsletter;
            guestUser.GuestUserProfile.AllowSmsMarketing = allowSmsMarketing;

            if (string.IsNullOrWhiteSpace(guestUser.GuestUserProfile.Bio))
                guestUser.GuestUserProfile.Bio = BuildGuestUserBio(lead);
        }

        lead.Status = LeadStatus.Converted;
        lead.ConvertedGuestUser = guestUser;
        lead.UpdatedById = userId;
        lead.UpdatedDate = now;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await WriteLeadConversionAuditAsync(
            lead,
            previousLeadStatus,
            previousConvertedGuestUserId,
            guestUser.Id);

        await WriteGuestUserAuditAsync(
            guestUser,
            createdGuestUser,
            guestUserChanges);

        var convertedLead = await GetLeadDetailsQuery()
            .AsNoTracking()
            .FirstAsync(item => item.Id == lead.Id);

        var convertedGuest = await _context.GuestUsers
            .AsNoTracking()
            .Include(item => item.GuestUserProfile)
            .FirstAsync(item => item.Id == guestUser.Id);

        return Ok(new LeadConversionResultDto
        {
            Lead = _mapper.Map<LeadReadDto>(convertedLead),
            GuestUser = _mapper.Map<GuestUserDto>(convertedGuest),
            CreatedGuestUser = createdGuestUser
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLead(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var lead = await _context.Leads.FirstOrDefaultAsync(item => item.Id == id);
        if (lead == null)
            return NotFound("Lead not found.");

        lead.IsDeleted = true;
        lead.DeletedById = userId;
        lead.DeletedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.WriteAsync(new AuditWriteRequest
        {
            TargetType = AuditLogTargetType.Lead,
            TargetId = lead.Id.ToString(),
            ActionType = AuditLogActionType.Updated,
            SourceType = AuditLogSourceType.AdminPanel,
            Actor = AuditActorInfo.FromPrincipal(User),
            Changes = new AuditChangeSetBuilder()
                .Add("isDeleted", false, true)
                .Build(),
            Reason = "Lead soft deleted from admin panel."
        });

        return NoContent();
    }

    private IQueryable<Lead> GetLeadDetailsQuery()
    {
        return _context.Leads
            .Include(lead => lead.CreatedBy)
            .Include(lead => lead.UpdatedBy)
            .Include(lead => lead.ConvertedGuestUser);
    }

    private async Task AttachLatestNotesAsync(List<LeadReadDto> leads, string userId)
    {
        if (leads.Count == 0)
            return;

        var leadIds = leads
            .Select(lead => lead.Id.ToString())
            .ToList();

        var latestNotes = await _context.AdminNotes
            .AsNoTracking()
            .Include(note => note.CreatedBy)
            .Include(note => note.UpdatedBy)
            .Where(note =>
                note.CreatedById == userId &&
                note.TargetType == AdminNoteTargetType.Lead &&
                leadIds.Contains(note.TargetId))
            .OrderByDescending(note => note.UpdatedDate ?? note.CreatedDate)
            .ToListAsync();

        var latestNotesByLeadId = latestNotes
            .GroupBy(note => note.TargetId)
            .ToDictionary(
                group => group.Key,
                group => _mapper.Map<AdminNoteReadDto>(group.First()));

        foreach (var lead in leads)
        {
            latestNotesByLeadId.TryGetValue(lead.Id.ToString(), out var latestNote);
            lead.LatestNote = latestNote;
        }
    }

    private async Task<IActionResult> GetLeadDetailsResultAsync(int leadId, string userId)
    {
        var lead = await GetLeadDetailsQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == leadId);

        if (lead == null)
            return NotFound("Lead not found.");

        var dto = _mapper.Map<LeadDetailsDto>(lead);
        dto.Notes = await _adminNoteService.GetNotesForTargetAsync(
            AdminNoteTargetType.Lead,
            lead.Id.ToString(),
            userId);
        dto.LatestNote = dto.Notes.FirstOrDefault();

        return Ok(dto);
    }

    private async Task WriteLeadConversionAuditAsync(
        Lead lead,
        LeadStatus previousStatus,
        int? previousConvertedGuestUserId,
        int convertedGuestUserId)
    {
        await _auditLogService.WriteAsync(new AuditWriteRequest
        {
            TargetType = AuditLogTargetType.Lead,
            TargetId = lead.Id.ToString(),
            ActionType = AuditLogActionType.StatusChanged,
            SourceType = AuditLogSourceType.AdminPanel,
            Actor = AuditActorInfo.FromPrincipal(User),
            Changes = new AuditChangeSetBuilder()
                .Add("status", previousStatus, lead.Status)
                .Add("convertedGuestUserId", previousConvertedGuestUserId, convertedGuestUserId)
                .Build(),
            Reason = "Lead converted to guest user from admin panel."
        });
    }

    private async Task WriteGuestUserAuditAsync(
        GuestUser guestUser,
        bool createdGuestUser,
        AuditChangeSetBuilder guestUserChanges)
    {
        if (createdGuestUser)
        {
            await _auditLogService.WriteAsync(new AuditWriteRequest
            {
                TargetType = AuditLogTargetType.GuestUser,
                TargetId = guestUser.Id.ToString(),
                ActionType = AuditLogActionType.Created,
                SourceType = AuditLogSourceType.AdminPanel,
                Actor = AuditActorInfo.FromPrincipal(User),
                Changes = new AuditChangeSetBuilder()
                    .AddCreated("name", guestUser.Name)
                    .AddCreated("surname", guestUser.Surname)
                    .AddCreated("email", guestUser.Email)
                    .AddCreated("phoneNumber", guestUser.PhoneNumber)
                    .AddCreated("allowNewsletter", guestUser.GuestUserProfile?.AllowNewsletter)
                    .AddCreated("allowSmsMarketing", guestUser.GuestUserProfile?.AllowSmsMarketing)
                    .Build(),
                Reason = "Guest user created from lead conversion."
            });

            return;
        }

        if (guestUserChanges.Count == 0)
            return;

        await _auditLogService.WriteAsync(new AuditWriteRequest
        {
            TargetType = AuditLogTargetType.GuestUser,
            TargetId = guestUser.Id.ToString(),
            ActionType = AuditLogActionType.Updated,
            SourceType = AuditLogSourceType.AdminPanel,
            Actor = AuditActorInfo.FromPrincipal(User),
            Changes = guestUserChanges.Build(),
            Reason = "Guest user refreshed from lead conversion."
        });
    }

    private static bool TryParseLeadStatus(string? status, out LeadStatus parsedStatus)
    {
        parsedStatus = LeadStatus.New;

        return !string.IsNullOrWhiteSpace(status) &&
               Enum.TryParse(status.Trim(), ignoreCase: true, out parsedStatus) &&
               Enum.IsDefined(parsedStatus);
    }

    private static string BuildUnsupportedStatusMessage()
    {
        var statuses = string.Join(", ", Enum.GetNames<LeadStatus>());
        return $"Unsupported lead status. Allowed values: {statuses}.";
    }

    private static string? BuildGuestUserBio(Lead lead)
    {
        if (string.IsNullOrWhiteSpace(lead.AdditionalMessage))
            return $"Lead group: {lead.GroupName}";

        return $"Lead group: {lead.GroupName}\n\n{lead.AdditionalMessage}";
    }
}
