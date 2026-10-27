(() => {
  document.querySelectorAll('[data-markdown-editor]').forEach(container => {
    const textarea = container.querySelector('textarea');
    if (!textarea || textarea.dataset.initialized) return;
    if (!window.EasyMDE || !window.DOMPurify) {
      console.warn('Markdown editor libraries unavailable; falling back to textarea.');
      return;
    }
    textarea.dataset.initialized = 'true';
    const editor = new EasyMDE({
      element: textarea,
      autoDownloadFontAwesome: true,
      spellChecker: false,
      status: ['lines', 'words', 'cursor'],
      toolbar: [
        'bold', 'italic', 'strikethrough', '|',
        'heading', 'heading-smaller', 'heading-bigger', '|',
        'quote', 'unordered-list', 'ordered-list', 'checklist', '|',
        'link', 'image', 'table', 'horizontal-rule', 'code', '|',
        'side-by-side', 'fullscreen', '|',
        'guide'
      ],
      previewRender: markdown => DOMPurify.sanitize(editor.markdown(markdown)),
      maxHeight: '650px',
      minHeight: '350px'
    });
    container.markdownEditor = editor;
    const previewButton = container.closest('form')?.querySelector('#previewAgentInstructions');
    if (previewButton) {
      previewButton.addEventListener('click', () => {
        EasyMDE.togglePreview(editor);
        const previewing = editor.codemirror.getWrapperElement().classList.contains('editor-preview-active');
        previewButton.textContent = previewing ? 'Edit' : 'Preview';
        previewButton.setAttribute('aria-pressed', String(previewing));
        if (!previewing) editor.codemirror.refresh();
      });
    }
    const actions = container.closest("form")?.querySelector(".agent-instructions-actions");
    const statusbar = container.querySelector(".editor-statusbar");
    if (actions && statusbar) statusbar.appendChild(actions);
    editor.codemirror.on('change', () => {
      textarea.value = editor.value();
      textarea.dispatchEvent(new Event('input', { bubbles: true }));
    });
  });
})();
