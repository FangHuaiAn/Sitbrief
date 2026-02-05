# Sitbrief Cloudflare Worker 快速部署指南

## 前置需求

✅ Cloudflare 帳號（已完成）  
✅ R2 bucket `statbrief` 已建立（已完成）  
☐ 購買網域並加入 Cloudflare  
☐ 安裝 Node.js 18+（如果尚未安裝）

---

## 部署步驟（10 分鐘）

### 1. 登入 Cloudflare

```bash
cd /Users/fanghuaian/Documents/Projects/Sitbrief/cloudflare-worker
npx wrangler login
```

瀏覽器會開啟，登入你的 Cloudflare 帳號。

### 2. 產生並設定 Access Token

```bash
# 產生隨機 token
openssl rand -hex 32

# 複製輸出的 token，然後執行：
npx wrangler secret put ACCESS_TOKEN

# 貼上剛才產生的 token
```

⚠️ **重要**：將這個 token 儲存到安全的地方（如 1Password），iOS App 會需要它。

### 3. 更新網域設定

編輯 `wrangler.toml` 第 14-17 行：

```toml
[env.production]
name = "sitbrief-api-gateway"
routes = [
  { pattern = "api.yourdomain.com/*", zone_name = "yourdomain.com" }
]
```

將 `yourdomain.com` 替換成你購買的網域。

### 4. 部署到生產環境

```bash
npm run deploy:production
```

成功後會顯示：

```
✨ Uploaded sitbrief-api-gateway (production)
✨ Published sitbrief-api-gateway (production)
  https://api.yourdomain.com
```

### 5. 設定 DNS（在 Cloudflare Dashboard）

當使用 routes 配置時，Worker 會自動攔截對應路由的請求。你需要確保網域有基本的 DNS 記錄：

#### 選項 A：使用 A 記錄（推薦）

1. 進入 Cloudflare Dashboard → 你的網域 → DNS → Records
2. 新增 A 記錄：
   - Type: `A`
   - Name: `api`
   - IPv4 address: `192.0.2.1`（隨便一個 IP，Worker 會攔截請求）
   - Proxy status: `Proxied`（橘色雲朵）✅

#### 選項 B：使用 CNAME 記錄

1. 新增 CNAME 記錄：
   - Type: `CNAME`
   - Name: `api`
   - Target: `strataperture.net`（指向你的主網域）
   - Proxy status: `Proxied`（橘色雲朵）✅

**重要**：一定要啟用 Proxy（橘色雲朵），這樣 Worker 才能攔截請求。

DNS 生效後（通常幾分鐘內），Worker 會自動處理所有 `api.strataperture.net/*` 的請求。

### 6. 測試 API

```bash
# Health check
curl https://api.strataperture.net/

# 測試認證（使用你的 token）
curl -H "Authorization: Bearer YOUR_TOKEN" \
     https://api.strataperture.net/api/metadata
```

應該會返回 JSON 資料。

---

## iOS App 設定

將以下資訊加入 iOS App：

```swift
// API 設定
let apiBaseURL = "https://api.yourdomain.com"
let accessToken = "YOUR_TOKEN" // 步驟 2 產生的 token
```

---

## 成本估算

| 項目 | 費用 |
|------|------|
| Cloudflare Worker | **免費**（每天 10 萬次請求） |
| R2 儲存 (< 1MB) | **免費**（前 10GB） |
| R2 讀取 | **免費**（每月 1000 萬次） |
| 網域 (.com) | **約 $10/年** |
| **總計** | **$10/年** |

---

## 維護建議

### 定期更換 Token（建議每 3 個月）

```bash
# 產生新 token
openssl rand -hex 32

# 更新 Cloudflare Secret
npx wrangler secret put ACCESS_TOKEN

# 更新 iOS App 中的 token
```

### 監控使用量

1. Cloudflare Dashboard → Workers & Pages → sitbrief-api-gateway
2. 查看 Metrics：請求數、錯誤率、延遲等

### 查看即時日誌

```bash
npm run tail
```

---

## 故障排除

### Worker 部署失敗

```bash
# 檢查設定
npx wrangler whoami
npx wrangler r2 bucket list

# 重新部署
npm run deploy:production
```

### API 返回 500 錯誤

```bash
# 查看日誌
npm run tail

# 常見原因：
# 1. R2 bucket 綁定錯誤 → 檢查 wrangler.toml
# 2. 檔案不存在 → 確認已上傳 JSON 到 R2
```

### Token 驗證失敗

```bash
# 確認 secret 已設定
npx wrangler secret list

# 重新設定
npx wrangler secret put ACCESS_TOKEN
```

---

## 下一步

完成部署後：

1. ✅ 將 API URL 和 Token 加入 iOS App
2. ✅ 測試 iOS App 能否正常讀取資料
3. ✅ 設定 Cloudflare Workers 的使用量警報（選用）
