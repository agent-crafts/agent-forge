(() => {
    const panel = document.querySelector('#agentPlaygroundPanel .agent-playground');
    if (!panel) return;

    const input = panel.querySelector('[data-playground-input]');
    const reset = panel.querySelector('[data-playground-reset]');
    const form = panel.querySelector('[data-playground-form]');

    // The runtime endpoint is not available yet. Do not simulate tool calls or agent responses.
    form.addEventListener('submit', event => event.preventDefault());
    reset.addEventListener('click', () => {
        input.value = '';
        input.focus();
        window.AgentForgeToast?.show({
            type: 'info',
            title: 'New session',
            message: 'Composer cleared. Agent execution is not connected yet.'
        });
    });
})();
