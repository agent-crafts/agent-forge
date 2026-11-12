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
    function tokenUsage(event) {
        const usage = event.details?.usage;
        if (!usage) return null;
        const read = key => {
            const value = usage[key] ?? usage[key.charAt(0).toUpperCase() + key.slice(1)];
            return typeof value === 'number' && Number.isFinite(value) ? value : null;
        };
        return {
            input: read('inputTokens'),
            output: read('outputTokens'),
            total: read('totalTokens'),
            cached: read('cachedInputTokens'),
            reasoning: read('reasoningTokens')
        };
    }
    function traceSummary(events) {
        const completed = [...events].reverse().find(event => event.type === 'run.completed');
        const failures = events.filter(event => statusOf(event) === 'failed');
        const tools = events.filter(event => event.type === 'tool.request');
        const toolResults = events.filter(event => event.type === 'tool.response');
        const llmCalls = events.filter(event => event.type === 'llm.request');
        const usage = events.filter(e => e.type === 'llm.response').map(tokenUsage).filter(Boolean);
        const sum = key => usage.some(u => u[key] !== null) ? usage.reduce((n, u) => n + (u[key] ?? 0), 0) : null;
        const tokens = { input: sum('input'), output: sum('output'), total: sum('total'), cached: sum('cached'), reasoning: sum('reasoning') };
        const outcome = completed
            ? statusOf(completed) === 'failed' ? 'Failed' : failures.length ? 'Completed with errors' : 'Completed'
            : failures.length ? 'Errors recorded' : 'Incomplete';
        return { completed, failures, tools, toolResults, llmCalls, outcome, tokens };
    }
    // A request is represented by one run; pair request/response events into business-readable steps.
    function buildSteps(events) {
        const steps = [];
        const pending = { llm: [], tool: [] };
        events.forEach((event, index) => {
            const kind = event.type.startsWith('llm.') ? 'llm' : event.type.startsWith('tool.') ? 'tool' : 'run';
            if (event.type === 'llm.request' || event.type === 'tool.request') {
                const step = { kind, request: event, response: null, index, title: kind === 'llm' ? 'AI inference' : (event.details?.toolName || 'Tool execution') };
                steps.push(step);
                pending[kind].push(step);
            } else if (event.type === 'llm.response' || event.type === 'tool.response') {
                const id = event.details?.toolCallId;
                let matchIndex = kind === 'tool' && id ? pending.tool.findIndex(step => step.request.details?.toolCallId === id) : -1;
                if (matchIndex < 0) matchIndex = 0;
                const step = pending[kind].splice(matchIndex, 1)[0];
                if (step) { step.response = event; if (event.details?.toolName) step.title = event.details.toolName; }
                else steps.push({ kind, request: null, response: event, index, title: kind === 'llm' ? 'AI inference' : (event.details?.toolName || 'Tool execution') });
            } else if (event.type !== 'run.started' && event.type !== 'run.completed') {
                steps.push({ kind: 'run', request: event, response: null, index, title: labels[event.type]?.[0] || event.type });
            }
        });
        let inference = 0;
        steps.forEach(step => { if (step.kind === 'llm') step.title = 'Inference #' + (++inference); });
        return steps;
    }
    function stepStatus(step) {
        return statusOf(step.response || step.request);
    }
    function stepDuration(step) {
        const value = step.response?.durationMs ?? step.request?.durationMs;
        return typeof value === 'number' ? value : null;
    }
    function stepSummary(step) {
        const event = step.response || step.request;
        if (step.kind === 'llm') return step.response ? 'The model returned an inference result.' : 'The model request was sent; no response event was recorded.';
        if (step.kind === 'tool') return traceDescription(event, stepStatus(step), 'Tool execution recorded.');
        return traceDescription(event, stepStatus(step), labels[event.type]?.[1] || 'Execution event.');
    }
    function addText(parent, tag, value, className) {
        const element = document.createElement(tag);
        if (className) element.className = className;
        element.textContent = value;
        parent.append(element);
        return element;
    }
    function renderTrace(events) {
        if (!Array.isArray(events) || !events.length) return;
        traceEmpty.hidden = true;
        traceList.hidden = false;
        const summary = traceSummary(events);
        const steps = buildSteps(events);
        const run = document.createElement('section');
        run.className = 'af-activity-run';
        const heading = document.createElement('div');
        heading.className = 'af-activity-heading';
        addText(heading, 'strong', 'Run #' + (traceList.children.length + 1));
        addText(heading, 'span', summary.outcome, 'af-activity-badge');
        run.append(heading);
        const metrics = document.createElement('div');
        metrics.className = 'af-trace-metrics';
        const duration = summary.completed?.durationMs;
        const totalStepTime = steps.reduce((n, s) => n + (stepDuration(s) || 0), 0);
        const metricValues = [
            ['Total time', typeof duration === 'number' ? durationLabel(duration) : 'N/A'],
            ['Step time sum', steps.some(s => stepDuration(s) !== null) ? durationLabel(totalStepTime) : 'N/A'],
            ['Steps', String(steps.length)],
            ['AI calls', String(summary.llmCalls.length)],
            ['Tool calls', String(summary.tools.length)],
            ['Errors', String(steps.filter(s => stepStatus(s) === 'failed').length)],
            ['Total tokens', summary.tokens.total == null ? 'N/A' : summary.tokens.total.toLocaleString()],
            ['Input tokens', summary.tokens.input == null ? 'N/A' : summary.tokens.input.toLocaleString()],
            ['Output tokens', summary.tokens.output == null ? 'N/A' : summary.tokens.output.toLocaleString()]
        ];
        metricValues.forEach(([label, value]) => {
            const cell = document.createElement('div');
            cell.className = 'af-trace-metric';
            addText(cell, 'strong', value);
            addText(cell, 'span', label);
            metrics.append(cell);
        });
        run.append(metrics);
        const controls = document.createElement('div');
        controls.className = 'af-trace-controls';
        const flow = addText(controls, 'button', 'View business flow', 'af-trace-copy');
        flow.type = 'button';
        const copy = addText(controls, 'button', 'Copy JSON', 'af-trace-copy');
        copy.type = 'button';
        copy.addEventListener('click', async () => {
            try { await navigator.clipboard.writeText(JSON.stringify(events, null, 2)); copy.textContent = 'Copied'; }
            catch { copy.textContent = 'Copy unavailable'; }
        });
        run.append(controls);
        const list = document.createElement('ol');
        list.className = 'af-activity-timeline af-trace-step-list';
        run.append(list);
        function openModal(title, contentBuilder) {
            const overlay = document.createElement('div');
            overlay.className = 'af-trace-modal-overlay';
            const dialog = document.createElement('section');
            dialog.className = 'af-trace-modal';
            dialog.setAttribute('role', 'dialog');
            dialog.setAttribute('aria-modal', 'true');
            const dialogTitle = 'af-trace-dialog-title-' + Math.random().toString(36).slice(2);
            dialog.setAttribute('aria-labelledby', dialogTitle);
            const top = document.createElement('div');
            top.className = 'af-trace-modal-header';
            const h = addText(top, 'h3', title);
            h.id = dialogTitle;
            const close = addText(top, 'button', 'Close ×', 'af-trace-copy');
            close.type = 'button';
            dialog.append(top);
            const body = document.createElement('div');
            body.className = 'af-trace-modal-body';
            contentBuilder(body);
            dialog.append(body);
            overlay.append(dialog);
            const previousFocus = document.activeElement;
            const dismiss = () => { document.removeEventListener('keydown', onKey); overlay.remove(); previousFocus?.focus(); };
            const onKey = e => {
                if (e.key === 'Escape') dismiss();
                if (e.key === 'Tab') {
                    const focusable = [...dialog.querySelectorAll('button,summary,[tabindex]')].filter(x => !x.hasAttribute('disabled'));
                    if (!focusable.length) return;
                    const first = focusable[0], last = focusable[focusable.length - 1];
                    if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
                    else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
                }
            };
            close.addEventListener('click', dismiss);
            overlay.addEventListener('click', e => { if (e.target === overlay) dismiss(); });
            document.addEventListener('keydown', onKey);
            document.body.append(overlay);
            close.focus();
        }
        function detailSection(parent, label, value) {
            const row = document.createElement('div');
            row.className = 'af-trace-detail-row';
            addText(row, 'strong', label);
            addText(row, 'span', value == null ? 'Not recorded' : String(value));
            parent.append(row);
        }
        function openStep(step) {
            openModal(step.title + ' · Step details', body => {
                addText(body, 'p', stepSummary(step));
                detailSection(body, 'Status', stepStatus(step));
                detailSection(body, 'Duration', stepDuration(step) === null ? 'N/A' : durationLabel(stepDuration(step)));
                if (step.kind === 'llm') {
                    const u = tokenUsage(step.response || {});
                    [['Input tokens',u?.input],['Output tokens',u?.output],['Total tokens',u?.total],['Cached input',u?.cached],['Reasoning',u?.reasoning]].forEach(([label,value]) => detailSection(body,label,value == null ? 'N/A' : value.toLocaleString()));
                } else detailSection(body, 'Tokens', 'N/A · tool execution');
                const details = step.response?.details || {};
                if (stepStatus(step) === 'failed') {
                    addText(body, 'h4', 'Technical error');
                    detailSection(body, 'Error code', details.errorCode || 'Not recorded');
                    detailSection(body, 'Technical message', details.errorMessage || details.message || 'Not recorded in trace');
                    addText(body, 'h4', 'Business impact');
                    addText(body, 'p', 'This step did not complete successfully. The effect on the user request depends on the agent response; a classified business error was not recorded.');
                }
                addText(body, 'h4', 'Input / Output');
                detailSection(body, 'Input', 'Full input payload is not recorded in the current trace.');
                detailSection(body, 'Output', 'Full output payload is not recorded in the current trace.');
                const raw = document.createElement('details');
                addText(raw, 'summary', 'Raw step events');
                addText(raw, 'pre', JSON.stringify({ request: step.request, response: step.response }, null, 2));
                body.append(raw);
            });
        }
        steps.forEach((step, index) => {
            const li = document.createElement('li');
            li.className = 'af-activity-step';
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'af-trace-event';
            const icon = addText(button, 'span', stepStatus(step) === 'failed' ? '!' : step.kind === 'llm' ? '✦' : '⚙', 'af-activity-icon');
            icon.setAttribute('aria-hidden', 'true');
            const content = document.createElement('span');
            content.className = 'af-trace-event-content';
            addText(content, 'strong', (index + 1) + '. ' + step.title);
            addText(content, 'small', stepSummary(step));
            const u = step.kind === 'llm' ? tokenUsage(step.response || {}) : null;
            addText(content, 'small', 'Time: ' + (stepDuration(step) === null ? 'N/A' : durationLabel(stepDuration(step))) + ' · Tokens: ' + (step.kind === 'llm' ? (u?.total == null ? 'N/A' : u.total.toLocaleString()) + ' (in ' + (u?.input ?? 'N/A') + ' / out ' + (u?.output ?? 'N/A') + ')' : 'N/A') + ' · ' + stepStatus(step), 'af-trace-token-inline');
            button.append(content);
            button.addEventListener('click', () => openStep(step));
            li.append(button);
            list.append(li);
        });
        flow.addEventListener('click', () => openModal('Request business flow', body => {
            addText(body, 'p', 'Observed execution sequence. Business decisions and branches are not captured by the current trace.');
            const diagram = document.createElement('div');
            diagram.className = 'af-trace-flow';
            const nodes = [{ title: 'Request received', summary: 'Agent run started', step: null },
                ...steps.map(s => ({ title: s.title, summary: stepSummary(s), step: s })),
                { title: summary.outcome, summary: 'Run outcome', step: null }];
            nodes.forEach((node, index) => {
                if (index) addText(diagram, 'div', '↓', 'af-trace-flow-arrow');
                const item = document.createElement('button');
                item.type = 'button';
                item.className = 'af-trace-flow-node';
                addText(item, 'strong', node.title);
                addText(item, 'small', node.summary);
                if (node.step) {
                    addText(item, 'small', 'Time: ' + (stepDuration(node.step) == null ? 'N/A' : durationLabel(stepDuration(node.step))) + ' · Status: ' + stepStatus(node.step));
                    item.addEventListener('click', () => {
                        document.querySelector('.af-trace-modal-overlay')?.querySelector('.af-trace-modal-header button')?.click();
                        openStep(node.step);
                    });
                } else item.disabled = true;
                diagram.append(item);
            });
            body.append(diagram);
        }));
        traceList.prepend(run);
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
