using Sitbrief.Core.Entities;
using Sitbrief.Core.Enums;

namespace Sitbrief.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(SitbriefDbContext context)
    {
        // Check if data already exists
        if (context.Articles.Any() || context.Topics.Any())
        {
            return; // Database has been seeded
        }

        // Create topics
        var topic1 = new Topic
        {
            Title = "中美南海爭議",
            Description = "中國與美國在南海地區的軍事對峙和領土爭議",
            Significance = "影響亞太地區軍事平衡和國際航運安全",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        var topic2 = new Topic
        {
            Title = "歐盟能源危機",
            Description = "歐洲各國尋求能源獨立和替代能源的挑戰",
            Significance = "關乎歐洲經濟穩定和能源安全",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        var topic3 = new Topic
        {
            Title = "中東和平進程",
            Description = "以色列與巴勒斯坦的和平談判進展",
            Significance = "影響中東地區長期穩定",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        context.Topics.AddRange(topic1, topic2, topic3);
        await context.SaveChangesAsync();

        // Create articles
        var article1 = new Article
        {
            Title = "美國海軍在南海進行軍事演習",
            Summary = "美國第七艦隊在南海爭議海域進行為期一週的軍事演習，引發中國強烈抗議。演習包括航母打擊群和兩棲作戰訓練。",
            SourceUrl = "https://example.com/article1",
            SourceName = "華爾街日報",
            SourceType = SourceType.NewsMedia,
            PublishedDate = DateTime.UtcNow.AddDays(-2),
            CreatedDate = DateTime.UtcNow
        };

        var article2 = new Article
        {
            Title = "CSIS報告：南海軍事化趨勢分析",
            Summary = "戰略與國際研究中心發布最新報告，分析中美在南海的軍事部署和未來可能的衝突點。報告警告區域緊張局勢升溫。",
            SourceUrl = "https://example.com/article2",
            SourceName = "CSIS",
            SourceType = SourceType.ThinkTank,
            PublishedDate = DateTime.UtcNow.AddDays(-3),
            CreatedDate = DateTime.UtcNow
        };

        var article3 = new Article
        {
            Title = "德國宣布新能源政策",
            Summary = "德國政府宣布加速發展再生能源，計劃在2030年前將再生能源佔比提高至80%。此舉旨在減少對俄羅斯天然氣的依賴。",
            SourceUrl = "https://example.com/article3",
            SourceName = "金融時報",
            SourceType = SourceType.NewsMedia,
            PublishedDate = DateTime.UtcNow.AddDays(-1),
            CreatedDate = DateTime.UtcNow
        };

        var article4 = new Article
        {
            Title = "布魯金斯研究所：歐洲能源轉型挑戰",
            Summary = "布魯金斯研究所發表深度分析，探討歐盟各國在能源轉型過程中面臨的經濟和技術挑戰，以及可能的解決方案。",
            SourceUrl = "https://example.com/article4",
            SourceName = "Brookings Institution",
            SourceType = SourceType.ThinkTank,
            PublishedDate = DateTime.UtcNow.AddDays(-2),
            CreatedDate = DateTime.UtcNow
        };

        context.Articles.AddRange(article1, article2, article3, article4);
        await context.SaveChangesAsync();

        // Create article-topic relationships
        var articleTopic1 = new ArticleTopic
        {
            ArticleId = article1.Id,
            TopicId = topic1.Id,
            Confidence = 0.95f,
            IsConfirmed = true,
            AddedDate = DateTime.UtcNow
        };

        var articleTopic2 = new ArticleTopic
        {
            ArticleId = article2.Id,
            TopicId = topic1.Id,
            Confidence = 0.98f,
            IsConfirmed = true,
            AddedDate = DateTime.UtcNow
        };

        var articleTopic3 = new ArticleTopic
        {
            ArticleId = article3.Id,
            TopicId = topic2.Id,
            Confidence = 0.92f,
            IsConfirmed = true,
            AddedDate = DateTime.UtcNow
        };

        var articleTopic4 = new ArticleTopic
        {
            ArticleId = article4.Id,
            TopicId = topic2.Id,
            Confidence = 0.96f,
            IsConfirmed = true,
            AddedDate = DateTime.UtcNow
        };

        context.ArticleTopics.AddRange(articleTopic1, articleTopic2, articleTopic3, articleTopic4);
        await context.SaveChangesAsync();
    }
}
