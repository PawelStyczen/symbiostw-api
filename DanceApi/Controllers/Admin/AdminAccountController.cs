using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// <-- add this

namespace DanceApi.Controllers.Admin
{
    
    
    [Route("api/Admin/Account")]
    [ApiController]
    [Authorize(Roles = "Instructor,Admin")]
    public class AdminAccountController : ControllerBase
    {private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper; // <-- add this
        

        public AdminAccountController(
            IUserRepository userRepository,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IMapper mapper,
            IConfiguration configuration
            
                ) // <-- add this
        
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper; // <-- add this
            _configuration = configuration;
        }

        // ===== Instructors =====

        [HttpPost("RegisterInstructor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterInstructor(
            [FromForm] RegisterModel model,
            [FromForm] IFormFile? image)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructor = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
            };

            var result = await _userRepository.CreateUserAsync(instructor, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            var roleAssignmentResult = await _userRepository.AddUserToRoleAsync(instructor, "Instructor");
            if (!roleAssignmentResult)
                return BadRequest("User registered, but failed to assign the Instructor role.");

            string? imageUrl = null;
            if (image != null)
            {
                var fileName = $"{Guid.NewGuid()}_{image.FileName}";
                var filePath = Path.Combine("wwwroot/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream);

                imageUrl = $"/uploads/{fileName}";
            }
            else
            {
                imageUrl = "/uploads/e756ec82-c3cb-435a-8398-d778a0609092_User-Account-Person-PNG-File-1026727248.png";
            }

            var instructorProfile = new InstructorProfile
            {
                UserId = instructor.Id,
                Bio = "New instructor profile",
                ExperienceYears = 0,
                Specialization = "General",
                FacebookLink = "",
                InstagramLink = "",
                TikTokLink = "",
                ImageUrl = imageUrl
            };

            _userRepository.AddInstructorProfile(instructorProfile);
            await _userRepository.SaveChangesAsync();

            return Ok("Instructor registered successfully.");
        }

        [HttpPost("LoginInstructor")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginInstructorOrAdmin([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (user.IsDeleted)
                        return Unauthorized("Account is deactivated.");

                    if (await _userManager.IsInRoleAsync(user, "Instructor"))
                    {
                        var token = GenerateJwtToken(user, "Instructor");
                        return Ok(new { Token = token });
                    }

                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        var token = GenerateJwtToken(user, "Admin");
                        return Ok(new { Token = token });
                    }

                    return Unauthorized("You must be an instructor or an admin to access this endpoint.");
                }
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

            return Unauthorized("Invalid login attempt.");
        }

        [HttpPost("LogoutInstructor")]
        [Authorize]
        public async Task<IActionResult> LogoutInstructor()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("No Authorization header provided.");
                    return Unauthorized("Token not provided.");
                }

                Console.WriteLine($"Received Authorization header: {token}");

                if (token.StartsWith("Bearer "))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                Console.WriteLine($"Extracted token: {token}");

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                foreach (var claim in jwtToken.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                }

                await _signInManager.SignOutAsync();
                Console.WriteLine("User logged out successfully.");
                return Ok("Instructor logged out successfully.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
                return StatusCode(500, $"Internal server error during logout: {ex.Message}");
            }
        }

        private string GenerateJwtToken(User user, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("Instructors")]
        public async Task<IActionResult> GetAllInstructors()
        {
            var instructors = await _userRepository.GetUsersByRoleAsync("Instructor");

            var active = instructors
                .Where(i => !i.IsDeleted)
                .ToList();

            if (!active.Any())
                return NotFound("No instructors found.");

            var dto = _mapper.Map<IEnumerable<InstructorDto>>(active);
            return Ok(dto);
        }

        [HttpGet("Instructors/{id}")]
        public async Task<IActionResult> GetInstructorById(string id)
        {
            var instructor = await _userManager.Users
                .Include(u => u.InstructorProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (instructor == null || instructor.IsDeleted || instructor.InstructorProfile == null)
                return NotFound($"Instructor with ID {id} not found.");

            var dto = _mapper.Map<InstructorDto>(instructor);
            return Ok(dto);
        }

        [HttpPut("Instructors/{id}")]
[Authorize(Roles = "Admin")] // or "Instructor,Admin" if instructors can edit themselves
[Consumes("multipart/form-data")]
public async Task<IActionResult> UpdateInstructor(
    string id,
    [FromForm] InstructorUpdateDto dto,
    [FromForm] IFormFile? image)
{
    // Load user + profile
    var instructor = await _userManager.Users
        .Include(u => u.InstructorProfile)
        .FirstOrDefaultAsync(u => u.Id == id);

    if (instructor == null || instructor.IsDeleted || instructor.InstructorProfile == null)
        return NotFound($"Instructor with ID {id} not found.");

    // Update profile fields
    instructor.Name = dto.Name;
    instructor.Surname = dto.Surname;
    instructor.InstructorProfile.Bio = dto.Bio;
    instructor.InstructorProfile.ExperienceYears = dto.ExperienceYears;
    instructor.InstructorProfile.Specialization = dto.Specialization;
    instructor.InstructorProfile.FacebookLink = dto.FacebookLink;
    instructor.InstructorProfile.InstagramLink = dto.InstagramLink;
    instructor.InstructorProfile.TikTokLink = dto.TikTokLink;

    // Handle image replacement (optional)
    if (image != null)
    {
        // delete old file if you want
        if (!string.IsNullOrWhiteSpace(instructor.InstructorProfile.ImageUrl))
        {
            var oldPath = Path.Combine("wwwroot", instructor.InstructorProfile.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldPath))
            {
                try { System.IO.File.Delete(oldPath); } catch { /* swallow or log */ }
            }
        }

        var fileName = $"{Guid.NewGuid()}_{image.FileName}";
        var filePath = Path.Combine("wwwroot/uploads", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await image.CopyToAsync(stream);

        instructor.InstructorProfile.ImageUrl = $"/uploads/{fileName}";
    }
    else if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
    {
        // if you want to allow setting an existing URL from DTO
        instructor.InstructorProfile.ImageUrl = dto.ImageUrl!;
    }

    // Persist
    var result = await _userManager.UpdateAsync(instructor);
    if (!result.Succeeded)
        return BadRequest(result.Errors.Select(e => e.Description));

    await _userRepository.SaveChangesAsync();
    return NoContent();
}
        [HttpDelete("Instructors/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteInstructor(string id)
        {
            var instructor = await _userManager.Users
                .Include(u => u.InstructorProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (instructor == null)
                return NotFound($"Instructor with ID {id} not found.");

            instructor.IsDeleted = true;
            var result = await _userManager.UpdateAsync(instructor);
            if (!result.Succeeded)
                return BadRequest("Failed to soft delete the instructor.");

            await _userRepository.SaveChangesAsync();
            return Ok($"Instructor {instructor.Name} {instructor.Surname} soft deleted successfully.");
        }

        // ===== Users (Soft Delete) =====

        [HttpGet("Users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound("User not found.");

            var result = new
            {
                user.Id,
                user.Name,
                user.Surname,
                user.Email
            };

            return Ok(result);
        }

        [HttpGet("Users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRegularUsers()
        {
            var users = await _userRepository.GetUsersByRoleAsync("User");
            var filtered = users.Where(u => !u.IsDeleted)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Surname,
                    u.Email,
                    u.UserName
                })
                .ToList();

            if (!filtered.Any())
                return NotFound("No regular users found.");

            return Ok(filtered);
        }

        [HttpPut("Users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] RegisterModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound("User not found.");

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok("User updated successfully.");
        }

        [HttpDelete("Users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found.");

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest("Failed to soft delete user.");

            await _userRepository.SaveChangesAsync();
            return Ok("User soft deleted successfully.");
        }

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
                Surname = model.Surname
            };

            var result = await _userRepository.CreateUserAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            if (!await _userRepository.AddUserToRoleAsync(user, "User"))
                return BadRequest("User registered, but failed to assign the User role.");

            return Ok("User registered successfully.");
        }
    }
}