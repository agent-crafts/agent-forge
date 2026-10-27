(() => {
  const button = document.getElementById("deleteToolButton");
  const context = document.getElementById("toolEditContext");
  const dialog = document.getElementById("deleteToolDialog");
  const confirm = document.getElementById("confirmDeleteTool");
  if (!button || !context || !dialog || !confirm) return;

  button.addEventListener("click", () => dialog.showModal());
  confirm.addEventListener("click", async () => {
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
      button.disabled = false;
      confirm.disabled = false;
      dialog.close();
      window.AgentForgeToast?.show({type:"error",title:"Could not delete tool",message:error.message || "Please try again."});
    }
  });
})();
