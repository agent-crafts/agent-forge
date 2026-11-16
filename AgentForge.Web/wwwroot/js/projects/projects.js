(() => {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const headers = { 'Content-Type': 'application/json', ...(token ? { RequestVerificationToken: token } : {}) };
    const error = document.getElementById('projectFormError');
    const showError = message => { if (error) { error.textContent = message; error.hidden = false; } };
    async function request(url, method, body) {
        const response = await fetch(url, { method, headers, body: body ? JSON.stringify(body) : undefined });
        if (!response.ok) {
            let message = 'Request failed. Please try again.';
            try { const data = await response.json(); message = data.error || data.title || message; } catch {}
            throw new Error(message);
        }
        return response;
    }
    const dialog = document.getElementById('projectDialog');
    document.getElementById('openProjectDialog')?.addEventListener('click', () => dialog?.showModal());
    document.getElementById('cancelProjectDialog')?.addEventListener('click', () => dialog?.close());
    document.getElementById('createProjectForm')?.addEventListener('submit', async event => {
        event.preventDefault();
        const button = event.currentTarget.querySelector('[type="submit"]');
        button.disabled = true;
        try {
            const response = await request('/api/projects', 'POST', {
                name: document.getElementById('projectName').value,
                description: document.getElementById('projectDescription').value
            });
            const { id } = await response.json();
            location.assign('/projects/' + encodeURIComponent(id));
        } catch (e) { showError(e.message); button.disabled = false; }
    });
    const editForm = document.getElementById('editProjectForm');
    const id = editForm?.dataset.projectId;
    editForm?.addEventListener('submit', async event => {
        event.preventDefault();
        const button = editForm.querySelector('[type="submit"]');
        button.disabled = true;
        try {
            await request('/api/projects/' + encodeURIComponent(id), 'PUT', {
                name: document.getElementById('editProjectName').value,
                description: document.getElementById('editProjectDescription').value
            });
            location.reload();
        } catch (e) { showError(e.message); button.disabled = false; }
    });
    document.getElementById('archiveProject')?.addEventListener('click', async event => {
        if (!confirm('Archive this project?')) return;
        event.currentTarget.disabled = true;
        try {
            await request('/api/projects/' + encodeURIComponent(id) + '/archive', 'POST');
            location.assign('/projects');
        } catch (e) { showError(e.message); event.currentTarget.disabled = false; }
    });
    const search = document.getElementById('projectSearch');
    search?.addEventListener('input', () => {
        const query = search.value.trim().toLowerCase();
        let visible = 0;
        document.querySelectorAll('[data-project-search]').forEach(card => {
            card.hidden = !card.dataset.projectSearch.includes(query);
            if (!card.hidden) visible++;
        });
        const empty = document.getElementById('projectSearchEmpty');
        if (empty) empty.hidden = visible > 0;
    });
})();
