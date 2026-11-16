(() => {
    const dialog = document.getElementById('createProjectDialog');
    const form = document.getElementById('projectCreateForm');
    const openButton = document.getElementById('openProjectDialog');
    const editButton = document.getElementById('editProjectButton');
    if (!dialog || !form || (!openButton && !editButton)) return;

    const get = id => document.getElementById(id);
    const title = get('createProjectTitle');
    const description = get('createProjectDescription');
    const saveButton = get('submitProjectForm');
    const feedback = get('projectFormFeedback');
    const iconInput = get('projectIconUpload');
    const preview = get('projectIconPreview');
    const fallback = get('projectIconFallback');
    let editingProject = null;
    let saving = false;
    let previewUrl = null;

    function showIcon(url) {
        preview.hidden = !url;
        if (url) preview.src = url;
        else preview.removeAttribute('src');
        fallback.hidden = !!url;
    }

    iconInput.addEventListener('change', () => {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        const file = iconInput.files?.[0];
        previewUrl = file ? URL.createObjectURL(file) : null;
        showIcon(previewUrl);
    });

    function setMode(editing) {
        title.textContent = editing ? 'Edit Project' : 'Create New Project';
        description.textContent = editing ? 'Update the project identity.' : 'Create the project identity.';
        saveButton.textContent = editing ? 'Save Changes' : 'Save';
    }

    function reset() {
        form.reset();
        feedback.textContent = '';
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        previewUrl = null;
        showIcon(null);
    }

    openButton?.addEventListener('click', () => {
        editingProject = null;
        setMode(false);
        reset();
        dialog.showModal();
    });

    editButton?.addEventListener('click', async () => {
        editButton.disabled = true;
        try {
            const id = get('projectProfile')?.dataset.projectId;
            const response = await fetch('/api/projects/' + encodeURIComponent(id), { credentials: 'same-origin' });
            if (!response.ok) throw new Error('Unable to load project (HTTP ' + response.status + ').');
            editingProject = await response.json();
            setMode(true);
            reset();
            get('projectName').value = editingProject.name;
            get('projectDescription').value = editingProject.description || '';
            const icon = editingProject.icon || '';
            if (icon.startsWith('projects/')) showIcon('/api/files/' + icon);
            dialog.showModal();
        } catch (error) {
            window.AgentForgeToast?.show({ type: 'error', title: 'Unable to edit project', message: error.message });
        } finally {
            editButton.disabled = false;
        }
    });

    dialog.addEventListener('cancel', event => { if (saving) event.preventDefault(); });
    dialog.querySelectorAll('[data-modal-close], #closeProjectDialog, #cancelProjectDialog').forEach(button => {
        button.addEventListener('click', event => {
            if (saving) { event.preventDefault(); event.stopImmediatePropagation(); }
        }, true);
    });

    saveButton.type = 'button';
    saveButton.addEventListener('click', event => { event.preventDefault(); void save(); });
    form.addEventListener('submit', event => { event.preventDefault(); void save(); });

    async function save() {
        if (saving) return;
        const name = get('projectName').value.trim();
        if (!name || !form.checkValidity()) {
            feedback.textContent = 'Enter a valid project name and description.';
            get('projectName').focus();
            return;
        }
        const token = dialog.querySelector('input[name="__RequestVerificationToken"]')?.value;
        saving = true;
        saveButton.disabled = true;
        feedback.textContent = 'Saving project...';
        try {
            let icon = editingProject?.icon || null;
            const file = iconInput.files?.[0];
            if (file) {
                if (file.size > 2 * 1024 * 1024 || !['image/png', 'image/jpeg', 'image/webp', 'image/gif'].includes(file.type))
                    throw new Error('Choose PNG, JPG, WebP or GIF under 2 MB.');
                const upload = new FormData();
                upload.append('file', file);
                const uploadedResponse = await fetch('/api/files/projects', {
                    method: 'POST', credentials: 'same-origin',
                    headers: token ? { RequestVerificationToken: token } : {},
                    body: upload
                });
                const uploaded = await uploadedResponse.json().catch(() => null);
                if (!uploadedResponse.ok) throw new Error(uploaded?.error || 'Icon upload failed.');
                icon = uploaded.filename;
            }
            const response = await fetch(editingProject ? '/api/projects/' + encodeURIComponent(editingProject.id) : '/api/projects', {
                method: editingProject ? 'PUT' : 'POST', credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json', ...(token ? { RequestVerificationToken: token } : {}) },
                body: JSON.stringify({ name, description: get('projectDescription').value.trim(), icon })
            });
            if (!response.ok) {
                const result = await response.json().catch(() => null);
                throw new Error(result?.error || result?.title || 'Could not save project (HTTP ' + response.status + ').');
            }
            let destination = window.location.pathname;
            if (!editingProject) {
                const created = await response.json();
                destination = '/projects/' + encodeURIComponent(created.id);
            }
            window.AgentForgeToast?.afterNavigation({
                type: 'success', title: editingProject ? 'Project updated' : 'Project created',
                message: 'Project details saved successfully.'
            });
            dialog.close();
            window.location.assign(destination);
        } catch (error) {
            feedback.textContent = error.message || 'Could not save project.';
            window.AgentForgeToast?.show({ type: 'error', title: 'Could not save project', message: feedback.textContent });
        } finally {
            saving = false;
            saveButton.disabled = false;
        }
    }
})();
