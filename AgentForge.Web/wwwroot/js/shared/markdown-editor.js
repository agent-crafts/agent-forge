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
    const instructionsForm = container.closest('#agentInstructionsForm');
    if (instructionsForm) {
      const editButton = instructionsForm.querySelector('#editAgentInstructions');
      const saveButton = instructionsForm.querySelector('#saveAgentInstructions');
      const setInstructionsMode = preview => {
        const currentlyPreview = editor.isPreviewActive();
        if (currentlyPreview !== preview) EasyMDE.togglePreview(editor);
        instructionsForm.classList.toggle('is-instructions-preview', preview);
        if (saveButton) saveButton.disabled = preview;
        if (!preview) {
          // Restore the editable CodeMirror after EasyMDE exits preview.
          editor.codemirror.setOption('readOnly', false);
          editor.codemirror.refresh();
          editor.codemirror.focus();
        }
      };
      container.setInstructionsMode = setInstructionsMode;
      editButton?.addEventListener('click', () => setInstructionsMode(false));
      setInstructionsMode(true);
    }
    if (container.hasAttribute('data-markdown-description')) {
      editor.codemirror.on('change', () => {
        const value = editor.value();
        textarea.value = value;
        textarea.dispatchEvent(new Event('input', { bubbles: true }));
      });
    }


    // Generated toolbar buttons must not submit the parent form.
    container.querySelectorAll('.editor-toolbar button').forEach(control => {
      control.type = 'button';
    });
    editor.codemirror.on('change', () => {
      textarea.value = editor.value();
      textarea.dispatchEvent(new Event('input', { bubbles: true }));
    });
  });
})();
