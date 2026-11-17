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
    // Read the live selection on activation; clicking a toolbar button must not
    // replace the selected description text with a stale cursor position.
    let savedSelection = null;
    const saveSelection = () => {
      savedSelection = { start: input.selectionStart, end: input.selectionEnd };
    };
    input.addEventListener('select', saveSelection);
    input.addEventListener('keyup', saveSelection);
    input.addEventListener('mouseup', saveSelection);
    controls.forEach(([label, prefix, suffix, placeholder]) => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'ui-markdown-toolbar-button';
      button.textContent = label;
      button.setAttribute('aria-label', label);
      button.addEventListener('pointerdown', event => {
        saveSelection();
        event.preventDefault();
      });
      button.addEventListener('click', () => {
        const { start, end } = savedSelection ?? { start: input.selectionStart, end: input.selectionEnd };
        const selected = input.value.slice(start, end);
        const inline = ['Bold', 'Italic', 'Code', 'Link'].includes(label);
        // Markdown emphasis delimiters must touch non-whitespace characters.
        // Keep whitespace outside the markers even when it was selected.
        const leading = inline ? (selected.match(/^\\s*/) || [''])[0] : '';
        const trailing = inline ? (selected.match(/\\s*$/) || [''])[0] : '';
        const coreStart = start + leading.length;
        const coreEnd = Math.max(coreStart, end - trailing.length);
        const core = input.value.slice(coreStart, coreEnd);
        const wrappedSelection = inline && core.startsWith(prefix) && core.endsWith(suffix) && core.length > prefix.length + suffix.length;
        const wrappedOutside = inline && coreStart >= prefix.length && input.value.slice(coreStart - prefix.length, coreStart) === prefix && input.value.slice(coreEnd, coreEnd + suffix.length) === suffix;
        let replaceStart = start;
        let replaceEnd = end;
        let insertion;
        let selectStart;
        let selectEnd;
        if (wrappedSelection) {
          const unwrapped = core.slice(prefix.length, -suffix.length);
          insertion = leading + unwrapped + trailing;
          selectStart = start + leading.length;
          selectEnd = selectStart + unwrapped.length;
        } else if (wrappedOutside) {
          replaceStart = coreStart - prefix.length;
          replaceEnd = coreEnd + suffix.length;
          insertion = leading + core + trailing;
          selectStart = replaceStart + leading.length;
          selectEnd = selectStart + core.length;
        } else if (inline && core) {
          insertion = leading + prefix + core + suffix + trailing;
          selectStart = start + leading.length + prefix.length;
          selectEnd = selectStart + core.length;
        } else {
          const content = selected || placeholder;
          insertion = prefix + content + suffix;
          selectStart = start + prefix.length;
          selectEnd = selectStart + content.length;
        }
        const next = input.value.slice(0, replaceStart) + insertion + input.value.slice(replaceEnd);
        if (input.maxLength >= 0 && next.length > input.maxLength) {
          input.setCustomValidity('Description is too long to apply this formatting.');
          input.reportValidity();
          input.setCustomValidity('');
          return;
        }
        input.value = next;
        input.focus();
        input.setSelectionRange(selectStart, selectEnd);
        saveSelection();
        input.dispatchEvent(new Event('input', { bubbles: true }));
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
