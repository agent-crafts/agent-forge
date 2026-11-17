(() => {
  // Render from textContent rather than HTML to preserve Razor's output encoding.
  const render = (source, target) => {
    if (typeof window.marked?.parse !== 'function' || !window.DOMPurify?.sanitize) {
      target.textContent = 'Markdown preview is unavailable. Please reload the page and try again.';
      return;
    }
    target.innerHTML = window.DOMPurify.sanitize(window.marked.parse(source, { breaks: true, gfm: true }));
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
    let selectionStart = 0;
    let selectionEnd = 0;
    const rememberSelection = () => {
      selectionStart = input.selectionStart;
      selectionEnd = input.selectionEnd;
    };
    ['select', 'keyup', 'click', 'input'].forEach(event => input.addEventListener(event, rememberSelection));
    controls.forEach(([label, prefix, suffix, placeholder]) => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'ui-markdown-toolbar-button';
      button.textContent = label;
      button.setAttribute('aria-label', label);
      // Keep the textarea selection when a toolbar control is clicked.
      button.addEventListener('mousedown', event => event.preventDefault());
      button.addEventListener('click', () => {
        const start = selectionStart;
        const end = selectionEnd;
        const selected = input.value.slice(start, end);
        const isInline = ['Bold', 'Italic', 'Code', 'Link'].includes(label);
        const alreadyWrapped = isInline && selected && input.value.slice(Math.max(0, start - prefix.length), start) === prefix && input.value.slice(end, end + suffix.length) === suffix;
        let insertion;
        let nextStart;
        let nextEnd;
        if (alreadyWrapped) {
          insertion = selected;
          const replaceStart = start - prefix.length;
          const replaceEnd = end + suffix.length;
          const next = input.value.slice(0, replaceStart) + insertion + input.value.slice(replaceEnd);
          input.value = next;
          nextStart = replaceStart;
          nextEnd = replaceStart + insertion.length;
        } else {
          const content = selected || placeholder;
          insertion = prefix + content + suffix;
          const next = input.value.slice(0, start) + insertion + input.value.slice(end);
          if (input.maxLength >= 0 && next.length > input.maxLength) {
            input.setCustomValidity('Description is too long to apply this formatting.');
            input.reportValidity();
            input.setCustomValidity('');
            return;
          }
          input.value = next;
          nextStart = start + prefix.length;
          nextEnd = nextStart + content.length;
        }
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.focus();
        input.setSelectionRange(nextStart, nextEnd);
        rememberSelection();
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
      render(input.value, body);
      previewDialog.showModal();
    });
  });
})();
