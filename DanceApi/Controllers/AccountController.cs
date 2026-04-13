using DanceApi.Interface;
using DanceApi.Helper;
using DanceApi.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DanceApi.Controllers
{
    [Route("api/Account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;

        public AccountController(
            IUserRepository userRepository,
            SignInManager<User> signInManager,
            IAuditLogService auditLogService)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _auditLogService = auditLogService;
        }

        // POST: api/Account/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
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

                var roleResult = await _userRepository.AddUserToRoleAsync(user, "User");
                if (roleResult)
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
                        Reason = "Regular user registered via public account endpoint."
                    });

                    return Ok("User registered successfully.");
                }
                else
                {
                    return BadRequest("User registered but failed to assign role.");
                }
            }
            
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        // POST: api/Account/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Ok("User logged in successfully.");
            }
            else if (result.IsLockedOut)
            {
                return BadRequest("User account is locked out.");
            }
            else
            {
                if (result.IsNotAllowed)
                    return Unauthorized("Login not allowed. Please confirm your email or contact support.");
                
                return Unauthorized("Invalid login attempt.");
            }
        }

        // POST: api/Account/Logout
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok("User logged out successfully.");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error during logout: {ex.Message}");
            }
        }
    }
}
