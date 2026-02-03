# Sitbrief 架構簡化計劃

**日期：** 2026-02-04  
**版本：** 2.0  
**狀態：** ✅ 已確認

---

## 決策確認

| 項目 | 決定 |
|------|------|
| 雲端托管方案 | Azure Static Web Apps |
| Blazor Admin | 移除 |
| iOS App 認證 | 簡單 Token |
| 同步頻率 | 手動同步 |

---

## 變更動機

1. **降低成本**：移除 Azure App Service，改用免費的靜態檔案托管
2. **簡化架構**：移除後端 API 和資料庫，改用 JSON 檔案
3. **本地 AI 優先**：利用 GitHub Copilot + MCP Server 進行 AI 分析
4. **更好的 AI 體驗**：MCP 可存取完整本地資料，提供更準確的分析

---

## 架構對比

### 舊架構

```
┌─────────────────┐     ┌──────────────────┐     ┌──────────────┐
│  Blazor Admin   │────▶│   Azure API      │────▶│  Claude API  │
│  (本地 WASM)    │     │  (App Service)   │     │  (Anthropic) │
└─────────────────┘     └──────────────────┘     └──────────────┘
                               │
                               ▼
┌─────────────────┐     ┌──────────────────┐
│    iOS App      │────▶│     SQLite       │
└─────────────────┘     └──────────────────┘

月成本：$0-13（Azure Free Tier 有限制）
```

### 新架構

```
┌─────────────────────────────────────────────────────────────┐
│                    本地端 (Mac)                              │
│                                                              │
│  ┌──────────────┐    ┌────────────┐    ┌──────────────────┐ │
│  │   VS Code    │◀──▶│ MCP Server │◀──▶│  SQLite (本地)   │ │
│  │ + Copilot    │    │ (Sitbrief) │    │  sitbrief.db     │ │
│  └──────────────┘    └────────────┘    └──────────────────┘ │
│         │                                       │           │
│         │ 自然語言指令                          │ 產生      │
│         ▼                                       ▼           │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              JSON 檔案 (輸出)                         │   │
│  │  ├── articles.json                                   │   │
│  │  ├── topics.json                                     │   │
│  │  └── metadata.json                                   │   │
│  └──────────────────────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            │ git push / az storage upload
                            ▼
              ┌───────────────────────────────┐
              │  Azure Static Web Apps (免費)  │
              │  或 GitHub Pages (免費)        │
              │  ├── articles.json            │
              │  ├── topics.json              │
              │  └── metadata.json            │
              └───────────────┬───────────────┘
                              │
                              │ HTTPS GET
                              ▼
              ┌───────────────────────────────┐
              │          iOS App              │
              │       (純讀取 JSON)           │
              └───────────────────────────────┘

月成本：$0
```

---

## 移除的元件

| 元件 | 原用途 | 替代方案 |
|------|--------|----------|
| Sitbrief.API | REST API 服務 | 靜態 JSON 檔案 |
| Azure App Service | 託管 API | Azure Static Web Apps / GitHub Pages |
| Claude API 整合 | 雲端 AI 分析 | 本地 Copilot + MCP |
| JWT 認證 | API 保護 | 不需要（靜態檔案 + SAS Token） |
| EF Core Migrations | 資料庫版控 | 不需要（本地 SQLite） |

## 保留的元件

| 元件 | 用途 | 備註 |
|------|------|------|
| Sitbrief.Core | 領域模型 | 供 MCP Server 使用 |
| SQLite 資料庫 | 本地資料存儲 | 僅在本地使用 |
| iOS App | 使用者介面 | 改為讀取靜態 JSON |

## 新增的元件

| 元件 | 用途 |
|------|------|
| Sitbrief.McpServer | MCP Server，讓 Copilot 存取本地資料 |
| JSON 輸出模組 | 將資料庫匯出為 JSON 檔案 |
| 同步腳本 | 上傳 JSON 到雲端 |

---

## 實作階段

### 階段 1：建立 MCP Server（2-3 小時）

**任務清單：**

- [ ] 1.1 建立 `Sitbrief.McpServer` 專案
- [ ] 1.2 設定 MCP 協議基礎架構
- [ ] 1.3 實作資料庫連線（使用現有 SQLite）
- [ ] 1.4 實作 MCP Tools:
  - `get_topics` - 取得所有主題
  - `get_articles` - 取得文章列表
  - `create_article` - 新增文章
  - `analyze_article` - AI 分析文章（回傳分析結果供 Copilot 處理）
  - `link_article_topics` - 連結文章與主題
  - `export_json` - 匯出 JSON 檔案
- [ ] 1.5 設定 VS Code MCP 配置
- [ ] 1.6 測試 Copilot 整合

**預期成果：**
```
@sitbrief 列出所有主題
→ Copilot 透過 MCP 查詢本地資料庫並回覆
```

### 階段 2：JSON 匯出與雲端托管（1-2 小時）

**任務清單：**

- [ ] 2.1 設計 JSON 結構（articles.json, topics.json）
- [ ] 2.2 實作 JSON 匯出功能
- [ ] 2.3 選擇雲端托管方案：
  - 選項 A：Azure Static Web Apps
  - 選項 B：GitHub Pages
  - 選項 C：Azure Blob Storage
- [ ] 2.4 設定雲端資源
- [ ] 2.5 實作同步指令 `@sitbrief sync`
- [ ] 2.6 設定認證（如需要）

**預期成果：**
```
@sitbrief sync
→ 產生 JSON 檔案並上傳到雲端
→ iOS App 可透過 URL 讀取最新資料
```

### 階段 3：更新 iOS App（1 小時）

**任務清單：**

- [ ] 3.1 修改 API 服務層，改為讀取靜態 JSON
- [ ] 3.2 實作本地快取機制
- [ ] 3.3 實作增量更新（比對 version）
- [ ] 3.4 測試離線瀏覽功能

### 階段 4：清理舊架構（30 分鐘）

**任務清單：**

- [ ] 4.1 移除 `Sitbrief.API` 專案
- [ ] 4.2 移除 `Sitbrief.Admin` 專案（或保留作為備用 UI）
- [ ] 4.3 移除 `Sitbrief.Infrastructure` 中的 Claude 服務
- [ ] 4.4 刪除 Azure App Service 資源
- [ ] 4.5 更新文件

---

## JSON 結構設計

### articles.json

```json
{
  "version": "2026-02-04T10:30:00Z",
  "generatedAt": "2026-02-04T10:30:00Z",
  "count": 156,
  "articles": [
    {
      "id": 156,
      "title": "China's Growing Influence in Africa",
      "summary": "中國透過一帶一路...",
      "sourceName": "Foreign Affairs",
      "sourceUrl": "https://...",
      "publishedDate": "2026-02-03",
      "createdAt": "2026-02-04T08:00:00Z",
      "topicIds": [3, 7, 12],
      "analysis": {
        "significance": 8,
        "aiSummary": "中國透過債務外交深化對非洲影響力...",
        "keyEntities": {
          "countries": ["中國", "肯亞", "坦尚尼亞"],
          "organizations": ["中國進出口銀行", "非洲聯盟"],
          "persons": []
        },
        "geopoliticalTags": ["一帶一路", "債務外交", "資源競爭"]
      }
    }
  ]
}
```

### topics.json

```json
{
  "version": "2026-02-04T10:30:00Z",
  "generatedAt": "2026-02-04T10:30:00Z",
  "count": 25,
  "topics": [
    {
      "id": 3,
      "title": "中國全球戰略",
      "description": "追蹤中國在全球的戰略布局，包括一帶一路、軍事擴張等",
      "articleCount": 45,
      "lastUpdated": "2026-02-04T08:00:00Z",
      "recentArticleIds": [156, 142, 138]
    }
  ]
}
```

### metadata.json

```json
{
  "version": "2026-02-04T10:30:00Z",
  "lastSync": "2026-02-04T10:30:00Z",
  "stats": {
    "totalArticles": 156,
    "totalTopics": 25,
    "articlesThisWeek": 12
  },
  "endpoints": {
    "articles": "articles.json",
    "topics": "topics.json"
  }
}
```

---

## 雲端托管方案比較

| 方案 | 成本 | 設定難度 | 優點 | 缺點 |
|------|------|---------|------|------|
| **Azure Static Web Apps** | 免費 | 低 | GitHub 自動部署、內建 HTTPS | 需要 Azure 帳號 |
| **GitHub Pages** | 免費 | 極低 | 完全免費、git push 即部署 | 預設公開（需 Private Repo） |
| **Azure Blob Storage** | ~$0.01/月 | 中 | 靈活、可設定 SAS | 需手動設定 CORS |

**建議：** 使用 **Azure Static Web Apps**（免費，已確認）。

---

## iOS App 認證機制

使用簡單的 API Token 認證：

```
// iOS App 請求時附帶 Header
Authorization: Bearer <static-token>
```

**Azure Static Web Apps 設定：**

```json
// staticwebapp.config.json
{
  "routes": [
    {
      "route": "/data/*",
      "headers": {
        "Cache-Control": "no-cache"
      }
    }
  ],
  "responseOverrides": {
    "401": {
      "statusCode": 401,
      "body": "Unauthorized"
    }
  }
}
```

**Token 驗證方式：**
- 使用 Azure Functions（免費配額內）進行簡單驗證
- 或直接使用 SAS-like token 在 URL 參數中

---

## 工作流程變化

### 新增文章

**舊流程：**
1. 開啟瀏覽器 → Blazor Admin
2. 點「新增文章」→ 填表單
3. 點「AI 分析」→ 等待雲端 Claude 回應
4. 勾選建議主題 → 儲存

**新流程：**
1. 在 VS Code 中對 Copilot 說：
   ```
   @sitbrief 新增文章
   標題：...
   摘要：...
   來源：...
   ```
2. Copilot 自動分析並儲存
3. `@sitbrief sync` 同步到雲端

### 查詢資料

**舊流程：**
1. 開啟 Blazor Admin
2. 瀏覽 / 搜尋

**新流程：**
1. `@sitbrief 列出本週新增的文章`
2. `@sitbrief 「中東」主題有哪些文章？`

---

## 風險與對策

| 風險 | 影響 | 對策 |
|------|------|------|
| MCP 技術不熟悉 | 開發時間增加 | 先建立最小可行版本 |
| Copilot 服務中斷 | 無法新增文章 | 保留直接操作資料庫的備用方式 |
| JSON 檔案過大 | iOS 載入慢 | 分頁 / 壓縮 / 增量更新 |
| 資料同步遺忘 | iOS 資料過時 | 手動同步（已確認） |

---

## 下一步

按以下順序執行：

1. ✅ 確認架構決策
2. 🔄 建立 MCP Server 專案骨架
3. 實作核心 MCP Tools
4. 測試 Copilot 整合
5. 設定 Azure Static Web Apps
6. 清理舊架構（移除 Sitbrief.API、Sitbrief.Admin）

