(() => {
    const search = document.getElementById('agentSearch');
    const category = document.getElementById('agentCategory');
    const cards = Array.from(document.querySelectorAll('.agent-card'));
    const count = document.getElementById('agentCount');
    const empty = document.getElementById('agentEmpty');
    if (!search || !category || !count || !empty) return;

    function update() {
        const query = search.value.trim().toLocaleLowerCase();
        const selectedCategory = category.value;
        let visible = 0;
        for (const card of cards) {
            const matches = card.dataset.search.includes(query) &&
                (selectedCategory === 'all' || card.dataset.category === selectedCategory);
            card.style.display = matches ? '' : 'none';
            if (matches) visible++;
        }
        count.textContent = visible === 1 ? '1 agent' : visible + ' agents';
        empty.hidden = visible !== 0;
    }
    search.addEventListener('input', update);
    category.addEventListener('change', update);
    update();
})();
