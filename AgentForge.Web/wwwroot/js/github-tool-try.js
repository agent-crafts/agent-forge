(() => {
  const form = document.getElementById("githubToolTryForm");
  if (!form) return;
  const button = document.getElementById("githubToolRun");
  const panel = document.getElementById("githubToolResult");
  const output = document.getElementById("githubToolResultJson");
  form.addEventListener("submit", async event => {
    event.preventDefault();
    if (!form.reportValidity()) return;
    button.disabled = true;
    button.textContent = "Running...";
    panel.hidden = false;
    output.textContent = "Loading GitHub issue...";
    try {
      const response = await fetch(form.action, {
        method: "POST",
        body: new FormData(form),
        credentials: "same-origin",
        headers: { "Accept": "application/json" }
      });
      const contentType = response.headers.get("content-type") || "";
      if (!contentType.includes("application/json")) throw new Error("Unexpected server response.");
      const result = await response.json();
      output.textContent = JSON.stringify(result, null, 2);
      if (!response.ok) output.textContent = "Request failed: " + output.textContent;
    } catch {
      output.textContent = "Could not complete the request. Check the application connection and try again.";
    } finally {
      button.disabled = false;
      button.textContent = "Run Tool";
    }
  });
})();