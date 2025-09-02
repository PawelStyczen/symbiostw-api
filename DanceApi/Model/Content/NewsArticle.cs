using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class NewsArticle : VisibleHighlightBaseEntity 
{
    [Required] [MaxLength(200)]  public string Title { get; set; } = null!;
    //TODO null!;
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; }
    public string? ImageUrl { get; set; } 
    public bool AllowComments { get; set; } = true; 
    

    public ICollection<NewsComment> Comments { get; set; } = new List<NewsComment>();
}