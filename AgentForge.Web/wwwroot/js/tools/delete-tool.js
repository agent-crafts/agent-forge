(() => {
  const button = document.getElementById("deleteToolButton");
  const context = document.getElementById("toolEditContext");
  if (!button || !context) return;

  button.addEventListener("click", async () => {
    if (!window.ConfirmDialog) return;
    const confirmed = await window.ConfirmDialog.show({
      title: "Delete tool?",
      message: "Delete " + context.dataset.toolDisplayName + "? It will be removed from the tool catalog but its data will be retained.",
      confirmText: "Delete tool",
      variant: "danger"
    });
    if (!confirmed) return;

    button.disabled = true;
    try {
      const response = await fetch("/api/tools/" + encodeURIComponent(context.dataset.toolId), {
        method: "DELETE"
      });
      if (!response.ok) throw new Error("Unable to delete tool (" + response.status + ").");
      window.location.assign("/tools");
    } catch (error) {
      button.disabled = false;
      window.alert(error.message || "Unable to delete tool.");
    }
  });
})();
