using System.ComponentModel.DataAnnotations;
using DanceApi.Model;

namespace DanceApi.Dto;

public class AdminNoteReadDto : BaseReadDto
{
    public int Id { get; set; }
    public AdminNoteTargetType TargetType { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class AdminNoteCreateDto
{
    [Required]
    public AdminNoteTargetType TargetType { get; set; }

    [Required]
    [MaxLength(450)]
    public string TargetId { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}

public class AdminNoteUpdateDto
{
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
}
