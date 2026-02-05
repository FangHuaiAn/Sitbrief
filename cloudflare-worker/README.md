# Sitbrief API Gateway - Cloudflare Worker

iOS App 存取 R2 檔案的 API Gateway，提供 Token 驗證和檔案代理功能。

## 功能

- ✅ Access Token 驗證
- ✅ R2 檔案代理
- ✅ CORS 支援
- ✅ 快取控制
- ✅ 完全免費（Cloudflare Workers 免費配額內）

## API 端點

| 端點 | 說明 |
|------|------|
| `GET /` | Health check |
| `GET /api/articles` | 取得文章列表 |
| `GET /api/topics` | 取得主題列表 |
| `GET /api/metadata` | 取得 metadata |

## 認證

所有 API 請求需要在 Header 中包含 Access Token：

```
Authorization: Bearer YOUR_ACCESS_TOKEN
```

或直接：

```
Authorization: YOUR_ACCESS_TOKEN
```

## 部署步驟

### 1. 安裝依賴

```bash
cd cloudflare-worker
npm install
```

### 2. 登入 Cloudflare

```bash
npx wrangler login
```

### 3. 設定 Access Token（Secret）

```bash
# 產生一個隨機 token（建議使用）
openssl rand -hex 32

# 設定為 Cloudflare Secret
npx wrangler secret put ACCESS_TOKEN
# 然後貼上你的 token
```

### 4. 更新網域設定

編輯 `wrangler.toml`，將 `yourdomain.com` 替換成你的網域：

```toml
[env.production]
routes = [
  { pattern = "api.yourdomain.com/*", zone_name = "yourdomain.com" }
]
```

### 5. 本地測試

```bash
npm run dev
```

測試 API：

```bash
# Health check
curl http://localhost:8787/

# 測試認證（需先設定 .dev.vars）
curl -H "Authorization: YOUR_TOKEN" http://localhost:8787/api/metadata
```

### 6. 部署到生產環境

```bash
npm run deploy:production
```

## 本地開發設定

建立 `.dev.vars` 檔案（不要提交到 git）：

```
ACCESS_TOKEN=your-test-token-here
```

## iOS App 整合

### Swift 範例

```swift
let baseURL = "https://api.yourdomain.com"
let token = "YOUR_ACCESS_TOKEN"

func fetchArticles() async throws -> Data {
    let url = URL(string: "\(baseURL)/api/articles")!
    var request = URLRequest(url: url)
    request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
    
    let (data, response) = try await URLSession.shared.data(for: request)
    
    guard let httpResponse = response as? HTTPURLResponse,
          httpResponse.statusCode == 200 else {
        throw NetworkError.invalidResponse
    }
    
    return data
}
```

## 成本

完全免費！Cloudflare Workers 免費配額：
- 每天 100,000 次請求
- 無限流量
- R2 讀取操作：每月 1000 萬次免費

單人使用的 Sitbrief 絕對在免費額度內。

## 安全建議

1. **定期更換 Token**：建議每月更換一次
2. **監控使用量**：在 Cloudflare Dashboard 查看請求量
3. **限制來源**：可以在 Worker 中加入 IP 白名單（選用）

## 故障排除

### 401 Unauthorized

- 檢查 ACCESS_TOKEN 是否正確設定
- 確認 Header 格式正確

### 404 Not Found

- 確認 R2 bucket 中有對應檔案
- 檢查檔案路徑是否正確（`Brief/articles.json`）

### 500 Internal Server Error

- 檢查 wrangler.toml 中的 R2 綁定設定
- 查看 Worker 日誌：`npm run tail`
