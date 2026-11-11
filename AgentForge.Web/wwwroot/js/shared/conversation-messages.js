// Shared message presentation for global Chat and Agent Playground.
// Tool cards are available to the future runtime; no tool execution is simulated here.
window.AgentForgeConversation = (() => {
    const allowedRoles = new Set(['user', 'assistant', 'tool']);
    function setContent(body, role, content) {
        const value = content ?? '';
        if (role === 'assistant' && window.marked?.parse && window.DOMPurify?.sanitize) {
            body.innerHTML = window.DOMPurify.sanitize(window.marked.parse(value));
        } else {
            // Never render untrusted content as HTML without both libraries.
            body.textContent = value;
        }
    }
    function append(container, role, content, options = {}) {
        if (!allowedRoles.has(role)) throw new Error('Unsupported conversation role');
        const row = document.createElement('div');
        row.className = 'af-message af-message--' + role;
        const label = document.createElement('div');
        label.className = 'af-message__label';
        label.textContent = role === 'user' ? 'You' : role === 'assistant' ? 'Assistant' : 'Tool execution';
        if (role !== 'user') {
            const avatar = document.createElement('div');
            avatar.className = 'af-message__avatar';
            avatar.setAttribute('aria-hidden', 'true');
            avatar.textContent = role === 'assistant' ? '✦' : '⚙';
            row.append(avatar);
        }
        const card = document.createElement('div');
        card.className = 'af-message__card';
        const body = document.createElement('div');
        body.className = 'af-message__body';
        setContent(body, role, content);
        if (role === 'tool') {
            const status = ['running', 'success', 'failed'].includes(options.status) ? options.status : 'running';
            card.classList.add('af-message__card--' + status);
            label.textContent = options.name || 'Tool execution';
            const badge = document.createElement('span');
            badge.className = 'af-message__status';
            badge.textContent = status === 'running' ? 'Running' : status === 'success' ? 'Success' : 'Failed';
            label.append(badge);
        }
        card.append(label, body);
        row.append(card);
        container.append(row);
        row.scrollIntoView({ block: 'nearest' });
        return body;
    }
    return { append, setContent };
})();
