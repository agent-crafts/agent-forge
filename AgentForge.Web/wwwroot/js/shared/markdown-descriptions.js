(() => {
  // Render from textContent rather than HTML to preserve Razor's output encoding.
  const render = (source, target) => {
    if (!window.marked || !window.DOMPurify) return;
    target.innerHTML = DOMPurify.sanitize(marked.parse(source, { breaks: true }));
    target.querySelectorAll('a[href]').forEach(link => {
      const url = link.getAttribute('href') || '';
      if (!/^(https?:\/\/|mailto:|\/)/i.test(url)) link.removeAttribute('href');
      else if (/^https?:\/\//i.test(url)) {
        link.target = '_blank';
        link.rel = 'noopener noreferrer';
      }
    });
  };

  document.querySelectorAll('[data-markdown-render]').forEach(node => {
    render(node.textContent, node);
  });

  document.querySelectorAll('[data-markdown-preview-button]').forEach(button => {
    const id = button.dataset.markdownPreviewButton;
    const input = document.getElementById(id);
    const preview = [...document.querySelectorAll('[data-markdown-preview]')]
      .find(node => node.dataset.markdownPreview === id);
    if (!input || !preview) return;
    button.addEventListener('click', () => {
      const show = preview.hidden;
      if (show) render(input.value, preview);
      preview.hidden = !show;
      button.textContent = show ? 'Edit Markdown' : 'Preview Markdown';
      button.setAttribute('aria-pressed', String(show));
      input.hidden = show;
    });
    input.addEventListener('input', () => {
      if (!preview.hidden) render(input.value, preview);
    });
  });
})();
