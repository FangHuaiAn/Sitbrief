using Sitbrief.Admin.Models;

namespace Sitbrief.Admin.Services;

public interface IApiClient
{
    // Authentication
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    // Articles
    Task<List<ArticleDto>> GetArticlesAsync();
    Task<ArticleDto?> GetArticleAsync(int id);
    Task<ArticleDto?> CreateArticleAsync(CreateArticleDto article);
    Task<bool> UpdateArticleAsync(int id, CreateArticleDto article);
    Task<bool> DeleteArticleAsync(int id);

    // Topics
    Task<List<TopicDto>> GetTopicsAsync();
    Task<TopicDetailDto?> GetTopicAsync(int id);
    Task<TopicDto?> CreateTopicAsync(CreateTopicDto topic);
    Task<bool> UpdateTopicAsync(int id, CreateTopicDto topic);
    Task<bool> DeleteTopicAsync(int id);

    // AI Analysis
    Task<AIAnalysisResultDto?> AnalyzeArticleAsync(int articleId);
    Task<bool> LinkArticleTopicsAsync(int articleId, List<int> topicIds, bool confirmed = true);
}
