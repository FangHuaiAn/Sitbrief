namespace Sitbrief.API.DTOs;

public class AIAnalysisDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public AIAnalysisResultDto Result { get; set; } = new();
    public DateTime AnalyzedDate { get; set; }
}
