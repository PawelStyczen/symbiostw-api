using AutoMapper;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Helper;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace DanceApi.Controllers.Admin
{
    [Route("api/Admin/Guest")]
    [ApiController]
    [Authorize(Roles = "Instructor,Admin")]
    public class AdminGuestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAdminNoteService _adminNoteService;
        private readonly IAuditLogService _auditLogService;

        public AdminGuestController(
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

        [SwaggerIgnore]
        [HttpPost("Instructors")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateGuestInstructor(
            [FromForm] GuestInstructorCreateDto dto,
            [FromForm] IFormFile? image)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string imageUrl;

            if (image != null)
            {
                var fileName = $"{Guid.NewGuid()}_{image.FileName}";
                var filePath = Path.Combine("wwwroot/uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream);

                imageUrl = $"/uploads/{fileName}";
            }
            else
            {
                imageUrl = "/uploads/e756ec82-c3cb-435a-8398-d778a0609092_User-Account-Person-PNG-File-1026727248.png";
            }

            var guestUser = new GuestUser
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                IsDeleted = false,
                GuestInstructorProfile = new GuestInstructorProfile
                {
                    Bio = dto.Bio ?? "New guest instructor profile",
                    ExperienceYears = dto.ExperienceYears,
                    Specialization = dto.Specialization,
                    FacebookLink = dto.FacebookLink,
                    InstagramLink = dto.InstagramLink,
                    TikTokLink = dto.TikTokLink,
                    ImageUrl = imageUrl,
                    ShowOnPublicInstructorsPage = dto.ShowOnPublicInstructorsPage
                }
            };

            _context.GuestUsers.Add(guestUser);
            await _context.SaveChangesAsync();

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
                    .AddCreated("guestInstructor", true)
                    .AddCreated("specialization", guestUser.GuestInstructorProfile?.Specialization)
                    .Build(),
                Reason = "Guest instructor created from admin panel."
            });

            return Ok("Guest instructor created successfully.");
        }

        [HttpGet("Instructors")]
        public async Task<IActionResult> GetAllGuestInstructors()
        {
            var guestInstructors = await _context.GuestUsers
                .Include(g => g.GuestInstructorProfile)
                .Where(g => !g.IsDeleted && g.GuestInstructorProfile != null)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<GuestInstructorDto>>(guestInstructors);
            return Ok(dto);
        }

        [HttpGet("Instructors/{id}")]
        public async Task<IActionResult> GetGuestInstructorById(int id)
        {
            var guestInstructor = await _context.GuestUsers
                .Include(g => g.GuestInstructorProfile)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guestInstructor == null || guestInstructor.IsDeleted || guestInstructor.GuestInstructorProfile == null)
                return NotFound($"Guest instructor with ID {id} not found.");

            var dto = _mapper.Map<GuestInstructorDto>(guestInstructor);
            return Ok(dto);
        }

        [SwaggerIgnore]
        [HttpPut("Instructors/{id}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateGuestInstructor(
            int id,
            [FromForm] GuestInstructorUpdateDto dto,
            [FromForm] IFormFile? image)
        {
            var guestInstructor = await _context.GuestUsers
                .Include(g => g.GuestInstructorProfile)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guestInstructor == null || guestInstructor.IsDeleted || guestInstructor.GuestInstructorProfile == null)
                return NotFound($"Guest instructor with ID {id} not found.");

            var changes = new AuditChangeSetBuilder()
                .Add("name", guestInstructor.Name, dto.Name)
                .Add("surname", guestInstructor.Surname, dto.Surname)
                .Add("email", guestInstructor.Email, dto.Email)
                .Add("phoneNumber", guestInstructor.PhoneNumber, string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim())
                .Add("bio", guestInstructor.GuestInstructorProfile.Bio, dto.Bio)
                .Add("experienceYears", guestInstructor.GuestInstructorProfile.ExperienceYears, dto.ExperienceYears)
                .Add("specialization", guestInstructor.GuestInstructorProfile.Specialization, dto.Specialization)
                .Add("facebookLink", guestInstructor.GuestInstructorProfile.FacebookLink, dto.FacebookLink)
                .Add("instagramLink", guestInstructor.GuestInstructorProfile.InstagramLink, dto.InstagramLink)
                .Add("tiktokLink", guestInstructor.GuestInstructorProfile.TikTokLink, dto.TikTokLink)
                .Add("showOnPublicInstructorsPage", guestInstructor.GuestInstructorProfile.ShowOnPublicInstructorsPage, dto.ShowOnPublicInstructorsPage);

            guestInstructor.Name = dto.Name;
            guestInstructor.Surname = dto.Surname;
            guestInstructor.Email = dto.Email;
            guestInstructor.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

            guestInstructor.GuestInstructorProfile.Bio = dto.Bio;
            guestInstructor.GuestInstructorProfile.ExperienceYears = dto.ExperienceYears;
            guestInstructor.GuestInstructorProfile.Specialization = dto.Specialization;
            guestInstructor.GuestInstructorProfile.FacebookLink = dto.FacebookLink;
            guestInstructor.GuestInstructorProfile.InstagramLink = dto.InstagramLink;
            guestInstructor.GuestInstructorProfile.TikTokLink = dto.TikTokLink;
            guestInstructor.GuestInstructorProfile.ShowOnPublicInstructorsPage = dto.ShowOnPublicInstructorsPage;

            if (image != null)
            {
                if (!string.IsNullOrWhiteSpace(guestInstructor.GuestInstructorProfile.ImageUrl))
                {
                    var oldPath = Path.Combine("wwwroot", guestInstructor.GuestInstructorProfile.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        try { System.IO.File.Delete(oldPath); } catch { }
                    }
                }

                var fileName = $"{Guid.NewGuid()}_{image.FileName}";
                var filePath = Path.Combine("wwwroot/uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream);

                guestInstructor.GuestInstructorProfile.ImageUrl = $"/uploads/{fileName}";
            }
            else if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                guestInstructor.GuestInstructorProfile.ImageUrl = dto.ImageUrl;
            }

            await _context.SaveChangesAsync();

            await _auditLogService.WriteAsync(new AuditWriteRequest
            {
                TargetType = AuditLogTargetType.GuestUser,
                TargetId = guestInstructor.Id.ToString(),
                ActionType = AuditLogActionType.Updated,
                SourceType = AuditLogSourceType.AdminPanel,
                Actor = AuditActorInfo.FromPrincipal(User),
                Changes = changes.Build(),
                Reason = "Guest instructor updated from admin panel."
            });

            return NoContent();
        }

        [HttpDelete("Instructors/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGuestInstructor(int id)
        {
            var guestInstructor = await _context.GuestUsers
                .Include(g => g.GuestInstructorProfile)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guestInstructor == null)
                return NotFound($"Guest instructor with ID {id} not found.");

            guestInstructor.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok($"Guest instructor {guestInstructor.Name} {guestInstructor.Surname} soft deleted successfully.");
        }

        [HttpPost("Users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateGuestUser([FromBody] GuestUserCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                Reason = "Guest user created from admin panel."
            });

            var result = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .FirstAsync(g => g.Id == guestUser.Id);

            return Ok(_mapper.Map<GuestUserDto>(result));
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetAllGuestUsers()
        {
            var guestUsers = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .Include(g => g.GuestInstructorProfile)
                .Where(g => !g.IsDeleted)
                .Where(g => g.GuestUserProfile != null &&
                            g.GuestInstructorProfile == null &&
                            !g.GuestUserProfile.IsPendingApproval)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<GuestUserDto>>(guestUsers);
            return Ok(dto);
        }

        [HttpGet("Users/Pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingGuestUsers()
        {
            var pendingGuestUsers = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .Include(g => g.GuestInstructorProfile)
                .Where(g => !g.IsDeleted)
                .Where(g => g.GuestUserProfile != null &&
                            g.GuestInstructorProfile == null &&
                            g.GuestUserProfile.IsPendingApproval)
                .OrderBy(g => g.Surname)
                .ThenBy(g => g.Name)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<GuestUserDto>>(pendingGuestUsers);
            return Ok(dto);
        }

        [HttpGet("Users/{id}")]
        public async Task<IActionResult> GetGuestUserById(int id)
        {
            var adminUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminUserId))
                return Unauthorized("User is not authorized.");

            var guestUser = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .Include(g => g.GuestInstructorProfile)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guestUser == null || guestUser.IsDeleted || guestUser.GuestUserProfile == null || guestUser.GuestInstructorProfile != null)
                return NotFound($"Guest user with ID {id} not found.");

            var dto = _mapper.Map<GuestUserDetailsDto>(guestUser);
            dto.Notes = await _adminNoteService.GetNotesForTargetAsync(
                AdminNoteTargetType.User,
                guestUser.Id.ToString(),
                adminUserId);

            return Ok(dto);
        }

        [HttpPut("Users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGuestUser(int id, [FromBody] GuestUserUpdateDto dto)
        {
            var guestUser = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .Include(g => g.GuestInstructorProfile)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guestUser == null || guestUser.IsDeleted || guestUser.GuestInstructorProfile != null)
                return NotFound($"Guest user with ID {id} not found.");

            var changes = new AuditChangeSetBuilder()
                .Add("name", guestUser.Name, dto.Name)
                .Add("surname", guestUser.Surname, dto.Surname)
                .Add("email", guestUser.Email, dto.Email)
                .Add("phoneNumber", guestUser.PhoneNumber, string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim())
                .Add("allowNewsletter", guestUser.GuestUserProfile?.AllowNewsletter, dto.AllowNewsletter)
                .Add("allowSmsMarketing", guestUser.GuestUserProfile?.AllowSmsMarketing, dto.AllowSmsMarketing)
                .Add("bio", guestUser.GuestUserProfile?.Bio, dto.Bio);

            guestUser.Name = dto.Name;
            guestUser.Surname = dto.Surname;
            guestUser.Email = dto.Email;
            guestUser.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

            if (guestUser.GuestUserProfile == null)
            {
                guestUser.GuestUserProfile = new GuestUserProfile
                {
                    GuestUserId = guestUser.Id,
                    IsPendingApproval = false
                };
            }

            guestUser.GuestUserProfile.AllowNewsletter = dto.AllowNewsletter;
            guestUser.GuestUserProfile.AllowSmsMarketing = dto.AllowSmsMarketing;
            guestUser.GuestUserProfile.Bio = dto.Bio;

            await _context.SaveChangesAsync();

            await _auditLogService.WriteAsync(new AuditWriteRequest
            {
                TargetType = AuditLogTargetType.GuestUser,
                TargetId = guestUser.Id.ToString(),
                ActionType = AuditLogActionType.Updated,
                SourceType = AuditLogSourceType.AdminPanel,
                Actor = AuditActorInfo.FromPrincipal(User),
                Changes = changes.Build(),
                Reason = "Guest user updated from admin panel."
            });

            return NoContent();
        }

        [HttpPut("Users/{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveGuestUser(int id)
        {
            var guestUser = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .Include(g => g.GuestInstructorProfile)
                .Include(g => g.MeetingGuestParticipants)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guestUser == null || guestUser.IsDeleted || guestUser.GuestUserProfile == null || guestUser.GuestInstructorProfile != null)
                return NotFound($"Guest user with ID {id} not found.");

            var previousPendingApproval = guestUser.GuestUserProfile.IsPendingApproval;
            var promotedParticipants = guestUser.MeetingGuestParticipants
                .Where(p => p.RegistrationStatus == ParticipantRegistrationStatus.Pending)
                .Select(p => new
                {
                    ParticipantId = p.Id,
                    PreviousStatus = p.RegistrationStatus
                })
                .ToList();

            guestUser.GuestUserProfile.IsPendingApproval = false;

            foreach (var participant in guestUser.MeetingGuestParticipants
                         .Where(p => p.RegistrationStatus == ParticipantRegistrationStatus.Pending))
            {
                participant.RegistrationStatus = ParticipantRegistrationStatus.Accepted;
            }

            await _context.SaveChangesAsync();

            await _auditLogService.WriteAsync(new AuditWriteRequest
            {
                TargetType = AuditLogTargetType.GuestUser,
                TargetId = guestUser.Id.ToString(),
                ActionType = AuditLogActionType.Approved,
                SourceType = AuditLogSourceType.AdminPanel,
                Actor = AuditActorInfo.FromPrincipal(User),
                Changes = new AuditChangeSetBuilder()
                    .Add("isPendingApproval", previousPendingApproval, guestUser.GuestUserProfile.IsPendingApproval)
                    .Build(),
                Reason = "Guest user approved from admin panel."
            });

            foreach (var promotedParticipant in promotedParticipants)
            {
                await _auditLogService.WriteAsync(new AuditWriteRequest
                {
                    TargetType = AuditLogTargetType.MeetingGuestParticipant,
                    TargetId = promotedParticipant.ParticipantId.ToString(),
                    ActionType = AuditLogActionType.Approved,
                    SourceType = AuditLogSourceType.AdminPanel,
                    Actor = AuditActorInfo.FromPrincipal(User),
                    Changes = new AuditChangeSetBuilder()
                        .Add("registrationStatus", promotedParticipant.PreviousStatus, ParticipantRegistrationStatus.Accepted)
                        .Build(),
                    Reason = "Guest participant approved together with guest user approval."
                });
            }

            return NoContent();
        }

        [HttpDelete("Users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGuestUser(int id)
        {
            var guestUser = await _context.GuestUsers
                .Include(g => g.GuestUserProfile)
                .Include(g => g.GuestInstructorProfile)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guestUser == null || guestUser.GuestInstructorProfile != null)
                return NotFound($"Guest user with ID {id} not found.");

            guestUser.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok($"Guest user {guestUser.Name} {guestUser.Surname} soft deleted successfully.");
        }
    }
}
