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
        panel.querySelector('[data-playground-welcome]')?.remove();
        const empty = messages.querySelector('[data-playground-empty]');
        empty?.remove();
        return window.AgentForgeConversation.append(messages, role, content);
    }

    const traceList = panel.querySelector('[data-playground-trace]');
    const traceEmpty = panel.querySelector('[data-playground-trace-empty]');
    const traceTitles = {
        'run.started': 'Run started',
        'llm.request': 'LLM request',
        'llm.response': 'LLM response',
        'run.completed': 'Run completed'
    };

    function renderTrace(events) {
        if (!Array.isArray(events) || !events.length) return;
        traceEmpty.hidden = true;
        traceList.hidden = false;
        const group = document.createElement('section');
        group.className = 'af-trace-run';
        const heading = document.createElement('div');
        heading.className = 'af-trace-run__heading';
        heading.textContent = 'Execution ' + (traceList.children.length + 1);
        group.append(heading);
        for (const event of events) {
            const item = document.createElement('details');
            item.className = 'af-trace-event';
            const summary = document.createElement('summary');
            const name = document.createElement('span');
            name.textContent = traceTitles[event.type] || event.type || 'Event';
            const status = document.createElement('span');
            status.className = 'af-trace-status af-trace-status--' + (event.status === 'failed' ? 'failed' : 'success');
            status.textContent = event.status === 'failed' ? 'Failed' : 'Done';
            const duration = document.createElement('span');
            duration.className = 'af-trace-duration';
            duration.textContent = typeof event.durationMs === 'number' && event.durationMs > 0
                ? event.durationMs + ' ms' : '';
            summary.append(name, status, duration);
            const detail = document.createElement('pre');
            detail.textContent = JSON.stringify({
                timestamp: event.timestamp,
                durationMs: event.durationMs,
                ...event.details
            }, null, 2);
            item.append(summary, detail);
            group.append(item);
        }
        traceList.prepend(group);
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
        appendMessage('user', message);
        input.value = '';
        const placeholder = appendMessage('assistant', 'Thinking...');
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
            if (currentGeneration === generation) renderTrace(result.trace);
            if (!response.ok) throw new Error(result.error || 'LLM request failed (HTTP ' + response.status + ').');
            if (currentGeneration === generation) placeholder.textContent = result.response;
        } catch (error) {
            if (currentGeneration === generation) {
                placeholder.textContent = error.name === 'AbortError' ? 'Request cancelled.' : 'Unable to get a response.';
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
        traceList.replaceChildren();
        traceList.hidden = true;
        traceEmpty.hidden = false;
        const empty = document.createElement('div');
        empty.className = 'm-auto max-w-md text-center text-sm text-muted';
        empty.dataset.playgroundEmpty = '';
        empty.textContent = 'Start a conversation. Messages are not saved between requests.';
        messages.append(empty);
        input.focus();
    });
})();
