using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Controllers;

[Route("api/Public/[controller]")]
[ApiController]
public class PublicContactMessageController : ControllerBase
{
    private readonly IContactMessageRepository _contactMessageRepository;
    private readonly IMapper _mapper;

    public PublicContactMessageController(IContactMessageRepository contactMessageRepository, IMapper mapper)
    {
        _contactMessageRepository = contactMessageRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ContactMessageCreateDto contactDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var contactMessage = _mapper.Map<ContactMessage>(contactDto);
        contactMessage.CreatedDate = DateTime.UtcNow; 

        var result = await _contactMessageRepository.CreateContactMessageAsync(contactMessage);

        if (!result) return StatusCode(500, "Error saving message.");

        return Ok(new { message = "Your message has been received. We'll get back to you soon!" });
    }
}