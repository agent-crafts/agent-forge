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
| Current engineering milestone | UI foundation and architecture assessment merged; static Projects list/profile UI committed on feature branch (not build-tested); Project domain foundation remains in draft PR #7 |
| Current mastery assessment | Not assessed |
| Next recommended action | Review static Projects UI PR; run build and browser checks; decide database and identity boundary before wiring persistence |
| Last progress review | 2026-10-21 (architecture assessment) |

## 2. Learning & engineering roadmap

These phases are **provisional** until reconciled with the project's training materials and learning roadmaps.

| Phase | Focus | Knowledge | Engineering | Mastery |
| --- | --- | --- | --- | --- |
| 00 | Baseline, curriculum alignment, repository architecture | In progress | UI foundation exists; architecture inspected, no implementation verified | Not assessed |
| 01 | LLM foundations: tokens, attention, transformers, inference | Not started | Not started | Not assessed |
| 02 | LLM application engineering: messages, context, persistence, streaming | Not started | Not started | Not assessed |
| 03 | Agent runtime: tool calling, state, execution loops, guardrails | Not started | Not started | Not assessed |
| 04 | Knowledge: embeddings, retrieval, RAG, memory | Not started | Not started | Not assessed |
| 05 | Evaluation and production AI: tracing, security, reliability, cost | Not started | Not started | Not assessed |
| 06 | AI-native SDLC: specs, coding agents, reviews, approvals, workflows | Not started | Not started | Not assessed |
| 07 | Advanced systems and capstone: orchestration, scale, independent design | Not started | Not started | Not assessed |

**Allowed progress values:** Not started / In progress / Completed / Blocked / Not assessed / Demonstrated (as appropriate). Keep **Knowledge**, **Engineering**, and **Mastery** separate.

## 3. Active module

- **Module ID / title:** TBD
- **Problem and why it matters:** TBD
- **Prerequisites:** TBD
- **Learning objectives:** TBD
- **Theory and internals covered:** TBD
- **Minimal implementation / experiment:** TBD
- **Production design and trade-offs:** TBD
- **AgentForge files / architecture touched:** TBD
- **Tests and evaluations:** TBD
- **Practice / debugging challenges:** TBD
- **Mastery criteria and evidence:** TBD
- **Open questions / blockers:** TBD

## 4. Decisions & unresolved gaps

| Date | Decision / gap | Rationale | Evidence / follow-up |
| --- | --- | --- | --- |
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

**Next suggested session:** Complete Module 00 curriculum/baseline review; use `docs/architecture-foundation-assessment.md` to decide database and identity boundary, then scope the first Project Foundation implementation PR. No build or tests have been run for the assessment.

## 6. Update rules for ChatGPT

- **When:** After a meaningful module, PR, assessment, or decision; not after every small message.
- **Where:** Edit `MASTERY_PROGRESS.md` in a focused feature branch or the active PR branch. Do not push to `main` without explicit permission.
- **How:** Read the latest file first, preserve relevant decisions, update only affected sections, cite actual PRs/commits/tests where useful, and keep entries concise.
- **Evidence:** Never mark a skill `Demonstrated` without an actual mastery check. Never claim a build or test was run unless verified.
- **Scope:** Distinguish **planned**, **implemented**, **verified**, and **mastered**.
- **Completion:** Summarize what changed and link the PR. If repository write access is unavailable, supply the exact proposed Markdown patch and state that it was not committed.
- **Continuity:** Keep this file as the progress ledger; use the project's Mastery Framework and Roadmaps as the detailed curriculum, not duplicated here.
