using System.Linq;
using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Controllers
{
    [Route("api/Public/Instructors")]
    [ApiController]
    public class PublicInstructorController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public PublicInstructorController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // GET: api/Public/Instructors
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllInstructors()
        {
            try
            {
                // Get all users in the Instructor role
                var instructors = await _userRepository.GetUsersByRoleAsync("Instructor");

                // Keep only active (not soft-deleted) users with active (not soft-deleted) profiles
                var activeInstructors = instructors
                    .Where(i =>
                        i != null
                        && !i.IsDeleted
                        && i.InstructorProfile != null
                     )
                    .ToList();

                if (activeInstructors.Count == 0)
                {
                    return NotFound("No instructors found.");
                }

                var instructorDtos = _mapper.Map<IEnumerable<PublicInstructorDto>>(activeInstructors);
                return Ok(instructorDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Public/Instructors/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInstructorById(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            // Not found if user or profile is soft-deleted (or profile missing)
            if (user == null
                || user.IsDeleted
                || user.InstructorProfile == null)
            {
                return NotFound($"Instructor with ID {id} not found.");
            }

            var instructorDto = _mapper.Map<PublicInstructorDto>(user);
            return Ok(instructorDto);
        }
    }
}