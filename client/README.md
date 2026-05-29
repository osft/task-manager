# Task Manager — Angular Client

Angular 19 SPA for the Task Manager interview project. Consumes the .NET Web API with JWT authentication and provides full task CRUD for project managers.

## Stack

- **Angular 19** (standalone components, lazy routes)
- **SCSS** with a dark editorial UI (Fraunces + IBM Plex Sans)
- **HttpClient** + functional JWT interceptor
- **Reactive forms** for auth and tasks

## Prerequisites

- [Node.js LTS](https://nodejs.org/) 18+ (22 recommended)
- [.NET 8 SDK](../README.md) and API running locally
- PostgreSQL via Docker (see root [README](../README.md))

## Quick start

```powershell
# Terminal 1 — database + API
cd c:\repos\task-manager
docker compose up postgres -d
dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj

# Terminal 2 — Angular dev server
cd client
npm install
npm start
```

- **UI:** http://localhost:4200
- **API:** http://localhost:8082 (configured in `src/environments/environment.development.ts`)

CORS is enabled on the API in Development for `http://localhost:4200`. An optional dev proxy is in `proxy.conf.json` (not required when using the environment `apiBaseUrl`).

## Demo login (seeded backend)

| Field | Value |
|-------|-------|
| Email | `demo@taskmanager.local` |
| Password | `Demo123!` |

Use **Use demo account** on the sign-in page to pre-fill credentials.

## Scripts

| Command | Description |
|---------|-------------|
| `npm start` | Dev server (`ng serve`, port 4200) |
| `npm run build` | Production build → `dist/client` |
| `npm test` | Unit tests (Karma) |

## Application structure

```text
client/src/app/
├── core/           # AuthService, TaskService, JWT interceptor, guards, models
├── features/
│   ├── auth/       # login, register, auth/callback (OAuth stub)
│   └── tasks/      # task list, create/edit form
├── layout/         # authenticated app shell
├── shared/         # priority badge, etc.
└── app.routes.ts
```

See [docs/FRONTEND-ARCHITECTURE.md](../docs/FRONTEND-ARCHITECTURE.md) for routing, auth flow, and API mapping.

## API integration

| Feature | Endpoint |
|---------|----------|
| Login | `POST /api/auth/login` |
| Register | `POST /api/auth/register` |
| Profile | `GET /api/me` |
| Tasks | `GET/POST/PUT/DELETE /api/tasks` |
| Statuses | `GET /api/task-statuses` |

Errors return `{ "error": "message" }` from the API middleware.

### Priority UX

- **High:** amber badge with pulse; due date capped at 48h from creation (create) or original `createdOnUtc` (edit)
- **Standard / Low:** up to one year from creation anchor
- Domain validation messages from the API are shown on save failure

## Environment config

| File | Use |
|------|-----|
| `environment.development.ts` | `apiBaseUrl: 'http://localhost:8082'` |
| `environment.ts` | Production — empty `apiBaseUrl` (same-origin via nginx in Phase 8) |

## Build for production

```powershell
npm run build
```

Output: `client/dist/client/browser`.

## Run in Docker (full stack)

From the repository root:

```powershell
docker compose up --build
```

Open **http://localhost:8083** — nginx serves the SPA and proxies `/api` to the API container. Production build uses an empty `apiBaseUrl` (same-origin requests).

| Port | Service |
|------|---------|
| **8083** | Angular UI (nginx) |
| **8082** | API + Swagger (direct) |

## Troubleshooting

| Issue | Fix |
|-------|-----|
| CORS errors | Ensure API runs with `ASPNETCORE_ENVIRONMENT=Development` |
| 401 on tasks | Sign in again; token may have expired |
| Empty task list | Log in as a user who owns tasks, or create a new task |
| API connection refused | Start Postgres + API; confirm port **8082** |

## Related docs

- [Root README](../README.md)
- [ARCHITECTURE.md](../ARCHITECTURE.md)
- [FRONTEND-ARCHITECTURE.md](../docs/FRONTEND-ARCHITECTURE.md)
