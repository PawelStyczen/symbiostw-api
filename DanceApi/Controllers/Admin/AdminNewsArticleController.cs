using System.Security.Claims;
using AutoMapper;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace DanceApi.Controllers.Admin
{
    [Route("api/Admin/[controller]")]
    [ApiController]
    public class AdminNewsArticleController : BaseController
    {
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly IMapper _mapper;

        public AdminNewsArticleController(INewsArticleRepository newsArticleRepository, IMapper mapper)
        {
            _newsArticleRepository = newsArticleRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNewsArticles()
        {
            var articles = await _newsArticleRepository.GetAllNewsArticlesAsync();
            var articleDtos = _mapper.Map<IEnumerable<NewsArticleReadDto>>(articles);
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
        [SwaggerIgnore]
        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateNewsArticle(
            [FromForm] NewsArticleCreateDto createArticleDto,
            [FromForm] IFormFile? image)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User is not authenticated.");

                // If an image was uploaded, save it; otherwise use the default
                var imageUrl = image != null
                    ? await FileHelper.SaveImageAsync(image)
                    : "/uploads/defaultNews.png";  // <-- make sure this file exists in wwwroot/uploads

                var article = _mapper.Map<NewsArticle>(createArticleDto);
                article.ImageUrl = imageUrl;

                var createdArticle = await _newsArticleRepository.CreateNewsArticleAsync(article, userId);
                var createdArticleDto = _mapper.Map<NewsArticleReadDto>(createdArticle);

                return CreatedAtAction(nameof(GetNewsArticleById),
                    new { id = createdArticle.Id }, createdArticleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [SwaggerIgnore]
        [Authorize]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateNewsArticle(int id, [FromForm] NewsArticleUpdateDto updateArticleDto, [FromForm] IFormFile? image)
        {
            var existingArticle = await _newsArticleRepository.GetNewsArticleByIdAsync(id);
            if (existingArticle == null)
            {
                return NotFound("Article not found.");
            }

            _mapper.Map(updateArticleDto, existingArticle);

            if (image != null)
            {
                FileHelper.DeleteImage(existingArticle.ImageUrl); 
                existingArticle.ImageUrl = await FileHelper.SaveImageAsync(image);
            }

            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authorized.");
            }

            var isUpdated = await _newsArticleRepository.UpdateNewsArticleAsync(existingArticle, userId);
            if (!isUpdated)
            {
                return NotFound("Failed to update article.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteNewsArticle(int id)
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authorized.");
            }

            var result = await _newsArticleRepository.SoftDeleteNewsArticleAsync(id, userId);
            if (!result)
            {
                return NotFound("Article not found.");
            }

            return NoContent();
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
        
       
    }
    
    
}