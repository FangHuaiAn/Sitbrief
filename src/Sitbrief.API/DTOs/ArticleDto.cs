using Sitbrief.Core.Enums;

namespace Sitbrief.API.DTOs;

public class ArticleDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string SourceUrl { get; set; }
    public required string SourceName { get; set; }
    public SourceType SourceType { get; set; }
    public DateTime PublishedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<TopicSummaryDto> Topics { get; set; } = new();
}

public class TopicSummaryDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
}
