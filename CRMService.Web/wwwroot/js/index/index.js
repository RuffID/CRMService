const SCOPE_EMPLOYEE = "employee";
const SCOPE_GROUP = "group";
const AXIS_CREATED_AT = "createdAt";
const AXIS_LOGGED_AT = "loggedAt";
const GRANULARITY_HOUR = "hour";
const GRANULARITY_DAY = "day";
const DEFAULT_CHART_HEIGHT = 620;
const ACTIVE_SERIES_OPACITY = 1;
const DIMMED_SERIES_OPACITY = 0.15;
const FILTERS_STORAGE_KEY = "crm_index_chart_filters_v1";

let ANTI_FORGERY_TOKEN = null;
let TIME_CHART = null;
let ALL_GROUPS = [];
let ALL_EMPLOYEES = [];
let SELECTED_GROUP_IDS = new Set();
let SELECTED_EMPLOYEE_IDS = new Set();
let IS_CHART_LOADING = false;
let LAST_CHART_DATA = null;
let LAST_SERIES_COLORS = [];
let ACTIVE_SERIES_ID = null;
let LAST_DATA_ZOOM_STATE = null;
let VISIBLE_EMPLOYEE_IDS = null;
let IS_INDEX_PAGE_INITIALIZED = false;

async function initIndexPage() {
    if (IS_INDEX_PAGE_INITIALIZED)
        return;

    IS_INDEX_PAGE_INITIALIZED = true;
    ANTI_FORGERY_TOKEN = getRequestVerificationToken();

    initChart();
    initFilters();
    restoreFiltersState();
    ensureDefaultPeriod();
    updateScopeVisibility();

    await loadGroups();
    await loadEmployees();
    await loadTimeChart();
}

function initChart() {
    const chartElement = document.getElementById("timeChart");
    if (!chartElement || typeof echarts === "undefined") return;

    TIME_CHART = echarts.init(chartElement);
    TIME_CHART.on("click", (params) => {
        if (!params || params.componentType !== "series") return;

        ACTIVE_SERIES_ID = String(params.seriesId || "");
        applyInteractiveState();
    });

    TIME_CHART.on("datazoom", () => {
        LAST_DATA_ZOOM_STATE = readCurrentDataZoomState();
    });

    window.addEventListener("resize", () => {
        if (TIME_CHART) TIME_CHART.resize();
    });

    initLegendControls();
    renderEmptyChart("Загрузка данных...");
    renderLegend([]);
}

function resizeIndexChart() {
    if (TIME_CHART)
        TIME_CHART.resize();
}

function initLegendControls() {
    const clearButton = document.getElementById("clearLegendSelectionButton");
    if (!clearButton) return;

    clearButton.addEventListener("click", () => {
        ACTIVE_SERIES_ID = null;
        applyInteractiveState();
    });
}

function initFilters() {
    const scopeSelect = document.getElementById("scopeSelect");
    const timeAxisSelect = document.getElementById("timeAxisSelect");
    const granularitySelect = document.getElementById("granularitySelect");
    const btnApply = document.getElementById("btnApplyChartFilters");
    const groupsSearch = document.getElementById("groupsSearch");
    const employeesSearch = document.getElementById("employeesSearch");
    const groupsSelectAll = document.getElementById("groupsSelectAll");
    const groupsClearAll = document.getElementById("groupsClearAll");
    const employeesSelectAll = document.getElementById("employeesSelectAll");
    const employeesClearAll = document.getElementById("employeesClearAll");

    if (scopeSelect) {
        scopeSelect.addEventListener("change", () => {
            saveFiltersState();
            updateScopeVisibility();
            updateSummaryText();
        });
    }

    if (timeAxisSelect) {
        timeAxisSelect.addEventListener("change", () => {
            saveFiltersState();
            updateSummaryText();
        });
    }

    if (granularitySelect) {
        granularitySelect.addEventListener("change", () => {
            saveFiltersState();
            updateSummaryText();
        });
    }

    const dateFrom = document.getElementById("dateFrom");
    const dateTo = document.getElementById("dateTo");

    if (dateFrom) {
        dateFrom.addEventListener("change", () => {
            saveFiltersState();
            updateSummaryText();
        });
    }

    if (dateTo) {
        dateTo.addEventListener("change", () => {
            saveFiltersState();
            updateSummaryText();
        });
    }

    if (btnApply) {
        btnApply.addEventListener("click", async () => {
            await loadTimeChart();
        });
    }

    if (groupsSearch) {
        groupsSearch.addEventListener("input", () => {
            renderGroupFilter();
        });
    }

    if (employeesSearch) {
        employeesSearch.addEventListener("input", () => {
            renderEmployeeFilter();
        });
    }

    if (groupsSelectAll) {
        groupsSelectAll.addEventListener("click", async () => {
            SELECTED_GROUP_IDS = new Set(ALL_GROUPS.map(x => Number(x.id)));
            saveFiltersState();
            renderGroupFilter();
            await loadEmployees();
            updateSummaryText();
        });
    }

    if (groupsClearAll) {
        groupsClearAll.addEventListener("click", async () => {
            SELECTED_GROUP_IDS = new Set();
            saveFiltersState();
            renderGroupFilter();
            await loadEmployees();
            updateSummaryText();
        });
    }

    if (employeesSelectAll) {
        employeesSelectAll.addEventListener("click", () => {
            SELECTED_EMPLOYEE_IDS = new Set(ALL_EMPLOYEES.map(x => Number(x.id)));
            saveFiltersState();
            renderEmployeeFilter();
            updateSummaryText();
        });
    }

    if (employeesClearAll) {
        employeesClearAll.addEventListener("click", () => {
            SELECTED_EMPLOYEE_IDS = new Set();
            saveFiltersState();
            renderEmployeeFilter();
            updateSummaryText();
        });
    }
}

function ensureDefaultPeriod() {
    const dateFrom = document.getElementById("dateFrom");
    const dateTo = document.getElementById("dateTo");
    if (!dateFrom || !dateTo) return;
    if (dateFrom.value && dateTo.value) return;

    const currentDate = new Date();
    const monthAgoDate = new Date(currentDate);
    monthAgoDate.setDate(currentDate.getDate() - 30);

    dateFrom.value = formatDateInputValue(monthAgoDate);
    dateTo.value = formatDateInputValue(currentDate);
}

async function loadGroups() {
    try {
        clearPageError();

        const response = await sendJsonRequest(
            "?handler=EmployeeGroupList",
            "GET",
            buildJsonHeaders(ANTI_FORGERY_TOKEN)
        );

        const data = unwrapOrThrow(response, "Ошибка загрузки групп.");
        ALL_GROUPS = Array.isArray(data)
            ? data
                .filter(x => x && Number.isFinite(Number(x.id)))
                .sort((a, b) => String(a.name || "").localeCompare(String(b.name || ""), "ru"))
            : [];

        renderGroupFilter();
        updateSummaryText();
        return true;
    } catch (error) {
        console.error(error);
        showPageError(error?.message || "Ошибка загрузки групп.");
        ALL_GROUPS = [];
        renderGroupFilter();
        return false;
    }
}

async function loadEmployees() {
    try {
        clearPageError();

        const payload = {
            groupIds: SELECTED_GROUP_IDS.size > 0 ? Array.from(SELECTED_GROUP_IDS) : null
        };

        const response = await sendJsonRequest(
            "?handler=EmployeeList",
            "POST",
            buildJsonHeaders(ANTI_FORGERY_TOKEN),
            payload
        );

        const data = unwrapOrThrow(response, "Ошибка загрузки сотрудников.");
        ALL_EMPLOYEES = Array.isArray(data)
            ? data
                .filter(x => x && Number.isFinite(Number(x.id)))
                .sort((a, b) => getEmployeeDisplayName(a).localeCompare(getEmployeeDisplayName(b), "ru"))
            : [];

        SELECTED_EMPLOYEE_IDS = new Set(
            Array.from(SELECTED_EMPLOYEE_IDS).filter(id => ALL_EMPLOYEES.some(employee => Number(employee.id) === id))
        );

        renderEmployeeFilter();
        updateSummaryText();
        return true;
    } catch (error) {
        console.error(error);
        showPageError(error?.message || "Ошибка загрузки сотрудников.");
        ALL_EMPLOYEES = [];
        SELECTED_EMPLOYEE_IDS = new Set();
        renderEmployeeFilter();
        return false;
    }
}

async function loadTimeChart() {
    if (IS_CHART_LOADING) return false;

    const validationMessage = validateFilters();
    if (validationMessage) {
        showPageError(validationMessage);
        renderEmptyChart("Проверьте фильтры.");
        renderLegend([]);
        return false;
    }

    try {
        IS_CHART_LOADING = true;
        clearPageError();
        updateSummaryText("Загрузка данных...");
        LAST_DATA_ZOOM_STATE = readCurrentDataZoomState();

        const response = await sendJsonRequest(
            "?handler=TimeChart",
            "POST",
            buildJsonHeaders(ANTI_FORGERY_TOKEN),
            buildChartPayload()
        );

        const data = unwrapOrThrow(response, "Ошибка загрузки графика.");
        ACTIVE_SERIES_ID = null;
        updateVisibleEmployeeIds(data);
        renderTimeChart(data);
        renderEmployeeFilter();
        saveFiltersState();
        updateSummaryText();
        return true;
    } catch (error) {
        console.error(error);
        showPageError(error?.message || "Ошибка загрузки графика.");
        renderEmptyChart("Не удалось построить график.");
        renderLegend([]);
        VISIBLE_EMPLOYEE_IDS = null;
        renderEmployeeFilter();
        updateSummaryText();
        return false;
    } finally {
        IS_CHART_LOADING = false;
    }
}

function buildChartPayload() {
    const dateFromValue = String(document.getElementById("dateFrom")?.value || "");
    const dateToValue = String(document.getElementById("dateTo")?.value || "");
    const scopeValue = String(document.getElementById("scopeSelect")?.value || SCOPE_EMPLOYEE);
    const timeAxisValue = String(document.getElementById("timeAxisSelect")?.value || AXIS_CREATED_AT);
    const granularityValue = String(document.getElementById("granularitySelect")?.value || GRANULARITY_DAY);

    return {
        dateFrom: `${dateFromValue}T00:00:00`,
        dateTo: `${dateToValue}T00:00:00`,
        scope: scopeValue,
        timeAxis: timeAxisValue,
        granularity: granularityValue,
        employeeIds: scopeValue === SCOPE_EMPLOYEE && SELECTED_EMPLOYEE_IDS.size > 0 ? Array.from(SELECTED_EMPLOYEE_IDS) : null,
        groupIds: SELECTED_GROUP_IDS.size > 0 ? Array.from(SELECTED_GROUP_IDS) : null
    };
}

function validateFilters() {
    const dateFromValue = String(document.getElementById("dateFrom")?.value || "");
    const dateToValue = String(document.getElementById("dateTo")?.value || "");

    if (!dateFromValue || !dateToValue)
        return "Укажите период.";

    if (dateToValue < dateFromValue)
        return "Дата окончания должна быть больше или равна дате начала.";

    return "";
}

function renderTimeChart(chartData) {
    if (!TIME_CHART) return;

    const buckets = Array.isArray(chartData?.buckets) ? chartData.buckets : [];
    const seriesItems = Array.isArray(chartData?.series)
        ? chartData.series.filter(seriesItem => getSeriesTotal(seriesItem) > 0)
        : [];

    if (buckets.length === 0 || seriesItems.length === 0) {
        LAST_CHART_DATA = null;
        LAST_SERIES_COLORS = [];
        renderEmptyChart("Нет данных за выбранный период.");
        renderLegend([]);
        updateLegendSelectionButton();
        return;
    }

    const seriesColors = buildSeriesColors(seriesItems);
    LAST_CHART_DATA = chartData;
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
            formatter: (params) => formatTooltip(params)
        },
        xAxis: {
            type: "time",
            axisLabel: {
                formatter: (value) => formatAxisLabel(value)
            }
        },
        yAxis: {
            type: "value",
            name: "Часы",
            axisLabel: {
                formatter: (value) => formatSpentTime(Number(value))
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
                data: buildSeriesPoints(buckets, seriesItem.values)
            };
        })
    };

    TIME_CHART.setOption(option, true);
    TIME_CHART.resize({ height: DEFAULT_CHART_HEIGHT });
    applySavedDataZoomState();
    renderLegend(seriesItems, seriesColors);
    applyInteractiveState();
}

function renderEmptyChart(message) {
    if (!TIME_CHART) return;

    TIME_CHART.setOption({
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

    TIME_CHART.resize({ height: DEFAULT_CHART_HEIGHT });
}

function renderGroupFilter() {
    const listElement = document.getElementById("groupsList");
    const badgeElement = document.getElementById("groupsBadge");
    const hintElement = document.getElementById("groupsHint");
    const searchValue = String(document.getElementById("groupsSearch")?.value || "").trim().toLowerCase();

    if (badgeElement) badgeElement.textContent = String(SELECTED_GROUP_IDS.size);
    if (!listElement) return;

    listElement.textContent = "";

    const items = ALL_GROUPS.filter(group => String(group.name || "").toLowerCase().includes(searchValue));
    if (hintElement) {
        const hiddenCount = ALL_GROUPS.length - items.length;
        hintElement.textContent = hiddenCount > 0 ? `Скрыто элементов: ${hiddenCount}` : "";
        hintElement.classList.toggle("d-none", hiddenCount === 0);
    }

    if (items.length === 0) {
        appendEmptyState(listElement, "Группы не найдены.");
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
        input.checked = SELECTED_GROUP_IDS.has(groupId);
        input.addEventListener("change", async () => {
            if (input.checked) SELECTED_GROUP_IDS.add(groupId);
            else SELECTED_GROUP_IDS.delete(groupId);

            saveFiltersState();
            renderGroupFilter();
            await loadEmployees();
            updateSummaryText();
        });

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = inputId;
        label.textContent = String(group.name || `Группа ${groupId}`);

        wrapper.append(input, label);
        listElement.appendChild(wrapper);
    }
}

function renderEmployeeFilter() {
    const listElement = document.getElementById("employeesList");
    const badgeElement = document.getElementById("employeesBadge");
    const hintElement = document.getElementById("employeesHint");
    const searchValue = String(document.getElementById("employeesSearch")?.value || "").trim().toLowerCase();

    if (badgeElement) badgeElement.textContent = String(SELECTED_EMPLOYEE_IDS.size);
    if (!listElement) return;

    listElement.textContent = "";

    const items = ALL_EMPLOYEES
        .filter(employee => isEmployeeVisibleInFilters(employee))
        .filter(employee => getEmployeeDisplayName(employee).toLowerCase().includes(searchValue));
    if (hintElement) {
        const hiddenCount = ALL_EMPLOYEES.length - items.length;
        hintElement.textContent = hiddenCount > 0 ? `Скрыто элементов: ${hiddenCount}` : "";
        hintElement.classList.toggle("d-none", hiddenCount === 0);
    }

    if (items.length === 0) {
        appendEmptyState(listElement, "Сотрудники не найдены.");
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
        input.checked = SELECTED_EMPLOYEE_IDS.has(employeeId);
        input.addEventListener("change", () => {
            if (input.checked) SELECTED_EMPLOYEE_IDS.add(employeeId);
            else SELECTED_EMPLOYEE_IDS.delete(employeeId);

            saveFiltersState();
            renderEmployeeFilter();
            updateSummaryText();
        });

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = inputId;
        label.textContent = getEmployeeDisplayName(employee);

        wrapper.append(input, label);
        listElement.appendChild(wrapper);
    }
}

function appendEmptyState(container, message) {
    const stateElement = document.createElement("div");
    stateElement.className = "text-muted small px-1 py-2";
    stateElement.textContent = message;
    container.appendChild(stateElement);
}

function updateScopeVisibility() {
    const scopeValue = String(document.getElementById("scopeSelect")?.value || SCOPE_EMPLOYEE);
    const employeeFilterColumn = document.getElementById("employeeFilterColumn");

    if (!employeeFilterColumn) return;
    employeeFilterColumn.classList.toggle("d-none", scopeValue !== SCOPE_EMPLOYEE);
}

function updateSummaryText(customText) {
    const summaryElement = document.getElementById("chartSummaryText");
    if (!summaryElement) return;

    if (customText) {
        summaryElement.textContent = customText;
        return;
    }

    const scopeValue = String(document.getElementById("scopeSelect")?.value || SCOPE_EMPLOYEE);
    const timeAxisValue = String(document.getElementById("timeAxisSelect")?.value || AXIS_CREATED_AT);
    const granularityValue = String(document.getElementById("granularitySelect")?.value || GRANULARITY_DAY);
    const selectedCount = scopeValue === SCOPE_GROUP ? SELECTED_GROUP_IDS.size : SELECTED_EMPLOYEE_IDS.size;
    const scopeText = scopeValue === SCOPE_GROUP ? "группы" : "сотрудники";
    const axisText = getTimeAxisText(timeAxisValue);
    const granularityText = granularityValue === GRANULARITY_HOUR
        ? "по часам"
        : "по дням";
    const selectionText = selectedCount > 0 ? `выбрано: ${selectedCount}` : "выбраны все";

    summaryElement.textContent = `Режим: ${scopeText}, ось времени: ${axisText}, шаг: ${granularityText}, ${selectionText}.`;
}

function showPageError(message) {
    const pageError = document.getElementById("pageError");
    if (!pageError) return;

    pageError.textContent = message || "Ошибка загрузки данных.";
    pageError.classList.remove("d-none");
}

function clearPageError() {
    const pageError = document.getElementById("pageError");
    if (!pageError) return;

    pageError.textContent = "";
    pageError.classList.add("d-none");
}

function getEmployeeDisplayName(employee) {
    const parts = [
        String(employee?.last_name || employee?.lastName || "").trim(),
        String(employee?.first_name || employee?.firstName || "").trim(),
        String(employee?.patronymic || "").trim()
    ].filter(Boolean);

    return parts.length > 0 ? parts.join(" ") : String(employee?.id || "");
}

function isEmployeeVisibleInFilters(employee) {
    if (!(VISIBLE_EMPLOYEE_IDS instanceof Set))
        return true;

    return VISIBLE_EMPLOYEE_IDS.has(Number(employee?.id));
}

function renderLegend(seriesItems, seriesColors) {
    const legendElement = document.getElementById("chartLegendList");
    if (!legendElement) return;

    legendElement.textContent = "";

    if (!Array.isArray(seriesItems) || seriesItems.length === 0) {
        appendEmptyState(legendElement, "Нет элементов для отображения.");
        return;
    }

    for (let index = 0; index < seriesItems.length; index++) {
        const item = seriesItems[index];
        const row = document.createElement("div");
        row.className = "d-flex align-items-start gap-2 py-1 rounded-2 px-1";
        row.dataset.seriesId = String(item?.id || "");
        row.style.cursor = "pointer";
        row.addEventListener("click", () => {
            ACTIVE_SERIES_ID = String(item?.id || "");
            applyInteractiveState();
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

function buildSeriesColors(seriesItems) {
    const colors = [];
    const baseOffset = buildColorBaseOffset(seriesItems);

    for (let index = 0; index < seriesItems.length; index++) {
        const hue = Math.round((baseOffset + (index * 137.508)) % 360);
        const saturation = 68 + (index % 3) * 6;
        const lightness = 42 + (index % 2) * 8;
        colors.push(`hsl(${hue}deg ${saturation}% ${lightness}%)`);
    }

    return colors;
}

function getTimeAxisText(timeAxisValue) {
    return timeAxisValue === AXIS_CREATED_AT
        ? "время создания списания"
        : "указанное время списания";
}

function applyInteractiveState() {
    updateLegendSelectionButton();

    if (!TIME_CHART || !LAST_CHART_DATA) return;

    const focusSeriesId = ACTIVE_SERIES_ID;
    const seriesItems = Array.isArray(LAST_CHART_DATA?.series) ? LAST_CHART_DATA.series : [];

    TIME_CHART.setOption({
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

    updateLegendVisualState(focusSeriesId);
}

function updateLegendSelectionButton() {
    const clearButton = document.getElementById("clearLegendSelectionButton");
    if (!clearButton) return;

    clearButton.classList.toggle("d-none", !ACTIVE_SERIES_ID);
}

function updateLegendVisualState(focusSeriesId) {
    const legendItems = document.querySelectorAll("#chartLegendList [data-series-id]");
    let ACTIVE_LEGEND_ITEM = null;

    for (const item of legendItems) {
        const isFocused = !focusSeriesId || item.dataset.seriesId === focusSeriesId;
        item.style.opacity = isFocused ? String(ACTIVE_SERIES_OPACITY) : String(DIMMED_SERIES_OPACITY);
        item.style.backgroundColor = item.dataset.seriesId === ACTIVE_SERIES_ID ? "rgba(13,110,253,0.08)" : "transparent";

        if (item.dataset.seriesId === ACTIVE_SERIES_ID)
            ACTIVE_LEGEND_ITEM = item;
    }

    if (ACTIVE_LEGEND_ITEM) {
        ACTIVE_LEGEND_ITEM.scrollIntoView({
            block: "nearest",
            behavior: "smooth"
        });
    }
}

function formatTooltip(params) {
    const items = Array.isArray(params) ? params : [];
    const nonZeroItems = items.filter((item) => Number(item?.value?.[1] ?? item?.value ?? 0) > 0);

    if (nonZeroItems.length === 0) {
        const axisValue = items[0]?.axisValue;
        return axisValue ? `${formatTooltipDate(axisValue)}<br/>Нет списаний` : "Нет списаний";
    }

    const focusSeriesId = ACTIVE_SERIES_ID;
    const sortedItems = [...nonZeroItems].sort((a, b) => Number(b?.value?.[1] ?? b?.value ?? 0) - Number(a?.value?.[1] ?? a?.value ?? 0));
    const lines = [`<div style="font-weight:600;margin-bottom:6px;">${escapeHtml(formatTooltipDate(sortedItems[0]?.axisValue))}</div>`];

    for (const item of sortedItems) {
        const isFocused = !focusSeriesId || String(item?.seriesId || "") === focusSeriesId;
        const opacity = isFocused ? ACTIVE_SERIES_OPACITY : DIMMED_SERIES_OPACITY;
        const fontWeight = isFocused ? 600 : 400;
        const color = String(item?.color || "#0d6efd");
        const name = escapeHtml(String(item?.seriesName || ""));
        const value = escapeHtml(formatSpentTime(Number(item?.value?.[1] ?? item?.value ?? 0)));

        lines.push(
            `<div style="opacity:${opacity};display:flex;align-items:center;justify-content:space-between;gap:12px;margin-bottom:2px;">` +
            `<div style="display:flex;align-items:center;gap:6px;">` +
            `<span style="width:10px;height:10px;display:inline-block;border-radius:999px;background:${escapeHtml(color)};"></span>` +
            `<span>${name}</span>` +
            `</div>` +
            `<span style="font-weight:${fontWeight};">${value}</span>` +
            `</div>`
        );
    }

    return lines.join("");
}

function formatTooltipDate(value) {
    const date = new Date(value);
    const granularityValue = String(document.getElementById("granularitySelect")?.value || GRANULARITY_DAY);

    if (granularityValue === GRANULARITY_HOUR) {
        return `${padNumber(date.getDate())}.${padNumber(date.getMonth() + 1)}.${date.getFullYear()} ${padNumber(date.getHours())}:00`;
    }

    return `${padNumber(date.getDate())}.${padNumber(date.getMonth() + 1)}.${date.getFullYear()}`;
}

function buildColorBaseOffset(seriesItems) {
    const firstSeriesKey = String(seriesItems?.[0]?.id || seriesItems?.[0]?.name || "");
    let hash = 0;

    for (let index = 0; index < firstSeriesKey.length; index++) {
        hash = ((hash << 5) - hash) + firstSeriesKey.charCodeAt(index);
        hash |= 0;
    }

    return Math.abs(hash) % 360;
}

function updateVisibleEmployeeIds(chartData) {
    const scopeValue = String(document.getElementById("scopeSelect")?.value || SCOPE_EMPLOYEE);
    if (scopeValue !== SCOPE_EMPLOYEE) {
        VISIBLE_EMPLOYEE_IDS = null;
        return;
    }

    const seriesItems = Array.isArray(chartData?.series) ? chartData.series : [];
    const visibleIds = new Set();

    for (const seriesItem of seriesItems) {
        const total = getSeriesTotal(seriesItem);

        if (total > 0)
            visibleIds.add(Number(seriesItem.id));
    }

    VISIBLE_EMPLOYEE_IDS = visibleIds;
    SELECTED_EMPLOYEE_IDS = new Set(
        Array.from(SELECTED_EMPLOYEE_IDS).filter(id => visibleIds.has(id))
    );

    saveFiltersState();
}

function restoreFiltersState() {
    const state = readFiltersState();
    if (!state) return;

    const scopeSelect = document.getElementById("scopeSelect");
    const timeAxisSelect = document.getElementById("timeAxisSelect");
    const granularitySelect = document.getElementById("granularitySelect");
    const dateFrom = document.getElementById("dateFrom");
    const dateTo = document.getElementById("dateTo");

    if (scopeSelect && (state.scope === SCOPE_EMPLOYEE || state.scope === SCOPE_GROUP))
        scopeSelect.value = state.scope;

    if (timeAxisSelect && (state.timeAxis === AXIS_CREATED_AT || state.timeAxis === AXIS_LOGGED_AT))
        timeAxisSelect.value = state.timeAxis;

    if (granularitySelect && (state.granularity === GRANULARITY_HOUR || state.granularity === GRANULARITY_DAY))
        granularitySelect.value = state.granularity;

    if (dateFrom && isValidDateInputValue(state.dateFrom))
        dateFrom.value = state.dateFrom;

    if (dateTo && isValidDateInputValue(state.dateTo))
        dateTo.value = state.dateTo;

    SELECTED_GROUP_IDS = new Set(normalizeStoredIds(state.groupIds));
    SELECTED_EMPLOYEE_IDS = new Set(normalizeStoredIds(state.employeeIds));
}

function saveFiltersState() {
    const scopeValue = String(document.getElementById("scopeSelect")?.value || SCOPE_EMPLOYEE);
    const timeAxisValue = String(document.getElementById("timeAxisSelect")?.value || AXIS_CREATED_AT);
    const granularityValue = String(document.getElementById("granularitySelect")?.value || GRANULARITY_DAY);
    const dateFromValue = String(document.getElementById("dateFrom")?.value || "");
    const dateToValue = String(document.getElementById("dateTo")?.value || "");

    const state = {
        scope: scopeValue,
        timeAxis: timeAxisValue,
        granularity: granularityValue,
        dateFrom: dateFromValue,
        dateTo: dateToValue,
        groupIds: Array.from(SELECTED_GROUP_IDS),
        employeeIds: Array.from(SELECTED_EMPLOYEE_IDS)
    };

    localStorage.setItem(FILTERS_STORAGE_KEY, JSON.stringify(state));
}

function readFiltersState() {
    const rawValue = localStorage.getItem(FILTERS_STORAGE_KEY);
    if (!rawValue) return null;

    try {
        const parsedValue = JSON.parse(rawValue);
        return parsedValue && typeof parsedValue === "object" ? parsedValue : null;
    } catch {
        return null;
    }
}

function normalizeStoredIds(values) {
    if (!Array.isArray(values))
        return [];

    return values
        .map(value => Number(value))
        .filter(value => Number.isInteger(value) && value > 0);
}

function isValidDateInputValue(value) {
    return /^\d{4}-\d{2}-\d{2}$/.test(String(value || ""));
}

function getSeriesTotal(seriesItem) {
    const values = Array.isArray(seriesItem?.values) ? seriesItem.values : [];
    return values.reduce((sum, value) => sum + Number(value || 0), 0);
}

function readCurrentDataZoomState() {
    if (!TIME_CHART) return LAST_DATA_ZOOM_STATE;

    const option = TIME_CHART.getOption();
    const dataZoomItems = Array.isArray(option?.dataZoom) ? option.dataZoom : [];
    if (dataZoomItems.length === 0) return LAST_DATA_ZOOM_STATE;

    const sliderItem = dataZoomItems.find((item) => item?.type === "slider") || dataZoomItems[0];
    if (!sliderItem) return LAST_DATA_ZOOM_STATE;

    const start = Number(sliderItem.start);
    const end = Number(sliderItem.end);
    if (!Number.isFinite(start) || !Number.isFinite(end)) return LAST_DATA_ZOOM_STATE;

    return {
        start,
        end
    };
}

function applySavedDataZoomState() {
    if (!TIME_CHART || !LAST_DATA_ZOOM_STATE) return;

    TIME_CHART.setOption({
        dataZoom: [
            {
                type: "inside",
                start: LAST_DATA_ZOOM_STATE.start,
                end: LAST_DATA_ZOOM_STATE.end
            },
            {
                type: "slider",
                start: LAST_DATA_ZOOM_STATE.start,
                end: LAST_DATA_ZOOM_STATE.end
            }
        ]
    });
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

function buildSeriesPoints(buckets, values) {
    const result = [];
    const safeValues = Array.isArray(values) ? values : [];

    for (let index = 0; index < buckets.length; index++) {
        const bucketValue = String(buckets[index] || "");
        const pointValue = Number(safeValues[index] || 0);
        result.push([bucketValue, pointValue]);
    }

    return result;
}

function formatSpentTime(value) {
    if (!Number.isFinite(value) || value <= 0) return "0 ч";
    if (Math.abs(value) >= 10) return `${value.toFixed(1)} ч`;
    return `${value.toFixed(2)} ч`;
}

function formatAxisLabel(value) {
    const granularityValue = String(document.getElementById("granularitySelect")?.value || GRANULARITY_DAY);
    const date = new Date(value);

    if (granularityValue === GRANULARITY_HOUR) {
        return `${padNumber(date.getDate())}.${padNumber(date.getMonth() + 1)} ${padNumber(date.getHours())}:00`;
    }

    return `${padNumber(date.getDate())}.${padNumber(date.getMonth() + 1)}.${date.getFullYear()}`;
}

function formatDateInputValue(date) {
    return `${date.getFullYear()}-${padNumber(date.getMonth() + 1)}-${padNumber(date.getDate())}`;
}

function padNumber(value) {
    return String(value).padStart(2, "0");
}

window.IndexSpentTimeChart = {
    ensureInitialized: initIndexPage,
    onShow: resizeIndexChart
};