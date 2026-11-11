(() => {
    const panel = document.querySelector('#agentPlaygroundPanel .agent-playground');
    if (!panel) return;

    const input = panel.querySelector('[data-playground-input]');
    const reset = panel.querySelector('[data-playground-reset]');
    const form = panel.querySelector('[data-playground-form]');
    const send = panel.querySelector('[data-playground-send]');
    const messages = panel.querySelector('[data-playground-messages]');
    const welcome = panel.querySelector('[data-playground-welcome]');
    const agentId = panel.dataset.agentId;
    let pending = false;
    let generation = 0;
    let controller = null;

    function appendMessage(role, content) {
        welcome?.remove();
        const item = document.createElement('div');
        item.className = 'rounded-xl border border-line p-3 text-sm whitespace-pre-wrap break-words';
        const heading = document.createElement('div');
        heading.className = 'mb-1 text-xs font-semibold text-muted';
        heading.textContent = role;
        const body = document.createElement('div');
        body.textContent = content;
        item.append(heading, body);
        messages.append(item);
        item.scrollIntoView({ block: 'nearest' });
        return item;
    }

    form.addEventListener('submit', async event => {
        event.preventDefault();
        const message = input.value.trim();
        if (!message || pending) return;
        pending = true;
        send.disabled = true;
        input.disabled = true;
        const currentGeneration = generation;
        controller = new AbortController();
        appendMessage('You', message);
        input.value = '';
        const placeholder = appendMessage('Agent', 'Thinking...');
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        try {
            const response = await fetch('/api/agents/' + encodeURIComponent(agentId) + '/playground/chat', {
                method: 'POST',
                credentials: 'same-origin',
                signal: controller.signal,
                headers: {
                    'Content-Type': 'application/json',
                    ...(token ? { RequestVerificationToken: token } : {})
                },
                body: JSON.stringify({ message })
            });
            const result = await response.json().catch(() => ({}));
            if (!response.ok) throw new Error(result.error || 'LLM request failed (HTTP ' + response.status + ').');
            if (currentGeneration === generation) placeholder.lastChild.textContent = result.response;
        } catch (error) {
            if (currentGeneration === generation) {
                placeholder.lastChild.textContent = error.name === 'AbortError' ? 'Request cancelled.' : 'Unable to get a response.';
                if (error.name !== 'AbortError')
                    window.AgentForgeToast?.show({ type: 'error', title: 'Playground request failed', message: error.message });
            }
        } finally {
            if (currentGeneration === generation) {
                pending = false;
                controller = null;
                send.disabled = false;
                input.disabled = false;
                input.focus();
            }
        }
    });

    reset.addEventListener('click', () => {
        generation++;
        controller?.abort();
        controller = null;
        pending = false;
        send.disabled = false;
        input.disabled = false;
        input.value = '';
        messages.replaceChildren();
        const empty = document.createElement('div');
        empty.className = 'm-auto max-w-md text-center text-sm text-muted';
        empty.textContent = 'Start a conversation. Messages are not saved between requests.';
        messages.append(empty);
        input.focus();
    });
})();
