(() => {
  const form = document.getElementById('chatForm');
  const prompt = document.getElementById('prompt');
  const send = document.getElementById('sendButton');
  const messages = document.getElementById('messages');
  const welcome = document.getElementById('welcome');
  const conversation = document.getElementById('conversation');
  const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
  const storageKey = 'agentforge.demo.chats.v1';
  const demoChats = [
    { id: 'demo-architecture', title: 'Agent Architecture', messages: [{ role: 'user', content: 'How should I structure an AI agent?' }, { role: 'assistant', content: 'Start with a model, instructions, tools, and a bounded execution loop.' }] },
    { id: 'demo-workflow', title: 'ADLC Workflow Ideas', messages: [{ role: 'user', content: 'What is an ADLC workflow?' }, { role: 'assistant', content: 'It connects specifications, implementation, tests, and review into a repeatable process.' }] }
  ];
  function loadChats() {
    try {
      const saved = JSON.parse(localStorage.getItem(storageKey));
      if (Array.isArray(saved) && saved.every(c => typeof c.id === 'string' && Array.isArray(c.messages))) return saved;
    } catch (_) { /* reset invalid demo storage */ }
    return demoChats;
  }
  let chats = loadChats();
  let currentChatId = chats[0]?.id || null;
  const tabList = document.getElementById('chatTabList');
  const chatToggle = document.getElementById('chatGroupToggle');
  const chatTabs = document.getElementById('chatTabs');
  const deleteDialog = document.getElementById('deleteChatDialog');
  const deleteDescription = document.getElementById('deleteChatDescription');
  const confirmDelete = document.getElementById('confirmDeleteChat');
  const cancelDelete = document.getElementById('cancelDeleteChat');
  let pendingDeleteId = null;
  function saveChats() { try { localStorage.setItem(storageKey, JSON.stringify(chats)); } catch (_) { /* storage may be unavailable */ } }
  function currentChat() { return chats.find(c => c.id === currentChatId); }
  function renderTabs() {
    tabList.replaceChildren();
    for (const chat of chats) {
      const tab = document.createElement('button');
      tab.type = 'button';
      tab.className = 'chat-tab' + (chat.id === currentChatId ? ' active' : '');
      tab.textContent = chat.title;
      tab.title = chat.title;
      tab.setAttribute('aria-current', chat.id === currentChatId ? 'page' : 'false');
      tab.addEventListener('click', () => selectChat(chat.id));
      const row = document.createElement('div');
      row.className = 'chat-tab-row';
      const remove = document.createElement('button');
      remove.type = 'button';
      remove.className = 'chat-tab-delete';
      remove.textContent = '🗑';
      remove.title = 'Delete ' + chat.title;
      remove.setAttribute('aria-label', 'Delete ' + chat.title);
      remove.addEventListener('click', () => deleteChat(chat.id));
      row.append(tab, remove);
      tabList.append(row);
    }
  }
  function selectChat(id) {
    if (activeController) activeController.abort();
    currentChatId = id;
    messages.replaceChildren();
    const chat = currentChat();
    for (const message of chat?.messages || []) appendMessage(message.role, message.content);
    welcome.hidden = !!chat?.messages.length;
    renderTabs();
    document.querySelector('.sidebar').classList.remove('open');
  }
  function deleteChat(id) {
    const chat = chats.find(c => c.id === id);
    if (!chat) return;
    pendingDeleteId = id;
    deleteDescription.textContent = 'Delete "' + chat.title + '" permanently? This action cannot be undone.';
    deleteDialog.showModal();
  }
  function confirmDeleteChat() {
    const id = pendingDeleteId;
    pendingDeleteId = null;
    deleteDialog.close();
    if (!id || !chats.some(c => c.id === id)) return;
    const wasActive = id === currentChatId;
    if (wasActive && activeController) activeController.abort();
    chats = chats.filter(c => c.id !== id);
    saveChats();
    if (wasActive) selectChat(chats[0]?.id || null);
    else renderTabs();
  }
  cancelDelete?.addEventListener('click', () => deleteDialog.close());
  confirmDelete?.addEventListener('click', confirmDeleteChat);
  deleteDialog?.addEventListener('close', () => { pendingDeleteId = null; });
  function createChat() {
    const id = crypto.randomUUID ? crypto.randomUUID() : String(Date.now());
    chats.unshift({ id, title: 'New Chat', messages: [] });
    saveChats();
    selectChat(id);
    prompt.value = '';
    resize();
    prompt.focus();
  }
  chatToggle?.addEventListener('click', () => {
    const expanded = chatToggle.getAttribute('aria-expanded') !== 'true';
    chatToggle.setAttribute('aria-expanded', String(expanded));
    chatTabs.hidden = !expanded;
  });
  let busy = false;
  let activeController = null;
  function scrollDown() { conversation.scrollTop = conversation.scrollHeight; }
  function appendMessage(role, content) {
    const row = document.createElement('div');
    row.className = 'message ' + role;
    if (role === 'assistant') {
      const avatar = document.createElement('div');
      avatar.className = 'avatar';
      avatar.textContent = '✦';
      row.append(avatar);
    }
    const bubble = document.createElement('div');
    bubble.className = 'bubble';
    bubble.textContent = content;
    row.append(bubble);
    messages.append(row);
    welcome.hidden = true;
    scrollDown();
    return bubble;
  }
  function resize() { prompt.style.height = 'auto'; prompt.style.height = Math.min(prompt.scrollHeight, 180) + 'px'; }
  prompt.addEventListener('input', resize);
  prompt.addEventListener('keydown', e => {
    if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) {
      e.preventDefault();
      form.requestSubmit();
    }
  });
  form.addEventListener('submit', async e => {
    e.preventDefault();
    const text = prompt.value.trim();
    if (!text || busy) return;
    busy = true;
    send.disabled = true;
    prompt.value = '';
    resize();
    const chat = currentChat();
    if (chat) { chat.messages.push({ role: 'user', content: text }); if (chat.title === 'New Chat') chat.title = text.slice(0, 34); saveChats(); renderTabs(); }
    appendMessage('user', text);
    const reply = appendMessage('assistant', 'Thinking…');
    activeController = new AbortController();
    try {
      const response = await fetch('/chat/messages', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ message: text }),
        signal: activeController.signal
      });
      const data = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(data.error || 'Request failed (' + response.status + ').');
      reply.textContent = data.response || '(No text returned)';
      if (chat && chats.includes(chat)) { chat.messages.push({ role: 'assistant', content: reply.textContent }); saveChats(); }
    } catch (err) {
      reply.textContent = err.name === 'AbortError' ? 'Request cancelled.' : 'Error: ' + err.message;
      if (chat && chats.includes(chat) && err.name !== 'AbortError') { chat.messages.push({ role: 'assistant', content: reply.textContent }); saveChats(); }
    } finally {
      busy = false;
      activeController = null;
      send.disabled = false;
      prompt.focus();
      scrollDown();
    }
  });
  document.getElementById('newChat').addEventListener('click', createChat);
  saveChats();
  if (new URLSearchParams(location.search).has('new')) createChat(); else selectChat(currentChatId);
  document.getElementById('mobileMenu').addEventListener('click', () => document.querySelector('.sidebar').classList.toggle('open'));
  document.querySelectorAll('[data-prompt]').forEach(button => button.addEventListener('click', () => {
    prompt.value = button.dataset.prompt;
    resize();
    prompt.focus();
  }));
})();