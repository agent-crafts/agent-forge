(() => {
    const search = document.getElementById("toolSearch");
    if (!search) return;
    const cards = Array.from(document.querySelectorAll("[data-search]"));
    const empty = document.getElementById("toolSearchEmpty");
    search.addEventListener("input", () => {
        const query = search.value.trim().toLocaleLowerCase();
        let matches = 0;
        cards.forEach(card => {
            const visible = card.dataset.search.includes(query);
            card.hidden = !visible;
            if (visible) matches++;
        });
        if (empty) empty.hidden = matches > 0 || cards.length === 0;
    });
})();
