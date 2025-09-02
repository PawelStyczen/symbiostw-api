using System.ComponentModel.DataAnnotations;

namespace DanceApi.Dto;

public class ContactMessageCreateDto
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
}

public class ContactMessageReadDto : BaseReadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
}