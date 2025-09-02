using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanceApi.Data;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Repository
{
   public class NewsArticleRepository : BaseRepository<NewsArticle>, INewsArticleRepository
    {
        private readonly AppDbContext _context;

        public NewsArticleRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NewsArticle>> GetAllNewsArticlesAsync()
        {
            return await _context.NewsArticles
                .Where(a => a.IsVisible && !a.IsDeleted)
                .Include(a => a.CreatedBy)  
                .Include(a => a.UpdatedBy) 
                .ToListAsync();
        }

        public async Task<NewsArticle?> GetNewsArticleByIdAsync(int id)
        {
            return await _context.NewsArticles
                .Include(a => a.CreatedBy)  
                .Include(a => a.UpdatedBy) 
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<NewsArticle> CreateNewsArticleAsync(NewsArticle article, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User ID is required to create an article.");
            }

            article.CreatedById = userId;
            article.CreatedDate = DateTime.UtcNow;

            _context.NewsArticles.Add(article);
            await _context.SaveChangesAsync();
            return await GetNewsArticleByIdAsync(article.Id); 
        }

        public async Task<bool> UpdateNewsArticleAsync(NewsArticle article, string userId)
        {
            var existingArticle = await _context.NewsArticles.FindAsync(article.Id);
            if (existingArticle == null || existingArticle.IsDeleted)
            {
                return false;
            }

            existingArticle.Title = article.Title;
            existingArticle.Content = article.Content;
            existingArticle.AllowComments = article.AllowComments;
            existingArticle.IsHighlighted = article.IsHighlighted;
            existingArticle.IsVisible = article.IsVisible;
            existingArticle.UpdatedById = userId;
            existingArticle.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteNewsArticleAsync(int id, string userId)
        {
            var article = await _context.NewsArticles.FindAsync(id);
            if (article == null || article.IsDeleted)
            {
                return false;
            }

            article.IsDeleted = true;
            article.DeletedById = userId;
            article.DeletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<NewsComment>> GetCommentsForArticleAsync(int articleId)
        {
            return await _context.NewsComments
                .Where(c => c.NewsArticleId == articleId && !c.IsDeleted)
                .Include(c => c.CreatedBy) 
                .ToListAsync();
        }
        
        public async Task<NewsComment?> AddCommentToArticleAsync(int articleId, string userId, string content)
        {
            var article = await _context.NewsArticles.FindAsync(articleId);
            if (article == null || !article.AllowComments) return null;

            var comment = new NewsComment
            {
                NewsArticleId = articleId,
                UserId = userId,
                Content = content,
                CreatedDate = DateTime.UtcNow,
                CreatedById = userId 
            };

            _context.NewsComments.Add(comment);
            await _context.SaveChangesAsync();

            return await _context.NewsComments
                .Include(c => c.CreatedBy)
                .Where(c => c.Id == comment.Id)
                .FirstOrDefaultAsync();
        }
        
        public async Task<IEnumerable<NewsComment>> GetCommentsByUserAsync(string userId)
        {
            return await _context.NewsComments
                .Where(c => c.CreatedById == userId)
                .OrderByDescending(c => c.CreatedDate)
                .Include(c => c.NewsArticle) 
                .ToListAsync();
        }
    }
}