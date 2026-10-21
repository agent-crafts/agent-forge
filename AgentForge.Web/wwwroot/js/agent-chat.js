(() => {
  const form = document.getElementById('chatForm');
  const prompt = document.getElementById('prompt');
  const send = document.getElementById('sendButton');
  const messages = document.getElementById('messages');
  const welcome = document.getElementById('welcome');
  const conversation = document.getElementById('conversation');
  const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
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
    } catch (err) {
      reply.textContent = err.name === 'AbortError' ? 'Request cancelled.' : 'Error: ' + err.message;
    } finally {
      busy = false;
      activeController = null;
      send.disabled = false;
      prompt.focus();
      scrollDown();
    }
  });
  document.getElementById('newChat').addEventListener('click', () => {
    if (activeController) activeController.abort();
    messages.replaceChildren();
    welcome.hidden = false;
    prompt.value = '';
    resize();
    prompt.focus();
    document.querySelector('.sidebar').classList.remove('open');
  });
  document.getElementById('mobileMenu').addEventListener('click', () => document.querySelector('.sidebar').classList.toggle('open'));
  document.querySelectorAll('[data-prompt]').forEach(button => button.addEventListener('click', () => {
    prompt.value = button.dataset.prompt;
    resize();
    prompt.focus();
  }));
})();