(() => {
  const form = document.getElementById("toolConfigurationForm");
  const context = document.getElementById("toolEditContext");
  if (!form || !context) return;
  const rows = document.getElementById("toolConfigurationRows");
  const status = document.getElementById("toolConfigurationStatus");
  const save = document.getElementById("saveToolConfiguration");
  const reserved = new Set(["__proto__", "prototype", "constructor"]);
  const keyPattern = /^[A-Za-z_][A-Za-z0-9_.-]*$/;
  const fixedKeys = context.dataset.toolExecutorKey === "get_github_issue" || context.dataset.toolName === "get_github_issue"
    ? new Set(["owner", "repo"])
    : context.dataset.toolExecutorKey === "http.get_json" ? new Set(["url"]) : new Set();

  function addRow(key = "", value = "") {
    const row = document.createElement("div");
    row.className = "flex w-full min-w-0 items-end gap-3";
    row.dataset.originalKey = key;
    row.dataset.originalValue = value;
    const keyField = document.createElement("label");
    keyField.className = "grid min-w-0 flex-1 gap-1";
    const keyLabel = document.createElement("span");
    keyLabel.className = "ui-field-label";
    keyLabel.textContent = "Key";
    const keyInput = document.createElement("input");
    keyInput.className = "ui-input";
    keyInput.type = "text";
    keyInput.placeholder = "SETTING_NAME";
    keyInput.value = key;
    keyInput.required = true;
    keyInput.maxLength = 128;
    keyField.append(keyLabel, keyInput);
    const valueField = document.createElement("label");
    valueField.className = "grid min-w-0 flex-1 gap-1";
    const valueLabel = document.createElement("span");
    valueLabel.className = "ui-field-label";
    valueLabel.textContent = "Value (optional)";
    const valueInput = document.createElement("input");
    valueInput.className = "ui-input";
    valueInput.type = "text";
    valueInput.placeholder = "Value";
    valueInput.value = value;
    valueField.append(valueLabel, valueInput);
    const remove = document.createElement("button");
    remove.type = "button";
    remove.className = "param-remove";
    remove.setAttribute("aria-label", "Remove configuration");
    remove.title = "Remove configuration";
    remove.innerHTML = '<svg aria-hidden="true" viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18M8 6V4h8v2M5 6l1 14h12l1-14M10 10v7M14 10v7"/></svg>';
    remove.addEventListener("click", () => { row.remove(); status.textContent = ""; });
    if (fixedKeys.has(key)) {
      keyInput.readOnly = true;
      remove.disabled = true;
      valueInput.required = true;
      valueLabel.textContent = "Value (required by executor)";
    }
    row.append(keyField, valueField, remove);
    rows.append(row);
  }

  let configuration = {};
  try {
    const parsed = JSON.parse(context.dataset.toolConfiguration || "{}");
    if (parsed && typeof parsed === "object" && !Array.isArray(parsed)) configuration = parsed;
  } catch {
    status.textContent = "Existing configuration could not be parsed. Saving is disabled to prevent data loss.";
    save.disabled = true;
    return;
  }
  Object.entries(configuration).forEach(([key, value]) => addRow(key, typeof value === "string" ? value : JSON.stringify(value)));
  document.getElementById("addToolConfiguration").addEventListener("click", () => {
    addRow();
    rows.lastElementChild?.querySelector("input")?.focus();
    status.textContent = "";
  });

  form.addEventListener("submit", async event => {
    event.preventDefault();
    const next = Object.create(null);
    for (const row of rows.children) {
      const [keyInput, valueInput] = row.querySelectorAll("input");
      const key = keyInput.value.trim();
      if (!keyPattern.test(key) || reserved.has(key)) {
        status.textContent = "Configuration keys must start with a letter or underscore and contain only letters, numbers, underscores, dots or hyphens.";
        keyInput.focus();
        return;
      }
      if (Object.hasOwn(next, key)) {
        status.textContent = "Duplicate configuration key: " + key;
        keyInput.focus();
        return;
      }
      if (fixedKeys.has(key) && !valueInput.value.trim()) {
        status.textContent = key + " is required by this executor.";
        valueInput.focus();
        return;
      }
      next[key] = key === row.dataset.originalKey && valueInput.value === row.dataset.originalValue && Object.hasOwn(configuration, key)
        ? configuration[key] : valueInput.value;
    }
    for (const key of fixedKeys) {
      if (!Object.hasOwn(next, key) || !next[key].trim()) {
        status.textContent = "Required configuration is missing: " + key;
        return;
      }
    }
    const token = document.querySelector('#addToolDialog input[name="__RequestVerificationToken"]')?.value;
    save.disabled = true;
    status.textContent = "Saving configuration…";
    try {
      const response = await fetch("/api/tools/" + encodeURIComponent(context.dataset.toolId), {
        method: "PUT",
        credentials: "same-origin",
        headers: { "Content-Type": "application/json", ...(token ? { "RequestVerificationToken": token } : {}) },
        body: JSON.stringify({
          id: context.dataset.toolId,
          displayName: context.dataset.toolDisplayName,
          description: context.dataset.toolDescription,
          category: Number(context.dataset.toolCategory),
          jsonSchema: context.dataset.toolSchema,
          imageUrl: context.dataset.toolImageUrl || null,
          executorKey: context.dataset.toolExecutorKey || null,
          configurationJson: JSON.stringify(next)
        })
      });
      if (!response.ok) throw new Error("Save failed (" + response.status + ").");
      configuration = next;
      context.dataset.toolConfiguration = JSON.stringify(next);
      rows.replaceChildren();
      Object.entries(next).forEach(([key, value]) => addRow(key, typeof value === "string" ? value : JSON.stringify(value)));
      status.textContent = "Configuration saved.";
    } catch (error) {
      status.textContent = error.message || "Could not save configuration.";
    } finally {
      save.disabled = false;
    }
  });
})();
