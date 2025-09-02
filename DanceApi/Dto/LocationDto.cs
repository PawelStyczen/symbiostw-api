using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Dto;

public class LocationCreateDto 
{
    public string Name { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string Description { get; set; }
    [FromForm(Name = "image")]
    public IFormFile? Image { get; set; }

}

public class LocationUpdateDto 
{
  
    public string Name { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string Description { get; set; }
    
    [FromForm(Name = "image")]
    public IFormFile? Image { get; set; }
    
}

public class LocationReadDto : BaseReadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }

}