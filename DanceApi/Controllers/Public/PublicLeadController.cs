using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Helper;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Controllers;

[Route("api/Public/Leads")]
[ApiController]
public class PublicLeadController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public PublicLeadController(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLead([FromBody] LeadCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(dto.Surname))
            return BadRequest("Surname is required.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");

        if (string.IsNullOrWhiteSpace(dto.GroupName))
            return BadRequest("Group is required.");

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        var lead = new Lead
        {
            Name = dto.Name.Trim(),
            Surname = dto.Surname.Trim(),
            Email = normalizedEmail,
            NormalizedEmail = normalizedEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
            WantsEmailInformation = dto.WantsEmailInformation,
            AllowsNewsletterAndSmsMarketing = dto.AllowsNewsletterAndSmsMarketing,
            GroupName = dto.GroupName.Trim(),
            AdditionalMessage = string.IsNullOrWhiteSpace(dto.AdditionalMessage) ? null : dto.AdditionalMessage.Trim(),
            Status = LeadStatus.New,
            SubmittedAtUtc = now,
            ConsentCapturedAtUtc = now,
            CreatedDate = now
        };

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        await _auditLogService.WriteAsync(new AuditWriteRequest
        {
            TargetType = AuditLogTargetType.Lead,
            TargetId = lead.Id.ToString(),
            ActionType = AuditLogActionType.Created,
            SourceType = AuditLogSourceType.PublicRequest,
            Actor = AuditActorInfo.PublicRequest(normalizedEmail),
            Changes = new AuditChangeSetBuilder()
                .AddCreated("name", lead.Name)
                .AddCreated("surname", lead.Surname)
                .AddCreated("email", lead.Email)
                .AddCreated("phoneNumber", lead.PhoneNumber)
                .AddCreated("wantsEmailInformation", lead.WantsEmailInformation)
                .AddCreated("allowsNewsletterAndSmsMarketing", lead.AllowsNewsletterAndSmsMarketing)
                .AddCreated("groupName", lead.GroupName)
                .AddCreated("additionalMessage", lead.AdditionalMessage)
                .AddCreated("status", lead.Status)
                .Build(),
            Reason = "Lead submitted from public form."
        });

        return Ok(new
        {
            message = "Lead submitted successfully.",
            leadId = lead.Id
        });
    }
}
