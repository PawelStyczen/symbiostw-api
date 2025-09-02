using System.Security.Claims;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using DanceApi.Controllers;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/Admin/[controller]")]
[ApiController]

public class AdminTypeOfMeetingController : BaseController 
{
    private readonly ITypeOfMeetingRepository _typeOfMeetingRepository;
    private readonly IMapper _mapper;

    public AdminTypeOfMeetingController(ITypeOfMeetingRepository typeOfMeetingRepository, IMapper mapper)
    {
        _typeOfMeetingRepository = typeOfMeetingRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTypeOfMeetings()
    {
        var typeOfMeetings = await _typeOfMeetingRepository.GetAllTypeOfMeetingsAsync();
        var typeOfMeetingDtos = _mapper.Map<IEnumerable<TypeOfMeetingReadDto>>(typeOfMeetings);
        return Ok(typeOfMeetingDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTypeOfMeetingById(int id)
    {
        var typeOfMeeting = await _typeOfMeetingRepository.GetTypeOfMeetingByIdAsync(id);
        if (typeOfMeeting == null)
        {
            return NotFound("Meeting type not found.");
        }

        var typeOfMeetingDto = _mapper.Map<TypeOfMeetingReadDto>(typeOfMeeting);
        return Ok(typeOfMeetingDto);
    }

    [SwaggerIgnore]
    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateTypeOfMeeting([FromForm] TypeOfMeetingCreateDto createDto, [FromForm] IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId(); 
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }

        var imageUrl = image != null ? await FileHelper.SaveImageAsync(image) : null;
        var typeOfMeeting = _mapper.Map<TypeOfMeeting>(createDto);
        typeOfMeeting.ImageUrl = imageUrl;
        typeOfMeeting.CreatedById = userId;
        typeOfMeeting.CreatedDate = DateTime.UtcNow;

        var createdTypeOfMeeting = await _typeOfMeetingRepository.CreateTypeOfMeetingAsync(typeOfMeeting, userId);
        var createdDto = _mapper.Map<TypeOfMeetingReadDto>(createdTypeOfMeeting);

        return CreatedAtAction(nameof(GetTypeOfMeetingById), new { id = createdTypeOfMeeting.Id }, createdDto);
    }

    [SwaggerIgnore]
    [Authorize]
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateTypeOfMeeting(int id, [FromForm] TypeOfMeetingUpdateDto updateDto, [FromForm] IFormFile? image)
    {
        var existingType = await _typeOfMeetingRepository.GetTypeOfMeetingByIdAsync(id);
        if (existingType == null)
        {
            return NotFound("Meeting type not found.");
        }

        _mapper.Map(updateDto, existingType);

        if (image != null)
        {
            FileHelper.DeleteImage(existingType.ImageUrl);
            existingType.ImageUrl = await FileHelper.SaveImageAsync(image);
        }

        var userId = GetUserId(); 
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authorized.");
        }

        var isUpdated = await _typeOfMeetingRepository.UpdateTypeOfMeetingAsync(existingType, userId);
        if (!isUpdated)
        {
            return StatusCode(500, "Failed to update the meeting type.");
        }

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteTypeOfMeeting(int id)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authorized.");
        }

        var result = await _typeOfMeetingRepository.SoftDeleteTypeOfMeetingAsync(id, userId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}