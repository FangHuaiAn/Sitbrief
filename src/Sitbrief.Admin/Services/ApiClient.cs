using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Sitbrief.Admin.Models;

namespace Sitbrief.Admin.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Authentication
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
        }
        return null;
    }

    // Articles
    public async Task<List<ArticleDto>> GetArticlesAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<ArticleDto>>("/api/articles", _jsonOptions)
            ?? new List<ArticleDto>();
    }

    public async Task<ArticleDto?> GetArticleAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<ArticleDto>($"/api/articles/{id}", _jsonOptions);
    }

    public async Task<ArticleDto?> CreateArticleAsync(CreateArticleDto article)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("/api/articles", article);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ArticleDto>(_jsonOptions);
        }
        return null;
    }

    public async Task<bool> UpdateArticleAsync(int id, CreateArticleDto article)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"/api/articles/{id}", article);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"/api/articles/{id}");
        return response.IsSuccessStatusCode;
    }

    // Topics
    public async Task<List<TopicDto>> GetTopicsAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<TopicDto>>("/api/topics", _jsonOptions)
            ?? new List<TopicDto>();
    }

    public async Task<TopicDetailDto?> GetTopicAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<TopicDetailDto>($"/api/topics/{id}", _jsonOptions);
    }

    public async Task<TopicDto?> CreateTopicAsync(CreateTopicDto topic)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("/api/topics", topic);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TopicDto>(_jsonOptions);
        }
        return null;
    }

    public async Task<bool> UpdateTopicAsync(int id, CreateTopicDto topic)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"/api/topics/{id}", topic);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTopicAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"/api/topics/{id}");
        return response.IsSuccessStatusCode;
    }

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
}
