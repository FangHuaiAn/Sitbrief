# Sitbrief Azure 部署指南（免費方案）

本指南說明如何將 Sitbrief API 部署到 Azure App Service 免費層。

## 📋 前置準備

1. [Azure 帳戶](https://azure.microsoft.com/free/)（免費帳戶即可）
2. [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) 已安裝
3. .NET 8.0 SDK

## 🔐 重要：設定生產環境密碼

部署前，您需要產生新的密碼 hash。**請勿使用開發環境的密碼。**

### 產生新密碼 Hash

```bash
# 在專案目錄執行
cd src/Sitbrief.API
dotnet run --environment Development -- --generate-password

# 或使用以下 C# 程式碼產生：
# BCrypt.Net.BCrypt.HashPassword("您的強密碼")
```

### 產生 JWT Secret

```bash
# 產生隨機 32+ 字元的密鑰
openssl rand -base64 32
```

記下這些值，稍後會用到。

## 🚀 部署步驟

### 1. 登入 Azure

```bash
az login
```

### 2. 設定變數

```bash
# 自訂這些值
RESOURCE_GROUP="sitbrief-rg"
APP_NAME="sitbrief-api"          # 必須全球唯一，會成為 xxx.azurewebsites.net
LOCATION="eastasia"              # 或 japaneast, southeastasia

# 安全設定（請替換成您的值）
ADMIN_USERNAME="your-admin-username"
ADMIN_PASSWORD_HASH='$2a$11$xxxxxx'   # BCrypt hash，用單引號
JWT_SECRET="your-32-character-or-longer-secret-key-here"
CLAUDE_API_KEY="your-claude-api-key"  # 可選，沒有則 AI 功能不可用
```

### 3. 建立資源群組

```bash
az group create --name $RESOURCE_GROUP --location $LOCATION
```

### 4. 建立 App Service Plan（免費層）

```bash
az appservice plan create \
  --name "${APP_NAME}-plan" \
  --resource-group $RESOURCE_GROUP \
  --sku F1 \
  --is-linux
```

### 5. 建立 Web App

```bash
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan "${APP_NAME}-plan" \
  --runtime "DOTNETCORE:8.0"
```

### 6. 設定環境變數（重要！）

```bash
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "ASPNETCORE_ENVIRONMENT=Production" \
    "Authentication__AdminUsername=$ADMIN_USERNAME" \
    "Authentication__AdminPasswordHash=$ADMIN_PASSWORD_HASH" \
    "Authentication__JwtSecret=$JWT_SECRET" \
    "Claude__ApiKey=$CLAUDE_API_KEY"
```

### 7. 建立資料目錄

```bash
# 啟用持久化儲存（SQLite 資料庫需要）
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings "WEBSITES_ENABLE_APP_SERVICE_STORAGE=true"
```

### 8. 發布應用程式

```bash
cd src/Sitbrief.API

# 建置發布版本
dotnet publish -c Release -o ./publish

# 建立 ZIP 檔案
cd publish
zip -r ../deploy.zip .
cd ..

# 部署到 Azure
az webapp deploy \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --src-path deploy.zip \
  --type zip

# 清理
rm -rf publish deploy.zip
```

### 9. 驗證部署

```bash
# 檢查健康狀態
curl https://${APP_NAME}.azurewebsites.net/health

# 測試登入
curl -X POST https://${APP_NAME}.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$ADMIN_USERNAME\",\"password\":\"您的密碼\"}"
```

## 🖥️ 本地使用 Admin

在您的電腦上執行 Blazor Admin，連接到 Azure API：

### 1. 修改 Admin 設定

編輯 `src/Sitbrief.Admin/wwwroot/appsettings.json`：

```json
{
  "ApiBaseUrl": "https://sitbrief-api.azurewebsites.net"
}
```

### 2. 執行 Admin

```bash
cd src/Sitbrief.Admin
dotnet run
```

### 3. 登入

開啟 http://localhost:5014，使用您設定的帳號密碼登入。

## 📊 監控與維護

### 查看日誌

```bash
az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP
```

### 重啟應用

```bash
az webapp restart --name $APP_NAME --resource-group $RESOURCE_GROUP
```

### 備份資料庫

```bash
# 下載 SQLite 資料庫檔案
az webapp log download \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --log-file sitbrief-backup.zip

# 或使用 Kudu Console: https://{APP_NAME}.scm.azurewebsites.net
# 導航到 /home/site/wwwroot/data/ 下載 sitbrief.db
```

## ⚠️ 免費層限制

| 限制項目 | 說明 |
|---------|------|
| **CPU 時間** | 每天 60 分鐘 |
| **記憶體** | 1 GB |
| **儲存空間** | 1 GB |
| **自訂網域** | ❌ 不支援 |
| **SSL 憑證** | ✅ 內建 (*.azurewebsites.net) |
| **實例數** | 1（無法擴展） |

如果超出限制，應用會暫停到隔天。如需更穩定服務，可升級到 B1（~$13/月）：

```bash
az appservice plan update \
  --name "${APP_NAME}-plan" \
  --resource-group $RESOURCE_GROUP \
  --sku B1
```

## 🗑️ 清理資源

如果不再需要，刪除所有資源：

```bash
az group delete --name $RESOURCE_GROUP --yes
```

## 🔧 故障排除

### API 無法啟動

```bash
# 檢查日誌
az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP

# 確認環境變數已設定
az webapp config appsettings list --name $APP_NAME --resource-group $RESOURCE_GROUP
```

### 資料庫錯誤

確認 SQLite 資料庫路徑正確，且 `/home/site/wwwroot/data/` 目錄存在。

### CORS 錯誤

如果 Admin 無法連接 API，新增您的本地位址到 AllowedOrigins：

```bash
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings "AllowedOrigins__0=http://localhost:5014"
```
