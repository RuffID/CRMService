const SCOPE_EMPLOYEE = "employee";
const SCOPE_GROUP = "group";
const AXIS_CREATED_AT = "createdAt";
const AXIS_LOGGED_AT = "loggedAt";
const GRANULARITY_HOUR = "hour";
const GRANULARITY_DAY = "day";
const DEFAULT_CHART_HEIGHT = 620;
const ACTIVE_SERIES_OPACITY = 1;
const DIMMED_SERIES_OPACITY = 0.15;
const CHART_FILTERS_STORAGE_KEY = "crm_report_spent_time_chart_filters_v1";

let CHART_ANTI_FORGERY_TOKEN = null;
let REPORT_SPENT_TIME_CHART = null;
let CHART_ALL_GROUPS = [];
let CHART_ALL_EMPLOYEES = [];
let CHART_SELECTED_GROUP_IDS = new Set();
let CHART_SELECTED_EMPLOYEE_IDS = new Set();
let IS_REPORT_CHART_LOADING = false;
let LAST_REPORT_CHART_DATA = null;
let LAST_SERIES_COLORS = [];
let LAST_REPORT_ACTIVE_SERIES_ID = null;
let LAST_REPORT_DATA_ZOOM_STATE = null;
let REPORT_VISIBLE_EMPLOYEE_IDS = null;
let IS_REPORT_CHART_INITIALIZED = false;

async function initReportSpentTimeChart() {
    if (IS_REPORT_CHART_INITIALIZED)
        return;

    IS_REPORT_CHART_INITIALIZED = true;
    CHART_ANTI_FORGERY_TOKEN = getRequestVerificationToken();

    initReportChart();
    initReportChartFilters();
    restoreReportChartFiltersState();
    ensureReportChartDefaultPeriod();
    updateReportChartScopeVisibility();

    await loadReportChartGroups();
    await loadReportChartEmployees();
    await loadSpentTimeChartReport();
}

function initReportChart() {
    const chartElement = document.getElementById("reportSpentTimeChart");
    if (!chartElement || typeof echarts === "undefined") return;

    REPORT_SPENT_TIME_CHART = echarts.init(chartElement);
    REPORT_SPENT_TIME_CHART.on("click", (params) => {
        if (!params || params.componentType !== "series") return;

        LAST_REPORT_ACTIVE_SERIES_ID = String(params.seriesId || "");
        applyReportChartInteractiveState();
    });

    REPORT_SPENT_TIME_CHART.on("datazoom", () => {
        LAST_REPORT_DATA_ZOOM_STATE = readCurrentReportChartDataZoomState();
    });

    window.addEventListener("resize", () => {
        if (REPORT_SPENT_TIME_CHART) REPORT_SPENT_TIME_CHART.resize();
    });

    initReportChartLegendControls();
    renderEmptyReportChart("Загрузка данных...");
    renderReportChartLegend([]);
}

function resizeReportChart() {
    if (REPORT_SPENT_TIME_CHART)
        REPORT_SPENT_TIME_CHART.resize();
}

function initReportChartLegendControls() {
    const clearButton = document.getElementById("chartClearLegendSelectionButton");
    if (!clearButton) return;

    clearButton.addEventListener("click", () => {
        LAST_REPORT_ACTIVE_SERIES_ID = null;
        applyReportChartInteractiveState();
    });
}

function initReportChartFilters() {
    const scopeSelect = document.getElementById("chartScopeSelect");
    const timeAxisSelect = document.getElementById("chartTimeAxisSelect");
    const granularitySelect = document.getElementById("chartGranularitySelect");
    const btnApply = document.getElementById("btnApplySpentTimeChartFilters");
    const groupsSearch = document.getElementById("chartGroupsSearch");
    const employeesSearch = document.getElementById("chartEmployeesSearch");
    const groupsSelectAll = document.getElementById("chartGroupsSelectAll");
    const groupsClearAll = document.getElementById("chartGroupsClearAll");
    const employeesSelectAll = document.getElementById("chartEmployeesSelectAll");
    const employeesClearAll = document.getElementById("chartEmployeesClearAll");

    if (scopeSelect) {
        scopeSelect.addEventListener("change", () => {
            saveReportChartFiltersState();
            updateReportChartScopeVisibility();
            updateReportChartSummaryText();
        });
    }

    if (timeAxisSelect) {
        timeAxisSelect.addEventListener("change", () => {
            saveReportChartFiltersState();
            updateReportChartSummaryText();
        });
    }

    if (granularitySelect) {
        granularitySelect.addEventListener("change", () => {
            saveReportChartFiltersState();
            updateReportChartSummaryText();
        });
    }

    const dateFrom = document.getElementById("chartDateFrom");
    const dateTo = document.getElementById("chartDateTo");

    if (dateFrom) {
        dateFrom.addEventListener("change", () => {
            saveReportChartFiltersState();
            updateReportChartSummaryText();
        });
    }

    if (dateTo) {
        dateTo.addEventListener("change", () => {
            saveReportChartFiltersState();
            updateReportChartSummaryText();
        });
    }

    if (btnApply) {
        btnApply.addEventListener("click", async () => {
            await loadSpentTimeChartReport();
        });
    }

    if (groupsSearch) {
        groupsSearch.addEventListener("input", () => {
            renderReportChartGroupFilter();
        });
    }

    if (employeesSearch) {
        employeesSearch.addEventListener("input", () => {
            renderReportChartEmployeeFilter();
        });
    }

    if (groupsSelectAll) {
        groupsSelectAll.addEventListener("click", async () => {
            CHART_SELECTED_GROUP_IDS = new Set(CHART_ALL_GROUPS.map(x => Number(x.id)));
            saveReportChartFiltersState();
            renderReportChartGroupFilter();
            await loadReportChartEmployees();
            updateReportChartSummaryText();
        });
    }

    if (groupsClearAll) {
        groupsClearAll.addEventListener("click", async () => {
            CHART_SELECTED_GROUP_IDS = new Set();
            saveReportChartFiltersState();
            renderReportChartGroupFilter();
            await loadReportChartEmployees();
            updateReportChartSummaryText();
        });
    }

    if (employeesSelectAll) {
        employeesSelectAll.addEventListener("click", () => {
            CHART_SELECTED_EMPLOYEE_IDS = new Set(CHART_ALL_EMPLOYEES.map(x => Number(x.id)));
            saveReportChartFiltersState();
            renderReportChartEmployeeFilter();
            updateReportChartSummaryText();
        });
    }

    if (employeesClearAll) {
        employeesClearAll.addEventListener("click", () => {
            CHART_SELECTED_EMPLOYEE_IDS = new Set();
            saveReportChartFiltersState();
            renderReportChartEmployeeFilter();
            updateReportChartSummaryText();
        });
    }
}

function ensureReportChartDefaultPeriod() {
    const dateFrom = document.getElementById("chartDateFrom");
    const dateTo = document.getElementById("chartDateTo");
    if (!dateFrom || !dateTo) return;
    if (dateFrom.value && dateTo.value) return;

    const currentDate = new Date();
    const monthAgoDate = new Date(currentDate);
    monthAgoDate.setDate(currentDate.getDate() - 30);

    dateFrom.value = formatReportChartDateInputValue(monthAgoDate);
    dateTo.value = formatReportChartDateInputValue(currentDate);
}

async function loadReportChartGroups() {
    try {
        clearReportChartPageError();

        const response = await sendJsonRequest(
            "?handler=EmployeeGroupList",
            "GET",
            buildJsonHeaders(CHART_ANTI_FORGERY_TOKEN)
        );

        const data = unwrapOrThrow(response, "Ошибка загрузки групп.");
        CHART_ALL_GROUPS = Array.isArray(data)
            ? data
                .filter(x => x && Number.isFinite(Number(x.id)))
                .sort((a, b) => String(a.name || "").localeCompare(String(b.name || ""), "ru"))
            : [];

        renderReportChartGroupFilter();
        updateReportChartSummaryText();
        return true;
    } catch (error) {
        console.error(error);
        showReportChartPageError(error?.message || "Ошибка загрузки групп.");
        CHART_ALL_GROUPS = [];
        renderReportChartGroupFilter();
        return false;
    }
}

async function loadReportChartEmployees() {
    try {
        clearReportChartPageError();

        const payload = {
            groupIds: CHART_SELECTED_GROUP_IDS.size > 0 ? Array.from(CHART_SELECTED_GROUP_IDS) : null
        };

        const response = await sendJsonRequest(
            "?handler=EmployeeList",
            "POST",
            buildJsonHeaders(CHART_ANTI_FORGERY_TOKEN),
            payload
        );

        const data = unwrapOrThrow(response, "Ошибка загрузки сотрудников.");
        CHART_ALL_EMPLOYEES = Array.isArray(data)
            ? data
                .filter(x => x && Number.isFinite(Number(x.id)))
                .sort((a, b) => getReportChartEmployeeDisplayName(a).localeCompare(getReportChartEmployeeDisplayName(b), "ru"))
            : [];

        CHART_SELECTED_EMPLOYEE_IDS = new Set(
            Array.from(CHART_SELECTED_EMPLOYEE_IDS).filter(id => CHART_ALL_EMPLOYEES.some(employee => Number(employee.id) === id))
        );

        renderReportChartEmployeeFilter();
        updateReportChartSummaryText();
        return true;
    } catch (error) {
        console.error(error);
        showReportChartPageError(error?.message || "Ошибка загрузки сотрудников.");
        CHART_ALL_EMPLOYEES = [];
        CHART_SELECTED_EMPLOYEE_IDS = new Set();
        renderReportChartEmployeeFilter();
        return false;
    }
}

async function loadSpentTimeChartReport() {
    if (IS_REPORT_CHART_LOADING) return false;

    const validationMessage = validateReportChartFilters();
    if (validationMessage) {
        showReportChartPageError(validationMessage);
        renderEmptyReportChart("Проверьте фильтры.");
        renderReportChartLegend([]);
        return false;
    }

    try {
        IS_REPORT_CHART_LOADING = true;
        clearReportChartPageError();
        updateReportChartSummaryText("Загрузка данных...");
        LAST_REPORT_DATA_ZOOM_STATE = readCurrentReportChartDataZoomState();

        const response = await sendJsonRequest(
            "?handler=TimeChart",
            "POST",
            buildJsonHeaders(CHART_ANTI_FORGERY_TOKEN),
            buildReportChartPayload()
        );

        const data = unwrapOrThrow(response, "Ошибка загрузки графика.");
        LAST_REPORT_ACTIVE_SERIES_ID = null;
        updateVisibleReportChartEmployeeIds(data);
        renderSpentTimeChartReport(data);
        renderReportChartEmployeeFilter();
        saveReportChartFiltersState();
        updateReportChartSummaryText();
        return true;
    } catch (error) {
        console.error(error);
        showReportChartPageError(error?.message || "Ошибка загрузки графика.");
        renderEmptyReportChart("Не удалось построить график.");
        renderReportChartLegend([]);
        REPORT_VISIBLE_EMPLOYEE_IDS = null;
        renderReportChartEmployeeFilter();
        updateReportChartSummaryText();
        return false;
    } finally {
        IS_REPORT_CHART_LOADING = false;
    }
}

function buildReportChartPayload() {
    const dateFromValue = String(document.getElementById("chartDateFrom")?.value || "");
    const dateToValue = String(document.getElementById("chartDateTo")?.value || "");
    const scopeValue = String(document.getElementById("chartScopeSelect")?.value || SCOPE_EMPLOYEE);
    const timeAxisValue = String(document.getElementById("chartTimeAxisSelect")?.value || AXIS_CREATED_AT);
    const granularityValue = String(document.getElementById("chartGranularitySelect")?.value || GRANULARITY_DAY);

    return {
        dateFrom: `${dateFromValue}T00:00:00`,
        dateTo: `${dateToValue}T00:00:00`,
        scope: scopeValue,
        timeAxis: timeAxisValue,
        granularity: granularityValue,
        employeeIds: scopeValue === SCOPE_EMPLOYEE && CHART_SELECTED_EMPLOYEE_IDS.size > 0 ? Array.from(CHART_SELECTED_EMPLOYEE_IDS) : null,
        groupIds: CHART_SELECTED_GROUP_IDS.size > 0 ? Array.from(CHART_SELECTED_GROUP_IDS) : null
    };
}

function validateReportChartFilters() {
    const dateFromValue = String(document.getElementById("chartDateFrom")?.value || "");
    const dateToValue = String(document.getElementById("chartDateTo")?.value || "");

    if (!dateFromValue || !dateToValue)
        return "Укажите период.";

    if (dateToValue < dateFromValue)
        return "Дата окончания должна быть больше или равна дате начала.";

    return "";
}

function renderSpentTimeChartReport(chartData) {
    if (!REPORT_SPENT_TIME_CHART) return;

    const buckets = Array.isArray(chartData?.buckets) ? chartData.buckets : [];
    const seriesItems = Array.isArray(chartData?.series)
        ? chartData.series.filter(seriesItem => getReportChartSeriesTotal(seriesItem) > 0)
        : [];

    if (buckets.length === 0 || seriesItems.length === 0) {
        LAST_REPORT_CHART_DATA = null;
        LAST_SERIES_COLORS = [];
        renderEmptyReportChart("Нет данных за выбранный период.");
        renderReportChartLegend([]);
        updateReportChartLegendSelectionButton();
        return;
    }

    const seriesColors = buildReportChartSeriesColors(seriesItems);
    LAST_REPORT_CHART_DATA = chartData;
    LAST_SERIES_COLORS = seriesColors;

    const option = {
        animation: false,
        grid: {
            left: 56,
            right: 24,
            top: 40,
            bottom: 84
        },
        tooltip: {
            trigger: "axis",
            formatter: (params) => formatReportChartTooltip(params)
        },
        xAxis: {
            type: "time",
            axisLabel: {
                formatter: (value) => formatReportChartAxisLabel(value)
            }
        },
        yAxis: {
            type: "value",
            name: "Часы",
            axisLabel: {
                formatter: (value) => formatReportChartSpentTime(Number(value))
            }
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
        series: seriesItems.map((seriesItem, index) => {
            return {
                id: String(seriesItem.id || ""),
                name: String(seriesItem.name || ""),
                type: "line",
                color: seriesColors[index],
                smooth: false,
                showSymbol: false,
                connectNulls: true,
                emphasis: {
                    disabled: true
                },
                select: {
                    disabled: true
                },
                data: buildReportChartSeriesPoints(buckets, seriesItem.values)
            };
        })
    };

    REPORT_SPENT_TIME_CHART.setOption(option, true);
    REPORT_SPENT_TIME_CHART.resize({ height: DEFAULT_CHART_HEIGHT });
    applySavedReportChartDataZoomState();
    renderReportChartLegend(seriesItems, seriesColors);
    applyReportChartInteractiveState();
}

function renderEmptyReportChart(message) {
    if (!REPORT_SPENT_TIME_CHART) return;

    REPORT_SPENT_TIME_CHART.setOption({
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

    REPORT_SPENT_TIME_CHART.resize({ height: DEFAULT_CHART_HEIGHT });
}

function renderReportChartGroupFilter() {
    const listElement = document.getElementById("chartGroupsList");
    const badgeElement = document.getElementById("chartGroupsBadge");
    const hintElement = document.getElementById("chartGroupsHint");
    const searchValue = String(document.getElementById("chartGroupsSearch")?.value || "").trim().toLowerCase();

    if (badgeElement) badgeElement.textContent = String(CHART_SELECTED_GROUP_IDS.size);
    if (!listElement) return;

    listElement.textContent = "";

    const items = CHART_ALL_GROUPS.filter(group => String(group.name || "").toLowerCase().includes(searchValue));
    if (hintElement) {
        const hiddenCount = CHART_ALL_GROUPS.length - items.length;
        hintElement.textContent = hiddenCount > 0 ? `Скрыто элементов: ${hiddenCount}` : "";
        hintElement.classList.toggle("d-none", hiddenCount === 0);
    }

    if (items.length === 0) {
        appendReportChartEmptyState(listElement, "Группы не найдены.");
        return;
    }

    for (const group of items) {
        const groupId = Number(group.id);
        const inputId = `group_filter_${groupId}`;

        const wrapper = document.createElement("div");
        wrapper.className = "form-check";

        const input = document.createElement("input");
        input.className = "form-check-input";
        input.type = "checkbox";
        input.id = inputId;
        input.name = inputId;
        input.value = String(groupId);
        input.checked = CHART_SELECTED_GROUP_IDS.has(groupId);
        input.addEventListener("change", async () => {
            if (input.checked) CHART_SELECTED_GROUP_IDS.add(groupId);
            else CHART_SELECTED_GROUP_IDS.delete(groupId);

            saveReportChartFiltersState();
            renderReportChartGroupFilter();
            await loadReportChartEmployees();
            updateReportChartSummaryText();
        });

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = inputId;
        label.textContent = String(group.name || `Группа ${groupId}`);

        wrapper.append(input, label);
        listElement.appendChild(wrapper);
    }
}

function renderReportChartEmployeeFilter() {
    const listElement = document.getElementById("chartEmployeesList");
    const badgeElement = document.getElementById("chartEmployeesBadge");
    const hintElement = document.getElementById("chartEmployeesHint");
    const searchValue = String(document.getElementById("chartEmployeesSearch")?.value || "").trim().toLowerCase();

    if (badgeElement) badgeElement.textContent = String(CHART_SELECTED_EMPLOYEE_IDS.size);
    if (!listElement) return;

    listElement.textContent = "";

    const items = CHART_ALL_EMPLOYEES
        .filter(employee => isReportChartEmployeeVisibleInFilters(employee))
        .filter(employee => getReportChartEmployeeDisplayName(employee).toLowerCase().includes(searchValue));
    if (hintElement) {
        const hiddenCount = CHART_ALL_EMPLOYEES.length - items.length;
        hintElement.textContent = hiddenCount > 0 ? `Скрыто элементов: ${hiddenCount}` : "";
        hintElement.classList.toggle("d-none", hiddenCount === 0);
    }

    if (items.length === 0) {
        appendReportChartEmptyState(listElement, "Сотрудники не найдены.");
        return;
    }

    for (const employee of items) {
        const employeeId = Number(employee.id);
        const inputId = `employee_filter_${employeeId}`;

        const wrapper = document.createElement("div");
        wrapper.className = "form-check";

        const input = document.createElement("input");
        input.className = "form-check-input";
        input.type = "checkbox";
        input.id = inputId;
        input.name = inputId;
        input.value = String(employeeId);
        input.checked = CHART_SELECTED_EMPLOYEE_IDS.has(employeeId);
        input.addEventListener("change", () => {
            if (input.checked) CHART_SELECTED_EMPLOYEE_IDS.add(employeeId);
            else CHART_SELECTED_EMPLOYEE_IDS.delete(employeeId);

            saveReportChartFiltersState();
            renderReportChartEmployeeFilter();
            updateReportChartSummaryText();
        });

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = inputId;
        label.textContent = getReportChartEmployeeDisplayName(employee);

        wrapper.append(input, label);
        listElement.appendChild(wrapper);
    }
}

function appendReportChartEmptyState(container, message) {
    const stateElement = document.createElement("div");
    stateElement.className = "text-muted small px-1 py-2";
    stateElement.textContent = message;
    container.appendChild(stateElement);
}

function updateReportChartScopeVisibility() {
    const scopeValue = String(document.getElementById("chartScopeSelect")?.value || SCOPE_EMPLOYEE);
    const employeeFilterColumn = document.getElementById("chartEmployeeFilterColumn");

    if (!employeeFilterColumn) return;
    employeeFilterColumn.classList.toggle("d-none", scopeValue !== SCOPE_EMPLOYEE);
}

function updateReportChartSummaryText(customText) {
    const summaryElement = document.getElementById("chartSummaryText");
    if (!summaryElement) return;

    if (customText) {
        summaryElement.textContent = customText;
        return;
    }

    const scopeValue = String(document.getElementById("chartScopeSelect")?.value || SCOPE_EMPLOYEE);
    const timeAxisValue = String(document.getElementById("chartTimeAxisSelect")?.value || AXIS_CREATED_AT);
    const granularityValue = String(document.getElementById("chartGranularitySelect")?.value || GRANULARITY_DAY);
    const selectedCount = scopeValue === SCOPE_GROUP ? CHART_SELECTED_GROUP_IDS.size : CHART_SELECTED_EMPLOYEE_IDS.size;
    const scopeText = scopeValue === SCOPE_GROUP ? "группы" : "сотрудники";
    const axisText = getReportChartTimeAxisText(timeAxisValue);
    const granularityText = granularityValue === GRANULARITY_HOUR
        ? "по часам"
        : "по дням";
    const selectionText = selectedCount > 0 ? `выбрано: ${selectedCount}` : "выбраны все";

    summaryElement.textContent = `Режим: ${scopeText}, ось времени: ${axisText}, шаг: ${granularityText}, ${selectionText}.`;
}

function showReportChartPageError(message) {
    const pageError = document.getElementById("chartPageError");
    if (!pageError) return;

    pageError.textContent = message || "Ошибка загрузки данных.";
    pageError.classList.remove("d-none");
}

function clearReportChartPageError() {
    const pageError = document.getElementById("chartPageError");
    if (!pageError) return;

    pageError.textContent = "";
    pageError.classList.add("d-none");
}

function getReportChartEmployeeDisplayName(employee) {
    const parts = [
        String(employee?.last_name || employee?.lastName || "").trim(),
        String(employee?.first_name || employee?.firstName || "").trim(),
        String(employee?.patronymic || "").trim()
    ].filter(Boolean);

    return parts.length > 0 ? parts.join(" ") : String(employee?.id || "");
}

function isReportChartEmployeeVisibleInFilters(employee) {
    if (!(REPORT_VISIBLE_EMPLOYEE_IDS instanceof Set))
        return true;

    return REPORT_VISIBLE_EMPLOYEE_IDS.has(Number(employee?.id));
}

function renderReportChartLegend(seriesItems, seriesColors) {
    const legendElement = document.getElementById("chartLegendList");
    if (!legendElement) return;

    legendElement.textContent = "";

    if (!Array.isArray(seriesItems) || seriesItems.length === 0) {
        appendReportChartEmptyState(legendElement, "Нет элементов для отображения.");
        return;
    }

    for (let index = 0; index < seriesItems.length; index++) {
        const item = seriesItems[index];
        const row = document.createElement("div");
        row.className = "d-flex align-items-start gap-2 py-1 rounded-2 px-1";
        row.dataset.seriesId = String(item?.id || "");
        row.style.cursor = "pointer";
        row.addEventListener("click", () => {
            LAST_REPORT_ACTIVE_SERIES_ID = String(item?.id || "");
            applyReportChartInteractiveState();
        });

        const marker = document.createElement("span");
        marker.className = "rounded-circle flex-shrink-0";
        marker.style.width = "12px";
        marker.style.height = "12px";
        marker.style.marginTop = "5px";
        marker.style.backgroundColor = seriesColors[index];

        const text = document.createElement("div");
        text.className = "small";
        text.textContent = String(item?.name || "");

        row.append(marker, text);
        legendElement.appendChild(row);
    }
}

function buildReportChartSeriesColors(seriesItems) {
    const colors = [];
    const baseOffset = buildReportChartColorBaseOffset(seriesItems);

    for (let index = 0; index < seriesItems.length; index++) {
        const hue = Math.round((baseOffset + (index * 137.508)) % 360);
        const saturation = 68 + (index % 3) * 6;
        const lightness = 42 + (index % 2) * 8;
        colors.push(`hsl(${hue}deg ${saturation}% ${lightness}%)`);
    }

    return colors;
}

function getReportChartTimeAxisText(timeAxisValue) {
    return timeAxisValue === AXIS_CREATED_AT
        ? "время создания списания"
        : "указанное время списания";
}

function applyReportChartInteractiveState() {
    updateReportChartLegendSelectionButton();

    if (!REPORT_SPENT_TIME_CHART || !LAST_REPORT_CHART_DATA) return;

    const focusSeriesId = LAST_REPORT_ACTIVE_SERIES_ID;
    const seriesItems = Array.isArray(LAST_REPORT_CHART_DATA?.series) ? LAST_REPORT_CHART_DATA.series : [];

    REPORT_SPENT_TIME_CHART.setOption({
        series: seriesItems.map((seriesItem) => {
            const seriesId = String(seriesItem?.id || "");
            const isFocused = !focusSeriesId || focusSeriesId === seriesId;
            const opacity = isFocused ? ACTIVE_SERIES_OPACITY : DIMMED_SERIES_OPACITY;

            return {
                id: seriesId,
                lineStyle: {
                    opacity,
                    width: isFocused ? 3 : 2
                },
                itemStyle: {
                    opacity
                },
                emphasis: {
                    disabled: true
                },
                select: {
                    disabled: true
                }
            };
        })
    });

    updateReportChartLegendVisualState(focusSeriesId);
}

function updateReportChartLegendSelectionButton() {
    const clearButton = document.getElementById("chartClearLegendSelectionButton");
    if (!clearButton) return;

    clearButton.classList.toggle("d-none", !LAST_REPORT_ACTIVE_SERIES_ID);
}

function updateReportChartLegendVisualState(focusSeriesId) {
    const legendItems = document.querySelectorAll("#chartLegendList [data-series-id]");
    let ACTIVE_LEGEND_ITEM = null;

    for (const item of legendItems) {
        const isFocused = !focusSeriesId || item.dataset.seriesId === focusSeriesId;
        item.style.opacity = isFocused ? String(ACTIVE_SERIES_OPACITY) : String(DIMMED_SERIES_OPACITY);
        item.style.backgroundColor = item.dataset.seriesId === LAST_REPORT_ACTIVE_SERIES_ID ? "rgba(13,110,253,0.08)" : "transparent";

        if (item.dataset.seriesId === LAST_REPORT_ACTIVE_SERIES_ID)
            ACTIVE_LEGEND_ITEM = item;
    }

    if (ACTIVE_LEGEND_ITEM) {
        ACTIVE_LEGEND_ITEM.scrollIntoView({
            block: "nearest",
            behavior: "smooth"
        });
    }
}

function formatReportChartTooltip(params) {
    const items = Array.isArray(params) ? params : [];
    const nonZeroItems = items.filter((item) => Number(item?.value?.[1] ?? item?.value ?? 0) > 0);

    if (nonZeroItems.length === 0) {
        const axisValue = items[0]?.axisValue;
        return axisValue ? `${formatReportChartTooltipDate(axisValue)}<br/>Нет списаний` : "Нет списаний";
    }

    const focusSeriesId = LAST_REPORT_ACTIVE_SERIES_ID;
    const sortedItems = [...nonZeroItems].sort((a, b) => Number(b?.value?.[1] ?? b?.value ?? 0) - Number(a?.value?.[1] ?? a?.value ?? 0));
    const lines = [`<div style="font-weight:600;margin-bottom:6px;">${escapeReportChartHtml(formatReportChartTooltipDate(sortedItems[0]?.axisValue))}</div>`];

    for (const item of sortedItems) {
        const isFocused = !focusSeriesId || String(item?.seriesId || "") === focusSeriesId;
        const opacity = isFocused ? ACTIVE_SERIES_OPACITY : DIMMED_SERIES_OPACITY;
        const fontWeight = isFocused ? 600 : 400;
        const color = String(item?.color || "#0d6efd");
        const name = escapeReportChartHtml(String(item?.seriesName || ""));
        const value = escapeReportChartHtml(formatReportChartSpentTime(Number(item?.value?.[1] ?? item?.value ?? 0)));

        lines.push(
            `<div style="opacity:${opacity};display:flex;align-items:center;justify-content:space-between;gap:12px;margin-bottom:2px;">` +
            `<div style="display:flex;align-items:center;gap:6px;">` +
            `<span style="width:10px;height:10px;display:inline-block;border-radius:999px;background:${escapeReportChartHtml(color)};"></span>` +
            `<span>${name}</span>` +
            `</div>` +
            `<span style="font-weight:${fontWeight};">${value}</span>` +
            `</div>`
        );
    }

    return lines.join("");
}

function formatReportChartTooltipDate(value) {
    const date = new Date(value);
    const granularityValue = String(document.getElementById("chartGranularitySelect")?.value || GRANULARITY_DAY);

    if (granularityValue === GRANULARITY_HOUR) {
        return `${padReportChartNumber(date.getDate())}.${padReportChartNumber(date.getMonth() + 1)}.${date.getFullYear()} ${padReportChartNumber(date.getHours())}:00`;
    }

    return `${padReportChartNumber(date.getDate())}.${padReportChartNumber(date.getMonth() + 1)}.${date.getFullYear()}`;
}

function buildReportChartColorBaseOffset(seriesItems) {
    const firstSeriesKey = String(seriesItems?.[0]?.id || seriesItems?.[0]?.name || "");
    let hash = 0;

    for (let index = 0; index < firstSeriesKey.length; index++) {
        hash = ((hash << 5) - hash) + firstSeriesKey.charCodeAt(index);
        hash |= 0;
    }

    return Math.abs(hash) % 360;
}

function updateVisibleReportChartEmployeeIds(chartData) {
    const scopeValue = String(document.getElementById("chartScopeSelect")?.value || SCOPE_EMPLOYEE);
    if (scopeValue !== SCOPE_EMPLOYEE) {
        REPORT_VISIBLE_EMPLOYEE_IDS = null;
        return;
    }

    const seriesItems = Array.isArray(chartData?.series) ? chartData.series : [];
    const visibleIds = new Set();

    for (const seriesItem of seriesItems) {
        const total = getReportChartSeriesTotal(seriesItem);

        if (total > 0)
            visibleIds.add(Number(seriesItem.id));
    }

    REPORT_VISIBLE_EMPLOYEE_IDS = visibleIds;
    CHART_SELECTED_EMPLOYEE_IDS = new Set(
        Array.from(CHART_SELECTED_EMPLOYEE_IDS).filter(id => visibleIds.has(id))
    );

    saveReportChartFiltersState();
}

function restoreReportChartFiltersState() {
    const state = readReportChartFiltersState();
    if (!state) return;

    const scopeSelect = document.getElementById("chartScopeSelect");
    const timeAxisSelect = document.getElementById("chartTimeAxisSelect");
    const granularitySelect = document.getElementById("chartGranularitySelect");
    const dateFrom = document.getElementById("chartDateFrom");
    const dateTo = document.getElementById("chartDateTo");

    if (scopeSelect && (state.scope === SCOPE_EMPLOYEE || state.scope === SCOPE_GROUP))
        scopeSelect.value = state.scope;

    if (timeAxisSelect && (state.timeAxis === AXIS_CREATED_AT || state.timeAxis === AXIS_LOGGED_AT))
        timeAxisSelect.value = state.timeAxis;

    if (granularitySelect && (state.granularity === GRANULARITY_HOUR || state.granularity === GRANULARITY_DAY))
        granularitySelect.value = state.granularity;

    if (dateFrom && isValidReportChartDateInputValue(state.dateFrom))
        dateFrom.value = state.dateFrom;

    if (dateTo && isValidReportChartDateInputValue(state.dateTo))
        dateTo.value = state.dateTo;

    CHART_SELECTED_GROUP_IDS = new Set(normalizeStoredReportChartIds(state.groupIds));
    CHART_SELECTED_EMPLOYEE_IDS = new Set(normalizeStoredReportChartIds(state.employeeIds));
}

function saveReportChartFiltersState() {
    const scopeValue = String(document.getElementById("chartScopeSelect")?.value || SCOPE_EMPLOYEE);
    const timeAxisValue = String(document.getElementById("chartTimeAxisSelect")?.value || AXIS_CREATED_AT);
    const granularityValue = String(document.getElementById("chartGranularitySelect")?.value || GRANULARITY_DAY);
    const dateFromValue = String(document.getElementById("chartDateFrom")?.value || "");
    const dateToValue = String(document.getElementById("chartDateTo")?.value || "");

    const state = {
        scope: scopeValue,
        timeAxis: timeAxisValue,
        granularity: granularityValue,
        dateFrom: dateFromValue,
        dateTo: dateToValue,
        groupIds: Array.from(CHART_SELECTED_GROUP_IDS),
        employeeIds: Array.from(CHART_SELECTED_EMPLOYEE_IDS)
    };

    localStorage.setItem(CHART_FILTERS_STORAGE_KEY, JSON.stringify(state));
}

function readReportChartFiltersState() {
    const rawValue = localStorage.getItem(CHART_FILTERS_STORAGE_KEY);
    if (!rawValue) return null;

    try {
        const parsedValue = JSON.parse(rawValue);
        return parsedValue && typeof parsedValue === "object" ? parsedValue : null;
    } catch {
        return null;
    }
}

function normalizeStoredReportChartIds(values) {
    if (!Array.isArray(values))
        return [];

    return values
        .map(value => Number(value))
        .filter(value => Number.isInteger(value) && value > 0);
}

function isValidReportChartDateInputValue(value) {
    return /^\d{4}-\d{2}-\d{2}$/.test(String(value || ""));
}

function getReportChartSeriesTotal(seriesItem) {
    const values = Array.isArray(seriesItem?.values) ? seriesItem.values : [];
    return values.reduce((sum, value) => sum + Number(value || 0), 0);
}

function readCurrentReportChartDataZoomState() {
    if (!REPORT_SPENT_TIME_CHART) return LAST_REPORT_DATA_ZOOM_STATE;

    const option = REPORT_SPENT_TIME_CHART.getOption();
    const dataZoomItems = Array.isArray(option?.dataZoom) ? option.dataZoom : [];
    if (dataZoomItems.length === 0) return LAST_REPORT_DATA_ZOOM_STATE;

    const sliderItem = dataZoomItems.find((item) => item?.type === "slider") || dataZoomItems[0];
    if (!sliderItem) return LAST_REPORT_DATA_ZOOM_STATE;

    const start = Number(sliderItem.start);
    const end = Number(sliderItem.end);
    if (!Number.isFinite(start) || !Number.isFinite(end)) return LAST_REPORT_DATA_ZOOM_STATE;

    return {
        start,
        end
    };
}

function applySavedReportChartDataZoomState() {
    if (!REPORT_SPENT_TIME_CHART || !LAST_REPORT_DATA_ZOOM_STATE) return;

    REPORT_SPENT_TIME_CHART.setOption({
        dataZoom: [
            {
                type: "inside",
                start: LAST_REPORT_DATA_ZOOM_STATE.start,
                end: LAST_REPORT_DATA_ZOOM_STATE.end
            },
            {
                type: "slider",
                start: LAST_REPORT_DATA_ZOOM_STATE.start,
                end: LAST_REPORT_DATA_ZOOM_STATE.end
            }
        ]
    });
}

function escapeReportChartHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

function buildReportChartSeriesPoints(buckets, values) {
    const result = [];
    const safeValues = Array.isArray(values) ? values : [];

    for (let index = 0; index < buckets.length; index++) {
        const bucketValue = String(buckets[index] || "");
        const pointValue = Number(safeValues[index] || 0);
        result.push([bucketValue, pointValue]);
    }

    return result;
}

function formatReportChartSpentTime(value) {
    if (!Number.isFinite(value) || value <= 0) return "0 ч";
    if (Math.abs(value) >= 10) return `${value.toFixed(1)} ч`;
    return `${value.toFixed(2)} ч`;
}

function formatReportChartAxisLabel(value) {
    const granularityValue = String(document.getElementById("chartGranularitySelect")?.value || GRANULARITY_DAY);
    const date = new Date(value);

    if (granularityValue === GRANULARITY_HOUR) {
        return `${padReportChartNumber(date.getDate())}.${padReportChartNumber(date.getMonth() + 1)} ${padReportChartNumber(date.getHours())}:00`;
    }

    return `${padReportChartNumber(date.getDate())}.${padReportChartNumber(date.getMonth() + 1)}.${date.getFullYear()}`;
}

function formatReportChartDateInputValue(date) {
    return `${date.getFullYear()}-${padReportChartNumber(date.getMonth() + 1)}-${padReportChartNumber(date.getDate())}`;
}

function padReportChartNumber(value) {
    return String(value).padStart(2, "0");
}

window.ReportSpentTimeChart = {
    ensureInitialized: initReportSpentTimeChart,
    onShow: resizeReportChart
};