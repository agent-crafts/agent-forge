(() => {
  const button = document.getElementById("deleteToolButton");
  const context = document.getElementById("toolEditContext");
  const dialog = document.getElementById("deleteToolDialog");
  const confirm = document.getElementById("confirmDeleteTool");
  if (!button || !context || !dialog || !confirm) return;

  button.addEventListener("click", () => dialog.showModal());
  let deleting = false;
  dialog.addEventListener("cancel", event => { if (deleting) event.preventDefault(); });
  dialog.querySelectorAll('form[method="dialog"]').forEach(form => {
    form.addEventListener("submit", event => { if (deleting) event.preventDefault(); });
  });
  confirm.addEventListener("click", async () => {
    if (deleting) return;
    deleting = true;
    dialog.setAttribute("aria-busy", "true");
    confirm.disabled = true;

    button.disabled = true;
    try {
      const response = await fetch("/api/tools/" + encodeURIComponent(context.dataset.toolId), {
        method: "DELETE"
      });
      if (!response.ok) throw new Error("Unable to delete tool (" + response.status + ").");
      window.AgentForgeToast?.afterNavigation({type:"success",title:"Tool deleted successfully",message:"The tool has been removed from the catalog."});
      window.location.assign("/tools");
    } catch (error) {
      deleting = false;
      dialog.removeAttribute("aria-busy");
      button.disabled = false;
      confirm.disabled = false;
      dialog.close();
      window.AgentForgeToast?.show({type:"error",title:"Could not delete tool",message:error.message || "Please try again."});
    }
  });
})();
