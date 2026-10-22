(() => {
  const form = document.getElementById("githubToolTryForm");
  if (!form) return;
  const get = id => document.getElementById(id);
  const run = get("githubToolRun"), runLabel = get("githubToolRunLabel");
  const status = get("githubToolStatus"), duration = get("githubToolDuration");
  const timestamp = get("githubToolTimestamp"), durationDetail = get("githubToolDurationDetail");
  const json = get("githubToolResultJson"), raw = get("githubToolResultRaw");
  const formatted = get("githubToolResultFormatted"), copy = get("githubToolCopy");
  const tabs = ["Json", "Formatted", "Raw"];
  let lastResult = null, rawText = "";
  const setText = (node, value) => { node.textContent = value; };
  const showTab = selected => tabs.forEach(name => {
    const active = name === selected;
    const tab = get("githubResponse" + name + "Tab");
    const panel = get("githubResponse" + name + "Panel");
    tab.setAttribute("aria-selected", String(active));
    tab.classList.toggle("border-accent", active);
    tab.classList.toggle("bg-active", active);
    tab.classList.toggle("text-accent", active);
    tab.classList.toggle("border-line", !active);
    tab.classList.toggle("text-muted", !active);
    panel.hidden = !active;
  });
  tabs.forEach(name => get("githubResponse" + name + "Tab").addEventListener("click", () => showTab(name)));
  const field = (label, value) => {
    const row = document.createElement("div");
    const heading = document.createElement("div");
    heading.className = "text-xs font-semibold text-muted";
    heading.textContent = label;
    const body = document.createElement("div");
    body.className = "mt-1 break-words text-sm text-heading";
    body.textContent = value == null ? "—" : String(value);
    row.append(heading, body);
    return row;
  };
  const renderFormatted = result => {
    formatted.replaceChildren();
    if (!result || !result.success || !result.data) {
      formatted.append(field("Result", result?.error?.message || result?.error || "No issue data returned."));
      return;
    }
    const issue = result.data;
    formatted.append(field("Issue number", issue.number), field("Title", issue.title), field("State", issue.state));
    if (typeof issue.url === "string" && /^https:\/\/github\.com\//.test(issue.url)) {
      const link = document.createElement("a");
      link.href = issue.url;
      link.target = "_blank";
      link.rel = "noopener noreferrer";
      link.className = "text-sm text-accent underline break-all";
      link.textContent = issue.url;
      formatted.append(link);
    }
  };
  const reset = () => {
    lastResult = null; rawText = "";
    setText(status, "Not run");
    status.className = "rounded-full bg-raised px-3 py-1.5 text-xs font-semibold text-muted";
    duration.hidden = true; setText(timestamp, 'Not run');
    setText(durationDetail, "Not run");
    window.AgentForgeJsonViewer.clear(json);
    setText(raw, "Run the tool to view the raw response.");
    setText(formatted, "Run the tool to view formatted issue details.");
    copy.disabled = true; setText(copy, "Copy"); showTab("Json");
  };
  form.addEventListener("reset", reset);
  copy.addEventListener("click", async () => {
    if (!lastResult || !navigator.clipboard) return;
    const selected = tabs.find(name => get("githubResponse" + name + "Tab").getAttribute("aria-selected") === "true");
    const value = selected === "Raw" ? rawText : JSON.stringify(lastResult, null, 2);
    try { await navigator.clipboard.writeText(value); setText(copy, "Copied"); }
    catch { setText(copy, "Copy failed"); }
  });
  form.addEventListener("submit", async event => {
    event.preventDefault();
    if (!form.reportValidity()) return;
    run.disabled = true; setText(runLabel, "Running...");
    setText(status, "Running");
    status.className = "rounded-full bg-active px-3 py-1.5 text-xs font-semibold text-accent";
    window.AgentForgeJsonViewer.clear(json, "Loading GitHub issue...");
    const started = performance.now();
    try {
      const response = await fetch(form.action, {
        method: "POST", body: new FormData(form), credentials: "same-origin",
        headers: { Accept: "application/json" }
      });
      const contentType = response.headers.get("content-type") || "";
      if (!contentType.includes("application/json")) throw new Error("Unexpected server response.");
      rawText = await response.text();
      lastResult = JSON.parse(rawText);
      window.AgentForgeJsonViewer.render(json, lastResult);
      setText(raw, rawText);
      renderFormatted(lastResult);
      const ok = response.ok && lastResult.success === true;
      setText(status, ok ? "✓ Success" : "✕ Failed");
      status.className = "rounded-full px-3 py-1.5 text-xs font-semibold " + (ok ? "bg-emerald-100 text-emerald-700" : "bg-red-100 text-red-700");
      copy.disabled = false;
    } catch {
      lastResult = { success: false, error: "Could not complete the request. Check the application connection and try again." };
      rawText = JSON.stringify(lastResult);
      window.AgentForgeJsonViewer.render(json, lastResult);
      setText(raw, rawText);
      renderFormatted(lastResult);
      setText(status, "✕ Failed");
      status.className = "rounded-full bg-red-100 px-3 py-1.5 text-xs font-semibold text-red-700";
      copy.disabled = false;
    } finally {
      const elapsed = ((performance.now() - started) / 1000).toFixed(2) + "s";
      duration.hidden = false; setText(duration, elapsed); setText(durationDetail, elapsed);
      
      const now = new Date(); timestamp.dateTime = now.toISOString();
      setText(timestamp, now.toLocaleString());
      run.disabled = false; setText(runLabel, "Run Tool");
    }
  });
  reset();
})();