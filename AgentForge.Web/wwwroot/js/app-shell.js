(() => {
  const button = document.getElementById('workspaceMenu');
  const sidebar = document.getElementById('workspaceSidebar');
  const chatToggle = document.getElementById('chatGroupToggle');
  const chatTabs = document.getElementById('chatTabs');
  const mobileQuery = window.matchMedia('(max-width: 640px)');

  function closeSidebar({ restoreFocus = false } = {}) {
    if (!sidebar || !button) return;
    sidebar.classList.remove('open');
    button.setAttribute('aria-expanded', 'false');
    if (restoreFocus) button.focus();
  }

  chatToggle?.addEventListener('click', () => {
    const expanded = chatToggle.getAttribute('aria-expanded') !== 'true';
    chatToggle.setAttribute('aria-expanded', String(expanded));
    if (chatTabs) chatTabs.hidden = !expanded;
  });

  document.getElementById('newChat')?.addEventListener('click', () => window.location.assign('/chat?new=1'));

  button?.addEventListener('click', () => {
    if (!sidebar) return;
    const expanded = sidebar.classList.toggle('open');
    button.setAttribute('aria-expanded', String(expanded));
  });

  document.addEventListener('keydown', event => {
    if (event.key === 'Escape' && sidebar?.classList.contains('open')) {
      closeSidebar({ restoreFocus: true });
    }
  });

  document.addEventListener('pointerdown', event => {
    if (!mobileQuery.matches || !sidebar?.classList.contains('open')) return;
    if (!sidebar.contains(event.target) && !button?.contains(event.target)) closeSidebar();
  });

  sidebar?.querySelectorAll('a[href]')?.forEach(link => {
    link.addEventListener('click', () => {
      if (mobileQuery.matches) closeSidebar();
    });
  });

  mobileQuery.addEventListener('change', event => {
    if (!event.matches) closeSidebar();
  });
})();
