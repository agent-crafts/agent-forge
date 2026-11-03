# Execution Gateway — focused learning and implementation scope

## Why
The existing `ExecuteToolCommandHandler` used to resolve C# executors by name. The gateway introduces one execution dispatch seam without changing the public Try Now endpoint or persisted Tool model.

## Current flow
`POST /api/tools/{id}/execute` → `ExecuteToolCommandHandler` (loads active tool and checks input) → `IToolExecutionGateway` → `CustomToolExecutionAdapter` → approved `IToolExecutor` → `ToolExecutionResult`.

## Responsibilities
- **Handler:** fetch tool, enforce active status, perform existing input checks. Current JSON Schema validation is partial; do not describe it as full schema compliance.
- **Gateway:** select exactly one matching adapter. Return `executor_unavailable` for zero matches, `ambiguous_executor` for multiple matches. Pass cancellation through.
- **Custom adapter:** only handle `ToolKind.Custom`; resolve an explicitly registered executor by tool name. Reject non-custom tools even when called directly.
- **Executor:** implement the approved operation. Provider-specific errors belong here or in the execution boundary.

## What is intentionally out of scope
MCP, OpenAPI, ToolSource persistence, dynamic external registration, agent runtime orchestration, and an AgentForge MCP server. Do not add abstraction layers or speculative provider support in this PR.

## Security boundary and limitations
- A persisted Tool record does not authorize execution. Only explicitly registered custom executors can run through the current adapter.
- The gateway is a dispatcher, **not** an authorization service or full input validator. The existing endpoint uses anti-forgery protection, but caller permissions must be reviewed separately before exposing privileged tools.
- No arbitrary code execution or arbitrary network endpoint support.
- Avoid claiming that the current partial schema checks or tool activation replace authorization.

## Verification required before ready for review
1. Build the application and run existing checks.
2. Test zero, one and multiple adapter matches; confirm the correct error codes.
3. Test custom versus external kind, including a same-name external tool that must not dispatch to a custom executor.
4. Test inactive tools, invalid input, successful execution and cancellation propagation through the handler.
5. Confirm the existing GitHub Issue Try Now flow still works.
6. Review endpoint authentication/authorization independently.

## Learning checkpoint
Explain the distinction between a persisted Tool, an execution adapter, the gateway, and a concrete executor; trace a request through each layer and explain why the gateway does not own business logic or authorization.

## Status
Execution seam implemented on this branch; build and automated test verification pending. External integrations are a separate future milestone.
