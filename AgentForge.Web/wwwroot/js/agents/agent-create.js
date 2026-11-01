(() => {
  const dialog = document.getElementById('createAgentDialog');
  const form = document.getElementById('agentCreateForm');
  const open = document.getElementById('openAgentDialog');
  const editOpen = document.getElementById('editAgentButton');
  if (!dialog || !form || (!open && !editOpen)) return;

  let editingAgent = null;
  let saving = false;
  let previewUrl = null;
  const categoryNames = ['Development', 'Planning', 'Quality', 'Testing', 'Operations', 'Documentation', 'Research', 'Data', 'Other'];
  const get = id => document.getElementById(id);
  const dialogTitle = get('createAgentTitle');
  const dialogDescription = get('createAgentDescription');
  const saveButton = get('submitAgentForm');
  const status = get('agentFormFeedback');
  const iconInput = get('agentIconUpload');
  const iconPreview = get('agentIconPreview');

  const showIcon = url => {
    iconPreview.hidden = !url;
    if (url) iconPreview.src = url;
    else iconPreview.removeAttribute('src');
  };

  iconInput.addEventListener('change', () => {
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    const file = iconInput.files?.[0];
    previewUrl = file ? URL.createObjectURL(file) : null;
    showIcon(previewUrl);
  });

  const fieldError = (field, message) => {
    field.setAttribute('aria-invalid', 'true');
    field.classList.add('ui-field-invalid');
    const node = document.getElementById(field.id + 'Error');
    if (!node) return;
    node.className = 'ui-field-error';
    node.textContent = message;
    const described = field.getAttribute('aria-describedby') || '';
    if (!described.split(' ').includes(node.id)) field.setAttribute('aria-describedby', (described + ' ' + node.id).trim());
  };

  const clearFieldError = field => {
    field.removeAttribute('aria-invalid');
    field.classList.remove('ui-field-invalid');
    const node = document.getElementById(field.id + 'Error');
    if (node) {
      node.textContent = '';
      node.classList.add('hidden');
    }
  };

  const fields = () => ['agentName', 'agentCategoryInput', 'agentDescription', 'agentTags'].map(get);

  const resetForm = () => {
    form.reset();
    fields().forEach(clearFieldError);
    iconInput.value = '';
    showIcon(null);
    status.textContent = '';
  };

  const setDialogMode = editing => {
    dialogTitle.textContent = editing ? 'Edit Agent' : 'Create New Agent';
    dialogDescription.textContent = editing
      ? 'Update the agent identity.'
      : 'Create the agent identity. Configure instructions and other settings from its profile.';
    saveButton.textContent = editing ? 'Save Changes' : 'Save';
  };

  const storedIconUrl = icon => {
    if (!icon) return null;
    if (icon.startsWith('/')) return icon;
    if (icon.startsWith('agents/')) return '/api/files/' + icon;
    const file = icon.split('/').pop() || '';
    const stem = file.replace(/\.[^.]+$/, '');
    const ext = (file.match(/\.[^.]+$/) || [''])[0].toLowerCase();
    return stem.length === 24 && /^[0-9a-f]+$/i.test(stem) && ['.png', '.jpg', '.webp', '.gif'].includes(ext)
      ? '/api/files/' + icon
      : null;
  };

  const categoryValue = agent => typeof agent.category === 'number' ? categoryNames[agent.category] || '' : agent.category || '';

  dialog.addEventListener('input', event => {
    if (event.target.matches('input,textarea,select')) clearFieldError(event.target);
  });
  dialog.addEventListener('cancel', event => { if (saving) event.preventDefault(); });
  dialog.querySelectorAll('[data-modal-close], #closeAgentDialog, #cancelAgentDialog').forEach(button => {
    button.addEventListener('click', event => {
      if (saving) {
        event.preventDefault();
        event.stopImmediatePropagation();
      }
    }, true);
  });

  open?.addEventListener('click', () => {
    editingAgent = null;
    setDialogMode(false);
    resetForm();
    dialog.showModal();
  });

  editOpen?.addEventListener('click', async () => {
    editOpen.disabled = true;
    try {
      const id = document.getElementById('agentInstructionsForm')?.dataset.agentId;
      const response = await fetch('/api/agents/' + encodeURIComponent(id), { credentials: 'same-origin' });
      if (!response.ok) throw new Error('Unable to load agent (HTTP ' + response.status + ').');
      editingAgent = await response.json();
      setDialogMode(true);
      resetForm();
      get('agentName').value = editingAgent.name;
      get('agentDescription').value = editingAgent.description;
      get('agentCategoryInput').value = categoryValue(editingAgent);
      get('agentTags').value = (editingAgent.tags || []).join(', ');
      showIcon(storedIconUrl(editingAgent.icon));
      dialog.showModal();
    } catch (error) {
      window.AgentForgeToast?.show({ type: 'error', title: 'Unable to edit agent', message: error.message });
    } finally {
      editOpen.disabled = false;
    }
  });

  saveButton.type = 'button';
  saveButton.addEventListener('click', event => {
    event.preventDefault();
    event.stopPropagation();
    void saveAgent();
  });
  form.addEventListener('submit', event => {
    event.preventDefault();
    void saveAgent();
  });

  async function saveAgent() {
    if (saving) return;
    const nameField = get('agentName');
    const displayCategory = get('agentCategoryInput');
    const descriptionField = get('agentDescription');
    const tagsField = get('agentTags');
    const name = nameField.value.trim();
    const description = descriptionField.value.trim();
    const tags = tagsField.value.split(',').map(tag => tag.trim()).filter(Boolean);
    const category = categoryNames.indexOf(displayCategory.value);
    const errors = [];
    const requiredFields = [
      [nameField, 'Agent name is required.'],
      [displayCategory, 'Category is required.'],
      [descriptionField, 'Description is required.']
    ];
    for (const [field, message] of requiredFields) {
      clearFieldError(field);
      if (!field.value.trim()) errors.push([field, message]);
      else if (!field.checkValidity()) errors.push([field, 'Enter a valid value.']);
    }
    clearFieldError(tagsField);
    if (displayCategory.value && category < 0 && !errors.some(([field]) => field === displayCategory))
      errors.push([displayCategory, 'Select a valid category.']);
    if (tags.length > 10) errors.push([tagsField, 'At most 10 tags are allowed.']);
    else if (tags.some(tag => tag.length > 40)) errors.push([tagsField, 'Each tag must be 1–40 characters.']);
    if (errors.length) {
      errors.forEach(([field, message]) => fieldError(field, message));
      status.textContent = 'Correct the highlighted fields.';
      errors[0][0].focus();
      return;
    }

    const payload = {
      name,
      description,
      category,
      icon: editingAgent?.icon ?? null,
      tags,
      systemInstructions: editingAgent?.systemInstructions ?? ''
    };
    const token = dialog.querySelector('input[name="__RequestVerificationToken"]')?.value;
    saving = true;
    saveButton.disabled = true;
    saveButton.setAttribute('aria-busy', 'true');
    const originalContent = saveButton.innerHTML;
    saveButton.innerHTML = '<span class="inline-flex items-center gap-2"><span class="inline-block size-4 animate-spin rounded-full border-2 border-current border-t-transparent" aria-hidden="true"></span>Saving…</span>';
    status.textContent = 'Saving agent...';

    try {
      const iconFile = iconInput.files?.[0];
      if (iconFile) {
        if (iconFile.size > 2 * 1024 * 1024 || !['image/png', 'image/jpeg', 'image/webp', 'image/gif'].includes(iconFile.type))
          throw new Error('Choose PNG, JPG, WebP or GIF under 2 MB.');
        const upload = new FormData();
        upload.append('file', iconFile);
        const uploadResponse = await fetch('/api/files/agents', {
          method: 'POST',
          credentials: 'same-origin',
          headers: token ? { RequestVerificationToken: token } : {},
          body: upload
        });
        const uploaded = await uploadResponse.json().catch(() => null);
        if (!uploadResponse.ok) throw new Error(uploaded?.error || 'Icon upload failed.');
        payload.icon = uploaded.filename;
      }
      const response = await fetch(editingAgent ? '/api/agents/' + encodeURIComponent(editingAgent.id) : '/api/agents', {
        method: editingAgent ? 'PUT' : 'POST',
        credentials: 'same-origin',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { RequestVerificationToken: token } : {})
        },
        body: JSON.stringify(editingAgent ? { ...payload, id: editingAgent.id } : payload)
      });
      if (!response.ok) {
        const contentType = response.headers.get('content-type') || '';
        const error = contentType.includes('json') ? await response.json() : await response.text();
        const message = typeof error === 'string' ? error : (error.detail || error.error || error.title || JSON.stringify(error.errors || error));
        throw new Error(message || 'HTTP ' + response.status);
      }
      window.AgentForgeToast?.afterNavigation({
        type: 'success',
        title: editingAgent ? 'Agent updated' : 'Agent created',
        message: editingAgent ? 'Agent details saved successfully.' : 'Agent created successfully.'
      });
      dialog.close();
      window.location.reload();
    } catch (error) {
      status.textContent = 'Could not save agent: ' + (error.message || 'Unknown error');
      window.AgentForgeToast?.show({
        type: 'error',
        title: editingAgent ? 'Could not update agent' : 'Could not create agent',
        message: error.message || 'Please try again.'
      });
    } finally {
      saving = false;
      saveButton.disabled = false;
      saveButton.removeAttribute('aria-busy');
      saveButton.innerHTML = originalContent;
    }
  }
})();
