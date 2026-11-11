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

    const summaryPanel = panel.querySelector('[data-playground-summary]');
    const formatTokens = value => typeof value === 'number' ? value.toLocaleString() : 'N/A';
    function renderTrace(events) {
        if (!Array.isArray(events) || !events.length) return;
        traceEmpty.hidden = true;
        traceList.hidden = false;
        summaryPanel.hidden = false;
        const run = events.find(e => e.type === 'run.completed');
        const response = events.find(e => e.type === 'llm.response');
        const usage = response?.details?.usage;
        const duration = run?.durationMs;
        const group = document.createElement('section');
        group.className = 'af-trace-run';
        const heading = document.createElement('div');
        heading.className = 'af-trace-run__heading';
        heading.textContent = 'Run ' + (traceList.children.length + 1) + ' · ' + (run?.status || 'unknown');
        group.append(heading);
        const metrics = [
            ['Duration', typeof duration === 'number' ? duration + ' ms' : 'N/A'],
            ['Tokens', formatTokens(usage?.totalTokens)],
            ['Cost', 'N/A']
        ];
        summaryPanel.replaceChildren();
        for (const [label, value] of metrics) {
            const cell = document.createElement('div');
            const metric = document.createElement('strong');
            metric.textContent = value;
            const caption = document.createElement('span');
            caption.textContent = label;
            cell.append(metric, caption);
            summaryPanel.append(cell);
        }
        for (const event of events) {
            const item = document.createElement('details');
            item.className = 'af-trace-event';
            const title = document.createElement('summary');
            const name = document.createElement('span');
            name.textContent = traceTitles[event.type] || event.type || 'Event';
            const status = document.createElement('span');
            status.className = 'af-trace-status af-trace-status--' + (event.status === 'failed' ? 'failed' : 'success');
            status.textContent = event.status === 'failed' ? 'Failed' : 'Done';
            const durationText = document.createElement('span');
            durationText.className = 'af-trace-duration';
            durationText.textContent = typeof event.durationMs === 'number' && event.durationMs > 0
                ? event.durationMs + ' ms' : '';
            title.append(name, status, durationText);
            item.append(title);
            const tabs = document.createElement('div');
            tabs.className = 'af-inspector-tabs';
            const content = document.createElement('pre');
            const details = event.details || {};
            const views = {
                Request: event.type === 'llm.request' ? details.arguments || details : { message: 'Not applicable to this event' },
                Response: event.type === 'llm.response' ? {
                    model: details.model, providerRequestId: details.providerRequestId,
                    finishReason: details.finishReason, toolCallCount: details.toolCallCount
                } : details,
                Usage: event.type === 'llm.response' ? {
                    ...details.usage, costUsd: details.costUsd, costStatus: details.costStatus
                } : { message: 'No usage reported for this event' },
                Timing: { timestamp: event.timestamp, durationMs: event.durationMs }
            };
            function choose(tab) {
                content.textContent = JSON.stringify(views[tab], null, 2);
                for (const button of tabs.children)
                    button.setAttribute('aria-pressed', String(button.textContent === tab));
            }
            for (const tab of Object.keys(views)) {
                const button = document.createElement('button');
                button.type = 'button';
                button.textContent = tab;
                button.addEventListener('click', () => choose(tab));
                tabs.append(button);
            }
            choose(event.type === 'llm.response' ? 'Usage' : 'Request');
            item.append(tabs, content);
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
