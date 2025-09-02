using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DanceApi.Dto;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace DanceApi.Controllers
{
    [Route("api/User/Account")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class UserAccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public UserAccountController(
            IUserRepository userRepository,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _userManager = userManager;
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
            };

            var result = await _userRepository.CreateUserAsync(user, model.Password);
            if (result.Succeeded)
            {
                var roleAssignmentResult = await _userRepository.AddUserToRoleAsync(user, "User");
                if (roleAssignmentResult)
                    return Ok("User registered successfully.");

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

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) // ❗ Block soft-deleted
                return NotFound("User not found.");

            var result = new
            {
                user.Email,
                user.Name,
                user.Surname,
            };

            return Ok(result);
        }

        [HttpPut("CurrentUser")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserDto.UpdateUserDto updatedUser)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.IsDeleted) // ❗ Block soft-deleted
                return NotFound("User not found.");

            if (!string.IsNullOrWhiteSpace(updatedUser.Name))
                user.Name = updatedUser.Name;

            if (!string.IsNullOrWhiteSpace(updatedUser.Surname))
                user.Surname = updatedUser.Surname;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok("User updated successfully.");
        }
    }
}