using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class SubPage : VisibleHighlightBaseEntity 
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } 
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } 
    [Required]
    public string Content { get; set; } 
    public string MetaTitle { get; set; } 
    public string MetaDescription { get; set; } 
    public bool IsPublished { get; set; } = false; 
    public string? ImageUrl { get; set; } 
}