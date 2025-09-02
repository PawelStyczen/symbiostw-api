using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class Location : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
    [Required]
    [MaxLength(200)]
    public string City { get; set; }
    [Required]
    [MaxLength(200)]
    public string Street { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; } 

 
    public ICollection<Meeting> Meetings { get; set; }
}