(() => {
    const panel = document.querySelector('#agentPlaygroundPanel .agent-playground');
    if (!panel) return;

    const layout = panel.querySelector('[data-playground-layout]');
    const tab = document.getElementById('agentPlaygroundTab');
    const fitViewport = () => {
        if (panel.closest('[hidden]')) return;
        const top = layout.getBoundingClientRect().top;
        layout.style.height = Math.max(360, window.innerHeight - top - 24) + 'px';
    };
    window.addEventListener('resize', fitViewport);
    tab?.addEventListener('click', () => requestAnimationFrame(fitViewport));
    fitViewport();

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
