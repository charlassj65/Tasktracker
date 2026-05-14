# OCAS Tracker

A .NET 9 Web API for managing task items, built as a technical assignment.

---

> **A note on scope**
>
> I treated this as a production-grade API, so I applied a few practices beyond the minimum requirements:
>
> - **Environment-specific configuration** — separate `appsettings` files for Development, Staging, and Production, each with its own database and log level
> - **Global exception handler** — one place maps exceptions to HTTP responses; controllers have no try/catch blocks
> - **Clean Architecture** — Domain and Application layers have zero infrastructure dependencies, making business logic independently testable
> - **Repository Pattern** — data access is behind an interface; EF Core is invisible to the service layer
> - **Strategy Pattern** — the AI summary provider is swappable via configuration with no code changes
>
> I also wired up **Serilog** for structured logging with level control from `appsettings`. I'm aware that's additional scope for a sample app — happy to skip past it or discuss on our call.

---

## Prerequisites

### To run with Docker (no SDK needed on the host)

| Tool | Version | Check | Install |
|---|---|---|---|
| Docker Desktop | 24 or later | `docker --version` | [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop/) |

### To run with PowerShell scripts (local)

| Tool | Version | Check | Install |
|---|---|---|---|
| .NET SDK | **9.x** | `dotnet --version` | [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) |
| EF Core CLI | any | `dotnet ef --version` | `dotnet tool install --global dotnet-ef` |
| Node.js *(UI only)* | 18 or later | `node --version` | [nodejs.org](https://nodejs.org) |

> The startup scripts check every prerequisite automatically and print a plain-English install instruction if anything is missing.

---

## Repository Structure

```
OCAS/
├── src/
│   ├── api/                              .NET backend
│   │   ├── TaskTracker.sln
│   │   ├── TaskTracker.Api/              Controllers, middleware, DI, configuration
│   │   │   ├── Controllers/
│   │   │   │   └── TasksController.cs
│   │   │   ├── Extensions/
│   │   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   │   └── WebApplicationExtensions.cs
│   │   │   ├── Middleware/
│   │   │   │   └── GlobalExceptionHandler.cs
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   ├── appsettings.Staging.json
│   │   │   └── appsettings.Production.json
│   │   ├── TaskTracker.Application/      DTOs, interfaces, services, business logic
│   │   │   ├── Configuration/
│   │   │   │   └── AiProviderSettings.cs
│   │   │   ├── DTOs/
│   │   │   │   ├── CreateTaskRequest.cs
│   │   │   │   ├── UpdateTaskRequest.cs
│   │   │   │   ├── TaskResponse.cs
│   │   │   │   └── TaskSummaryResponse.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── ITaskRepository.cs
│   │   │   │   ├── ITaskService.cs
│   │   │   │   ├── ITaskSummaryService.cs
│   │   │   │   └── IAiSummaryProvider.cs
│   │   │   └── Services/
│   │   │       ├── TaskService.cs
│   │   │       └── TaskSummaryService.cs
│   │   ├── TaskTracker.Domain/           Core entities and enums (no dependencies)
│   │   │   ├── Entities/
│   │   │   │   └── TaskItem.cs
│   │   │   └── Enums/
│   │   │       └── TaskItemStatus.cs
│   │   ├── TaskTracker.Infrastructure/   EF Core, SQLite, repositories, AI providers
│   │   │   ├── Data/
│   │   │   │   └── AppDbContext.cs
│   │   │   ├── Migrations/
│   │   │   ├── Repositories/
│   │   │   │   └── TaskRepository.cs
│   │   │   └── AI/
│   │   │       ├── SimpleTaskSummaryProvider.cs
│   │   │       └── ExternalAiSummaryProvider.cs
│   │   └── TaskTracker.Tests/            Unit tests
│   │       ├── Services/
│   │       │   ├── TaskServiceTests.cs
│   │       │   └── TaskSummaryServiceTests.cs
│   │       └── Controllers/
│   │           └── TasksControllerTests.cs
│   └── ui/                               React + TypeScript SPA (optional demo)
├── docker-compose.yml
├── start-api.ps1                         Script — API only
├── start-all.ps1                         Script — API + UI
└── README.md
```

---

## How to Run

### Option 1 — API only (PowerShell)

Use this to check the API in isolation — Swagger opens automatically.

```powershell
.\start-api.ps1
```

What it does:

1. Checks .NET SDK and EF Core CLI — prints a clear install hint if either is missing
2. Applies any pending database migrations
3. Starts the API in Development mode

| | |
|---|---|
| API base URL | http://localhost:5200 |
| Swagger UI | http://localhost:5200/swagger |

### Option 2 — API + UI (PowerShell)

```powershell
.\start-all.ps1
```

Opens the API in a new terminal window, then starts the React dev server in this window.

| | |
|---|---|
| API | http://localhost:5200 |
| UI | http://localhost:5173 |

### Option 3 — Docker Compose

No .NET SDK or Node.js required on the host. Migrations run automatically on container startup.

```powershell
docker compose up --build     # first run, or after a code change
docker compose up             # subsequent runs (images already built)
docker compose up -d          # run in the background
docker compose logs -f api    # tail API logs
docker compose down           # stop
docker compose down -v        # stop and wipe the database volume
```

| | |
|---|---|
| API | http://localhost:5200 |
| Swagger UI | http://localhost:5200/swagger |
| UI | http://localhost:3000 |

### Run tests

```powershell
cd src\api
dotnet test
```

---

## API Reference

Base URL: `http://localhost:5200`  
Swagger UI: `http://localhost:5200/swagger` *(Development mode only)*

### Endpoints

| Method | Endpoint | Description | Success | Error |
|---|---|---|---|---|
| `POST` | `/tasks` | Create a new task | `201 Created` | `400 Bad Request` |
| `GET` | `/tasks` | List all tasks | `200 OK` | — |
| `GET` | `/tasks/{id}` | Get a task by ID | `200 OK` | `404 Not Found` |
| `PUT` | `/tasks/{id}` | Update a task | `204 No Content` | `400` / `404` |
| `DELETE` | `/tasks/{id}` | Delete a task | `204 No Content` | `404 Not Found` |
| `GET` | `/tasks/summary/today` | Summary of tasks due today | `200 OK` | — |

### TaskItem fields

| Field | Type | Rules |
|---|---|---|
| `id` | integer | Auto-generated, read-only |
| `title` | string | **Required**, max 100 characters |
| `description` | string | Optional |
| `status` | enum | `Todo` \| `InProgress` \| `Done` |
| `dueDate` | datetime | Optional — ISO 8601 format |

### Request body (POST and PUT)

```json
{
  "title": "Complete assignment",
  "description": "Build Task Tracker API",
  "status": "Todo",
  "dueDate": "2026-05-20T00:00:00"
}
```

### Business rule

A task **cannot** be marked as `Done` if the title is empty or whitespace.  
This rule is intentionally scoped to `Done` status only — tasks with status `Todo` or `InProgress` are not subject to this restriction, matching the assignment spec exactly.

Response when the rule is violated:

```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "A task cannot be marked as Done if the Title is empty or whitespace."
}
```

### Summary endpoint response

```json
{
  "date": "2026-05-13T00:00:00Z",
  "totalTasks": 4,
  "summary": "You have 4 tasks today. 1 completed, 2 in progress, 1 pending."
}
```

---

## Backend Architecture

### Layer responsibilities

| Project | Responsibility |
|---|---|
| `TaskTracker.Domain` | `TaskItem` entity, `TaskItemStatus` enum — no external dependencies |
| `TaskTracker.Application` | DTOs, service interfaces, repository interfaces, business logic |
| `TaskTracker.Infrastructure` | EF Core `DbContext`, SQLite, repository implementations, AI providers |
| `TaskTracker.Api` | Controllers, global exception handler, DI wiring, `appsettings` files |
| `TaskTracker.Tests` | Unit tests against the service and controller layers |

### Dependency direction

```
TaskTracker.Api
    → TaskTracker.Application
    → TaskTracker.Domain          ← no outbound dependencies

TaskTracker.Infrastructure
    → TaskTracker.Application
    → TaskTracker.Domain
```

`Domain` knows nothing about EF Core, HTTP, or any framework.  
`Application` knows nothing about EF Core, SQLite, or ASP.NET Core.  
All infrastructure concerns are confined to `Infrastructure`.

### Design patterns applied

| Pattern | Where |
|---|---|
| **Clean Architecture** | Project layer separation keeps business logic independent of frameworks |
| **Repository Pattern** | `ITaskRepository` / `TaskRepository` — service layer never touches `DbContext` directly |
| **Service Layer** | `ITaskService` / `TaskService` — controllers delegate all logic; they only handle HTTP concerns |
| **Strategy Pattern** | `IAiSummaryProvider` — swap the AI implementation by changing one DI registration |
| **Global Exception Handler** | `GlobalExceptionHandler` implements `IExceptionHandler` — maps `InvalidOperationException` → `400`, everything else → `500` |
| **Dependency Injection** | ASP.NET Core built-in container wires all layers; no `new` inside business code |

### Other implementation notes

- **Validation** — `[Required]` and `[MaxLength(100)]` on request DTOs; `[ApiController]` returns `400` automatically on model state failure.
- **Existence before business rule** — `UpdateAsync` fetches the task first; if it does not exist it returns `false` (→ `404`) before the business rule is ever checked. This gives predictable HTTP semantics.
- **Status persisted as string** — `HasConversion<string>()` keeps the SQLite file human-readable and avoids integer mapping issues if the enum grows.
- **Swagger gated to Development** — both `AddSwaggerGen()` and `UseSwagger()` are inside `IsDevelopment()` guards; Swagger is completely absent in Staging and Production.

### Test coverage — 16 tests, 0 failures

**`TaskServiceTests`** — service layer / business logic

| Test | What it covers |
|---|---|
| `CreateAsync` throws when `status=Done`, title is whitespace | Validation failure |
| `CreateAsync` throws when `status=Done`, title is empty | Validation failure |
| `CreateAsync` succeeds with a valid request | Successful create path |
| `UpdateAsync` returns `false` when task does not exist | Not-found path |
| `UpdateAsync` throws when `status=Done`, title is whitespace | Business rule on update |
| `UpdateAsync` updates entity and calls repository when valid | Successful update path |

**`TaskSummaryServiceTests`** — summary service / AI provider interaction

| Test | What it covers |
|---|---|
| Returns default message when no tasks due today | Empty state |
| Calls `IAiSummaryProvider` exactly once when tasks exist | Provider is invoked correctly |
| Returns the summary produced by the provider | Successful summary path |
| Sets `Date` to today's UTC date | Response shape |

**`TasksControllerTests`** — HTTP layer / controller responses

| Test | What it covers |
|---|---|
| `Create` returns `201 CreatedAtAction` | Successful create |
| `GetById` returns `404` when task not found | Not-found |
| `Update` returns `204 NoContent` on success | Successful update |
| `Update` returns `404` when task not found | Not-found |
| `Delete` returns `204 NoContent` on success | Successful delete |
| `Delete` returns `404` when task not found | Not-found |

---

## Additional Features

### React UI

A React + TypeScript front end (`src/ui`) is included as a demo interface — **not part of the core assignment**, but useful for walking through all endpoints visually.

- Full task CRUD with status badges and a delete confirmation dialog
- Today's summary page with a refresh button
- Built with Vite, React Router v6, Axios, CSS Modules

Run it with `.\start-all.ps1` or `docker compose up --build`.

### AI-style Task Summary

The `GET /tasks/summary/today` endpoint is implemented using a provider abstraction so it runs locally without any external dependency, but can be pointed at a real AI API by changing configuration only — no code changes required.

**Default — local, no API key needed**

`SimpleTaskSummaryProvider` counts tasks by status and returns a plain-language sentence:

```
"You have 4 tasks today. 1 completed, 2 in progress, 1 pending."
```

**Switching to a real AI provider**

Set the following in `appsettings.Development.json`:

```json
"AiProvider": {
  "Type": "External",
  "ApiKey": "your-api-key-here",
  "Model": "your-model-name"
}
```

The DI registration reads the config and swaps the provider automatically. If the external call fails, it falls back to the local summary and logs a warning — the endpoint never returns an error due to an AI outage.

To add any other provider (OpenAI, Azure OpenAI, Vertex AI, etc.): implement `IAiSummaryProvider` in `Infrastructure`, register it in DI. Nothing else changes.
