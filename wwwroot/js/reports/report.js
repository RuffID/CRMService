const reportSortKey = "crm_report_sort_v1";
const reportFontScaleKey = "crm_report_font_scale_v1";
const reportFocusKey = "crm_report_focus_v1";
const reportColOrderKey = "crm_report_col_order_v1";
const reportPlanTextColorKey = "crm_report_plan_text_color_v1";

let isManualReloadInProgress = false;
let antiForgeryToken = null;
let lastReportItems = [];
let isHeaderDragging = false;
let dragOverColId = null;
let dragSrcColId = null;
let headerInsert = { targetId: null, side: null };
let headerInsertEl = null;
let lastReportUpdatedAt = null;

let columnOrder = null;
let planModePeriod = "month";
let planColorRules = [];

window.resetReportSorting = resetReportSorting;
window.loadPerformanceReport = loadPerformanceReport;
window.applySortAndRender = applySortAndRender;
window.getPlanModePeriod = () => planModePeriod;
window.setPlanModePeriod = (p) => { planModePeriod = (p === "day") ? "day" : "month"; };

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

    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el);
    });
}

async function initPlanColors() {
    try {
        const resp = await sendJsonRequest(
            `?handler=PlanColorRules`,
            "GET",
            buildJsonHeaders(antiForgeryToken)
        );

        const data = unwrapOrThrow(resp, "Ошибка загрузки цветовой схемы.");
        planColorRules = Array.isArray(data) ? data : [];

        window.getPlanColorByPercent = (pct) => getPlanColorByPercentFromRules(pct, planColorRules);
    } catch {
        planColorRules = [];
        window.getPlanColorByPercent = () => "";
    }
}

function getPlanColorByPercentFromRules(pct, rules) {
    const p = Number(pct);
    if (!Number.isFinite(p)) return "";

    const items = Array.isArray(rules) ? rules : [];
    for (const r of items) {
        const from = Number(r?.fromPercent);
        const to = Number(r?.toPercent);
        const color = String(r?.color || "").trim();

        if (!Number.isFinite(from) || !Number.isFinite(to)) continue;
        if (to <= from) continue;
        if (!/^#([0-9a-fA-F]{6})$/.test(color)) continue;

        if (p >= from && p < to) return color.toUpperCase();
    }

    return "";
}

function initReportSorting() {
    const grid = document.getElementById("grid");
    if (!grid) return;

    ensureSortIndicators();

    grid.querySelectorAll("th[data-sort-key]").forEach(th => {
        th.addEventListener("click", () => {
            if (isHeaderDragging) return;

            const key = th.dataset.sortKey;
            if (!key) return;

            const st = loadSortState();

            let dir = "asc";
            if (st && st.key === key) dir = (st.dir === "asc") ? "desc" : "asc";

            saveSortState({ key, dir });
            updateSortIndicators();
            applySortAndRender();
        });
    });

    updateSortIndicators();
}

function initReportReloadButton() {
    const btn = document.getElementById("reportReload");
    if (!btn) return;

    btn.addEventListener("click", async () => {
        if (isManualReloadInProgress) return;

        isManualReloadInProgress = true;
        btn.disabled = true;

        const ok = await loadPerformanceReport();

        if (ok) await new Promise(r => setTimeout(r, 2000));

        btn.disabled = false;
        isManualReloadInProgress = false;
    });
}

function initReportFontScale() {
    const btnMinus = document.getElementById("fontMinus");
    const btnPlus = document.getElementById("fontPlus");
    if (!btnMinus || !btnPlus) return;

    applyFontScale(loadFontScale());

    btnMinus.addEventListener("click", () => changeFontScale(-0.1));
    btnPlus.addEventListener("click", () => changeFontScale(+0.1));
}

function initReportFocusMode() {
    const btn = document.getElementById("reportFocusToggle");
    if (!btn) return;

    btn.addEventListener("click", () => toggleReportFocus());

    const saved = localStorage.getItem(reportFocusKey);
    if (saved === "1") enableReportFocus();
}

function initReportColumnOrder() {
    const grid = document.getElementById("grid");
    if (!grid) return;

    const theadRow = grid.querySelector("thead tr");
    if (!theadRow) return;

    const defaultOrder = Array.from(theadRow.querySelectorAll("th[data-col-id]"))
        .map(th => th.dataset.colId)
        .filter(x => !!x);

    columnOrder = loadColumnOrder(defaultOrder);
    applyColumnOrderToHeader(columnOrder);
    ensureHeaderInsertIndicator();

    let dragSrcId = null;

    theadRow.querySelectorAll("th[data-col-id]").forEach(th => {
        th.addEventListener("dragstart", (e) => {
            const id = th.dataset.colId;
            if (!id) return;

            isHeaderDragging = true;
            dragSrcId = id;
            dragSrcColId = id;

            setHeaderDragSourceVisual(dragSrcColId);

            try { e.dataTransfer.effectAllowed = "move"; } catch { }
            try { e.dataTransfer.setData("text/plain", id); } catch { }
        });

        th.addEventListener("dragend", () => {
            isHeaderDragging = false;
            dragSrcId = null;
            dragSrcColId = null;

            hideHeaderInsertIndicator();
            clearHeaderDragSourceVisual();
        });

        th.addEventListener("dragover", (e) => {
            e.preventDefault();
            showHeaderInsertIndicatorAt(e.clientX, e.clientY);
            try { e.dataTransfer.dropEffect = "move"; } catch { }
        });

        th.addEventListener("drop", (e) => {
            e.preventDefault();

            const targetId = headerInsert.targetId;
            const side = headerInsert.side;

            let srcId = null;
            try { srcId = e.dataTransfer.getData("text/plain"); } catch { }
            if (!srcId) srcId = dragSrcId;

            hideHeaderInsertIndicator();
            clearHeaderDragSourceVisual();

            if (!srcId || !targetId || !side) return;
            if (srcId === targetId) return;

            const next = moveArrayItemBySide(columnOrder, srcId, targetId, side);
            if (!next || next.length === 0) return;

            columnOrder = next;
            saveColumnOrder(columnOrder);
            applyColumnOrderToHeader(columnOrder);

            updateSortIndicators();
            applySortAndRender();
        });
    });
}

async function loadPerformanceReport() {
    const payload = buildReportPayload();

    try {
        const response = await sendJsonRequest(`?handler=Report`, "POST", buildJsonHeaders(antiForgeryToken), payload);

        const data = unwrapOrThrow(response, "Ошибка загрузки отчёта.");
        lastReportItems = Array.isArray(data) ? data : [];
        lastReportUpdatedAt = new Date();
        applySortAndRender();
        return true;
    } catch (e) {
        console.error(e);
        return false;
    }
}

function buildReportPayload() {
    const st = (typeof window.readState === "function") ? window.readState() : {};

    const employees = st.employees || [];
    const groups = st.groups || [];

    return {
        dateFrom: st.dateFrom ? new Date(st.dateFrom).toISOString() : null,
        dateTo: st.dateTo ? new Date(st.dateTo).toISOString() : null,

        employeeIds: employees.length > 0 ? employees.map(Number) : null,
        groupIds: employees.length === 0 && groups.length > 0 ? groups.map(Number) : null,

        statusIds: (st.statuses || []).map(Number),
        priorityIds: (st.priorities || []).map(Number),
        typeIds: (st.types || []).map(Number),

        hideWithoutSolved: !!st.hideWithoutSolved,
        hideWithoutCurrent: !!st.hideWithoutCurrent,
        hideWithoutTime: !!st.hideWithoutTime
    };
}

function applySortAndRender() {
    let items = Array.isArray(lastReportItems) ? [...lastReportItems] : [];

    items = applyClientFilters(items);

    const st = loadSortState();
    if (st) sortReportItems(items, st.key, st.dir);

    renderTableRows(items);
    renderSummaryBar(items);
}

function applyClientFilters(items) {
    const st = (typeof window.readState === "function") ? window.readState() : {};

    let res = items;

    if (st.hideWithoutSolved) res = res.filter(x => Number(x?.solvedIssues ?? 0) > 0);
    if (st.hideWithoutTime) res = res.filter(x => Number(x?.spentedTime ?? 0) > 0);
    if (st.hideWithoutCurrent) res = res.filter(x => Number(x?.issues?.length ?? 0) > 0);

    return res;
}

function renderTableRows(items) {
    const tbody = document.getElementById("rows");
    tbody.innerHTML = "";

    let order = Array.isArray(columnOrder) && columnOrder.length > 0
        ? [...columnOrder]
        : ["name", "resolved", "current", "time"];

    if (!isPlanModeOn()) order = order.filter(x => x !== "plan");

    syncHeaderColumns(order);

    if (!items || items.length === 0) {
        const tr = document.createElement("tr");

        const td = document.createElement("td");
        td.colSpan = order.length;
        td.className = "text-center text-muted py-4";
        td.textContent = "Нет данных";

        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }

    for (const it of items) {
        const tr = document.createElement("tr");

        for (const col of order) {
            tr.appendChild(buildCellForColumn(col, it));
        }

        tbody.appendChild(tr);
    }

    applyRowDensity(0.3);
}

function buildCellForColumn(colId, it) {
    const td = document.createElement("td");

    if (colId === "name") {
        td.textContent = buildFullName(it);
        return td;
    }

    if (colId === "resolved") {
        td.className = "text-center";

        const solved = Number(it?.solvedIssues ?? 0);
        td.textContent = String(solved);

        if (isPlanModeOn()) {
            const plan = getPlanValueForItem(it);
            const c = getResolvedTextColorByPlan(solved, plan);

            if (c) {
                td.style.color = c;
            } else {
                td.style.color = "";
                td.style.fontWeight = "";
            }
        } else {
            td.style.color = "";
            td.style.fontWeight = "";
        }

        return td;
    }

    if (colId === "plan") {
        td.className = "text-center";

        const plan = getPlanValueForItem(it);
        const txt = (plan === null || plan === undefined) ? "" : String(plan);

        const span = document.createElement("span");
        span.textContent = txt;

        if (txt)
            span.style.color = getPlanTextColor();        

        td.appendChild(span);
        return td;
    }

    if (colId === "current") {
        td.className = "text-center";
        td.textContent = String(it?.issues ? it.issues.length : 0);
        return td;
    }

    if (colId === "time") {
        const spent = Number(it?.spentedTime ?? 0);
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

    for (const it of rows) {
        totalSolved += Number(it?.solvedIssues ?? 0);
        totalCurrent += Number(it?.issues?.length ?? 0);
        totalMinutes += Math.round(Number(it?.spentedTime ?? 0) * 60);
    }

    const updatedText = lastReportUpdatedAt
        ? `• Обновлено: ${formatLastUpdated(lastReportUpdatedAt)} • `
        : "";

    const periodTag = isPlanModeOn()
        ? (planModePeriod === "day"
            ? ' • <span style="color:#0d6efd;">Период: день</span>'
            : ' • <span style="color:#dc3545;">Период: месяц</span>')
        : "";

    left.innerHTML = `${updatedText}Сотрудников: ${totalEmployees} • Решённые: ${totalSolved} • Текущие: ${totalCurrent}${periodTag}`;
    right.textContent = `Время: ${formatMinutesShort(totalMinutes)}`;
}

function getPlanValueForItem(it) {
    const p = getActivePeriod();
    const raw = (p === "day") ? (it?.dayPlan ?? null) : (p === "month" ? (it?.monthPlan ?? null) : null);
    const n = Number(raw);
    if (!Number.isFinite(n) || n <= 0)
        return null; // <= 0 скрываем

    return n
}

function getPlanTextColor() {
    const v = String(localStorage.getItem(reportPlanTextColorKey) || "").trim();
    if (/^#([0-9a-fA-F]{6})$/.test(v)) return v.toUpperCase();
    return "#FFC107";
}

function getResolvedTextColorByPlan(solved, plan) {
    const s = Number(solved ?? 0);
    const p = Number(plan ?? 0);

    if (!Number.isFinite(s) || !Number.isFinite(p) || p <= 0)
        return "";

    if (s === 0)
        return "#000000";

    const pct = Math.floor((s / p) * 100);
    if (typeof window.getPlanColorByPercent === "function")
        return window.getPlanColorByPercent(pct) || "";

    return "";
}

function getActivePeriod() {
    if (isPlanModeOn()) return planModePeriod;
    if (isEoModeOn()) return "day";
    return null;
}

function isEoModeOn() {
    const el = document.getElementById("eoMode");
    return !!el && el.checked === true;
}

function isPlanModeOn() {
    const el = document.getElementById("planMode");
    return !!el && el.checked === true;
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

function formatLastUpdated(d) {
    const hh = String(d.getHours()).padStart(2, "0");
    const mm = String(d.getMinutes()).padStart(2, "0");
    return `${hh}:${mm}`;
}

function buildFullName(it) {
    const ln = it?.lastName || "";
    const fn = it?.firstName || "";
    const pn = it?.patronymic || "";
    return [ln, fn, pn].filter(x => x && x.trim().length > 0).join(" ").trim();
}

function loadSortState() {
    const raw = localStorage.getItem(reportSortKey);
    if (!raw) return null;

    try {
        const st = JSON.parse(raw);
        if (!st || !st.key || !st.dir) return null;
        return st;
    } catch {
        return null;
    }
}

function saveSortState(state) {
    if (!state) localStorage.removeItem(reportSortKey);
    else localStorage.setItem(reportSortKey, JSON.stringify(state));
}

function clearSortIndicators() {
    document.querySelectorAll('#grid th[data-sort-key] .sort-indicator').forEach(x => {
        x.classList.add("opacity-0");
        x.textContent = "▲";
    });
}

function updateSortIndicators() {
    clearSortIndicators();

    const st = loadSortState();
    if (!st) return;

    const th = document.querySelector(`#grid th[data-sort-key="${st.key}"]`);
    if (!th) return;

    const ind = th.querySelector(".sort-indicator");
    if (!ind) return;

    ind.classList.remove("opacity-0");
    ind.textContent = (st.dir === "asc") ? "▲" : "▼";
}

function sortReportItems(items, key, dir) {
    const sign = (dir === "desc") ? -1 : 1;

    const getName = (it) => (buildFullName(it) || "").trim().toLowerCase();
    const getResolved = (it) => Number(it?.solvedIssues ?? 0);
    const getCurrent = (it) => Number(it?.issues?.length ?? 0);
    const getTime = (it) => Number(it?.spentedTime ?? 0);

    const cmpNum = (a, b) => (a === b ? 0 : (a < b ? -1 : 1));
    const cmpStr = (a, b) => a.localeCompare(b, "ru");

    items.sort((a, b) => {
        let r = 0;

        if (key === "name") r = cmpStr(getName(a), getName(b));
        else if (key === "resolved") r = cmpNum(getResolved(a), getResolved(b));
        else if (key === "current") r = cmpNum(getCurrent(a), getCurrent(b));
        else if (key === "time") r = cmpNum(getTime(a), getTime(b));
        else r = 0;

        if (r === 0 && key !== "name") r = cmpStr(getName(a), getName(b));
        return r * sign;
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

    grid.querySelectorAll("th[data-sort-key]").forEach(th => {
        th.style.cursor = "pointer";
        th.classList.add("user-select-none");

        if (th.querySelector(".sort-indicator")) return;

        const ind = document.createElement("span");
        ind.className = "sort-indicator ms-1 opacity-0";
        ind.textContent = "▲";
        th.appendChild(ind);
    });
}

function changeFontScale(delta) {
    const cur = loadFontScale();
    const next = clamp(round1(cur + delta), 0.7, 2.0);
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
    const v = raw ? Number(raw) : NaN;
    if (!Number.isFinite(v)) return 1.0;
    return clamp(v, 0.7, 2.0);
}

function saveFontScale(scale) {
    localStorage.setItem(reportFontScaleKey, String(scale));
}

function clamp(v, min, max) {
    return Math.min(max, Math.max(min, v));
}

function round1(v) {
    return Math.round(v * 10) / 10;
}

function applyRowDensity(scale) {
    const grid = document.getElementById("grid");
    if (!grid) return;

    const basePadding = 8;
    const px = Math.max(2, Math.round(basePadding * scale));

    grid.querySelectorAll("th, td").forEach(cell => {
        cell.style.paddingTop = px + "px";
        cell.style.paddingBottom = px + "px";
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
    if (root && root.requestFullscreen && !document.fullscreenElement) {
        root.requestFullscreen().catch(() => { });
    }

    if (root) {
        root.style.minHeight = "100vh";
        root.style.overflowY = "auto";
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
    if (root) {
        root.style.minHeight = "";
        root.style.overflowY = "";
        root.style.paddingBottom = "";
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
        const cleaned = arr.filter(x => typeof x === "string" && set.has(x));
        for (const x of defaultOrder) if (!cleaned.includes(x)) cleaned.push(x);
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
    theadRow.querySelectorAll("th[data-col-id]").forEach(th => {
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

    const el = document.createElement("div");
    el.style.position = "absolute";
    el.style.top = "0";
    el.style.bottom = "0";
    el.style.width = "3px";
    el.style.transform = "translateX(-1px)";
    el.style.background = "rgba(13,110,253,0.95)";
    el.style.boxShadow = "0 0 0 2px rgba(13,110,253,0.15)";
    el.style.borderRadius = "2px";
    el.style.pointerEvents = "none";
    el.style.display = "none";
    el.style.zIndex = "2000";

    wrap.appendChild(el);
    headerInsertEl = el;
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

    const r = th.getBoundingClientRect();
    const mid = r.left + r.width / 2;
    const side = xClient < mid ? "before" : "after";

    const wrapRect = wrap.getBoundingClientRect();
    const x = (side === "before") ? r.left : r.right;

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

    grid.querySelectorAll("thead th[data-col-id]").forEach(th => {
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

    grid.querySelectorAll("thead th[data-col-id]").forEach(th => {
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
    ths.forEach(th => {
        const id = th.dataset.colId;
        const show = !!id && visible.has(id);

        th.classList.toggle("d-none", !show);
        th.setAttribute("draggable", show ? "true" : "false");
    });

    const shown = ths.filter(th => !th.classList.contains("d-none"));
    shown.forEach((th, idx) => {
        th.classList.remove("border-end", "border-secondary-subtle", "border-secondary");
        if (idx !== shown.length - 1) th.classList.add("border-end", "border-secondary-subtle");
    });
}