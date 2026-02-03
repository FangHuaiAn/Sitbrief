# Sitbrief

A geopolitical news aggregation platform focused on international relations and strategic analysis.

## Overview

Sitbrief helps users build situational awareness by aggregating articles from premium news sources and think tanks, organizing them by topics, and providing AI-assisted content curation.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Local Environment (Mac)                   │
│                                                              │
│  ┌──────────────┐    ┌────────────┐    ┌──────────────────┐ │
│  │   VS Code    │◀──▶│ MCP Server │◀──▶│  SQLite (local)  │ │
│  │ + Copilot    │    │ (Sitbrief) │    │  sitbrief.db     │ │
│  └──────────────┘    └────────────┘    └──────────────────┘ │
│         │                                       │           │
│         │ Natural language                      │ Export    │
│         ▼                                       ▼           │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              JSON Files (output)                      │   │
│  │  ├── articles.json                                   │   │
│  │  ├── topics.json                                     │   │
│  │  └── metadata.json                                   │   │
│  └──────────────────────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            │ git push / sync
                            ▼
              ┌───────────────────────────────┐
              │  Static Hosting (Free)         │
              │  (GitHub Pages / Azure SWA)    │
              │  ├── articles.json            │
              │  ├── topics.json              │
              │  └── metadata.json            │
              └───────────────┬───────────────┘
                              │
                              │ HTTPS GET
                              ▼
              ┌───────────────────────────────┐
              │          iOS App              │
              │       (Read-only JSON)        │
              └───────────────────────────────┘
```

### Key Design Decisions

- **Local-first**: All content management happens locally via VS Code + Copilot
- **AI via MCP**: GitHub Copilot provides AI analysis through MCP Server
- **Static backend**: No server-side code, just JSON files served via static hosting
- **Zero cost**: Free tier hosting (GitHub Pages or Azure Static Web Apps)

## Project Structure

```
Sitbrief/
├── src/
│   ├── Sitbrief.McpServer/    # MCP Server for Copilot integration
│   ├── Sitbrief.Core/         # Domain entities and interfaces
│   └── Sitbrief.Infrastructure/ # Data access (SQLite)
├── data/
│   └── sitbrief.db            # Local SQLite database
├── output/
│   ├── articles.json          # Exported articles for iOS
│   ├── topics.json            # Exported topics for iOS
│   └── metadata.json          # Sync metadata
├── docs/
│   └── plans/                 # Design and implementation docs
└── README.md
```

## Tech Stack

- **MCP Server:** .NET 8.0 + Model Context Protocol
- **AI:** GitHub Copilot (via MCP integration)
- **Database:** SQLite (local only)
- **Hosting:** GitHub Pages or Azure Static Web Apps (free)
- **iOS App:** Swift + SwiftUI

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- VS Code with GitHub Copilot
- Git

### Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd Sitbrief
```

2. Configure MCP Server in VS Code:
```json
// .vscode/settings.json
{
  "mcp": {
    "servers": {
      "sitbrief": {
        "command": "dotnet",
        "args": ["run", "--project", "src/Sitbrief.McpServer"]
      }
    }
  }
}
```

3. Start using with Copilot:
```
@sitbrief 列出所有主題
@sitbrief 新增文章 標題：... 摘要：...
@sitbrief sync
```

## Workflow

### Adding Articles

```
@sitbrief 新增文章

標題：China's Growing Influence in Africa
來源：Foreign Affairs
網址：https://...
摘要：中國透過一帶一路倡議持續擴大在非洲的經濟影響力...
```

Copilot will:
1. Query existing topics from local database
2. Analyze the article content
3. Suggest topic classifications
4. Save to local SQLite database

### Syncing to Cloud

```
@sitbrief sync
```

This will:
1. Export articles.json and topics.json
2. Push to GitHub / upload to Azure
3. iOS App can then fetch the latest data

## MCP Tools

| Tool | Description |
|------|-------------|
| `get_topics` | List all topics |
| `get_articles` | List articles with filters |
| `create_article` | Add a new article |
| `analyze_article` | Get AI analysis for an article |
| `link_article_topics` | Connect article to topics |
| `export_json` | Generate JSON files |
| `sync` | Upload to cloud hosting |

## Development Status

✅ Phase 1: Backend Foundation (Complete)
- Domain entities
- EF Core with SQLite
- Repository pattern

🚧 Phase 2: MCP Server + Local AI (In Progress)
- MCP Server implementation
- Copilot integration
- JSON export

🚧 Phase 3: Static Hosting (Planned)
- GitHub Pages / Azure Static Web Apps setup
- Sync automation

🚧 Phase 4: iOS App (Planned)
- Swift app reading static JSON
- Offline caching
- Topic browsing

## Documentation

- [Design Document](docs/plans/2026-01-22-sitbrief-design.md)
- [Architecture Simplification Plan](docs/plans/2026-02-04-architecture-simplification.md)

## License

Private project - All rights reserved
