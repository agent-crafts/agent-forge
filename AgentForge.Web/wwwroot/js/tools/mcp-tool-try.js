(() => {
  const form = document.getElementById("mcpToolTryForm");
  if (!form) return;

  const args = document.getElementById("mcpToolArguments");
  const error = document.getElementById("mcpToolArgumentsError");
  const run = document.getElementById("mcpToolRun");
  const status = document.getElementById("mcpToolStatus");
  const result = document.getElementById("mcpToolResult");
  const viewer = window.AgentForgeJsonViewer;

  const clearError = () => {
    args.removeAttribute("aria-invalid");
    error.hidden = true;
    error.textContent = "";
  };
  args.addEventListener("input", clearError);

  form.addEventListener("submit", async event => {
    event.preventDefault();
    if (run.disabled) return;
    clearError();

    let input;
    try {
      input = JSON.parse(args.value);
      if (!input || typeof input !== "object" || Array.isArray(input)) {
        throw new Error("Expected a JSON object.");
      }
    } catch {
      args.setAttribute("aria-invalid", "true");
      error.hidden = false;
      error.textContent = "Enter a valid JSON object, such as {}.";
      status.textContent = "Invalid arguments";
      viewer.clear(result);
      args.focus();
      return;
    }

    run.disabled = true;
    status.textContent = "Running…";
    viewer.clear(result, "Waiting for the tool response…");
    try {
      const response = await fetch(form.action, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "RequestVerificationToken": form.elements.__RequestVerificationToken.value
        },
        body: args.value
      });
      const payload = await response.json().catch(() => null);
      if (response.ok && payload?.success) {
        status.textContent = "Success";
        viewer.render(result, payload.data);
      } else {
        status.textContent = `Failed (HTTP ${response.status})`;
        if (payload) viewer.render(result, payload);
        else viewer.clear(result, "The server returned no JSON response.");
      }
    } catch {
      status.textContent = "Request failed";
      viewer.clear(result, "Could not reach AgentForge. Try again.");
    } finally {
      run.disabled = false;
    }
  });
})();
