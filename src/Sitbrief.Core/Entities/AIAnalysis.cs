namespace Sitbrief.Core.Entities;

public class AIAnalysis
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public required string SuggestedTopicsJson { get; set; }
    public required string KeyEntitiesJson { get; set; }
    public required string GeopoliticalTagsJson { get; set; }
    public int SignificanceScore { get; set; }
    public DateTime AnalyzedDate { get; set; }

    // Navigation property
    public Article Article { get; set; } = null!;
}
