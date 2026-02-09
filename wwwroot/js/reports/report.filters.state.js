document.addEventListener("DOMContentLoaded", () => {
    applySavedFiltersPanelState();
});

function applySavedFiltersPanelState() {
    try {
        const key = "crm_report_filters_expanded_v1";
        const expanded = localStorage.getItem(key) === "1";

        const panel = document.getElementById("filtersPanel");
        const toggle = document.getElementById("filtersToggle");
        if (!panel) return;

        if (expanded) {
            panel.classList.add("show");
            panel.classList.remove("collapse");
            panel.classList.add("collapse");

            if (toggle) toggle.setAttribute("aria-expanded", "true");
        } else {
            panel.classList.remove("show");

            if (toggle) toggle.setAttribute("aria-expanded", "false");
        }
    } catch { }
}