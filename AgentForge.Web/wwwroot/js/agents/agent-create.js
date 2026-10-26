(() => {
    const form = document.getElementById('agentCreateForm');
    if (!form) return;
    const name = form.elements.namedItem('name');
    const category = form.elements.namedItem('category');
    const description = form.elements.namedItem('description');
    const icon = form.elements.namedItem('icon');
    const tags = form.elements.namedItem('tags');
    const summary = document.getElementById('agentPreviewSummary');
    const updatePreview = () => {
        document.getElementById('agentPreviewName').textContent = name.value.trim() || 'New Agent';
        document.getElementById('agentPreviewDescription').textContent = description.value.trim() || 'Your agent description will appear here.';
        document.getElementById('agentPreviewCategory').textContent = category.value || 'Uncategorized';
        document.getElementById('agentPreviewIcon').textContent = icon.value;
        const tagContainer = document.getElementById('agentPreviewTags');
        tagContainer.replaceChildren();
        tags.value.split(',').map(tag => tag.trim()).filter(Boolean).slice(0, 8).forEach(tag => {
            const chip = document.createElement('span');
            chip.textContent = tag;
            tagContainer.appendChild(chip);
        });
        summary.hidden = true;
    };
    form.addEventListener('input', updatePreview);
    form.addEventListener('change', updatePreview);
    form.addEventListener('reset', () => requestAnimationFrame(updatePreview));
    form.addEventListener('submit', event => {
        event.preventDefault();
        if (!form.reportValidity()) return;
        summary.hidden = false;
        summary.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    });
    updatePreview();
})();
