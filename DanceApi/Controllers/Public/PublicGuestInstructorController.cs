using AutoMapper;
using DanceApi.Data;
using DanceApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Controllers;

[Route("api/Public/GuestInstructors")]
[ApiController]
public class PublicGuestInstructorController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PublicGuestInstructorController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGuestInstructorById(int id)
    {
        var guestInstructor = await _context.GuestUsers
            .Include(g => g.GuestInstructorProfile)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (guestInstructor == null
            || guestInstructor.IsDeleted
            || guestInstructor.GuestInstructorProfile == null)
        {
            return NotFound($"Guest instructor with ID {id} not found.");
        }

        var dto = _mapper.Map<PublicGuestInstructorDto>(guestInstructor);
        return Ok(dto);
    }
}
