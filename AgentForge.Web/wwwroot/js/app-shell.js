(() => {
  const key = 'agentforge.workspacePreview';
  const profile = document.getElementById('workspaceProfileToggle');
  const menu = document.getElementById('workspaceProfileMenu');
  const adminNav = document.querySelector('.workspace-admin-nav');
  const userNav = document.querySelector('.workspace-user-nav');
  const adminFooter = document.querySelector('.workspace-admin-only');
  const brand = document.querySelector('[data-workspace-brand]');
  const mobileButton = document.getElementById('workspaceMenu');
  const sidebar = document.getElementById('workspaceSidebar');
  const chatToggle = document.getElementById('chatGroupToggle');
  const chatTabs = document.getElementById('chatTabs');
  const mobileQuery = window.matchMedia('(max-width: 640px)');

  // This is a UI-only preview. Never use this setting for authorization.
  function readMode() {
    try { return localStorage.getItem(key) === 'user' ? 'user' : 'admin'; }
    catch { return 'admin'; }
  }
  function setMode(mode) {
    const isUser = mode === 'user';
    adminNav.hidden = isUser;
    userNav.hidden = !isUser;
    if (adminFooter) adminFooter.hidden = isUser;
    if (brand) brand.textContent = isUser ? 'PROJECT WORKSPACE' : 'ADMINISTRATION';
    menu?.querySelectorAll('[data-workspace-select]').forEach(item => {
      item.setAttribute('aria-checked', String(item.dataset.workspaceSelect === mode));
    });
    try { localStorage.setItem(key, mode); } catch { /* Storage may be unavailable. */ }
  }
  function closeProfile(restoreFocus = false) {
    if (!menu || !profile) return;
    menu.hidden = true;
    profile.setAttribute('aria-expanded', 'false');
    if (restoreFocus) profile.focus();
  }
  function closeSidebar(restoreFocus = false) {
    if (!sidebar || !mobileButton) return;
    sidebar.classList.remove('open');
    mobileButton.setAttribute('aria-expanded', 'false');
    if (restoreFocus) mobileButton.focus();
  }
  if (adminNav && userNav) setMode(readMode());
  profile?.addEventListener('click', () => {
    menu.hidden = !menu.hidden;
    profile.setAttribute('aria-expanded', String(!menu.hidden));
    if (!menu.hidden) menu.querySelector('[aria-checked="true"]')?.focus();
  });
  menu?.querySelectorAll('[data-workspace-select]').forEach(item => {
    item.addEventListener('click', () => {
      setMode(item.dataset.workspaceSelect);
      closeProfile(true);
      // Existing pages are not project-scoped. Return to the workspace entry point.
      window.location.assign('/projects');
    });
  });
  document.addEventListener('pointerdown', event => {
    if (menu && !menu.hidden && !menu.contains(event.target) && !profile?.contains(event.target)) closeProfile();
    if (mobileQuery.matches && sidebar?.classList.contains('open') &&
        !sidebar.contains(event.target) && !mobileButton?.contains(event.target)) closeSidebar();
  });
  document.addEventListener('keydown', event => {
    if (event.key !== 'Escape') return;
    if (menu && !menu.hidden) closeProfile(true);
    else if (sidebar?.classList.contains('open')) closeSidebar(true);
  });
  chatToggle?.addEventListener('click', () => {
    const expanded = chatToggle.getAttribute('aria-expanded') !== 'true';
    chatToggle.setAttribute('aria-expanded', String(expanded));
    if (chatTabs) chatTabs.hidden = !expanded;
  });
  if (!document.getElementById('chatForm')) {
    document.getElementById('newChat')?.addEventListener('click', () => window.location.assign('/chat?new=1'));
  }
  mobileButton?.addEventListener('click', () => {
    if (!sidebar) return;
    mobileButton.setAttribute('aria-expanded', String(sidebar.classList.toggle('open')));
  });
  sidebar?.querySelectorAll('a[href]').forEach(link => {
    link.addEventListener('click', () => { if (mobileQuery.matches) closeSidebar(); });
  });
  mobileQuery.addEventListener('change', event => { if (!event.matches) closeSidebar(); });
})();