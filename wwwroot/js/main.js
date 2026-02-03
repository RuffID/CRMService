document.addEventListener("DOMContentLoaded", () => {
    initSidebar();
});

function initSidebar() {
    const side = document.getElementById("side");
    if (!side) return;

    const toggle = document.getElementById("sideToggle");
    const arrow = document.getElementById("sideArrow");
    const texts = document.querySelectorAll(".side-text");

    const widthExpanded = side.dataset.widthExpanded;
    const widthCollapsed = side.dataset.widthCollapsed;

    let expanded = localStorage.getItem("crm_side_expanded") === "1";
    setExpanded(expanded);

    toggle.addEventListener("click", () => {
        expanded = !expanded;
        setExpanded(expanded);
        localStorage.setItem("crm_side_expanded", expanded ? "1" : "0");
        document.cookie = "crm_side_expanded=" + (expanded ? "1" : "0") + "; path=/; max-age=31536000";
    });

    function setExpanded(expanded) {
        side.style.width = expanded ? widthExpanded : widthCollapsed;
        arrow.textContent = expanded ? "‹" : "›";
        texts.forEach(x => x.classList.toggle("d-none", !expanded));
    }
}