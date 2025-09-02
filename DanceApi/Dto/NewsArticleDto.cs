namespace DanceApi.Dto;

public class NewsArticleReadDto : BaseReadDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string? ImageUrl { get; set; }
    public bool AllowComments { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsVisible { get; set; }
    
   
    public string? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
}

public class NewsArticleCreateDto 
{
    public string Title { get; set; }
    public string Content { get; set; }
    public bool AllowComments { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsVisible { get; set; }
}

public class NewsArticleUpdateDto 
{
    public string Title { get; set; }
    public string Content { get; set; }
    public bool AllowComments { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsVisible { get; set; }
}

public class NewsCommentReadDto : BaseReadDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string UserName { get; set; } 
    public int NewsArticleId { get; set; } 
    public string? NewsArticleTitle { get; set; } 
}

public class NewsCommentCreateDto
{
    public int NewsArticleId { get; set; }
    public string Content { get; set; } = null!;
}