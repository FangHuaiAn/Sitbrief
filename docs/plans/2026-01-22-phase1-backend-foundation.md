# Sitbrief Phase 1: Backend Foundation Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a working ASP.NET Core API with Entity Framework Core, SQLite database, and basic CRUD operations for articles and topics.

**Architecture:** Three-layer architecture with API layer (controllers), Core layer (business logic and entities), and Infrastructure layer (data access with EF Core). API exposes RESTful endpoints for managing articles and topics.

**Tech Stack:** ASP.NET Core 8.0, Entity Framework Core, SQLite, C# 12

---

## Prerequisites

- .NET 8.0 SDK installed
- Visual Studio 2022, Rider, or VS Code with C# extension
- Git
- Postman or similar for API testing

---

## Task 1: Create Solution Structure

**Files:**
- Create: `src/Sitbrief.sln`
- Create: `src/Sitbrief.Core/Sitbrief.Core.csproj`
- Create: `src/Sitbrief.Infrastructure/Sitbrief.Infrastructure.csproj`
- Create: `src/Sitbrief.API/Sitbrief.API.csproj`

**Step 1: Create solution and project structure**

```bash
cd /Users/fanghuaian/Documents/Projects/Sitbrief
mkdir -p src
cd src
dotnet new sln -n Sitbrief
mkdir -p Sitbrief.Core Sitbrief.Infrastructure Sitbrief.API
dotnet new classlib -n Sitbrief.Core -o Sitbrief.Core -f net8.0
dotnet new classlib -n Sitbrief.Infrastructure -o Sitbrief.Infrastructure -f net8.0
dotnet new webapi -n Sitbrief.API -o Sitbrief.API -f net8.0
```

**Step 2: Add projects to solution**

```bash
dotnet sln add Sitbrief.Core/Sitbrief.Core.csproj
dotnet sln add Sitbrief.Infrastructure/Sitbrief.Infrastructure.csproj
dotnet sln add Sitbrief.API/Sitbrief.API.csproj
```

**Step 3: Set up project references**

```bash
cd Sitbrief.API
dotnet add reference ../Sitbrief.Core/Sitbrief.Core.csproj
dotnet add reference ../Sitbrief.Infrastructure/Sitbrief.Infrastructure.csproj
cd ../Sitbrief.Infrastructure
dotnet add reference ../Sitbrief.Core/Sitbrief.Core.csproj
cd ..
```

**Step 4: Verify build**

```bash
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 5: Clean up default files**

```bash
rm Sitbrief.Core/Class1.cs
rm Sitbrief.Infrastructure/Class1.cs
rm Sitbrief.API/WeatherForecast.cs
rm Sitbrief.API/Controllers/WeatherForecastController.cs
```

**Step 6: Commit**

```bash
git add .
git commit -m "feat: initialize solution structure with three projects

- Add Sitbrief.Core for domain entities
- Add Sitbrief.Infrastructure for data access
- Add Sitbrief.API for REST endpoints
- Set up project references

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 2: Create Domain Entities

**Files:**
- Create: `src/Sitbrief.Core/Entities/Article.cs`
- Create: `src/Sitbrief.Core/Entities/Topic.cs`
- Create: `src/Sitbrief.Core/Entities/ArticleTopic.cs`
- Create: `src/Sitbrief.Core/Entities/AIAnalysis.cs`
- Create: `src/Sitbrief.Core/Enums/SourceType.cs`

**Step 1: Create SourceType enum**

Create file: `src/Sitbrief.Core/Enums/SourceType.cs`

```csharp
namespace Sitbrief.Core.Enums;

public enum SourceType
{
    NewsMedia = 0,
    ThinkTank = 1
}
```

**Step 2: Create Article entity**

Create file: `src/Sitbrief.Core/Entities/Article.cs`

```csharp
using Sitbrief.Core.Enums;

namespace Sitbrief.Core.Entities;

public class Article
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string SourceUrl { get; set; }
    public required string SourceName { get; set; }
    public SourceType SourceType { get; set; }
    public string? Content { get; set; }
    public DateTime PublishedDate { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation properties
    public ICollection<ArticleTopic> ArticleTopics { get; set; } = new List<ArticleTopic>();
    public AIAnalysis? AIAnalysis { get; set; }
}
```

**Step 3: Create Topic entity**

Create file: `src/Sitbrief.Core/Entities/Topic.cs`

```csharp
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
```

**Step 4: Create ArticleTopic junction entity**

Create file: `src/Sitbrief.Core/Entities/ArticleTopic.cs`

```csharp
namespace Sitbrief.Core.Entities;

public class ArticleTopic
{
    public int ArticleId { get; set; }
    public int TopicId { get; set; }
    public float Confidence { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime AddedDate { get; set; }

    // Navigation properties
    public Article Article { get; set; } = null!;
    public Topic Topic { get; set; } = null!;
}
```

**Step 5: Create AIAnalysis entity**

Create file: `src/Sitbrief.Core/Entities/AIAnalysis.cs`

```csharp
namespace Sitbrief.Core.Entities;

public class AIAnalysis
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public required string SuggestedTopicsJson { get; set; }
    public required string KeyEntitiesJson { get; set; }
    public required string GeopoliticalTagsJson { get; set; }
    public int SignificanceScore { get; set; }
    public DateTime AnalyzedDate { get; set; }

    // Navigation property
    public Article Article { get; set; } = null!;
}
```

**Step 6: Verify compilation**

```bash
cd src
dotnet build Sitbrief.Core
```

Expected: BUILD SUCCEEDED

**Step 7: Commit**

```bash
git add .
git commit -m "feat(core): add domain entities for articles and topics

- Add Article entity with source information
- Add Topic entity for grouping articles
- Add ArticleTopic for many-to-many relationship
- Add AIAnalysis for storing AI suggestions
- Add SourceType enum

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 3: Set Up Entity Framework Core

**Files:**
- Modify: `src/Sitbrief.Infrastructure/Sitbrief.Infrastructure.csproj`
- Create: `src/Sitbrief.Infrastructure/Data/SitbriefDbContext.cs`

**Step 1: Add EF Core packages to Infrastructure project**

```bash
cd src/Sitbrief.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
```

**Step 2: Verify packages installed**

```bash
dotnet list package
```

Expected: Shows EntityFrameworkCore packages

**Step 3: Create DbContext**

Create file: `src/Sitbrief.Infrastructure/Data/SitbriefDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Sitbrief.Core.Entities;

namespace Sitbrief.Infrastructure.Data;

public class SitbriefDbContext : DbContext
{
    public SitbriefDbContext(DbContextOptions<SitbriefDbContext> options)
        : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<ArticleTopic> ArticleTopics => Set<ArticleTopic>();
    public DbSet<AIAnalysis> AIAnalyses => Set<AIAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Article
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Summary).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.SourceUrl).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.SourceName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).HasMaxLength(50000);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
        });

        // Configure Topic
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Significance).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.LastUpdatedDate).HasDefaultValueSql("datetime('now')");
        });

        // Configure ArticleTopic (Many-to-Many)
        modelBuilder.Entity<ArticleTopic>(entity =>
        {
            entity.HasKey(e => new { e.ArticleId, e.TopicId });

            entity.HasOne(e => e.Article)
                .WithMany(a => a.ArticleTopics)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Topic)
                .WithMany(t => t.ArticleTopics)
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Confidence).HasDefaultValue(1.0f);
            entity.Property(e => e.IsConfirmed).HasDefaultValue(false);
            entity.Property(e => e.AddedDate).HasDefaultValueSql("datetime('now')");
        });

        // Configure AIAnalysis
        modelBuilder.Entity<AIAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Article)
                .WithOne(a => a.AIAnalysis)
                .HasForeignKey<AIAnalysis>(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.SuggestedTopicsJson).IsRequired();
            entity.Property(e => e.KeyEntitiesJson).IsRequired();
            entity.Property(e => e.GeopoliticalTagsJson).IsRequired();
            entity.Property(e => e.SignificanceScore).HasDefaultValue(5);
            entity.Property(e => e.AnalyzedDate).HasDefaultValueSql("datetime('now')");
        });
    }
}
```

**Step 4: Build to verify**

```bash
cd ..
dotnet build Sitbrief.Infrastructure
```

Expected: BUILD SUCCEEDED

**Step 5: Commit**

```bash
git add .
git commit -m "feat(infrastructure): add EF Core DbContext with entity configurations

- Add Entity Framework Core packages
- Create SitbriefDbContext with DbSet properties
- Configure entity relationships and constraints
- Set up many-to-many relationship for ArticleTopic

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 4: Create Initial Migration

**Files:**
- Modify: `src/Sitbrief.API/Sitbrief.API.csproj`
- Create: `src/Sitbrief.Infrastructure/Migrations/[timestamp]_InitialCreate.cs`

**Step 1: Add EF Core tools to API project**

```bash
cd src/Sitbrief.API
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
```

**Step 2: Install EF Core tools globally (if not already installed)**

```bash
dotnet tool install --global dotnet-ef
```

Or update if already installed:

```bash
dotnet tool update --global dotnet-ef
```

**Step 3: Create initial migration**

```bash
cd ../Sitbrief.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Sitbrief.API --context SitbriefDbContext
```

Expected: Migration files created in Migrations folder

**Step 4: Verify migration files created**

```bash
ls Migrations/
```

Expected: See files like `[timestamp]_InitialCreate.cs` and `[timestamp]_InitialCreate.Designer.cs`

**Step 5: Commit**

```bash
git add .
git commit -m "feat(infrastructure): add initial database migration

- Create InitialCreate migration for all entities
- Configure EF Core design-time tools

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 5: Configure API Project

**Files:**
- Modify: `src/Sitbrief.API/Program.cs`
- Modify: `src/Sitbrief.API/appsettings.json`
- Modify: `src/Sitbrief.API/appsettings.Development.json`

**Step 1: Update appsettings.json**

Edit file: `src/Sitbrief.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=sitbrief.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Step 2: Update appsettings.Development.json**

Edit file: `src/Sitbrief.API/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=sitbrief.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Step 3: Update Program.cs to configure services**

Edit file: `src/Sitbrief.API/Program.cs`

Replace entire content with:

```csharp
using Microsoft.EntityFrameworkCore;
using Sitbrief.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<SitbriefDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations automatically in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SitbriefDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevelopmentCors");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Step 4: Build to verify**

```bash
cd ../Sitbrief.API
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 5: Commit**

```bash
git add .
git commit -m "feat(api): configure API with DbContext and CORS

- Add DbContext registration with SQLite
- Configure CORS for development
- Enable automatic migrations in development
- Update appsettings with connection string

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 6: Create Repository Interfaces

**Files:**
- Create: `src/Sitbrief.Core/Interfaces/IArticleRepository.cs`
- Create: `src/Sitbrief.Core/Interfaces/ITopicRepository.cs`

**Step 1: Create IArticleRepository interface**

Create file: `src/Sitbrief.Core/Interfaces/IArticleRepository.cs`

```csharp
using Sitbrief.Core.Entities;

namespace Sitbrief.Core.Interfaces;

public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(int id);
    Task<IEnumerable<Article>> GetAllAsync();
    Task<Article> AddAsync(Article article);
    Task UpdateAsync(Article article);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

**Step 2: Create ITopicRepository interface**

Create file: `src/Sitbrief.Core/Interfaces/ITopicRepository.cs`

```csharp
using Sitbrief.Core.Entities;

namespace Sitbrief.Core.Interfaces;

public interface ITopicRepository
{
    Task<Topic?> GetByIdAsync(int id);
    Task<IEnumerable<Topic>> GetAllAsync();
    Task<Topic?> GetWithArticlesAsync(int id);
    Task<Topic> AddAsync(Topic topic);
    Task UpdateAsync(Topic topic);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

**Step 3: Build to verify**

```bash
cd ../Sitbrief.Core
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add .
git commit -m "feat(core): add repository interfaces

- Add IArticleRepository for article data operations
- Add ITopicRepository for topic data operations
- Define async methods for CRUD operations

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 7: Implement Repositories

**Files:**
- Create: `src/Sitbrief.Infrastructure/Repositories/ArticleRepository.cs`
- Create: `src/Sitbrief.Infrastructure/Repositories/TopicRepository.cs`

**Step 1: Create ArticleRepository implementation**

Create file: `src/Sitbrief.Infrastructure/Repositories/ArticleRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;
using Sitbrief.Infrastructure.Data;

namespace Sitbrief.Infrastructure.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly SitbriefDbContext _context;

    public ArticleRepository(SitbriefDbContext context)
    {
        _context = context;
    }

    public async Task<Article?> GetByIdAsync(int id)
    {
        return await _context.Articles
            .Include(a => a.ArticleTopics)
            .ThenInclude(at => at.Topic)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Article>> GetAllAsync()
    {
        return await _context.Articles
            .Include(a => a.ArticleTopics)
            .ThenInclude(at => at.Topic)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<Article> AddAsync(Article article)
    {
        article.CreatedDate = DateTime.UtcNow;
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task UpdateAsync(Article article)
    {
        _context.Articles.Update(article);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article != null)
        {
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Articles.AnyAsync(a => a.Id == id);
    }
}
```

**Step 2: Create TopicRepository implementation**

Create file: `src/Sitbrief.Infrastructure/Repositories/TopicRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;
using Sitbrief.Infrastructure.Data;

namespace Sitbrief.Infrastructure.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly SitbriefDbContext _context;

    public TopicRepository(SitbriefDbContext context)
    {
        _context = context;
    }

    public async Task<Topic?> GetByIdAsync(int id)
    {
        return await _context.Topics.FindAsync(id);
    }

    public async Task<IEnumerable<Topic>> GetAllAsync()
    {
        return await _context.Topics
            .OrderByDescending(t => t.LastUpdatedDate)
            .ToListAsync();
    }

    public async Task<Topic?> GetWithArticlesAsync(int id)
    {
        return await _context.Topics
            .Include(t => t.ArticleTopics)
            .ThenInclude(at => at.Article)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Topic> AddAsync(Topic topic)
    {
        topic.CreatedDate = DateTime.UtcNow;
        topic.LastUpdatedDate = DateTime.UtcNow;
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic;
    }

    public async Task UpdateAsync(Topic topic)
    {
        topic.LastUpdatedDate = DateTime.UtcNow;
        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic != null)
        {
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Topics.AnyAsync(t => t.Id == id);
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
git add .
git commit -m "feat(infrastructure): implement repositories

- Add ArticleRepository with EF Core queries
- Add TopicRepository with EF Core queries
- Include related entities in queries
- Set timestamps on create/update

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 8: Register Repositories in DI Container

**Files:**
- Modify: `src/Sitbrief.API/Program.cs`

**Step 1: Register repositories in Program.cs**

Edit file: `src/Sitbrief.API/Program.cs`

Add after the DbContext registration (around line 15):

```csharp
// Add repositories
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
```

Also add the using statements at the top:

```csharp
using Sitbrief.Core.Interfaces;
using Sitbrief.Infrastructure.Repositories;
```

**Step 2: Build to verify**

```bash
cd Sitbrief.API
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add .
git commit -m "feat(api): register repositories in dependency injection

- Register IArticleRepository with ArticleRepository
- Register ITopicRepository with TopicRepository
- Configure scoped lifetime for repositories

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 9: Create DTOs for API Responses

**Files:**
- Create: `src/Sitbrief.API/DTOs/ArticleDto.cs`
- Create: `src/Sitbrief.API/DTOs/TopicDto.cs`
- Create: `src/Sitbrief.API/DTOs/CreateArticleDto.cs`
- Create: `src/Sitbrief.API/DTOs/CreateTopicDto.cs`

**Step 1: Create ArticleDto**

Create file: `src/Sitbrief.API/DTOs/ArticleDto.cs`

```csharp
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
```

**Step 2: Create TopicDto**

Create file: `src/Sitbrief.API/DTOs/TopicDto.cs`

```csharp
namespace Sitbrief.API.DTOs;

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

Create file: `src/Sitbrief.API/DTOs/CreateArticleDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using Sitbrief.Core.Enums;

namespace Sitbrief.API.DTOs;

public class CreateArticleDto
{
    [Required]
    [MaxLength(500)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Summary { get; set; }

    [Required]
    [Url]
    [MaxLength(1000)]
    public required string SourceUrl { get; set; }

    [Required]
    [MaxLength(200)]
    public required string SourceName { get; set; }

    [Required]
    public SourceType SourceType { get; set; }

    [Required]
    public DateTime PublishedDate { get; set; }

    [MaxLength(50000)]
    public string? Content { get; set; }
}
```

**Step 4: Create CreateTopicDto**

Create file: `src/Sitbrief.API/DTOs/CreateTopicDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Sitbrief.API.DTOs;

public class CreateTopicDto
{
    [Required]
    [MaxLength(300)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Description { get; set; }

    [MaxLength(1000)]
    public string? Significance { get; set; }
}
```

**Step 5: Build to verify**

```bash
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 6: Commit**

```bash
git add .
git commit -m "feat(api): add DTOs for API requests and responses

- Add ArticleDto and TopicDto for responses
- Add CreateArticleDto and CreateTopicDto for requests
- Add validation attributes
- Separate summary and detail DTOs

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 10: Create Articles Controller

**Files:**
- Create: `src/Sitbrief.API/Controllers/ArticlesController.cs`

**Step 1: Create ArticlesController**

Create file: `src/Sitbrief.API/Controllers/ArticlesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Sitbrief.API.DTOs;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;

namespace Sitbrief.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleRepository _articleRepository;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(
        IArticleRepository articleRepository,
        ILogger<ArticlesController> logger)
    {
        _articleRepository = articleRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAllArticles()
    {
        try
        {
            var articles = await _articleRepository.GetAllAsync();
            var articleDtos = articles.Select(MapToDto).ToList();
            return Ok(articleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving articles");
            return StatusCode(500, "An error occurred while retrieving articles");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleDto>> GetArticle(int id)
    {
        try
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound($"Article with ID {id} not found");
            }

            return Ok(MapToDto(article));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article {ArticleId}", id);
            return StatusCode(500, "An error occurred while retrieving the article");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ArticleDto>> CreateArticle(CreateArticleDto createDto)
    {
        try
        {
            var article = new Article
            {
                Title = createDto.Title,
                Summary = createDto.Summary,
                SourceUrl = createDto.SourceUrl,
                SourceName = createDto.SourceName,
                SourceType = createDto.SourceType,
                Content = createDto.Content,
                PublishedDate = createDto.PublishedDate
            };

            var createdArticle = await _articleRepository.AddAsync(article);
            var articleDto = MapToDto(createdArticle);

            return CreatedAtAction(
                nameof(GetArticle),
                new { id = createdArticle.Id },
                articleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article");
            return StatusCode(500, "An error occurred while creating the article");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArticle(int id, CreateArticleDto updateDto)
    {
        try
        {
            var existingArticle = await _articleRepository.GetByIdAsync(id);
            if (existingArticle == null)
            {
                return NotFound($"Article with ID {id} not found");
            }

            existingArticle.Title = updateDto.Title;
            existingArticle.Summary = updateDto.Summary;
            existingArticle.SourceUrl = updateDto.SourceUrl;
            existingArticle.SourceName = updateDto.SourceName;
            existingArticle.SourceType = updateDto.SourceType;
            existingArticle.Content = updateDto.Content;
            existingArticle.PublishedDate = updateDto.PublishedDate;

            await _articleRepository.UpdateAsync(existingArticle);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", id);
            return StatusCode(500, "An error occurred while updating the article");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        try
        {
            var exists = await _articleRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound($"Article with ID {id} not found");
            }

            await _articleRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return StatusCode(500, "An error occurred while deleting the article");
        }
    }

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Summary = article.Summary,
            SourceUrl = article.SourceUrl,
            SourceName = article.SourceName,
            SourceType = article.SourceType,
            PublishedDate = article.PublishedDate,
            CreatedDate = article.CreatedDate,
            Topics = article.ArticleTopics.Select(at => new TopicSummaryDto
            {
                Id = at.Topic.Id,
                Title = at.Topic.Title
            }).ToList()
        };
    }
}
```

**Step 2: Build to verify**

```bash
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add .
git commit -m "feat(api): add Articles controller with CRUD endpoints

- Add GET /api/articles (all articles)
- Add GET /api/articles/{id} (single article)
- Add POST /api/articles (create article)
- Add PUT /api/articles/{id} (update article)
- Add DELETE /api/articles/{id} (delete article)
- Include error handling and logging

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 11: Create Topics Controller

**Files:**
- Create: `src/Sitbrief.API/Controllers/TopicsController.cs`

**Step 1: Create TopicsController**

Create file: `src/Sitbrief.API/Controllers/TopicsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Sitbrief.API.DTOs;
using Sitbrief.Core.Entities;
using Sitbrief.Core.Interfaces;

namespace Sitbrief.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITopicRepository _topicRepository;
    private readonly ILogger<TopicsController> _logger;

    public TopicsController(
        ITopicRepository topicRepository,
        ILogger<TopicsController> logger)
    {
        _topicRepository = topicRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TopicDto>>> GetAllTopics()
    {
        try
        {
            var topics = await _topicRepository.GetAllAsync();
            var topicDtos = topics.Select(MapToDto).ToList();
            return Ok(topicDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving topics");
            return StatusCode(500, "An error occurred while retrieving topics");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TopicDetailDto>> GetTopic(int id)
    {
        try
        {
            var topic = await _topicRepository.GetWithArticlesAsync(id);
            if (topic == null)
            {
                return NotFound($"Topic with ID {id} not found");
            }

            return Ok(MapToDetailDto(topic));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving topic {TopicId}", id);
            return StatusCode(500, "An error occurred while retrieving the topic");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TopicDto>> CreateTopic(CreateTopicDto createDto)
    {
        try
        {
            var topic = new Topic
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Significance = createDto.Significance
            };

            var createdTopic = await _topicRepository.AddAsync(topic);
            var topicDto = MapToDto(createdTopic);

            return CreatedAtAction(
                nameof(GetTopic),
                new { id = createdTopic.Id },
                topicDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating topic");
            return StatusCode(500, "An error occurred while creating the topic");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTopic(int id, CreateTopicDto updateDto)
    {
        try
        {
            var existingTopic = await _topicRepository.GetByIdAsync(id);
            if (existingTopic == null)
            {
                return NotFound($"Topic with ID {id} not found");
            }

            existingTopic.Title = updateDto.Title;
            existingTopic.Description = updateDto.Description;
            existingTopic.Significance = updateDto.Significance;

            await _topicRepository.UpdateAsync(existingTopic);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating topic {TopicId}", id);
            return StatusCode(500, "An error occurred while updating the topic");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTopic(int id)
    {
        try
        {
            var exists = await _topicRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound($"Topic with ID {id} not found");
            }

            await _topicRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting topic {TopicId}", id);
            return StatusCode(500, "An error occurred while deleting the topic");
        }
    }

    private static TopicDto MapToDto(Topic topic)
    {
        return new TopicDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            Significance = topic.Significance,
            CreatedDate = topic.CreatedDate,
            LastUpdatedDate = topic.LastUpdatedDate,
            ArticleCount = topic.ArticleTopics?.Count ?? 0
        };
    }

    private static TopicDetailDto MapToDetailDto(Topic topic)
    {
        return new TopicDetailDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Description = topic.Description,
            Significance = topic.Significance,
            CreatedDate = topic.CreatedDate,
            LastUpdatedDate = topic.LastUpdatedDate,
            ArticleCount = topic.ArticleTopics?.Count ?? 0,
            Articles = topic.ArticleTopics.Select(at => new ArticleDto
            {
                Id = at.Article.Id,
                Title = at.Article.Title,
                Summary = at.Article.Summary,
                SourceUrl = at.Article.SourceUrl,
                SourceName = at.Article.SourceName,
                SourceType = at.Article.SourceType,
                PublishedDate = at.Article.PublishedDate,
                CreatedDate = at.Article.CreatedDate,
                Topics = new List<TopicSummaryDto>()
            }).ToList()
        };
    }
}
```

**Step 2: Build to verify**

```bash
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 3: Commit**

```bash
git add .
git commit -m "feat(api): add Topics controller with CRUD endpoints

- Add GET /api/topics (all topics)
- Add GET /api/topics/{id} (topic with articles)
- Add POST /api/topics (create topic)
- Add PUT /api/topics/{id} (update topic)
- Add DELETE /api/topics/{id} (delete topic)
- Include error handling and logging

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 12: Add Seed Data

**Files:**
- Create: `src/Sitbrief.Infrastructure/Data/DbInitializer.cs`
- Modify: `src/Sitbrief.API/Program.cs`

**Step 1: Create DbInitializer**

Create file: `src/Sitbrief.Infrastructure/Data/DbInitializer.cs`

```csharp
using Sitbrief.Core.Entities;
using Sitbrief.Core.Enums;

namespace Sitbrief.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(SitbriefDbContext context)
    {
        // Check if data already exists
        if (context.Articles.Any() || context.Topics.Any())
        {
            return; // Database has been seeded
        }

        // Create topics
        var topic1 = new Topic
        {
            Title = "中美南海爭議",
            Description = "中國與美國在南海地區的軍事對峙和領土爭議",
            Significance = "影響亞太地區軍事平衡和國際航運安全",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        var topic2 = new Topic
        {
            Title = "歐盟能源危機",
            Description = "歐洲各國尋求能源獨立和替代能源的挑戰",
            Significance = "關乎歐洲經濟穩定和能源安全",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        var topic3 = new Topic
        {
            Title = "中東和平進程",
            Description = "以色列與巴勒斯坦的和平談判進展",
            Significance = "影響中東地區長期穩定",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        context.Topics.AddRange(topic1, topic2, topic3);
        await context.SaveChangesAsync();

        // Create articles
        var article1 = new Article
        {
            Title = "美國海軍在南海進行軍事演習",
            Summary = "美國第七艦隊在南海爭議海域進行為期一週的軍事演習，引發中國強烈抗議。演習包括航母打擊群和兩棲作戰訓練。",
            SourceUrl = "https://example.com/article1",
            SourceName = "華爾街日報",
            SourceType = SourceType.NewsMedia,
            PublishedDate = DateTime.UtcNow.AddDays(-2),
            CreatedDate = DateTime.UtcNow
        };

        var article2 = new Article
        {
            Title = "CSIS報告：南海軍事化趨勢分析",
            Summary = "戰略與國際研究中心發布最新報告，分析中美在南海的軍事部署和未來可能的衝突點。報告警告區域緊張局勢升溫。",
            SourceUrl = "https://example.com/article2",
            SourceName = "CSIS",
            SourceType = SourceType.ThinkTank,
            PublishedDate = DateTime.UtcNow.AddDays(-3),
            CreatedDate = DateTime.UtcNow
        };

        var article3 = new Article
        {
            Title = "德國宣布新能源政策",
            Summary = "德國政府宣布加速發展再生能源，計劃在2030年前將再生能源佔比提高至80%。此舉旨在減少對俄羅斯天然氣的依賴。",
            SourceUrl = "https://example.com/article3",
            SourceName = "金融時報",
            SourceType = SourceType.NewsMedia,
            PublishedDate = DateTime.UtcNow.AddDays(-1),
            CreatedDate = DateTime.UtcNow
        };

        var article4 = new Article
        {
            Title = "布魯金斯研究所：歐洲能源轉型挑戰",
            Summary = "布魯金斯研究所發表深度分析，探討歐盟各國在能源轉型過程中面臨的經濟和技術挑戰，以及可能的解決方案。",
            SourceUrl = "https://example.com/article4",
            SourceName = "Brookings Institution",
            SourceType = SourceType.ThinkTank,
            PublishedDate = DateTime.UtcNow.AddDays(-2),
            CreatedDate = DateTime.UtcNow
        };

        context.Articles.AddRange(article1, article2, article3, article4);
        await context.SaveChangesAsync();

        // Create article-topic relationships
        var articleTopic1 = new ArticleTopic
        {
            ArticleId = article1.Id,
            TopicId = topic1.Id,
            Confidence = 0.95f,
            IsConfirmed = true,
            AddedDate = DateTime.UtcNow
        };

        var articleTopic2 = new ArticleTopic
        {
            ArticleId = article2.Id,
            TopicId = topic1.Id,
            Confidence = 0.98f,
            IsConfirmed = true,
            AddedDate = DateTime.UtcNow
        };

        var articleTopic3 = new ArticleTopic
        {
            ArticleId = article3.Id,
            TopicId = topic2.Id,
            Confidence = 0.92f,
            IsConfirmed = true,
            AddedDate = DateTime.UtcNow
        };

        var articleTopic4 = new ArticleTopic
        {
            ArticleId = article4.Id,
            TopicId = topic2.Id,
            Confidence = 0.96f,
            IsConfirmed = true,
            AddedDate = DateTime.UtcNow
        };

        context.ArticleTopics.AddRange(articleTopic1, articleTopic2, articleTopic3, articleTopic4);
        await context.SaveChangesAsync();
    }
}
```

**Step 2: Update Program.cs to call seed method**

Edit file: `src/Sitbrief.API/Program.cs`

Modify the migration section (around line 30) to:

```csharp
// Apply migrations and seed data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SitbriefDbContext>();
    dbContext.Database.Migrate();
    await DbInitializer.SeedAsync(dbContext);
}
```

Add using statement at top:

```csharp
using Sitbrief.Infrastructure.Data;
```

**Step 3: Build to verify**

```bash
dotnet build
```

Expected: BUILD SUCCEEDED

**Step 4: Commit**

```bash
git add .
git commit -m "feat(infrastructure): add seed data for development

- Create DbInitializer with sample topics and articles
- Add geopolitical examples in Chinese
- Create article-topic relationships
- Auto-seed on startup in development mode

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Task 13: Test the API

**Files:**
- None (testing only)

**Step 1: Delete existing database (if any)**

```bash
cd ../..
rm -f src/Sitbrief.API/sitbrief.db
```

**Step 2: Run the API**

```bash
cd src/Sitbrief.API
dotnet run
```

Expected: Application starts, shows "Now listening on: http://localhost:5xxx"

**Step 3: Open Swagger UI**

Open browser to: `http://localhost:5xxx/swagger` (use the port from step 2)

Expected: Swagger UI loads with all endpoints visible

**Step 4: Test GET /api/topics**

In Swagger UI:
1. Click on "GET /api/topics"
2. Click "Try it out"
3. Click "Execute"

Expected: Returns 3 topics with Chinese titles

**Step 5: Test GET /api/topics/{id}**

In Swagger UI:
1. Click on "GET /api/topics/{id}"
2. Click "Try it out"
3. Enter id: 1
4. Click "Execute"

Expected: Returns topic with related articles

**Step 6: Test GET /api/articles**

In Swagger UI:
1. Click on "GET /api/articles"
2. Click "Try it out"
3. Click "Execute"

Expected: Returns 4 articles

**Step 7: Test POST /api/articles**

In Swagger UI:
1. Click on "POST /api/articles"
2. Click "Try it out"
3. Use this JSON:

```json
{
  "title": "台海局勢新發展",
  "summary": "兩岸關係出現新的對話機會",
  "sourceUrl": "https://example.com/new-article",
  "sourceName": "外交事務",
  "sourceType": 0,
  "publishedDate": "2026-01-22T10:00:00Z"
}
```

4. Click "Execute"

Expected: Returns 201 Created with the new article

**Step 8: Verify database file created**

```bash
ls -lh sitbrief.db
```

Expected: Database file exists with reasonable size

**Step 9: Stop the API**

Press Ctrl+C in the terminal

**Step 10: Document test results**

All tests should pass. If any fail, investigate before proceeding.

---

## Task 14: Add README

**Files:**
- Create: `README.md`

**Step 1: Create README**

Create file: `/Users/fanghuaian/Documents/Projects/Sitbrief/README.md`

```markdown
# Sitbrief

A geopolitical news aggregation platform focused on international relations and strategic analysis.

## Overview

Sitbrief helps users build situational awareness by aggregating articles from premium news sources and think tanks, organizing them by topics, and providing AI-assisted content curation.

## Project Structure

```
Sitbrief/
├── src/
│   ├── Sitbrief.API/          # ASP.NET Core Web API
│   ├── Sitbrief.Core/         # Domain entities and interfaces
│   └── Sitbrief.Infrastructure/ # Data access with EF Core
├── docs/
│   └── plans/                 # Design and implementation docs
└── README.md
```

## Tech Stack

- **Backend:** ASP.NET Core 8.0
- **Database:** SQLite (development), PostgreSQL (production)
- **ORM:** Entity Framework Core
- **API Documentation:** Swagger/OpenAPI

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Git

### Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd Sitbrief
```

2. Restore dependencies:
```bash
cd src
dotnet restore
```

3. Run the API:
```bash
cd Sitbrief.API
dotnet run
```

4. Open Swagger UI:
```
http://localhost:5xxx/swagger
```

The database will be created automatically with seed data on first run.

## API Endpoints

### Topics

- `GET /api/topics` - Get all topics
- `GET /api/topics/{id}` - Get topic with articles
- `POST /api/topics` - Create a new topic
- `PUT /api/topics/{id}` - Update a topic
- `DELETE /api/topics/{id}` - Delete a topic

### Articles

- `GET /api/articles` - Get all articles
- `GET /api/articles/{id}` - Get a single article
- `POST /api/articles` - Create a new article
- `PUT /api/articles/{id}` - Update an article
- `DELETE /api/articles/{id}` - Delete an article

## Development Status

✅ Phase 1: Backend Foundation (Complete)
- Solution structure
- Domain entities
- EF Core with SQLite
- Repository pattern
- RESTful API endpoints
- Seed data

🚧 Phase 2: Admin Web App (Planned)
- Blazor WebAssembly
- Authentication
- Article management UI
- Topic management UI

🚧 Phase 3: AI Integration (Planned)
- Claude API integration
- Automatic topic suggestions
- Content analysis

🚧 Phase 4: iOS App (Planned)
- Native Swift app
- Situational awareness UI
- Bookmark functionality

## Documentation

- [Design Document](docs/plans/2026-01-22-sitbrief-design.md)
- [Phase 1 Implementation Plan](docs/plans/2026-01-22-phase1-backend-foundation.md)

## License

Private project - All rights reserved
```

**Step 2: Commit**

```bash
cd ../..
git add README.md
git commit -m "docs: add README with project overview and setup

- Add project structure overview
- Document API endpoints
- Add getting started guide
- Include development status

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

---

## Phase 1 Complete! 🎉

You now have a fully functional backend API with:

- ✅ Three-layer architecture (API, Core, Infrastructure)
- ✅ Entity Framework Core with SQLite
- ✅ Domain entities (Article, Topic, ArticleTopic, AIAnalysis)
- ✅ Repository pattern
- ✅ RESTful API endpoints for CRUD operations
- ✅ Swagger documentation
- ✅ Seed data for testing
- ✅ Proper error handling and logging

### Verification Checklist

- [ ] Solution builds without errors
- [ ] API runs and listens on localhost
- [ ] Swagger UI is accessible
- [ ] All GET endpoints return data
- [ ] POST endpoints create new records
- [ ] Database file is created
- [ ] Seed data is loaded

### Next Steps

**Phase 2: Admin Web App** will include:
- Blazor WebAssembly project
- Authentication system
- Article management UI
- Topic management UI
- AI integration preparation

---

## Troubleshooting

**Build errors:**
- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

**Migration errors:**
- Delete database: `rm sitbrief.db`
- Re-run application

**Port conflicts:**
- The API uses a random port by default
- Check console output for the actual port

**Database locked:**
- Stop the application
- Delete the database file
- Restart the application
