# GenAI workflow — Task Manager interview exercise

This document satisfies the **GenAI portion** of the assessment: how generative AI (Cursor Agent + Claude) was used, how outputs were validated, and what was corrected before accepting code.

**Tooling:** [Cursor IDE](https://cursor.com) with project-local skills under `.cursor/skills/` and phased plan under `.plans/`.

---

## 1. Approach

| Principle | How it was applied |
|-----------|-------------------|
| **Skills first** | Before each phase, the agent read `.cursor/skills/task-manager-clean-architecture/SKILL.md` (backend) or `frontend-design/SKILL.md` (UI). |
| **Strict TDD** | Domain and Application slices started with **failing xUnit tests**, then minimal implementation, then refactor. |
| **Phased delivery** | Work followed `.plans/task_manager_interview_plan_0b572b1a.plan.md` (Phases 0–8) to avoid scope creep. |
| **Human review** | I reviewed diffs, ran `dotnet test` / `ng build`, and rejected or corrected AI suggestions that broke layering or duplicated domain rules. |

---

## 2. Example prompts used

### Phase 1 — Domain + Factory (TDD)

```text
Read .cursor/skills/task-manager-clean-architecture/SKILL.md.

User story: As a Project Manager, I want priority-based task rules so High tasks are due
within 48h and Standard/Low within 1 year. Tasks must be fully validated at creation via Factory.

Constraints: Clean Architecture, TDD (failing xUnit tests first), no Dapper/MediatR,
Factory as creation gate, Strategy per TaskPriority.

Deliver: failing tests for HighPriorityDueDateStrategy and TaskFactory rejection cases only.
Do not implement production code yet.
```

### Phase 6 — API layer

```text
Proceed with Phase 6: Tasks API controllers, DTOs, WebApplicationFactory integration tests.
Use ports 8082 (API) and 5433 (Postgres). Controllers must stay thin and delegate to use cases.
```

### Phase 7 — Angular UI

```text
Proceed with Phase 7. Use the frontend-design skill for a distinctive UI.
JWT interceptor, auth guard, task CRUD, priority badges, due-date hints aligned with domain rules.
```

### Code review follow-up

```text
Review the .NET and Angular code for duplication, memory leaks, and performance issues.
Then implement the high/medium fixes from the review.
```

---

## 3. How AI output was validated

| Check | Command / action |
|-------|------------------|
| **Unit & integration tests** | `dotnet test` after every backend slice (74+ tests; integration skipped when Postgres down) |
| **Build** | `dotnet build`, `ng build` |
| **Dependency direction** | Domain has zero Infrastructure references; API calls use cases only |
| **Forbidden packages** | No Dapper, no MediatR; EF Core only in Infrastructure |
| **Security** | Tasks scoped by `UserId`; passwords never logged; JWT on protected routes |
| **Manual edge cases** | High + 49h due date → 400; cross-user task ID → 404; 401 without token |
| **Docker demo path** | `docker compose up --build` → UI :8083, API :8082, Postgres :5433 |

---

## 4. What was corrected (critical review)

Examples where AI output was **changed or rejected**:

| Issue | Correction |
|-------|------------|
| **Priority rules in DTOs** | Kept due-date rules in Domain Factory/Strategy only — not duplicated in API Data Annotations. |
| **Task creation gate** | Ensured `TaskItem` constructor is internal; only `TaskFactory` creates tasks. |
| **Phase 3 ADO.NET → Phase 5 EF** | Consolidated persistence behind EF repositories; removed interim ADO repos from production path. |
| **Unused `IUnitOfWork`** | Documented intent; repos call `SaveChangesAsync` today; UoW reserved for multi-entity transactions. |
| **Registration race** | Mapped PostgreSQL unique violation (`23505`) to **409 Conflict** instead of generic 400. |
| **Email query performance** | Removed `Email.ToLower()` in SQL; emails stored normalized lowercase. |
| **Angular `computed()` bug** | Priority hint used `computed()` on non-signal form value — fixed with `toSignal(valueChanges)`. |
| **Duplicate `/api/me`** | Removed profile fetch from login; shell loads profile once. |
| **Native datetime picker** | Replaced invisible `datetime-local` on dark theme with custom calendar component. |
| **Scaffold noise** | Removed unused `ApplicationAssemblyMarker` / `DomainAssemblyMarker` classes. |
| **Port conflicts** | Default host ports set to **5433 / 8082 / 8083** to avoid local Docker clashes. |

---

## 5. Sample code to show the panel

Recommended walkthrough (Red → Green TDD and patterns):

1. **Strategy boundary test** — `tests/TaskManager.Domain.Tests/Strategies/HighPriorityDueDateStrategyTests.cs`  
   High priority due date at 48h vs 49h from creation.

2. **Factory as creation gate** — `src/TaskManager.Domain/Factories/TaskFactory.cs`  
   Strategy invoked before `TaskItem` is constructed.

3. **Use case + mocked repo** — `tests/TaskManager.Application.Tests/Tasks/CreateTaskUseCaseTests.cs`  
   Factory rejects invalid due date before repository is called.

4. **EF persistence** — `src/TaskManager.Infrastructure/Persistence/Repositories/EfTaskRepository.cs`  
   User-scoped queries, `AsNoTracking` on reads.

5. **API integration** — `tests/TaskManager.Api.Tests/TasksEndpointTests.cs`  
   401 without JWT; 400 for High + 49h (when Postgres available).

6. **Angular priority UX** — `client/src/app/shared/components/datetime-picker/` + `task-rules.ts`  
   Visible calendar; client hints mirror server rules.

---

## 6. What I owned vs what AI generated

| Human-owned | AI-assisted |
|-------------|-------------|
| User story, acceptance criteria, phase plan | Boilerplate, test stubs, repetitive CRUD wiring |
| Architecture boundaries and pattern choices | Initial controller/DTO/entity scaffolding |
| Review of every merge-worthy diff | Dockerfile, nginx config, Angular component structure |
| Demo narrative and port strategy | Documentation drafts (edited before commit) |
| Final decision to reject over-engineering | Suggestions for pagination, caching (accepted selectively) |

---

## 7. Limitations acknowledged

- **EF vs brief wording:** If the written brief says “no EF,” the panel trade-off is documented in [ARCHITECTURE.md](../ARCHITECTURE.md) — Domain/Application stay ORM-free; EF is Infrastructure-only with Repository pattern.
- **Integration tests:** Skipped automatically when PostgreSQL is not running (`SkippableFact`).
- **GenAI doc:** This file describes the workflow honestly; all code was reviewed and test-validated before inclusion in the repository.
