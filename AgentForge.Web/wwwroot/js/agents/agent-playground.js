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
    const summaryPanel = panel.querySelector('[data-playground-summary]');
    const labels = {
        'run.started': ['Agent started', 'Processing your request.', '▶'],
        'llm.request': ['Contacting AI', 'Sending the request to the AI model.', '↗'],
        'llm.response': ['AI responded', 'The AI model returned a response.', '✦'],
        'run.completed': ['Completed', 'Your response is ready.', '✓']
    };
    const durationLabel = ms => ms >= 1000 ? (ms / 1000).toFixed(1) + 's' : ms + 'ms';
    function statusOf(event) {
        if (event.status === 'failed' || event.status === 'error') return 'failed';
        if (event.status === 'completed' || event.status === 'success') return 'success';
        return 'neutral';
    }
    function renderTrace(events) {
        if (!Array.isArray(events) || !events.length) return;
        traceEmpty.hidden = true;
        traceList.hidden = false;
        const run = events.find(event => event.type === 'run.completed');
        const failed = events.some(event => statusOf(event) === 'failed');
        const outcome = failed ? 'Failed' : run && statusOf(run) === 'success' ? 'Completed' : 'Recorded';
        const runNumber = traceList.children.length + 1;
        const group = document.createElement('section');
        group.className = 'af-activity-run';
        const heading = document.createElement('div');
        heading.className = 'af-activity-heading';
        const title = document.createElement('strong');
        title.textContent = 'Run #' + runNumber;
        const badge = document.createElement('span');
        badge.className = 'af-activity-badge af-activity-badge--' + (failed ? 'failed' : run && statusOf(run) === 'success' ? 'success' : 'neutral');
        badge.textContent = outcome;
        heading.append(title, badge);
        group.append(heading);
        const timeline = document.createElement('ol');
        timeline.className = 'af-activity-timeline';
        for (const event of events) {
            const item = document.createElement('li');
            item.className = 'af-activity-step';
            const state = statusOf(event);
            const icon = document.createElement('span');
            icon.className = 'af-activity-icon af-activity-icon--' + (state === 'failed' ? 'failed' : event.type === 'run.completed' ? 'success' : 'neutral');
            icon.setAttribute('aria-hidden', 'true');
            const [label, description, symbol] = labels[event.type] || ['Execution step', 'The agent recorded an activity.', '•'];
            icon.textContent = state === 'failed' ? '!' : symbol;
            const body = document.createElement('div');
            body.className = 'af-activity-body';
            const line = document.createElement('div');
            line.className = 'af-activity-line';
            const name = document.createElement('strong');
            name.textContent = state === 'failed' && event.type === 'run.completed' ? 'Run failed' : label;
            line.append(name);
            if (typeof event.durationMs === 'number' && event.durationMs >= 0) {
                const time = document.createElement('span');
                time.className = 'af-activity-time';
                time.textContent = durationLabel(event.durationMs);
                line.append(time);
            }
            const detail = document.createElement('p');
            detail.textContent = state === 'failed' ? 'This step failed. Open details for more information.' : description;
            body.append(line, detail);
            const technical = document.createElement('details');
            technical.className = 'af-activity-details';
            const toggle = document.createElement('summary');
            toggle.textContent = 'Technical details';
            const raw = document.createElement('pre');
            raw.textContent = JSON.stringify(event, null, 2);
            technical.append(toggle, raw);
            body.append(technical);
            item.append(icon, body);
            timeline.append(item);
        }
        group.append(timeline);
        const footer = document.createElement('div');
        footer.className = 'af-activity-footer';
        const count = document.createElement('span');
        count.textContent = events.length + ' steps · ' + outcome.toLowerCase();
        footer.append(count);
        if (typeof run?.durationMs === 'number') {
            const time = document.createElement('span');
            time.textContent = durationLabel(run.durationMs) + ' total';
            footer.append(time);
        }
        group.append(footer);
        traceList.prepend(group);
        summaryPanel.hidden = true;
        summaryPanel.replaceChildren();
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
        summaryPanel.hidden = true;
        summaryPanel.replaceChildren();
        const empty = document.createElement('div');
        empty.className = 'm-auto max-w-md text-center text-sm text-muted';
        empty.dataset.playgroundEmpty = '';
        empty.textContent = 'Start a conversation. Messages are not saved between requests.';
        messages.append(empty);
        input.focus();
    });
})();
