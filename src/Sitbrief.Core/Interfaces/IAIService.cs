using Sitbrief.Core.Entities;

namespace Sitbrief.Core.Interfaces;

public interface IAIService
{
    Task<AIAnalysisResponse> AnalyzeArticleAsync(Article article, IEnumerable<Topic> existingTopics);
}

public class AIAnalysisResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SuggestedExistingTopic> SuggestedExistingTopics { get; set; } = new();
    public List<SuggestedNewTopic> SuggestedNewTopics { get; set; } = new();
    public KeyEntities KeyEntities { get; set; } = new();
    public List<string> GeopoliticalTags { get; set; } = new();
    public int Significance { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class SuggestedExistingTopic
{
    public int TopicId { get; set; }
    public float Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SuggestedNewTopic
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class KeyEntities
{
    public List<string> Countries { get; set; } = new();
    public List<string> Organizations { get; set; } = new();
    public List<string> Persons { get; set; } = new();
}
