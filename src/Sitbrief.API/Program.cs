using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Sitbrief.Core.Interfaces;
using Sitbrief.Infrastructure.Data;
using Sitbrief.Infrastructure.Repositories;
using Sitbrief.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Only enable Swagger in Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
}

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Authentication:JwtSecret"];
var jwtIssuer = builder.Configuration["Authentication:JwtIssuer"];
var jwtAudience = builder.Configuration["Authentication:JwtAudience"];

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "JWT Secret must be configured and at least 32 characters long. " +
        "Set Authentication:JwtSecret in configuration or environment variable.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// Add DbContext - Use SQLite for both dev and production (cost-effective)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SitbriefDbContext>(options =>
    options.UseSqlite(connectionString));

// Add repositories
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();

// Add AI service
builder.Services.AddHttpClient<IAIService, ClaudeAIService>();

// Add Rate Limiting for production
if (builder.Environment.IsProduction())
{
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 10
                }));
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
}

// Add CORS
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        // Production: Only allow specific origins
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SitbriefDbContext>();

// Add Application Insights (if configured)
var appInsightsKey = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsKey))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

var app = builder.Build();

// Apply migrations and ensure database exists
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SitbriefDbContext>();
    dbContext.Database.Migrate();
    
    // Only seed in development
    if (app.Environment.IsDevelopment())
    {
        await DbInitializer.SeedAsync(dbContext);
    }
}

// Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Security headers for production
if (app.Environment.IsProduction())
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
        await next();
    });
    
    app.UseRateLimiter();
    app.UseHsts();
}

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
