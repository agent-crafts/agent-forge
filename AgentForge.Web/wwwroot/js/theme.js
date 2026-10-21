(() => {
  const key = 'agentforge.theme';
  const root = document.documentElement;
  const media = window.matchMedia('(prefers-color-scheme: dark)');
  const allowed = new Set(['light', 'dark', 'system']);
  function preference() {
    try { const saved = localStorage.getItem(key); return allowed.has(saved) ? saved : 'system'; }
    catch { return 'system'; }
  }
  function apply(mode) {
    const resolved = mode === 'system' ? (media.matches ? 'dark' : 'light') : mode;
    root.dataset.theme = resolved;
    root.style.colorScheme = resolved;
    const meta = document.querySelector('meta[name="theme-color"]');
    if (meta) meta.content = resolved === 'dark' ? '#0d1117' : '#f8fafc';
    document.querySelectorAll('[data-theme-option]').forEach(button => {
      const active = button.dataset.themeOption === mode;
      button.setAttribute('aria-pressed', String(active));
      button.classList.toggle('active', active);
    });
  }
  window.AgentForgeTheme = { set(mode) {
    if (!allowed.has(mode)) return;
    try { localStorage.setItem(key, mode); } catch { /* storage may be disabled */ }
    apply(mode);
  }};
  apply(preference());
  media.addEventListener('change', () => { if (preference() === 'system') apply('system'); });
  document.addEventListener('DOMContentLoaded', () => {
    apply(preference());
    document.querySelectorAll('[data-theme-option]').forEach(button => {
      button.addEventListener('click', () => window.AgentForgeTheme.set(button.dataset.themeOption));
    });
  });
})();
