using Sitbrief.Core.Enums;

namespace Sitbrief.Core.Entities;

public class Article
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string SourceUrl { get; set; }
    public required string SourceName { get; set; }
    public SourceType SourceType { get; set; }
    public string? Content { get; set; }
    public DateTime PublishedDate { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation properties
    public ICollection<ArticleTopic> ArticleTopics { get; set; } = new List<ArticleTopic>();
    public AIAnalysis? AIAnalysis { get; set; }
}
