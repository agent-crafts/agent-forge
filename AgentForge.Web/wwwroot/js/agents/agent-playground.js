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
        'run.completed': ['Completed', 'Your response is ready.', '✓'],
        'tool.request': ['Calling tool', 'The agent requested a tool execution.', '↗'],
        'tool.response': ['Tool result', 'The tool returned an execution result.', '⚙']
    };
    const durationLabel = ms => ms >= 1000 ? (ms / 1000).toFixed(1) + 's' : ms + 'ms';
    function statusOf(event) {
        if (event.status === 'failed' || event.status === 'error') return 'failed';
        if (event.status === 'completed' || event.status === 'success') return 'success';
        return 'neutral';
    }
    function traceDescription(event, state, fallback) {
        const details = event.details || {};
        if (event.type === 'tool.request')
            return 'Executing ' + (details.toolName || 'the requested tool') + '.';
        if (event.type === 'tool.response') {
            if (state !== 'failed') return (details.toolName || 'Tool') + ' executed successfully.';
            const code = details.errorCode || 'unknown_error';
            const guidance = {
                invalid_configuration: 'Check the saved tool configuration and required MCP connection settings.',
                mcp_execution_failed: 'Check MCP endpoint connectivity and server logs.',
                mcp_response_too_large: 'The MCP response exceeded the configured size limit.',
                tool_result_too_large: 'The tool output exceeded the agent run limit.',
                upstream_timeout: 'The upstream service timed out. Check connectivity and retry.'
            };
            return 'Tool execution failed (' + code + '). ' + (guidance[code] || 'Inspect the tool configuration and server logs.');
        }
        if (state === 'failed') return 'This step failed. Review technical details for diagnostic information.';
        return fallback;
    }
    function traceSummary(events) {
        const completed = [...events].reverse().find(event => event.type === 'run.completed');
        const failures = events.filter(event => statusOf(event) === 'failed');
        const tools = events.filter(event => event.type === 'tool.request');
        const toolResults = events.filter(event => event.type === 'tool.response');
        const llmCalls = events.filter(event => event.type === 'llm.request');
        const outcome = completed
            ? statusOf(completed) === 'failed' ? 'Failed' : failures.length ? 'Completed with errors' : 'Completed'
            : failures.length ? 'Errors recorded' : 'Incomplete';
        return { completed, failures, tools, toolResults, llmCalls, outcome };
    }
    function renderTrace(events) {
        if (!Array.isArray(events) || !events.length) return;
        traceEmpty.hidden = true;
        traceList.hidden = false;
        const { completed, failures, tools, toolResults, llmCalls, outcome } = traceSummary(events);
        const group = document.createElement('section');
        group.className = 'af-activity-run';
        const header = document.createElement('div');
        header.className = 'af-activity-heading';
        const title = document.createElement('strong');
        title.textContent = 'Run #' + (traceList.children.length + 1);
        const badge = document.createElement('span');
        badge.className = 'af-activity-badge af-activity-badge--' + (completed && statusOf(completed) === 'failed' ? 'failed' : failures.length ? 'neutral' : completed ? 'success' : 'neutral');
        badge.textContent = outcome;
        header.append(title, badge);
        group.append(header);

        const metrics = document.createElement('div');
        metrics.className = 'af-trace-metrics';
        for (const [value, label] of [
            [typeof completed?.durationMs === 'number' ? durationLabel(completed.durationMs) : '—', 'Duration'],
            [String(llmCalls.length), 'AI calls'],
            [String(tools.length), 'Tool calls'],
            [String(toolResults.filter(e => statusOf(e) === 'failed').length), 'Tool errors']
        ]) {
            const metric = document.createElement('div');
            metric.className = 'af-trace-metric';
            const number = document.createElement('strong');
            number.textContent = value;
            const caption = document.createElement('span');
            caption.textContent = label;
            metric.append(number, caption);
            metrics.append(metric);
        }
        group.append(metrics);

        const controls = document.createElement('div');
        controls.className = 'af-trace-controls';
        const filter = document.createElement('select');
        filter.className = 'af-trace-filter';
        filter.setAttribute('aria-label', 'Filter execution events');
        for (const [value, label] of [['all', 'All events'], ['llm', 'AI calls'], ['tool', 'Tools'], ['failed', 'Errors']]) {
            const option = document.createElement('option');
            option.value = value;
            option.textContent = label;
            filter.append(option);
        }
        const copy = document.createElement('button');
        copy.type = 'button';
        copy.className = 'af-trace-copy';
        copy.textContent = 'Copy JSON';
        copy.addEventListener('click', async () => {
            try {
                await navigator.clipboard.writeText(JSON.stringify(events, null, 2));
                copy.textContent = 'Copied';
            } catch {
                copy.textContent = 'Copy unavailable';
            }
        });
        controls.append(filter, copy);
        group.append(controls);

        const workspace = document.createElement('div');
        workspace.className = 'af-trace-workspace';
        const timeline = document.createElement('ol');
        timeline.className = 'af-activity-timeline';
        timeline.setAttribute('aria-label', 'Execution events');
        const inspector = document.createElement('section');
        inspector.className = 'af-trace-inspector';
        inspector.setAttribute('aria-label', 'Selected event details');
        const rows = [];

        function selectEvent(index) {
            rows.forEach((row, i) => {
                row.button.setAttribute('aria-pressed', String(i === index));
                row.item.classList.toggle('af-trace-selected', i === index);
            });
            const event = events[index];
            inspector.replaceChildren();
            const heading = document.createElement('div');
            heading.className = 'af-trace-inspector-heading';
            const eventTitle = document.createElement('strong');
            eventTitle.textContent = (labels[event.type]?.[0] || event.type) + (event.details?.toolName ? ': ' + event.details.toolName : '');
            const status = document.createElement('span');
            status.className = 'af-activity-badge af-activity-badge--' + (statusOf(event) === 'failed' ? 'failed' : 'neutral');
            status.textContent = event.status || 'recorded';
            heading.append(eventTitle, status);
            inspector.append(heading);
            const summary = document.createElement('p');
            summary.textContent = traceDescription(event, statusOf(event), labels[event.type]?.[1] || 'Execution event');
            inspector.append(summary);
            const metadata = document.createElement('dl');
            metadata.className = 'af-trace-metadata';
            for (const [key, value] of [
                ['Type', event.type],
                ['Duration', typeof event.durationMs === 'number' ? durationLabel(event.durationMs) : null],
                ['Error code', event.details?.errorCode],
                ['Tool call ID', event.details?.toolCallId]
            ]) {
                if (value == null) continue;
                const dt = document.createElement('dt');
                dt.textContent = key;
                const dd = document.createElement('dd');
                dd.textContent = value;
                metadata.append(dt, dd);
            }
            inspector.append(metadata);
            const details = document.createElement('details');
            details.className = 'af-activity-details';
            details.open = true;
            const toggle = document.createElement('summary');
            toggle.textContent = 'Raw event JSON';
            const raw = document.createElement('pre');
            raw.textContent = JSON.stringify(event, null, 2);
            details.append(toggle, raw);
            inspector.append(details);
        }

        events.forEach((event, index) => {
            const state = statusOf(event);
            const item = document.createElement('li');
            item.className = 'af-activity-step';
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'af-trace-event';
            button.setAttribute('aria-pressed', 'false');
            const symbol = document.createElement('span');
            symbol.className = 'af-activity-icon af-activity-icon--' + (state === 'failed' ? 'failed' : event.type === 'run.completed' ? 'success' : 'neutral');
            symbol.textContent = state === 'failed' ? '!' : (labels[event.type]?.[2] || '•');
            symbol.setAttribute('aria-hidden', 'true');
            const content = document.createElement('span');
            content.className = 'af-trace-event-content';
            const name = document.createElement('strong');
            name.textContent = (labels[event.type]?.[0] || event.type) + (event.details?.toolName ? ': ' + event.details.toolName : '');
            const description = document.createElement('small');
            description.textContent = traceDescription(event, state, labels[event.type]?.[1] || 'Execution event');
            content.append(name, description);
            button.append(symbol, content);
            if (typeof event.durationMs === 'number') {
                const time = document.createElement('span');
                time.className = 'af-activity-time';
                time.textContent = durationLabel(event.durationMs);
                button.append(time);
            }
            button.addEventListener('click', () => selectEvent(index));
            item.append(button);
            timeline.append(item);
            rows.push({ item, button, event });
        });
        filter.addEventListener('change', () => {
            rows.forEach(({ item, event }) => {
                const visible = filter.value === 'all' ||
                    filter.value === 'failed' && statusOf(event) === 'failed' ||
                    filter.value === 'tool' && event.type.startsWith('tool.') ||
                    filter.value === 'llm' && event.type.startsWith('llm.');
                item.hidden = !visible;
            });
            const first = rows.find(row => !row.item.hidden);
            if (first) selectEvent(rows.indexOf(first));
            else inspector.replaceChildren();
        });
        workspace.append(timeline, inspector);
        group.append(workspace);
        traceList.prepend(group);
        selectEvent(Math.max(0, events.findIndex(event => statusOf(event) === 'failed')));
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
            if (currentGeneration === generation) window.AgentForgeConversation.setContent(placeholder, 'assistant', result.response);
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
