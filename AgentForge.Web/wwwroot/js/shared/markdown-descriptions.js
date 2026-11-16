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

  // One dedicated modal is shared by all description editors.
  const previewDialog = document.createElement('dialog');
  previewDialog.className = 'ui-markdown-preview-dialog';
  previewDialog.setAttribute('aria-labelledby', 'markdownPreviewTitle');
  previewDialog.innerHTML = `
    <div class="ui-markdown-preview-header">
      <h2 id="markdownPreviewTitle">Markdown Preview</h2>
      <button type="button" class="ui-markdown-toolbar-button" data-close-markdown-preview aria-label="Close preview">Close</button>
    </div>
    <div class="ui-markdown-content ui-markdown-preview-body" data-preview-body></div>
  `;
  document.body.appendChild(previewDialog);
  previewDialog.querySelector('[data-close-markdown-preview]').addEventListener('click', () => previewDialog.close());
  previewDialog.addEventListener('click', event => {
    if (event.target === previewDialog) previewDialog.close();
  });

  document.querySelectorAll('[data-markdown-preview-button]').forEach(button => {
    const input = document.getElementById(button.dataset.markdownPreviewButton);
    if (!input) return;
    const toolbar = input.previousElementSibling;
    if (toolbar?.classList.contains('ui-markdown-toolbar')) {
      button.classList.remove('ui-button', 'ui-button--secondary');
      button.classList.add('ui-markdown-toolbar-button');
      button.textContent = 'Preview';
      toolbar.appendChild(button);
    }
    button.removeAttribute('aria-pressed');
    button.addEventListener('click', () => {
      const body = previewDialog.querySelector('[data-preview-body]');
      body.replaceChildren();
      if (window.marked && window.DOMPurify) render(input.value, body);
      else body.textContent = input.value;
      previewDialog.showModal();
    });
  });
})();
