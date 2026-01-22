using Microsoft.AspNetCore.Mvc;
using Sitbrief.API.DTOs;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;

namespace Sitbrief.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleRepository _articleRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IAIService _aiService;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(
        IArticleRepository articleRepository,
        ITopicRepository topicRepository,
        IAIService aiService,
        ILogger<ArticlesController> logger)
    {
        _articleRepository = articleRepository;
        _topicRepository = topicRepository;
        _aiService = aiService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAllArticles()
    {
        try
        {
            var articles = await _articleRepository.GetAllAsync();
            var articleDtos = articles.Select(MapToDto).ToList();
            return Ok(articleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving articles");
            return StatusCode(500, "An error occurred while retrieving articles");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleDto>> GetArticle(int id)
    {
        try
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound($"Article with ID {id} not found");
            }

            return Ok(MapToDto(article));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article {ArticleId}", id);
            return StatusCode(500, "An error occurred while retrieving the article");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ArticleDto>> CreateArticle(CreateArticleDto createDto)
    {
        try
        {
            var article = new Article
            {
                Title = createDto.Title,
                Summary = createDto.Summary,
                SourceUrl = createDto.SourceUrl,
                SourceName = createDto.SourceName,
                SourceType = createDto.SourceType,
                Content = createDto.Content,
                PublishedDate = createDto.PublishedDate
            };

            var createdArticle = await _articleRepository.AddAsync(article);
            var articleDto = MapToDto(createdArticle);

            return CreatedAtAction(
                nameof(GetArticle),
                new { id = createdArticle.Id },
                articleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article");
            return StatusCode(500, "An error occurred while creating the article");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArticle(int id, CreateArticleDto updateDto)
    {
        try
        {
            var existingArticle = await _articleRepository.GetByIdAsync(id);
            if (existingArticle == null)
            {
                return NotFound($"Article with ID {id} not found");
            }

            existingArticle.Title = updateDto.Title;
            existingArticle.Summary = updateDto.Summary;
            existingArticle.SourceUrl = updateDto.SourceUrl;
            existingArticle.SourceName = updateDto.SourceName;
            existingArticle.SourceType = updateDto.SourceType;
            existingArticle.Content = updateDto.Content;
            existingArticle.PublishedDate = updateDto.PublishedDate;

            await _articleRepository.UpdateAsync(existingArticle);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", id);
            return StatusCode(500, "An error occurred while updating the article");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        try
        {
            var exists = await _articleRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound($"Article with ID {id} not found");
            }

            await _articleRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return StatusCode(500, "An error occurred while deleting the article");
        }
    }

    [HttpPost("{id}/analyze")]
    public async Task<ActionResult<AIAnalysisResultDto>> AnalyzeArticle(int id)
    {
        try
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound($"Article with ID {id} not found");
            }

            var topics = await _topicRepository.GetAllAsync();
            var result = await _aiService.AnalyzeArticleAsync(article, topics);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            var dto = new AIAnalysisResultDto
            {
                SuggestedExistingTopics = result.SuggestedExistingTopics.Select(t => new SuggestedExistingTopicDto
                {
                    TopicId = t.TopicId,
                    Confidence = t.Confidence,
                    Reason = t.Reason
                }).ToList(),
                SuggestedNewTopics = result.SuggestedNewTopics.Select(t => new SuggestedNewTopicDto
                {
                    Title = t.Title,
                    Description = t.Description
                }).ToList(),
                KeyEntities = new KeyEntitiesDto
                {
                    Countries = result.KeyEntities.Countries,
                    Organizations = result.KeyEntities.Organizations,
                    Persons = result.KeyEntities.Persons
                },
                GeopoliticalTags = result.GeopoliticalTags,
                Significance = result.Significance,
                Summary = result.Summary
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing article {ArticleId}", id);
            return StatusCode(500, "An error occurred while analyzing the article");
        }
    }

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Summary = article.Summary,
            SourceUrl = article.SourceUrl,
            SourceName = article.SourceName,
            SourceType = article.SourceType,
            PublishedDate = article.PublishedDate,
            CreatedDate = article.CreatedDate,
            Topics = article.ArticleTopics.Select(at => new TopicSummaryDto
            {
                Id = at.Topic.Id,
                Title = at.Topic.Title
            }).ToList()
        };
    }
}
