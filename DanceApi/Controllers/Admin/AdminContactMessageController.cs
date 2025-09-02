using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Controllers;

[Route("api/Admin/[controller]")]
[ApiController]
[Authorize]
public class AdminContactMessageController : ControllerBase
{
    private readonly IContactMessageRepository _contactMessageRepository;
    private readonly IMapper _mapper;

    public AdminContactMessageController(IContactMessageRepository contactMessageRepository, IMapper mapper)
    {
        _contactMessageRepository = contactMessageRepository;
        _mapper = mapper;
    }

 
    [HttpGet]
    public async Task<IActionResult> GetAllMessages([FromQuery] bool includeRead = false)
    {
        var messages = await _contactMessageRepository.GetAllMessagesAsync(includeRead);
        return Ok(messages);
    }
 
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var result = await _contactMessageRepository.DeleteMessageAsync(id);
        if (!result) return NotFound("Message not found.");
        return NoContent();
    }


    [HttpPut("{id}/mark-replied")]
    public async Task<IActionResult> MarkMessageAsReplied(int id)
    {
        var result = await _contactMessageRepository.MarkMessageAsRepliedAsync(id);
        if (!result) return NotFound("Message not found.");
        return Ok(new { message = "Message marked as replied." });
    }
    [HttpPut("{id}/mark-read")]
    public async Task<IActionResult> MarkMessageAsRead(int id)
    {
        var result = await _contactMessageRepository.MarkMessageAsReadAsync(id);
        if (!result) return NotFound("Message not found.");
        return Ok(new { message = "Message marked as read." });
    }
    
}