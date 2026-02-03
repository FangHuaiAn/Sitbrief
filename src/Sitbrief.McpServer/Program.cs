using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sitbrief.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);

// 設定資料庫連線
var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Documents", "Projects", "Sitbrief", "data", "sitbrief.db");

// 確保目錄存在
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<SitbriefDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// 設定 MCP Server
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<Sitbrief.McpServer.Tools.TopicTools>()
    .WithTools<Sitbrief.McpServer.Tools.ArticleTools>()
    .WithTools<Sitbrief.McpServer.Tools.AnalysisTools>()
    .WithTools<Sitbrief.McpServer.Tools.ExportTools>();

var app = builder.Build();

// 確保資料庫已建立
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SitbriefDbContext>();
    await db.Database.EnsureCreatedAsync();
}

await app.RunAsync();
