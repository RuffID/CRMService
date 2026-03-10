const ISSUE_DYNAMICS_CHART_FILTERS_STORAGE_KEY = "crm_report_issue_dynamics_chart_filters_v1";
const ISSUE_DYNAMICS_GRANULARITY_HOUR = "hour";
const ISSUE_DYNAMICS_GRANULARITY_DAY = "day";
const ISSUE_DYNAMICS_DEFAULT_CHART_HEIGHT = 620;
const ISSUE_DYNAMICS_CREATED_SERIES_COLOR = "#0d6efd";
const ISSUE_DYNAMICS_COMPLETED_SERIES_COLOR = "#198754";

let ISSUE_DYNAMICS_ANTI_FORGERY_TOKEN = null;
let ISSUE_DYNAMICS_CHART = null;
let IS_ISSUE_DYNAMICS_INITIALIZED = false;
let IS_ISSUE_DYNAMICS_LOADING = false;

document.addEventListener("DOMContentLoaded", () => {
    initIssueDynamicsPageShell();
});

function initIssueDynamicsPageShell() {
    ISSUE_DYNAMICS_ANTI_FORGERY_TOKEN = getRequestVerificationToken();
}

async function initIssueDynamicsChartReport() {
    if (IS_ISSUE_DYNAMICS_INITIALIZED)
        return;

    IS_ISSUE_DYNAMICS_INITIALIZED = true;

    initIssueDynamicsChart();
    initIssueDynamicsFilters();
    restoreIssueDynamicsFiltersState();
    ensureIssueDynamicsDefaultPeriod();
    updateIssueDynamicsSummaryText();

    await loadIssueDynamicsChart();
}

function initIssueDynamicsChart() {
    const chartElement = document.getElementById("issueDynamicsChart");
    if (!chartElement || typeof echarts === "undefined")
        return;

    ISSUE_DYNAMICS_CHART = echarts.init(chartElement);

    window.addEventListener("resize", () => {
        if (ISSUE_DYNAMICS_CHART)
            ISSUE_DYNAMICS_CHART.resize();
    });

    renderIssueDynamicsEmptyChart("Загрузка данных...");
}

function initIssueDynamicsFilters() {
    const granularitySelect = document.getElementById("issueDynamicsGranularitySelect");
    const dateFromInput = document.getElementById("issueDynamicsDateFrom");
    const dateToInput = document.getElementById("issueDynamicsDateTo");
    const applyButton = document.getElementById("btnApplyIssueDynamicsFilters");

    if (granularitySelect) {
        granularitySelect.addEventListener("change", () => {
            saveIssueDynamicsFiltersState();
            updateIssueDynamicsSummaryText();
        });
    }

    if (dateFromInput) {
        dateFromInput.addEventListener("change", () => {
            saveIssueDynamicsFiltersState();
            updateIssueDynamicsSummaryText();
        });
    }

    if (dateToInput) {
        dateToInput.addEventListener("change", () => {
            saveIssueDynamicsFiltersState();
            updateIssueDynamicsSummaryText();
        });
    }

    if (applyButton) {
        applyButton.addEventListener("click", async () => {
            await loadIssueDynamicsChart();
        });
    }
}

function ensureIssueDynamicsDefaultPeriod() {
    const dateFromInput = document.getElementById("issueDynamicsDateFrom");
    const dateToInput = document.getElementById("issueDynamicsDateTo");
    if (!dateFromInput || !dateToInput)
        return;

    if (dateFromInput.value && dateToInput.value)
        return;

    const CURRENT_DATE = new Date();
    const MONTH_AGO_DATE = new Date(CURRENT_DATE);
    MONTH_AGO_DATE.setDate(CURRENT_DATE.getDate() - 30);

    dateFromInput.value = formatIssueDynamicsDateInputValue(MONTH_AGO_DATE);
    dateToInput.value = formatIssueDynamicsDateInputValue(CURRENT_DATE);
}

async function loadIssueDynamicsChart() {
    if (IS_ISSUE_DYNAMICS_LOADING)
        return false;

    const validationMessage = validateIssueDynamicsFilters();
    if (validationMessage) {
        showIssueDynamicsPageError(validationMessage);
        renderIssueDynamicsEmptyChart("Проверьте период.");
        return false;
    }

    try {
        IS_ISSUE_DYNAMICS_LOADING = true;
        clearIssueDynamicsPageError();
        updateIssueDynamicsSummaryText("Загрузка данных...");

        const response = await sendJsonRequest(
            buildIssueDynamicsRequestUrl(),
            "GET",
            buildJsonHeaders(ISSUE_DYNAMICS_ANTI_FORGERY_TOKEN)
        );

        const data = unwrapOrThrow(response, "Ошибка загрузки графика.");
        renderIssueDynamicsChart(data);
        saveIssueDynamicsFiltersState();
        updateIssueDynamicsSummaryText();
        return true;
    } catch (error) {
        console.error(error);
        showIssueDynamicsPageError(error?.message || "Ошибка загрузки графика.");
        renderIssueDynamicsEmptyChart("Не удалось построить график.");
        updateIssueDynamicsSummaryText();
        return false;
    } finally {
        IS_ISSUE_DYNAMICS_LOADING = false;
    }
}

function buildIssueDynamicsRequestUrl() {
    const dateFromValue = String(document.getElementById("issueDynamicsDateFrom")?.value || "");
    const dateToValue = String(document.getElementById("issueDynamicsDateTo")?.value || "");
    const granularityValue = String(document.getElementById("issueDynamicsGranularitySelect")?.value || ISSUE_DYNAMICS_GRANULARITY_DAY);
    const params = new URLSearchParams();

    params.set("handler", "IssueDynamicsChart");
    params.set("dateFrom", `${dateFromValue}T00:00:00`);
    params.set("dateTo", `${dateToValue}T00:00:00`);
    params.set("granularity", granularityValue);

    return `?${params.toString()}`;
}

function validateIssueDynamicsFilters() {
    const dateFromValue = String(document.getElementById("issueDynamicsDateFrom")?.value || "");
    const dateToValue = String(document.getElementById("issueDynamicsDateTo")?.value || "");

    if (!dateFromValue || !dateToValue)
        return "Укажите период.";

    if (dateToValue < dateFromValue)
        return "Дата окончания должна быть больше или равна дате начала.";

    return "";
}

function renderIssueDynamicsChart(chartData) {
    if (!ISSUE_DYNAMICS_CHART)
        return;

    const buckets = Array.isArray(chartData?.buckets) ? chartData.buckets : [];
    const createdValues = normalizeIssueDynamicsValues(chartData?.createdValues, buckets.length);
    const completedValues = normalizeIssueDynamicsValues(chartData?.completedValues, buckets.length);
    const hasData = createdValues.some(value => value > 0) || completedValues.some(value => value > 0);

    if (buckets.length === 0 || !hasData) {
        renderIssueDynamicsEmptyChart("Нет данных за выбранный период.");
        return;
    }

    ISSUE_DYNAMICS_CHART.setOption({
        animation: false,
        grid: {
            left: 56,
            right: 24,
            top: 40,
            bottom: 84
        },
        tooltip: {
            trigger: "axis",
            formatter: (params) => formatIssueDynamicsTooltip(params)
        },
        legend: {
            bottom: 0
        },
        xAxis: {
            type: "time",
            axisLabel: {
                formatter: (value) => formatIssueDynamicsAxisLabel(value)
            }
        },
        yAxis: {
            type: "value",
            name: "Заявки",
            minInterval: 1
        },
        dataZoom: [
            {
                type: "inside",
                throttle: 50
            },
            {
                type: "slider",
                height: 24,
                bottom: 24
            }
        ],
        series: [
            {
                name: "Создано",
                type: "line",
                smooth: false,
                showSymbol: false,
                color: ISSUE_DYNAMICS_CREATED_SERIES_COLOR,
                data: buildIssueDynamicsSeriesPoints(buckets, createdValues)
            },
            {
                name: "Решено",
                type: "line",
                smooth: false,
                showSymbol: false,
                color: ISSUE_DYNAMICS_COMPLETED_SERIES_COLOR,
                data: buildIssueDynamicsSeriesPoints(buckets, completedValues)
            }
        ]
    }, true);

    ISSUE_DYNAMICS_CHART.resize({ height: ISSUE_DYNAMICS_DEFAULT_CHART_HEIGHT });
}

function renderIssueDynamicsEmptyChart(message) {
    if (!ISSUE_DYNAMICS_CHART)
        return;

    ISSUE_DYNAMICS_CHART.setOption({
        animation: false,
        title: {
            left: "center",
            top: "middle",
            text: message,
            textStyle: {
                color: "#6c757d",
                fontSize: 16,
                fontWeight: "normal"
            }
        },
        xAxis: {
            type: "time",
            show: false
        },
        yAxis: {
            type: "value",
            show: false
        },
        series: []
    }, true);

    ISSUE_DYNAMICS_CHART.resize({ height: ISSUE_DYNAMICS_DEFAULT_CHART_HEIGHT });
}

function updateIssueDynamicsSummaryText(customText) {
    const summaryElement = document.getElementById("issueDynamicsSummaryText");
    if (!summaryElement)
        return;

    if (customText) {
        summaryElement.textContent = customText;
        return;
    }

    const granularityValue = String(document.getElementById("issueDynamicsGranularitySelect")?.value || ISSUE_DYNAMICS_GRANULARITY_DAY);
    const dateFromValue = String(document.getElementById("issueDynamicsDateFrom")?.value || "");
    const dateToValue = String(document.getElementById("issueDynamicsDateTo")?.value || "");
    const granularityText = granularityValue === ISSUE_DYNAMICS_GRANULARITY_HOUR ? "по часам" : "по дням";

    summaryElement.textContent = `Период: ${dateFromValue} - ${dateToValue}, шаг: ${granularityText}.`;
}

function showIssueDynamicsPageError(message) {
    const pageError = document.getElementById("issueDynamicsPageError");
    if (!pageError)
        return;

    pageError.textContent = message || "Ошибка загрузки данных.";
    pageError.classList.remove("d-none");
}

function clearIssueDynamicsPageError() {
    const pageError = document.getElementById("issueDynamicsPageError");
    if (!pageError)
        return;

    pageError.textContent = "";
    pageError.classList.add("d-none");
}

function normalizeIssueDynamicsValues(values, expectedLength) {
    const normalizedValues = Array.isArray(values) ? values : [];
    const result = [];

    for (let index = 0; index < expectedLength; index++) {
        result.push(Number(normalizedValues[index] || 0));
    }

    return result;
}

function buildIssueDynamicsSeriesPoints(buckets, values) {
    const result = [];

    for (let index = 0; index < buckets.length; index++) {
        result.push([String(buckets[index] || ""), Number(values[index] || 0)]);
    }

    return result;
}

function formatIssueDynamicsTooltip(params) {
    const items = Array.isArray(params) ? params : [];
    if (items.length === 0)
        return "Нет данных";

    const lines = [`<div style="font-weight:600;margin-bottom:6px;">${escapeIssueDynamicsHtml(formatIssueDynamicsTooltipDate(items[0]?.axisValue))}</div>`];

    for (const item of items) {
        const color = escapeIssueDynamicsHtml(String(item?.color || "#0d6efd"));
        const name = escapeIssueDynamicsHtml(String(item?.seriesName || ""));
        const value = escapeIssueDynamicsHtml(String(Number(item?.value?.[1] ?? item?.value ?? 0)));

        lines.push(
            `<div style="display:flex;align-items:center;justify-content:space-between;gap:12px;margin-bottom:2px;">` +
            `<div style="display:flex;align-items:center;gap:6px;">` +
            `<span style="width:10px;height:10px;display:inline-block;border-radius:999px;background:${color};"></span>` +
            `<span>${name}</span>` +
            `</div>` +
            `<span style="font-weight:600;">${value}</span>` +
            `</div>`
        );
    }

    return lines.join("");
}

function formatIssueDynamicsTooltipDate(value) {
    const date = new Date(value);
    const granularityValue = String(document.getElementById("issueDynamicsGranularitySelect")?.value || ISSUE_DYNAMICS_GRANULARITY_DAY);

    if (granularityValue === ISSUE_DYNAMICS_GRANULARITY_HOUR)
        return `${padIssueDynamicsNumber(date.getDate())}.${padIssueDynamicsNumber(date.getMonth() + 1)}.${date.getFullYear()} ${padIssueDynamicsNumber(date.getHours())}:00`;

    return `${padIssueDynamicsNumber(date.getDate())}.${padIssueDynamicsNumber(date.getMonth() + 1)}.${date.getFullYear()}`;
}

function formatIssueDynamicsAxisLabel(value) {
    const date = new Date(value);
    const granularityValue = String(document.getElementById("issueDynamicsGranularitySelect")?.value || ISSUE_DYNAMICS_GRANULARITY_DAY);

    if (granularityValue === ISSUE_DYNAMICS_GRANULARITY_HOUR)
        return `${padIssueDynamicsNumber(date.getDate())}.${padIssueDynamicsNumber(date.getMonth() + 1)} ${padIssueDynamicsNumber(date.getHours())}:00`;

    return `${padIssueDynamicsNumber(date.getDate())}.${padIssueDynamicsNumber(date.getMonth() + 1)}.${date.getFullYear()}`;
}

function formatIssueDynamicsDateInputValue(date) {
    return `${date.getFullYear()}-${padIssueDynamicsNumber(date.getMonth() + 1)}-${padIssueDynamicsNumber(date.getDate())}`;
}

function padIssueDynamicsNumber(value) {
    return String(value).padStart(2, "0");
}

function saveIssueDynamicsFiltersState() {
    const state = {
        granularity: String(document.getElementById("issueDynamicsGranularitySelect")?.value || ISSUE_DYNAMICS_GRANULARITY_DAY),
        dateFrom: String(document.getElementById("issueDynamicsDateFrom")?.value || ""),
        dateTo: String(document.getElementById("issueDynamicsDateTo")?.value || "")
    };

    localStorage.setItem(ISSUE_DYNAMICS_CHART_FILTERS_STORAGE_KEY, JSON.stringify(state));
}

function restoreIssueDynamicsFiltersState() {
    const state = readIssueDynamicsFiltersState();
    if (!state)
        return;

    const granularitySelect = document.getElementById("issueDynamicsGranularitySelect");
    const dateFromInput = document.getElementById("issueDynamicsDateFrom");
    const dateToInput = document.getElementById("issueDynamicsDateTo");

    if (granularitySelect && (state.granularity === ISSUE_DYNAMICS_GRANULARITY_HOUR || state.granularity === ISSUE_DYNAMICS_GRANULARITY_DAY))
        granularitySelect.value = state.granularity;

    if (dateFromInput && isValidIssueDynamicsDateInputValue(state.dateFrom))
        dateFromInput.value = state.dateFrom;

    if (dateToInput && isValidIssueDynamicsDateInputValue(state.dateTo))
        dateToInput.value = state.dateTo;
}

function readIssueDynamicsFiltersState() {
    const rawValue = localStorage.getItem(ISSUE_DYNAMICS_CHART_FILTERS_STORAGE_KEY);
    if (!rawValue)
        return null;

    try {
        const parsedValue = JSON.parse(rawValue);
        return parsedValue && typeof parsedValue === "object" ? parsedValue : null;
    } catch {
        return null;
    }
}

function isValidIssueDynamicsDateInputValue(value) {
    return /^\d{4}-\d{2}-\d{2}$/.test(String(value || ""));
}

function escapeIssueDynamicsHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

function resizeIssueDynamicsChart() {
    if (ISSUE_DYNAMICS_CHART)
        ISSUE_DYNAMICS_CHART.resize();
}

window.ReportIssueDynamicsChart = {
    ensureInitialized: initIssueDynamicsChartReport,
    onShow: resizeIssueDynamicsChart
};
