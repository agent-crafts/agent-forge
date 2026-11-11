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
        'run.started': 'Started working',
        'llm.request': 'Asked the AI model',
        'llm.response': 'Received an AI response',
        'run.completed': 'Finished'
    };

    function describeEvent(event) {
        const details = event.details || {};
        switch (event.type) {
            case 'run.started':
                return 'The agent started processing your message.';
            case 'llm.request':
                return 'The agent sent your request to the configured AI model.';
            case 'llm.response': {
                const model = typeof details.model === 'string' && details.model.trim()
                    ? details.model : 'the AI model';
                return 'Received a response from ' + model + '.';
            }
            case 'run.completed':
                return event.status === 'failed'
                    ? 'The agent could not finish this request.'
                    : event.status === 'completed' || event.status === 'success'
                        ? 'The agent finished processing your request.'
                        : 'The run ended. Check its status for more information.';
            default:
                return 'The agent recorded an execution step.';
        }
    }

    function eventStatus(event) {
        if (event.status === 'failed' || event.status === 'error') return ['Failed', 'failed'];
        if (event.status === 'completed' || event.status === 'success') return ['Completed', 'success'];
        if (event.status === 'started' || event.status === 'running' || event.status === 'pending')
            return ['In progress', 'pending'];
        return ['Recorded', 'pending'];
    }

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
            const item = document.createElement('div');
            item.className = 'af-trace-event';
            const title = document.createElement('div');
            title.className = 'flex flex-wrap items-center gap-2';
            const name = document.createElement('strong');
            name.textContent = traceTitles[event.type] || 'Execution step';
            const status = document.createElement('span');
            const [statusText, statusKind] = eventStatus(event);
            status.className = 'af-trace-status af-trace-status--' + statusKind;
            status.textContent = statusText;
            const durationText = document.createElement('span');
            durationText.className = 'af-trace-duration';
            durationText.textContent = typeof event.durationMs === 'number' && event.durationMs > 0
                ? event.durationMs + ' ms' : '';
            title.append(name, status, durationText);
            const description = document.createElement('p');
            description.className = 'mt-2 text-sm text-muted';
            description.textContent = describeEvent(event);
            item.append(title, description);

            const technical = document.createElement('details');
            technical.className = 'mt-2';
            const toggle = document.createElement('summary');
            toggle.className = 'cursor-pointer text-xs text-muted';
            toggle.textContent = 'Technical details';
            const content = document.createElement('pre');
            content.className = 'mt-2 overflow-x-auto text-xs';
            content.textContent = JSON.stringify(event, null, 2);
            technical.append(toggle, content);
            item.append(technical);
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
