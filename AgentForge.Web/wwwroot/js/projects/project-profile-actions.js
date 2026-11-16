(() => {
    const profile = document.getElementById('projectProfile');
    const archiveButton = document.getElementById('archiveProjectButton');
    const dialog = document.getElementById('archiveProjectDialog');
    const confirmButton = document.getElementById('confirmArchiveProject');
    if (!profile || !archiveButton || !dialog || !confirmButton) return;

    let busy = false;
    archiveButton.addEventListener('click', () => dialog.showModal());
    dialog.addEventListener('cancel', event => { if (busy) event.preventDefault(); });
    dialog.querySelectorAll('form[method="dialog"]').forEach(form => {
        form.addEventListener('submit', event => { if (busy) event.preventDefault(); });
    });
    confirmButton.addEventListener('click', async () => {
        if (busy) return;
        busy = true;
        confirmButton.disabled = true;
        dialog.setAttribute('aria-busy', 'true');
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const response = await fetch('/api/projects/' + encodeURIComponent(profile.dataset.projectId) + '/archive', {
                method: 'POST', credentials: 'same-origin',
                headers: token ? { RequestVerificationToken: token } : {}
            });
            if (!response.ok) throw new Error('Could not archive project (HTTP ' + response.status + ').');
            window.AgentForgeToast?.afterNavigation({ type: 'success', title: 'Project archived', message: 'Project removed from the active list.' });
            window.location.assign('/projects');
        } catch (error) {
            dialog.close();
            window.AgentForgeToast?.show({ type: 'error', title: 'Could not archive project', message: error.message });
        } finally {
            busy = false;
            confirmButton.disabled = false;
            dialog.removeAttribute('aria-busy');
        }
    });
})();
