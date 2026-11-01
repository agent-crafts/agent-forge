# Unified Tool Platform — implementation plan

## Goal
Any registered tool (custom or remote MCP for the MVP; additional providers later) is discoverable, testable, and invokable through one contract. The agent runtime never depends on provider-specific execution code. Registration alone must never authorize execution.

## Existing baseline (main, 2026-11-01)
- Persisted `Tool` metadata, CRUD and `POST /api/tools/{id}/execute`.
- `IToolRegistry` exposes an in-memory `AIFunction` list; `IToolExecutorRegistry` maps approved C# executor names.
- `ExecuteToolCommand` performs partial JSON Schema validation, then resolves an executor by name.
- GitHub issue executor is intentionally repository-restricted.
- Agent-to-tool assignment and external provider execution have not been verified.

## Increment 1 — provider-independent execution seam (implemented on this branch)
- `IToolExecutionGateway` and `IToolExecutionAdapter` introduced.
- Existing registered C# executors run through `CustomToolExecutionAdapter`.
- `ExecuteToolCommand` now delegates dispatch to the gateway.
- Existing endpoint and persisted tool records are unchanged.
- No external source is executable yet; this increment is architectural preparation, not full feature delivery.

## Next increments — required before feature completion
1. **ToolSource persistence (MCP MVP):** source identity, provider type, safe endpoint configuration, secret reference, health, and lifecycle. Add an additive migration and preserve existing tool records.
2. **Canonical tool contract:** stable tool IDs, provider-specific remote name, input/output schemas, schema version/hash, read/write capability, timeouts and limits. Separate source and tool; enforce uniqueness per source.
3. **External adapter:** remote MCP HTTP discovery/list/call through one MCP adapter. OpenAPI import is explicitly deferred. Avoid arbitrary URL execution, redirects to private networks, or user-supplied credentials in schemas.
4. **Registration & test UI:** connect source, validate connection, discover/import, review permissions, register tools, edit mapping, Try Now with schema-generated input and structured result/error; same gateway for test and agent runs.
5. **Runtime integration:** load enabled tool definitions dynamically, filter by agent/user permissions, normalize function calls, validate responses, and enforce approvals for mutations.
6. **Security:** authentication/authorization, network egress allowlist, DNS rebinding/SSRF protection, scoped secret storage, rate and size limits, cancellation, timeout, output sanitization and audit logging.
7. **Verification:** build, unit tests for routing and schema validation, fake MCP integration tests, PostgreSQL migration tests, existing GitHub Try Now regression, and browser E2E for source registration/test.
8. **Rollout:** feature flag new adapters, keep existing custom execution path, run migration/backfill, and gradually enable external sources.

## MVP scope decisions
- Only two adapters: existing approved custom C# executors and remote HTTP MCP tools.
- Do not build an AgentForge MCP server, OpenAPI importer, plugin marketplace, or arbitrary code execution.
- The same backend execution endpoint powers Try Now and agent invocation; both must enforce their caller's permissions.
- External tools must never be dispatched to the custom executor by a matching name alone.

## Architectural constraints
- Persisted tool metadata is not proof of an executable or authorized capability.
- Executor adapters are registered by provider capability, not hardcoded per external tool.
- Discovery is separate from registration; registration is separate from activation; activation is separate from authorization.
- Never pass raw provider secrets to the LLM.
- Agent-to-tool assignments and user/resource policies must be enforced server-side.
- Keep the existing `POST /api/tools/{id}/execute` API compatible during migration.

## Acceptance criteria
- A new external tool can be discovered, registered, and tested without adding C# code for that tool name.
- The same registered tool can be invoked by an authorized agent through the same gateway.
- Disabled/unapproved tools cannot run; failures are normalized; execution is traced.
- Existing custom tools and the GitHub issue Try Now flow continue to work.
- No network request can target an unapproved host or private address.

## Status
Implementation in progress. Increment 1 committed. Remaining items are not implemented or verified.
