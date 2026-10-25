(() => {
    const search = document.getElementById("toolSearch");
    const category = document.getElementById("toolCategory");
    if (!search || !category) return;

    const cards = Array.from(document.querySelectorAll("[data-search]"));
    const empty = document.getElementById("toolSearchEmpty");

    const filter = () => {
        const query = search.value.trim().toLocaleLowerCase();
        const selectedCategory = category.value;
        let matches = 0;

        cards.forEach(card => {
            const matchesText = card.dataset.search.includes(query);
            const matchesCategory = selectedCategory === "all" || card.dataset.category === selectedCategory;
            const visible = matchesText && matchesCategory;
            card.hidden = !visible;
            if (visible) matches++;
        });

        if (empty) {
            empty.hidden = matches > 0 || cards.length === 0;
            if (!empty.hidden) {
                empty.textContent = selectedCategory !== "all" && !query
                    ? "No registered tools in this category yet."
                    : "No tools match your filters.";
            }
        }
    };

    search.addEventListener("input", filter);
    category.addEventListener("change", filter);
    filter();
})();
