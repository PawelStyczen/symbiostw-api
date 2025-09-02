using DanceApi.Model;

namespace DanceApi.Interface;
public interface INewsArticleRepository
{
    Task<IEnumerable<NewsArticle>> GetAllNewsArticlesAsync();
    Task<NewsArticle?> GetNewsArticleByIdAsync(int id);
  
    Task<NewsArticle> CreateNewsArticleAsync(NewsArticle article, string userId);

    Task<bool> UpdateNewsArticleAsync(NewsArticle article, string userId);
    Task<bool> SoftDeleteNewsArticleAsync(int id, string userId);
    
    Task<IEnumerable<NewsComment>> GetCommentsForArticleAsync(int articleId);
    Task<NewsComment?> AddCommentToArticleAsync(int articleId, string userId, string content);
    Task<IEnumerable<NewsComment>> GetCommentsByUserAsync(string userId);
}
