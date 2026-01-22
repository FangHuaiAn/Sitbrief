# Sitbrief Phase 3: AI Integration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Integrate Claude API for AI-powered article analysis, providing topic suggestions and geopolitical insights to streamline the content management workflow.

**Architecture:** Add a Claude AI service in the Core layer that analyzes articles and returns structured suggestions. The API exposes an analyze endpoint that triggers AI analysis, stores results in AIAnalysis table, and returns suggestions. The Blazor Admin UI adds an "AI Analyze" button that displays suggestions and allows the admin to accept/modify them.

**Tech Stack:** Anthropic Claude API (claude-3-5-sonnet), HttpClient, System.Text.Json, ASP.NET Core 8.0, Blazor WebAssembly

---

## Prerequisites

- Phase 1 and Phase 2 completed
- Claude API key (obtain from console.anthropic.com)
- API running on http://localhost:5167

---

## Task 1: Add Claude API Configuration

**Files:**
- Modify: `src/Sitbrief.API/appsettings.json`
- Modify: `src/Sitbrief.API/appsettings.Development.json`

**Step 1: Add Claude configuration to appsettings.json**

Edit file: `src/Sitbrief.API/appsettings.json`

Add after the "Authentication" section:

```json
  "Claude": {
    "ApiKey": "",
    "Model": "claude-3-5-sonnet-20241022",
    "MaxTokens": 2000,
    "Temperature": 0.3
  },
```

Note: ApiKey should be empty in committed file. Use environment variable `Claude__ApiKey` in production.

**Step 2: Add Claude configuration to appsettings.Development.json**

Edit file: `src/Sitbrief.API/appsettings.Development.json`

Add the same Claude section. For local development, you can set the API key here (but don't commit it):

```json
  "Claude": {
    "ApiKey": "your-api-key-here",
    "Model": "claude-3-5-sonnet-20241022",
    "MaxTokens": 2000,
    "Temperature": 0.3
  },
```

**Step 3: Verify configuration loads**

```bash
cd src/Sitbrief.API
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 4: Commit (without API key)**

```bash
git add src/Sitbrief.API/appsettings.json
git commit -m "feat(api): add Claude API configuration structure

- Add Claude configuration section to appsettings
- Configure model, max tokens, and temperature
- API key loaded from environment variable

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 2: Create AI Analysis DTOs

**Files:**
- Create: `src/Sitbrief.API/DTOs/AIAnalysisDto.cs`
- Create: `src/Sitbrief.API/DTOs/AIAnalysisResultDto.cs`

**Step 1: Create AIAnalysisResultDto for API response**

Create file: `src/Sitbrief.API/DTOs/AIAnalysisResultDto.cs`

```csharp
namespace Sitbrief.API.DTOs;

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
```

**Step 2: Create AIAnalysisDto for stored analysis**

Create file: `src/Sitbrief.API/DTOs/AIAnalysisDto.cs`

```csharp
namespace Sitbrief.API.DTOs;

public class AIAnalysisDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public AIAnalysisResultDto Result { get; set; } = new();
    public DateTime AnalyzedDate { get; set; }
}
```

**Step 3: Build to verify**

```bash
cd src
dotnet build Sitbrief.API
```

Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add src/Sitbrief.API/DTOs/AIAnalysisDto.cs src/Sitbrief.API/DTOs/AIAnalysisResultDto.cs
git commit -m "feat(api): add AI analysis DTOs

- Add AIAnalysisResultDto for Claude API response
- Add SuggestedExistingTopicDto and SuggestedNewTopicDto
- Add KeyEntitiesDto for extracted entities
- Add AIAnalysisDto for stored analysis

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 3: Create IAIService Interface

**Files:**
- Create: `src/Sitbrief.Core/Interfaces/IAIService.cs`

**Step 1: Create IAIService interface**

Create file: `src/Sitbrief.Core/Interfaces/IAIService.cs`

```csharp
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
```

**Step 2: Build to verify**

```bash
dotnet build Sitbrief.Core
```

Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add src/Sitbrief.Core/Interfaces/IAIService.cs
git commit -m "feat(core): add IAIService interface for AI analysis

- Define IAIService with AnalyzeArticleAsync method
- Add AIAnalysisResponse with structured result
- Add supporting classes for topics and entities

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 4: Implement ClaudeAIService

**Files:**
- Create: `src/Sitbrief.Infrastructure/Services/ClaudeAIService.cs`
- Modify: `src/Sitbrief.Infrastructure/Sitbrief.Infrastructure.csproj`

**Step 1: Add HttpClient package (if needed)**

```bash
cd src/Sitbrief.Infrastructure
dotnet list package | grep -i http || echo "HttpClient is built-in"
```

No additional package needed - HttpClient is part of .NET.

**Step 2: Create ClaudeAIService**

Create file: `src/Sitbrief.Infrastructure/Services/ClaudeAIService.cs`

```csharp
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
```

**Step 3: Build to verify**

```bash
cd ..
dotnet build Sitbrief.Infrastructure
```

Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add src/Sitbrief.Infrastructure/Services/ClaudeAIService.cs
git commit -m "feat(infrastructure): implement ClaudeAIService

- Add Claude API integration with HttpClient
- Build geopolitical analysis prompt in Chinese
- Parse structured JSON response
- Handle API errors gracefully
- Support configuration for model, tokens, temperature

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 5: Register AI Service in DI Container

**Files:**
- Modify: `src/Sitbrief.API/Program.cs`

**Step 1: Register IAIService**

Edit file: `src/Sitbrief.API/Program.cs`

Add after the repository registrations (around line 20):

```csharp
// Add AI service
builder.Services.AddHttpClient<IAIService, ClaudeAIService>();
```

Add the using statement at top:

```csharp
using Sitbrief.Infrastructure.Services;
```

**Step 2: Build to verify**

```bash
cd Sitbrief.API
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add src/Sitbrief.API/Program.cs
git commit -m "feat(api): register ClaudeAIService in DI container

- Add HttpClient registration for IAIService
- Configure ClaudeAIService as implementation

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 6: Add Analyze Endpoint to ArticlesController

**Files:**
- Modify: `src/Sitbrief.API/Controllers/ArticlesController.cs`

**Step 1: Add IAIService and ITopicRepository dependencies**

Edit file: `src/Sitbrief.API/Controllers/ArticlesController.cs`

Update the constructor and fields:

```csharp
private readonly IArticleRepository _articleRepository;
private readonly ITopicRepository _topicRepository;
private readonly IAIService _aiService;
private readonly ILogger<ArticlesController> _logger;

public ArticlesController(
    IArticleRepository articleRepository,
    ITopicRepository topicRepository,
    IAIService aiService,
    ILogger<ArticlesController> logger)
{
    _articleRepository = articleRepository;
    _topicRepository = topicRepository;
    _aiService = aiService;
    _logger = logger;
}
```

**Step 2: Add the analyze endpoint**

Add after the Delete endpoint (before the MapToDto method):

```csharp
[HttpPost("{id}/analyze")]
public async Task<ActionResult<AIAnalysisResultDto>> AnalyzeArticle(int id)
{
    try
    {
        var article = await _articleRepository.GetByIdAsync(id);
        if (article == null)
        {
            return NotFound($"Article with ID {id} not found");
        }

        var topics = await _topicRepository.GetAllAsync();
        var result = await _aiService.AnalyzeArticleAsync(article, topics);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        var dto = new AIAnalysisResultDto
        {
            SuggestedExistingTopics = result.SuggestedExistingTopics.Select(t => new SuggestedExistingTopicDto
            {
                TopicId = t.TopicId,
                Confidence = t.Confidence,
                Reason = t.Reason
            }).ToList(),
            SuggestedNewTopics = result.SuggestedNewTopics.Select(t => new SuggestedNewTopicDto
            {
                Title = t.Title,
                Description = t.Description
            }).ToList(),
            KeyEntities = new KeyEntitiesDto
            {
                Countries = result.KeyEntities.Countries,
                Organizations = result.KeyEntities.Organizations,
                Persons = result.KeyEntities.Persons
            },
            GeopoliticalTags = result.GeopoliticalTags,
            Significance = result.Significance,
            Summary = result.Summary
        };

        return Ok(dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error analyzing article {ArticleId}", id);
        return StatusCode(500, "An error occurred while analyzing the article");
    }
}
```

**Step 3: Build to verify**

```bash
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add src/Sitbrief.API/Controllers/ArticlesController.cs
git commit -m "feat(api): add POST /api/articles/{id}/analyze endpoint

- Add AI analysis endpoint for articles
- Inject IAIService and ITopicRepository
- Return structured analysis result
- Handle errors gracefully

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 7: Add Article-Topic Linking Endpoint

**Files:**
- Modify: `src/Sitbrief.API/Controllers/ArticlesController.cs`
- Create: `src/Sitbrief.API/DTOs/LinkTopicsDto.cs`

**Step 1: Create LinkTopicsDto**

Create file: `src/Sitbrief.API/DTOs/LinkTopicsDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Sitbrief.API.DTOs;

public class LinkTopicsDto
{
    [Required]
    public List<int> TopicIds { get; set; } = new();

    public bool Confirmed { get; set; } = true;
}
```

**Step 2: Add IArticleRepository method for linking**

Edit file: `src/Sitbrief.Core/Interfaces/IArticleRepository.cs`

Add new method:

```csharp
Task LinkTopicsAsync(int articleId, IEnumerable<int> topicIds, bool confirmed = true);
```

**Step 3: Implement in ArticleRepository**

Edit file: `src/Sitbrief.Infrastructure/Repositories/ArticleRepository.cs`

Add after the ExistsAsync method:

```csharp
public async Task LinkTopicsAsync(int articleId, IEnumerable<int> topicIds, bool confirmed = true)
{
    // Remove existing links
    var existingLinks = _context.ArticleTopics.Where(at => at.ArticleId == articleId);
    _context.ArticleTopics.RemoveRange(existingLinks);

    // Add new links
    foreach (var topicId in topicIds)
    {
        _context.ArticleTopics.Add(new ArticleTopic
        {
            ArticleId = articleId,
            TopicId = topicId,
            Confidence = 1.0f,
            IsConfirmed = confirmed,
            AddedDate = DateTime.UtcNow
        });
    }

    await _context.SaveChangesAsync();
}
```

**Step 4: Add link topics endpoint to ArticlesController**

Add after the analyze endpoint:

```csharp
[HttpPost("{id}/topics")]
public async Task<IActionResult> LinkTopics(int id, LinkTopicsDto dto)
{
    try
    {
        var exists = await _articleRepository.ExistsAsync(id);
        if (!exists)
        {
            return NotFound($"Article with ID {id} not found");
        }

        await _articleRepository.LinkTopicsAsync(id, dto.TopicIds, dto.Confirmed);
        return NoContent();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error linking topics to article {ArticleId}", id);
        return StatusCode(500, "An error occurred while linking topics");
    }
}
```

**Step 5: Build to verify**

```bash
cd ..
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 6: Commit**

```bash
git add src/Sitbrief.API/DTOs/LinkTopicsDto.cs src/Sitbrief.Core/Interfaces/IArticleRepository.cs src/Sitbrief.Infrastructure/Repositories/ArticleRepository.cs src/Sitbrief.API/Controllers/ArticlesController.cs
git commit -m "feat(api): add POST /api/articles/{id}/topics endpoint for linking

- Add LinkTopicsDto for request body
- Add LinkTopicsAsync to IArticleRepository
- Implement topic linking in ArticleRepository
- Add endpoint to replace article-topic links

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 8: Add AI Analysis Models to Blazor Admin

**Files:**
- Create: `src/Sitbrief.Admin/Models/AIAnalysisResultDto.cs`

**Step 1: Create AI analysis models for Blazor**

Create file: `src/Sitbrief.Admin/Models/AIAnalysisResultDto.cs`

```csharp
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
```

**Step 2: Build to verify**

```bash
cd src
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add src/Sitbrief.Admin/Models/AIAnalysisResultDto.cs
git commit -m "feat(admin): add AI analysis models

- Add AIAnalysisResultDto for API response
- Add SuggestedExistingTopicDto and SuggestedNewTopicDto
- Add KeyEntitiesDto for entities
- Add LinkTopicsRequest for linking

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 9: Add AI Methods to ApiClient

**Files:**
- Modify: `src/Sitbrief.Admin/Services/IApiClient.cs`
- Modify: `src/Sitbrief.Admin/Services/ApiClient.cs`

**Step 1: Add methods to IApiClient**

Edit file: `src/Sitbrief.Admin/Services/IApiClient.cs`

Add new methods:

```csharp
// AI Analysis
Task<AIAnalysisResultDto?> AnalyzeArticleAsync(int articleId);
Task<bool> LinkArticleTopicsAsync(int articleId, List<int> topicIds, bool confirmed = true);
```

**Step 2: Implement in ApiClient**

Edit file: `src/Sitbrief.Admin/Services/ApiClient.cs`

Add after the existing article methods:

```csharp
// AI Analysis
public async Task<AIAnalysisResultDto?> AnalyzeArticleAsync(int articleId)
{
    await SetAuthHeaderAsync();
    var response = await _httpClient.PostAsync($"/api/articles/{articleId}/analyze", null);
    if (response.IsSuccessStatusCode)
    {
        return await response.Content.ReadFromJsonAsync<AIAnalysisResultDto>(_jsonOptions);
    }
    return null;
}

public async Task<bool> LinkArticleTopicsAsync(int articleId, List<int> topicIds, bool confirmed = true)
{
    await SetAuthHeaderAsync();
    var request = new LinkTopicsRequest
    {
        TopicIds = topicIds,
        Confirmed = confirmed
    };
    var response = await _httpClient.PostAsJsonAsync($"/api/articles/{articleId}/topics", request);
    return response.IsSuccessStatusCode;
}
```

**Step 3: Build to verify**

```bash
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add src/Sitbrief.Admin/Services/IApiClient.cs src/Sitbrief.Admin/Services/ApiClient.cs
git commit -m "feat(admin): add AI analysis methods to ApiClient

- Add AnalyzeArticleAsync for AI analysis
- Add LinkArticleTopicsAsync for topic linking
- Update IApiClient interface

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 10: Update Add Article Page with AI Analysis UI

**Files:**
- Modify: `src/Sitbrief.Admin/Pages/Articles/Add.razor`

**Step 1: Replace the Add.razor file with AI-enabled version**

Edit file: `src/Sitbrief.Admin/Pages/Articles/Add.razor`

Replace entire content with:

```razor
@page "/articles/add"
@using Sitbrief.Admin.Models
@using Sitbrief.Admin.Services
@inject IApiClient ApiClient
@inject NavigationManager Navigation

<PageTitle>新增文章 - Sitbrief</PageTitle>

<div class="page-header">
    <h1>新增文章</h1>
</div>

<div class="form-container">
    <EditForm Model="@article" OnValidSubmit="HandleSubmit">
        <DataAnnotationsValidator />

        <div class="form-group">
            <label for="title">標題 *</label>
            <InputText id="title" @bind-Value="article.Title" class="form-control" />
            <ValidationMessage For="@(() => article.Title)" />
        </div>

        <div class="form-group">
            <label for="summary">摘要 *</label>
            <InputTextArea id="summary" @bind-Value="article.Summary" class="form-control" rows="4" />
            <ValidationMessage For="@(() => article.Summary)" />
        </div>

        <div class="form-group">
            <label for="sourceUrl">來源網址 *</label>
            <InputText id="sourceUrl" @bind-Value="article.SourceUrl" class="form-control" />
            <ValidationMessage For="@(() => article.SourceUrl)" />
        </div>

        <div class="form-row">
            <div class="form-group">
                <label for="sourceName">媒體/智庫名稱 *</label>
                <InputText id="sourceName" @bind-Value="article.SourceName" class="form-control" />
                <ValidationMessage For="@(() => article.SourceName)" />
            </div>

            <div class="form-group">
                <label for="sourceType">來源類型 *</label>
                <InputSelect id="sourceType" @bind-Value="article.SourceType" class="form-control">
                    <option value="0">新聞媒體</option>
                    <option value="1">智庫</option>
                </InputSelect>
                <ValidationMessage For="@(() => article.SourceType)" />
            </div>
        </div>

        <div class="form-group">
            <label for="publishedDate">發布日期 *</label>
            <InputDate id="publishedDate" @bind-Value="article.PublishedDate" class="form-control" />
            <ValidationMessage For="@(() => article.PublishedDate)" />
        </div>

        <div class="form-group">
            <label for="content">完整內容（選填）</label>
            <InputTextArea id="content" @bind-Value="article.Content" class="form-control" rows="6" />
            <ValidationMessage For="@(() => article.Content)" />
        </div>

        @* AI Analysis Section *@
        @if (createdArticleId.HasValue)
        {
            <div class="ai-section">
                <div class="ai-header">
                    <h3>AI 分析</h3>
                    <button type="button" class="btn btn-ai" @onclick="RunAIAnalysis" disabled="@isAnalyzing">
                        @if (isAnalyzing)
                        {
                            <span>分析中...</span>
                        }
                        else
                        {
                            <span>🤖 AI 分析</span>
                        }
                    </button>
                </div>

                @if (analysisResult != null)
                {
                    <div class="analysis-result">
                        @* Summary *@
                        <div class="result-section">
                            <h4>📝 AI 摘要</h4>
                            <p class="ai-summary">@analysisResult.Summary</p>
                        </div>

                        @* Significance *@
                        <div class="result-section">
                            <h4>⭐ 重要性評分</h4>
                            <div class="significance-bar">
                                <div class="significance-fill" style="width: @(analysisResult.Significance * 10)%"></div>
                                <span class="significance-label">@analysisResult.Significance / 10</span>
                            </div>
                        </div>

                        @* Suggested Existing Topics *@
                        @if (analysisResult.SuggestedExistingTopics.Any())
                        {
                            <div class="result-section">
                                <h4>📌 建議關聯的現有主題</h4>
                                <div class="suggested-topics">
                                    @foreach (var topic in analysisResult.SuggestedExistingTopics)
                                    {
                                        var topicInfo = availableTopics.FirstOrDefault(t => t.Id == topic.TopicId);
                                        if (topicInfo != null)
                                        {
                                            <label class="topic-suggestion">
                                                <input type="checkbox"
                                                       checked="@selectedTopicIds.Contains(topic.TopicId)"
                                                       @onchange="@(e => ToggleTopic(topic.TopicId, (bool)e.Value!))" />
                                                <span class="topic-info">
                                                    <span class="topic-title">@topicInfo.Title</span>
                                                    <span class="confidence-badge">信心度: @(topic.Confidence * 100:F0)%</span>
                                                </span>
                                                <span class="topic-reason">@topic.Reason</span>
                                            </label>
                                        }
                                    }
                                </div>
                            </div>
                        }

                        @* Suggested New Topics *@
                        @if (analysisResult.SuggestedNewTopics.Any())
                        {
                            <div class="result-section">
                                <h4>💡 建議建立的新主題</h4>
                                <div class="new-topics">
                                    @foreach (var newTopic in analysisResult.SuggestedNewTopics)
                                    {
                                        <div class="new-topic-card">
                                            <strong>@newTopic.Title</strong>
                                            <p>@newTopic.Description</p>
                                            <button type="button" class="btn btn-sm btn-secondary"
                                                    @onclick="@(() => CreateAndSelectTopic(newTopic))">
                                                建立此主題
                                            </button>
                                        </div>
                                    }
                                </div>
                            </div>
                        }

                        @* Key Entities *@
                        <div class="result-section">
                            <h4>🏷️ 關鍵實體</h4>
                            <div class="entities">
                                @if (analysisResult.KeyEntities.Countries.Any())
                                {
                                    <div class="entity-group">
                                        <span class="entity-label">國家：</span>
                                        @foreach (var country in analysisResult.KeyEntities.Countries)
                                        {
                                            <span class="entity-tag country">@country</span>
                                        }
                                    </div>
                                }
                                @if (analysisResult.KeyEntities.Organizations.Any())
                                {
                                    <div class="entity-group">
                                        <span class="entity-label">組織：</span>
                                        @foreach (var org in analysisResult.KeyEntities.Organizations)
                                        {
                                            <span class="entity-tag org">@org</span>
                                        }
                                    </div>
                                }
                                @if (analysisResult.KeyEntities.Persons.Any())
                                {
                                    <div class="entity-group">
                                        <span class="entity-label">人物：</span>
                                        @foreach (var person in analysisResult.KeyEntities.Persons)
                                        {
                                            <span class="entity-tag person">@person</span>
                                        }
                                    </div>
                                }
                            </div>
                        </div>

                        @* Geopolitical Tags *@
                        @if (analysisResult.GeopoliticalTags.Any())
                        {
                            <div class="result-section">
                                <h4>🌍 地緣政治標籤</h4>
                                <div class="geo-tags">
                                    @foreach (var tag in analysisResult.GeopoliticalTags)
                                    {
                                        <span class="geo-tag">@tag</span>
                                    }
                                </div>
                            </div>
                        }
                    </div>
                }
                else if (!string.IsNullOrEmpty(analysisError))
                {
                    <div class="alert alert-warning">@analysisError</div>
                }
            </div>
        }

        @* Manual Topic Selection (shown when no AI analysis yet) *@
        @if (!createdArticleId.HasValue || analysisResult == null)
        {
            <div class="form-group">
                <label>關聯主題（選填）</label>
                @if (loadingTopics)
                {
                    <p>載入主題中...</p>
                }
                else if (availableTopics.Any())
                {
                    <div class="topics-list">
                        @foreach (var topic in availableTopics)
                        {
                            <label class="topic-checkbox">
                                <input type="checkbox"
                                       checked="@selectedTopicIds.Contains(topic.Id)"
                                       @onchange="@(e => ToggleTopic(topic.Id, (bool)e.Value!))" />
                                <span>@topic.Title</span>
                            </label>
                        }
                    </div>
                }
                else
                {
                    <p class="text-muted">尚無可用主題，請先在<a href="/topics">主題管理</a>中建立主題。</p>
                }
            </div>
        }

        @if (!string.IsNullOrEmpty(errorMessage))
        {
            <div class="alert alert-danger">@errorMessage</div>
        }

        @if (!string.IsNullOrEmpty(successMessage))
        {
            <div class="alert alert-success">@successMessage</div>
        }

        <div class="form-actions">
            <button type="button" class="btn btn-secondary" @onclick="Cancel">取消</button>
            @if (!createdArticleId.HasValue)
            {
                <button type="submit" class="btn btn-primary" disabled="@isSubmitting">
                    @if (isSubmitting)
                    {
                        <span>儲存中...</span>
                    }
                    else
                    {
                        <span>儲存文章</span>
                    }
                </button>
            }
            else
            {
                <button type="button" class="btn btn-primary" @onclick="SaveWithTopics" disabled="@isSubmitting">
                    @if (isSubmitting)
                    {
                        <span>儲存中...</span>
                    }
                    else
                    {
                        <span>確認並完成</span>
                    }
                </button>
            }
        </div>
    </EditForm>
</div>

<style>
    .page-header {
        margin-bottom: 1.5rem;
    }

    .form-container {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        max-width: 900px;
    }

    .form-group {
        margin-bottom: 1.5rem;
    }

    .form-group label {
        display: block;
        margin-bottom: 0.5rem;
        font-weight: 500;
        color: #333;
    }

    .form-control {
        width: 100%;
        padding: 0.5rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 1rem;
    }

    .form-control:focus {
        outline: none;
        border-color: #007bff;
        box-shadow: 0 0 0 3px rgba(0,123,255,0.1);
    }

    .form-row {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 1rem;
    }

    /* AI Section Styles */
    .ai-section {
        margin: 2rem 0;
        padding: 1.5rem;
        background: #f8f9fa;
        border-radius: 8px;
        border: 1px solid #e0e0e0;
    }

    .ai-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1rem;
    }

    .ai-header h3 {
        margin: 0;
    }

    .btn-ai {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        border: none;
        padding: 0.5rem 1rem;
        border-radius: 4px;
        cursor: pointer;
        font-size: 1rem;
    }

    .btn-ai:hover:not(:disabled) {
        opacity: 0.9;
    }

    .btn-ai:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }

    .analysis-result {
        margin-top: 1rem;
    }

    .result-section {
        margin-bottom: 1.5rem;
        padding-bottom: 1rem;
        border-bottom: 1px solid #e0e0e0;
    }

    .result-section:last-child {
        border-bottom: none;
        margin-bottom: 0;
        padding-bottom: 0;
    }

    .result-section h4 {
        margin: 0 0 0.75rem;
        font-size: 1rem;
        color: #333;
    }

    .ai-summary {
        background: white;
        padding: 1rem;
        border-radius: 4px;
        border-left: 3px solid #667eea;
        margin: 0;
    }

    .significance-bar {
        background: #e0e0e0;
        border-radius: 4px;
        height: 24px;
        position: relative;
        overflow: hidden;
    }

    .significance-fill {
        background: linear-gradient(90deg, #4caf50, #ff9800, #f44336);
        height: 100%;
        transition: width 0.3s;
    }

    .significance-label {
        position: absolute;
        right: 8px;
        top: 50%;
        transform: translateY(-50%);
        font-weight: 500;
        color: #333;
    }

    .suggested-topics {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .topic-suggestion {
        display: flex;
        align-items: flex-start;
        gap: 0.75rem;
        padding: 0.75rem;
        background: white;
        border-radius: 4px;
        cursor: pointer;
    }

    .topic-suggestion input {
        margin-top: 4px;
    }

    .topic-info {
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }

    .topic-title {
        font-weight: 500;
    }

    .confidence-badge {
        background: #e3f2fd;
        color: #1976d2;
        padding: 0.125rem 0.5rem;
        border-radius: 4px;
        font-size: 0.75rem;
    }

    .topic-reason {
        font-size: 0.875rem;
        color: #666;
        display: block;
        margin-top: 0.25rem;
    }

    .new-topics {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
    }

    .new-topic-card {
        background: white;
        padding: 1rem;
        border-radius: 4px;
        border: 1px dashed #ccc;
    }

    .new-topic-card strong {
        display: block;
        margin-bottom: 0.25rem;
    }

    .new-topic-card p {
        margin: 0 0 0.5rem;
        color: #666;
        font-size: 0.875rem;
    }

    .entities {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .entity-group {
        display: flex;
        align-items: center;
        flex-wrap: wrap;
        gap: 0.5rem;
    }

    .entity-label {
        font-weight: 500;
        min-width: 60px;
    }

    .entity-tag {
        padding: 0.25rem 0.5rem;
        border-radius: 4px;
        font-size: 0.875rem;
    }

    .entity-tag.country {
        background: #e8f5e9;
        color: #2e7d32;
    }

    .entity-tag.org {
        background: #e3f2fd;
        color: #1565c0;
    }

    .entity-tag.person {
        background: #fce4ec;
        color: #c2185b;
    }

    .geo-tags {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
    }

    .geo-tag {
        background: #fff3e0;
        color: #e65100;
        padding: 0.25rem 0.75rem;
        border-radius: 16px;
        font-size: 0.875rem;
    }

    /* Topics List */
    .topics-list {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        max-height: 200px;
        overflow-y: auto;
    }

    .topic-checkbox {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        cursor: pointer;
    }

    .topic-checkbox input[type="checkbox"] {
        cursor: pointer;
    }

    .text-muted {
        color: #666;
        font-size: 0.875rem;
    }

    .form-actions {
        display: flex;
        gap: 1rem;
        justify-content: flex-end;
        margin-top: 2rem;
        padding-top: 1rem;
        border-top: 1px solid #ddd;
    }

    .btn {
        padding: 0.5rem 1.5rem;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 1rem;
    }

    .btn-primary {
        background-color: #007bff;
        color: white;
    }

    .btn-primary:hover:not(:disabled) {
        background-color: #0056b3;
    }

    .btn-secondary {
        background-color: #6c757d;
        color: white;
    }

    .btn-secondary:hover {
        background-color: #5a6268;
    }

    .btn-sm {
        padding: 0.25rem 0.5rem;
        font-size: 0.875rem;
    }

    .btn:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }

    .alert {
        padding: 1rem;
        border-radius: 4px;
        margin-bottom: 1rem;
    }

    .alert-danger {
        background-color: #f8d7da;
        color: #721c24;
        border: 1px solid #f5c6cb;
    }

    .alert-success {
        background-color: #d4edda;
        color: #155724;
        border: 1px solid #c3e6cb;
    }

    .alert-warning {
        background-color: #fff3cd;
        color: #856404;
        border: 1px solid #ffeeba;
    }
</style>

@code {
    private CreateArticleDto article = new();
    private List<TopicDto> availableTopics = new();
    private HashSet<int> selectedTopicIds = new();
    private bool isSubmitting = false;
    private bool loadingTopics = true;
    private bool isAnalyzing = false;
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;
    private string analysisError = string.Empty;

    private int? createdArticleId = null;
    private AIAnalysisResultDto? analysisResult = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadTopicsAsync();
    }

    private async Task LoadTopicsAsync()
    {
        try
        {
            loadingTopics = true;
            availableTopics = await ApiClient.GetTopicsAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"載入主題時發生錯誤：{ex.Message}";
        }
        finally
        {
            loadingTopics = false;
        }
    }

    private void ToggleTopic(int topicId, bool isChecked)
    {
        if (isChecked)
        {
            selectedTopicIds.Add(topicId);
        }
        else
        {
            selectedTopicIds.Remove(topicId);
        }
    }

    private async Task HandleSubmit()
    {
        try
        {
            isSubmitting = true;
            errorMessage = string.Empty;
            successMessage = string.Empty;

            var createdArticle = await ApiClient.CreateArticleAsync(article);

            if (createdArticle != null)
            {
                createdArticleId = createdArticle.Id;
                successMessage = "文章已建立！您可以點擊「AI 分析」按鈕讓 AI 建議關聯主題，或直接選擇主題後點擊「確認並完成」。";
            }
            else
            {
                errorMessage = "建立文章失敗，請稍後再試。";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"建立文章時發生錯誤：{ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private async Task RunAIAnalysis()
    {
        if (!createdArticleId.HasValue) return;

        try
        {
            isAnalyzing = true;
            analysisError = string.Empty;
            analysisResult = null;

            var result = await ApiClient.AnalyzeArticleAsync(createdArticleId.Value);

            if (result != null)
            {
                analysisResult = result;

                // Auto-select high confidence topics
                foreach (var topic in result.SuggestedExistingTopics.Where(t => t.Confidence >= 0.7f))
                {
                    selectedTopicIds.Add(topic.TopicId);
                }
            }
            else
            {
                analysisError = "AI 分析失敗，您可以手動選擇主題或稍後再試。";
            }
        }
        catch (Exception ex)
        {
            analysisError = $"分析時發生錯誤：{ex.Message}";
        }
        finally
        {
            isAnalyzing = false;
        }
    }

    private async Task CreateAndSelectTopic(SuggestedNewTopicDto newTopic)
    {
        try
        {
            var createDto = new CreateTopicDto
            {
                Title = newTopic.Title,
                Description = newTopic.Description
            };

            var created = await ApiClient.CreateTopicAsync(createDto);
            if (created != null)
            {
                availableTopics.Add(created);
                selectedTopicIds.Add(created.Id);

                // Remove from suggested new topics
                analysisResult?.SuggestedNewTopics.RemoveAll(t =>
                    t.Title == newTopic.Title && t.Description == newTopic.Description);
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"建立主題時發生錯誤：{ex.Message}";
        }
    }

    private async Task SaveWithTopics()
    {
        if (!createdArticleId.HasValue) return;

        try
        {
            isSubmitting = true;
            errorMessage = string.Empty;

            if (selectedTopicIds.Any())
            {
                var success = await ApiClient.LinkArticleTopicsAsync(
                    createdArticleId.Value,
                    selectedTopicIds.ToList(),
                    confirmed: true);

                if (!success)
                {
                    errorMessage = "關聯主題失敗，請稍後再試。";
                    return;
                }
            }

            successMessage = "文章及主題關聯已儲存！";
            await Task.Delay(1500);
            Navigation.NavigateTo("/articles");
        }
        catch (Exception ex)
        {
            errorMessage = $"儲存時發生錯誤：{ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/articles");
    }
}
```

**Step 2: Build to verify**

```bash
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add src/Sitbrief.Admin/Pages/Articles/Add.razor
git commit -m "feat(admin): add AI analysis UI to Add Article page

- Add AI Analyze button after article creation
- Display AI suggestions with confidence scores
- Show key entities and geopolitical tags
- Allow creating suggested new topics
- Auto-select high confidence topic suggestions
- Two-step workflow: create article, then analyze and link topics

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

## Task 11: Test AI Integration

**Files:**
- None (testing only)

**Step 1: Set Claude API key**

Set environment variable (don't commit):

```bash
export Claude__ApiKey="your-actual-api-key"
```

Or add to `appsettings.Development.json` temporarily.

**Step 2: Start the API**

```bash
cd src/Sitbrief.API
dotnet run
```

**Step 3: Test analyze endpoint with curl**

```bash
# Get auth token
TOKEN=$(curl -s -X POST http://localhost:5167/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

# Test analyze on article 1
curl -s -X POST http://localhost:5167/api/articles/1/analyze \
  -H "Authorization: Bearer $TOKEN" | python3 -m json.tool
```

Expected: JSON response with suggested topics, entities, etc.

**Step 4: Test link topics endpoint**

```bash
curl -s -X POST http://localhost:5167/api/articles/1/topics \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"topicIds":[1,2],"confirmed":true}' \
  -w "\nHTTP Status: %{http_code}"
```

Expected: HTTP Status: 204

**Step 5: Run Blazor Admin and test UI**

```bash
cd ../Sitbrief.Admin
dotnet run
```

Test workflow:
1. Login with admin/admin123
2. Go to "新增文章"
3. Fill in article details
4. Click "儲存文章"
5. Click "AI 分析" button
6. Review AI suggestions
7. Select topics
8. Click "確認並完成"

**Step 6: Document test results**

All tests should pass. If Claude API key is not configured, the UI should gracefully show an error and allow manual topic selection.

---

## Phase 3 Complete! 🎉

You now have AI-powered article analysis with:

- ✅ Claude API service integration
- ✅ Article analysis endpoint (POST /api/articles/{id}/analyze)
- ✅ Topic linking endpoint (POST /api/articles/{id}/topics)
- ✅ AI analysis UI in Add Article page
- ✅ Display of suggested topics with confidence scores
- ✅ Key entities extraction (countries, organizations, persons)
- ✅ Geopolitical tags
- ✅ Significance scoring
- ✅ One-click new topic creation from AI suggestions
- ✅ Graceful degradation when AI unavailable

### Verification Checklist

- [ ] Claude API key configured
- [ ] Analyze endpoint returns structured suggestions
- [ ] Link topics endpoint works
- [ ] Blazor UI shows AI analysis button
- [ ] AI suggestions display correctly
- [ ] Can create topics from suggestions
- [ ] Can link topics to article
- [ ] Works without AI (manual mode)

### Next Steps

**Phase 4: iOS App MVP** will include:
- Swift/SwiftUI iOS project
- Topic list view
- Topic detail view with articles
- API integration
- Basic navigation

---

## Troubleshooting

**Claude API errors:**
- Verify API key is set correctly
- Check API key has sufficient credits
- Review logs for detailed error messages

**Empty analysis results:**
- Ensure article has title and summary
- Check Claude response in logs
- Verify JSON parsing works

**UI not showing AI section:**
- Article must be created first
- Check for JavaScript errors in browser console
- Verify ApiClient methods work
