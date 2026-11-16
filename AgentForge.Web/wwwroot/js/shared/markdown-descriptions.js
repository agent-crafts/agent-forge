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

  // Reusable formatting controls for description fields in create/edit dialogs.
  document.querySelectorAll('textarea[data-markdown-input]').forEach(input => {
    const toolbar = document.createElement('div');
    toolbar.className = 'ui-markdown-toolbar';
    toolbar.setAttribute('role', 'toolbar');
    toolbar.setAttribute('aria-label', 'Markdown formatting');
    const controls = [
      ['Bold', '**', '**', 'bold text'],
      ['Italic', '*', '*', 'italic text'],
      ['Heading', '## ', '', 'Heading'],
      ['Bullet list', '- ', '', 'List item'],
      ['Numbered list', '1. ', '', 'List item'],
      ['Quote', '> ', '', 'Quote'],
      ['Code', '`', '`', 'code'],
      ['Link', '[', '](https://example.com)', 'link text']
    ];
    controls.forEach(([label, prefix, suffix, placeholder]) => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'ui-markdown-toolbar-button';
      button.textContent = label;
      button.setAttribute('aria-label', label);
      button.addEventListener('click', () => {
        const start = input.selectionStart;
        const end = input.selectionEnd;
        const selected = input.value.slice(start, end) || placeholder;
        const insertion = prefix + selected + suffix;
        const next = input.value.slice(0, start) + insertion + input.value.slice(end);
        if (input.maxLength >= 0 && next.length > input.maxLength) return;
        input.value = next;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.focus();
        input.setSelectionRange(start + prefix.length, start + prefix.length + selected.length);
      });
      toolbar.appendChild(button);
    });
    input.before(toolbar);
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
    const dialog = input.closest('dialog');
    dialog?.addEventListener('close', () => {
      input.hidden = false;
      preview.hidden = true;
      button.textContent = 'Preview Markdown';
      button.setAttribute('aria-pressed', 'false');
    });
    input.addEventListener('input', () => {
      if (!preview.hidden) render(input.value, preview);
    });
  });
})();
