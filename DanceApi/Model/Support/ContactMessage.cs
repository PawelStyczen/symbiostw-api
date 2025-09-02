using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class ContactMessage : BaseEntity 
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; }

    [Required]
    public string Message { get; set; }

    public bool IsRead { get; set; } = false; 

    public bool IsReplied { get; set; } = false; 
}