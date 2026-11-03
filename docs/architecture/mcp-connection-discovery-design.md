# MCP Connection & Discovery — Architecture Proposal (Draft)

Status: **Design for review — not implemented**  
Branch: `feature/mcp-connection-discovery-design`  
Milestone: External Tools / MCP Connection & Discovery (no tool execution yet)

## Context and existing boundaries
- AgentForge persists `Tool` records and distinguishes `ToolKind.Custom` from `ToolKind.External`.
- `ToolExecutionGateway` dispatches through `IToolExecutionAdapter`; the existing `CustomToolExecutionAdapter` explicitly rejects external tools.
- Add Tool currently offers OpenAPI, MCP Server and Integration source choices, but the external flow is not wired.
- Preserve the existing Custom Tool behavior. Do not change the execution gateway or register external tools as executable in this milestone.

## Decisions proposed for review
1. **Connections shared at application scope; tool availability scoped per agent.** A connection is not a grant to an agent. Existing agent-tool relationship and authorization must be inspected before implementation.
2. **MCP Streamable HTTP only for v1.** No server-side arbitrary process launch/stdio.
3. **Discover → review → explicitly approve → persist.** Discovery does not grant execution permission.
4. **Snapshot definitions with controlled refresh.** Store approved input schema and a fingerprint; detect changes, show a diff, and require review for changed contracts.
5. **Credentials are references, not plain text in Tool.ConfigurationJson.** Authentication/storage solution to be selected after inspecting existing security infrastructure.

## Conceptual architecture
```text
External Tools UI
  → MCP Connection API (validate configuration, test connection)
  → IMcpConnectionService / IMcpDiscoveryService
  → MCP SDK client (Streamable HTTP; initialize; tools/list pagination)
  → DiscoveredTool DTOs (untrusted, not persisted)
  → Review/Approval API
  → ToolSource/Connection + approved ImportedTool snapshot (persisted)
  → Agent-tool assignment (separate permission boundary)

Future milestone only:
  ExecuteToolCommandHandler → ToolExecutionGateway → McpToolExecutionAdapter → MCP tools/call
```

## Domain concepts (proposal, not schema migration)
| Concept | Responsibility | Candidate fields |
| --- | --- | --- |
| `ToolConnection` / `ToolSource` | One trusted MCP endpoint and its lifecycle | id, name, protocol, endpoint, credential reference, status, last checked |
| `DiscoveredTool` | Temporary server-supplied description | remote name, description, input schema, optional annotations |
| `ImportedTool` | Approved local snapshot linked to a source | local tool id, source id, remote name, schema fingerprint, approved schema, status |
| `AgentToolAssignment` | Agent-specific allowlist | agent id, imported tool id, enabled flag |

Use existing `Tool` and assignment entities where appropriate; **do not introduce new tables until existing models and migrations are reviewed**.

## Proposed API workflow (illustrative, not committed contract)
1. Create connection with HTTPS endpoint and credential reference.
2. Test connection: initialize and negotiate supported capabilities; no tool import.
3. Discover all tool pages with bounded pagination, timeout, response size and count limits.
4. Review: show tool name, description, schema, source and risk indicators. No auto-approval.
5. Import selected definitions with provenance and stable local identifiers; default disabled for agents.
6. Refresh: compare schema fingerprints and mark changed/missing tools for review; never silently widen permissions.

## Trust and security boundaries
- Validate and allowlist endpoint destinations; defend against SSRF, redirects to private IPs, DNS rebinding and metadata endpoints. Enforce TLS, network egress policy, timeouts and bounded responses.
- Treat descriptions and schemas from remote MCP servers as untrusted input, including prompt-injection content; validate schema shape/size and sanitize rendering.
- Store secrets in approved secret storage, never in tool JSON, logs or client payloads.
- Separate **connection permission**, **import approval**, **agent assignment**, and **execution authorization**.
- Tool annotations are hints, not an authoritative risk classification.
- Tool list changes or server notifications trigger **review**, not automatic enabling.
- Audit connection tests, discoveries, approvals and later execution; redact secrets.

## Failure behavior
- Connection failure: preserve previously approved snapshots, mark source unavailable.
- Partial pagination / malformed tool: report discovery failure, do not silently import a partial catalog.
- Name collisions: use source-scoped remote names and stable local IDs.
- Changed or removed tool: mark stale/unavailable pending review; do not auto-delete or auto-execute.
- Cancellation and timeout: propagate and surface bounded, sanitized errors.

## Incremental engineering plan
1. **Design review (this PR):** verify boundaries, domain models, existing agent assignment, authorization and secret storage. Agree on source identity and status model.
2. **Connection foundation:** persisted connection configuration, validation and test-connection endpoint; no import.
3. **Discovery:** MCP SDK initialization, capability negotiation, `tools/list` pagination, normalized DTO and bounded errors.
4. **Approval/import:** review UI, selected-tool persistence, schema snapshots and refresh comparison.
5. **Execution (separate milestone/PR):** approved-only MCP adapter, `tools/call`, runtime auth, auditing, and agent integration.

## Verification and learning evidence
- This is a documentation-only design; **no build, integration test, browser test, network connection or mastery assessment performed**.
- Before implementation: inspect current Core/Infrastructure models, migrations, service registration, auth, and existing agent-tool linking.
- Knowledge checkpoint: explain connection vs discovered vs approved vs agent-enabled vs executable; trace `tools/list` and identify each trust boundary.
- Engineering checkpoint: later prove discovery pagination, invalid endpoints, duplicate names, changed schemas, cancellation, and denied tool execution through tests.

## Open questions for review
1. Is application-wide sharing sufficient, or should connection ownership be workspace/tenant-scoped?
2. What secret store and credential lifecycle already exist?
3. Should import create `ToolKind.External` records directly, or use separate approved snapshot records linked to `Tool`?
4. What approval role and per-agent selection contract are already available?
5. How should disconnected servers and breaking schema changes appear in the UI?
