(() => {
  const editButton = document.getElementById('editAgentButton');
  const deleteButton = document.getElementById('deleteAgentButton');
  const instructionsForm = document.getElementById('agentInstructionsForm');
  if (!editButton || !deleteButton || !instructionsForm) return;
  const id = instructionsForm.dataset.agentId;
  const token = instructionsForm.querySelector('input[name="__RequestVerificationToken"]')?.value;
  const endpoint = '/api/agents/' + encodeURIComponent(id);
  const categories = ['Development', 'Planning', 'Quality', 'Testing', 'Operations', 'Documentation', 'Research', 'Data', 'Other'];

  const editDialog = document.createElement('dialog');
  editDialog.className = 'agent-dialog rounded-xl border border-line bg-surface p-6 text-content shadow-xl w-full max-w-lg backdrop:bg-black/50';
  editDialog.innerHTML = `<form class="flex flex-col gap-4">
    <h2 class="text-xl font-semibold">Edit agent</h2>
    <label class="flex flex-col gap-1">Name<input name="name" class="ui-input" required maxlength="80"></label>
    <label class="flex flex-col gap-1">Category<span class="ui-select-wrap relative block"><select name="category" class="ui-input w-full appearance-none pr-10" required>${categories.map((category, index) => `<option value="${index}">${category}</option>`).join('')}</select><svg class="ui-select-chevron pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 text-muted" aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m6 9 6 6 6-6"/></svg></span></label>
    <label class="flex flex-col gap-1">Description<textarea name="description" class="ui-input" required maxlength="300" rows="4"></textarea></label>
    <label class="flex flex-col gap-1">Tags (comma-separated)<input name="tags" class="ui-input" maxlength="200"></label>
    <p class="text-sm text-red-600" role="alert" hidden></p>
    <div class="flex justify-end gap-2"><button type="button" data-cancel class="ui-button">Cancel</button><button type="submit" class="ui-button ui-button--primary">Save Changes</button></div>
  </form>`;
  document.body.appendChild(editDialog);
  const editForm = editDialog.querySelector('form');
  const editError = editForm.querySelector('[role="alert"]');
  const editSave = editForm.querySelector('[type="submit"]');
  let current = null;
  let busy = false;

  async function loadAgent() {
    const response = await fetch(endpoint, { credentials: 'same-origin' });
    if (!response.ok) throw new Error('Unable to load agent (HTTP ' + response.status + ').');
    return response.json();
  }
  async function apiRequest(method, body) {
    const response = await fetch(endpoint, {
      method, credentials: 'same-origin',
      headers: { 'RequestVerificationToken': token, ...(body ? { 'Content-Type': 'application/json' } : {}) },
      ...(body ? { body: JSON.stringify(body) } : {})
    });
    if (!response.ok) {
      const error = await response.json().catch(() => null);
      throw new Error(error?.error || error?.title || 'Request failed (HTTP ' + response.status + ').');
    }
  }
  function showError(element, error) {
    element.textContent = error.message || 'Something went wrong.';
    element.hidden = false;
  }
  editButton.addEventListener('click', async () => {
    if (busy) return;
    editButton.disabled = true;
    editError.hidden = true;
    try {
      current = await loadAgent();
      editForm.elements.namedItem('name').value = current.name;
      editForm.elements.namedItem('description').value = current.description;
      editForm.elements.namedItem('category').value = String(current.category);
      editForm.elements.namedItem('tags').value = (current.tags || []).join(', ');
      editDialog.showModal();
    } catch (error) {
      window.AgentForgeToast?.show({ type: 'error', title: 'Unable to edit agent', message: error.message });
    } finally { editButton.disabled = false; }
  });
  editForm.querySelector('[data-cancel]').addEventListener('click', () => { if (!busy) editDialog.close(); });
  editDialog.addEventListener('cancel', event => { if (busy) event.preventDefault(); });
  editForm.addEventListener('submit', async event => {
    event.preventDefault();
    if (busy || !editForm.reportValidity()) return;
    busy = true;
    editSave.disabled = true;
    editError.hidden = true;
    try {
      await apiRequest('PUT', {
        id: current.id,
        name: editForm.elements.namedItem('name').value.trim(),
        description: editForm.elements.namedItem('description').value.trim(),
        category: Number(editForm.elements.namedItem('category').value),
        icon: current.icon,
        tags: editForm.elements.namedItem('tags').value.split(',').map(tag => tag.trim()).filter(Boolean),
        systemInstructions: current.systemInstructions
      });
      window.AgentForgeToast?.afterNavigation({ type: 'success', title: 'Agent updated', message: 'Agent details saved successfully.' });
      window.location.reload();
    } catch (error) { showError(editError, error); }
    finally { busy = false; editSave.disabled = false; }
  });

  const deleteDialog = document.createElement('dialog');
  deleteDialog.className = 'rounded-xl border border-line bg-surface p-6 text-content shadow-xl w-full max-w-md backdrop:bg-black/50';
  deleteDialog.innerHTML = `<h2 class="text-xl font-semibold">Delete agent?</h2>
    <p class="my-4">This deactivates the agent and removes it from the active agents list.</p>
    <p role="alert" class="text-sm text-red-600" hidden></p>
    <div class="flex justify-end gap-2"><button type="button" data-cancel class="ui-button">Cancel</button><button type="button" data-confirm class="ui-button ui-button--danger">Delete agent</button></div>`;
  document.body.appendChild(deleteDialog);
  const deleteError = deleteDialog.querySelector('[role="alert"]');
  const confirmDelete = deleteDialog.querySelector('[data-confirm]');
  deleteButton.addEventListener('click', () => { deleteError.hidden = true; deleteDialog.showModal(); });
  deleteDialog.querySelector('[data-cancel]').addEventListener('click', () => { if (!busy) deleteDialog.close(); });
  deleteDialog.addEventListener('cancel', event => { if (busy) event.preventDefault(); });
  confirmDelete.addEventListener('click', async () => {
    if (busy) return;
    busy = true;
    confirmDelete.disabled = true;
    deleteError.hidden = true;
    try {
      await apiRequest('DELETE');
      window.AgentForgeToast?.afterNavigation({ type: 'success', title: 'Agent deleted', message: 'Agent removed from the active list.' });
      window.location.assign('/agents');
    } catch (error) { showError(deleteError, error); }
    finally { busy = false; confirmDelete.disabled = false; }
  });
})();
