using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DanceApi.Dto;
using DanceApi.Helper;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Controllers
{
    [Route("api/User/Account")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class UserAccountController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public UserAccountController(
            IUserRepository userRepository,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IAuditLogService auditLogService)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        // POST: api/User/Account/RegisterUser
        [HttpPost("RegisterUser")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim()
            };

            var result = await _userRepository.CreateUserAsync(user, model.Password);
            if (result.Succeeded)
            {
                _userRepository.AddUserProfile(new UserProfile
                {
                    UserId = user.Id,
                    AllowNewsletter = model.AllowNewsletter,
                    AllowSmsMarketing = model.AllowSmsMarketing,
                    AboutMe = string.Empty
                });
                await _userRepository.SaveChangesAsync();

                var roleAssignmentResult = await _userRepository.AddUserToRoleAsync(user, "User");
                if (roleAssignmentResult)
                {
                    var changes = new AuditChangeSetBuilder()
                        .AddCreated("name", user.Name)
                        .AddCreated("surname", user.Surname)
                        .AddCreated("email", user.Email)
                        .AddCreated("phoneNumber", user.PhoneNumber)
                        .AddCreated("allowNewsletter", model.AllowNewsletter)
                        .AddCreated("allowSmsMarketing", model.AllowSmsMarketing)
                        .Build();

                    await _auditLogService.WriteAsync(new AuditWriteRequest
                    {
                        TargetType = AuditLogTargetType.User,
                        TargetId = user.Id,
                        ActionType = AuditLogActionType.Created,
                        SourceType = AuditLogSourceType.PublicRequest,
                        Actor = AuditActorInfo.PublicRequest(user.Email),
                        Changes = changes,
                        Reason = "Regular user registered via public user account endpoint."
                    });

                    return Ok("User registered successfully.");
                }

                return BadRequest("User registered, but failed to assign the User role.");
            }

            return BadRequest(result.Errors.Select(e => e.Description));
        }

        // POST: api/User/Account/LoginUser
        [HttpPost("LoginUser")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _signInManager.SignOutAsync();

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid login attempt.");

            // ❗ Block soft-deleted accounts
            if (user.IsDeleted)
                return Unauthorized("Account is deactivated.");

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Double-check (defense in depth)
                if (user.IsDeleted)
                    return Unauthorized("Account is deactivated.");

                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            if (result.IsLockedOut)
                return BadRequest("User account is locked out.");

            if (result.IsNotAllowed)
                return Unauthorized("Login not allowed. Please confirm your email or contact support.");

            return Unauthorized("Invalid login attempt.");
        }

        // POST: api/User/Account/LogoutUser
        [HttpPost("LogoutUser")]
        [Authorize]
        public async Task<IActionResult> LogoutUser()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok("User logged out successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error during logout: {ex.Message}");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey12345678901234567890!"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "http://localhost",
                audience: "http://localhost",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // GET: api/User/Account/Users
        [HttpGet("Users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetUsersByRoleAsync("User");

                // ❗ Hide soft-deleted users
                var activeUsers = users?.Where(u => !u.IsDeleted).ToList() ?? new List<User>();

                if (!activeUsers.Any())
                    return NotFound("No users found.");

                var result = activeUsers.Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Surname,
                    u.Email,
                    u.PhoneNumber,
                    AllowNewsletter = u.UserProfile != null ? u.UserProfile.AllowNewsletter : true,
                    AllowSmsMarketing = u.UserProfile != null && u.UserProfile.AllowSmsMarketing,
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("CurrentUser")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            var user = await _userManager.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.IsDeleted) // ❗ Block soft-deleted
                return NotFound("User not found.");

            var result = new
            {
                user.Email,
                user.PhoneNumber,
                user.Name,
                user.Surname,
                AllowNewsletter = user.UserProfile?.AllowNewsletter ?? true,
                AllowSmsMarketing = user.UserProfile?.AllowSmsMarketing ?? false,
            };

            return Ok(result);
        }

        [HttpPut("CurrentUser")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserDto.UpdateUserDto updatedUser)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.IsDeleted) // ❗ Block soft-deleted
                return NotFound("User not found.");

            var changes = new AuditChangeSetBuilder()
                .Add("name", user.Name, updatedUser.Name ?? user.Name)
                .Add("surname", user.Surname, updatedUser.Surname ?? user.Surname)
                .Add("phoneNumber", user.PhoneNumber, updatedUser.PhoneNumber != null
                    ? string.IsNullOrWhiteSpace(updatedUser.PhoneNumber) ? null : updatedUser.PhoneNumber.Trim()
                    : user.PhoneNumber)
                .Add("allowNewsletter", user.UserProfile?.AllowNewsletter, updatedUser.AllowNewsletter ?? user.UserProfile?.AllowNewsletter)
                .Add("allowSmsMarketing", user.UserProfile?.AllowSmsMarketing, updatedUser.AllowSmsMarketing ?? user.UserProfile?.AllowSmsMarketing);

            if (!string.IsNullOrWhiteSpace(updatedUser.Name))
                user.Name = updatedUser.Name;

            if (!string.IsNullOrWhiteSpace(updatedUser.Surname))
                user.Surname = updatedUser.Surname;

            if (updatedUser.PhoneNumber != null)
                user.PhoneNumber = string.IsNullOrWhiteSpace(updatedUser.PhoneNumber) ? null : updatedUser.PhoneNumber.Trim();

            if (user.UserProfile == null)
            {
                user.UserProfile = new UserProfile
                {
                    UserId = user.Id,
                    AboutMe = string.Empty
                };
            }

            if (updatedUser.AllowNewsletter.HasValue)
                user.UserProfile.AllowNewsletter = updatedUser.AllowNewsletter.Value;

            if (updatedUser.AllowSmsMarketing.HasValue)
                user.UserProfile.AllowSmsMarketing = updatedUser.AllowSmsMarketing.Value;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _auditLogService.WriteAsync(new AuditWriteRequest
            {
                TargetType = AuditLogTargetType.User,
                TargetId = user.Id,
                ActionType = AuditLogActionType.Updated,
                SourceType = AuditLogSourceType.UserPanel,
                Actor = AuditActorInfo.FromPrincipal(User),
                Changes = changes.Build(),
                Reason = "User updated own profile."
            });

            return Ok("User updated successfully.");
        }
    }
}
