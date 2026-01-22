# Sitbrief 設計文檔

**日期：** 2026-01-22
**版本：** 1.0

## 專案概述

Sitbrief 是一個專注於地緣政治和國際關係的新聞聚合平台，包含 Web 管理後台和 iOS 使用者應用程式。

### 核心目標
- 聚合來自付費媒體和智庫的地緣政治新聞
- 將相關文章匯集在統一主題下
- 幫助使用者快速建立完整的狀態覺知（Situational Awareness）
- 透過 AI 輔助提升內容管理效率

### 目標使用者
- **管理員**：單一內容管理者，負責新增文章和管理主題
- **一般使用者**：關注地緣政治的讀者，透過 iOS App 瀏覽內容

---

## 技術架構

### 技術棧選擇

**後端**
- ASP.NET Core 8.0+ (C#)
- Entity Framework Core
- SQLite（開發階段）
- JWT 身份驗證

**管理後台**
- Blazor WebAssembly
- C# 全棧開發

**iOS 應用程式**
- Swift + SwiftUI
- URLSession/Alamofire
- Core Data/UserDefaults（本地儲存）

**AI 服務**
- Claude API (Anthropic)
- 用於文章分析和主題建議

### 系統架構

```
┌─────────────────┐
│  iOS App        │
│  (Swift)        │
└────────┬────────┘
         │ HTTPS
         ▼
┌─────────────────┐     ┌──────────────────┐
│  Blazor Admin   │────▶│   ASP.NET Core   │
│  (WebAssembly)  │     │      API         │
└─────────────────┘     └────────┬─────────┘
                                 │
                    ┌────────────┼────────────┐
                    ▼            ▼            ▼
              ┌──────────┐  ┌────────┐  ┌──────────┐
              │ SQLite   │  │ Claude │  │   Auth   │
              │   DB     │  │  API   │  │  (JWT)   │
              └──────────┘  └────────┘  └──────────┘
```

### 架構分層

- **API 層**：處理 HTTP 請求，薄控制器模式
- **Core 層**：業務邏輯，領域模型
- **Infrastructure 層**：資料存取、外部服務整合
- **Presentation 層**：Blazor Admin、iOS App

---

## 資料模型

### 核心實體

#### Article（文章）
```csharp
public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }           // 文章標題
    public string Summary { get; set; }         // 摘要
    public string SourceUrl { get; set; }       // 原始連結
    public string SourceName { get; set; }      // 媒體/智庫名稱
    public SourceType SourceType { get; set; }  // 新聞媒體/智庫
    public string Content { get; set; }         // 完整內容（可選）
    public DateTime PublishedDate { get; set; } // 發布日期
    public DateTime CreatedDate { get; set; }   // 加入系統時間

    // 導航屬性
    public ICollection<ArticleTopic> ArticleTopics { get; set; }
    public AIAnalysis AIAnalysis { get; set; }
}

public enum SourceType
{
    NewsMedia,      // 新聞媒體
    ThinkTank       // 智庫
}
```

#### Topic（主題/事件）
```csharp
public class Topic
{
    public int Id { get; set; }
    public string Title { get; set; }           // 主題標題
    public string Description { get; set; }     // 簡短描述
    public string Significance { get; set; }    // 重要性說明
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }

    // 導航屬性
    public ICollection<ArticleTopic> ArticleTopics { get; set; }
}
```

#### ArticleTopic（多對多關聯）
```csharp
public class ArticleTopic
{
    public int ArticleId { get; set; }
    public int TopicId { get; set; }
    public float Confidence { get; set; }       // AI 信心分數 0-1
    public bool IsConfirmed { get; set; }       // 是否經管理員確認
    public DateTime AddedDate { get; set; }

    // 導航屬性
    public Article Article { get; set; }
    public Topic Topic { get; set; }
}
```

#### AIAnalysis（AI 分析記錄）
```csharp
public class AIAnalysis
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public string SuggestedTopicsJson { get; set; }     // JSON 格式建議主題
    public string KeyEntitiesJson { get; set; }         // 關鍵實體
    public string GeopoliticalTagsJson { get; set; }    // 地理標籤
    public int SignificanceScore { get; set; }          // 重要性 1-10
    public DateTime AnalyzedDate { get; set; }

    // 導航屬性
    public Article Article { get; set; }
}
```

### 資料關係
- 一篇文章可關聯多個主題（多對多）
- 一篇文章有一筆 AI 分析記錄（一對一，可選）
- ArticleTopic 保留 AI 建議和人工確認狀態

---

## API 設計

### 公開端點（iOS App）

#### 主題相關
```
GET /api/topics
查詢參數：
  - page: int (頁碼)
  - pageSize: int (每頁數量)
  - sortBy: string (time|heat)
回應：主題列表，包含統計資訊

GET /api/topics/{id}
回應：主題詳細資訊

GET /api/topics/{id}/articles
查詢參數：
  - sortBy: string (time)
回應：主題下的所有文章
```

#### 文章相關
```
GET /api/articles/{id}
回應：文章詳細資訊
```

#### 統計相關
```
GET /api/statistics
回應：
  - 最近 24 小時新主題數
  - 最近 24 小時新文章數
  - 活躍地理區域
```

### 管理端點（需要驗證）

#### 身份驗證
```
POST /api/auth/login
請求：{ username, password }
回應：{ token, expiresAt }
```

#### 文章管理
```
POST /api/articles
請求：{ title, summary, sourceUrl, sourceName, sourceType, publishedDate }
回應：新建的文章物件

POST /api/articles/{id}/analyze
觸發 AI 分析
回應：{
  suggestedExistingTopics: [{ topicId, confidence }],
  suggestedNewTopics: [{ title, description }],
  keyEntities: [string],
  geopoliticalTags: [string],
  significance: int,
  summary: string
}

POST /api/articles/{id}/topics
請求：{ topicIds: [int], confirmed: true }
建立文章與主題關聯

PUT /api/articles/{id}
更新文章資訊

DELETE /api/articles/{id}
刪除文章
```

#### 主題管理
```
GET /api/admin/topics
回應：所有主題（包含統計）

POST /api/topics
請求：{ title, description, significance }
回應：新建的主題

PUT /api/topics/{id}
更新主題資訊

POST /api/topics/{id}/merge/{targetId}
合併主題（將文章轉移到目標主題）

DELETE /api/topics/{id}
刪除主題（保留文章）
```

### API 回應格式

**成功回應：**
```json
{
  "success": true,
  "data": { ... },
  "message": "操作成功"
}
```

**錯誤回應：**
```json
{
  "success": false,
  "error": {
    "code": "INVALID_INPUT",
    "message": "錯誤描述",
    "details": { ... }
  }
}
```

---

## Claude AI 整合

### 分析流程

1. **準備上下文**
   - 收集文章資訊（標題、摘要、來源、日期）
   - 查詢最近的 20-30 個現有主題

2. **構建提示詞**
```
你是地緣政治分析專家。分析以下文章並提供結構化建議。

文章資訊：
標題：{article.Title}
摘要：{article.Summary}
來源：{article.SourceName}
發布日期：{article.PublishedDate}

現有主題列表：
{list of existing topics with IDs}

請以 JSON 格式回覆：
{
  "suggestedExistingTopics": [
    { "topicId": int, "confidence": float, "reason": string }
  ],
  "suggestedNewTopics": [
    { "title": string, "description": string }
  ],
  "keyEntities": {
    "countries": [string],
    "organizations": [string],
    "persons": [string]
  },
  "geopoliticalTags": [string],
  "significance": int (1-10),
  "summary": string
}
```

3. **API 呼叫設定**
   - 模型：Claude 3.5 Sonnet
   - max_tokens：1500-2000
   - temperature：0.3（低溫度確保一致性）

4. **結果處理**
   - 驗證 JSON 格式
   - 儲存到 AIAnalysis 表
   - 在管理介面顯示建議

### 成本控制策略

- 僅在手動觸發時呼叫 API
- 快取現有主題列表
- 可設定每日呼叫次數上限
- 錯誤時允許完全手動操作

---

## 管理後台設計

### 頁面結構

#### 登入頁面
- 簡潔的帳號密碼輸入
- JWT Token 儲存在 localStorage
- 登入失敗提示

#### 儀表板（Dashboard）
- 今日新增文章數
- 活躍主題數量
- 最近新增的文章列表
- 待處理的 AI 建議

#### 新增文章頁面
1. **文章資訊表單**
   - 標題（必填）
   - 摘要（必填）
   - 原始 URL（必填）
   - 媒體/智庫名稱（必填）
   - 來源類型（下拉選單）
   - 發布日期（日期選擇器）

2. **AI 分析按鈕**
   - 點擊後顯示載入狀態
   - 呼叫 `/api/articles/{id}/analyze`

3. **AI 建議顯示**
   - **建議的現有主題**：
     - 列出匹配的主題
     - 顯示信心分數和理由
     - 可勾選接受
   - **建議的新主題**：
     - 顯示標題和描述
     - 可編輯或直接建立
   - **關鍵實體**：
     - 標籤形式顯示國家、組織、人物
   - **重要性評分**：視覺化顯示

4. **確認與儲存**
   - 選擇或建立主題
   - 確認關聯
   - 儲存文章

#### 管理主題頁面
- 主題列表（表格或卡片）
  - 標題
  - 文章數量
  - 最後更新時間
  - 操作按鈕（編輯、合併、刪除）
- 點擊主題查看關聯文章
- 編輯主題標題和描述
- 合併重複主題功能

#### 文章列表頁面
- 所有文章列表
- 篩選：按主題、媒體、日期
- 搜尋功能
- 編輯和刪除操作

### 工作流程

**典型新增文章流程：**
1. 進入「新增文章」頁面
2. 貼上文章資訊（從付費媒體複製）
3. 點擊「AI 分析」按鈕
4. 查看 AI 建議：
   - 若有匹配的現有主題 → 勾選接受
   - 若無匹配 → 接受 AI 建議的新主題或手動建立
5. 確認並儲存
6. 文章立即在 iOS App 上可見

---

## iOS App 設計

### 狀態覺知導向設計原則

**目標：** 讓使用者快速建立地緣政治事件的完整認知

**設計策略：**
- 視覺化層級和熱度
- 快速掃描關鍵資訊
- 清晰的資訊密度
- 時間脈絡呈現

### 主畫面 - 主題時間軸

#### 頂部總覽面板（可選折疊）
```
┌─────────────────────────────────────┐
│  過去 24 小時                        │
│  5 個新主題 | 12 篇新文章            │
│  活躍區域：🌏 東亞  🌍 中東         │
└─────────────────────────────────────┘
```

#### 主題卡片設計

**高熱度主題（24小時內有新文章）：**
```
┌─────────────────────────────────────┐
│ 🔴 NEW  3小時前                      │
│                                     │
│ 中美南海爭議升級                     │
│ 兩國海軍在南沙群島進行對峙演習       │
│                                     │
│ 🏢 CSIS  📰 WSJ  📰 FT  🏢 RAND     │
│                                     │
│ 8 篇報導 | 跨度 3 天 | 3 家智庫      │
│ 🌏 東亞 · 🇨🇳 中國 · 🇺🇸 美國        │
└─────────────────────────────────────┘
```

**一般主題：**
```
┌─────────────────────────────────────┐
│ 5天前                                │
│                                     │
│ 歐盟能源危機應對                     │
│ 各國尋求替代能源方案                 │
│                                     │
│ 📰 Economist  📰 Bloomberg          │
│                                     │
│ 4 篇報導 | 跨度 7 天                 │
│ 🌍 歐洲 · ⚡ 能源                    │
└─────────────────────────────────────┘
```

#### 卡片視覺層級
- **尺寸**：24h內有新文章 → 較大卡片
- **邊框**：同時有智庫和媒體 → 特殊顏色邊框
- **標記**：NEW 標籤、熱度圖示
- **顏色**：使用顏色強度區分時效性

### 主題詳細頁面

#### 頁面結構
```
┌─────────────────────────────────────┐
│ ← 返回                               │
│                                     │
│ 中美南海爭議升級                     │
│                                     │
│ 📊 8 篇報導 | 5 個來源 | 3 天跨度   │
│ 🌏 東亞 · 🇨🇳 中國 · 🇺🇸 美國        │
│                                     │
│ ✨ 為什麼重要                        │
│ 這標誌著兩國軍事對峙的新階段...      │
│                                     │
├─────────────────────────────────────┤
│ 📰 媒體報導 (5)                      │
│                                     │
│ ┌─────────────────────────────────┐ │
│ │ 📰 Wall Street Journal          │ │
│ │ 3小時前                          │ │
│ │ U.S. and China Naval Forces...  │ │
│ │ 美國和中國海軍在南海進行...       │ │
│ │ 🔖                               │ │
│ └─────────────────────────────────┘ │
│                                     │
├─────────────────────────────────────┤
│ 🏢 智庫分析 (3)                      │
│                                     │
│ ┌─────────────────────────────────┐ │
│ │ 🏢 CSIS                          │ │
│ │ 1天前                            │ │
│ │ Strategic Implications of...    │ │
│ │ 南海爭議的戰略意涵...             │ │
│ │ 🔖                               │ │
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

#### 時間軸視圖（可選切換）
- 文章按時間垂直排列
- 視覺化事件發展脈絡
- 不同來源用不同顏色標示

### 書籤頁面

```
┌─────────────────────────────────────┐
│ 書籤                                 │
│                                     │
│ 📌 已儲存 12 篇文章                  │
│                                     │
│ ┌─────────────────────────────────┐ │
│ │ 📰 Financial Times              │ │
│ │ China's New Economic Policy     │ │
│ │ 主題：中國經濟政策                │ │
│ │ 儲存於 2天前                     │ │
│ └─────────────────────────────────┘ │
│                                     │
└─────────────────────────────────────┘
```

### 技術實作細節

#### 資料層
```swift
struct Topic: Codable, Identifiable {
    let id: Int
    let title: String
    let description: String
    let significance: String
    let articleCount: Int
    let sources: [Source]
    let tags: [String]
    let lastUpdated: Date
    let hasNewArticles: Bool
}

struct Article: Codable, Identifiable {
    let id: Int
    let title: String
    let summary: String
    let sourceUrl: String
    let sourceName: String
    let sourceType: SourceType
    let publishedDate: Date
}
```

#### API Service
```swift
class APIService {
    func fetchTopics(page: Int, sortBy: String) async throws -> [Topic]
    func fetchTopicDetail(id: Int) async throws -> TopicDetail
    func fetchArticles(topicId: Int) async throws -> [Article]
    func fetchStatistics() async throws -> Statistics
}
```

#### 本地儲存（書籤）
```swift
class BookmarkManager {
    @Published var bookmarks: [Article] = []

    func addBookmark(_ article: Article)
    func removeBookmark(_ article: Article)
    func isBookmarked(_ article: Article) -> Bool

    // 使用 UserDefaults 或 Core Data 持久化
}
```

---

## 身份驗證實作

### 管理後台驗證

#### 配置方式
```json
// appsettings.json (開發環境)
{
  "Authentication": {
    "AdminUsername": "admin",
    "AdminPasswordHash": "$2a$11$..." // BCrypt hash
  },
  "JWT": {
    "Secret": "your-secret-key-min-32-chars",
    "Issuer": "Sitbrief",
    "Audience": "SitbriefAdmin",
    "ExpirationHours": 12
  }
}
```

生產環境使用環境變數：
```bash
export ADMIN_USERNAME=admin
export ADMIN_PASSWORD_HASH=$2a$11$...
export JWT_SECRET=your-secret-key
```

#### JWT Token 結構
```json
{
  "sub": "admin",
  "role": "Administrator",
  "iat": 1706000000,
  "exp": 1706043200,
  "iss": "Sitbrief",
  "aud": "SitbriefAdmin"
}
```

#### Blazor 實作

**AuthenticationStateProvider：**
```csharp
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login",
            new { username, password });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            await _localStorage.SetItemAsync("authToken", result.Token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return true;
        }
        return false;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
```

#### 後端驗證中介層
```csharp
public class JwtMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (token != null)
            AttachUserToContext(context, token);

        await _next(context);
    }
}
```

### 安全措施

- HTTPS 強制（生產環境）
- Token 過期自動登出
- 登入失敗次數限制（3-5次）
- 密碼使用 BCrypt 雜湊（work factor: 11）
- 敏感設定使用環境變數

---

## 專案結構

```
Sitbrief/
│
├── src/
│   ├── Sitbrief.API/                    # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── ArticlesController.cs
│   │   │   ├── TopicsController.cs
│   │   │   ├── AuthController.cs
│   │   │   └── StatisticsController.cs
│   │   ├── Middleware/
│   │   │   ├── JwtMiddleware.cs
│   │   │   └── ErrorHandlerMiddleware.cs
│   │   ├── DTOs/
│   │   │   ├── ArticleDto.cs
│   │   │   ├── TopicDto.cs
│   │   │   └── AIAnalysisResultDto.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── Sitbrief.Core/                   # 核心業務邏輯
│   │   ├── Entities/
│   │   │   ├── Article.cs
│   │   │   ├── Topic.cs
│   │   │   ├── ArticleTopic.cs
│   │   │   └── AIAnalysis.cs
│   │   ├── Interfaces/
│   │   │   ├── IArticleRepository.cs
│   │   │   ├── ITopicRepository.cs
│   │   │   ├── IArticleService.cs
│   │   │   ├── ITopicService.cs
│   │   │   └── IAIService.cs
│   │   └── Services/
│   │       ├── ArticleService.cs
│   │       ├── TopicService.cs
│   │       └── ClaudeAIService.cs
│   │
│   ├── Sitbrief.Infrastructure/         # 資料存取層
│   │   ├── Data/
│   │   │   ├── SitbriefDbContext.cs
│   │   │   └── DbInitializer.cs
│   │   ├── Repositories/
│   │   │   ├── ArticleRepository.cs
│   │   │   └── TopicRepository.cs
│   │   └── Migrations/
│   │
│   └── Sitbrief.Admin/                  # Blazor WebAssembly
│       ├── Pages/
│       │   ├── Index.razor
│       │   ├── Login.razor
│       │   ├── Dashboard.razor
│       │   ├── Articles/
│       │   │   ├── AddArticle.razor
│       │   │   ├── EditArticle.razor
│       │   │   └── ArticleList.razor
│       │   └── Topics/
│       │       ├── TopicList.razor
│       │       └── TopicDetail.razor
│       ├── Services/
│       │   ├── ApiClient.cs
│       │   └── JwtAuthenticationStateProvider.cs
│       ├── Shared/
│       │   ├── MainLayout.razor
│       │   └── NavMenu.razor
│       ├── wwwroot/
│       └── Program.cs
│
├── ios/
│   └── Sitbrief/                        # iOS 專案
│       ├── Models/
│       │   ├── Topic.swift
│       │   ├── Article.swift
│       │   └── Statistics.swift
│       ├── Services/
│       │   ├── APIService.swift
│       │   └── BookmarkManager.swift
│       ├── ViewModels/
│       │   ├── TopicListViewModel.swift
│       │   └── TopicDetailViewModel.swift
│       ├── Views/
│       │   ├── TopicListView.swift
│       │   ├── TopicDetailView.swift
│       │   ├── ArticleRowView.swift
│       │   └── BookmarksView.swift
│       └── SitbriefApp.swift
│
├── docs/
│   └── plans/
│       └── 2026-01-22-sitbrief-design.md
│
├── tests/
│   ├── Sitbrief.Tests/
│   └── Sitbrief.Integration.Tests/
│
├── .gitignore
├── Sitbrief.sln
└── README.md
```

---

## 開發階段規劃

### 階段一：後端基礎（1-2週）

**目標：** 建立可運作的 API 和資料層

**任務：**
1. 建立解決方案結構（API, Core, Infrastructure）
2. 設定 Entity Framework Core 和 SQLite
3. 實作資料模型（Article, Topic, ArticleTopic, AIAnalysis）
4. 建立 DbContext 和初始 Migration
5. 實作 Repository 層
6. 實作 Service 層基本邏輯
7. 建立 Controllers 和公開 API 端點
8. 新增種子資料用於測試
9. 測試 API（使用 Postman 或 Swagger）

**完成標準：**
- API 可以 CRUD 文章和主題
- 可以建立文章與主題的關聯
- 基本的錯誤處理

### 階段二：管理後台核心功能（1-2週）

**目標：** 手動工作流程完整可用

**任務：**
1. 建立 Blazor WebAssembly 專案
2. 實作登入頁面和身份驗證
3. 建立 ApiClient 服務
4. 實作主選單和路由
5. 建立「新增文章」表單（不含 AI）
6. 建立「主題管理」頁面
   - 列出所有主題
   - 建立新主題
   - 編輯主題
7. 建立「文章列表」頁面
8. 實作手動選擇主題功能
9. 測試完整的手動工作流程

**完成標準：**
- 可以登入管理後台
- 可以手動新增文章和主題
- 可以建立文章與主題的關聯
- 可以查看所有文章和主題

### 階段三：AI 整合（1週）

**目標：** AI 輔助功能可用

**任務：**
1. 建立 Claude API 整合服務
2. 實作 AIAnalysis 資料模型和儲存
3. 設計和測試提示詞
4. 在「新增文章」頁面加入「AI 分析」按鈕
5. 實作 AI 建議顯示 UI
6. 實作確認/調整 AI 建議的流程
7. 加入錯誤處理（API 失敗時的降級處理）
8. 測試和優化

**完成標準：**
- AI 可以分析文章並提供主題建議
- 管理員可以接受或調整 AI 建議
- 系統保留 AI 分析記錄
- API 失敗時可降級為手動模式

### 階段四：iOS App MVP（2週）

**目標：** 基本的 iOS 閱讀體驗

**任務：**
1. 建立 iOS 專案（Xcode + SwiftUI）
2. 建立資料模型（Topic, Article）
3. 實作 APIService
4. 建立主題列表視圖（TopicListView）
   - 基本卡片設計
   - 下拉重新整理
5. 建立主題詳細視圖（TopicDetailView）
   - 顯示文章列表
   - 點擊開啟 Safari View Controller
6. 實作基本導航
7. 測試 API 連接和資料顯示

**完成標準：**
- 可以瀏覽所有主題
- 可以查看主題下的文章
- 可以點擊文章開啟原始網頁
- 基本的使用者體驗流暢

### 階段五：增強功能（2週）

**目標：** 狀態覺知優化和完整功能

**任務：**
1. **iOS UI 增強：**
   - 實作熱度視覺化
   - 加入媒體 logo 和來源圖示
   - 實作總覽面板
   - 優化卡片設計和層級
   - 加入地理標籤和關鍵實體顯示
2. **書籤功能：**
   - 實作 BookmarkManager
   - 本地儲存（UserDefaults）
   - 書籤頁面
3. **統計 API：**
   - 實作統計端點
   - iOS 總覽面板資料來源
4. **管理後台增強：**
   - 主題合併功能
   - 儀表板統計
   - 搜尋和篩選
5. **效能優化：**
   - API 快取
   - 分頁載入
   - 圖片最佳化

**完成標準：**
- iOS App 提供優秀的狀態覺知體驗
- 書籤功能完整可用
- 管理後台功能齊全
- 系統效能良好

---

## 部署考量

### 開發環境

- 後端：本地運行（`dotnet run`）
- 資料庫：SQLite 檔案（`sitbrief.db`）
- Blazor：開發伺服器
- iOS：模擬器或實體裝置

### 未來生產環境選項

**雲端部署：**
- Azure App Service（推薦，與 .NET 深度整合）
- AWS Elastic Beanstalk
- DigitalOcean App Platform

**資料庫遷移：**
- PostgreSQL（推薦生產環境）
- SQL Server（如果使用 Azure）

**考量因素：**
- HTTPS 憑證
- 環境變數管理
- 日誌和監控
- 資料庫備份
- API 速率限制

---

## 風險與挑戰

### 技術風險

1. **AI 成本控制**
   - 緩解：手動觸發、每日上限、快取策略

2. **SQLite 擴展性**
   - 緩解：初期足夠，未來可遷移到 PostgreSQL

3. **Claude API 穩定性**
   - 緩解：完整的降級機制，允許純手動操作

### 產品風險

1. **內容管理負擔**
   - 緩解：AI 輔助減輕負擔，批次操作功能

2. **使用者參與度**
   - 緩解：專注狀態覺知體驗，清晰的資訊呈現

---

## 成功指標

### 技術指標
- API 回應時間 < 200ms（90 percentile）
- iOS App 啟動時間 < 2 秒
- AI 分析準確率 > 80%（需人工確認）

### 產品指標
- 每日新增文章數量（管理員工作效率）
- iOS App 每日活躍使用者
- 平均閱讀主題數
- 書籤使用率

---

## 後續擴展可能性

### 功能擴展
- 多語言支援（英文、中文）
- 推播通知（重要主題更新）
- 使用者帳號系統（進階功能）
- 社群分享功能
- 文章全文抓取（RSS/爬蟲）

### 技術擴展
- Android App
- Web 使用者前台
- 進階 AI 功能（趨勢預測）
- 資料視覺化儀表板

---

## 附錄

### 參考技術文檔
- ASP.NET Core: https://docs.microsoft.com/aspnet/core
- Entity Framework Core: https://docs.microsoft.com/ef/core
- Blazor: https://docs.microsoft.com/aspnet/core/blazor
- Claude API: https://docs.anthropic.com
- SwiftUI: https://developer.apple.com/documentation/swiftui

### 開發工具
- IDE: Visual Studio 2022 / Rider / VS Code
- iOS: Xcode 15+
- API 測試: Postman / Swagger
- 版本控制: Git
- 專案管理: GitHub Issues / Linear

---

**文檔版本：** 1.0
**最後更新：** 2026-01-22
**狀態：** 設計完成，待實作
