namespace Sitbrief.Core.Entities;

public class Topic
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Significance { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }

    // Navigation properties
    public ICollection<ArticleTopic> ArticleTopics { get; set; } = new List<ArticleTopic>();
}
