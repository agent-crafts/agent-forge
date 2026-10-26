# AgentForge — Mastery Progress Tracker

> **Purpose:** The durable, evidence-based handoff between training sessions, ChatGPT conversations, and AgentForge engineering work.
>
> **Goal:** AI-Native Engineering Mastery = Knowledge + Working Software + Demonstrated Mastery.
>
> **Update policy:** Update this file at the end of each meaningful learning module, implementation milestone, or mastery assessment. Do not mark a skill mastered based only on discussion or a merged PR.

## 1. Current snapshot

| Field | Value |
| --- | --- |
| Current phase | Module 00 — Baseline & Roadmap Alignment (planned) |
| Current learning module | Not started |
| Current engineering milestone | UI foundation merged; static Agents Gallery added on draft PR #19 (build/browser unverified); GitHub Tool Try Now UI, read-only endpoint, and shared JSON viewer integrated on PR #12 (build/browser unverified); PostgreSQL persistence foundation, generic/specialized repositories, unit of work, and initial Tool create/get/page application handlers added on feature branch (unverified); Tool update/deactivate and Project use cases pending |
| Current mastery assessment | Not assessed |
| Next recommended action | Confirm latest PR #16 Chromium CI passes; expand Tools Add/Edit/Delete and Try Now browser coverage with test data; review accessibility/visual QA. Separately verify persistence migrations and integration tests |
| Last progress review | 2026-10-25 (UI PR #16: component and Tools polish committed; Chromium smoke checks added; initial browser CI exposed launch-profile port mismatch, fixed in workflow; retest pending) |

**Agents Gallery design update (2026-10-25):** Cards now reuse the Tools gallery's shared `ui-item-card`, `ui-icon-tile`, `ui-item-content`, and badge styles in a two-column grid on draft PR #19. Static preview only; build and browser verification pending. No mastery assessment evidence.\n\n**Agent Details milestone (2026-10-25):** Read-only sample detail route (`/agents/{id}`), shared-style profile/overview/capabilities, gallery navigation, and unknown-ID 404 added on draft PR #19. Build and browser checks pending; configuration and runtime not implemented; mastery not assessed.

**Agent Overview UI (2026-10-26):** Implemented responsive read-only Overview dashboard with About, capabilities, sample use cases, agent metadata, tags, and explicit preview limitations on PR #19. Uses existing sample-agent data; model, tools, author, version, and execution metrics are intentionally not fabricated. Build/browser verification pending; mastery not assessed.

## 2. Learning & engineering roadmap

These phases are **provisional** until reconciled with the project's training materials and learning roadmaps.

| Phase | Focus | Knowledge | Engineering | Mastery |
| --- | --- | --- | --- | --- |
| 00 | Baseline, curriculum alignment, repository architecture | In progress | UI foundation exists; persistence and initial Tool application handlers committed on feature branch, unverified | Not assessed |
| 01 | LLM foundations: tokens, attention, transformers, inference | Not started | Not started | Not assessed |
| 02 | LLM application engineering: messages, context, persistence, streaming | Not started | Not started | Not assessed |
| 03 | Agent runtime: tool calling, state, execution loops, guardrails | Not started | Not started | Not assessed |
| 04 | Knowledge: embeddings, retrieval, RAG, memory | Not started | Not started | Not assessed |
| 05 | Evaluation and production AI: tracing, security, reliability, cost | Not started | Not started | Not assessed |
| 06 | AI-native SDLC: specs, coding agents, reviews, approvals, workflows | Not started | Not started | Not assessed |
| 07 | Advanced systems and capstone: orchestration, scale, independent design | Not started | Not started | Not assessed |

**Allowed progress values:** Not started / In progress / Completed / Blocked / Not assessed / Demonstrated (as appropriate). Keep **Knowledge**, **Engineering**, and **Mastery** separate.

## 3. Active module

- **Module ID / title:** Module 00 — PostgreSQL persistence foundation
- **Problem and why it matters:** Project metadata needs durable, versioned relational persistence.
- **Prerequisites:** TBD
- **Learning objectives:** TBD
- **Theory and internals covered:** TBD
- **Minimal implementation / experiment:** TBD
- **Production design and trade-offs:** TBD
- **AgentForge files / architecture touched:** Core Project entity; Infrastructure DbContext, mappings, migrations, DI; Web configuration; Docker Compose.
- **Tests and evaluations:** Pending: .NET build, migration application, schema constraints, model drift and integration tests.
- **Practice / debugging challenges:** TBD
- **Mastery criteria and evidence:** TBD
- **Open questions / blockers:** .NET SDK and PostgreSQL runtime not available in this execution environment; no verified migration execution.

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
- **Build investigation:** Local build log reported 25 missing Mediator / Microsoft.Extensions.AI type errors. Both packages are declared in Application csproj but absent from the compiler references. Added forced NuGet restore/build script and PR CI. Successful build not yet verified; Infrastructure also reports vulnerable cryptography dependencies.\n- **Next action:** Run restore/build CI, then verify gallery browser behavior.

## 4. Decisions & unresolved gaps

| Date | Decision / gap | Rationale | Evidence / follow-up |
| --- | --- | --- | --- |
| 2026-10-24 | PostgreSQL and single-user local-first boundary chosen | Versioned EF Core migrations in Infrastructure; identity/membership deferred | Feature branch; build/migration verification pending |\n| 2026-10-22 | External tool contract uses `issue_number` and `minimum: 1` | Stable factory schema customization plus adapter mapping; user-confirmed schema/runtime stub checks | PR #10; no live LLM call yet |
| 2026-10-21 | Track learning, implementation, and mastery independently | Merged code alone does not establish understanding | This tracker |
| 2026-10-21 | Align training resources before finalizing the phase order | Prevent prerequisite gaps and disconnected feature work | Module 00 |
| 2026-10-21 | Project-centric Agent Studio is proposed; database and identity model remain open | Establish a project boundary before agents and orchestration | docs/architecture-foundation-assessment.md |

## 5. Next-session handoff

**Read this section first in a new chat.**

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
