using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public enum AdminNoteTargetType
{
    User = 1,
    Meeting = 2,
    Event = 3
}

public class AdminNote : BaseEntity
{
    [Required]
    public AdminNoteTargetType TargetType { get; set; }

    [Required]
    [MaxLength(450)]
    public string TargetId { get; set; } = null!;

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = null!;
}
