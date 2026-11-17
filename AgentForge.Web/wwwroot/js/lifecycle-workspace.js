(() => {
  "use strict";
  const root = document.querySelector(".lf-root");
  if (!root) return;
  root.querySelectorAll("[data-lf-tab]").forEach(button => {
    button.addEventListener("click", () => {
      const tab = button.dataset.lfTab;
      root.querySelectorAll("[data-lf-tab]").forEach(item => {
        const active = item === button;
        item.classList.toggle("is-active", active);
        item.setAttribute("aria-selected", String(active));
      });
      root.querySelectorAll("[data-lf-panel]").forEach(panel => { panel.hidden = panel.dataset.lfPanel !== tab; });
    });
  });
  root.querySelectorAll("[data-lf-viewport]").forEach(button => {
    button.addEventListener("click", () => {
      root.querySelectorAll("[data-lf-viewport]").forEach(item => item.classList.toggle("is-active", item === button));
      const preview = root.querySelector(".lf-browser");
      if (preview) preview.classList.toggle("is-mobile", button.dataset.lfViewport === "mobile");
    });
  });
  root.querySelectorAll("[data-lf-file]").forEach(button => {
    button.addEventListener("click", () => {
      const file = button.dataset.lfFile;
      root.querySelectorAll("[data-lf-file]").forEach(item => item.classList.toggle("is-active", item === button));
      root.querySelectorAll("[data-lf-code]").forEach(code => { code.hidden = code.dataset.lfCode !== file; });
      const name = root.querySelector("#lfFilename");
      if (name) name.textContent = file === "booking" ? "booking.ts · TASK-011" : "availability.ts · TASK-010";
    });
  });
  const form = root.querySelector("#lfDemoChat");
  if (!form) return;
  form.addEventListener("submit", event => {
    event.preventDefault();
    const input = root.querySelector("#lfPrompt");
    const messages = root.querySelector("#lfMessages");
    const value = input?.value.trim();
    if (!value || !messages) return;
    const user = document.createElement("div");
    user.className = "lf-message lf-user";
    user.textContent = value;
    messages.appendChild(user);
    const reply = document.createElement("div");
    reply.className = "lf-message lf-agent";
    reply.textContent = "This is a scripted UI demo. Explore the sample artifacts in the right panel; real agent conversations will be connected in a later milestone.";
    messages.appendChild(reply);
    input.value = "";
    messages.scrollTop = messages.scrollHeight;
  });
})();
