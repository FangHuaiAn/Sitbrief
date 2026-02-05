using Microsoft.EntityFrameworkCore;
using Sitbrief.Infrastructure.Data;
using Sitbrief.McpServer.Tools;

// 設定資料庫連線
var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Documents", "Projects", "Sitbrief", "data", "sitbrief.db");

var optionsBuilder = new DbContextOptionsBuilder<SitbriefDbContext>();
optionsBuilder.UseSqlite($"Data Source={dbPath}");

using var db = new SitbriefDbContext(optionsBuilder.Options);

// 檢查命令列參數
if (args.Length > 0 && args[0] == "add")
{
    // 快速新增文章模式 - 從檔案讀取
    var articleFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Documents", "Projects", "Sitbrief", "temp_article.txt");
    
    if (!File.Exists(articleFile))
    {
        Console.WriteLine("❌ 找不到 temp_article.txt 檔案");
        return;
    }
    
    var pastedText = await File.ReadAllTextAsync(articleFile);
    var articleTools = new ArticleTools(db);
    var result = await articleTools.QuickAddArticle(pastedText);
    Console.WriteLine(result);
    
    // 刪除暫存檔
    File.Delete(articleFile);
}
else
{
    // 預設：匯出並同步
    Console.WriteLine("=== Sitbrief 匯出與同步 ===\n");
    
    Console.WriteLine("📦 步驟 1: 匯出 JSON 檔案\n");
    var exportTools = new ExportTools(db);
    var exportResult = await exportTools.ExportJson();
    Console.WriteLine(exportResult);

    Console.WriteLine("\n☁️  步驟 2: 同步到 Cloudflare R2\n");
    var cloudTools = new CloudSyncTools();
    var syncResult = await cloudTools.CleanAndSync();
    Console.WriteLine(syncResult);

    Console.WriteLine("\n✅ 完成！");
}
