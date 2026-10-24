// Shared, promise-based confirmation UI. Resolves false for Cancel, Escape and backdrop clicks.
(() => {
  const dialog = document.getElementById('appConfirmDialog');
  if (!dialog) return;
  const title = document.getElementById('appConfirmTitle');
  const message = document.getElementById('appConfirmMessage');
  const accept = document.getElementById('appConfirmAccept');
  const cancel = document.getElementById('appConfirmCancel');
  let resolvePending = null;

  function finish(confirmed) {
    if (!resolvePending) return;
    const resolve = resolvePending;
    resolvePending = null;
    dialog.close();
    resolve(confirmed);
  }

  accept.addEventListener('click', () => finish(true));
  cancel.addEventListener('click', () => finish(false));
  dialog.addEventListener('cancel', event => {
    event.preventDefault();
    finish(false);
  });
  dialog.addEventListener('click', event => {
    if (event.target === dialog) finish(false);
  });
  dialog.addEventListener('close', () => {
    if (resolvePending) {
      const resolve = resolvePending;
      resolvePending = null;
      resolve(false);
    }
  });

  window.ConfirmDialog = Object.freeze({
    show({ title: heading = 'Confirm action', message: body = 'Are you sure?', confirmText = 'Confirm', cancelText = 'Cancel', variant = 'default' } = {}) {
      if (resolvePending) return Promise.resolve(false);
      const mode = ['default', 'warning', 'danger'].includes(variant) ? variant : 'default';
      title.textContent = heading;
      message.textContent = body;
      accept.textContent = confirmText;
      cancel.textContent = cancelText;
      dialog.dataset.variant = mode;
      return new Promise(resolve => {
        resolvePending = resolve;
        try { dialog.showModal(); } catch (error) {
          resolvePending = null;
          resolve(false);
        }
      });
    }
  });
})();
