using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Sitbrief.Core.Entities;
using Sitbrief.Infrastructure.Data;

namespace Sitbrief.McpServer.Tools;

[McpServerToolType]
public class TopicTools
{
    private readonly SitbriefDbContext _db;

    public TopicTools(SitbriefDbContext db)
    {
        _db = db;
    }

    [McpServerTool, Description("取得所有主題列表")]
    public async Task<string> GetTopics()
    {
        var topics = await _db.Topics
            .OrderBy(t => t.Title)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                ArticleCount = t.ArticleTopics.Count
            })
            .ToListAsync();

        if (topics.Count == 0)
        {
            return "目前沒有任何主題。";
        }

        var lines = topics.Select(t =>
            $"- [{t.Id}] {t.Title} ({t.ArticleCount} 篇文章)\n  {t.Description}");

        return $"共 {topics.Count} 個主題：\n\n{string.Join("\n\n", lines)}";
    }

    [McpServerTool, Description("依 ID 取得單一主題及其文章")]
    public async Task<string> GetTopic(
        [Description("主題 ID")] int topicId)
    {
        var topic = await _db.Topics
            .Include(t => t.ArticleTopics)
            .ThenInclude(at => at.Article)
            .FirstOrDefaultAsync(t => t.Id == topicId);

        if (topic == null)
        {
            return $"找不到 ID 為 {topicId} 的主題。";
        }

        var articles = topic.ArticleTopics
            .OrderByDescending(at => at.Article.PublishedDate)
            .Take(10)
            .Select(at => at.Article);

        var articleLines = articles.Select(a =>
            $"  - [{a.Id}] {a.Title} ({a.PublishedDate:yyyy-MM-dd})");

        return $"""
            主題：{topic.Title}
            描述：{topic.Description}
            文章數：{topic.ArticleTopics.Count}
            
            最近文章：
            {string.Join("\n", articleLines)}
            """;
    }

    [McpServerTool, Description("建立新主題")]
    public async Task<string> CreateTopic(
        [Description("主題標題")] string title,
        [Description("主題描述")] string description)
    {
        var topic = new Topic
        {
            Title = title,
            Description = description,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        _db.Topics.Add(topic);
        await _db.SaveChangesAsync();

        return $"已建立主題「{title}」(ID: {topic.Id})";
    }
}
