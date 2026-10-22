(() => {
  const append = (parent, text, type) => {
    if (!type) { parent.append(document.createTextNode(text)); return; }
    const span = document.createElement("span");
    span.className = "ui-json-" + type;
    span.textContent = text;
    parent.append(span);
  };
  const highlight = (element, text) => {
    const fragment = document.createDocumentFragment();
    let i = 0;
    while (i < text.length) {
      const start = i;
      if (text[i] === '"') {
        i++;
        while (i < text.length) {
          if (text.charCodeAt(i) === 92) { i += 2; continue; }
          if (text[i++] === '"') break;
        }
        let next = i;
        while (next < text.length && (text[next] === ' ' || text[next] === '\t')) next++;
        append(fragment, text.slice(start, i), text[next] === ':' ? "key" : "string");
      } else if ("-0123456789".includes(text[i])) {
        i++;
        while (i < text.length && "0123456789.eE+-".includes(text[i])) i++;
        append(fragment, text.slice(start, i), "number");
      } else if (["true", "false", "null"].some(word => text.startsWith(word, i))) {
        const word = ["true", "false", "null"].find(word => text.startsWith(word, i));
        i += word.length;
        append(fragment, word, "literal");
      } else {
        i++;
        append(fragment, text.slice(start, i));
      }
    }
    element.replaceChildren(fragment);
  };
  window.AgentForgeJsonViewer = {
    render(element, value) {
      if (element) highlight(element, JSON.stringify(value, null, 2));
    },
    renderText(element, jsonText) {
      if (!element) return;
      try { this.render(element, JSON.parse(jsonText)); }
      catch { element.textContent = jsonText; }
    },
    clear(element, message = "Run the tool to view the response.") {
      if (element) element.textContent = message;
    },
    initialize(root = document) {
      root.querySelectorAll("[data-json-viewer]").forEach(element => {
        if (element.dataset.jsonSource === "static") this.renderText(element, element.textContent);
      });
    }
  };
  window.AgentForgeJsonViewer.initialize();
})();
