# Sitbrief

A geopolitical news aggregation platform focused on international relations and strategic analysis.

## Overview

Sitbrief helps users build situational awareness by aggregating articles from premium news sources and think tanks, organizing them by topics, and providing AI-assisted content curation.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                      Local Environment (Mac)                         │
│                                                                       │
│  ┌──────────────┐    ┌────────────┐    ┌──────────────────┐          │
│  │   VS Code    │◀──▶│ MCP Server │◀──▶│  SQLite (local)  │          │
│  │ + Copilot    │    │ (Sitbrief) │    │  sitbrief.db     │          │
│  └──────────────┘    └────────────┘    └──────────────────┘          │
│         │                   │                   │                     │
│         │                   │ Export            │                     │
│         │                   ▼                   │                     │
│  ┌──────────────────────────────────────────────────────┐            │
│  │              JSON Files (output/)                     │            │
│  │  ├── metadata.json                                   │            │
│  │  ├── topics.json                                     │            │
│  │  └── articles/                                       │            │
│  │      ├── latest.json (最新 20 篇)                     │            │
│  │      └── page-{n}.json (分頁)                         │            │
│  └──────────────────────────────────────────────────────┘            │
│         │                                                             │
│  ┌──────────────────────────────────────────────────────┐            │
│  │              Aggregator (Python)                      │            │
│  │  定時抓取 CSIS, RAND 等智庫最新標題                    │            │
│  │  輸出: headlines.json / headlines.html                │            │
│  └──────────────────────────────────────────────────────┘            │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
                            │ AWS S3 SDK
                            ▼
              ┌───────────────────────────────┐
              │     Cloudflare R2 Storage      │
              │     (statbrief bucket)         │
              │     Brief/                     │
              │     ├── metadata.json         │
              │     ├── topics.json           │
              │     └── articles/*.json       │
              └───────────────┬───────────────┘
                              │
                              │ Token Auth
                              ▼
              ┌───────────────────────────────┐
              │     Cloudflare Worker          │
              │     api.strataperture.net      │
              │     (API Gateway)              │
              └───────────────┬───────────────┘
                              │
                              │ HTTPS GET
                              ▼
              ┌───────────────────────────────┐
              │          iOS App              │
              │       (Read-only JSON)        │
              └───────────────────────────────┘
```

## User Workflow

### 方式一：從 Aggregator 選取文章

```bash
# 1. 執行聚合器抓取最新標題
cd aggregator
python aggregator.py

# 2. 開啟 HTML 瀏覽標題列表
open output/headlines.html

# 3. 看到有興趣的文章，複製資訊貼給 Copilot
# 或使用 Bookmarklet 一鍵複製
```

### 方式二：手動新增文章

**使用 Bookmarklet：**
1. 開啟 `tools/bookmarklet.html`，將按鈕拖到書籤列
2. 在任何新聞頁面點擊「📋 擷取文章」
3. 貼到 VS Code 對話，Copilot 自動解析新增

**手動輸入格式：**
```
標題：Naval Leaders Need to Think Fast, Slow, and Augmented
來源：U.S. Naval Institute
日期：2026-02-01
網址：https://www.usni.org/...
摘要：本文探討如何在新興科技時代維持航空母艦的攻勢能力...
```

### 方式三：從 URL 匯入

```
「匯入 https://www.example.com/article」
```
（注意：部分網站有 Cloudflare 保護，可能無法自動抓取）

### 同步到雲端

```bash
# 匯出 JSON 並上傳到 R2
cd src
dotnet run --project SyncR2/SyncR2.csproj
```

## Data Flow

```
┌─────────────────┐     ┌──────────────┐     ┌─────────────────┐
│    來源網站     │     │   Aggregator  │     │   Bookmarklet   │
│ CSIS, RAND, ... │     │   (Python)    │     │    (Browser)    │
└────────┬────────┘     └───────┬───────┘     └────────┬────────┘
         │                      │                      │
         │ RSS / Web Scrape     │ headlines.html       │ 複製格式化文字
         ▼                      ▼                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                        VS Code + Copilot                         │
│                    (使用者選擇 & 編輯文章)                        │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                                 │ MCP: QuickAddArticle / CreateArticle
                                 ▼
                    ┌────────────────────────┐
                    │     SQLite Database    │
                    │     (data/sitbrief.db) │
                    └────────────┬───────────┘
                                 │
                                 │ MCP: ExportJson
                                 ▼
                    ┌────────────────────────┐
                    │    Local JSON Files    │
                    │    (output/)           │
                    └────────────┬───────────┘
                                 │
                                 │ MCP: SyncToCloud (AWS SDK)
                                 ▼
                    ┌────────────────────────┐
                    │    Cloudflare R2       │
                    │    (statbrief bucket)  │
                    └────────────┬───────────┘
                                 │
                                 │ Cloudflare Worker (Token Auth)
                                 ▼
                    ┌────────────────────────┐
                    │      iOS App           │
                    │  (Read JSON via API)   │
                    └────────────────────────┘
```

## Project Structure

```
Sitbrief/
├── src/
│   ├── Sitbrief.McpServer/      # MCP Server for Copilot integration
│   │   └── Tools/
│   │       ├── ArticleTools.cs   # 文章 CRUD + URL 匯入
│   │       ├── TopicTools.cs     # 主題管理
│   │       ├── ExportTools.cs    # JSON 匯出（分頁結構）
│   │       └── CloudSyncTools.cs # R2 同步
│   ├── Sitbrief.Core/           # Domain entities and interfaces
│   ├── Sitbrief.Infrastructure/ # Data access (SQLite + EF Core)
│   └── SyncR2/                  # 同步執行程式
├── aggregator/                  # Python 智庫聚合器
│   ├── aggregator.py
│   ├── sources.yaml             # 來源配置
│   └── output/
│       ├── headlines.json
│       └── headlines.html       # 可瀏覽的標題列表
├── cloudflare-worker/           # API Gateway
│   ├── src/index.ts
│   └── wrangler.toml
├── tools/
│   └── bookmarklet.html         # 瀏覽器書籤工具
├── data/
│   └── sitbrief.db              # Local SQLite database
├── output/                      # Exported JSON files
│   ├── metadata.json
│   ├── topics.json
│   └── articles/
│       ├── latest.json
│       └── page-{n}.json
└── docs/
    └── plans/                   # Design documents
```

## API Endpoints

Base URL: `https://api.strataperture.net`

| Endpoint | Description |
|----------|-------------|
| `GET /api/metadata` | 版本資訊與文章總數 |
| `GET /api/topics` | 所有主題 |
| `GET /api/articles/latest` | 最新 20 篇文章 |
| `GET /api/articles/page/{n}` | 第 n 頁文章（每頁 20 篇） |

**認證方式：** Bearer Token
```bash
curl -H "Authorization: Bearer <token>" https://api.strataperture.net/api/articles/latest
```

## MCP Tools

| Tool | Description |
|------|-------------|
| `GetTopics` | 列出所有主題 |
| `GetArticles` | 列出文章（支援篩選） |
| `QuickAddArticle` | 從貼上的格式化文字新增文章 |
| `CreateArticle` | 手動新增文章（指定所有欄位） |
| `ImportArticleFromUrl` | 從 URL 抓取並匯入文章 |
| `FetchArticleFromUrl` | 預覽 URL 內容（不儲存） |
| `LinkArticleTopics` | 連結文章到主題 |
| `ExportJson` | 匯出 JSON 到 output/ |
| `SyncToCloud` | 上傳到 Cloudflare R2 |
| `CleanAndSync` | 清除舊檔案後重新上傳 |

## Scheduled Tasks

**Aggregator（每日 8:00）：**
```bash
# 查看狀態
launchctl list | grep sitbrief

# 手動執行
launchctl start com.sitbrief.aggregator

# 停用
launchctl unload ~/Library/LaunchAgents/com.sitbrief.aggregator.plist
```

## Tech Stack

| Component | Technology |
|-----------|------------|
| MCP Server | .NET 8.0 + ModelContextProtocol |
| Database | SQLite + Entity Framework Core |
| Cloud Storage | Cloudflare R2 (S3-compatible) |
| API Gateway | Cloudflare Workers (TypeScript) |
| Aggregator | Python + Playwright + httpx |
| Domain | strataperture.net |

## Development Status

- ✅ Phase 1: Backend Foundation (Complete)
- ✅ Phase 2: MCP Server + Copilot Integration (Complete)
- ✅ Phase 3: Cloud Hosting - Cloudflare R2 + Worker (Complete)
- ✅ Aggregator: CSIS + RAND (Complete)
- 🚧 Phase 4: iOS App (Planned)

## Quick Start

```bash
# 1. 抓取智庫最新標題
cd aggregator && python aggregator.py

# 2. 在 VS Code 中與 Copilot 對話新增文章
# 貼上格式化文章資訊

# 3. 同步到雲端
cd src && dotnet run --project SyncR2/SyncR2.csproj

# 4. 驗證 API
curl -H "Authorization: Bearer <token>" \
     https://api.strataperture.net/api/articles/latest
```

## Documentation

- [Design Document](docs/plans/2026-01-22-sitbrief-design.md)
- [Architecture Simplification](docs/plans/2026-02-04-architecture-simplification.md)

## License

Private project - All rights reserved
