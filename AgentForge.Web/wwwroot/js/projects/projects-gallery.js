(() => {
    const search = document.getElementById('projectSearch');
    const cards = Array.from(document.querySelectorAll('.project-card'));
    const empty = document.getElementById('projectEmpty');
    if (!search || !empty) return;
    function update() {
        const query = search.value.trim().toLocaleLowerCase();
        let visible = 0;
        for (const card of cards) {
            const matches = card.dataset.search.includes(query);
            card.style.display = matches ? '' : 'none';
            if (matches) visible++;
        }
        empty.hidden = visible !== 0;
    }
    search.addEventListener('input', update);
    update();
})();
