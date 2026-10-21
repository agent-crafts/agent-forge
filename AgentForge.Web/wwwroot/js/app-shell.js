(() => {
  const button = document.getElementById('workspaceMenu');
  const sidebar = document.getElementById('workspaceSidebar');
  const chatToggle = document.getElementById('chatGroupToggle');
  const chatTabs = document.getElementById('chatTabs');
  chatToggle?.addEventListener('click', () => {
    const expanded = chatToggle.getAttribute('aria-expanded') !== 'true';
    chatToggle.setAttribute('aria-expanded', String(expanded));
    chatTabs.hidden = !expanded;
  });
  document.getElementById('newChat')?.addEventListener('click', () => window.location.assign('/chat?new=1'));
  button?.addEventListener('click', () => {
    const expanded = sidebar.classList.toggle('open');
    button.setAttribute('aria-expanded', String(expanded));
  });
})();
