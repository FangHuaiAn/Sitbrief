namespace Sitbrief.Admin.Models;

public class AIAnalysisResultDto
{
    public List<SuggestedExistingTopicDto> SuggestedExistingTopics { get; set; } = new();
    public List<SuggestedNewTopicDto> SuggestedNewTopics { get; set; } = new();
    public KeyEntitiesDto KeyEntities { get; set; } = new();
    public List<string> GeopoliticalTags { get; set; } = new();
    public int Significance { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class SuggestedExistingTopicDto
{
    public int TopicId { get; set; }
    public float Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SuggestedNewTopicDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class KeyEntitiesDto
{
    public List<string> Countries { get; set; } = new();
    public List<string> Organizations { get; set; } = new();
    public List<string> Persons { get; set; } = new();
}

public class LinkTopicsRequest
{
    public List<int> TopicIds { get; set; } = new();
    public bool Confirmed { get; set; } = true;
}
