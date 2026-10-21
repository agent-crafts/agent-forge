# AgentForge — Architecture Foundation Assessment

**Scope:** Read-only inspection of `main` on 2026-10-21. This is an architecture assessment, not a feature implementation or proof of build/test success.

## Current architecture (verified in source)

| Layer | Existing implementation | Assessment |
| --- | --- | --- |
| Core | `AgentForge.Core` contains `ILlmClient` and `IConfigService` | Lightweight abstractions; no project domain entities yet |
| Application | `ChatCommand`, `ConfigService`, DI registration | Single-message use case; no project application services |
| Infrastructure | Typed `HttpClient` implementation `LlmClient` and DI | Local OpenAI-compatible inference; no database provider or repository |
| Web | ASP.NET Core MVC/Razor, `ChatController`, `WorkspaceController`, shared shell | UI foundation exists; workspace routes are placeholders |

Project references follow `Web → Application + Infrastructure`, `Application → Core`, and `Infrastructure → Core`. These are consistent with the intended inward dependency direction.

### Concrete evidence

- `AgentForge.Web/Program.cs`: MVC registration, application/infrastructure DI, routing and authorization middleware; no authentication scheme, DbContext, or migration registration.
- `AgentForge.Web/Controllers/WorkspaceController.cs`: navigation-only pages; `projects`, `agents`, `tools`, `knowledge`, and `runs` are among the placeholder routes.
- `AgentForge.Web/Views/Shared/_Sidebar.cshtml`: global Development Operations and Agent Studio navigation; no selected-project context.
- `AgentForge.Web/Views/Shared/_PlaceholderContent.cshtml`: explicitly states backend integration is planned.
- `AgentForge.Application/Chat/Commands/ChatCommand.cs`: delegates a single message to `ILlmClient`.
- `AgentForge.Infrastructure/Clients/LlmClient.cs`: sends a single user message to `chat/completions`; no tool execution loop or persistent conversation state.
- `AgentForge.Web/appsettings.json`: local inference configuration; no persistence configuration.
- `AgentForge.*/*.csproj`: .NET 10; inspected project files have no EF Core or database provider references.

## Foundation gaps and decisions

| Priority | Gap | Recommendation |
| --- | --- | --- |
| P0 | No Project entity or lifecycle | Introduce a `Project` aggregate with stable ID, name, description, timestamps and explicit lifecycle rules in Core |
| P0 | No project persistence | Add a provider-backed DbContext/repository in Infrastructure and migration strategy; choose provider before coding |
| P0 | No project application use cases | Implement create/list/get/update/archive with validation and cancellation support |
| P0 | No project-aware routing | Make the project list the entry point and use stable project IDs in project-scoped Agent Studio routes |
| P0 | No authentication or project authorization | Define identity and membership model before claiming tenant isolation; do not treat project IDs alone as access control |
| P1 | No test projects observed in repository tree | Add unit tests for domain rules and integration tests for project persistence/authorization |
| P1 | No project-scoped audit/observability contract | Define project IDs in trace/log context, avoid logging secrets and sensitive prompts |
| P1 | Repository includes tracked `bin/`, `obj/`, `.DS_Store` | Add suitable `.gitignore` and clean tracked artifacts in a separate focused PR |

## Proposed implementation slices (not yet implemented)

1. **Project domain + application contracts:** domain rules, DTOs/commands, validation, unit tests. No speculative multi-tenant framework.
2. **Persistence + migration:** choose database, implement EF Core mapping, repositories and integration tests.
3. **Project web flow:** list/create/detail/edit/archive, project-aware navigation, CSRF protection, error handling.
4. **Access control:** authentication and membership checks before allowing cross-user access; negative authorization tests.
5. **Verification:** build, automated tests, migration smoke test, manual project switching and data-isolation checks.

## Open decisions before implementation

- Database provider for local development and production (SQLite, PostgreSQL, SQL Server).
- Whether the first release is single-user local-only or multi-user from day one; determines when authentication/membership is required.
- Whether `Workspace` is a first-class persisted entity now or deferred until cross-project sharing is needed.
- Archive versus hard-delete semantics, uniqueness scope for project names, and project ID URL format.

## Verification limits

This assessment was based on GitHub source inspection and repository tree metadata. **No `dotnet build`, `dotnet test`, runtime execution, security test, or migration was run.** Recommendations are proposals, not implemented or verified capabilities.

**Next action:** Agree on the database and identity boundary, then implement the smallest Project domain/application slice on a separate feature branch with tests.
