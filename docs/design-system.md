# AgentForge UI Design System — implementation plan

Status: **Approved direction; implementation pending**. This document is the shared contract for future UI work. Existing `ui-components.css` provides partial primitives but is not yet a complete design system.

## Problem and evidence
- `AgentForge.Web/wwwroot/css/ui-components.css` defines `.ui-button` default/focus/disabled but no canonical button hover/active variants.
- Razor pages mix `.ui-button` with Tailwind appearance utilities (`bg-*`, `border-*`, `text-*`) and page-specific CSS, causing specificity and hover conflicts.
- `.agent-field` and inline validation are Agent-specific rather than shared; duplicated rules increase drift.
- The shared layout loads `ui-components.css` before `app.css`, so later CSS can override shared controls. Audit the final cascade before changing load order.

## Ownership and conventions
1. **One visual owner per control.** Use shared `ui-*` component classes for colors, backgrounds, borders, radii, shadows, typography, state transitions and focus indicators. Use Tailwind only for placement/layout (grid, flex, gap, margin, width, responsive).
2. **No local button hover patches.** Fix `ui-button` or its named variant centrally; avoid selectors tied to page IDs.
3. **Semantic HTML first.** `button` for actions, `a` for navigation; correct `type`, disabled state, keyboard and focus behavior.
4. **Token-driven colors.** Reuse existing theme tokens and introduce semantic tokens only when needed (e.g., danger, danger-hover, control-focus-ring). Support both light and dark themes; no magic page-level hex values.
5. **Shared validation contract.** `ui-field` wraps label, control, hint and inline error. Invalid control uses `aria-invalid="true"`, error element ID referenced by `aria-describedby`; error is removed when valid. Required is indicated visually and semantically.
6. **Incremental migration.** Do not redesign or change API behavior as part of visual migration. New controls must use the design system. Existing exceptions must be tracked.
7. **Avoid duplicate definitions.** Before adding a selector, search existing tokens/components and update their source instead of appending another override.

## Component API (target)
| Component | Class contract | States |
| --- | --- | --- |
| Button | `ui-button ui-button--primary/secondary/outline/ghost/danger` | default, hover, active, focus-visible, disabled, loading |
| Input | `ui-input` | default, hover, focus-visible, invalid, disabled, readonly |
| Select | `ui-select` | same control states |
| Textarea | `ui-textarea` | same control states |
| Field | `ui-field`, `ui-field-label`, `ui-field-hint`, `ui-field-error` | optional, required, invalid |
| Dialog | `ui-dialog` with header/body/footer | open, keyboard close, focus restoration |
| Badge | `ui-badge` with semantic variant | neutral, success, warning, danger |
| Card | `ui-card` | static, interactive hover/focus, selected |

Example (target API, **not yet guaranteed implemented**):

```html
<div class="ui-field">
  <label class="ui-field-label" for="agent-name">Agent name <span aria-hidden="true">*</span></label>
  <input class="ui-input" id="agent-name" name="name" required aria-invalid="true" aria-describedby="agent-name-error">
  <span class="ui-field-error" id="agent-name-error">Agent name is required.</span>
</div>
<button type="submit" class="ui-button ui-button--primary">Create Agent</button>
<button type="button" class="ui-button ui-button--secondary">Cancel</button>
```

## Implementation sequence and acceptance criteria

### DS-01 — Audit and tokens
- Inventory shared CSS, `app.css`, Tailwind utilities, Razor markup, existing theme tokens and all button hover overrides.
- Document specificity/cascade conflicts; pick a canonical token source and preserve both themes.
- Add a component/state matrix for regression testing.
- **Done when:** every currently used button style and hover conflict has a documented owner and migration path.

### DS-02 — Buttons (first implementation PR)
- Implement the five button variants and consistent hover/active/focus-visible/disabled states in shared CSS.
- Ensure disabled never gets hover styling, hover maintains text contrast, and keyboard focus remains visible.
- Migrate **Agents gallery Add Agent** and **Create Agent modal** actions to named variants; remove competing appearance utilities.
- **Done when:** gallery and modal hover/focus/disabled states are visually checked at desktop/mobile, light/dark, mouse/keyboard; no page-level hover override remains for migrated buttons.

### DS-03 — Form controls and validation
- Centralize input/select/textarea, labels, hint, required and danger error styling.
- Migrate Create Agent steps and ensure errors appear on Next, all invalid fields are marked, first invalid gets focus, errors clear as corrected; test both steps.
- **Done when:** required validation uses the shared field contract and does not depend on Agent-specific danger CSS.

### DS-04 — Dialogs, badges, cards
- Define shared dialog shell and consistent badge/card variants; migrate Create Agent dialog and Agents gallery without altering behavior.
- **Done when:** keyboard close/focus return, small-screen overflow, card hover and theme contrast pass checks.

### DS-05 — Tools and remaining UI
- Migrate Tools gallery/details and Try Now controls in small reviewable PRs; track exceptions and remove obsolete CSS only after usage search.
- **Done when:** no duplicate page-specific control appearance remains for migrated views and no regressions in Tools interactions.

### DS-06 — Guardrails and regression checks
- Create a development-only UI component showcase with every variant/state and both themes.
- Add automated checks for forbidden per-page appearance overrides where feasible, plus Playwright smoke checks for hover, focus, invalid, disabled and modal interactions.
- Document component usage and update this contract as APIs evolve.
- **Done when:** CI exercises the key states and future agents can find examples and clear review criteria.

## Verification checklist for every UI PR
- Verify .NET/Tailwind build, browser smoke checks and screenshots if tooling permits; report missing verification explicitly.
- Test light/dark, desktop/mobile, hover, keyboard focus, disabled and error states.
- Check contrast, labels, accessible names, semantic markup and reduced-motion considerations.
- Confirm no new duplicate selectors or conflicting Tailwind visual utilities on migrated controls.
- Update this document when introducing a new control/variant; update `MASTERY_PROGRESS.md` at milestones, separating Knowledge / Engineering / Mastery.
- Work on feature branches and PRs; never push to `main` directly.

## Dependency and PR boundaries
DS-01 and DS-02 should precede broad Agent/Tools UI work. Backend Agent persistence is independent and should remain a separate PR. Prefer one focused PR per milestone; link follow-up PRs here as completed. Do not claim these milestones implemented until code and verification evidence exist.

## Vertical-slice component architecture (target)

The canonical implementation is a **typed ASP.NET Core ViewComponent**, with its C# code and ViewModel co-located by component slice. Razor views retain the ASP.NET Core discovery path:

```text
AgentForge.Web/
  Features/
    Shared/
      Components/
        Button/
          ButtonViewComponent.cs
          ButtonViewModel.cs
        Input/
          InputViewComponent.cs
          InputViewModel.cs
        Select/
        Textarea/
        FormField/
        Modal/
        Badge/
        Card/
    Agents/
      CreateAgent/
        ...feature-specific behavior...
  Views/
    Shared/
      Components/
        Button/Default.cshtml
        Input/Default.cshtml
        Select/Default.cshtml
        Textarea/Default.cshtml
        FormField/Default.cshtml
        Modal/Default.cshtml
        Badge/Default.cshtml
        Card/Default.cshtml
```

Invoke via `@await Component.InvokeAsync("Button", new { model = new ButtonViewModel { Text = "Create Agent", Variant = "Primary" } })` (import the model namespace through `_ViewImports.cshtml` as appropriate). A component's model specifies options; its view renders markup; shared CSS owns hover/focus/invalid states. Do not place all models in one global folder. The current flat `Views/Shared/Components/_Button.cshtml`-style partials are **transitional**, not the intended architecture. Migration must replace usages and delete obsolete partials only after checking references. Feature-specific components stay inside their feature slice.
