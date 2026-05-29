# Presentation — thought process (Task Manager exercise)

This file is the **presentation / narrative** for the interview assessment: how the solution was approached, why key decisions were made, and how to demo the result.

**Single public repository (code + docs):**  
**https://github.com/osft/task-manager**

---

## 1. Problem framing

The brief asks for a **task management app** with:

- Clean Architecture and TDD  
- User authentication and per-user task isolation  
- **Priority-based due-date rules** (business logic, not just CRUD)  
- Angular frontend and Docker deploy  
- **GenAI workflow** with critical review ([docs/GENAI.md](docs/GENAI.md))

I anchored everything on one user story:

> *As a Project Manager handling tight deadlines*, I want to *create and track tasks with automatic, priority-based rules*, *so that* I can securely manage my workload and ensure high-urgency tasks have stricter deadlines.

That story drives **Factory** (validated creation), **Strategy** (48h vs 1 year), and **JWT-scoped tasks**.

---

## 2. Architecture thought process

```text
Angular (8083)  →  nginx /api proxy  →  Web API (8082)  →  Use cases  →  Domain
                                              ↓
                                    EF repositories (Infrastructure)
                                              ↓
                                         PostgreSQL (5433)
```

**Why Clean Architecture layers?**

| Layer | Role |
|-------|------|
| **Domain** | `TaskFactory`, priority strategies, `TaskItem` — zero dependencies |
| **Application** | Use cases + repository **interfaces** — orchestration only |
| **Infrastructure** | EF Core, JWT, BCrypt — implements ports |
| **API** | Thin controllers, DTO validation, Swagger (dev) |
| **Client** | Angular SPA, JWT interceptor, custom calendar UX |

**Patterns chosen deliberately:**

- **Factory** — only path that creates `TaskItem`; invalid due dates never become entities.  
- **Strategy** — `HighPriorityDueDateStrategy` vs `StandardLowPriorityDueDateStrategy`; resolver picks by enum.  
- **Repository** — Application depends on `ITaskRepository`, not EF types.  
- **No MediatR / Dapper** — per brief; explicit use-case classes keep the panel demo traceable.

---

## 3. Phased delivery (how the work unfolded)

| Phase | Focus | Outcome |
|-------|--------|---------|
| **0** | Docker Postgres, `db/init.sql`, solution scaffold | Repeatable DB + seed user |
| **1–2** | Domain TDD: Factory + strategies | 48h / 1yr rules with boundary tests |
| **3** | Auth + JWT (OAuth/MFA-ready stubs) | Login, register, `/api/me` |
| **3b** | log4net | Structured logging without leaking secrets |
| **4** | Task CRUD use cases | User isolation in application layer |
| **5** | EF Core repositories | Single persistence stack |
| **6** | REST API + integration tests | Swagger, DTO vs domain 400s |
| **7** | Angular UI | Auth, board, form, priority UX |
| **8** | Full Docker compose | One-command demo |

Each phase ended with **`dotnet test`** or **`ng build`** green before moving on.

---

## 4. TDD example (what to show in the panel)

**Red:** `HighPriorityDueDateStrategyTests` — due date 49 hours after creation must fail.  

**Green:** `HighPriorityDueDateStrategy` enforces `TimeSpan.FromHours(48)`.  

**Refactor:** `TaskFactory` calls strategy before constructing `TaskItem`; `UpdateTaskUseCase` re-validates against original `CreatedOnUtc` via `ITaskDueDateValidator`.

This proves business rules live in **Domain**, not in controllers or SQL.

---

## 5. Security & data isolation

- JWT on all task routes; anonymous only for health + auth.  
- Repository methods filter by `UserId` (`GetByIdForUserAsync`, list scoped).  
- Wrong user's task ID → **404** (not 403) to avoid leaking existence.  
- Passwords hashed with BCrypt; tokens in client `localStorage` (SPA trade-off documented).

---

## 6. UI thought process

Initial dark “command center” theme hid the native **datetime-local** picker. After feedback:

- Switched to **Client Manager–style** layout: dark sidebar + light content.  
- Built a **custom calendar popup** (`datetime-picker`) with visible grid + time selects.  
- Added **Close session** in top-bar user menu (clears JWT, redirects to login).

Client due-date hints mirror server rules; **server always wins** on save.

---

## 7. DevOps & demo script

```powershell
copy .env.example .env
docker compose up --build
```

| Step | Action |
|------|--------|
| 1 | Open **http://localhost:8083** |
| 2 | Login: `demo@taskmanager.local` / `Demo123!` |
| 3 | Create **High** task with due date within 48h → success |
| 4 | Create **High** task beyond 48h → API 400, message shown in UI |
| 5 | Create **Standard** task with due date within 1 year → success |
| 6 | Optional: Swagger at **http://localhost:8082/swagger** |
| 7 | **Close session** from profile menu → back to login |

---

## 8. GenAI role (summary)

See **[docs/GENAI.md](docs/GENAI.md)** for full detail. In short:

1. **Prompted** with skills + constraints (TDD, no MediatR, Factory/Strategy).  
2. **Validated** with tests, layering checks, manual edge cases.  
3. **Corrected** duplicated rules, marker scaffolds, Angular signal bug, DB error mapping, UI calendar visibility.

AI accelerated scaffolding; **architecture, review, and test gates were human-driven.**

---

## 9. What I would do next (out of scope)

- Pagination on task list  
- Server-side logout / token blocklist  
- CI pipeline (GitHub Actions: build + test + Docker)  
- EF migrations instead of init-only SQL for production evolution  

---

## 10. Assessment checklist

| Requirement | Location |
|-------------|----------|
| Public GitHub repo (single link) | https://github.com/osft/task-manager |
| Thought process / presentation | This file (`PRESENTATION.md`) |
| GenAI workflow (mandatory) | [docs/GENAI.md](docs/GENAI.md) |
| Runnable demo | [README.md](README.md) — `docker compose up --build` |
| Architecture reference | [ARCHITECTURE.md](ARCHITECTURE.md) |
