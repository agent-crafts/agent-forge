(() => {
  const dialog = document.getElementById('createAgentDialog');
  const form = document.getElementById('agentCreateForm');
  const open = document.getElementById('openAgentDialog');
  if (!dialog || !form || !open) return;

  const sections = [...form.querySelectorAll('[data-agent-step]')];
  const indicators = [...form.querySelectorAll('[data-step-indicator]')];
  const back = document.getElementById('previousAgentStep');
  const next = document.getElementById('nextAgentStep');
  const submit = document.getElementById('submitAgentForm');
  const feedback = document.getElementById('agentFormFeedback');
  const iconInput = document.getElementById('agentIconUpload');
  const iconPreview = document.getElementById('agentIconPreview');
  let previewUrl = null;
  iconInput.addEventListener('change', () => {
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    const file = iconInput.files?.[0];
    previewUrl = file ? URL.createObjectURL(file) : null;
    iconPreview.hidden = !previewUrl;
    if (previewUrl) iconPreview.src = previewUrl;
    else iconPreview.removeAttribute('src');
  });
  form.noValidate = true;
  let step = 0;
  let saving = false;

  function showStep(index) {
    step = index;
    sections.forEach((section, i) => { section.hidden = i !== step; });
    indicators.forEach((indicator, i) => {
      indicator.classList.toggle('is-active', i === step);
      indicator.classList.toggle('is-complete', i < step);
      if (i === step) indicator.setAttribute('aria-current', 'step');
      else indicator.removeAttribute('aria-current');
    });
    back.hidden = step === 0;
    next.hidden = step === sections.length - 1;
    submit.hidden = step !== sections.length - 1;
    feedback.hidden = true;
  }

  function validateField(input) {
    const empty = input.required && !input.value.trim();
    const message = empty ? 'This field is required.' : (input.validity.valid ? '' : input.validationMessage);
    const wrapper = input.closest('.agent-field');
    const error = wrapper?.querySelector('.ui-field-error') || wrapper?.querySelector('.agent-field-error');
    if (error) {
      error.textContent = message;
      error.hidden = !message;
    }
    input.classList.toggle('is-invalid', Boolean(message));
    input.setAttribute('aria-invalid', message ? 'true' : 'false');
    const hint = document.getElementById(input.id + '-hint');
    const describedBy = [hint?.id, message ? error?.id : null].filter(Boolean).join(' ');
    if (describedBy) input.setAttribute('aria-describedby', describedBy);
    else input.removeAttribute('aria-describedby');
    return !message;
  }

  function validateStep(index) {
    let first = null;
    sections[index].querySelectorAll('input,select,textarea').forEach(input => {
      if (!validateField(input) && !first) first = input;
    });
    if (first) {
      if (step !== index) showStep(index);
      first.focus();
      return false;
    }
    return true;
  }

  function updateField(event) {
    const input = event.target;
    if (input.matches('input,select,textarea') && input.hasAttribute('aria-invalid')) validateField(input);
  }

  form.addEventListener('input', updateField);
  form.addEventListener('change', updateField);
  open.addEventListener('click', () => { showStep(0); dialog.showModal(); });
  document.getElementById('closeAgentDialog').addEventListener('click', () => { if (!saving) dialog.close(); });
  document.getElementById('cancelAgentDialog').addEventListener('click', () => { if (!saving) dialog.close(); });
  next.addEventListener('click', () => { if (!saving && validateStep(step)) showStep(step + 1); });
  back.addEventListener('click', () => { if (!saving) showStep(step - 1); });
  submit.addEventListener('click', event => {
    event.preventDefault();
    void createAgent();
  });

  form.addEventListener('submit', event => {
    event.preventDefault();
    void createAgent();
  });

  async function createAgent() {
    if (saving) return;
    if (!validateStep(0) || !validateStep(1)) return;

    const value = name => String(form.elements.namedItem(name)?.value ?? '').trim();
    const tags = value('tags').split(',').map(tag => tag.trim()).filter(Boolean);
    const categoryNames = ['Development', 'Planning', 'Quality', 'Testing', 'Operations', 'Documentation', 'Research', 'Data', 'Other'];
    const category = categoryNames.indexOf(value('category'));
    if (category < 0) {
      showStep(0);
      feedback.textContent = 'Select a valid category.';
      feedback.hidden = false;
      return;
    }

    const payload = {
      name: value('name'),
      description: value('description'),
      category,
      icon: null,
      tags,
      systemInstructions: value('instructions')
    };
    const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
    saving = true;
    submit.disabled = true;
    back.disabled = true;
    next.disabled = true;
    feedback.textContent = 'Creating agent…';
    feedback.hidden = false;

    try {
      const iconFile = iconInput.files?.[0];
      if (iconFile) {
        if (iconFile.size > 2 * 1024 * 1024 || !['image/png', 'image/jpeg', 'image/webp', 'image/gif'].includes(iconFile.type))
          throw new Error('Choose a PNG, JPG, WebP or GIF image smaller than 2 MB.');
        const upload = new FormData();
        upload.append('file', iconFile);
        const uploadResponse = await fetch('/api/agents/icons', {
          method: 'POST', credentials: 'same-origin',
          headers: { 'RequestVerificationToken': token }, body: upload
        });
        const uploaded = await uploadResponse.json().catch(() => null);
        if (!uploadResponse.ok) throw new Error(uploaded?.error || `Icon upload failed (HTTP ${uploadResponse.status}).`);
        payload.icon = uploaded.filename;
      }
      const response = await fetch('/api/agents', {
        method: 'POST',
        credentials: 'same-origin',
        headers: {
          'Content-Type': 'application/json',
          'RequestVerificationToken': token
        },
        body: JSON.stringify(payload)
      });
      if (!response.ok) {
        const error = await response.json().catch(() => null);
        throw new Error(error?.error || error?.title || `Could not create agent (HTTP ${response.status}).`);
      }
      // Reload the server-rendered gallery to show the newly persisted agent.
      form.reset();
      dialog.close();
      window.location.reload();
    } catch (error) {
      feedback.textContent = error.message || 'Could not create agent. Please try again.';
      feedback.hidden = false;
    } finally {
      saving = false;
      submit.disabled = false;
      back.disabled = false;
      next.disabled = false;
    }
  }
  showStep(0);
})();
