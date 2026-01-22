using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;

namespace Sitbrief.Infrastructure.Services;

public class ClaudeAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeAIService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string ClaudeApiUrl = "https://api.anthropic.com/v1/messages";

    public ClaudeAIService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ClaudeAIService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<AIAnalysisResponse> AnalyzeArticleAsync(Article article, IEnumerable<Topic> existingTopics)
    {
        try
        {
            var apiKey = _configuration["Claude:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return new AIAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = "Claude API key not configured"
                };
            }

            var model = _configuration["Claude:Model"] ?? "claude-3-5-sonnet-20241022";
            var maxTokens = int.Parse(_configuration["Claude:MaxTokens"] ?? "2000");
            var temperature = float.Parse(_configuration["Claude:Temperature"] ?? "0.3");

            var prompt = BuildPrompt(article, existingTopics);
            var requestBody = new ClaudeRequest
            {
                Model = model,
                MaxTokens = maxTokens,
                Messages = new[]
                {
                    new ClaudeMessage { Role = "user", Content = prompt }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, ClaudeApiUrl);
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API error: {StatusCode} - {Response}",
                    response.StatusCode, responseContent);
                return new AIAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = $"Claude API error: {response.StatusCode}"
                };
            }

            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent, _jsonOptions);
            if (claudeResponse?.Content == null || claudeResponse.Content.Length == 0)
            {
                return new AIAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = "Empty response from Claude API"
                };
            }

            var analysisText = claudeResponse.Content[0].Text;
            return ParseAnalysisResponse(analysisText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing article {ArticleId}", article.Id);
            return new AIAnalysisResponse
            {
                Success = false,
                ErrorMessage = $"Analysis failed: {ex.Message}"
            };
        }
    }

    private string BuildPrompt(Article article, IEnumerable<Topic> existingTopics)
    {
        var topicsList = string.Join("\n", existingTopics.Select(t =>
            $"- ID: {t.Id}, 標題: {t.Title}, 描述: {t.Description}"));

        return $@"你是地緣政治分析專家。分析以下文章並提供結構化建議。

文章資訊：
標題：{article.Title}
摘要：{article.Summary}
來源：{article.SourceName}
發布日期：{article.PublishedDate:yyyy-MM-dd}

現有主題列表：
{topicsList}

請以 JSON 格式回覆（只回覆 JSON，不要其他文字）：
{{
  ""suggestedExistingTopics"": [
    {{ ""topicId"": int, ""confidence"": float (0-1), ""reason"": string }}
  ],
  ""suggestedNewTopics"": [
    {{ ""title"": string, ""description"": string }}
  ],
  ""keyEntities"": {{
    ""countries"": [string],
    ""organizations"": [string],
    ""persons"": [string]
  }},
  ""geopoliticalTags"": [string],
  ""significance"": int (1-10),
  ""summary"": string (一句話摘要此文章的地緣政治意義)
}}

注意：
1. suggestedExistingTopics 只能包含上述現有主題列表中的主題 ID
2. confidence 表示此文章與該主題的關聯程度，0-1 之間
3. 如果文章不適合任何現有主題，可以在 suggestedNewTopics 建議新主題
4. significance 表示此文章的地緣政治重要性，1 最低，10 最高";
    }

    private AIAnalysisResponse ParseAnalysisResponse(string responseText)
    {
        try
        {
            // Extract JSON from response (in case there's extra text)
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');
            if (jsonStart == -1 || jsonEnd == -1)
            {
                return new AIAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid response format: no JSON found"
                };
            }

            var jsonText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var parsed = JsonSerializer.Deserialize<ClaudeAnalysisResult>(jsonText, _jsonOptions);

            if (parsed == null)
            {
                return new AIAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to parse analysis result"
                };
            }

            return new AIAnalysisResponse
            {
                Success = true,
                SuggestedExistingTopics = parsed.SuggestedExistingTopics?.Select(t => new SuggestedExistingTopic
                {
                    TopicId = t.TopicId,
                    Confidence = t.Confidence,
                    Reason = t.Reason ?? string.Empty
                }).ToList() ?? new List<SuggestedExistingTopic>(),
                SuggestedNewTopics = parsed.SuggestedNewTopics?.Select(t => new SuggestedNewTopic
                {
                    Title = t.Title ?? string.Empty,
                    Description = t.Description ?? string.Empty
                }).ToList() ?? new List<SuggestedNewTopic>(),
                KeyEntities = new KeyEntities
                {
                    Countries = parsed.KeyEntities?.Countries ?? new List<string>(),
                    Organizations = parsed.KeyEntities?.Organizations ?? new List<string>(),
                    Persons = parsed.KeyEntities?.Persons ?? new List<string>()
                },
                GeopoliticalTags = parsed.GeopoliticalTags ?? new List<string>(),
                Significance = parsed.Significance,
                Summary = parsed.Summary ?? string.Empty
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Claude response: {Response}", responseText);
            return new AIAnalysisResponse
            {
                Success = false,
                ErrorMessage = $"Failed to parse response: {ex.Message}"
            };
        }
    }

    // Claude API request/response models
    private class ClaudeRequest
    {
        public string Model { get; set; } = string.Empty;
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
        public ClaudeMessage[] Messages { get; set; } = Array.Empty<ClaudeMessage>();
    }

    private class ClaudeMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private class ClaudeResponse
    {
        public ClaudeContentBlock[] Content { get; set; } = Array.Empty<ClaudeContentBlock>();
    }

    private class ClaudeContentBlock
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    private class ClaudeAnalysisResult
    {
        public List<ClaudeSuggestedExistingTopic>? SuggestedExistingTopics { get; set; }
        public List<ClaudeSuggestedNewTopic>? SuggestedNewTopics { get; set; }
        public ClaudeKeyEntities? KeyEntities { get; set; }
        public List<string>? GeopoliticalTags { get; set; }
        public int Significance { get; set; }
        public string? Summary { get; set; }
    }

    private class ClaudeSuggestedExistingTopic
    {
        public int TopicId { get; set; }
        public float Confidence { get; set; }
        public string? Reason { get; set; }
    }

    private class ClaudeSuggestedNewTopic
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    private class ClaudeKeyEntities
    {
        public List<string>? Countries { get; set; }
        public List<string>? Organizations { get; set; }
        public List<string>? Persons { get; set; }
    }
}
