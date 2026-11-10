(() => {
    const panel = document.querySelector('#agentToolsPanel .agent-tools');
    if (!panel) return;
    const search = panel.querySelector('[data-agent-tool-search]');
    const category = panel.querySelector('[data-agent-tool-category]');
    const rows = [...panel.querySelectorAll('[data-agent-tool-row]')];
    const save = panel.querySelector('[data-agent-tools-save]');
    const status = panel.querySelector('[data-agent-tools-status]');
    const filter = () => {
        const q = search.value.trim().toLowerCase();
        let visible = 0;
        rows.forEach(row => {
            const show = (!q || row.dataset.search.includes(q)) &&
                (!category.value || row.dataset.category === category.value);
            row.hidden = !show;
            if (show) visible++;
        });
        panel.querySelector('[data-agent-tool-empty]').hidden = visible !== 0;
    };
    search.addEventListener('input', filter);
    category.addEventListener('change', filter);
    save.addEventListener('click', async () => {
        save.disabled = true;
        status.textContent = 'Saving...';
        const toolIds = [...panel.querySelectorAll('[data-agent-tool-toggle]:checked')].map(x => x.value);
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        try {
            const response = await fetch('/api/agents/' + encodeURIComponent(panel.dataset.agentId) + '/tools', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json', ...(token ? { 'RequestVerificationToken': token } : {}) },
                body: JSON.stringify({ toolIds })
            });
            if (!response.ok) {
                const body = await response.json().catch(() => ({}));
                throw new Error(body.error || 'Could not save agent tools.');
            }
            status.textContent = 'Tool assignments saved.';
        } catch (error) {
            status.textContent = error.message;
        } finally {
            save.disabled = false;
        }
    });
})();
