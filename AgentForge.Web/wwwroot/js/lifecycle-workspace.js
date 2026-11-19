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
      business: "# Healthcare Appointment Platform\n\n## Problem\nPatients need a simple way to discover providers and book appointments.\n\n## Goals\n- Find doctors and available appointments\n- Prevent double bookings\n- Allow cancellation at least 24 hours before a visit",
      api: "openapi: 3.0.3\ninfo:\n  title: Healthcare Appointment API\n  version: 0.1.0\npaths:\n  /doctors:\n    get:\n      summary: Search doctors\n  /appointments:\n    post:\n      summary: Book an available slot",
      acceptance: "# Booking acceptance criteria\n\n- Only future, available slots can be booked.\n- Two patients cannot reserve the same slot.\n- Successful booking returns a confirmation.\n- Invalid patients or providers receive validation errors."
    };
    // Real file URLs can be supplied by data-file-url on each document row when storage is connected.
    // PDF and DOCX require real file bytes; never invent a rendered document from metadata.
    const previewContainer = root.querySelector("[data-lf-doc-preview-content]");
    let activePreview = 0;
    const previewError = text => {
      previewContainer.replaceChildren();
      const notice = document.createElement("p");
      notice.className = "lf-doc-preview-error";
      notice.textContent = text;
      previewContainer.appendChild(notice);
    };
    const renderPreview = async row => {
      const token = ++activePreview;
      previewContainer.replaceChildren();
      const type = row.dataset.type.toLowerCase();
      const url = row.dataset.fileUrl;
      const sample = examples[row.dataset.preview];
      try {
        if (type === "markdown") {
          const content = url ? await (await fetch(url)).text() : sample;
          if (!content) throw new Error("No Markdown content is available.");
          if (!window.marked || !window.DOMPurify) throw new Error("Markdown renderer is unavailable.");
          const html = window.DOMPurify.sanitize(window.marked.parse(content));
          if (token !== activePreview) return;
          previewContainer.innerHTML = html;
          previewContainer.classList.add("lf-doc-markdown-preview");
          return;
        }
        previewContainer.classList.remove("lf-doc-markdown-preview");
        if (type === "yaml" || type === "yml" || type === "json" || type === "text") {
          const content = url ? await (await fetch(url)).text() : sample;
          if (!content) throw new Error("No text content is available.");
          if (token !== activePreview) return;
          const pre = document.createElement("pre");
          pre.className = "lf-doc-code";
          const code = document.createElement("code");
          code.textContent = content;
          pre.appendChild(code);
          previewContainer.appendChild(pre);
          return;
        }
        if (!url) throw new Error("A real " + row.dataset.type + " file is required for preview. This demo only has metadata for this document.");
        const response = await fetch(url);
        if (!response.ok) throw new Error("Unable to load document (HTTP " + response.status + ").");
        const bytes = await response.arrayBuffer();
        if (token !== activePreview) return;
        if (type === "pdf") {
          if (!window.pdfjsLib) throw new Error("PDF.js renderer is unavailable.");
          const pdf = await window.pdfjsLib.getDocument({ data: new Uint8Array(bytes) }).promise;
          try {
            for (let index = 1; index <= pdf.numPages; index++) {
              if (token !== activePreview) return;
              const page = await pdf.getPage(index);
              const base = page.getViewport({ scale: 1 });
              const scale = Math.min(1.6, Math.max(0.5, (previewContainer.clientWidth - 16) / base.width));
              const viewport = page.getViewport({ scale });
              const canvas = document.createElement("canvas");
              canvas.className = "lf-doc-pdf-canvas";
              canvas.width = Math.ceil(viewport.width);
              canvas.height = Math.ceil(viewport.height);
              await page.render({ canvasContext: canvas.getContext("2d"), viewport }).promise;
              if (token !== activePreview) return;
              previewContainer.appendChild(canvas);
            }
          } finally { await pdf.destroy(); }
          return;
        }
        if (type === "word" || type === "docx") {
          if (!window.mammoth) throw new Error("Word renderer is unavailable.");
          const result = await window.mammoth.convertToHtml({ arrayBuffer: bytes });
          if (token !== activePreview) return;
          if (!window.DOMPurify) throw new Error("HTML sanitizer is unavailable.");
          previewContainer.innerHTML = window.DOMPurify.sanitize(result.value);
          previewContainer.classList.add("lf-doc-markdown-preview");
          return;
        }
        throw new Error("Unsupported preview format: " + row.dataset.type);
      } catch (error) {
        if (token === activePreview) previewError(error.message || "Preview failed.");
      }
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
    dialog.addEventListener("close", () => { activePreview++; previewContainer.replaceChildren(); });
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
