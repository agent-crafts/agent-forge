(() => {
    const root = document.querySelector('[data-agent-skills]');
    if (!root) return;
    const $ = selector => root.querySelector(selector);
    const skills = [
        { name: 'Code Review', description: 'Review pull requests for correctness, security, and maintainability.', origin: 'Created', icon: '⌘' },
        { name: 'Test Generation', description: 'Generate unit and integration tests following project conventions.', origin: 'Created', icon: '⚗' },
        { name: 'API Documentation', description: 'Produce API references and usage examples.', origin: 'Imported', icon: '▤' },
        { name: 'Debugging', description: 'Investigate failures and identify root causes.', origin: 'Created', icon: '⌕' }
    ].map(skill => ({ ...skill, instructions: '# ' + skill.name + '\n\n' + skill.description + '\n\n## Instructions\n1. Understand the task.\n2. Follow project conventions.\n3. Produce actionable results.' }));
    const grid = $('[data-skill-grid]');
    const search = $('[data-skill-search]');
    const filter = $('[data-skill-filter]');
    const dialog = $('[data-skill-dialog]');
    const editor = $('[data-skill-editor]');
    const form = $('[data-skill-form]');
    let mode = 'create';
    let editing = -1;
    const el = (tag, className, text) => { const node = document.createElement(tag); if (className) node.className = className; if (text !== undefined) node.textContent = text; return node; };
    function render() {
        grid.replaceChildren();
        const query = search.value.trim().toLowerCase();
        const visible = skills.map((skill, index) => ({ skill, index })).filter(({ skill }) =>
            (!filter.value || skill.origin === filter.value) &&
            (skill.name + ' ' + skill.description).toLowerCase().includes(query));
        $('[data-skill-empty]').hidden = visible.length !== 0;
        visible.forEach(({ skill, index }) => {
            const card = el('article', 'agent-skill-card ui-item-card');
            const icon = el('span', 'agent-skill-icon ui-icon-tile', skill.icon);
            icon.setAttribute('aria-hidden', 'true');
            const body = el('div', 'agent-skill-content');
            body.append(el('h3', '', skill.name), el('p', '', skill.description));
            const footer = el('div', 'agent-skill-footer');
            footer.append(el('span', 'ui-tool-card-badge', skill.origin));
            const edit = el('button', 'agent-skill-edit', 'Edit skill');
            edit.type = 'button';
            edit.addEventListener('click', () => openEditor(index));
            footer.append(edit);
            body.append(footer);
            card.append(icon, body);
            grid.append(card);
        });
    }
    function openDialog() {
        $('[data-skill-choices]').hidden = false;
        form.hidden = true;
        $('[data-skill-dialog-title]').textContent = 'Add Skill';
        form.reset();
        dialog.showModal();
    }
    function chooseMode(next) {
        mode = next;
        $('[data-skill-choices]').hidden = true;
        form.hidden = false;
        $('[data-skill-dialog-title]').textContent = next === 'create' ? 'Create Skill' : next === 'upload' ? 'Upload Skill' : 'Import from GitHub';
        $('[data-skill-form-note]').textContent = next === 'create' ? 'Define a skill for this agent.' : next === 'upload' ? 'Preview an uploaded skill entry. No file is processed.' : 'Preview a skill entry from a repository. No network import occurs.';
        $('[data-skill-instructions]').hidden = next !== 'create';
        $('[data-skill-upload]').hidden = next !== 'upload';
        $('[data-skill-github]').hidden = next !== 'github';
    }
    function openEditor(index) {
        editing = index;
        $('[data-skill-editor-title]').textContent = skills[index].name;
        $('[data-skill-markdown]').value = skills[index].instructions;
        editor.showModal();
    }
    $('[data-skill-add]').addEventListener('click', openDialog);
    root.querySelectorAll('[data-skill-close]').forEach(button => button.addEventListener('click', () => dialog.close()));
    root.querySelectorAll('[data-skill-editor-close]').forEach(button => button.addEventListener('click', () => editor.close()));
    root.querySelectorAll('[data-skill-mode]').forEach(button => button.addEventListener('click', () => chooseMode(button.dataset.skillMode)));
    $('[data-skill-back]').addEventListener('click', () => { form.hidden = true; $('[data-skill-choices]').hidden = false; $('[data-skill-dialog-title]').textContent = 'Add Skill'; });
    form.addEventListener('submit', event => {
        event.preventDefault();
        const data = new FormData(form);
        const name = String(data.get('name') || '').trim();
        const description = String(data.get('description') || '').trim();
        if (!name || !description) return;
        skills.unshift({ name, description, origin: mode === 'create' ? 'Created' : 'Imported', icon: '◇', instructions: String(data.get('instructions') || '# ' + name + '\n\n' + description) });
        search.value = ''; filter.value = ''; render(); dialog.close();
    });
    $('[data-skill-editor-save]').addEventListener('click', () => {
        if (editing < 0) return;
        skills[editing].instructions = $('[data-skill-markdown]').value;
        editor.close();
    });
    search.addEventListener('input', render);
    filter.addEventListener('change', render);
    render();
})();