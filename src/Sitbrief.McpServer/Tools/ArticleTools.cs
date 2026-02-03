using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Enums;
using Sitbrief.Infrastructure.Data;

namespace Sitbrief.McpServer.Tools;

[McpServerToolType]
public class ArticleTools
{
    private readonly SitbriefDbContext _db;

    public ArticleTools(SitbriefDbContext db)
    {
        _db = db;
    }

    [McpServerTool, Description("取得文章列表，可依主題或關鍵字篩選")]
    public async Task<string> GetArticles(
        [Description("主題 ID（可選）")] int? topicId = null,
        [Description("搜尋關鍵字（可選）")] string? keyword = null,
        [Description("最大回傳數量")] int limit = 20)
    {
        var query = _db.Articles
            .Include(a => a.ArticleTopics)
            .ThenInclude(at => at.Topic)
            .AsQueryable();

        if (topicId.HasValue)
        {
            query = query.Where(a => a.ArticleTopics.Any(at => at.TopicId == topicId.Value));
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(a =>
                a.Title.Contains(keyword) ||
                a.Summary.Contains(keyword));
        }

        var articles = await query
            .OrderByDescending(a => a.PublishedDate)
            .Take(limit)
            .ToListAsync();

        if (articles.Count == 0)
        {
            return "沒有找到符合條件的文章。";
        }

        var lines = articles.Select(a =>
        {
            var topics = string.Join(", ", a.ArticleTopics.Select(at => at.Topic.Title));
            return $"""
                [{a.Id}] {a.Title}
                  來源：{a.SourceName} | 日期：{a.PublishedDate:yyyy-MM-dd}
                  主題：{(string.IsNullOrEmpty(topics) ? "未分類" : topics)}
                  摘要：{(a.Summary.Length > 100 ? a.Summary[..100] + "..." : a.Summary)}
                """;
        });

        return $"共 {articles.Count} 篇文章：\n\n{string.Join("\n\n", lines)}";
    }

    [McpServerTool, Description("依 ID 取得單一文章完整內容")]
    public async Task<string> GetArticle(
        [Description("文章 ID")] int articleId)
    {
        var article = await _db.Articles
            .Include(a => a.ArticleTopics)
            .ThenInclude(at => at.Topic)
            .Include(a => a.AIAnalysis)
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
        {
            return $"找不到 ID 為 {articleId} 的文章。";
        }

        var topics = string.Join(", ", article.ArticleTopics.Select(at => at.Topic.Title));
        var analysis = article.AIAnalysis;

        var result = $"""
            標題：{article.Title}
            來源：{article.SourceName}
            網址：{article.SourceUrl}
            日期：{article.PublishedDate:yyyy-MM-dd}
            主題：{(string.IsNullOrEmpty(topics) ? "未分類" : topics)}
            
            摘要：
            {article.Summary}
            """;

        if (analysis != null)
        {
            result += $"""
                
                
                === AI 分析 ===
                重要性：{analysis.SignificanceScore}/10
                AI 摘要：{analysis.SuggestedTopicsJson}
                關鍵實體：{analysis.KeyEntitiesJson}
                標籤：{analysis.GeopoliticalTagsJson}
                """;
        }

        return result;
    }

    [McpServerTool, Description("新增文章")]
    public async Task<string> CreateArticle(
        [Description("文章標題")] string title,
        [Description("文章摘要")] string summary,
        [Description("來源名稱")] string sourceName,
        [Description("來源網址")] string sourceUrl,
        [Description("發布日期 (YYYY-MM-DD)")] string publishedDate,
        [Description("主題 ID 列表，以逗號分隔（可選）")] string? topicIds = null)
    {
        if (!DateTime.TryParse(publishedDate, out var date))
        {
            return "日期格式錯誤，請使用 YYYY-MM-DD 格式。";
        }

        var article = new Article
        {
            Title = title,
            Summary = summary,
            SourceName = sourceName,
            SourceUrl = sourceUrl,
            SourceType = SourceType.NewsMedia,
            PublishedDate = date,
            CreatedDate = DateTime.UtcNow
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync();

        // 連結主題
        if (!string.IsNullOrEmpty(topicIds))
        {
            var ids = topicIds.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            foreach (var topicId in ids)
            {
                var topic = await _db.Topics.FindAsync(topicId);
                if (topic != null)
                {
                    _db.Set<ArticleTopic>().Add(new ArticleTopic
                    {
                        ArticleId = article.Id,
                        TopicId = topicId
                    });
                }
            }
            await _db.SaveChangesAsync();
        }

        return $"""
            已建立文章「{title}」(ID: {article.Id})
            
            現在你可以：
            1. 分析這篇文章的地緣政治意義
            2. 根據現有主題列表建議分類
            3. 使用 LinkArticleTopics 連結到相關主題
            """;
    }

    [McpServerTool, Description("將文章連結到主題")]
    public async Task<string> LinkArticleTopics(
        [Description("文章 ID")] int articleId,
        [Description("主題 ID 列表，以逗號分隔")] string topicIds)
    {
        var article = await _db.Articles
            .Include(a => a.ArticleTopics)
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
        {
            return $"找不到 ID 為 {articleId} 的文章。";
        }

        var ids = topicIds.Split(',')
            .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
            .Where(id => id > 0)
            .ToList();

        var linkedTopics = new List<string>();

        foreach (var topicId in ids)
        {
            // 檢查是否已連結
            if (article.ArticleTopics.Any(at => at.TopicId == topicId))
            {
                continue;
            }

            var topic = await _db.Topics.FindAsync(topicId);
            if (topic != null)
            {
                _db.Set<ArticleTopic>().Add(new ArticleTopic
                {
                    ArticleId = articleId,
                    TopicId = topicId
                });
                linkedTopics.Add(topic.Title);
            }
        }

        await _db.SaveChangesAsync();

        if (linkedTopics.Count == 0)
        {
            return "沒有新的主題被連結（可能已經連結過了）。";
        }

        return $"已將文章連結到主題：{string.Join(", ", linkedTopics)}";
    }
}
