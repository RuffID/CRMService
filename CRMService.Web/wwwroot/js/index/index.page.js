const SPENT_TIME_CHART_KEY = "spent-time-chart";

let ACTIVE_GRAPH_KEY = null;

document.addEventListener("DOMContentLoaded", () => {
    initIndexGraphsPage();
});

function initIndexGraphsPage() {
    const spentTimeChartButton = document.getElementById("openSpentTimeChartButton");
    if (spentTimeChartButton) {
        spentTimeChartButton.addEventListener("click", async () => {
            await showGraph(SPENT_TIME_CHART_KEY);
        });
    }

    applyActiveMenuState();
    showEmptyState();
}

async function showGraph(graphKey) {
    ACTIVE_GRAPH_KEY = graphKey;
    applyActiveMenuState();

    if (graphKey === SPENT_TIME_CHART_KEY) {
        showSpentTimeChartPage();

        if (window.IndexSpentTimeChart?.ensureInitialized) {
            await window.IndexSpentTimeChart.ensureInitialized();
        }

        if (window.IndexSpentTimeChart?.onShow) {
            window.IndexSpentTimeChart.onShow();
        }

        return;
    }

    showEmptyState();
}

function applyActiveMenuState() {
    const spentTimeChartButton = document.getElementById("openSpentTimeChartButton");
    if (!spentTimeChartButton) return;

    spentTimeChartButton.classList.toggle("active", ACTIVE_GRAPH_KEY === SPENT_TIME_CHART_KEY);
}

function showEmptyState() {
    const emptyState = document.getElementById("indexGraphsEmptyState");
    const spentTimeChartPage = document.getElementById("spentTimeChartPage");

    if (emptyState) emptyState.classList.remove("d-none");
    if (spentTimeChartPage) spentTimeChartPage.classList.add("d-none");
}

function showSpentTimeChartPage() {
    const emptyState = document.getElementById("indexGraphsEmptyState");
    const spentTimeChartPage = document.getElementById("spentTimeChartPage");

    if (emptyState) emptyState.classList.add("d-none");
    if (spentTimeChartPage) spentTimeChartPage.classList.remove("d-none");
}