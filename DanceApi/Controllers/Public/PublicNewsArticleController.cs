using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DanceApi.Controllers;


[Route("api/Public/[controller]")]
[ApiController]

public class PublicNewsArticleController : BaseController
{
    
    private readonly INewsArticleRepository _newsArticleRepository;
    private readonly IMapper _mapper;
    
    public PublicNewsArticleController(INewsArticleRepository newsArticleRepository, IMapper mapper)
    {
        _newsArticleRepository = newsArticleRepository;
        _mapper = mapper;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllNewsArticles()
    {
        var articles = await _newsArticleRepository.GetAllNewsArticlesAsync();

        var sortedArticles = articles
            .OrderByDescending(a => a.CreatedDate) // newest first
            .ToList();

        var articleDtos = _mapper.Map<IEnumerable<NewsArticleReadDto>>(sortedArticles);
        return Ok(articleDtos);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNewsArticleById(int id)
    {
        var article = await _newsArticleRepository.GetNewsArticleByIdAsync(id);
        if (article == null)
        {
            return NotFound("Article not found.");
        }

        var articleDto = _mapper.Map<NewsArticleReadDto>(article);
        return Ok(articleDto);
    }
    
    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetCommentsForArticle(int id)
    {
        var comments = await _newsArticleRepository.GetCommentsForArticleAsync(id);

        if (comments == null || !comments.Any())
        {
            return Ok(new List<NewsCommentReadDto>()); 
        }

        var commentDtos = _mapper.Map<IEnumerable<NewsCommentReadDto>>(comments);
        return Ok(commentDtos);
    }
    
    [Authorize]
    [HttpPost("{id}/comments")]
    [Consumes("application/json")]
    public async Task<IActionResult> AddCommentToArticle(int id, [FromBody] NewsCommentCreateDto commentDto)
    {
        if (id != commentDto.NewsArticleId) return BadRequest("Article ID mismatch.");

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized("User is not authenticated.");

        var newComment = await _newsArticleRepository.AddCommentToArticleAsync(id, userId, commentDto.Content);
        if (newComment == null) return BadRequest("Could not add comment. Article might not allow comments.");

        var commentReadDto = _mapper.Map<NewsCommentReadDto>(newComment);
        return CreatedAtAction(nameof(GetCommentsForArticle), new { id }, commentReadDto);
    }
    
    [Authorize]
    [HttpGet("Comments")]
    public async Task<IActionResult> GetUserComments()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized("User is not authenticated.");

        var comments = await _newsArticleRepository.GetCommentsByUserAsync(userId);
    
        if (comments == null || !comments.Any())
        {
            return Ok(new List<NewsCommentReadDto>()); 
        }

        var commentDtos = _mapper.Map<IEnumerable<NewsCommentReadDto>>(comments);
        return Ok(commentDtos);
    }
    [HttpGet("highlighted")]
    public async Task<IActionResult> GetHighlightedNews()
    {
        var articles = await _newsArticleRepository.GetAllNewsArticlesAsync();
    
      
        var highlightedArticles = articles
            .Where(a => a.IsHighlighted)
            .OrderByDescending(a => a.CreatedDate)
            .ToList();
    
        var articleDtos = _mapper.Map<IEnumerable<NewsArticleReadDto>>(highlightedArticles);
        return Ok(articleDtos);
    }
}