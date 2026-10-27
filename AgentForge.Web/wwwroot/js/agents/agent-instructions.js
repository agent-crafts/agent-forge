(() => {
  const form = document.getElementById('agentInstructionsForm');
  if (!form) return;
  const editor = document.getElementById('agentSystemInstructions');
  const button = document.getElementById('saveAgentInstructions');
  const feedback = document.getElementById('agentInstructionsFeedback');
  const id = form.dataset.agentId;
  const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;

  const toolbar = form.querySelector('.agent-editor-toolbar');
  toolbar?.addEventListener('click', event => {
    const action = event.target.closest('button[data-markdown]')?.dataset.markdown;
    if (!action) return;
    const start = editor.selectionStart;
    const end = editor.selectionEnd;
    const selected = editor.value.slice(start, end);
    const templates = {
      heading: ['## ', selected || 'Heading', ''],
      bold: ['**', selected || 'bold text', '**'],
      italic: ['*', selected || 'italic text', '*'],
      bullet: ['- ', selected || 'List item', ''],
      number: ['1. ', selected || 'List item', ''],
      code: ['`', selected || 'code', '`'],
      link: ['[', selected || 'link text', '](https://example.com)']
    };
    const template = templates[action];
    if (!template) return;
    const [before, middle, after] = template;
    const replacement = before + middle + after;
    if (editor.value.length - (end - start) + replacement.length > editor.maxLength) {
      feedback.textContent = 'Instructions cannot exceed 12,000 characters.';
      return;
    }
    editor.setRangeText(replacement, start, end, 'select');
    editor.focus();
    editor.setSelectionRange(start + before.length, start + before.length + middle.length);
    editor.dispatchEvent(new Event('input', { bubbles: true }));
  });

  form.addEventListener('submit', async event => {
    event.preventDefault();
    if (button.disabled) return;
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
          systemInstructions: editor.value
        })
      });
      if (!response.ok) {
        const error = await response.json().catch(() => null);
        throw new Error(error?.error || error?.title || 'Save failed (HTTP ' + response.status + ').');
      }
      feedback.textContent = 'Instructions saved.';
    } catch (error) {
      feedback.textContent = error.message || 'Unable to save instructions.';
    } finally {
      button.disabled = false;
    }
  });
})();
