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
        'quote', 'unordered-list', 'ordered-list', '|',
        'link', 'image', 'table', 'horizontal-rule', 'code', '|',
        'side-by-side', 'fullscreen', '|',
        'guide'
      ],
      previewRender: markdown => DOMPurify.sanitize(editor.markdown(markdown)),
      maxHeight: container.hasAttribute('data-markdown-description') ? '350px' : '650px',
      minHeight: container.hasAttribute('data-markdown-description') ? '160px' : '350px'
    });
    container.markdownEditor = editor;
    if (container.hasAttribute('data-markdown-description')) {
      editor.codemirror.on('change', () => {
        const value = editor.value();
        textarea.value = value;
        textarea.dispatchEvent(new Event('input', { bubbles: true }));
      });
    }
    const previewButton = container.closest('form')?.querySelector('#previewAgentInstructions');
    const editButton = container.closest('form')?.querySelector('#editAgentInstructions');
    const syncInstructionsMode = () => {
      if (!previewButton) return;
      const previewing = editor.codemirror.getWrapperElement().classList.contains('editor-preview-active');
      const form = container.closest('form');
      form?.classList.toggle('is-instructions-preview', previewing);
      previewButton.hidden = previewing;
      if (editButton) editButton.hidden = !previewing;
      form?.querySelectorAll('.editor-toolbar button, .editor-toolbar a').forEach(control => {
        if (control.tagName === 'BUTTON') control.disabled = previewing;
        control.setAttribute('aria-disabled', String(previewing));
        if (control.tagName === 'A') control.tabIndex = previewing ? -1 : 0;
      });
      const save = form?.querySelector('#saveAgentInstructions');
      if (save) {
        save.hidden = previewing;
        save.disabled = previewing;
      }
    };
    if (previewButton) container.syncInstructionsMode = syncInstructionsMode;
    if (previewButton) {
      const setInstructionsMode = previewing => {
        const isPreview = editor.codemirror.getWrapperElement().classList.contains('editor-preview-active');
        if (isPreview !== previewing) EasyMDE.togglePreview(editor);
        syncInstructionsMode();
        if (!previewing) editor.codemirror.refresh();
      };
      container.setInstructionsMode = setInstructionsMode;
      previewButton.addEventListener('click', () => setInstructionsMode(true));
      editButton?.addEventListener('click', () => setInstructionsMode(false));
    }
    // Keep form actions outside EasyMDE's generated statusbar/toolbar.
    // Toolbar controls must never implicitly submit the instructions form.
    if (previewButton) {
      container.querySelectorAll('.editor-toolbar button').forEach(control => {
        control.type = 'button';
      });
    }
    editor.codemirror.on('change', () => {
      textarea.value = editor.value();
      textarea.dispatchEvent(new Event('input', { bubbles: true }));
    });
  });
})();
