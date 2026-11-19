(() => {
  "use strict";
  const root = document.querySelector(".lf-root");
  if (!root) return;
  // Static document-library demo: searchable metadata and illustrative read-only previews.
  const docRows = [...root.querySelectorAll("[data-lf-doc-row]")];
  if (docRows.length) {
    const search = root.querySelector("[data-lf-doc-search]");
    const source = root.querySelector("[data-lf-doc-source]");
    const count = root.querySelector("[data-lf-doc-count]");
    const empty = root.querySelector("[data-lf-doc-empty]");
    const dialog = root.querySelector("[data-lf-doc-dialog]");
    const message = root.querySelector("[data-lf-doc-message]");
    const examples = {
      business: "# Healthcare Appointment Platform\\n\\n## Problem\\nPatients need a simple way to discover providers and book appointments.\\n\\n## Goals\\n- Find doctors and available appointments\\n- Prevent double bookings\\n- Allow cancellation at least 24 hours before a visit",
      architecture: "System Architecture — sample PDF outline\\n\\nWeb application → API → Appointment service → Data store\\n\\nThis is an illustrative preview only; no PDF file is stored.",
      api: "openapi: 3.0.3\\ninfo:\\n  title: Healthcare Appointment API\\n  version: 0.1.0\\npaths:\\n  /doctors:\\n    get:\\n      summary: Search doctors\\n  /appointments:\\n    post:\\n      summary: Book an available slot",
      acceptance: "# Booking acceptance criteria\\n\\n- Only future, available slots can be booked.\\n- Two patients cannot reserve the same slot.\\n- Successful booking returns a confirmation.\\n- Invalid patients or providers receive validation errors.",
      notes: "Stakeholder Notes — sample Word document outline\\n\\n- Prioritize appointment discovery and booking.\\n- Include responsive layouts and accessible error states.\\n- Confirm cancellation policy before release.\\n\\nThis is an illustrative preview only; no Word file is stored."
    };
    // Format-aware, text-only demo rendering. Never interpret document content as HTML.
    const renderPreview = row => {
      const container = root.querySelector("[data-lf-doc-preview-content]");
      container.replaceChildren();
      const format = row.dataset.type.toLowerCase();
      const content = examples[row.dataset.preview] || "";
      const add = (parent, tag, value, className) => {
        const element = document.createElement(tag);
        if (className) element.className = className;
        element.textContent = value;
        parent.appendChild(element);
        return element;
      };
      if (format === "yaml" || format === "yml") {
        const code = add(container, "pre", "", "lf-doc-code");
        add(code, "code", content);
        return;
      }
      const page = document.createElement("div");
      page.className = format === "pdf" ? "lf-doc-preview-page is-pdf" : "lf-doc-preview-page";
      container.appendChild(page);
      if (format === "pdf") {
        add(page, "div", "ARCHITECTURE OVERVIEW", "lf-doc-page-eyebrow");
        add(page, "h2", "System Architecture");
        add(page, "p", "Healthcare Appointment Platform · Illustrative PDF layout");
        const flow = add(page, "div", "", "lf-doc-architecture-flow");
        ["Web application", "API", "Appointment service", "Data store"].forEach((name, index) => {
          if (index) add(flow, "span", "→", "lf-doc-flow-arrow");
          add(flow, "span", name, "lf-doc-flow-node");
        });
        add(page, "p", "This is a sample page layout, not a rendered PDF file.");
        return;
      }
      if (format === "word") {
        add(page, "div", "STAKEHOLDER NOTES", "lf-doc-page-eyebrow");
        add(page, "h2", "Stakeholder Notes");
        add(page, "p", "Healthcare Appointment Platform");
        const list = document.createElement("ul");
        page.appendChild(list);
        ["Prioritize appointment discovery and booking.", "Include responsive layouts and accessible error states.", "Confirm cancellation policy before release."].forEach(item => add(list, "li", item));
        add(page, "p", "Illustrative Word-style preview; no .docx file is stored.", "lf-doc-page-footnote");
        return;
      }
      // Markdown: render headings and lists as readable document elements.
      let list = null;
      content.split(/\\n/).forEach(line => {
        const value = line.trim();
        if (!value) { list = null; return; }
        if (value.startsWith("# ")) { add(page, "h2", value.slice(2)); list = null; }
        else if (value.startsWith("## ")) { add(page, "h3", value.slice(3)); list = null; }
        else if (value.startsWith("- ")) {
          if (!list) { list = document.createElement("ul"); page.appendChild(list); }
          add(list, "li", value.slice(2));
        } else { add(page, "p", value); list = null; }
      });
    };
    const refresh = () => {
      const term = search.value.trim().toLowerCase();
      const selected = source.value;
      let visible = 0;
      docRows.forEach(row => {
        const matches = (selected === "all" || row.dataset.source === selected)
          && (row.dataset.name.toLowerCase().includes(term) || row.dataset.type.toLowerCase().includes(term));
        row.hidden = !matches;
        if (matches) visible++;
      });
      count.textContent = `${visible} document${visible === 1 ? "" : "s"}`;
      empty.hidden = visible !== 0;
    };
    search.addEventListener("input", refresh);
    source.addEventListener("change", refresh);
    root.querySelectorAll("[data-lf-doc-preview]").forEach(button => {
      button.addEventListener("click", () => {
        const row = button.closest("[data-lf-doc-row]");
        root.querySelector("[data-lf-doc-preview-title]").textContent = row.dataset.name;
        root.querySelector("[data-lf-doc-preview-meta]").textContent = `${row.dataset.type} · ${row.dataset.updated} · ${row.dataset.source === "ai" ? "Agent" : "User"}`;
        renderPreview(row);
        if (typeof dialog.showModal === "function") dialog.showModal();
      });
    });
    root.querySelectorAll("[data-lf-doc-close]").forEach(button => button.addEventListener("click", () => dialog.close()));
    const demoNotice = action => {
      message.textContent = `${action} is not available in this static demo. No files are stored or downloaded.`;
      message.hidden = false;
    };
    root.querySelectorAll("[data-lf-doc-download]").forEach(button => button.addEventListener("click", () => demoNotice("Download")));
    root.querySelectorAll("[data-lf-doc-demo-action]").forEach(button => button.addEventListener("click", () => demoNotice(button.dataset.lfDocDemoAction)));
    refresh();
  }
  const boardButtons = root.querySelectorAll("[data-lf-board-view]");
  boardButtons.forEach(button => {
    button.addEventListener("click", () => {
      const view = button.dataset.lfBoardView;
      boardButtons.forEach(item => {
        const active = item === button;
        item.classList.toggle("is-active", active);
        item.setAttribute("aria-pressed", String(active));
      });
      root.querySelectorAll("[data-lf-board-panel]").forEach(panel => {
        panel.hidden = panel.dataset.lfBoardPanel !== view;
      });
    });
  });
  const featureDetails = root.querySelectorAll('[data-lf-board-panel="features"] .lf-specs-feature-details');
  const featureToggle = root.querySelector("[data-lf-features-toggle]");
  if (featureToggle && featureDetails.length) {
    const syncFeatureToggle = () => {
      const allExpanded = [...featureDetails].every(details => details.open);
      featureToggle.textContent = allExpanded ? "Collapse all" : "Expand all";
      featureToggle.setAttribute("aria-expanded", String(allExpanded));
    };
    featureToggle.addEventListener("click", () => {
      const expand = ![...featureDetails].every(details => details.open);
      featureDetails.forEach(details => { details.open = expand; });
      syncFeatureToggle();
    });
    featureDetails.forEach(details => details.addEventListener("toggle", syncFeatureToggle));
    syncFeatureToggle();
  }
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
