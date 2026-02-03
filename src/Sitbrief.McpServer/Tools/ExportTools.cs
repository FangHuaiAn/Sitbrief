using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Sitbrief.Infrastructure.Data;

namespace Sitbrief.McpServer.Tools;

[McpServerToolType]
public class ExportTools
{
    private readonly SitbriefDbContext _db;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly string OutputPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Documents", "Projects", "Sitbrief", "output");

    public ExportTools(SitbriefDbContext db)
    {
        _db = db;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    [McpServerTool, Description("匯出 JSON 檔案供 iOS App 使用")]
    public async Task<string> ExportJson()
    {
        Directory.CreateDirectory(OutputPath);
        var timestamp = DateTime.UtcNow;

        // 匯出文章
        var articles = await _db.Articles
            .Include(a => a.ArticleTopics)
            .Include(a => a.AIAnalysis)
            .OrderByDescending(a => a.PublishedDate)
            .ToListAsync();

        var articlesExport = new
        {
            Version = timestamp.ToString("o"),
            GeneratedAt = timestamp.ToString("o"),
            Count = articles.Count,
            Articles = articles.Select(a => new
            {
                a.Id,
                a.Title,
                a.Summary,
                a.SourceName,
                a.SourceUrl,
                PublishedDate = a.PublishedDate.ToString("yyyy-MM-dd"),
                CreatedAt = a.CreatedDate.ToString("o"),
                TopicIds = a.ArticleTopics.Select(at => at.TopicId).ToList(),
                Analysis = a.AIAnalysis != null ? new
                {
                    a.AIAnalysis.SignificanceScore,
                    SuggestedTopics = a.AIAnalysis.SuggestedTopicsJson,
                    KeyEntities = a.AIAnalysis.KeyEntitiesJson,
                    GeopoliticalTags = a.AIAnalysis.GeopoliticalTagsJson
                } : null
            })
        };

        var articlesJson = JsonSerializer.Serialize(articlesExport, _jsonOptions);
        await File.WriteAllTextAsync(Path.Combine(OutputPath, "articles.json"), articlesJson);

        // 匯出主題
        var topics = await _db.Topics
            .Include(t => t.ArticleTopics)
            .OrderBy(t => t.Title)
            .ToListAsync();

        var topicsExport = new
        {
            Version = timestamp.ToString("o"),
            GeneratedAt = timestamp.ToString("o"),
            Count = topics.Count,
            Topics = topics.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                ArticleCount = t.ArticleTopics.Count,
                LastUpdated = t.LastUpdatedDate.ToString("o"),
                RecentArticleIds = t.ArticleTopics
                    .OrderByDescending(at => at.Article?.PublishedDate)
                    .Take(5)
                    .Select(at => at.ArticleId)
                    .ToList()
            })
        };

        var topicsJson = JsonSerializer.Serialize(topicsExport, _jsonOptions);
        await File.WriteAllTextAsync(Path.Combine(OutputPath, "topics.json"), topicsJson);

        // 匯出 metadata
        var metadata = new
        {
            Version = timestamp.ToString("o"),
            LastSync = timestamp.ToString("o"),
            Stats = new
            {
                TotalArticles = articles.Count,
                TotalTopics = topics.Count,
                ArticlesThisWeek = articles.Count(a =>
                    a.PublishedDate >= DateTime.UtcNow.AddDays(-7))
            },
            Endpoints = new
            {
                Articles = "articles.json",
                Topics = "topics.json"
            }
        };

        var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);
        await File.WriteAllTextAsync(Path.Combine(OutputPath, "metadata.json"), metadataJson);

        return $"""
            ✅ JSON 匯出完成！
            
            輸出目錄：{OutputPath}
            
            檔案：
            - articles.json ({articles.Count} 篇文章)
            - topics.json ({topics.Count} 個主題)
            - metadata.json
            
            下一步：執行 sync 指令上傳到 Azure Static Web Apps
            """;
    }

    [McpServerTool, Description("顯示匯出狀態")]
    public Task<string> ExportStatus()
    {
        var articlesPath = Path.Combine(OutputPath, "articles.json");
        var topicsPath = Path.Combine(OutputPath, "topics.json");
        var metadataPath = Path.Combine(OutputPath, "metadata.json");

        if (!File.Exists(articlesPath))
        {
            return Task.FromResult("尚未匯出任何 JSON 檔案。請先執行 ExportJson。");
        }

        var articlesInfo = new FileInfo(articlesPath);
        var topicsInfo = new FileInfo(topicsPath);
        var metadataInfo = new FileInfo(metadataPath);

        return Task.FromResult($"""
            📁 匯出目錄：{OutputPath}
            
            檔案狀態：
            - articles.json: {articlesInfo.Length / 1024.0:F1} KB (最後更新: {articlesInfo.LastWriteTime:yyyy-MM-dd HH:mm})
            - topics.json: {topicsInfo.Length / 1024.0:F1} KB (最後更新: {topicsInfo.LastWriteTime:yyyy-MM-dd HH:mm})
            - metadata.json: {metadataInfo.Length / 1024.0:F1} KB (最後更新: {metadataInfo.LastWriteTime:yyyy-MM-dd HH:mm})
            """);
    }

    private static List<string> ParseList(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return new List<string>();

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
