# Task Manager

Task Management Web API (.NET 8) with Angular frontend, Clean Architecture, TDD, and PostgreSQL (EF Core / Npgsql).

## Assessment submission

| Deliverable | Link |
|-------------|------|
| **Public GitHub repository (single link)** | **https://github.com/osft/task-manager** |
| **Thought process / presentation** | [PRESENTATION.md](PRESENTATION.md) |
| **GenAI workflow (required)** | [docs/GENAI.md](docs/GENAI.md) |

## User Story

> **As a Project Manager handling tight deadlines**, I want to **create and track tasks with automatic, priority-based rules**, **so that I can securely manage my daily workload and ensure high-urgency tasks are validated with stricter deadlines.**

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)

## Phase 0 — Local database

1. Copy environment template:

   ```powershell
   copy .env.example .env
   ```

2. Start PostgreSQL (schema + seed data applied on first run):

   ```powershell
   docker compose up postgres -d
   ```

3. Verify Postgres is healthy:

   ```powershell
   docker compose ps
   ```

   Connection: `localhost:5433` — database `taskmanager`, user/password `taskmanager` / `taskmanager_dev`.

   If port 5433 is also in use, set `POSTGRES_PORT` in `.env`.

## Build and test

```powershell
dotnet restore
dotnet build
dotnet test
```

Integration tests (`[Trait("Category", "Integration")]`) require PostgreSQL running (`docker compose up postgres -d`). Override the connection with `TASK_MANAGER_TEST_CONNECTION` if needed.

## Run API (host dev)

```powershell
docker compose up postgres -d
dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj
```

Default URL: **http://localhost:8082** (see `launchSettings.json`). Database: **localhost:5433**.

## Run Angular client (host dev)

```powershell
cd client
npm install
npm start
```

- UI: **http://localhost:4200**
- Ensure the API is running on **8082** (CORS enabled in Development)

- Health: `GET /api/health` (anonymous)
- Swagger (Development only): `http://localhost:8082/swagger`
- Register: `POST /api/auth/register`
- Login: `POST /api/auth/login` → use returned JWT for protected routes
- Profile: `GET /api/me`
- Tasks: `GET/POST /api/tasks`, `GET/PUT/DELETE /api/tasks/{id}`
- Task statuses: `GET /api/task-statuses`

## Full stack in Docker (Phase 8)

One command for Postgres + API + Angular (nginx):

```powershell
copy .env.example .env
docker compose up --build
```

| Service | URL |
|---------|-----|
| **App (UI)** | http://localhost:8083 |
| **API / Swagger** | http://localhost:8082/swagger |
| **Postgres** | `localhost:5433` |

The UI container proxies `/api` to the API service, so the browser uses a single origin on port **8083**.

### API only in Docker

```powershell
docker compose up postgres api --build
```

- API: http://localhost:8082
- Swagger: http://localhost:8082/swagger

## Logging (log4net)

The API uses **log4net** via `Microsoft.Extensions.Logging.Log4Net.AspNetCore`. Configuration: [`src/TaskManager.Api/log4net.config`](src/TaskManager.Api/log4net.config).

| Output | Location |
|--------|----------|
| Console | Standard output when running `dotnet run` |
| Rolling file | `src/TaskManager.Api/logs/taskmanager-YYYYMMDD.log` (relative to the API working directory) |

- Root level: **INFO**; `TaskManager` namespace: **DEBUG** in Development.
- Failed login/register attempts are logged at **Warning** (email only — never passwords or tokens).
- Unhandled exceptions are logged at **Error** in `ExceptionHandlingMiddleware`.

To change verbosity, edit `log4net.config` or set `Logging:LogLevel` in `appsettings.json` (ILogger bridge).

## Demo credentials (seeded in PostgreSQL)

| Field | Value |
|-------|-------|
| Email | `demo@taskmanager.local` |
| Password | `Demo123!` |
| Name | Demo PM |
| Alias | demo-pm |

## Documentation

| Document | Description |
|----------|-------------|
| [PRESENTATION.md](PRESENTATION.md) | Interview narrative and thought process |
| [docs/GENAI.md](docs/GENAI.md) | GenAI prompts, validation, corrections (required) |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Layers, patterns, phase map, system diagram |
| [client/README.md](client/README.md) | Angular client setup |
| [docs/FRONTEND-ARCHITECTURE.md](docs/FRONTEND-ARCHITECTURE.md) | Angular routing, auth, API mapping |
| [docs/PHASE-7-DOCUMENTATION.md](docs/PHASE-7-DOCUMENTATION.md) | Checklist for Phase 7 code + docs |

## Solution layout

```text
src/
  TaskManager.Domain/           # Entities, Factory, Strategy (Phase 1+)
  TaskManager.Application/      # Use cases, interfaces
  TaskManager.Infrastructure/   # EF Core + Npgsql, auth providers
  TaskManager.Api/              # Web API, Swagger, controllers
client/                         # Angular SPA (Phase 7)
tests/                          # xUnit + FluentAssertions + Moq
db/init.sql                     # Schema + seed data
docker-compose.yml              # Postgres + API + client (ports 5433 / 8082 / 8083)
Dockerfile                      # Multi-stage API image
Dockerfile.client               # Angular build + nginx (/api proxy)
```

## Development workflow (TDD)

1. Write failing tests first.
2. Get approval before implementation.
3. Implement minimum code to pass.
4. Refactor with green tests.

See [`.cursor/skills/task-manager-clean-architecture/SKILL.md`](.cursor/skills/task-manager-clean-architecture/SKILL.md).

## Project status

All planned phases (0–8) are **complete**: backend, Angular UI, Docker full stack, and assessment documentation.
