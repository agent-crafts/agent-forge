window.AgentForgeJsonViewer = {
  render(element, value) {
    if (!element) return;
    const formatted = JSON.stringify(value, null, 2);
    element.textContent = formatted;
  }
};
