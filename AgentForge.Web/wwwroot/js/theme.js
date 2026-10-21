(() => {
  const key = 'agentforge.theme';
  const root = document.documentElement;
  const media = window.matchMedia('(prefers-color-scheme: dark)');

  function savedTheme() {
    try {
      const value = localStorage.getItem(key);
      return value === 'light' || value === 'dark' ? value : null;
    } catch {
      return null;
    }
  }

  function apply(theme) {
    root.dataset.theme = theme;
    root.style.colorScheme = theme;
    const meta = document.querySelector('meta[name="theme-color"]');
    if (meta) meta.content = theme === 'dark' ? '#0d1117' : '#f8fafc';

    document.querySelectorAll('[data-theme-toggle]').forEach(button => {
      const next = theme === 'dark' ? 'light' : 'dark';
      button.setAttribute('aria-label', 'Switch to ' + next + ' theme');
      button.title = 'Switch to ' + next + ' theme';
    });
  }

  window.AgentForgeTheme = {
    set(theme) {
      if (theme !== 'light' && theme !== 'dark') return;
      try { localStorage.setItem(key, theme); } catch { /* Storage may be unavailable. */ }
      apply(theme);
    }
  };

  apply(savedTheme() ?? (media.matches ? 'dark' : 'light'));
  media.addEventListener('change', () => {
    if (!savedTheme()) apply(media.matches ? 'dark' : 'light');
  });
  document.addEventListener('DOMContentLoaded', () => {
    apply(root.dataset.theme);
    document.querySelectorAll('[data-theme-toggle]').forEach(button => {
      button.addEventListener('click', () => {
        window.AgentForgeTheme.set(root.dataset.theme === 'dark' ? 'light' : 'dark');
      });
    });
  });
})();
