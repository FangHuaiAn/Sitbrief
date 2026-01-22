# Sitbrief

A geopolitical news aggregation platform focused on international relations and strategic analysis.

## Overview

Sitbrief helps users build situational awareness by aggregating articles from premium news sources and think tanks, organizing them by topics, and providing AI-assisted content curation.

## Project Structure

```
Sitbrief/
├── src/
│   ├── Sitbrief.API/          # ASP.NET Core Web API
│   ├── Sitbrief.Core/         # Domain entities and interfaces
│   └── Sitbrief.Infrastructure/ # Data access with EF Core
├── docs/
│   └── plans/                 # Design and implementation docs
└── README.md
```

## Tech Stack

- **Backend:** ASP.NET Core 8.0
- **Database:** SQLite (development), PostgreSQL (production)
- **ORM:** Entity Framework Core
- **API Documentation:** Swagger/OpenAPI

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Git

### Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd Sitbrief
```

2. Restore dependencies:
```bash
cd src
dotnet restore
```

3. Run the API:
```bash
cd Sitbrief.API
dotnet run
```

4. Open Swagger UI:
```
http://localhost:5xxx/swagger
```

The database will be created automatically with seed data on first run.

## API Endpoints

### Topics

- `GET /api/topics` - Get all topics
- `GET /api/topics/{id}` - Get topic with articles
- `POST /api/topics` - Create a new topic
- `PUT /api/topics/{id}` - Update a topic
- `DELETE /api/topics/{id}` - Delete a topic

### Articles

- `GET /api/articles` - Get all articles
- `GET /api/articles/{id}` - Get a single article
- `POST /api/articles` - Create a new article
- `PUT /api/articles/{id}` - Update an article
- `DELETE /api/articles/{id}` - Delete an article

## Development Status

✅ Phase 1: Backend Foundation (Complete)
- Solution structure
- Domain entities
- EF Core with SQLite
- Repository pattern
- RESTful API endpoints
- Seed data

🚧 Phase 2: Admin Web App (Planned)
- Blazor WebAssembly
- Authentication
- Article management UI
- Topic management UI

🚧 Phase 3: AI Integration (Planned)
- Claude API integration
- Automatic topic suggestions
- Content analysis

🚧 Phase 4: iOS App (Planned)
- Native Swift app
- Situational awareness UI
- Bookmark functionality

## Documentation

- [Design Document](docs/plans/2026-01-22-sitbrief-design.md)
- [Phase 1 Implementation Plan](docs/plans/2026-01-22-phase1-backend-foundation.md)

## License

Private project - All rights reserved
