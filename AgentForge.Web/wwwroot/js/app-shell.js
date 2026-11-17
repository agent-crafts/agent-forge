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
  const projectSelect = document.getElementById('workspaceProjectSelect');
  const projectOverview = document.getElementById('workspaceProjectOverview');
  const projectFeedback = document.getElementById('workspaceProjectFeedback');
  const projectKey = 'agentforge.activeProjectPreview';
  const projectToggle = document.getElementById('workspaceProjectToggle');
  const projectOptions = document.getElementById('workspaceProjectOptions');
  const projectName = document.getElementById('workspaceProjectName');
  function closeProjectOptions() {
    if (projectOptions) projectOptions.hidden = true;
    projectToggle?.setAttribute('aria-expanded', 'false');
  }
  function syncProjectPicker() {
    if (!projectSelect || !projectToggle || !projectOptions) return;
    projectToggle.disabled = projectSelect.disabled;
    projectName.textContent = projectSelect.selectedOptions[0]?.textContent || 'Select project';
    projectOptions.replaceChildren();
    for (const option of projectSelect.options) {
      if (!option.value) continue;
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'workspace-project-option ui-dropdown-option';
      button.setAttribute('role', 'option');
      button.setAttribute('aria-selected', String(option.value === projectSelect.value));
      button.textContent = option.textContent;
      button.addEventListener('click', () => {
        projectSelect.value = option.value;
        closeProjectOptions();
        projectSelect.dispatchEvent(new Event('change', { bubbles: true }));
      });
      projectOptions.append(button);
    }
  }
  projectToggle?.addEventListener('click', () => {
    if (!projectOptions || projectToggle.disabled) return;
    const open = projectOptions.hidden;
    projectOptions.hidden = !open;
    projectToggle.setAttribute('aria-expanded', String(open));
    if (open) projectOptions.querySelector('[aria-selected="true"]')?.focus();
  });
  projectOptions?.addEventListener('keydown', event => {
    const options = Array.from(projectOptions.querySelectorAll('[role="option"]'));
    const index = options.indexOf(document.activeElement);
    if (event.key === 'Escape') { closeProjectOptions(); projectToggle.focus(); }
    else if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault();
      options[(index + (event.key === 'ArrowDown' ? 1 : -1) + options.length) % options.length]?.focus();
    }
  });
  document.addEventListener('pointerdown', event => {
    if (!event.target.closest('.workspace-project-picker')) closeProjectOptions();
  });
  const projectDetailsPattern = /^\/projects\/([0-9a-f-]{36})(?:\/(?:board|specifications|assistant))?\/?$/i;
  const onProjectGrid = window.location.pathname.replace(/\/$/, '') === '/projects';

  function selectedProjectId() {
    return projectDetailsPattern.exec(window.location.pathname)?.[1] || '';
  }
  function updateProjectOverview() {
    const id = projectSelect?.value;
    if (projectOverview) {
      projectOverview.href = id ? '/projects/' + encodeURIComponent(id) : '/projects';
      projectOverview.setAttribute('aria-disabled', String(!id));
    }
    userNav?.querySelectorAll('[data-project-section]').forEach(link => {
      link.href = id ? '/projects/' + encodeURIComponent(id) + '/' + link.dataset.projectSection : '#';
      link.setAttribute('aria-disabled', String(!id));
    });
  }
  function showProjectLandingMessage(message) {
    const landing = document.getElementById('userProjectLanding');
    if (landing) {
      landing.textContent = message;
      landing.hidden = false;
    }
  }
  async function loadProjectSwitcher() {
    if (!projectSelect) return;
    projectSelect.disabled = true;
    try {
      // Project membership is not implemented yet: this is a catalog preview, not an authorized "My Projects" list.
      const projects = [];
      let pageNumber = 1;
      let totalPages = 1;
      do {
        const response = await fetch('/api/projects?pageNumber=' + pageNumber + '&pageSize=100', { headers: { Accept: 'application/json' } });
        if (!response.ok) throw new Error('Unable to load projects (' + response.status + ').');
        const page = await response.json();
        const items = page.items ?? page.Items;
        if (!Array.isArray(items)) throw new Error('Unexpected projects response.');
        projects.push(...items);
        totalPages = Number(page.totalPages ?? page.TotalPages ?? 1);
        pageNumber++;
      } while (pageNumber <= totalPages && pageNumber <= 100);
      projectSelect.replaceChildren();
      if (!projects.length) {
        projectSelect.add(new Option('No projects available', ''));
        showProjectLandingMessage('No projects available yet.');
        updateProjectOverview();
        return;
      }
      for (const project of projects) {
        const id = String(project.id ?? project.Id ?? '');
        const name = String(project.name ?? project.Name ?? '');
        if (/^[0-9a-f-]{36}$/i.test(id) && name) projectSelect.add(new Option(name, id));
      }
      let saved = '';
      try { saved = localStorage.getItem(projectKey) || ''; } catch { /* unavailable */ }
      const current = selectedProjectId();
      const initial = [current, saved].find(id => Array.from(projectSelect.options).some(o => o.value === id));
      if (initial) projectSelect.value = initial;
      updateProjectOverview();
      if (readMode() === 'user' && onProjectGrid && projectSelect.value) {
        window.location.replace('/projects/' + encodeURIComponent(projectSelect.value));
      }
    } catch (error) {
      projectSelect.replaceChildren(new Option('Projects unavailable', ''));
      if (projectFeedback) {
        projectFeedback.hidden = false;
        projectFeedback.textContent = error.message;
      }
      showProjectLandingMessage('Could not load projects. Please refresh to try again.');
      updateProjectOverview();
    } finally {
      projectSelect.disabled = !Array.from(projectSelect.options).some(option => option.value);
      syncProjectPicker();
    }
  }
  projectSelect?.addEventListener('change', () => {
    const id = projectSelect.value;
    if (!id) return;
    try { localStorage.setItem(projectKey, id); } catch { /* unavailable */ }
    updateProjectOverview();
    const section = /^\/projects\/[0-9a-f-]{36}\/(board|specifications|assistant)\/?$/i.exec(window.location.pathname)?.[1];
    window.location.assign('/projects/' + encodeURIComponent(id) + (section ? '/' + section : ''));
  });
  void loadProjectSwitcher();

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
    closeProjectOptions();
    document.documentElement.dataset.workspaceMode = isUser ? 'user' : 'admin';
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