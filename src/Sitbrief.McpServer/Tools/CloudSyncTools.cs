using System.ComponentModel;
using Amazon.S3;
using Amazon.S3.Model;
using ModelContextProtocol.Server;

namespace Sitbrief.McpServer.Tools;

[McpServerToolType]
public class CloudSyncTools
{
    private const string BucketName = "statbrief";
    private const string R2Endpoint = "https://0cfbb72c4eab7aaf66611ab26f2e9d75.r2.cloudflarestorage.com";
    private const string BriefFolder = "Brief";
    
    private static readonly string OutputPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Documents", "Projects", "Sitbrief", "output");

    private AmazonS3Client CreateR2Client()
    {
        var accessKey = Environment.GetEnvironmentVariable("CLOUDFLARE_ACCESSKEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("CLOUDFLARE_SECRET_ACCESSKEY");

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException(
                "請設定環境變數 CLOUDFLARE_ACCESSKEY_ID 和 CLOUDFLARE_SECRET_ACCESSKEY");
        }

        var config = new AmazonS3Config
        {
            ServiceURL = R2Endpoint,
            ForcePathStyle = true,
            SignatureVersion = "v4"
        };

        return new AmazonS3Client(accessKey, secretKey, config);
    }

    [McpServerTool, Description("將 JSON 檔案同步到 Cloudflare R2（支援分頁結構）")]
    public async Task<string> SyncToCloud()
    {
        // 檢查本地檔案是否存在
        var metadataPath = Path.Combine(OutputPath, "metadata.json");
        var topicsPath = Path.Combine(OutputPath, "topics.json");
        var articlesDir = Path.Combine(OutputPath, "articles");

        if (!File.Exists(metadataPath))
        {
            return "❌ 尚未匯出 JSON 檔案。請先執行 ExportJson。";
        }

        try
        {
            using var client = CreateR2Client();
            var uploadedFiles = new List<string>();

            // 上傳 metadata.json
            await UploadFileAsync(client, metadataPath, $"{BriefFolder}/metadata.json");
            uploadedFiles.Add("metadata.json");

            // 上傳 topics.json
            await UploadFileAsync(client, topicsPath, $"{BriefFolder}/topics.json");
            uploadedFiles.Add("topics.json");

            // 上傳 articles 目錄下所有檔案
            if (Directory.Exists(articlesDir))
            {
                var articleFiles = Directory.GetFiles(articlesDir, "*.json");
                foreach (var file in articleFiles)
                {
                    var fileName = Path.GetFileName(file);
                    await UploadFileAsync(client, file, $"{BriefFolder}/articles/{fileName}");
                    uploadedFiles.Add($"articles/{fileName}");
                }
            }

            return $"""
                ✅ 同步完成！
                
                已上傳到 Cloudflare R2 ({uploadedFiles.Count} 個檔案)：
                - {string.Join("\n- ", uploadedFiles)}
                
                📎 API 端點：
                https://api.strataperture.net/api/metadata
                https://api.strataperture.net/api/topics
                https://api.strataperture.net/api/articles/latest
                https://api.strataperture.net/api/articles/page/1
                """;
        }
        catch (Exception ex)
        {
            return $"❌ 同步失敗：{ex.Message}";
        }
    }

    [McpServerTool, Description("檢查 R2 bucket 中的檔案")]
    public async Task<string> ListCloudFiles()
    {
        try
        {
            using var client = CreateR2Client();
            
            var request = new ListObjectsV2Request
            {
                BucketName = BucketName,
                Prefix = $"{BriefFolder}/"
            };

            var response = await client.ListObjectsV2Async(request);

            if (response.S3Objects.Count == 0)
            {
                return $"📁 {BriefFolder}/ 目錄中沒有檔案。";
            }

            var files = response.S3Objects
                .OrderBy(obj => obj.Key)
                .Select(obj => $"- {obj.Key} ({obj.Size / 1024.0:F1} KB)")
                .ToList();

            return $"""
                📁 Cloudflare R2 ({BucketName}/{BriefFolder}/)：
                
                {string.Join("\n", files)}
                
                共 {response.S3Objects.Count} 個檔案
                """;
        }
        catch (Exception ex)
        {
            return $"❌ 列出檔案失敗：{ex.Message}";
        }
    }

    [McpServerTool, Description("清除 R2 上的舊檔案並重新上傳")]
    public async Task<string> CleanAndSync()
    {
        try
        {
            using var client = CreateR2Client();
            
            // 列出所有現有檔案
            var listRequest = new ListObjectsV2Request
            {
                BucketName = BucketName,
                Prefix = $"{BriefFolder}/"
            };
            var listResponse = await client.ListObjectsV2Async(listRequest);

            // 刪除所有現有檔案
            var deletedCount = 0;
            foreach (var obj in listResponse.S3Objects)
            {
                await client.DeleteObjectAsync(BucketName, obj.Key);
                deletedCount++;
            }

            // 重新同步
            var syncResult = await SyncToCloud();

            return $"""
                🧹 已清除 {deletedCount} 個舊檔案
                
                {syncResult}
                """;
        }
        catch (Exception ex)
        {
            return $"❌ 清除同步失敗：{ex.Message}";
        }
    }

    private async Task UploadFileAsync(AmazonS3Client client, string localPath, string key)
    {
        var fileContent = await File.ReadAllBytesAsync(localPath);
        
        using var stream = new MemoryStream(fileContent);
        var putRequest = new PutObjectRequest
        {
            BucketName = BucketName,
            Key = key,
            InputStream = stream,
            ContentType = "application/json",
            DisablePayloadSigning = true
        };

        await client.PutObjectAsync(putRequest);
    }
}
