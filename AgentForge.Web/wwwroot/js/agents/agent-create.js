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
  let step = 0;
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
    next.hidden = step === 2;
    submit.hidden = step !== 2;
    feedback.hidden = true;
    if (step === 2) {
      const value = name => String(form.elements.namedItem(name).value).trim();
      for (const field of ['name','category','description','icon','tags']) {
        document.getElementById('agentReview' + field[0].toUpperCase() + field.slice(1)).textContent = value(field) || '—';
      }
      document.getElementById('agentReviewInstructions').textContent = value('instructions') || '—';
    }
  }
  function validateStep(index) {
    const inputs = [...sections[index].querySelectorAll('input,select,textarea')];
    for (const input of inputs) {
      if (!input.value.trim() && input.required) input.setCustomValidity('This field is required.');
      else input.setCustomValidity('');
      if (!input.reportValidity()) { input.focus(); return false; }
    }
    return true;
  }
  form.addEventListener('input', event => { if (event.target.setCustomValidity) event.target.setCustomValidity(''); });
  function close() { dialog.close(); }
  open.addEventListener('click', () => { showStep(0); dialog.showModal(); });
  document.getElementById('closeAgentDialog').addEventListener('click', close);
  document.getElementById('cancelAgentDialog').addEventListener('click', close);
  next.addEventListener('click', () => { if (validateStep(step)) showStep(step + 1); });
  back.addEventListener('click', () => showStep(step - 1));
  form.addEventListener('submit', event => {
    event.preventDefault();
    if (!validateStep(0)) { showStep(0); validateStep(0); return; }
    if (!validateStep(1)) { showStep(1); validateStep(1); return; }
    feedback.textContent = 'Form validated. Agent creation is not saved yet; backend integration is pending.';
    feedback.hidden = false;
  });
  showStep(0);
})();
