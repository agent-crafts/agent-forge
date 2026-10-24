# PostgreSQL persistence foundation

This slice establishes a **single-user, local-first** persistence boundary. It does **not** provide authentication, tenant isolation, or project authorization. Do not expose project APIs to untrusted users until authorization is implemented.

## Local development

1. Set a non-committed password: `export POSTGRES_PASSWORD='<local-secret>'`.
2. Run `docker compose up -d postgres`.
3. Set `ConnectionStrings__AgentForge='Host=localhost;Port=5432;Database=agentforge;Username=agentforge;Password=<local-secret>'` in your environment or use ASP.NET Core user-secrets.
4. Install `dotnet-ef` compatible with EF Core 10 if needed.
5. Apply migrations explicitly from the repository root:

```bash
dotnet ef database update --project AgentForge.Infrastructure --startup-project AgentForge.Web
```

The application registers the DbContext but **does not automatically migrate** at startup. Production migrations must be applied through a controlled deployment step with a backup and rollback plan.

## Architecture

- `AgentForge.Core`: framework-independent Project entity in `AgentForge.Core/Domains/Projects`.
- `AgentForge.Infrastructure`: Npgsql DbContext, mappings, design-time factory, and versioned migrations.
- `AgentForge.Web`: configuration composition root.
- Secrets must be supplied externally; `appsettings.json` contains no password.
- First schema: `projects` with a UUID key, name/description constraints, archive state, UTC timestamps, and creation-time index.
- Identity and project membership are intentionally deferred.

## Verification checklist (not yet executed)

```bash
dotnet restore
dotnet build
dotnet ef migrations list --project AgentForge.Infrastructure --startup-project AgentForge.Web
dotnet ef database update --project AgentForge.Infrastructure --startup-project AgentForge.Web
```

Then check `projects` and `__EFMigrationsHistory` in PostgreSQL, insert a valid project, verify blank names are rejected, and test a fresh database migration. Review generated migration/snapshot consistency with `dotnet ef migrations has-pending-model-changes`. Avoid running `database update 0` against any database containing valuable data.
