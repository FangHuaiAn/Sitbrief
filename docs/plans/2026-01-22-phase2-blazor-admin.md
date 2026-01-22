# Sitbrief Phase 2: Blazor Admin Web App Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a Blazor WebAssembly admin interface for managing articles and topics, with authentication and full CRUD capabilities.

**Architecture:** Blazor WebAssembly SPA that communicates with the existing ASP.NET Core API. Uses JWT authentication for admin access. No AI integration in this phase - pure manual workflow.

**Tech Stack:** Blazor WebAssembly, C# 12, .NET 8.0, Blazored.LocalStorage, JWT authentication

---

## Prerequisites

- Phase 1 completed (API backend running)
- .NET 8.0 SDK installed
- API running on http://localhost:5167 (or configured port)

---

## Task 1: Create Blazor WebAssembly Project

**Files:**
- Create: `src/Sitbrief.Admin/Sitbrief.Admin.csproj`
- Modify: `src/Sitbrief.sln`

**Step 1: Create Blazor WebAssembly project**

```bash
cd /Users/fanghuaian/Documents/Projects/Sitbrief/src
dotnet new blazorwasm -n Sitbrief.Admin -o Sitbrief.Admin -f net8.0
```

**Step 2: Add project to solution**

```bash
dotnet sln add Sitbrief.Admin/Sitbrief.Admin.csproj
```

**Step 3: Add required NuGet packages**

```bash
cd Sitbrief.Admin
dotnet add package Blazored.LocalStorage --version 4.5.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.0.0
```

**Step 4: Verify build**

```bash
cd ..
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 5: Clean up default files**

```bash
cd Sitbrief.Admin
rm -rf Pages/Counter.razor Pages/Weather.razor
rm -f wwwroot/sample-data/weather.json
```

**Step 6: Commit**

```bash
git add .
git commit -m "feat(admin): create Blazor WebAssembly project

- Add Blazor WebAssembly project for admin interface
- Add Blazored.LocalStorage for token storage
- Add JWT package for authentication
- Remove default sample pages

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 2: Create API Models (DTOs)

**Files:**
- Create: `src/Sitbrief.Admin/Models/ArticleDto.cs`
- Create: `src/Sitbrief.Admin/Models/TopicDto.cs`
- Create: `src/Sitbrief.Admin/Models/CreateArticleDto.cs`
- Create: `src/Sitbrief.Admin/Models/CreateTopicDto.cs`
- Create: `src/Sitbrief.Admin/Models/LoginRequest.cs`
- Create: `src/Sitbrief.Admin/Models/LoginResponse.cs`

**Step 1: Create ArticleDto**

Create file: `src/Sitbrief.Admin/Models/ArticleDto.cs`

```csharp
namespace Sitbrief.Admin.Models;

public class ArticleDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string SourceUrl { get; set; }
    public required string SourceName { get; set; }
    public int SourceType { get; set; }
    public DateTime PublishedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<TopicSummaryDto> Topics { get; set; } = new();
}

public class TopicSummaryDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
}
```

**Step 2: Create TopicDto**

Create file: `src/Sitbrief.Admin/Models/TopicDto.cs`

```csharp
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
```

**Step 3: Create CreateArticleDto**

Create file: `src/Sitbrief.Admin/Models/CreateArticleDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Sitbrief.Admin.Models;

public class CreateArticleDto
{
    [Required(ErrorMessage = "標題為必填")]
    [MaxLength(500, ErrorMessage = "標題長度不能超過 500 字元")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "摘要為必填")]
    [MaxLength(2000, ErrorMessage = "摘要長度不能超過 2000 字元")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "來源網址為必填")]
    [Url(ErrorMessage = "請輸入有效的網址")]
    [MaxLength(1000)]
    public string SourceUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "媒體/智庫名稱為必填")]
    [MaxLength(200)]
    public string SourceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "來源類型為必填")]
    public int SourceType { get; set; }

    [Required(ErrorMessage = "發布日期為必填")]
    public DateTime PublishedDate { get; set; } = DateTime.Now;

    [MaxLength(50000)]
    public string? Content { get; set; }
}
```

**Step 4: Create CreateTopicDto**

Create file: `src/Sitbrief.Admin/Models/CreateTopicDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Sitbrief.Admin.Models;

public class CreateTopicDto
{
    [Required(ErrorMessage = "標題為必填")]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "描述為必填")]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Significance { get; set; }
}
```

**Step 5: Create authentication models**

Create file: `src/Sitbrief.Admin/Models/LoginRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Sitbrief.Admin.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "使用者名稱為必填")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼為必填")]
    public string Password { get; set; } = string.Empty;
}
```

Create file: `src/Sitbrief.Admin/Models/LoginResponse.cs`

```csharp
namespace Sitbrief.Admin.Models;

public class LoginResponse
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

**Step 6: Build to verify**

```bash
cd ..
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 7: Commit**

```bash
git add .
git commit -m "feat(admin): add API model DTOs

- Add ArticleDto and TopicDto for API responses
- Add CreateArticleDto and CreateTopicDto for forms
- Add LoginRequest and LoginResponse for auth
- Add validation attributes with Chinese error messages

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 3: Create API Client Service

**Files:**
- Create: `src/Sitbrief.Admin/Services/ApiClient.cs`
- Create: `src/Sitbrief.Admin/Services/IApiClient.cs`

**Step 1: Create IApiClient interface**

Create file: `src/Sitbrief.Admin/Services/IApiClient.cs`

```csharp
using Sitbrief.Admin.Models;

namespace Sitbrief.Admin.Services;

public interface IApiClient
{
    // Authentication
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    // Articles
    Task<List<ArticleDto>> GetArticlesAsync();
    Task<ArticleDto?> GetArticleAsync(int id);
    Task<ArticleDto?> CreateArticleAsync(CreateArticleDto article);
    Task<bool> UpdateArticleAsync(int id, CreateArticleDto article);
    Task<bool> DeleteArticleAsync(int id);

    // Topics
    Task<List<TopicDto>> GetTopicsAsync();
    Task<TopicDetailDto?> GetTopicAsync(int id);
    Task<TopicDto?> CreateTopicAsync(CreateTopicDto topic);
    Task<bool> UpdateTopicAsync(int id, CreateTopicDto topic);
    Task<bool> DeleteTopicAsync(int id);
}
```

**Step 2: Create ApiClient implementation**

Create file: `src/Sitbrief.Admin/Services/ApiClient.cs`

```csharp
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
}
```

**Step 3: Build to verify**

```bash
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add .
git commit -m "feat(admin): add API client service

- Create IApiClient interface with all API methods
- Implement ApiClient with HttpClient
- Add JWT token management with local storage
- Support all CRUD operations for articles and topics

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 4: Configure Services and Authentication

**Files:**
- Modify: `src/Sitbrief.Admin/Program.cs`
- Modify: `src/Sitbrief.Admin/wwwroot/appsettings.json`
- Create: `src/Sitbrief.Admin/wwwroot/appsettings.Development.json`

**Step 1: Create appsettings.json**

Create file: `src/Sitbrief.Admin/wwwroot/appsettings.json`

```json
{
  "ApiBaseUrl": "http://localhost:5167"
}
```

**Step 2: Create appsettings.Development.json**

Create file: `src/Sitbrief.Admin/wwwroot/appsettings.Development.json`

```json
{
  "ApiBaseUrl": "http://localhost:5167"
}
```

**Step 3: Update Program.cs**

Edit file: `src/Sitbrief.Admin/Program.cs`

Replace entire content with:

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Sitbrief.Admin;
using Sitbrief.Admin.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Configure HttpClient with API base URL
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5167";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Register API client
builder.Services.AddScoped<IApiClient, ApiClient>();

await builder.Build().RunAsync();
```

**Step 4: Build to verify**

```bash
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 5: Commit**

```bash
git add .
git commit -m "feat(admin): configure services and API base URL

- Add appsettings for API configuration
- Register Blazored.LocalStorage
- Configure HttpClient with API base URL
- Register ApiClient service in DI

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 5: Create Login Page

**Files:**
- Create: `src/Sitbrief.Admin/Pages/Login.razor`
- Create: `src/Sitbrief.Admin/Pages/Login.razor.cs`

**Step 1: Create Login.razor**

Create file: `src/Sitbrief.Admin/Pages/Login.razor`

```razor
@page "/login"
@using Sitbrief.Admin.Models
@inject IApiClient ApiClient
@inject NavigationManager Navigation
@inject Blazored.LocalStorage.ILocalStorageService LocalStorage

<PageTitle>登入 - Sitbrief 管理後台</PageTitle>

<div class="login-container">
    <div class="login-box">
        <h2>Sitbrief 管理後台</h2>

        <EditForm Model="@loginRequest" OnValidSubmit="HandleLogin">
            <DataAnnotationsValidator />

            <div class="form-group">
                <label for="username">使用者名稱</label>
                <InputText id="username" @bind-Value="loginRequest.Username" class="form-control" />
                <ValidationMessage For="@(() => loginRequest.Username)" />
            </div>

            <div class="form-group">
                <label for="password">密碼</label>
                <InputText id="password" type="password" @bind-Value="loginRequest.Password" class="form-control" />
                <ValidationMessage For="@(() => loginRequest.Password)" />
            </div>

            @if (!string.IsNullOrEmpty(errorMessage))
            {
                <div class="alert alert-danger">@errorMessage</div>
            }

            <button type="submit" class="btn btn-primary" disabled="@isLoading">
                @if (isLoading)
                {
                    <span>登入中...</span>
                }
                else
                {
                    <span>登入</span>
                }
            </button>
        </EditForm>
    </div>
</div>

<style>
    .login-container {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 100vh;
        background-color: #f5f5f5;
    }

    .login-box {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        width: 100%;
        max-width: 400px;
    }

    .login-box h2 {
        margin-bottom: 1.5rem;
        text-align: center;
        color: #333;
    }

    .form-group {
        margin-bottom: 1rem;
    }

    .form-group label {
        display: block;
        margin-bottom: 0.5rem;
        font-weight: 500;
    }

    .form-control {
        width: 100%;
        padding: 0.5rem;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 1rem;
    }

    .btn {
        width: 100%;
        padding: 0.75rem;
        border: none;
        border-radius: 4px;
        font-size: 1rem;
        cursor: pointer;
        margin-top: 1rem;
    }

    .btn-primary {
        background-color: #007bff;
        color: white;
    }

    .btn-primary:hover:not(:disabled) {
        background-color: #0056b3;
    }

    .btn:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }

    .alert {
        padding: 0.75rem;
        border-radius: 4px;
        margin-top: 1rem;
    }

    .alert-danger {
        background-color: #f8d7da;
        color: #721c24;
        border: 1px solid #f5c6cb;
    }
</style>

@code {
    private LoginRequest loginRequest = new();
    private string errorMessage = string.Empty;
    private bool isLoading = false;

    private async Task HandleLogin()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;

            var response = await ApiClient.LoginAsync(loginRequest);

            if (response != null)
            {
                await LocalStorage.SetItemAsync("authToken", response.Token);
                Navigation.NavigateTo("/");
            }
            else
            {
                errorMessage = "登入失敗：使用者名稱或密碼錯誤";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"登入時發生錯誤：{ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
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
git add .
git commit -m "feat(admin): add login page

- Create login page with form validation
- Handle JWT token storage on successful login
- Add error handling and loading states
- Style login form with centered layout

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 6: Add Backend Authentication Support

**Files:**
- Modify: `src/Sitbrief.API/Controllers/AuthController.cs` (create new)
- Modify: `src/Sitbrief.API/Program.cs`
- Modify: `src/Sitbrief.API/appsettings.json`

**Step 1: Add JWT packages to API**

```bash
cd ../Sitbrief.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package BCrypt.Net-Next --version 4.0.3
```

**Step 2: Update appsettings.json with authentication config**

Edit file: `src/Sitbrief.API/appsettings.json`

Add after "ConnectionStrings" section:

```json
  "Authentication": {
    "AdminUsername": "admin",
    "AdminPasswordHash": "$2a$11$rqitp7AqL3KHPc1OKq.KJu5Z.yYxT0K3M3xEhCVp7nF3kHH6EuH7i",
    "JwtSecret": "your-secret-key-min-32-characters-long-for-security",
    "JwtIssuer": "Sitbrief",
    "JwtAudience": "SitbriefAdmin",
    "JwtExpirationHours": 12
  },
```

Note: The password hash above is for password "admin123" - change in production!

**Step 3: Create AuthController**

Create file: `src/Sitbrief.API/Controllers/AuthController.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Sitbrief.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var adminUsername = _configuration["Authentication:AdminUsername"];
            var adminPasswordHash = _configuration["Authentication:AdminPasswordHash"];

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminPasswordHash))
            {
                return StatusCode(500, "Authentication not configured");
            }

            // Verify username
            if (request.Username != adminUsername)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, adminPasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(request.Username);
            var expirationHours = int.Parse(_configuration["Authentication:JwtExpirationHours"] ?? "12");

            return Ok(new
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "An error occurred during login");
        }
    }

    private string GenerateJwtToken(string username)
    {
        var secret = _configuration["Authentication:JwtSecret"];
        var issuer = _configuration["Authentication:JwtIssuer"];
        var audience = _configuration["Authentication:JwtAudience"];
        var expirationHours = int.Parse(_configuration["Authentication:JwtExpirationHours"] ?? "12");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
```

**Step 4: Configure JWT authentication in Program.cs**

Edit file: `src/Sitbrief.API/Program.cs`

Add after `builder.Services.AddSwaggerGen();` (around line 10):

```csharp
// Configure JWT Authentication
var jwtSecret = builder.Configuration["Authentication:JwtSecret"];
var jwtIssuer = builder.Configuration["Authentication:JwtIssuer"];
var jwtAudience = builder.Configuration["Authentication:JwtAudience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSecret!))
    };
});

builder.Services.AddAuthorization();
```

Add the using statements at the top:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
```

Ensure `app.UseAuthentication();` is called before `app.UseAuthorization();` in the middleware section (around line 50):

```csharp
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
```

**Step 5: Build to verify**

```bash
cd ..
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 6: Commit**

```bash
git add .
git commit -m "feat(api): add JWT authentication for admin

- Add AuthController with login endpoint
- Configure JWT Bearer authentication
- Add BCrypt for password verification
- Add authentication configuration to appsettings
- Secure API with JWT tokens

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 7: Create Dashboard Page

**Files:**
- Create: `src/Sitbrief.Admin/Pages/Index.razor`
- Create: `src/Sitbrief.Admin/Shared/MainLayout.razor`
- Create: `src/Sitbrief.Admin/Shared/NavMenu.razor`

**Step 1: Update MainLayout.razor**

Edit file: `src/Sitbrief.Admin/Shared/MainLayout.razor`

Replace entire content with:

```razor
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <span class="title">Sitbrief 管理後台</span>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>

<style>
    .page {
        display: flex;
        height: 100vh;
    }

    .sidebar {
        background-color: #2c3e50;
        width: 250px;
        color: white;
    }

    main {
        flex: 1;
        display: flex;
        flex-direction: column;
    }

    .top-row {
        background-color: #f8f9fa;
        border-bottom: 1px solid #e0e0e0;
        height: 60px;
        display: flex;
        align-items: center;
    }

    .title {
        font-size: 1.25rem;
        font-weight: 500;
    }

    .content {
        padding: 1.5rem;
        flex: 1;
        overflow-y: auto;
    }
</style>
```

**Step 2: Update NavMenu.razor**

Edit file: `src/Sitbrief.Admin/Shared/NavMenu.razor`

Replace entire content with:

```razor
<div class="nav-menu">
    <div class="logo">
        <h3>Sitbrief</h3>
    </div>

    <nav class="nav-links">
        <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
            <span>儀表板</span>
        </NavLink>

        <NavLink class="nav-link" href="articles">
            <span>文章管理</span>
        </NavLink>

        <NavLink class="nav-link" href="topics">
            <span>主題管理</span>
        </NavLink>

        <NavLink class="nav-link" href="articles/add">
            <span>新增文章</span>
        </NavLink>
    </nav>

    <div class="nav-footer">
        <button class="logout-btn" @onclick="Logout">登出</button>
    </div>
</div>

<style>
    .nav-menu {
        display: flex;
        flex-direction: column;
        height: 100%;
    }

    .logo {
        padding: 1.5rem;
        border-bottom: 1px solid rgba(255,255,255,0.1);
    }

    .logo h3 {
        margin: 0;
        color: white;
        font-size: 1.5rem;
    }

    .nav-links {
        flex: 1;
        padding: 1rem 0;
    }

    .nav-link {
        display: block;
        padding: 0.75rem 1.5rem;
        color: rgba(255,255,255,0.8);
        text-decoration: none;
        transition: all 0.2s;
    }

    .nav-link:hover {
        background-color: rgba(255,255,255,0.1);
        color: white;
    }

    .nav-link.active {
        background-color: rgba(255,255,255,0.15);
        color: white;
        border-left: 3px solid #3498db;
    }

    .nav-footer {
        padding: 1rem 1.5rem;
        border-top: 1px solid rgba(255,255,255,0.1);
    }

    .logout-btn {
        width: 100%;
        padding: 0.75rem;
        background-color: rgba(255,255,255,0.1);
        border: 1px solid rgba(255,255,255,0.2);
        color: white;
        border-radius: 4px;
        cursor: pointer;
        transition: all 0.2s;
    }

    .logout-btn:hover {
        background-color: rgba(255,255,255,0.2);
    }
</style>

@code {
    [Inject] private NavigationManager? Navigation { get; set; }
    [Inject] private Blazored.LocalStorage.ILocalStorageService? LocalStorage { get; set; }

    private async Task Logout()
    {
        await LocalStorage!.RemoveItemAsync("authToken");
        Navigation!.NavigateTo("/login");
    }
}
```

**Step 3: Update Index.razor (Dashboard)**

Edit file: `src/Sitbrief.Admin/Pages/Index.razor`

Replace entire content with:

```razor
@page "/"
@using Sitbrief.Admin.Models
@inject IApiClient ApiClient
@inject NavigationManager Navigation
@inject Blazored.LocalStorage.ILocalStorageService LocalStorage

<PageTitle>儀表板 - Sitbrief</PageTitle>

<h1>儀表板</h1>

@if (isLoading)
{
    <p>載入中...</p>
}
else if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">@errorMessage</div>
}
else
{
    <div class="dashboard-stats">
        <div class="stat-card">
            <h3>@totalArticles</h3>
            <p>總文章數</p>
        </div>
        <div class="stat-card">
            <h3>@totalTopics</h3>
            <p>總主題數</p>
        </div>
        <div class="stat-card">
            <h3>@todayArticles</h3>
            <p>今日新增文章</p>
        </div>
    </div>

    <div class="recent-articles">
        <h2>最近新增的文章</h2>
        @if (recentArticles.Any())
        {
            <table class="table">
                <thead>
                    <tr>
                        <th>標題</th>
                        <th>來源</th>
                        <th>發布日期</th>
                        <th>主題</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var article in recentArticles)
                    {
                        <tr>
                            <td>@article.Title</td>
                            <td>@article.SourceName</td>
                            <td>@article.PublishedDate.ToString("yyyy-MM-dd")</td>
                            <td>
                                @foreach (var topic in article.Topics)
                                {
                                    <span class="badge">@topic.Title</span>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <p>尚無文章</p>
        }
    </div>
}

<style>
    .dashboard-stats {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 1rem;
        margin-bottom: 2rem;
    }

    .stat-card {
        background: white;
        padding: 1.5rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        text-align: center;
    }

    .stat-card h3 {
        font-size: 2.5rem;
        margin: 0;
        color: #3498db;
    }

    .stat-card p {
        margin: 0.5rem 0 0;
        color: #666;
    }

    .recent-articles {
        background: white;
        padding: 1.5rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .recent-articles h2 {
        margin-top: 0;
    }

    .table {
        width: 100%;
        border-collapse: collapse;
    }

    .table th {
        background-color: #f8f9fa;
        padding: 0.75rem;
        text-align: left;
        border-bottom: 2px solid #dee2e6;
    }

    .table td {
        padding: 0.75rem;
        border-bottom: 1px solid #dee2e6;
    }

    .badge {
        display: inline-block;
        padding: 0.25rem 0.5rem;
        background-color: #e3f2fd;
        color: #1976d2;
        border-radius: 4px;
        font-size: 0.875rem;
        margin-right: 0.25rem;
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
</style>

@code {
    private bool isLoading = true;
    private string errorMessage = string.Empty;
    private int totalArticles = 0;
    private int totalTopics = 0;
    private int todayArticles = 0;
    private List<ArticleDto> recentArticles = new();

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthenticationAsync();
        await LoadDashboardDataAsync();
    }

    private async Task CheckAuthenticationAsync()
    {
        var token = await LocalStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrEmpty(token))
        {
            Navigation.NavigateTo("/login");
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;

            var articles = await ApiClient.GetArticlesAsync();
            var topics = await ApiClient.GetTopicsAsync();

            totalArticles = articles.Count;
            totalTopics = topics.Count;

            var today = DateTime.Today;
            todayArticles = articles.Count(a => a.CreatedDate.Date == today);

            recentArticles = articles
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToList();
        }
        catch (Exception ex)
        {
            errorMessage = $"載入資料時發生錯誤：{ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

**Step 4: Build to verify**

```bash
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 5: Commit**

```bash
git add .
git commit -m "feat(admin): add dashboard page with statistics

- Create dashboard with article/topic statistics
- Add navigation menu with logout functionality
- Style main layout with sidebar
- Show recent articles table
- Add authentication check on page load

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 8: Create Articles List Page

**Files:**
- Create: `src/Sitbrief.Admin/Pages/Articles/List.razor`

**Step 1: Create Articles List page**

Create file: `src/Sitbrief.Admin/Pages/Articles/List.razor`

```razor
@page "/articles"
@using Sitbrief.Admin.Models
@inject IApiClient ApiClient
@inject NavigationManager Navigation

<PageTitle>文章管理 - Sitbrief</PageTitle>

<div class="page-header">
    <h1>文章管理</h1>
    <button class="btn btn-primary" @onclick="NavigateToAdd">新增文章</button>
</div>

@if (isLoading)
{
    <p>載入中...</p>
}
else if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">@errorMessage</div>
}
else
{
    <div class="articles-container">
        @if (articles.Any())
        {
            <table class="table">
                <thead>
                    <tr>
                        <th>標題</th>
                        <th>來源</th>
                        <th>類型</th>
                        <th>發布日期</th>
                        <th>主題</th>
                        <th>操作</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var article in articles)
                    {
                        <tr>
                            <td>
                                <a href="@article.SourceUrl" target="_blank" class="article-link">
                                    @article.Title
                                </a>
                            </td>
                            <td>@article.SourceName</td>
                            <td>
                                <span class="badge badge-@(article.SourceType == 0 ? "media" : "think-tank")">
                                    @(article.SourceType == 0 ? "新聞媒體" : "智庫")
                                </span>
                            </td>
                            <td>@article.PublishedDate.ToString("yyyy-MM-dd")</td>
                            <td>
                                @foreach (var topic in article.Topics)
                                {
                                    <span class="badge badge-topic">@topic.Title</span>
                                }
                            </td>
                            <td>
                                <button class="btn btn-sm btn-danger" @onclick="() => DeleteArticle(article.Id)">
                                    刪除
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <div class="empty-state">
                <p>尚無文章，請點擊上方「新增文章」按鈕開始新增。</p>
            </div>
        }
    </div>
}

@if (showDeleteConfirm)
{
    <div class="modal-overlay" @onclick="CancelDelete">
        <div class="modal-content" @onclick:stopPropagation>
            <h3>確認刪除</h3>
            <p>確定要刪除這篇文章嗎？此操作無法復原。</p>
            <div class="modal-actions">
                <button class="btn btn-secondary" @onclick="CancelDelete">取消</button>
                <button class="btn btn-danger" @onclick="ConfirmDelete">確認刪除</button>
            </div>
        </div>
    </div>
}

<style>
    .page-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1.5rem;
    }

    .articles-container {
        background: white;
        padding: 1.5rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .table {
        width: 100%;
        border-collapse: collapse;
    }

    .table th {
        background-color: #f8f9fa;
        padding: 0.75rem;
        text-align: left;
        border-bottom: 2px solid #dee2e6;
        font-weight: 600;
    }

    .table td {
        padding: 0.75rem;
        border-bottom: 1px solid #dee2e6;
    }

    .article-link {
        color: #007bff;
        text-decoration: none;
    }

    .article-link:hover {
        text-decoration: underline;
    }

    .badge {
        display: inline-block;
        padding: 0.25rem 0.5rem;
        border-radius: 4px;
        font-size: 0.875rem;
        margin-right: 0.25rem;
    }

    .badge-media {
        background-color: #e3f2fd;
        color: #1976d2;
    }

    .badge-think-tank {
        background-color: #f3e5f5;
        color: #7b1fa2;
    }

    .badge-topic {
        background-color: #fff3e0;
        color: #e65100;
    }

    .btn {
        padding: 0.5rem 1rem;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 1rem;
    }

    .btn-primary {
        background-color: #007bff;
        color: white;
    }

    .btn-primary:hover {
        background-color: #0056b3;
    }

    .btn-sm {
        padding: 0.25rem 0.5rem;
        font-size: 0.875rem;
    }

    .btn-danger {
        background-color: #dc3545;
        color: white;
    }

    .btn-danger:hover {
        background-color: #c82333;
    }

    .btn-secondary {
        background-color: #6c757d;
        color: white;
    }

    .btn-secondary:hover {
        background-color: #5a6268;
    }

    .empty-state {
        text-align: center;
        padding: 3rem;
        color: #666;
    }

    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0,0,0,0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    .modal-content {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        max-width: 400px;
        width: 90%;
    }

    .modal-content h3 {
        margin-top: 0;
    }

    .modal-actions {
        display: flex;
        gap: 1rem;
        margin-top: 1.5rem;
        justify-content: flex-end;
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
</style>

@code {
    private bool isLoading = true;
    private string errorMessage = string.Empty;
    private List<ArticleDto> articles = new();
    private bool showDeleteConfirm = false;
    private int articleToDelete = 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadArticlesAsync();
    }

    private async Task LoadArticlesAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;
            articles = await ApiClient.GetArticlesAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"載入文章時發生錯誤：{ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateToAdd()
    {
        Navigation.NavigateTo("/articles/add");
    }

    private void DeleteArticle(int id)
    {
        articleToDelete = id;
        showDeleteConfirm = true;
    }

    private void CancelDelete()
    {
        showDeleteConfirm = false;
        articleToDelete = 0;
    }

    private async Task ConfirmDelete()
    {
        try
        {
            var success = await ApiClient.DeleteArticleAsync(articleToDelete);
            if (success)
            {
                await LoadArticlesAsync();
            }
            else
            {
                errorMessage = "刪除文章失敗";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"刪除文章時發生錯誤：{ex.Message}";
        }
        finally
        {
            showDeleteConfirm = false;
            articleToDelete = 0;
        }
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
git add .
git commit -m "feat(admin): add articles list page

- Create articles list with table view
- Show article details, source, topics
- Add delete functionality with confirmation modal
- Link to article source URLs
- Add empty state message

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 9: Create Add Article Page

**Files:**
- Create: `src/Sitbrief.Admin/Pages/Articles/Add.razor`

**Step 1: Create Add Article page**

Create file: `src/Sitbrief.Admin/Pages/Articles/Add.razor`

```razor
@page "/articles/add"
@using Sitbrief.Admin.Models
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
        max-width: 800px;
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
</style>

@code {
    private CreateArticleDto article = new();
    private List<TopicDto> availableTopics = new();
    private HashSet<int> selectedTopicIds = new();
    private bool isSubmitting = false;
    private bool loadingTopics = true;
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;

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
                // TODO: Associate with selected topics (will be implemented with AI integration)
                successMessage = "文章已成功建立！";

                // Reset form
                article = new CreateArticleDto();
                selectedTopicIds.Clear();

                // Navigate to articles list after delay
                await Task.Delay(1500);
                Navigation.NavigateTo("/articles");
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
git add .
git commit -m "feat(admin): add create article page

- Create article form with all fields
- Add topic selection with checkboxes
- Include form validation
- Add success/error messages
- Navigate to articles list after creation

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 10: Create Topics Management Page

**Files:**
- Create: `src/Sitbrief.Admin/Pages/Topics/List.razor`
- Create: `src/Sitbrief.Admin/Pages/Topics/Add.razor`

**Step 1: Create Topics List page**

Create file: `src/Sitbrief.Admin/Pages/Topics/List.razor`

```razor
@page "/topics"
@using Sitbrief.Admin.Models
@inject IApiClient ApiClient
@inject NavigationManager Navigation

<PageTitle>主題管理 - Sitbrief</PageTitle>

<div class="page-header">
    <h1>主題管理</h1>
    <button class="btn btn-primary" @onclick="NavigateToAdd">新增主題</button>
</div>

@if (isLoading)
{
    <p>載入中...</p>
}
else if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">@errorMessage</div>
}
else
{
    <div class="topics-container">
        @if (topics.Any())
        {
            <div class="topics-grid">
                @foreach (var topic in topics)
                {
                    <div class="topic-card">
                        <div class="topic-header">
                            <h3>@topic.Title</h3>
                            <span class="article-count">@topic.ArticleCount 篇文章</span>
                        </div>
                        <p class="topic-description">@topic.Description</p>
                        @if (!string.IsNullOrEmpty(topic.Significance))
                        {
                            <p class="topic-significance">
                                <strong>重要性：</strong>@topic.Significance
                            </p>
                        }
                        <div class="topic-footer">
                            <span class="topic-date">
                                最後更新：@topic.LastUpdatedDate.ToString("yyyy-MM-dd")
                            </span>
                            <button class="btn btn-sm btn-danger" @onclick="() => DeleteTopic(topic.Id)">
                                刪除
                            </button>
                        </div>
                    </div>
                }
            </div>
        }
        else
        {
            <div class="empty-state">
                <p>尚無主題，請點擊上方「新增主題」按鈕開始建立。</p>
            </div>
        }
    </div>
}

@if (showDeleteConfirm)
{
    <div class="modal-overlay" @onclick="CancelDelete">
        <div class="modal-content" @onclick:stopPropagation>
            <h3>確認刪除</h3>
            <p>確定要刪除這個主題嗎？關聯的文章不會被刪除。</p>
            <div class="modal-actions">
                <button class="btn btn-secondary" @onclick="CancelDelete">取消</button>
                <button class="btn btn-danger" @onclick="ConfirmDelete">確認刪除</button>
            </div>
        </div>
    </div>
}

<style>
    .page-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1.5rem;
    }

    .topics-container {
        background: white;
        padding: 1.5rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .topics-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
        gap: 1.5rem;
    }

    .topic-card {
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        padding: 1.5rem;
        background: #fafafa;
    }

    .topic-header {
        display: flex;
        justify-content: space-between;
        align-items: start;
        margin-bottom: 1rem;
    }

    .topic-header h3 {
        margin: 0;
        font-size: 1.25rem;
        color: #333;
    }

    .article-count {
        background-color: #e3f2fd;
        color: #1976d2;
        padding: 0.25rem 0.5rem;
        border-radius: 4px;
        font-size: 0.875rem;
        white-space: nowrap;
    }

    .topic-description {
        color: #666;
        margin-bottom: 0.75rem;
        line-height: 1.5;
    }

    .topic-significance {
        color: #555;
        font-size: 0.875rem;
        padding: 0.5rem;
        background-color: #fff3e0;
        border-left: 3px solid #ff9800;
        margin-bottom: 1rem;
    }

    .topic-footer {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding-top: 1rem;
        border-top: 1px solid #e0e0e0;
    }

    .topic-date {
        font-size: 0.875rem;
        color: #999;
    }

    .btn {
        padding: 0.5rem 1rem;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 1rem;
    }

    .btn-primary {
        background-color: #007bff;
        color: white;
    }

    .btn-primary:hover {
        background-color: #0056b3;
    }

    .btn-sm {
        padding: 0.25rem 0.5rem;
        font-size: 0.875rem;
    }

    .btn-danger {
        background-color: #dc3545;
        color: white;
    }

    .btn-danger:hover {
        background-color: #c82333;
    }

    .btn-secondary {
        background-color: #6c757d;
        color: white;
    }

    .btn-secondary:hover {
        background-color: #5a6268;
    }

    .empty-state {
        text-align: center;
        padding: 3rem;
        color: #666;
    }

    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0,0,0,0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    .modal-content {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        max-width: 400px;
        width: 90%;
    }

    .modal-content h3 {
        margin-top: 0;
    }

    .modal-actions {
        display: flex;
        gap: 1rem;
        margin-top: 1.5rem;
        justify-content: flex-end;
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
</style>

@code {
    private bool isLoading = true;
    private string errorMessage = string.Empty;
    private List<TopicDto> topics = new();
    private bool showDeleteConfirm = false;
    private int topicToDelete = 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadTopicsAsync();
    }

    private async Task LoadTopicsAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;
            topics = await ApiClient.GetTopicsAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"載入主題時發生錯誤：{ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateToAdd()
    {
        Navigation.NavigateTo("/topics/add");
    }

    private void DeleteTopic(int id)
    {
        topicToDelete = id;
        showDeleteConfirm = true;
    }

    private void CancelDelete()
    {
        showDeleteConfirm = false;
        topicToDelete = 0;
    }

    private async Task ConfirmDelete()
    {
        try
        {
            var success = await ApiClient.DeleteTopicAsync(topicToDelete);
            if (success)
            {
                await LoadTopicsAsync();
            }
            else
            {
                errorMessage = "刪除主題失敗";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"刪除主題時發生錯誤：{ex.Message}";
        }
        finally
        {
            showDeleteConfirm = false;
            topicToDelete = 0;
        }
    }
}
```

**Step 2: Create Add Topic page**

Create file: `src/Sitbrief.Admin/Pages/Topics/Add.razor`

```razor
@page "/topics/add"
@using Sitbrief.Admin.Models
@inject IApiClient ApiClient
@inject NavigationManager Navigation

<PageTitle>新增主題 - Sitbrief</PageTitle>

<div class="page-header">
    <h1>新增主題</h1>
</div>

<div class="form-container">
    <EditForm Model="@topic" OnValidSubmit="HandleSubmit">
        <DataAnnotationsValidator />

        <div class="form-group">
            <label for="title">標題 *</label>
            <InputText id="title" @bind-Value="topic.Title" class="form-control"
                       placeholder="例如：中美南海爭議" />
            <ValidationMessage For="@(() => topic.Title)" />
        </div>

        <div class="form-group">
            <label for="description">描述 *</label>
            <InputTextArea id="description" @bind-Value="topic.Description" class="form-control" rows="4"
                          placeholder="簡短描述這個主題的內容和範圍" />
            <ValidationMessage For="@(() => topic.Description)" />
        </div>

        <div class="form-group">
            <label for="significance">重要性說明（選填）</label>
            <InputTextArea id="significance" @bind-Value="topic.Significance" class="form-control" rows="3"
                          placeholder="說明為什麼這個主題重要，對地緣政治的影響" />
            <ValidationMessage For="@(() => topic.Significance)" />
            <small class="form-text">此欄位將顯示在主題卡片中，幫助使用者理解主題重要性</small>
        </div>

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
            <button type="submit" class="btn btn-primary" disabled="@isSubmitting">
                @if (isSubmitting)
                {
                    <span>建立中...</span>
                }
                else
                {
                    <span>建立主題</span>
                }
            </button>
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
        max-width: 800px;
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

    .form-text {
        display: block;
        margin-top: 0.25rem;
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
</style>

@code {
    private CreateTopicDto topic = new();
    private bool isSubmitting = false;
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;

    private async Task HandleSubmit()
    {
        try
        {
            isSubmitting = true;
            errorMessage = string.Empty;
            successMessage = string.Empty;

            var createdTopic = await ApiClient.CreateTopicAsync(topic);

            if (createdTopic != null)
            {
                successMessage = "主題已成功建立！";

                // Reset form
                topic = new CreateTopicDto();

                // Navigate to topics list after delay
                await Task.Delay(1500);
                Navigation.NavigateTo("/topics");
            }
            else
            {
                errorMessage = "建立主題失敗，請稍後再試。";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"建立主題時發生錯誤：{ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/topics");
    }
}
```

**Step 3: Build to verify**

```bash
dotnet build Sitbrief.Admin
```

Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add .
git commit -m "feat(admin): add topics management pages

- Create topics list with card layout
- Show article count and significance
- Add topic creation form
- Include delete functionality with confirmation
- Display last updated date

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 11: Test the Blazor Admin Application

**Files:**
- None (testing only)

**Step 1: Ensure API is running**

```bash
cd ../Sitbrief.API
dotnet run &
```

Wait for API to start (should show "Now listening on: http://localhost:5167")

**Step 2: Run Blazor application**

```bash
cd ../Sitbrief.Admin
dotnet run
```

Expected: Application starts, shows "Now listening on: http://localhost:5xxx"

**Step 3: Test in browser**

Open browser to the Blazor app URL (shown in console)

**Test Login:**
1. Navigate to the application
2. Should redirect to /login
3. Enter username: `admin`
4. Enter password: `admin123`
5. Click "登入"
6. Should redirect to dashboard

**Test Dashboard:**
1. Verify statistics are displayed (article count, topic count)
2. Verify recent articles table shows seed data

**Test Topics:**
1. Click "主題管理" in sidebar
2. Verify existing topics from seed data are displayed
3. Click "新增主題"
4. Fill in form with test data
5. Click "建立主題"
6. Verify topic is created and appears in list

**Test Articles:**
1. Click "文章管理" in sidebar
2. Verify existing articles from seed data are displayed
3. Click "新增文章"
4. Fill in form with test data
5. Select one or more topics
6. Click "儲存文章"
7. Verify article is created and appears in list

**Test Navigation and Logout:**
1. Navigate between pages using sidebar
2. Click "登出" button
3. Verify redirect to login page
4. Verify token is cleared (cannot access dashboard without login)

**Step 4: Stop applications**

Press Ctrl+C in both terminals to stop API and Blazor app

**Step 5: Document test results**

All tests should pass. Note any issues for fixes.

---

## Phase 2 Complete! 🎉

You now have a fully functional Blazor WebAssembly admin interface with:

- ✅ JWT authentication with login/logout
- ✅ Dashboard with statistics
- ✅ Article management (list, create, delete)
- ✅ Topic management (list, create, delete)
- ✅ API client service with token management
- ✅ Responsive UI with proper styling
- ✅ Form validation with Chinese error messages
- ✅ Navigation menu and routing

### Verification Checklist

- [ ] Can login with admin/admin123
- [ ] Dashboard displays correct statistics
- [ ] Can view all articles with topics
- [ ] Can create new articles
- [ ] Can delete articles
- [ ] Can view all topics
- [ ] Can create new topics
- [ ] Can delete topics
- [ ] Logout works correctly
- [ ] Token authentication works

### Next Steps

**Phase 3: AI Integration** will include:
- Claude API service integration
- Article analysis endpoint
- Topic suggestion UI
- AI-assisted article-topic linking
- Confidence scoring

---

## Troubleshooting

**Login fails:**
- Ensure API is running on http://localhost:5167
- Check authentication configuration in appsettings.json
- Verify password hash matches "admin123"

**CORS errors:**
- Ensure API has CORS configured for development
- Check API Program.cs has UseCors("DevelopmentCors")

**API not found:**
- Update ApiBaseUrl in Blazor appsettings.json
- Ensure API is running before starting Blazor app

**Token not persisting:**
- Check browser local storage in DevTools
- Verify Blazored.LocalStorage is registered in Program.cs
