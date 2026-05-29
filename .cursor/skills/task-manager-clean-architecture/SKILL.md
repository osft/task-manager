---
name: task-manager-clean-architecture
description: >-
  Builds a Task Management Web API (.NET) with Angular frontend using Clean
  Architecture, DDD, strict TDD, Npgsql ADO.NET against PostgreSQL, Factory
  as creation gate, and priority-based Strategy for due-date rules. Use when
  working on the task manager app, user stories, domain logic, use cases,
  Web API endpoints, or Angular integration.
---

# Task Manager — Clean Architecture & TDD

## Cursor Prompt Prefix

Always start backend work with:

> Read and follow `.cursor/skills/task-manager-clean-architecture/SKILL.md`. TDD only: write failing xUnit tests first; wait for my approval before implementation.

## Role

Expert .NET software architect specializing in Clean Architecture, DDD, and strict Test-Driven Development (TDD). Building a Task Management Web API with an Angular frontend backed by PostgreSQL.

## User Story

> **As a Project Manager handling tight deadlines**, I want to **create and track tasks with automatic, priority-based rules**, **so that I can securely manage my daily workload and ensure high-urgency tasks are validated with stricter deadlines.**

### Acceptance Criteria

#### Secure Access
- Users must log in to see or modify tasks.
- Tasks belong to a specific user and cannot be accessed by others.

#### Dynamic Behavior (Strategy Pattern)
- **High** priority: due date must be within **48 hours** from the moment of creation.
- **Standard / Low** priority: due date must be in the future, up to **1 year** from creation.

#### Immutability & Integrity (Factory Pattern)
- A task can never exist in an invalid state.
- It must be fully validated at the exact second of creation.
- `TaskFactory` is the **only** gate that materializes a `Task` entity. If validation fails, no entity is returned and nothing is persisted.

## Strict Constraints

- **No EF Core, Dapper, or MediatR.** All data access must use native ADO.NET with **Npgsql** (`NpgsqlConnection`, `NpgsqlCommand`, `NpgsqlDataReader`) or raw in-memory collections mapped manually.
- **Database:** PostgreSQL (local Docker or hosted).
- **Clean Architecture:** Strict separation between Domain, Application (Use Cases), Infrastructure (Data Access), and Presentation (Web API). Dependencies flow inward.
- **TDD Workflow:** For any feature requested, always write the failing unit tests first using xUnit and FluentAssertions/Moq. Do not write the implementation until the user approves the tests.
- **Design Patterns:** **Factory Pattern** as validated creation gate; **Strategy Pattern** for priority-based due-date rules.

## Layer Responsibilities

| Layer | Contains | Depends On |
|-------|----------|------------|
| **Domain** | Entities, value objects, `ITaskFactory`, `ITaskDueDateValidationStrategy`, resolver | Nothing |
| **Application** | Use cases, repository interfaces, DTOs | Domain |
| **Infrastructure** | Npgsql ADO.NET repositories, manual mapping, JWT auth | Application |
| **Presentation** | ASP.NET Core Web API controllers | Application |

## Domain Model

### Task
- `Id`, `UserId`, `Title`, `Description`
- `Status`: Todo, InProgress, Done
- `Priority`: **High**, **Standard**, **Low**
- `DueDate`, `CreatedAt`, `UpdatedAt`

### User
- `Id`, `Email`, `PasswordHash`, `CreatedAt`

## Design Patterns

### Factory Pattern — creation gate

`ITaskFactory.Create(...)` is the sole path to instantiate a `Task`:

1. Capture `createdAtUtc` (injectable in tests for deterministic boundaries).
2. Resolve due-date strategy via `ITaskValidationStrategyResolver`.
3. Run strategy validation + invariants (e.g. non-empty title).
4. Return a fully valid entity **or** fail — never a partial/invalid task.

### Strategy Pattern — priority-based due-date rules

| Class | Rule |
|-------|------|
| `HighPriorityDueDateStrategy` | `dueDate > createdAtUtc` AND `dueDate <= createdAtUtc + 48 hours` |
| `StandardLowPriorityDueDateStrategy` | `dueDate > createdAtUtc` AND `dueDate <= createdAtUtc + 1 year` |
| `ITaskValidationStrategyResolver` | Maps `TaskPriority.High` → High strategy; `Standard`/`Low` → StandardLow strategy |

On update, re-validate when priority or due date changes, anchored to the task's original `CreatedAt`.

## TDD Workflow (mandatory)

1. Receive a user story or acceptance criterion.
2. Write **failing** unit tests only (xUnit + FluentAssertions + Moq).
3. Present tests to the user and **wait for approval**.
4. Only after approval, implement the minimum code to pass tests.
5. Refactor while keeping tests green.

### Priority TDD order
1. Priority due-date strategies + resolver (boundary tests at +47h, +49h, +366 days).
2. `TaskFactory` creation gate (rejects invalid input before any repository call).
3. Auth (register/login, JWT).
4. Task CRUD use cases with user isolation.
5. Npgsql repositories and API integration tests.

## Prohibited

- EF Core, Dapper, MediatR
- Implementation before test approval
- Outward dependencies from Domain or Application
- Instantiating `Task` outside `TaskFactory`
