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
    private const int PageSize = 20;

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

    [McpServerTool, Description("匯出 JSON 檔案供 iOS App 使用（分頁結構）")]
    public async Task<string> ExportJson()
    {
        // 建立目錄結構
        Directory.CreateDirectory(OutputPath);
        var articlesDir = Path.Combine(OutputPath, "articles");
        Directory.CreateDirectory(articlesDir);
        
        var timestamp = DateTime.UtcNow;

        // 取得所有文章
        var articles = await _db.Articles
            .Include(a => a.ArticleTopics)
            .Include(a => a.AIAnalysis)
            .OrderByDescending(a => a.PublishedDate)
            .ToListAsync();

        // 轉換文章格式
        var articleDtos = articles.Select(a => new
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
        }).ToList();

        // 計算總頁數
        var totalPages = (int)Math.Ceiling(articleDtos.Count / (double)PageSize);
        if (totalPages == 0) totalPages = 1;

        // 匯出 latest.json（最新 20 篇）
        var latestExport = new
        {
            Version = timestamp.ToString("o"),
            GeneratedAt = timestamp.ToString("o"),
            TotalCount = articleDtos.Count,
            TotalPages = totalPages,
            PageSize = PageSize,
            Page = 0,
            Articles = articleDtos.Take(PageSize).ToList()
        };
        await WriteJsonAsync(Path.Combine(articlesDir, "latest.json"), latestExport);

        // 匯出分頁檔案
        for (int page = 1; page <= totalPages; page++)
        {
            var pageArticles = articleDtos
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var pageExport = new
            {
                Version = timestamp.ToString("o"),
                GeneratedAt = timestamp.ToString("o"),
                TotalCount = articleDtos.Count,
                TotalPages = totalPages,
                PageSize = PageSize,
                Page = page,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1,
                Articles = pageArticles
            };
            await WriteJsonAsync(Path.Combine(articlesDir, $"page-{page}.json"), pageExport);
        }

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
        await WriteJsonAsync(Path.Combine(OutputPath, "topics.json"), topicsExport);

        // 匯出 metadata
        var metadata = new
        {
            Version = timestamp.ToString("o"),
            LastSync = timestamp.ToString("o"),
            Stats = new
            {
                TotalArticles = articles.Count,
                TotalTopics = topics.Count,
                TotalPages = totalPages,
                PageSize = PageSize,
                ArticlesThisWeek = articles.Count(a =>
                    a.PublishedDate >= DateTime.UtcNow.AddDays(-7))
            },
            Endpoints = new
            {
                Latest = "articles/latest.json",
                Articles = "articles/page-{page}.json",
                Topics = "topics.json"
            }
        };
        await WriteJsonAsync(Path.Combine(OutputPath, "metadata.json"), metadata);

        return $"""
            ✅ JSON 匯出完成！（分頁結構）
            
            輸出目錄：{OutputPath}
            
            檔案結構：
            ├── metadata.json
            ├── topics.json ({topics.Count} 個主題)
            └── articles/
                ├── latest.json (最新 {Math.Min(PageSize, articles.Count)} 篇)
                └── page-1.json ~ page-{totalPages}.json ({articles.Count} 篇文章)
            
            下一步：執行 SyncToCloud 指令上傳到 Cloudflare R2
            """;
    }

    [McpServerTool, Description("顯示匯出狀態")]
    public Task<string> ExportStatus()
    {
        var metadataPath = Path.Combine(OutputPath, "metadata.json");
        var topicsPath = Path.Combine(OutputPath, "topics.json");
        var articlesDir = Path.Combine(OutputPath, "articles");

        if (!File.Exists(metadataPath))
        {
            return Task.FromResult("尚未匯出任何 JSON 檔案。請先執行 ExportJson。");
        }

        var metadataInfo = new FileInfo(metadataPath);
        var topicsInfo = new FileInfo(topicsPath);
        
        var articleFiles = Directory.Exists(articlesDir) 
            ? Directory.GetFiles(articlesDir, "*.json") 
            : Array.Empty<string>();

        return Task.FromResult($"""
            📁 匯出目錄：{OutputPath}
            
            檔案狀態：
            - metadata.json: {metadataInfo.Length / 1024.0:F1} KB (最後更新: {metadataInfo.LastWriteTime:yyyy-MM-dd HH:mm})
            - topics.json: {topicsInfo.Length / 1024.0:F1} KB (最後更新: {topicsInfo.LastWriteTime:yyyy-MM-dd HH:mm})
            - articles/: {articleFiles.Length} 個檔案
            """);
    }

    private async Task WriteJsonAsync<T>(string path, T data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(path, json);
    }
}
