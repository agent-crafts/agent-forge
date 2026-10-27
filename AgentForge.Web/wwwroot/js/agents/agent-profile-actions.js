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
  const deleteDialog = document.getElementById('deleteAgentDialog');
  const confirmDelete = document.getElementById('confirmDeleteAgent');
  if (!deleteDialog || !confirmDelete) return;
  deleteButton.addEventListener('click', () => { deleteDialog.showModal(); });
  deleteDialog.querySelectorAll('form[method="dialog"]').forEach(form => form.addEventListener('submit', event => { if (busy) event.preventDefault(); }));
  deleteDialog.addEventListener('cancel', event => { if (busy) event.preventDefault(); });
  confirmDelete.addEventListener('click', async () => {
    if (busy) return;
    busy = true;
    confirmDelete.disabled = true;
    deleteDialog.setAttribute('aria-busy', 'true');
    deleteButton.disabled = true;
    try {
      await apiRequest('DELETE');
      window.AgentForgeToast?.afterNavigation({ type: 'success', title: 'Agent deleted', message: 'Agent removed from the active list.' });
      window.location.assign('/agents');
    } catch (error) {
      deleteDialog.removeAttribute('aria-busy');
      deleteButton.disabled = false;
      deleteDialog.close();
      window.AgentForgeToast?.show({ type: 'error', title: 'Could not delete agent', message: error.message || 'Please try again.' });
    } finally { busy = false; confirmDelete.disabled = false; }
  });
})();
