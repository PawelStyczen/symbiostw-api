using System.Security.Claims;
using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DanceApi.Controllers;

[Route("api/Public/[controller]")]
[ApiController]
public class PublicTypeOfMeetingController : ControllerBase
{
    private readonly ITypeOfMeetingRepository _typeOfMeetingRepository;
    private readonly IMapper _mapper;

    public PublicTypeOfMeetingController(ITypeOfMeetingRepository typeOfMeetingRepository, IMapper mapper)
    {
        _typeOfMeetingRepository = typeOfMeetingRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTypeOfMeetings()
    {
        var typeOfMeetings = await _typeOfMeetingRepository.GetAllTypeOfMeetingsVisibleAsync();
        var typeOfMeetingDtos = _mapper.Map<IEnumerable<TypeOfMeetingReadDto>>(typeOfMeetings);
        return Ok(typeOfMeetingDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTypeOfMeetingById(int id)
    {
        var typeOfMeeting = await _typeOfMeetingRepository.GetTypeOfMeetingByIdAsync(id);
        if (typeOfMeeting == null)
        {
            return NotFound();
        }

        var typeOfMeetingDto = _mapper.Map<TypeOfMeetingReadDto>(typeOfMeeting);
        return Ok(typeOfMeetingDto);
    }
    [HttpGet("highlighted")]
    public async Task<IActionResult> GetHighlightedTypeOfMeetings()
    {
        var typeOfMeetings = await _typeOfMeetingRepository.GetAllTypeOfMeetingsVisibleAsync();
        var highlightedTypeOfMeetings = typeOfMeetings.Where(t => t.IsHighlighted).ToList(); 
        var typeOfMeetingDtos = _mapper.Map<IEnumerable<TypeOfMeetingReadDto>>(highlightedTypeOfMeetings);
        return Ok(typeOfMeetingDtos);
    }
}