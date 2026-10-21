# AgentForge UI Styling

The app uses Tailwind CSS v4 plus semantic CSS custom properties for Dark / Light / System themes.

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

The theme switcher in the chat header supports System, Light and Dark. `wwwroot/js/theme.js` persists the selection in localStorage and updates `data-theme` on the root HTML element. System follows `prefers-color-scheme` changes.

Shared semantic tokens live in `wwwroot/css/agent-chat.css`. Tailwind exposes `bg-app`, `bg-surface`, `text-content`, `border-line`, and `text-accent` through `@theme inline`. Prefer semantic colors over hardcoded color utilities in new views.
