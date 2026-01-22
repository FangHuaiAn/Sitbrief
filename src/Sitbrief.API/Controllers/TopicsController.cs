using Microsoft.AspNetCore.Mvc;
using Sitbrief.API.DTOs;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;

namespace Sitbrief.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITopicRepository _topicRepository;
    private readonly ILogger<TopicsController> _logger;

    public TopicsController(
        ITopicRepository topicRepository,
        ILogger<TopicsController> logger)
    {
        _topicRepository = topicRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TopicDto>>> GetAllTopics()
    {
        try
        {
            var topics = await _topicRepository.GetAllAsync();
            var topicDtos = topics.Select(MapToDto).ToList();
            return Ok(topicDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving topics");
            return StatusCode(500, "An error occurred while retrieving topics");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TopicDetailDto>> GetTopic(int id)
    {
        try
        {
            var topic = await _topicRepository.GetWithArticlesAsync(id);
            if (topic == null)
            {
                return NotFound($"Topic with ID {id} not found");
            }

            return Ok(MapToDetailDto(topic));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving topic {TopicId}", id);
            return StatusCode(500, "An error occurred while retrieving the topic");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TopicDto>> CreateTopic(CreateTopicDto createDto)
    {
        try
        {
            var topic = new Topic
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Significance = createDto.Significance
            };

            var createdTopic = await _topicRepository.AddAsync(topic);
            var topicDto = MapToDto(createdTopic);

            return CreatedAtAction(
                nameof(GetTopic),
                new { id = createdTopic.Id },
                topicDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating topic");
            return StatusCode(500, "An error occurred while creating the topic");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTopic(int id, CreateTopicDto updateDto)
    {
        try
        {
            var existingTopic = await _topicRepository.GetByIdAsync(id);
            if (existingTopic == null)
            {
                return NotFound($"Topic with ID {id} not found");
            }

            existingTopic.Title = updateDto.Title;
            existingTopic.Description = updateDto.Description;
            existingTopic.Significance = updateDto.Significance;

            await _topicRepository.UpdateAsync(existingTopic);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating topic {TopicId}", id);
            return StatusCode(500, "An error occurred while updating the topic");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTopic(int id)
    {
        try
        {
            var exists = await _topicRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound($"Topic with ID {id} not found");
            }

            await _topicRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting topic {TopicId}", id);
            return StatusCode(500, "An error occurred while deleting the topic");
        }
    }

    private static TopicDto MapToDto(Topic topic)
    {
        return new TopicDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            Significance = topic.Significance,
            CreatedDate = topic.CreatedDate,
            LastUpdatedDate = topic.LastUpdatedDate,
            ArticleCount = topic.ArticleTopics?.Count ?? 0
        };
    }

    private static TopicDetailDto MapToDetailDto(Topic topic)
    {
        return new TopicDetailDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            Significance = topic.Significance,
            CreatedDate = topic.CreatedDate,
            LastUpdatedDate = topic.LastUpdatedDate,
            ArticleCount = topic.ArticleTopics?.Count ?? 0,
            Articles = topic.ArticleTopics?.Select(at => new ArticleDto
            {
                Id = at.Article.Id,
                Title = at.Article.Title,
                Summary = at.Article.Summary,
                SourceUrl = at.Article.SourceUrl,
                SourceName = at.Article.SourceName,
                SourceType = at.Article.SourceType,
                PublishedDate = at.Article.PublishedDate,
                CreatedDate = at.Article.CreatedDate,
                Topics = new List<TopicSummaryDto>()
            }).ToList() ?? new List<ArticleDto>()
        };
    }
}
