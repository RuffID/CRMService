const REPORT_VIEW_STORAGE_KEY = "crm_reports_active_view_v1";
const EMPLOYEE_PRODUCTIVITY_REPORT_VIEW = "employee-productivity-report";
const SPENT_TIME_CHART_REPORT_VIEW = "spent-time-chart-report";

let ACTIVE_REPORT_VIEW = null;

document.addEventListener("DOMContentLoaded", () => {
    initReportsPage();
});

function initReportsPage() {
    const openPerformanceReportButton = document.getElementById("openPerformanceReportButton");
    const openSpentTimeChartReportButton = document.getElementById("openSpentTimeChartReportButton");

    if (openPerformanceReportButton) {
        openPerformanceReportButton.addEventListener("click", async () => {
            await showReportView(EMPLOYEE_PRODUCTIVITY_REPORT_VIEW);
        });
    }

    if (openSpentTimeChartReportButton) {
        openSpentTimeChartReportButton.addEventListener("click", async () => {
            await showReportView(SPENT_TIME_CHART_REPORT_VIEW);
        });
    }

    const savedReportView = readActiveReportView();
    const initialReportView = savedReportView || EMPLOYEE_PRODUCTIVITY_REPORT_VIEW;

    showReportView(initialReportView);
}

async function showReportView(reportView) {
    ACTIVE_REPORT_VIEW = reportView === SPENT_TIME_CHART_REPORT_VIEW
        ? SPENT_TIME_CHART_REPORT_VIEW
        : EMPLOYEE_PRODUCTIVITY_REPORT_VIEW;

    saveActiveReportView(ACTIVE_REPORT_VIEW);
    applyActiveReportMenuState();
    toggleReportPages();

    if (ACTIVE_REPORT_VIEW === SPENT_TIME_CHART_REPORT_VIEW) {
        if (window.ReportSpentTimeChart?.ensureInitialized) {
            await window.ReportSpentTimeChart.ensureInitialized();
        }

        if (window.ReportSpentTimeChart?.onShow) {
            window.ReportSpentTimeChart.onShow();
        }
    }
}

function toggleReportPages() {
    const performanceReportPage = document.getElementById("performanceReportPage");
    const spentTimeChartReportPage = document.getElementById("spentTimeChartReportPage");
    const isPerformanceReportActive = ACTIVE_REPORT_VIEW === EMPLOYEE_PRODUCTIVITY_REPORT_VIEW;

    if (performanceReportPage) {
        performanceReportPage.classList.toggle("d-none", !isPerformanceReportActive);
    }

    if (spentTimeChartReportPage) {
        spentTimeChartReportPage.classList.toggle("d-none", isPerformanceReportActive);
    }
}

function applyActiveReportMenuState() {
    const openPerformanceReportButton = document.getElementById("openPerformanceReportButton");
    const openSpentTimeChartReportButton = document.getElementById("openSpentTimeChartReportButton");

    if (openPerformanceReportButton) {
        openPerformanceReportButton.classList.toggle("active", ACTIVE_REPORT_VIEW === EMPLOYEE_PRODUCTIVITY_REPORT_VIEW);
    }

    if (openSpentTimeChartReportButton) {
        openSpentTimeChartReportButton.classList.toggle("active", ACTIVE_REPORT_VIEW === SPENT_TIME_CHART_REPORT_VIEW);
    }
}

function saveActiveReportView(reportView) {
    localStorage.setItem(REPORT_VIEW_STORAGE_KEY, reportView);
}

function readActiveReportView() {
    const savedValue = localStorage.getItem(REPORT_VIEW_STORAGE_KEY);

    if (savedValue === SPENT_TIME_CHART_REPORT_VIEW) {
        return SPENT_TIME_CHART_REPORT_VIEW;
    }

    if (savedValue === EMPLOYEE_PRODUCTIVITY_REPORT_VIEW) {
        return EMPLOYEE_PRODUCTIVITY_REPORT_VIEW;
    }

    return null;
}

window.getActiveReportView = () => ACTIVE_REPORT_VIEW;
window.isActiveReportView = (reportView) => ACTIVE_REPORT_VIEW === reportView;
