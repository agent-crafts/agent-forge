# AgentForge — instructions for coding agents

Read `MASTERY_PROGRESS.md` before starting work. Verify current repository, branch and open PR state; do not infer status from past chats. Follow Understand → Inspect → Learn → Design → Implement → Verify → Review → PR → Mastery Check. Keep Knowledge, Engineering and demonstrated Mastery distinct. Never push directly to `main` without explicit authorization.

## Mandatory UI design system guidance

**Read [docs/design-system.md](docs/design-system.md) before any Razor, CSS, Tailwind, or UI JavaScript change.** It defines the canonical control contracts and staged migration plan.

- Search for existing `ui-*` controls and theme tokens before writing new styles. Reuse the shared design system instead of creating page-specific variants.
- Keep **visual states** (default, hover, active, focus, disabled, invalid) in shared component CSS. Tailwind utilities may control layout, not override control colors, borders, typography or state behavior.
- Buttons must use named variants once implemented. Fix hover issues in the shared button component, never by adding a one-off override.
- Forms must use shared field/validation conventions, with accessible labels, inline errors, `aria-invalid` and `aria-describedby`.
- Maintain light/dark and responsive behavior; verify mouse, keyboard and error flows. Preserve existing application behavior during migration.
- Until a component milestone is implemented, treat documented class contracts as **target APIs**, not proof that classes exist. Do not claim completed migration without code/tests.
- Include design-system compliance and regression evidence in UI PR descriptions. If a new control is necessary, update design-system documentation with its API, states and usage example.

For all engineering work: keep PRs small and reviewable, verify when tools permit, disclose what remains untested, and update `MASTERY_PROGRESS.md` after meaningful milestones. Backend persistence and UI design-system changes should be reviewed separately.

## Vertical-slice UI components (mandatory)

Shared reusable controls use ASP.NET Core **ViewComponents with strongly typed models**. Co-locate C# implementation and model per slice at `AgentForge.Web/Views/Components/{ComponentName}/` (e.g., `ButtonViewComponent.cs`, `ButtonViewModel.cs`). Put Razor views alongside C# files at `AgentForge.Web/Views/Components/{ComponentName}/Default.cshtml`. The ViewComponent uses an explicit absolute Razor view path.

Feature-specific UI logic stays in its feature slice (e.g., `Features/Agents/CreateAgent/`), not in Shared. Do not add a global `ViewComponents/` or `Models/Components/` dumping ground. Use typed models, not `ViewDataDictionary`, for component contracts. Central CSS in `wwwroot/css/ui-components.css` owns visual states. Migrate existing flat `_Button.cshtml` etc. partials incrementally; do not treat them as the target architecture. Follow `docs/design-system.md` and verify builds/browser behavior before claiming migration complete.

**Final folder rule:** Each component's `.cs`, ViewModel `.cs`, and `Default.cshtml` must be in the SAME `Views/Components/{Name}/` folder. No component Razor files under `Views/Shared/Components/` for new code. ViewComponents render their co-located view using `View("~/Views/Components/{Name}/Default.cshtml", model)`; no global Razor location override is needed. The Button slice is implemented as the first example; other flat partials still require migration.

**Canonical path (latest decision):** `AgentForge.Web/Views/Components/{Name}/` holds `{Name}ViewComponent.cs`, `{Name}ViewModel.cs`, and `Default.cshtml` together. Example: Button uses `AgentForge.Web/Views/Components/Button/`. Its `Invoke` calls `View("~/Views/Components/Button/Default.cshtml", model)`. Do not use `Views/Components/` or `Views/Shared/Components/` for new shared components. Existing flat partials are legacy and should be migrated separately.

**Latest user-approved location:** All reusable Razor UI component files belong under `AgentForge.Web/Views/Components/{ComponentName}/`. The old `AgentForge.Web/Views/Shared/Components/` directory must stay empty/nonexistent. Each new typed ViewComponent must co-locate `{Name}ViewComponent.cs`, `{Name}ViewModel.cs`, and `Default.cshtml` in that one directory. Use explicit `View("~/Views/Components/{Name}/Default.cshtml", model)` for nonstandard discovery. The existing Input, Select, Textarea, Field, Modal, Badge, Card views have been physically moved but are still transitional ViewData-based partials; converting them to strongly typed ViewComponents is a separate unfinished task. Do not imply full typed migration or test verification.
