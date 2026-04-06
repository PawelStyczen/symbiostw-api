using AutoMapper;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Controllers;

[Route("api/Public/Events")]
[ApiController]
public class PublicEventsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PublicEventsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("CurrentOrNext")]
    public async Task<IActionResult> GetCurrentOrNextEvents([FromQuery] int count = 3)
    {
        if (count <= 0) count = 3;
        if (count > 10) count = 10; // rozsądny limit

        var now = DateTime.UtcNow;

        var baseQuery = _context.Meetings
            .AsNoTracking()
            .Include(m => m.TypeOfMeeting)
            .Include(m => m.Location)
            .Include(m => m.Instructor)
            .Where(m =>
                    !m.IsDeleted &&
                    m.IsVisible &&
                    m.TypeOfMeeting != null &&
                    m.TypeOfMeeting.IsEvent
                // opcjonalnie:
                // && m.Instructor != null && !m.Instructor.IsDeleted
            );

        // 1) trwające teraz
        var current = await baseQuery
            .Where(m => m.Date <= now && m.Date.AddMinutes(m.Duration) >= now)
            .OrderBy(m => m.Date)
            .Take(count)
            .ToListAsync();

        var results = new List<Model.Meeting>(current);

        // 2) dopełnij przyszłymi, jeśli brakuje
        if (results.Count < count)
        {
            var missing = count - results.Count;
            var currentIds = results.Select(m => m.Id).ToList();

            var next = await baseQuery
                .Where(m => m.Date > now && !currentIds.Contains(m.Id))
                .OrderBy(m => m.Date)
                .Take(missing)
                .ToListAsync();

            results.AddRange(next);
        }

        if (results.Count == 0)
            return NoContent();

        // finalny sort
        results = results.OrderBy(m => m.Date).ToList();

        var dto = _mapper.Map<List<MeetingDto>>(results);
        return Ok(dto);
    }
}