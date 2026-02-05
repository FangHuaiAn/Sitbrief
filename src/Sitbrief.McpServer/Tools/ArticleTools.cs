using System.ComponentModel;
using System.Net.Http;
using System.Text.RegularExpressions;
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

    [McpServerTool, Description("從貼上的格式化文字快速新增文章。支援格式：標題：xxx / 來源：xxx / 摘要：xxx / 網址：xxx / 日期：xxx")]
    public async Task<string> QuickAddArticle(
        [Description("貼上的文章資訊（包含標題、來源、摘要等）")] string pastedText,
        [Description("主題 ID 列表，以逗號分隔（可選）")] string? topicIds = null)
    {
        // 解析貼上的文字
        var lines = pastedText.Split('\n');
        string? title = null, sourceName = null, summary = null, sourceUrl = null, dateStr = null;
        var summaryLines = new List<string>();
        var inSummary = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.StartsWith("標題：") || trimmed.StartsWith("標題:"))
            {
                title = trimmed[3..].Trim();
                inSummary = false;
            }
            else if (trimmed.StartsWith("來源：") || trimmed.StartsWith("來源:"))
            {
                sourceName = trimmed[3..].Trim();
                inSummary = false;
            }
            else if (trimmed.StartsWith("網址：") || trimmed.StartsWith("網址:") || trimmed.StartsWith("URL：") || trimmed.StartsWith("URL:"))
            {
                sourceUrl = trimmed.Substring(trimmed.IndexOf('：') + 1).Trim();
                if (sourceUrl.StartsWith(":")) sourceUrl = sourceUrl[1..].Trim();
                inSummary = false;
            }
            else if (trimmed.StartsWith("日期：") || trimmed.StartsWith("日期:"))
            {
                dateStr = trimmed[3..].Trim();
                inSummary = false;
            }
            else if (trimmed.StartsWith("摘要：") || trimmed.StartsWith("摘要:"))
            {
                var summaryStart = trimmed[3..].Trim();
                if (!string.IsNullOrEmpty(summaryStart))
                    summaryLines.Add(summaryStart);
                inSummary = true;
            }
            else if (inSummary)
            {
                summaryLines.Add(trimmed);
            }
        }

        summary = string.Join(" ", summaryLines).Trim();

        // 驗證必要欄位
        if (string.IsNullOrEmpty(title))
            return "❌ 無法解析標題。請確保格式包含「標題：xxx」";

        if (string.IsNullOrEmpty(summary))
            return "❌ 無法解析摘要。請確保格式包含「摘要：xxx」";

        // 設定預設值
        sourceName ??= "Unknown";
        sourceUrl ??= "";
        
        DateTime publishedDate = DateTime.Today;
        if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var parsed))
        {
            publishedDate = parsed;
        }

        // 檢查重複
        if (!string.IsNullOrEmpty(sourceUrl))
        {
            var existing = await _db.Articles.FirstOrDefaultAsync(a => a.SourceUrl == sourceUrl);
            if (existing != null)
                return $"⚠️ 這篇文章已存在！(ID: {existing.Id})\n標題：{existing.Title}";
        }

        // 建立文章
        var article = new Article
        {
            Title = title,
            Summary = summary,
            SourceName = sourceName,
            SourceUrl = sourceUrl,
            SourceType = SourceType.NewsMedia,
            PublishedDate = publishedDate,
            CreatedDate = DateTime.UtcNow
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync();

        // 連結主題
        var linkedTopics = new List<string>();
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
                    linkedTopics.Add(topic.Title);
                }
            }
            await _db.SaveChangesAsync();
        }

        var topicInfo = linkedTopics.Count > 0 
            ? $"主題：{string.Join(", ", linkedTopics)}" 
            : "尚未分類";

        return $"""
            ✅ 文章已新增！(ID: {article.Id})

            標題：{title}
            來源：{sourceName}
            日期：{publishedDate:yyyy-MM-dd}
            {topicInfo}

            下一步：執行 SyncToCloud 同步到雲端
            """;
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

    [McpServerTool, Description("從 URL 抓取網頁資訊，預覽文章內容（不會儲存）")]
    public async Task<string> FetchArticleFromUrl(
        [Description("文章網址")] string url)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36");
            client.Timeout = TimeSpan.FromSeconds(15);
            
            var html = await client.GetStringAsync(url);
            
            // 提取標題
            var title = ExtractMetaContent(html, "og:title") 
                     ?? ExtractMetaContent(html, "twitter:title")
                     ?? ExtractHtmlTitle(html)
                     ?? "無法取得標題";
            
            // 提取描述/摘要
            var description = ExtractMetaContent(html, "og:description")
                           ?? ExtractMetaContent(html, "twitter:description")
                           ?? ExtractMetaContent(html, "description")
                           ?? "無法取得摘要";
            
            // 提取網站名稱
            var siteName = ExtractMetaContent(html, "og:site_name")
                        ?? ExtractDomain(url);
            
            // 提取發布日期
            var publishedDate = ExtractMetaContent(html, "article:published_time")
                             ?? ExtractMetaContent(html, "datePublished")
                             ?? ExtractMetaContent(html, "pubdate")
                             ?? DateTime.Today.ToString("yyyy-MM-dd");
            
            // 嘗試解析日期
            if (DateTime.TryParse(publishedDate, out var date))
            {
                publishedDate = date.ToString("yyyy-MM-dd");
            }
            else
            {
                publishedDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            return $"""
                ✅ 已從網頁抓取以下資訊：

                標題：{title}
                來源：{siteName}
                日期：{publishedDate}
                網址：{url}
                
                摘要：
                {description}

                ---
                如果資訊正確，請告訴我「確認新增」或提供修改建議。
                我會幫你儲存這篇文章。
                """;
        }
        catch (Exception ex)
        {
            return $"❌ 無法抓取網頁：{ex.Message}\n\n請手動提供文章資訊。";
        }
    }

    [McpServerTool, Description("從 URL 直接匯入文章並儲存")]
    public async Task<string> ImportArticleFromUrl(
        [Description("文章網址")] string url,
        [Description("自訂標題（可選，留空使用網頁標題）")] string? customTitle = null,
        [Description("自訂摘要（可選，留空使用網頁描述）")] string? customSummary = null,
        [Description("主題 ID 列表，以逗號分隔（可選）")] string? topicIds = null)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36");
            client.Timeout = TimeSpan.FromSeconds(15);
            
            var html = await client.GetStringAsync(url);
            
            var title = customTitle 
                     ?? ExtractMetaContent(html, "og:title") 
                     ?? ExtractMetaContent(html, "twitter:title")
                     ?? ExtractHtmlTitle(html)
                     ?? "未命名文章";
            
            var summary = customSummary
                       ?? ExtractMetaContent(html, "og:description")
                       ?? ExtractMetaContent(html, "twitter:description")
                       ?? ExtractMetaContent(html, "description")
                       ?? "";
            
            var siteName = ExtractMetaContent(html, "og:site_name")
                        ?? ExtractDomain(url);
            
            var publishedDateStr = ExtractMetaContent(html, "article:published_time")
                                ?? ExtractMetaContent(html, "datePublished");
            
            DateTime publishedDate = DateTime.Today;
            if (!string.IsNullOrEmpty(publishedDateStr) && DateTime.TryParse(publishedDateStr, out var parsed))
            {
                publishedDate = parsed;
            }

            // 檢查是否已存在相同 URL
            var existing = await _db.Articles.FirstOrDefaultAsync(a => a.SourceUrl == url);
            if (existing != null)
            {
                return $"⚠️ 這篇文章已經存在！(ID: {existing.Id})\n標題：{existing.Title}";
            }

            var article = new Article
            {
                Title = title,
                Summary = summary,
                SourceName = siteName,
                SourceUrl = url,
                SourceType = SourceType.NewsMedia,
                PublishedDate = publishedDate,
                CreatedDate = DateTime.UtcNow
            };

            _db.Articles.Add(article);
            await _db.SaveChangesAsync();

            // 連結主題
            var linkedTopics = new List<string>();
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
                        linkedTopics.Add(topic.Title);
                    }
                }
                await _db.SaveChangesAsync();
            }

            var topicInfo = linkedTopics.Count > 0 
                ? $"主題：{string.Join(", ", linkedTopics)}" 
                : "尚未分類";

            return $"""
                ✅ 文章已匯入！(ID: {article.Id})

                標題：{title}
                來源：{siteName}
                日期：{publishedDate:yyyy-MM-dd}
                {topicInfo}

                下一步：執行 SyncToCloud 同步到雲端
                """;
        }
        catch (Exception ex)
        {
            return $"❌ 匯入失敗：{ex.Message}";
        }
    }

    // === 輔助方法 ===

    private static string? ExtractMetaContent(string html, string property)
    {
        // 嘗試 property 屬性
        var pattern = $@"<meta[^>]+(?:property|name)=[""']{property}[""'][^>]+content=[""']([^""']+)[""']";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        if (match.Success) return System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
        
        // 嘗試 content 在前的格式
        pattern = $@"<meta[^>]+content=[""']([^""']+)[""'][^>]+(?:property|name)=[""']{property}[""']";
        match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        if (match.Success) return System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
        
        return null;
    }

    private static string? ExtractHtmlTitle(string html)
    {
        var match = Regex.Match(html, @"<title[^>]*>([^<]+)</title>", RegexOptions.IgnoreCase);
        return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value.Trim()) : null;
    }

    private static string ExtractDomain(string url)
    {
        try
        {
            var uri = new Uri(url);
            var host = uri.Host;
            // 移除 www. 前綴
            if (host.StartsWith("www."))
                host = host[4..];
            return host;
        }
        catch
        {
            return "Unknown";
        }
    }
}
