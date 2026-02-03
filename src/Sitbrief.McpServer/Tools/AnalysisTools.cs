using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Sitbrief.Core.Entities;
using Sitbrief.Infrastructure.Data;

namespace Sitbrief.McpServer.Tools;

[McpServerToolType]
public class AnalysisTools
{
    private readonly SitbriefDbContext _db;

    public AnalysisTools(SitbriefDbContext db)
    {
        _db = db;
    }

    [McpServerTool, Description("取得文章與所有現有主題供 AI 分析（用於文章分類）")]
    public async Task<string> GetArticleForAnalysis(
        [Description("文章 ID")] int articleId)
    {
        var article = await _db.Articles
            .Include(a => a.ArticleTopics)
            .ThenInclude(at => at.Topic)
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
        {
            return $"找不到 ID 為 {articleId} 的文章。";
        }

        var allTopics = await _db.Topics
            .OrderBy(t => t.Title)
            .Select(t => new { t.Id, t.Title, t.Description })
            .ToListAsync();

        var topicsList = string.Join("\n", allTopics.Select(t =>
            $"- ID: {t.Id} | 標題: {t.Title} | 描述: {t.Description}"));

        var currentTopics = string.Join(", ", article.ArticleTopics.Select(at => at.Topic.Title));

        return $"""
            === 待分析文章 ===
            ID: {article.Id}
            標題: {article.Title}
            來源: {article.SourceName}
            日期: {article.PublishedDate:yyyy-MM-dd}
            目前主題: {(string.IsNullOrEmpty(currentTopics) ? "未分類" : currentTopics)}
            
            摘要:
            {article.Summary}
            
            === 現有主題列表 ({allTopics.Count} 個) ===
            {topicsList}
            
            === 請進行分析 ===
            請分析這篇文章：
            1. 建議連結到哪些現有主題（使用上述 ID）
            2. 評估地緣政治重要性（1-10 分）
            3. 提取關鍵實體（國家、組織、人物）
            4. 提供一句話摘要
            5. 如果需要新主題，請建議
            
            分析完成後，請使用 SaveAnalysis 工具儲存結果。
            """;
    }

    [McpServerTool, Description("儲存 AI 分析結果到文章")]
    public async Task<string> SaveAnalysis(
        [Description("文章 ID")] int articleId,
        [Description("地緣政治重要性 (1-10)")] int significance,
        [Description("建議主題（JSON 格式）")] string suggestedTopicsJson,
        [Description("關鍵實體（JSON 格式）")] string keyEntitiesJson,
        [Description("地緣政治標籤（JSON 格式）")] string geopoliticalTagsJson)
    {
        var article = await _db.Articles
            .Include(a => a.AIAnalysis)
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
        {
            return $"找不到 ID 為 {articleId} 的文章。";
        }

        if (article.AIAnalysis == null)
        {
            article.AIAnalysis = new AIAnalysis
            {
                ArticleId = articleId,
                SuggestedTopicsJson = suggestedTopicsJson,
                KeyEntitiesJson = keyEntitiesJson,
                GeopoliticalTagsJson = geopoliticalTagsJson,
                SignificanceScore = Math.Clamp(significance, 1, 10),
                AnalyzedDate = DateTime.UtcNow
            };
            _db.Set<AIAnalysis>().Add(article.AIAnalysis);
        }
        else
        {
            article.AIAnalysis.SignificanceScore = Math.Clamp(significance, 1, 10);
            article.AIAnalysis.SuggestedTopicsJson = suggestedTopicsJson;
            article.AIAnalysis.KeyEntitiesJson = keyEntitiesJson;
            article.AIAnalysis.GeopoliticalTagsJson = geopoliticalTagsJson;
            article.AIAnalysis.AnalyzedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return $"""
            ✅ 已儲存文章「{article.Title}」的 AI 分析結果
            
            重要性: {significance}/10
            建議主題: {suggestedTopicsJson}
            關鍵實體: {keyEntitiesJson}
            標籤: {geopoliticalTagsJson}
            """;
    }

    [McpServerTool, Description("批次取得未分析的文章")]
    public async Task<string> GetUnanalyzedArticles(
        [Description("最大回傳數量")] int limit = 10)
    {
        var articles = await _db.Articles
            .Where(a => a.AIAnalysis == null)
            .OrderByDescending(a => a.PublishedDate)
            .Take(limit)
            .Select(a => new { a.Id, a.Title, a.PublishedDate, a.SourceName })
            .ToListAsync();

        if (articles.Count == 0)
        {
            return "所有文章都已分析完畢！";
        }

        var lines = articles.Select(a =>
            $"- [{a.Id}] {a.Title} ({a.SourceName}, {a.PublishedDate:yyyy-MM-dd})");

        return $"""
            找到 {articles.Count} 篇未分析的文章：
            
            {string.Join("\n", lines)}
            
            使用 GetArticleForAnalysis 來分析特定文章。
            """;
    }
}
