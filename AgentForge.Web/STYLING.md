# AgentForge UI Styling

The app uses Tailwind CSS v4 plus semantic CSS custom properties for Dark / Light themes (with an initial system-preference default).

## Build

From `AgentForge.Web`:

```sh
npm install
npm run build:css
# For ongoing UI work
npm run watch:css
```

The Tailwind CLI compiles `Styles/app.css` into `wwwroot/css/app.css`. The checked-in output is a minimal fallback; **build Tailwind after cloning** for full utility support. The existing chat component styles continue working independently through `agent-chat.css`.

## Themes

The header toggle switches between Light and Dark. `wwwroot/js/theme.js` persists an explicit choice in localStorage and updates `data-theme` on the root HTML element. When no choice is saved, the initial theme follows `prefers-color-scheme` changes. There is currently no separate System selection control.

Shared semantic tokens live in `wwwroot/css/agent-chat.css`. Tailwind exposes `bg-app`, `bg-surface`, `text-content`, `border-line`, and `text-accent` through `@theme inline`. Prefer semantic colors over hardcoded color utilities in new views.
