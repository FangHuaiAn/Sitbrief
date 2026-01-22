namespace Sitbrief.Admin.Models;

public class TopicDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Significance { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public int ArticleCount { get; set; }
}

public class TopicDetailDto : TopicDto
{
    public List<ArticleDto> Articles { get; set; } = new();
}
