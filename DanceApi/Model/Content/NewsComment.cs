using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class NewsComment : BaseEntity 
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; }
    [Required]
    public string UserId { get; set; } 
    [Required]
    public int NewsArticleId { get; set; } 
    [Required]
    public NewsArticle NewsArticle { get; set; } 
   
}