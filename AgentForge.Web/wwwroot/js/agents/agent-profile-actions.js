(() => {
  const editButton = document.getElementById('editAgentButton');
  const deleteButton = document.getElementById('deleteAgentButton');
  const instructionsForm = document.getElementById('agentInstructionsForm');
  if (!editButton || !deleteButton || !instructionsForm) return;
  const id = instructionsForm.dataset.agentId;
  const token = instructionsForm.querySelector('input[name="__RequestVerificationToken"]')?.value;
  const endpoint = '/api/agents/' + encodeURIComponent(id);
  let busy = false;
  async function apiRequest(method) {
    const response = await fetch(endpoint, { method, credentials: 'same-origin', headers: { 'RequestVerificationToken': token } });
    if (!response.ok) throw new Error('Request failed (HTTP ' + response.status + ').');
  }
  function showError(element, error) {
    element.textContent = error.message || 'Something went wrong.';
    element.hidden = false;
  }
  const deleteDialog = document.createElement('dialog');
  deleteDialog.className = 'rounded-xl border border-line bg-surface p-6 text-content shadow-xl w-full max-w-md';
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
