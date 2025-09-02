using System.Security.Claims;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/Public/[controller]")]
[ApiController]
public class PublicLocationController : ControllerBase
{
    private readonly ILocationRepository _locationRepository;
    private readonly IMapper _mapper;

    public PublicLocationController(ILocationRepository locationRepository, IMapper mapper)
    {
        _locationRepository = locationRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLocations()
    {
        var locations = await _locationRepository.GetAllLocationsAsync();
        var locationDtos = _mapper.Map<IEnumerable<LocationReadDto>>(locations);
        return Ok(locationDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLocationById(int id)
    {
        var location = await _locationRepository.GetLocationByIdAsync(id);
        if (location == null)
        {
            return NotFound("Location not found.");
        }

        var locationDto = _mapper.Map<LocationReadDto>(location);
        return Ok(locationDto);
    }
    
}