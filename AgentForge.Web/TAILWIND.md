# Tailwind CSS in AgentForge.Web

Tailwind CSS v4 is the default for new and migrated Razor UI. Prefer utility classes and shared semantic colors instead of page-specific CSS. The existing shell/chat styles remain in place until their incremental migration.

## Build

Node.js and npm are required to compile the CSS.

```bash
cd AgentForge.Web
npm install
npm run build:css
dotnet run
```

During UI work, run `npm run watch:css` in a second terminal. For publishing, run `npm run build:css` before `dotnet publish`. The generated output is `wwwroot/css/app.css`, linked by `Views/Shared/_Layout.cshtml`. The checked-in `app.css` is a minimal placeholder, **not** a complete Tailwind build. Normal `dotnet build` and `dotnet run` now execute an MSBuild target that installs npm dependencies and compiles Tailwind before the web build. Use `npm run watch:css` for live CSS updates. The target can be skipped with `-p:SkipTailwindBuild=true` when CSS has already been compiled.

## Design conventions

- Use Tailwind utility classes in Razor views for layout, spacing, typography, responsive breakpoints, hover and focus states.
- Use semantic color utilities (`bg-surface`, `bg-app`, `text-heading`, `text-content`, `text-muted`, `border-line`, `text-accent`) defined in `Styles/app.css`.
- These colors follow the existing `data-theme` light/dark switch without duplicated dark-mode palettes.
- Tailwind Preflight is excluded while legacy MVC CSS is being migrated, to avoid unexpectedly resetting existing components.
- Keep JavaScript behavior separate from visual styling. Existing Tools search and category filtering are unchanged.
- Tool creation is not implemented; Add Tool stays disabled.

## Remaining work

Convert the shared Sidebar/Topbar/Chat and remaining views in separate small PRs, remove obsolete CSS when each view is migrated, and add a reliable CSS compilation step to CI/publish. No claim of browser or .NET build verification is made by this change.
