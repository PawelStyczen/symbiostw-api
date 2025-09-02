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
public class AdminLocationController : BaseController
{
    private readonly ILocationRepository _locationRepository;
    private readonly IMapper _mapper;

    public AdminLocationController(ILocationRepository locationRepository, IMapper mapper)
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

    [Authorize]
    [HttpPost]
    [SwaggerIgnore]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateLocation([FromForm] LocationCreateDto createLocationDto, [FromForm] IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetUserId(); 
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var imageUrl = image != null ? await FileHelper.SaveImageAsync(image) : null;

            var location = _mapper.Map<Location>(createLocationDto);
            location.ImageUrl = imageUrl;
            location.CreatedById = userId; 
            location.CreatedDate = DateTime.UtcNow;

            var createdLocation = await _locationRepository.CreateLocationAsync(location, userId);
            var createdLocationDto = _mapper.Map<LocationReadDto>(createdLocation);

            return CreatedAtAction(nameof(GetLocationById), new { id = createdLocation.Id }, createdLocationDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [SwaggerIgnore]
    [Authorize]
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateLocation(int id, [FromForm] LocationUpdateDto updateLocationDto, [FromForm] IFormFile? image)
    {
        var existingLocation = await _locationRepository.GetLocationByIdAsync(id);
        if (existingLocation == null)
        {
            return NotFound("Location not found.");
        }

        _mapper.Map(updateLocationDto, existingLocation);

        if (image != null)
        {
            FileHelper.DeleteImage(existingLocation.ImageUrl); 
            existingLocation.ImageUrl = await FileHelper.SaveImageAsync(image);
        }

        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authorized.");
        }

        var isUpdated = await _locationRepository.UpdateLocationAsync(existingLocation, userId);
        if (!isUpdated)
        {
            return NotFound("Failed to update location.");
        }

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteLocation(int id)
    {
        var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authorized.");
        }

        var result = await _locationRepository.SoftDeleteLocationAsync(id, userId);
        if (!result)
        {
            return NotFound("Location not found.");
        }

        return NoContent();
    }
}