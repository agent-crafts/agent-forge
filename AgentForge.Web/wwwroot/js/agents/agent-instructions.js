(() => {
  const form = document.getElementById('agentInstructionsForm');
  if (!form) return;
  const editor = document.getElementById('agentSystemInstructions');
  const button = document.getElementById('saveAgentInstructions');
  const feedback = document.getElementById('agentInstructionsFeedback');
  const id = form.dataset.agentId;
  const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;

  form.addEventListener('submit', async event => {
    event.preventDefault();
    if (button.disabled) return;
    const instructions = form.querySelector('[data-markdown-editor]')?.markdownEditor?.value() ?? editor.value;
    if (instructions.length > 12000) {
      feedback.textContent = 'Instructions cannot exceed 12,000 characters.';
      return;
    }
    button.disabled = true;
    feedback.textContent = 'Saving…';
    try {
      // Read the latest persisted definition so editing instructions does not
      // accidentally overwrite name, category, icon, or tags.
      const currentResponse = await fetch('/api/agents/' + encodeURIComponent(id), {
        credentials: 'same-origin'
      });
      if (!currentResponse.ok) throw new Error('Could not load agent (HTTP ' + currentResponse.status + ').');
      const current = await currentResponse.json();
      const response = await fetch('/api/agents/' + encodeURIComponent(id), {
        method: 'PUT',
        credentials: 'same-origin',
        headers: {
          'Content-Type': 'application/json',
          'RequestVerificationToken': token
        },
        body: JSON.stringify({
          id: current.id,
          name: current.name,
          description: current.description,
          category: current.category,
          icon: current.icon,
          tags: current.tags,
          systemInstructions: form.querySelector("[data-markdown-editor]")?.markdownEditor?.value() ?? editor.value
        })
      });
      if (!response.ok) {
        const error = await response.json().catch(() => null);
        throw new Error(error?.error || error?.title || 'Save failed (HTTP ' + response.status + ').');
      }
      feedback.textContent = '';
      window.AgentForgeToast?.show({ type: 'success', title: 'Instructions saved', message: 'Agent instructions updated successfully.' });
    } catch (error) {
      feedback.textContent = error.message || 'Unable to save instructions.';
    } finally {
      button.disabled = false;
    }
  });
})();
