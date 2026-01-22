namespace Sitbrief.Core.Entities;

public class ArticleTopic
{
    public int ArticleId { get; set; }
    public int TopicId { get; set; }
    public double Confidence { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime AddedDate { get; set; }

    // Navigation properties
    public Article Article { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
}
