# AgentForge — Mastery Progress Tracker

> **Purpose:** The durable, evidence-based handoff between training sessions, ChatGPT conversations, and AgentForge engineering work.
>
> **Goal:** AI-Native Engineering Mastery = Knowledge + Working Software + Demonstrated Mastery.
>
> **Update policy:** Update this file at the end of each meaningful learning module, implementation milestone, or mastery assessment. Do not mark a skill mastered based only on discussion or a merged PR.

**Agent Runtime Foundation + Tool Calling Loop (2026-11-12, draft PR #37):** Added per-run AgentRunContext, IAgentRuntime/AgentRuntime, bounded multi-inference orchestration with agent-assigned active tool allowlist, gateway execution, tool result messages, execution limits, and a thin Playground command adapter preserving existing HTTP/trace contracts. Changes committed on feature/single-agent-runtime-foundation; no persistent sessions or schema changes. Review comment addressed: tools catalog loaded once and filtered by assigned IDs; orchestration extracted into focused private methods. Build and Playground smoke test remain unverified (dotnet CLI unavailable in execution environment). Knowledge: runtime boundaries discussed; Engineering: implementation pending verification; Mastery: not assessed. User requested no automated test code. Next: run dotnet build and manual Playground smoke tests for text-only, allowed tool, denied tool, tool failure, and execution limits; review PR diff before merge.\n\n
**MCP response size fix (2026-11-10, PR #27):** Increased the bounded MCP HTTP response allowance from 1 MiB to 8 MiB and introduced a distinct `mcp_response_too_large` failure. Committed to the existing PR branch; build, protocol regression tests, and live DeepWiki Try Now retry remain unverified. This is a bounded mitigation, not streaming/pagination of large tool results; mastery not assessed.

**MCP execution Phase 1 (2026-11-03, feature/mcp-tool-execution):** Implemented initial guarded Streamable HTTP MCP client (`initialize`, initialized notification, `tools/call`), external MCP execution adapter, and DI gateway registration. User manually confirmed earlier discovery/import flow works. Execution build, automated tests, real MCP calls, auth review and browser verification remain pending; mastery not assessed. Next: verify against a real MCP endpoint, add protocol/error tests, review security and refine SSE handling before merge.

**MCP discovery controller cleanup (2026-11-03, PR #26):** Extracted MCP transport/discovery into scoped typed `McpDiscoveryService`; both discover and import reuse the same service, removing controller-action invocation and JSON round-trip. Preserved API routes and MCP execution deferral. Committed; build, protocol, database, and browser verification pending; mastery not assessed.

**MCP import/persistence (2026-11-03, PR #26):** Added server-revalidated selected-tool import API and atomic batch metadata persistence command; wired external Save to import selected switches. Persists kind External, integration category, schema, endpoint/tool name and non-executable mcp executor key. Duplicate-name imports return conflict; MCP execution deferred. Code committed; build, DB and browser verification pending; mastery not assessed.

**MCP discovery switch selection (2026-11-03):** Replaced left checkboxes with right-aligned shared agent-tool-toggle switches; removed Select all UI and behavior, retained selected count and search. Commits on PR #26; static source review only, build/browser verification pending.\n\n**MCP grid card parity (2026-11-03):** Updated MCP discovery card markup to reuse Tools gallery shared card/icon/content/badge CSS classes, preserving selection and one-line description. Committed to PR #26; build and browser verification pending, mastery not assessed.

**MCP tool cards (2026-11-03):** Render discovered tools as compact selectable cards with title, single-line description and optional Read/Write keyword badge inferred only from tool names. Feature branch PR #26; no build/browser verification, no registration or mastery assessment.

**MCP discovery unstyled reset (2026-11-03):** Removed custom classes and generated row styles from MCP-only modal section while preserving discovery/search/selection logic. Changes committed on PR #26; build/browser verification pending.

**MCP discovery UI refinement (2026-11-03):** Added partial Select All state, selected-row highlighting, and no-search-results feedback in PR #26. Changes committed to feature branch; build and browser checks pending. No tool registration or mastery assessment.

**MCP discovery UI rewrite (2026-11-03):** Rebuilt existing external-tool modal as a compact endpoint + searchable results layout matching the supplied reference; removed decorative badges and redundant headings, improved row hierarchy and filtering, and disabled unsupported external Save. Committed on PR #26; build/browser verification pending, registration not implemented, mastery not assessed.

**MCP discovery modal simplification (2026-11-03):** Removed stepper and replaced external-tool UI with endpoint/discover controls and searchable selectable results. UI committed on PR #26; registration remains unimplemented. Build and browser verification pending; mastery not assessed.

**Agent controller merge (2026-11-01):** Removed `AgentsApiController`. `AgentsController` is now the `/api/agents` API, matching `ToolsController`. Agent gallery and details actions moved to `WorkspaceController`. Build succeeded; browser verification pending. Mastery not assessed.

**Agent modal alignment (2026-11-01):** Create/edit agent dialog now uses the tool dialog shell (720px, header, footer, field labels, inline errors, save spinner). Agent fields unchanged: name, category, description, icon, tags. Browser verification pending. Mastery not assessed.

**Agent Overview simplification (2026-10-27):** Removed redundant Capabilities panel from `_OverviewTab.cshtml`; tags and dedicated Tools/Skills tabs retained. UI change committed; browser verification pending. No mastery assessment.

**Agent Overview preview cleanup (2026-10-27):** Removed static Preview status panel; UI-only change committed, browser verification pending. Mastery not assessed.

**Agent Overview Key features cleanup (2026-10-27):** Removed duplicate Key features list from About this agent; retained description, Tags, and Typical use cases. UI committed; browser verification pending.

**Agent creation/profile instructions split (2026-10-27):** Creation modal now has only identity fields; empty system instructions allowed in domain; profile Instructions tab has editable textarea and Save Changes wired to GET/PUT Agent API, preserving other fields. Code committed on PR #22; build, browser and DB behavior unverified; no tests added per user preference. Mastery not assessed.

**Agent create HTTP 500 investigation (2026-10-27):** Found PostgreSQL instructions check constraint still required nonblank values after creation modal began sending empty instructions. Updated EF configuration and snapshot; added `AllowEmptyAgentInstructions` migration. Likely cause identified from code, not confirmed by server exception. Migration execution, build, and browser verification pending; no tests added. Next: apply migration and retry creation.

**Agent Overview Typical use cases cleanup (2026-10-27):** Removed tag-generated Typical use cases panel; retained About, Agent information, and Tags. Change committed on PR #22; browser verification pending. No mastery assessment.

**Agent Overview Tags placement (2026-10-27):** Moved Tags card beneath About this agent in left column; Agent information remains in right column. UI-only change committed to PR #22; browser verification pending.

**Agent creation toast (2026-10-27):** Reused shared `AgentForgeToast.afterNavigation` for success notification after persisted create and gallery reload. Committed on PR #22; browser verification pending. No tests added; mastery not assessed.

**Agent Instructions Markdown editor (2026-10-27):** Added formatting toolbar for heading, bold, italic, lists, code, and links using textarea selection and Markdown insertion; existing Save API preserved. Committed on PR #22; build and browser verification pending; no tests added; mastery not assessed.

**Agent information persistence wiring (2026-10-27):** Exposed persisted CreatedAt, UpdatedAt, IsActive in AgentCard and displayed timestamps, status, ID, category in Overview. Removed unsupported Version, Author, Model, Temperature placeholders pending real features. Committed to PR #22; build/browser verification pending; mastery not assessed.

**Reusable Markdown editor (2026-10-27):** Replaced bespoke Instructions toolbar with shared `_MarkdownEditor` partial and EasyMDE integration; supports standard formatting, Markdown preview, side-by-side, fullscreen, and sanitized preview via DOMPurify. Preserves Markdown persistence and 12k length check. External pinned CDN assets required; dependency availability, build, browser and preview verification pending. No tests added; mastery not assessed.

**Instructions save toast (2026-10-27):** Added shared success toast after confirmed instructions PUT; existing inline error feedback retained. Committed on PR #22; browser verification pending. No mastery assessment.

**Instructions feedback cleanup (2026-10-27):** Removed duplicate inline success message beside Save Changes; success uses shared toast, while validation and save errors remain inline. Committed on PR #22; browser verification pending.

**Markdown toolbar contrast fix (2026-10-27):** Enabled EasyMDE icon-font loading and scoped light-toolbar/dark-icon contrast, hover, active, and editor surface colors. Committed on PR #22; actual browser verification pending; no mastery assessment.

**Instructions editor action alignment (2026-10-27):** Removed redundant field label and placed Save Changes with EasyMDE status information, retaining fallback placement when editor is unavailable. Changes committed on PR #22; browser verification pending.

**Parameterized HTTP URL templates (2026-11-03, PR #25):** Added {id}-style path substitution for scalar runtime inputs in http.get_json and Try Now form fields from JSON Schema. LLM registry does not expose parameterized URLs as zero-argument functions. Build and browser verification pending; mastery not assessed.

**HTTP URL-derived parameters (2026-11-03, PR #25):** HTTP GET JSON parameter names are generated from distinct URL {placeholders} on input/edit, with disabled fields and synchronized JSON schema; all are required. Browser/build verification pending.

**Parameter editor controls (2026-11-03, PR #25):** Hide Add Parameter and per-row Delete Parameter for every execution type; Required checkboxes are disabled/read-only while preserving stored required flags. Build/browser verification pending.

**GitHub parameter contract UI (2026-11-03, PR #25):** GitHub issue execution now presents a predefined, non-editable issue_number schema in Add/Edit Tool; add/remove parameter controls hidden and saved schema canonicalized. Build/browser verification pending.

**GitHub Try Now schema alignment (2026-11-03, PR #25):** Submit only issue_number at execution; owner/repo are persisted configuration, not runtime input. Executor requires configured repository. Build/browser verification pending.

**GitHub issue dynamic repository (2026-11-03, PR #25):** Removed fixed owner/repo from GitHub HTTP client and executor. Added configured owner/repo inputs to tool creation and runtime issue_number; Try Now reads persisted repository configuration. Legacy built-in takes owner/repo as inputs. Build/browser tests pending; mastery not assessed.

**HTTP tool diagnostics (2026-11-03, PR #25):** Added server-side HttpClient exception-chain logging for connection failures and timeouts (host only, no URL path/query) and distinct upstream_timeout response. No changes to SSRF safeguards, proxy policy or outbound connectivity; runtime root cause still needs logs. Build and integration verification pending, mastery not assessed.

**Dynamic URL form alignment (2026-11-03, PR #25):** Removed hardcoded fakestoreapi.com validation and helper copy from Tools Add/Edit UI. Browser validation accepts public-looking HTTPS URLs; server-side DNS/IP policy remains authoritative. JS/browser verification pending; mastery not assessed.

**Dynamic public HTTPS URLs (2026-11-03, PR #25):** Removed hardcoded fakestoreapi.com checks in Registry, Executor and HTTP client. Shared public HTTPS URL policy plus DNS-to-public-IP pinned socket connection (no redirects/proxies) protects against private-network SSRF; existing GET-only, 10s timeout and 1 MiB JSON limit remain. Changes committed, build/runtime tests pending; mastery not assessed. Next: run CI, create two tools against distinct public HTTPS APIs, and test blocked private targets.

**Tool registry naming (2026-11-03, PR #25):** Renamed Application/Tools/Registers to Application/Tools/Registry, moving GitHub/HTTP definitions, IToolRegistry, ToolRegistry, ToolResult and updating namespaces/DI/executor imports. Naming-only refactor; build and runtime verification pending, mastery not assessed. Next: run build and smoke tests.

**ToolResult location (2026-11-03, PR #25):** Moved ToolResult<T> and ToolError into Application/Tools/Registers; execution result imports shared ToolError. No behavior change. Build pending, mastery not assessed.

**Registers folder organization (2026-11-03, PR #25):** Moved GitHubTools/SchemaFactory, HttpApiTools/SchemaFactory, ToolRegistry and IToolRegistry under Application/Tools/Registers with provider subfolders; updated namespaces and DI/executor references. Organizational refactor only; build and runtime checks pending, mastery not assessed. Next: compile and verify tool catalog and execution.

**Tool details Razor compile fix (2026-11-03, PR #25):** Extended ToolsController.ToolCatalogItem with ExecutorKey and ConfigurationJson and mapped both from persisted Tool in WorkspaceController gallery/details projections. Addresses missing Razor model properties used by HTTP Try Now and edit context. Build and browser verification pending; mastery not assessed. Next: run build and verify details/edit and HTTP Try Now.

**Dynamic HTTP LLM registry (2026-11-03, PR #25):** Added IToolRegistry.GetToolsAsync to load active, approved configured http.get_json tools from persistence, validate endpoint and create LLM AIFunction via HttpApiToolSchemaFactory; retained GetTools built-in GitHub compatibility. ChatCommand currently only generates text and does not invoke the registry, so no LLM tool calling is claimed. Build and runtime tests pending; mastery not assessed. Next: build, migrate DB, test configured HTTP tools and separately wire LLM orchestration when in scope.

**HTTP API layered refactor (2026-11-03, PR #25):** Split configured HTTP GET execution into Core IHttpApiClient, Infrastructure HttpApiClient, Application HttpApiTools, HttpApiToolSchemaFactory, and thin HttpGetJsonExecutor; aligned with GitHub's client/tools/schema/executor pattern. Existing ToolRegistry remains GitHub-only for LLM exposure; HTTP execution registry resolves configured tools through executor key. Build, integration, migration, and E2E not verified; mastery not assessed. Next: verify build and run two configured product tools; separately decide how to expose persisted tools to LLM registry.

**Dynamic HTTP JSON tool UI (2026-11-03, PR #25):** Wired Create/Edit custom-tool dialog to executor key and approved Fake Store API URL, and added HTTP JSON Try Now panel. Backend and migration already on branch. Build/CI, DB migration, and browser/end-to-end verification pending; no mastery assessment. Next: verify CI, apply migration, create two configured product tools and execute both.

**Execution Gateway focus (2026-11-01, feature/unified-tool-platform):** Scoped PR #25 to gateway dispatch and compatibility with approved custom C# executors; explicitly rejected external tools in the custom adapter. MCP/OpenAPI, ToolSource, dynamic external registration and agent integration are deferred. Build, automated tests and authorization review pending. See `docs/architecture/unified-tool-platform.md`. Engineering in progress; mastery not assessed.

## 1. Current snapshot

| Field | Value |
| --- | --- |
| Current phase | Module 00 — Baseline & Roadmap Alignment (planned) |
| Current learning module | Module 03 preparation — Tool Engineering lab (MCP execution) |
| Current engineering milestone | DeepWiki import verified by the user. Draft PR #27 adds MCP execution and Try Now, including SSE response correlation and distinct failure messages. Local .NET build and protocol checks passed. A live call through the application still needs a successful retry. |
| Current mastery assessment | Not assessed |
| Next recommended action | Restart AgentForge on the latest branch and retry DeepWiki `read_wiki_structure` from Try Now. If it fails, use the response's MCP operation and reason to isolate the failure. |
| Last progress review | 2026-11-10 — the user reproduced a response-validation failure; added operation-specific diagnostics for the next retry. |

**Agents Gallery design update (2026-10-25):** Cards now reuse the Tools gallery's shared `ui-item-card`, `ui-icon-tile`, `ui-item-content`, and badge styles in a two-column grid on draft PR #19. Static preview only; build and browser verification pending. No mastery assessment evidence.

**Agent Details milestone (2026-10-25):** Read-only sample detail route (`/agents/{id}`), shared-style profile/overview/capabilities, gallery navigation, and unknown-ID 404 added on draft PR #19. Build and browser checks pending; configuration and runtime not implemented; mastery not assessed.

**Agent Overview UI (2026-10-26):** Implemented responsive read-only Overview dashboard with About, capabilities, sample use cases, agent metadata, tags, and explicit preview limitations on PR #19. Uses existing sample-agent data; model, tools, author, version, and execution metrics are intentionally not fabricated. Build/browser verification pending; mastery not assessed.

**Agent create submit wiring fix (2026-10-27):** Modal footer Create Agent button now calls async createAgent directly instead of relying on requestSubmit for an external form button; form submit reuses same handler. Code committed, browser/API verification pending; no tests added. Mastery not assessed.

**Agent default icon alignment (2026-10-27):** Gallery/details now render the same robot SVG path as the Agents sidebar when no uploaded image is present; new Agent.Icon defaults to empty. Existing legacy glyph values also render the robot fallback; no migration required. Build/browser verification pending.

**Agent icon upload (2026-10-27):** Added optional raster icon upload (PNG/JPEG/WebP/GIF, 2 MB limit, signature checks, antiforgery) to app-local wwwroot/uploads/agent-icons, storing generated filename in existing Agent.Icon string column. Gallery/details resolve filenames to local URLs; default glyph retained. No schema migration. Upload occurs before Agent create, so failed create can leave an orphan file; local filesystem requires persistent shared storage for multi-instance deployment. Build/browser/upload/database verification pending; no tests added. Mastery not assessed.

**Agent category filter source (2026-10-27):** Gallery filter options now come from AgentCategory enum supplied by AgentsController (not hardcoded in the view or inferred from current records). Client-side filtering remains current-page only; build/browser unverified. Mastery not assessed.

**Agent persisted gallery/details (2026-10-27):** Replaced hardcoded MVC sample agents with Mediator GetAgentsPageQuery and GetAgentByIdQuery. Active-only 20-item paged gallery, enum category choices, page links, persisted details and actual system instructions; existing client-side search/category filter applies to current page only. Build/browser/database checks pending; no tests added. Next: verify EF migration and end-to-end create/list/details; consider server-side search and category filtering. Mastery not assessed.

**Agent create modal integration (2026-10-27):** Removed Review step; Basic Information → Instructions now submits to POST /api/agents with CreateAgentCommand JSON and antiforgery token. Shows API errors, prevents duplicate submissions, and reloads on success. Gallery/details still use sample data; .NET/browser/database verification pending; no tests added by request. Mastery not assessed.

**Agent HTTP controller slice (2026-10-27):** Added separate `AgentsApiController` under `/api/agents` for create/update/deactivate/get-by-ID/paged list using Mediator application handlers. Active-only reads, input errors mapped to 400, antiforgery on mutations, and explicit response projection. Existing sample-data MVC pages remain unchanged. Build/HTTP/database verification pending; no tests added. Next: wire UI to persisted data and validate API security/authentication before deployment. Mastery not assessed.

**Agent application slice (2026-10-27):** Added Mediator CQRS commands for create, update, and deactivate and queries for by-ID and paginated Agent retrieval. Handlers use existing generic repository and unit of work; no UI/API integration. Commits are unverified by build or database execution; no tests added by request. Mastery not assessed.

**Agent domain/persistence slice (2026-10-27):** On `feature/agent-domain-persistence`, added global reusable Agent aggregate, string-persisted AgentCategory enum, EF Core mapping, additive PostgreSQL migration/snapshot without test-project files (removed per scope request). This is committed Engineering progress, not runtime verification: .NET SDK/PostgreSQL unavailable here; tests and migration have not been executed. No Agent UI/API/runtime wiring. Mastery not assessed.

## 2. Learning & engineering roadmap

These phases are **provisional** until reconciled with the project's training materials and learning roadmaps.

| Phase | Focus | Knowledge | Engineering | Mastery |
| --- | --- | --- | --- | --- |
| 00 | Baseline, curriculum alignment, repository architecture | In progress | UI and persistence foundations exist; Agent domain and PostgreSQL migration committed on feature branch, unverified | Not assessed |
| 01 | LLM foundations: tokens, attention, transformers, inference | Not started | Not started | Not assessed |
| 02 | LLM application engineering: messages, context, persistence, streaming | Not started | Not started | Not assessed |
| 03 | Agent runtime: tool calling, state, execution loops, guardrails | In progress — MCP call flow, response correlation, and read bounds explored | In progress — MCP execution and Try Now on draft PR #27; protocol checks passed, live application retry pending | Not assessed |
| 04 | Knowledge: embeddings, retrieval, RAG, memory | Not started | Not started | Not assessed |
| 05 | Evaluation and production AI: tracing, security, reliability, cost | Not started | Not started | Not assessed |
| 06 | AI-native SDLC: specs, coding agents, reviews, approvals, workflows | Not started | Not started | Not assessed |
| 07 | Advanced systems and capstone: orchestration, scale, independent design | Not started | Not started | Not assessed |

**Allowed progress values:** Not started / In progress / Completed / Blocked / Not assessed / Demonstrated (as appropriate). Keep **Knowledge**, **Engineering**, and **Mastery** separate.

## 3. Active module

### Current engineering lab — MCP execution (Module 03 preparation)

- **Knowledge:** Streamable HTTP can carry progress notifications before the JSON-RPC response. The response ID correlates it to the request.
- **Engineering:** CI build passed; local .NET build (Tailwind skipped) and all migrations on disposable PostgreSQL passed. A direct Cloudflare documentation search returned results. AgentForge import returned 502 in this environment, so a test-only external tool was seeded through Create. Execute rejected a missing `query` with `400 invalid_input`; a valid query reached the adapter and returned `400 mcp_execution_failed`. An isolated client call reported a socket error on direct connection to the same endpoint.
- **Try Now:** Uses editable JSON initialized from the existing schema example, the shared JSON viewer and the existing antiforgery-protected execution endpoint. Local .NET/Tailwind build and Chromium smoke checks passed for keyboard navigation, JSON validation, mobile light/dark visibility and existing Try Now variants. The real API rejected a missing required argument. Success and network failure used intercepted browser responses. The user confirmed DeepWiki import. Live Try Now remains to be verified.
- **Execution fix:** The user reported `mcp_execution_failed` from Try Now. The client now finds the matching JSON-RPC response in SSE without waiting for EOF, enforces a 1 MiB cap and 30-second overall deadline, and logs safe transport diagnostics. .NET fixtures covered notifications, wrong IDs, multiline events, open streams, size and cancellation. A live .NET protocol probe via the environment proxy returned DeepWiki wiki sections. The application API still encounters direct DNS failure in this environment, so the user-side cause and successful application retry are pending.
- **Response diagnosis:** The user reproduced `InvalidDataException` at `mcp.deepwiki.com`. Execution now reports whether `initialize` or `tools/call` failed and identifies the reason, including any numeric JSON-RPC error code. The upstream message and payload remain private. The exact user-side failure is pending a retry with the new diagnostics.
- **Mastery:** Not assessed.

### Module 00 — Agent domain and persistence

- **Module ID / title:** Module 00 — Agent domain, persistence, and application use cases
- **Problem and why it matters:** Global reusable Agent definitions need validated domain state and durable, versioned PostgreSQL persistence.
- **Prerequisites:** TBD
- **Learning objectives:** TBD
- **Theory and internals covered:** TBD
- **Minimal implementation / experiment:** TBD
- **Production design and trade-offs:** TBD
- **AgentForge files / architecture touched:** Core Agent/AgentCategory; Infrastructure AgentConfiguration, AddAgents migration and snapshot; Application Agent CQRS handlers; Web AgentsApiController, persisted Agents MVC gallery/details.
- **Tests and evaluations:** No AgentForge.Tests files in this PR; verification pending: .NET build, migration application, schema constraints and model drift.
- **Practice / debugging challenges:** TBD
- **Mastery criteria and evidence:** TBD
- **Open questions / blockers:** .NET SDK and PostgreSQL runtime not available in this execution environment; no verified migration execution. Next: build and verify Mediator handler registration and end-to-end create/list/details against PostgreSQL; implement server-side filtering if needed.

### Parallel lab — Tool Engineering (Module 03 preparation)

- **Knowledge:** Generated JSON Schema versus C# runtime validation; external `issue_number` versus internal `issueNumber`; schema-to-invocation mapping explored.
- **Engineering:** On PR #10, GitHub tool factory generates the public schema with `issue_number`, `minimum: 1`, and no `CancellationToken` property. User ran the SchemaVerification console lab and supplied output showing schema assertions, successful stubbed invocation with 42, and rejection of 0/-5 (ArgumentOutOfRangeException) and string input (ArgumentException); invalid arguments never reached the stub GitHub client.
- **Mastery:** Not assessed; successful lab execution is not independent mastery evidence.
- **Registry/UI follow-up:** On `feature/tool-execution-security-registry`, implemented an application-owned `IToolRegistry`/`ToolRegistry`, DI registration, and a read-only `/tools` MVC gallery with compact horizontal two-column cards, combined search and GitHub/Jira category dropdown (Jira currently empty), and a `/tools/{name}` details page showing function metadata, overview and input schema. The gallery design was updated on PR #12; Tools gallery/details Razor views now use Tailwind v4 utility classes and semantic theme colors, with CSS compilation documented in AgentForge.Web/TAILWIND.md. Tools gallery card spacing, typography, and hover states were refined using Tailwind utilities. The approved Tool Details dashboard (profile, schema-derived parameter table, copyable JSON and example input) and per-tool gallery display names/icons are now applied directly to feature/tool-execution-security-registry. Stable registry names and routes are unchanged; no execution controls were added. Shared shell/chat still use legacy CSS. Nested Tool Details uses a shared topbar breadcrumb (Tools / display name), with no redundant back link or section tabs; overview and parameters now include descriptive subtitles. Build/browser verification remains pending. Implementation is not yet build/browser verified; no execution authorization or LLM orchestration exists.
- **Next action:** Build and browser-test the registry gallery, Details tabs, and GitHub Try Now workflow, including success/404/invalid-input paths; add access controls and rate limiting before wider exposure. Full build/CI and live UI invocation remain unverified.

- **Pending verification:** `ToolResult<T>` and `ToolError` are now shared contracts directly under `Application/Tools`; GitHubTools returns `ToolResult<GitHubIssue>` with a safe `issue_not_found` error on HTTP 404. User-provided console output confirms the stubbed 404 and 500 checks passed; full build/CI remains unverified.


**Shared JSON viewer (2026-10-22):** On PR #12, extracted a reusable Razor partial and safe DOM-based syntax-highlighting renderer, now used for Tool Overview schema/example and live Try Now JSON response. Repository integration checked; browser and .NET build not verified. This is Engineering progress only; Mastery not assessed. 

**Tool Try Now (2026-10-22):** On PR #12, added a narrowly scoped, antiforgery-protected GitHub issue lookup endpoint and a two-column Try Now UI with fixed repository metadata, issue-number input, JSON/formatted/raw response tabs, copy, reset, execution status, and elapsed time. The UI is designed around the existing `get_github_issue` handler; build/browser/live GitHub verification is pending. This is engineering implementation, not mastery evidence.

**UI engineering (2026-10-22):** Replaced the page-specific `wwwroot/css/tools.css` with globally loaded `wwwroot/css/ui-components.css` containing reusable UI primitives (toolbar, form controls, cards, profile, badges, panels, tables, code blocks). Both Tools views now use shared classes. Tailwind utilities remain supported; CSS build automation and browser/build verification are still pending. This is engineering progress, not mastery evidence.

### Parallel UI engineering (2026-10-25)

- **Knowledge:** No new learning module assessment.
- **Engineering:** Draft PR #16 adds a Tailwind/.NET CI workflow, mobile sidebar close behavior, responsive Tools toolbar wrapping, and a shared Chat layout/sidebar migration. Initial workflow checks passed; checks for latest Chat commits and browser verification pending. This is the first slice, not the full UI redesign.
- **Mastery:** Not assessed.
- **Next action:** Verify PR #16 checks and responsive behavior; then implement design tokens, shared shell/Chat migration, and component consistency in separate reviewable PRs.

### Parallel Agent Studio UI (2026-10-25)

- **Knowledge:** No new mastery assessment.
- **Engineering:** Draft PR #19 adds `/agents` Razor gallery with nine static agents, search, category filtering, responsive cards, and empty state. No persistence, execution, or orchestration. Build and browser verification pending.
- **Mastery:** Not assessed.
- **Build investigation:** Local build log reported 25 missing Mediator / Microsoft.Extensions.AI type errors. Both packages are declared in Application csproj but absent from the compiler references. Added forced NuGet restore/build script and PR CI. Successful build not yet verified; Infrastructure also reports vulnerable cryptography dependencies.
- **Next action:** Run restore/build CI, then verify gallery browser behavior.

## 4. Decisions & unresolved gaps

| Date | Decision / gap | Rationale | Evidence / follow-up |
| --- | --- | --- | --- |
| 2026-11-10 | MCP execution diagnostics | SSE fixture verified the reader fix. Live protocol probe succeeded via the environment proxy, while the application API logged `NameResolutionError` on its direct connection in this environment. | Retry Try Now on the user's machine and use the new API message and application warning if it fails. |
| 2026-10-24 | PostgreSQL and single-user local-first boundary chosen | Versioned EF Core migrations in Infrastructure; identity/membership deferred | Feature branch; build/migration verification pending |
| 2026-10-22 | External tool contract uses `issue_number` and `minimum: 1` | Stable factory schema customization plus adapter mapping; user-confirmed schema/runtime stub checks | PR #10; no live LLM call yet |
| 2026-10-21 | Track learning, implementation, and mastery independently | Merged code alone does not establish understanding | This tracker |
| 2026-10-21 | Align training resources before finalizing the phase order | Prevent prerequisite gaps and disconnected feature work | Module 00 |
| 2026-10-21 | Project-centric Agent Studio is proposed; database and identity model remain open | Establish a project boundary before agents and orchestration | docs/architecture-foundation-assessment.md |

| 2026-10-27 | Agent definitions are global and reusable; category is an enum stored as string | Keep Project linkage, permissions and runtime outside MVP | Agent domain/persistence feature branch; build/migration/tests unverified |

## 5. Next-session handoff

**Read this section first in a new chat.** Continue draft PR #27 on `feature/mcp-tool-execution`. Update and restart AgentForge. Open the imported DeepWiki `read_wiki_structure` tool, enter `{"repoName":"dotnet/aspnetcore"}` in Try Now, and verify Success with returned wiki sections. If the call fails, collect the response and `McpToolExecutionAdapter` warning. Agent CRUD and mastery assessment are separate checks.

1. Read this file from the latest repository branch (prefer `main` for merged progress).
2. Verify current branches, PRs, and relevant code through GitHub; do not rely solely on this summary.
3. Identify the active module and distinguish proven outcomes from plans.
4. Resume from the first unresolved learning objective or engineering dependency.
5. At the end of the session, update the snapshot, roadmap status, active module, gaps, and handoff based on verified progress.

**Agent Gallery handoff:** Draft PR #19 on `feature/agents-static-gallery`; validate .NET build and browser behavior, then proceed to Agent Builder/Playground in a separate PR.

**Parallel Tool Engineering handoff:** Shared JSON viewer added to Overview and Try Now on PR #12; verify compilation and UI appearance in browser.  Schema verification, stubbed argument binding, invalid-input rejection, safe 404 result, and propagated 500 exception passed per user-provided console output. Next: verify Tool Registry and card/detail UI; then authorization and LLM orchestration; do not claim production integration or mastery.

**Next suggested session:** Verify the PostgreSQL foundation PR with .NET build, EF migration drift check, fresh PostgreSQL migration and persistence tests; then scope Project application services. Identity is single-user local-first; multi-user authorization is deferred.

## 6. Update rules for ChatGPT

- **When:** After a meaningful module, PR, assessment, or decision; not after every small message.
- **Where:** Edit `MASTERY_PROGRESS.md` in a focused feature branch or the active PR branch. Do not push to `main` without explicit permission.
- **How:** Read the latest file first, preserve relevant decisions, update only affected sections, cite actual PRs/commits/tests where useful, and keep entries concise.
- **Evidence:** Never mark a skill `Demonstrated` without an actual mastery check. Never claim a build or test was run unless verified.
- **Scope:** Distinguish **planned**, **implemented**, **verified**, and **mastered**.
- **Completion:** Summarize what changed and link the PR. If repository write access is unavailable, supply the exact proposed Markdown patch and state that it was not committed.
- **Continuity:** Keep this file as the progress ledger; use the project's Mastery Framework and Roadmaps as the detailed curriculum, not duplicated here.

**Agent creation modal (2026-10-26):** Draft PR #20 replaces the standalone create page with a responsive modal launched from the Agents gallery. Three steps: Basic Information, Instructions, Review. No duplicated step title, no live preview. Browser validation and review are client-side only; no backend persistence. Build/browser verification pending. Next: implement Agent domain, persistence, create/get API, wire submit, and add tests. Knowledge: implementation patterns under review; Engineering: UI committed; Mastery: not assessed.

**Design system planning (2026-10-26):** PR #20 now includes `docs/design-system.md` (DS-01–DS-06 plan) and root `AGENTS.md` (mandatory guidance for future coding agents). Documentation committed; shared controls and migrations not implemented or verified. Knowledge: documented; Engineering: planning; Mastery: not assessed. Next: audit CSS cascade, then implement shared button hover/variants and migrate Agent modal/gallery.

**Shared Razor component foundation (2026-10-26):** PR #20 now adds reusable partials under `AgentForge.Web/Views/Shared/Components/` (Button, Input, Select, Textarea, Field, Modal, Badge, Card), central CSS variants/hover states, and migrates Create Agent modal footer buttons. Documentation mandates this path for future agents. Engineering: initial code committed, build/browser not verified; Knowledge: component contract documented; Mastery: not assessed. Next: compile and browser-test, refine component APIs, migrate remaining Agent and Tools controls.

**Vertical-slice design-system decision (2026-10-26):** PR #20 docs now require typed ViewComponents with co-located C# models/implementations under `Features/Shared/Components/{Name}/` and Razor views under `Views/Shared/Components/{Name}/Default.cshtml`. Current flat partials remain transitional; code migration and build/browser verification pending. Engineering: documentation updated; Mastery: not assessed.

**Component location correction (2026-10-26):** PR #20 moves the typed Button ViewComponent, model, and Razor view into `AgentForge.Web/Components/Button/`, updates its Agent modal consumer, and removes the former Button slice under `Features/Shared/Components/`. Docs now require `AgentForge.Web/Components/{Name}/` with all three files together. Other flat partials remain transitional; no build/browser verification yet. Knowledge: convention documented; Engineering: Button moved; Mastery: not assessed.

**Final shared UI component location (2026-10-26):** PR #20 moved all eight initial shared Razor views out of `Views/Shared/Components/` into `AgentForge.Web/Views/Components/{Name}/Default.cshtml`, moved Button ViewComponent/model into the same Button folder, and updated Agent modal and documentation. Other seven views remain ViewData-based transitional partials; typed component conversion and build/browser verification pending. Knowledge: final path documented; Engineering: relocation committed; Mastery: not assessed.

**Authoritative relocation (2026-10-26):** PR #20 now places all eight initial shared component views in `AgentForge.Web/Components/{Name}/`, co-locates Button ViewComponent/model/view, updates Agent modal references, and deletes former `Views/Components/` files. `AGENTS.md` and `docs/design-system.md` require only `Web/Components/`. Seven views remain untyped; build/browser verification not performed. Knowledge: documented; Engineering: relocation committed; Mastery: not assessed. Next: typed conversions and verification.

**Design system showcase (2026-10-26):** PR #20 adds `/design-system` gallery (controller, Razor view, responsive gallery CSS) rendering eight existing shared component families, including interactive modal and button states. Shared modal gets a native close form. Engineering: code committed, no build/browser verification; Knowledge: gallery documented; Mastery: not assessed. Next: run build/browser checks, convert transitional components to typed slices, extend regression tests.

**Delete confirmation UI (2026-10-27):** PR #20 adds typed `Components/Delete/` with shared modal styling, safe Cancel/Delete ordering, and non-destructive `/design-system` preview. Knowledge: API documented. Engineering: committed; integration with real delete actions, build and browser verification pending. Mastery: not assessed. Next: verify and migrate existing confirmation consumers selectively.

**Shared dialog button composition (2026-10-27):** Modal and Delete Razor components now invoke the typed Button ViewComponent for Close, Cancel, and primary/destructive actions. Button model adds optional aria label and CSS class; shared CSS preserves compact close control. Engineering: committed on PR #20; build/browser verification pending. Knowledge: reuse contract established. Mastery: not assessed. Next: verify rendering, keyboard dismissal and real delete flows.

**Razor Tag Helper API (2026-10-27):** PR #20 adds `UiComponentTagHelper` for all nine UI families, registers it in view imports, and migrates Modal/Delete internal buttons plus `/design-system` gallery to tag syntax. Knowledge: API documented. Engineering: changes committed; app-wide call-site migration and build/browser checks pending. Mastery: not assessed. Next: compile/test tag helper bindings and migrate real application call sites incrementally.

**Component review (2026-10-27):** Inspected nine shared component view families and Tag Helper adapter. Fixed co-located Razor Tag Helper imports, migrated Field child controls to Tag Helpers, and avoided missing Modal description references. Engineering: commits on PR #20; not build/browser verified; remaining six transitional ViewData components still require typed-model migration and validation/accessibility regression coverage. Knowledge: identified component registration boundary. Mastery: not assessed. Next: run build and interactive regression checks before broad app call-site migration.

**App view migration started (2026-10-27):** Replaced compatible legacy button invocations/markup with `<ui-button>` in Agents Create dialog, Tools Add dialog, and Design System gallery on PR #20. Engineering: committed only; build/browser checks pending. Knowledge: broad migration is blocked by Tag Helper gaps for native input types, value/readonly/pattern, arbitrary data/ARIA attributes, select options and existing validation hooks. Mastery: not assessed. Next: extend shared component contracts, then migrate gallery filters, tool forms, agent skill forms and remaining shared dialogs in small verified batches.

**Further UI migration (2026-10-27):** PR #20 now uses `<ui-field>` for six Create Agent fields (preserving outer `.agent-field` wrappers for existing validation JS), and `<ui-input>`/`<ui-select>` for Agents and Tools gallery search/category filters. Added input type and autocomplete support to shared input adapter. Engineering: changes committed to feature branch, no build/browser verification; not app-wide complete. Knowledge: remaining Tool forms, Agent Skills/Tools, Tool Details, Chat and shared dialogs need contract extensions and migration. Mastery: not assessed. Next: verify Razor compilation, styling, filtering and validation, then migrate remaining views in reviewed batches.

**Tools Add/Edit dialog migration (2026-10-27):** Replaced legacy `_Dialog` partial in `Views/Tools/_AddToolDialog.cshtml` with shared `<ui-modal>` and shared Save/Cancel/Close buttons. Updated JS close selectors to stable IDs. Existing form controls and dynamic parameter rows still require component migration; build/browser regression checks pending. Knowledge: dialog body injection and close hooks reviewed. Engineering: committed on PR #20, unverified. Mastery: not assessed.

**Tool deletion confirmation (2026-10-27):** Tools Details now renders `<ui-delete>`; delete-tool.js opens the component dialog and invokes the existing DELETE API only on confirmation. Agent delete remains disabled/unimplemented. Engineering: changes committed to PR #20, build/browser tests pending. Mastery: not assessed.

**Legacy delete dialog cleanup (2026-10-27):** Removed `_ConfirmDialog.cshtml`, `confirm-dialog.js`, `confirm-dialog.css`, and their `_Layout.cshtml` references; shared `Components/Delete/` remains. Engineering: committed to PR #20; build/browser verification and full reference audit pending. Mastery: not assessed.

**PR #20 pre-merge review (2026-10-27):** Fixed shared Agent wizard error-node reuse and preserved hint `aria-describedby`; removed literal escaped newlines in Tools Details; prevented dismissing Tool Delete confirmation while DELETE is pending. CI build/JS syntax/Tailwind checks previously passed on earlier SHA; latest fixes require fresh CI. Browser interaction tests remain unverified. Agent creation remains UI-only with no persistence. Knowledge: reviewed component contracts; Engineering: fixes committed; Mastery: not assessed. Next: verify CI on final SHA and merge PR #20 if green, then implement backend persistence in a separate PR.

**Tool icon upload (2026-10-27):** Added Tool form icon upload/preview, validated upload endpoint, local URL acceptance, and icon rendering on catalog/profile. Committed to PR #22; build and browser verification pending. No mastery assessment.

**Shared file API (2026-10-28):** Consolidated Agent/Tool icon upload into FilesController and LocalFileStorageService; updated both forms, retained legacy Agent icon retrieval, and removed feature-specific icon controllers. Changes committed to PR #22. Build, deployment persistence, and browser upload/download verification pending; no mastery assessment.

**Core file storage contract (2026-10-28):** Moved IFileStorageService and StoredFile to AgentForge.Core/Services, using Stream and file metadata rather than ASP.NET IFormFile; updated Web implementation, controller, and DI references. Changes committed to PR #22; build and runtime verification pending. No mastery assessment.

**Application FileService placement (2026-10-28):** Moved concrete file storage implementation to AgentForge.Application/Services/FileService.cs, kept IFileStorageService in Core, and registered it via AddApplicationServices; removed Web implementation. Committed to PR #22. Build and browser verification pending; no mastery assessment.
