const reportSortKey = "crm_report_sort_v1";
const reportFontScaleKey = "crm_report_font_scale_v1";
const reportFocusKey = "crm_report_focus_v1";
const reportColOrderKey = "crm_report_col_order_v1";

let isManualReloadInProgress = false;
let antiForgeryToken = null;
let reportItems = [];
let reportUpdatedAt = null;
let planColorRules = [];

let isHeaderDragging = false;
let dragSrcColId = null;
let headerInsert = { targetId: null, side: null };
let headerInsertEl = null;
let columnOrder = null;

window.resetReportSorting = resetReportSorting;
window.loadPerformanceReport = loadPerformanceReport;
window.applySortAndRender = applySortAndRender;
window.reloadPlanColors = initPlanColors;

document.addEventListener("DOMContentLoaded", () => {
    initReport();
});

document.addEventListener("fullscreenchange", () => {
    if (!document.fullscreenElement && document.body.classList.contains("report-focus-on")) {
        disableReportFocus();
    }
});

async function initReport() {
    antiForgeryToken = getRequestVerificationToken();

    initReportSorting();
    initReportFontScale();
    applyRowDensity(0.3);
    initReportFocusMode();
    initReportReloadButton();
    initReportColumnOrder();

    await initPlanColors();

    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((element) => {
        new bootstrap.Tooltip(element);
    });
}

async function initPlanColors() {
    const planId = getSelectedPlanId();
    if (!planId) {
        planColorRules = [];
        window.getPlanColorByPercent = () => "";
        return;
    }

    try {
        const response = await sendJsonRequest(
            `?handler=PlanColorRules&planId=${encodeURIComponent(planId)}`,
            "GET",
            buildJsonHeaders(antiForgeryToken)
        );

        const data = unwrapOrThrow(response, "Ошибка загрузки цветовой схемы.");
        planColorRules = Array.isArray(data) ? data : [];

        window.getPlanColorByPercent = (pct) => getPlanColorByPercentFromRules(pct, planColorRules);
    } catch {
        planColorRules = [];
        window.getPlanColorByPercent = () => "";
    }
}

function getPlanColorByPercentFromRules(pct, rules) {
    const percent = Number(pct);
    if (!Number.isFinite(percent)) return "";

    const items = Array.isArray(rules) ? [...rules] : [];
    items.sort((a, b) => {
        const aFrom = Number(a?.fromPercent ?? 0);
        const bFrom = Number(b?.fromPercent ?? 0);
        if (aFrom !== bFrom) return aFrom - bFrom;

        const aTo = Number(a?.toPercent ?? Number.MAX_SAFE_INTEGER);
        const bTo = Number(b?.toPercent ?? Number.MAX_SAFE_INTEGER);
        return aTo - bTo;
    });

    for (const item of items) {
        const from = Number(item?.fromPercent);
        const to = item?.toPercent === null || item?.toPercent === undefined
            ? Number.MAX_SAFE_INTEGER
            : Number(item.toPercent);
        const color = String(item?.color || "").trim();

        if (!Number.isFinite(from) || !Number.isFinite(to)) continue;
        if (!/^#([0-9a-fA-F]{6})$/.test(color)) continue;

        if (percent >= from && percent <= to) {
            return color.toUpperCase();
        }
    }

    return "";
}

function initReportSorting() {
    const grid = document.getElementById("grid");
    if (!grid) return;

    ensureSortIndicators();

    grid.querySelectorAll("th[data-sort-key]").forEach((th) => {
        th.addEventListener("click", () => {
            if (isHeaderDragging) return;

            const key = th.dataset.sortKey;
            if (!key) return;

            const state = loadSortState();
            let direction = "asc";

            if (state && state.key === key) direction = state.dir === "asc" ? "desc" : "asc";

            saveSortState({ key: key, dir: direction });
            updateSortIndicators();
            applySortAndRender();
        });
    });

    updateSortIndicators();
}

function initReportReloadButton() {
    const button = document.getElementById("reportReload");
    if (!button) return;

    button.addEventListener("click", async () => {
        if (isManualReloadInProgress) return;

        isManualReloadInProgress = true;
        button.disabled = true;

        if (typeof window.reloadPlanColors === "function") {
            await window.reloadPlanColors();
        }

        const ok = await loadPerformanceReport();
        if (ok) await new Promise((resolve) => setTimeout(resolve, 2000));

        button.disabled = false;
        isManualReloadInProgress = false;
    });
}

function initReportFontScale() {
    const buttonMinus = document.getElementById("fontMinus");
    const buttonPlus = document.getElementById("fontPlus");
    if (!buttonMinus || !buttonPlus) return;

    applyFontScale(loadFontScale());

    buttonMinus.addEventListener("click", () => changeFontScale(-0.1));
    buttonPlus.addEventListener("click", () => changeFontScale(0.1));
}

function initReportFocusMode() {
    const button = document.getElementById("reportFocusToggle");
    if (!button) return;

    button.addEventListener("click", () => toggleReportFocus());

    const saved = localStorage.getItem(reportFocusKey);
    if (saved === "1") enableReportFocus();
}

function initReportColumnOrder() {
    const grid = document.getElementById("grid");
    if (!grid) return;

    const theadRow = grid.querySelector("thead tr");
    if (!theadRow) return;

    const defaultOrder = Array.from(theadRow.querySelectorAll("th[data-col-id]"))
        .map((th) => th.dataset.colId)
        .filter((x) => !!x);

    columnOrder = loadColumnOrder(defaultOrder);
    applyColumnOrderToHeader(columnOrder);
    ensureHeaderInsertIndicator();

    let dragSourceId = null;

    theadRow.querySelectorAll("th[data-col-id]").forEach((th) => {
        th.addEventListener("dragstart", (event) => {
            const id = th.dataset.colId;
            if (!id) return;

            isHeaderDragging = true;
            dragSourceId = id;
            dragSrcColId = id;

            setHeaderDragSourceVisual(dragSrcColId);

            try { event.dataTransfer.effectAllowed = "move"; } catch { }
            try { event.dataTransfer.setData("text/plain", id); } catch { }
        });

        th.addEventListener("dragend", () => {
            isHeaderDragging = false;
            dragSourceId = null;
            dragSrcColId = null;

            hideHeaderInsertIndicator();
            clearHeaderDragSourceVisual();
        });

        th.addEventListener("dragover", (event) => {
            event.preventDefault();
            showHeaderInsertIndicatorAt(event.clientX, event.clientY);
            try { event.dataTransfer.dropEffect = "move"; } catch { }
        });

        th.addEventListener("drop", (event) => {
            event.preventDefault();

            const targetId = headerInsert.targetId;
            const side = headerInsert.side;

            let sourceId = null;
            try { sourceId = event.dataTransfer.getData("text/plain"); } catch { }
            if (!sourceId) sourceId = dragSourceId;

            hideHeaderInsertIndicator();
            clearHeaderDragSourceVisual();

            if (!sourceId || !targetId || !side) return;
            if (sourceId === targetId) return;

            const nextOrder = moveArrayItemBySide(columnOrder, sourceId, targetId, side);
            if (!nextOrder || nextOrder.length === 0) return;

            columnOrder = nextOrder;
            saveColumnOrder(columnOrder);
            applyColumnOrderToHeader(columnOrder);

            updateSortIndicators();
            applySortAndRender();
        });
    });
}

async function loadPerformanceReport() {
    try {
        if (typeof window.enforceReportModeDates === "function") {
            window.enforceReportModeDates();
        }

        await initPlanColors();

        const payload = buildReportPayload();
        const response = await sendJsonRequest("?handler=Report", "POST", buildJsonHeaders(antiForgeryToken), payload);
        const data = unwrapOrThrow(response, "Ошибка загрузки отчёта.");

        reportItems = Array.isArray(data) ? data : [];
        reportUpdatedAt = new Date();

        applySortAndRender();
        document.dispatchEvent(new CustomEvent("crm-report-rendered"));

        return true;
    } catch (error) {
        console.error(error);
        return false;
    }
}

function buildReportPayload() {
    const state = typeof window.readState === "function" ? window.readState() : {};

    const employees = state.employees || [];
    const groups = state.groups || [];
    const selectedPlanId = normalizeGuid(state.selectedPlanId || getSelectedPlanId());

    return {
        dateFrom: state.dateFrom ? new Date(state.dateFrom).toISOString() : null,
        dateTo: state.dateTo ? new Date(state.dateTo).toISOString() : null,
        planId: selectedPlanId,
        employeeIds: employees.length > 0 ? employees.map(Number) : null,
        groupIds: employees.length === 0 && groups.length > 0 ? groups.map(Number) : null,
        statusIds: (state.statuses || []).map(Number),
        priorityIds: (state.priorities || []).map(Number),
        typeIds: (state.types || []).map(Number),
        hideWithoutSolved: !!state.hideWithoutSolved,
        hideWithoutCurrent: !!state.hideWithoutCurrent,
        hideWithoutTime: !!state.hideWithoutTime
    };
}

function applySortAndRender() {
    let items = Array.isArray(reportItems) ? [...reportItems] : [];
    items = applyClientFilters(items);

    const sortState = loadSortState();
    if (sortState) sortReportItems(items, sortState.key, sortState.dir);

    renderTableRows(items);
    renderSummaryBar(items);
}

function applyClientFilters(items) {
    const state = typeof window.readState === "function" ? window.readState() : {};
    let filtered = items;

    if (state.hideWithoutSolved) filtered = filtered.filter((x) => Number(x?.solvedIssues ?? 0) > 0);
    if (state.hideWithoutTime) filtered = filtered.filter((x) => Number(x?.spentedTime ?? 0) > 0);
    if (state.hideWithoutCurrent) filtered = filtered.filter((x) => Number(x?.currentIssuesCount ?? 0) > 0);

    return filtered;
}

function renderTableRows(items) {
    const tbody = document.getElementById("rows");
    if (!tbody) return;

    tbody.textContent = "";

    const state = typeof window.readState === "function" ? window.readState() : {};
    const selectedPlanId = normalizeGuid(state.selectedPlanId || getSelectedPlanId());
    const showPlanColumn = !!selectedPlanId;

    let order = Array.isArray(columnOrder) && columnOrder.length > 0
        ? [...columnOrder]
        : ["name", "resolved", "plan", "current", "time"];

    if (!showPlanColumn) {
        order = order.filter((x) => x !== "plan");
    } else if (!order.includes("plan")) {
        order.splice(2, 0, "plan");
    }

    syncHeaderColumns(order);

    if (!items || items.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");
        cell.colSpan = order.length;
        cell.className = "text-center text-muted py-4";
        cell.textContent = "Нет данных";
        row.appendChild(cell);
        tbody.appendChild(row);
        return;
    }

    for (const item of items) {
        const row = document.createElement("tr");
        for (const columnId of order) {
            row.appendChild(buildCellForColumn(columnId, item));
        }
        tbody.appendChild(row);
    }

    applyRowDensity(0.3);
}

function buildCellForColumn(columnId, item) {
    const td = document.createElement("td");

    if (columnId === "name") {
        td.textContent = buildFullName(item);
        return td;
    }

    if (columnId === "resolved") {
        const solved = Number(item?.solvedIssues ?? 0);
        td.className = "text-center";
        td.textContent = String(solved);

        const planValue = Number(item?.planValue ?? 0);
        const color = getResolvedTextColorByPlan(solved, planValue);
        if (color) td.style.color = color;

        return td;
    }

    if (columnId === "plan") {
        td.className = "text-center";

        const planValue = item?.planValue;
        const text = planValue === null || planValue === undefined ? "" : String(planValue);

        const span = document.createElement("span");
        span.textContent = text;

        const planColor = String(item?.planColor || "").trim();
        if (/^#([0-9a-fA-F]{6})$/.test(planColor)) {
            span.style.color = planColor.toUpperCase();
        }

        td.appendChild(span);
        return td;
    }

    if (columnId === "current") {
        td.className = "text-center";
        td.textContent = String(Number(item?.currentIssuesCount ?? 0));
        return td;
    }

    if (columnId === "time") {
        const spent = Number(item?.spentedTime ?? 0);
        td.className = "text-center";
        td.textContent = formatHours(spent);
        td.title = `${Math.round(spent * 60)} минут`;
        return td;
    }

    td.textContent = "";
    return td;
}

function renderSummaryBar(items) {
    const left = document.getElementById("summaryLeft");
    const right = document.getElementById("summaryRight");
    if (!left || !right) return;

    const rows = Array.isArray(items) ? items : [];
    const totalEmployees = rows.length;

    let totalSolved = 0;
    let totalCurrent = 0;
    let totalMinutes = 0;

    for (const item of rows) {
        totalSolved += Number(item?.solvedIssues ?? 0);
        totalCurrent += Number(item?.currentIssuesCount ?? 0);
        totalMinutes += Math.round(Number(item?.spentedTime ?? 0) * 60);
    }

    const updatedText = reportUpdatedAt
        ? `• Обновлено: ${formatLastUpdated(reportUpdatedAt)} • `
        : "";

    const selectedPlanMeta =
        typeof window.getSelectedReportPlanMeta === "function"
            ? window.getSelectedReportPlanMeta()
            : null;

    const planBadge = selectedPlanMeta && selectedPlanMeta.name
        ? ` • <span class="px-2 py-1 rounded-1 fw-semibold"
               style="${selectedPlanMeta.color ? `background-color:${selectedPlanMeta.color}; color:#111;` : ""}">
           План: ${escapeHtml(selectedPlanMeta.name)}
       </span>`
        : "";

    left.innerHTML =
        `${updatedText}Сотрудников: ${totalEmployees} • Решённые: ${totalSolved} • Текущие: ${totalCurrent}${planBadge}`;

    right.textContent = `Время: ${formatMinutesShort(totalMinutes)}`;
}

function getResolvedTextColorByPlan(solved, plan) {
    if (!Number.isFinite(solved) || !Number.isFinite(plan) || plan <= 0) {
        return "";
    }

    const percent = Math.floor((solved / plan) * 100);
    if (typeof window.getPlanColorByPercent === "function") {
        return window.getPlanColorByPercent(percent) || "";
    }

    return "";
}

function getSelectedPlanId() {
    if (typeof window.getSelectedReportPlanId === "function") {
        return normalizeGuid(window.getSelectedReportPlanId());
    }

    const element = document.getElementById("reportPlanSelect");
    return normalizeGuid(element ? element.value : null);
}

function normalizeGuid(value) {
    const text = String(value || "").trim();
    return text.length > 0 ? text : null;
}

function formatHours(hours) {
    if (!hours || hours <= 0) return "0 минут";

    const totalMinutes = Math.round(hours * 60);
    const h = Math.floor(totalMinutes / 60);
    const m = totalMinutes % 60;

    if (h > 0 && m > 0) return `${h} ч. ${m} м.`;
    if (h > 0) return `${h} ч.`;
    return `${m} м.`;
}

function formatMinutesShort(totalMinutes) {
    if (!totalMinutes || totalMinutes <= 0) return "0 м.";
    const h = Math.floor(totalMinutes / 60);
    const m = totalMinutes % 60;
    if (h > 0 && m > 0) return `${h} ч. ${m} м.`;
    if (h > 0) return `${h} ч.`;
    return `${m} м.`;
}

function formatLastUpdated(date) {
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    return `${hours}:${minutes}`;
}

function buildFullName(item) {
    const ln = item?.lastName || "";
    const fn = item?.firstName || "";
    const pn = item?.patronymic || "";
    return [ln, fn, pn].filter((x) => x && x.trim().length > 0).join(" ").trim();
}

function loadSortState() {
    const raw = localStorage.getItem(reportSortKey);
    if (!raw) return null;

    try {
        const state = JSON.parse(raw);
        if (!state || !state.key || !state.dir) return null;
        return state;
    } catch {
        return null;
    }
}

function saveSortState(state) {
    if (!state) localStorage.removeItem(reportSortKey);
    else localStorage.setItem(reportSortKey, JSON.stringify(state));
}

function clearSortIndicators() {
    document.querySelectorAll("#grid th[data-sort-key] .sort-indicator").forEach((x) => {
        x.classList.add("opacity-0");
        x.textContent = "▲";
    });
}

function updateSortIndicators() {
    clearSortIndicators();

    const state = loadSortState();
    if (!state) return;

    const th = document.querySelector(`#grid th[data-sort-key="${state.key}"]`);
    if (!th) return;

    const indicator = th.querySelector(".sort-indicator");
    if (!indicator) return;

    indicator.classList.remove("opacity-0");
    indicator.textContent = state.dir === "asc" ? "▲" : "▼";
}

function sortReportItems(items, key, dir) {
    const sign = dir === "desc" ? -1 : 1;

    const getName = (item) => (buildFullName(item) || "").trim().toLowerCase();
    const getResolved = (item) => Number(item?.solvedIssues ?? 0);
    const getPlan = (item) => Number(item?.planValue ?? 0);
    const getCurrent = (item) => Number(item?.currentIssuesCount ?? 0);
    const getTime = (item) => Number(item?.spentedTime ?? 0);

    const cmpNum = (a, b) => a === b ? 0 : (a < b ? -1 : 1);
    const cmpStr = (a, b) => a.localeCompare(b, "ru");

    items.sort((a, b) => {
        let result = 0;

        if (key === "name") result = cmpStr(getName(a), getName(b));
        else if (key === "resolved") result = cmpNum(getResolved(a), getResolved(b));
        else if (key === "plan") result = cmpNum(getPlan(a), getPlan(b));
        else if (key === "current") result = cmpNum(getCurrent(a), getCurrent(b));
        else if (key === "time") result = cmpNum(getTime(a), getTime(b));

        if (result === 0 && key !== "name") result = cmpStr(getName(a), getName(b));
        return result * sign;
    });
}

function resetReportSorting() {
    saveSortState(null);
    updateSortIndicators();
    applySortAndRender();
}

function ensureSortIndicators() {
    const grid = document.getElementById("grid");
    if (!grid) return;

    grid.querySelectorAll("th[data-sort-key]").forEach((th) => {
        th.style.cursor = "pointer";
        th.classList.add("user-select-none");

        if (th.querySelector(".sort-indicator")) return;

        const indicator = document.createElement("span");
        indicator.className = "sort-indicator ms-1 opacity-0";
        indicator.textContent = "▲";
        th.appendChild(indicator);
    });
}

function changeFontScale(delta) {
    const current = loadFontScale();
    const next = clamp(round1(current + delta), 0.7, 2.0);
    saveFontScale(next);
    applyFontScale(next);
}

function applyFontScale(scale) {
    const grid = document.getElementById("grid");
    if (grid) grid.style.fontSize = `${scale}em`;

    const label = document.getElementById("fontScaleLabel");
    if (label) label.textContent = `(${scale.toFixed(1)})`;
}

function loadFontScale() {
    const raw = localStorage.getItem(reportFontScaleKey);
    const value = raw ? Number(raw) : NaN;
    if (!Number.isFinite(value)) return 1.0;
    return clamp(value, 0.7, 2.0);
}

function saveFontScale(scale) {
    localStorage.setItem(reportFontScaleKey, String(scale));
}

function clamp(value, min, max) {
    return Math.min(max, Math.max(min, value));
}

function round1(value) {
    return Math.round(value * 10) / 10;
}

function applyRowDensity(scale) {
    const grid = document.getElementById("grid");
    if (!grid) return;

    const basePadding = 8;
    const px = Math.max(2, Math.round(basePadding * scale));

    grid.querySelectorAll("th, td").forEach((cell) => {
        cell.style.paddingTop = `${px}px`;
        cell.style.paddingBottom = `${px}px`;
    });
}

function toggleReportFocus() {
    const isOn = document.body.classList.contains("report-focus-on");
    if (isOn) disableReportFocus();
    else enableReportFocus();
}

function enableReportFocus() {
    document.body.classList.add("report-focus-on");
    localStorage.setItem(reportFocusKey, "1");

    const side = document.getElementById("side");
    if (side) side.classList.add("d-none");

    const header = document.querySelector("header");
    if (header) header.classList.add("d-none");

    const main = document.querySelector("main");
    if (main) {
        main.classList.add("p-0");
        main.classList.remove("p-3");
    }

    const root = document.getElementById("reportFocusRoot");
    const viewport = document.getElementById("reportFocusViewport");
    const FULLSCREEN_TARGET = viewport || root;

    if (FULLSCREEN_TARGET && FULLSCREEN_TARGET.requestFullscreen && !document.fullscreenElement) {
        FULLSCREEN_TARGET.requestFullscreen().catch(() => { });
    }

    if (root) {
        root.style.minHeight = "100vh";
        root.style.overflowY = "auto";
    }

    if (viewport) {
        viewport.style.minHeight = "100vh";
    }

    const bar = document.getElementById("reportSummaryBar");
    if (bar) {
        bar.style.position = "fixed";
        bar.style.left = "0";
        bar.style.right = "0";
        bar.style.bottom = "0";
        bar.style.zIndex = "3000";
    }

    if (root) root.style.paddingBottom = bar ? `${bar.offsetHeight}px` : "48px";
}

function disableReportFocus() {
    document.body.classList.remove("report-focus-on");
    localStorage.setItem(reportFocusKey, "0");

    const side = document.getElementById("side");
    if (side) side.classList.remove("d-none");

    const header = document.querySelector("header");
    if (header) header.classList.remove("d-none");

    const main = document.querySelector("main");
    if (main) main.classList.remove("p-0");

    if (document.fullscreenElement) document.exitFullscreen().catch(() => { });

    const root = document.getElementById("reportFocusRoot");
    const viewport = document.getElementById("reportFocusViewport");
    if (root) {
        root.style.minHeight = "";
        root.style.overflowY = "";
        root.style.paddingBottom = "";
    }

    if (viewport) {
        viewport.style.minHeight = "";
    }

    const bar = document.getElementById("reportSummaryBar");
    if (bar) {
        bar.style.position = "";
        bar.style.left = "";
        bar.style.right = "";
        bar.style.bottom = "";
    }
}

function loadColumnOrder(defaultOrder) {
    const raw = localStorage.getItem(reportColOrderKey);
    if (!raw) return [...defaultOrder];

    try {
        const arr = JSON.parse(raw);
        if (!Array.isArray(arr) || arr.length === 0) return [...defaultOrder];

        const set = new Set(defaultOrder);
        const cleaned = arr.filter((x) => typeof x === "string" && set.has(x));
        for (const x of defaultOrder) {
            if (!cleaned.includes(x)) cleaned.push(x);
        }

        return cleaned;
    } catch {
        return [...defaultOrder];
    }
}

function saveColumnOrder(order) {
    localStorage.setItem(reportColOrderKey, JSON.stringify(order));
}

function applyColumnOrderToHeader(order) {
    const grid = document.getElementById("grid");
    if (!grid) return;

    const theadRow = grid.querySelector("thead tr");
    if (!theadRow) return;

    const map = {};
    theadRow.querySelectorAll("th[data-col-id]").forEach((th) => {
        const id = th.dataset.colId;
        if (id) map[id] = th;
    });

    for (const id of order) {
        const th = map[id];
        if (th) theadRow.appendChild(th);
    }

    const ths = Array.from(theadRow.querySelectorAll("th[data-col-id]"));
    ths.forEach((th, idx) => {
        th.classList.remove("border-end", "border-secondary-subtle", "border-secondary");
        if (idx !== ths.length - 1) th.classList.add("border-end", "border-secondary-subtle");
    });
}

function moveArrayItemBySide(arr, srcId, targetId, side) {
    const srcIdx = arr.indexOf(srcId);
    const tgtIdx = arr.indexOf(targetId);
    if (srcIdx < 0 || tgtIdx < 0) return null;

    const next = [...arr];
    next.splice(srcIdx, 1);

    let insertIdx = next.indexOf(targetId);
    if (insertIdx < 0) return null;
    if (side === "after") insertIdx += 1;

    next.splice(insertIdx, 0, srcId);
    return next;
}

function ensureHeaderInsertIndicator() {
    if (headerInsertEl) return;

    const grid = document.getElementById("grid");
    if (!grid) return;

    const wrap = grid.closest(".table-responsive") || grid.parentElement;
    if (!wrap) return;

    if (getComputedStyle(wrap).position === "static") wrap.style.position = "relative";

    const indicator = document.createElement("div");
    indicator.style.position = "absolute";
    indicator.style.top = "0";
    indicator.style.bottom = "0";
    indicator.style.width = "3px";
    indicator.style.transform = "translateX(-1px)";
    indicator.style.background = "rgba(13,110,253,0.95)";
    indicator.style.boxShadow = "0 0 0 2px rgba(13,110,253,0.15)";
    indicator.style.borderRadius = "2px";
    indicator.style.pointerEvents = "none";
    indicator.style.display = "none";
    indicator.style.zIndex = "2000";

    wrap.appendChild(indicator);
    headerInsertEl = indicator;
}

function showHeaderInsertIndicatorAt(xClient, yClient) {
    ensureHeaderInsertIndicator();
    if (!headerInsertEl) return;

    const grid = document.getElementById("grid");
    if (!grid) return;

    const wrap = grid.closest(".table-responsive") || grid.parentElement;
    if (!wrap) return;

    const theadRow = grid.querySelector("thead tr");
    if (!theadRow) return;

    const th = document.elementFromPoint(xClient, yClient)?.closest("th[data-col-id]");
    if (!th || !theadRow.contains(th)) {
        hideHeaderInsertIndicator();
        return;
    }

    const id = th.dataset.colId;
    if (!id) {
        hideHeaderInsertIndicator();
        return;
    }

    const rect = th.getBoundingClientRect();
    const mid = rect.left + rect.width / 2;
    const side = xClient < mid ? "before" : "after";

    const wrapRect = wrap.getBoundingClientRect();
    const x = side === "before" ? rect.left : rect.right;

    headerInsert.targetId = id;
    headerInsert.side = side;

    headerInsertEl.style.left = `${Math.round(x - wrapRect.left + wrap.scrollLeft)}px`;

    const theadRect = theadRow.getBoundingClientRect();
    const top = Math.round(theadRect.top - wrapRect.top + wrap.scrollTop);
    const bottom = Math.round(theadRect.bottom - wrapRect.top + wrap.scrollTop);

    headerInsertEl.style.top = `${top}px`;
    headerInsertEl.style.height = `${Math.max(12, bottom - top)}px`;
    headerInsertEl.style.display = "block";
}

function hideHeaderInsertIndicator() {
    headerInsert.targetId = null;
    headerInsert.side = null;
    if (headerInsertEl) headerInsertEl.style.display = "none";
}

function setHeaderDragSourceVisual(id) {
    const grid = document.getElementById("grid");
    if (!grid) return;

    grid.querySelectorAll("thead th[data-col-id]").forEach((th) => {
        th.style.opacity = "";
        th.style.transition = "opacity 120ms ease";
    });

    if (!id) return;

    const th = grid.querySelector(`thead th[data-col-id="${CSS.escape(id)}"]`);
    if (th) th.style.opacity = "0.6";
}

function clearHeaderDragSourceVisual() {
    const grid = document.getElementById("grid");
    if (!grid) return;

    grid.querySelectorAll("thead th[data-col-id]").forEach((th) => {
        th.style.opacity = "";
        th.style.transition = "";
    });
}

function syncHeaderColumns(order) {
    const grid = document.getElementById("grid");
    if (!grid) return;

    const theadRow = grid.querySelector("thead tr");
    if (!theadRow) return;

    const visible = new Set(order);

    const ths = Array.from(theadRow.querySelectorAll("th[data-col-id]"));
    ths.forEach((th) => {
        const id = th.dataset.colId;
        const show = !!id && visible.has(id);

        th.classList.toggle("d-none", !show);
        th.setAttribute("draggable", show ? "true" : "false");
    });

    const shown = ths.filter((th) => !th.classList.contains("d-none"));
    shown.forEach((th, idx) => {
        th.classList.remove("border-end", "border-secondary-subtle", "border-secondary");
        if (idx !== shown.length - 1) th.classList.add("border-end", "border-secondary-subtle");
    });
}

function escapeHtml(value) {
    return String(value || "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}