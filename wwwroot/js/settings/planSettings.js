const psSortKey = "crm_plan_settings_sort_v1";
const psFilterKey = "crm_plan_settings_filter_v1";
const psGroupFilterKey = "crm_plan_settings_group_filter_v1";
const reportPlanTextColorKey = "crm_report_plan_text_color_v1";
let sharedColorPicker = null;
let sharedColorPickerOnPick = null;
let selectedGroupIds = null;
let antiForgeryToken = null;
let isColorRulesCollapsed = false;
let isPlansCollapsed = false;

let allRows = [];
let visibleRows = [];

let sortState = { key: "name", dir: "asc" };
let selectedEmployeeIds = null;

let isReloading = false;
let isSaving = false;

let colorRules = [];
const maxPercent = 999;

document.addEventListener("DOMContentLoaded", () => {
    antiForgeryToken = getRequestVerificationToken();

    initSorting();
    initButtons();
    initNameFilter();
    initGroupFilter();

    loadInitial();
});

async function loadInitial() {
    loadSavedState();
    initPlanTextColorUi();
    await reloadEmployees();
    await reloadColorRules();
    applyCollapseState();
}

function initButtons() {
    const btnReload = document.getElementById("btnReloadEmployees");
    const btnSave = document.getElementById("btnSavePlans");
    const btnAddRule = document.getElementById("btnAddColorRule");

    btnReload.addEventListener("click", async () => {
        if (isReloading || isSaving) return;
        isReloading = true;
        btnReload.disabled = true;

        await reloadEmployees();

        btnReload.disabled = false;
        isReloading = false;
    });

    btnSave.addEventListener("click", async () => {
        if (isSaving || isReloading) return;
        isSaving = true;
        btnSave.disabled = true;

        clearPageError();

        const okUi = validateColorRulesBeforeSave();
        if (!okUi) {
            btnSave.disabled = false;
            isSaving = false;
            return;
        }

        const okPlanColor = savePlanTextColorToStorage();
        if (!okPlanColor) {
            btnSave.disabled = false;
            isSaving = false;
            return;
        }

        const okPlans = await savePlanSettings();
        const okColors = await saveColorRules();

        if (okPlans && okColors) {
            await reloadEmployees();
            await reloadColorRules();
        }

        btnSave.disabled = false;
        isSaving = false;
    });

    btnAddRule.addEventListener("click", () => {
        colorRules.push({
            id: null,
            fromPercent: 0,
            toPercent: 0,
            color: null
        });
        renderColorRules();
    });

    const btnToggleColors = document.getElementById("btnToggleColorRules");
    if (btnToggleColors) {
        btnToggleColors.addEventListener("click", () => {
            isColorRulesCollapsed = !isColorRulesCollapsed;
            applyCollapseState();
        });
    }

    const btnTogglePlans = document.getElementById("btnTogglePlans");
    if (btnTogglePlans) {
        btnTogglePlans.addEventListener("click", () => {
            isPlansCollapsed = !isPlansCollapsed;
            applyCollapseState();
        });
    }
}

async function reloadEmployees() {
    try {
        const resp = await sendJsonRequest("?handler=EmployeePlanRows", "GET", buildJsonHeaders(antiForgeryToken));
        const data = unwrapOrThrow(resp, "Ошибка загрузки сотрудников.");
        allRows = Array.isArray(data) ? data : [];
        applyFilterSortRender();
        return true;
    } catch (e) {
        console.error(e);
        return false;
    }
}

async function reloadColorRules() {
    try {
        const resp = await sendJsonRequest("?handler=PlanColorRules", "GET", buildJsonHeaders(antiForgeryToken));
        const data = unwrapOrThrow(resp, "Ошибка загрузки цветовой схемы.");
        colorRules = Array.isArray(data) ? data : [];
        sortColorRules();
        renderColorRules();
        return true;
    } catch (e) {
        console.error(e);
        return false;
    }
}

function initSorting() {
    const grid = document.getElementById("plansGrid");
    if (!grid) return;

    grid.querySelectorAll("th[data-sort-key]").forEach(th => {
        th.style.cursor = "pointer";
        th.addEventListener("click", () => {
            const key = th.dataset.sortKey;
            if (!key) return;

            if (sortState.key === key) sortState.dir = (sortState.dir === "asc") ? "desc" : "asc";
            else {
                sortState.key = key;
                sortState.dir = "asc";
            }

            saveState();
            applyFilterSortRender();
        });
    });
}

function initGroupFilter() {
    const btn = document.getElementById("btnGroupFilter");
    const modalEl = document.getElementById("groupFilterModal");
    if (!btn || !modalEl) return;

    const modal = new bootstrap.Modal(modalEl);

    btn.addEventListener("click", (e) => {
        e.stopPropagation();
        rebuildGroupFilterList();
        const locked = hasActiveNameFilter();
        setFilterModalLocked("group", locked);
        modal.show();
    });

    const search = document.getElementById("groupFilterSearch");
    if (search) {
        search.addEventListener("input", () => rebuildGroupFilterList());
    }

    const selAll = document.getElementById("groupFilterSelectAll");
    if (selAll) {
        selAll.addEventListener("click", () => {
            selectedGroupIds = getAllGroupIds();
            saveState();
            rebuildGroupFilterList();
        });
    }

    const clrAll = document.getElementById("groupFilterClearAll");
    if (clrAll) {
        clrAll.addEventListener("click", () => {
            selectedGroupIds = null;
            saveState();
            rebuildGroupFilterList();
        });
    }

    const apply = document.getElementById("groupFilterApply");
    if (apply) {
        apply.addEventListener("click", () => {
            applyFilterSortRender();
        });
    }
}

function applyFilterSortRender() {
    visibleRows = applyEmployeeFilter(allRows);
    sortRows(visibleRows);
    renderPlanRows(visibleRows);
    renderPlanSummary(visibleRows);
    renderPlansHint();
    rebuildNameFilterList();
    loadSavedState();
}

function applyEmployeeFilter(rows) {
    let res = rows;

    if (Array.isArray(selectedEmployeeIds) && selectedEmployeeIds.length > 0) {
        const set = new Set(selectedEmployeeIds.map(Number));
        res = res.filter(x => set.has(Number(x.employeeId)));
    }

    if (Array.isArray(selectedGroupIds) && selectedGroupIds.length > 0) {
        const gs = new Set(selectedGroupIds.map(Number));
        res = res.filter(x => {
            if (!Array.isArray(x.groups) || x.groups.length === 0) return false;
            return x.groups.some(g => gs.has(Number(g.id)));
        });
    }

    return res;
}

function sortRows(rows) {
    const sign = (sortState.dir === "desc") ? -1 : 1;

    const name = (x) => String(x.fullName || "").trim().toLowerCase();
    const groupsKey = (x) => formatGroups(x.groups).toLowerCase();
    const m = (x) => (x.monthPlan === null || x.monthPlan === undefined) ? -1 : Number(x.monthPlan);
    const d = (x) => (x.dayPlan === null || x.dayPlan === undefined) ? -1 : Number(x.dayPlan);

    rows.sort((a, b) => {
        let r = 0;

        if (sortState.key === "name") r = name(a).localeCompare(name(b), "ru");
        else if (sortState.key === "groups") r = groupsKey(a).localeCompare(groupsKey(b), "ru");
        else if (sortState.key === "month") r = (m(a) === m(b)) ? 0 : (m(a) < m(b) ? -1 : 1);
        else if (sortState.key === "day") r = (d(a) === d(b)) ? 0 : (d(a) < d(b) ? -1 : 1);

        if (r === 0 && sortState.key !== "name") r = name(a).localeCompare(name(b), "ru");
        return r * sign;
    });
}

function renderPlanRows(rows) {
    const tbody = document.getElementById("plansRows");
    if (!tbody) return;

    tbody.textContent = "";

    if (!rows || rows.length === 0) {
        const tr = document.createElement("tr");
        const td = document.createElement("td");
        td.colSpan = 4;
        td.className = "text-center text-muted py-4";
        td.textContent = "Нет данных";
        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }

    for (const r of rows) {
        const tr = document.createElement("tr");

        const tdName = document.createElement("td");
        tdName.textContent = r.fullName || "";
        tr.appendChild(tdName);

        const tdGroups = document.createElement("td");
        tdGroups.textContent = formatGroups(r.groups);
        tr.appendChild(tdGroups);

        const tdMonth = document.createElement("td");
        tdMonth.className = "text-center";
        tdMonth.appendChild(buildPlanInput(r.employeeId, "month", r.monthPlan));
        tr.appendChild(tdMonth);

        const tdDay = document.createElement("td");
        tdDay.className = "text-center";
        tdDay.appendChild(buildPlanInput(r.employeeId, "day", r.dayPlan));
        tr.appendChild(tdDay);

        tbody.appendChild(tr);
    }
}

function buildPlanInput(employeeId, kind, value) {
    const input = document.createElement("input");
    input.type = "text";
    input.inputMode = "numeric";
    input.className = "form-control form-control-sm text-center";
    input.style.maxWidth = "140px";
    input.style.margin = "0 auto";
    input.style.display = "block";
    input.dataset.employeeId = String(employeeId);
    input.dataset.kind = kind;
    input.id = `plan_${employeeId}_${kind}`;
    input.name = `plan_${employeeId}_${kind}`;
    input.value = (value === null || value === undefined) ? "" : String(value);

    input.addEventListener("input", () => {
        const cleaned = (input.value || "").replace(/[^\d]/g, "");
        input.value = cleaned;
    });

    return input;
}

function renderPlanSummary(rows) {
    const left = document.getElementById("plansSummaryLeft");
    const right = document.getElementById("plansSummaryRight");
    if (!left || !right) return;

    const cnt = Array.isArray(rows) ? rows.length : 0;

    let filledMonth = 0;
    let filledDay = 0;

    for (const r of (rows || [])) {
        if (r.monthPlan !== null && r.monthPlan !== undefined) filledMonth++;
        if (r.dayPlan !== null && r.dayPlan !== undefined) filledDay++;
    }

    left.textContent = `Сотрудников: ${cnt}`;
    right.textContent = `Заполнено: месяц ${filledMonth} • день ${filledDay}`;
}

function renderPlansHint() {
    const el = document.getElementById("plansHint");
    if (!el) return;

    const parts = [];

    if (Array.isArray(selectedEmployeeIds) && selectedEmployeeIds.length > 0) {
        parts.push(`сотрудники ${selectedEmployeeIds.length}`);
    }

    if (Array.isArray(selectedGroupIds) && selectedGroupIds.length > 0) {
        parts.push(`группы ${selectedGroupIds.length}`);
    }

    el.textContent = parts.length > 0 ? `Фильтр: ${parts.join(" • ")}` : "—";
}

async function savePlanSettings() {
    try {
        const items = collectPlanInputs();
        const payload = { items };

        const resp = await sendJsonRequest("?handler=SavePlanSettings", "POST", buildJsonHeaders(antiForgeryToken), payload);
        unwrapOrThrow(resp, "Ошибка сохранения планов.");
        return true;
    } catch (e) {
        console.error(e);
        return false;
    }
}

function collectPlanInputs() {
    const map = new Map();

    document.querySelectorAll('#plansGrid input[data-employee-id][data-kind]').forEach(inp => {
        const employeeId = Number(inp.dataset.employeeId);
        if (!Number.isFinite(employeeId)) return;

        if (!map.has(employeeId)) {
            map.set(employeeId, { employeeId: employeeId, monthPlan: null, dayPlan: null });
        }

        const row = map.get(employeeId);
        const kind = inp.dataset.kind;
        const raw = (inp.value || "").trim();
        const val = raw === "" ? null : Number(raw);

        if (val !== null && (!Number.isInteger(val) || val < 0)) return;

        if (kind === "month") row.monthPlan = val;
        if (kind === "day") row.dayPlan = val;
    });

    return Array.from(map.values());
}

function initNameFilter() {
    const btn = document.getElementById("btnNameFilter");
    const modalEl = document.getElementById("nameFilterModal");
    if (!btn || !modalEl) return;

    const modal = new bootstrap.Modal(modalEl);

    btn.addEventListener("click", (e) => {
        e.stopPropagation();
        rebuildNameFilterList();
        const locked = hasActiveGroupFilter();
        setFilterModalLocked("name", locked);
        modal.show();
    });

    const search = document.getElementById("nameFilterSearch");
    if (search) {
        search.addEventListener("input", () => rebuildNameFilterList());
    }

    const selAll = document.getElementById("nameFilterSelectAll");
    if (selAll) {
        selAll.addEventListener("click", () => {
            selectedEmployeeIds = allRows.map(x => Number(x.employeeId));
            saveState();
            rebuildNameFilterList();
        });
    }

    const clrAll = document.getElementById("nameFilterClearAll");
    if (clrAll) {
        clrAll.addEventListener("click", () => {
            selectedEmployeeIds = null;
            saveState();
            rebuildNameFilterList();
        });
    }

    const apply = document.getElementById("nameFilterApply");
    if (apply) {
        apply.addEventListener("click", () => {
            applyFilterSortRender();
        });
    }
}

function rebuildNameFilterList() {
    const host = document.getElementById("nameFilterList");
    if (!host) return;

    host.textContent = "";

    const qEl = document.getElementById("nameFilterSearch");
    const q = (qEl ? qEl.value : "").trim().toLowerCase();

    const set = Array.isArray(selectedEmployeeIds) ? new Set(selectedEmployeeIds.map(Number)) : null;

    const rows = Array.isArray(allRows) ? [...allRows] : [];
    rows.sort((a, b) => String(a.fullName || "").localeCompare(String(b.fullName || ""), "ru"));

    const filtered = q ? rows.filter(x => String(x.fullName || "").toLowerCase().includes(q)) : rows;

    const hint = document.getElementById("nameFilterHint");
    if (hint) {
        hint.classList.add("d-none");
        hint.textContent = "";
        if (q && filtered.length === 0) {
            hint.textContent = "Ничего не найдено";
            hint.classList.remove("d-none");
        }
    }

    for (const r of filtered) {
        const wrap = document.createElement("div");
        wrap.className = "form-check";
        wrap.style.marginLeft = "12px";

        const input = document.createElement("input");
        input.type = "checkbox";
        input.className = "form-check-input";
        input.id = `empf_${r.employeeId}`;
        input.checked = set ? set.has(Number(r.employeeId)) : false;

        input.addEventListener("change", () => {
            const id = Number(r.employeeId);
            if (!Number.isFinite(id)) return;

            let arr = Array.isArray(selectedEmployeeIds) ? [...selectedEmployeeIds] : [];
            if (input.checked) {
                if (!arr.includes(id)) arr.push(id);
            } else {
                arr = arr.filter(x => x !== id);
            }

            selectedEmployeeIds = arr.length > 0 ? arr : null;
            saveState();
        });

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = input.id;
        label.textContent = r.fullName || "";

        wrap.append(input, label);
        host.appendChild(wrap);
    }
}

function loadSavedState() {
    const s1 = localStorage.getItem(psSortKey);
    if (s1) {
        try {
            const st = JSON.parse(s1);
            if (st && st.key && st.dir) sortState = st;
        } catch { }
    }

    const s2 = localStorage.getItem(psFilterKey);
    if (s2) {
        try {
            const st = JSON.parse(s2);
            if (Array.isArray(st)) selectedEmployeeIds = st;
        } catch { }
    }

    const s3 = localStorage.getItem(psGroupFilterKey);
    if (s3) {
        try {
            const st = JSON.parse(s3);
            if (Array.isArray(st)) selectedGroupIds = st;
        } catch { }
    }
}

function saveState() {
    localStorage.setItem(psSortKey, JSON.stringify(sortState));
    if (Array.isArray(selectedEmployeeIds) && selectedEmployeeIds.length > 0) {
        localStorage.setItem(psFilterKey, JSON.stringify(selectedEmployeeIds));
    } else {
        localStorage.removeItem(psFilterKey);
    }

    if (Array.isArray(selectedGroupIds) && selectedGroupIds.length > 0) {
        localStorage.setItem(psGroupFilterKey, JSON.stringify(selectedGroupIds));
    } else {
        localStorage.removeItem(psGroupFilterKey);
    }
}

function renderColorRules() {
    const tbody = document.getElementById("colorRulesRows");
    if (!tbody) return;

    tbody.textContent = "";

    if (!colorRules || colorRules.length === 0) {
        const tr = document.createElement("tr");
        const td = document.createElement("td");
        td.colSpan = 4;
        td.className = "text-center text-muted py-3";
        td.textContent = "Нет правил";
        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }

    for (let i = 0; i < colorRules.length; i++) {
        const r = colorRules[i];

        const tr = document.createElement("tr");

        const tdFrom = document.createElement("td");
        tdFrom.appendChild(buildIntInput("from", r.fromPercent, v => { r.fromPercent = v; }, i));
        tr.appendChild(tdFrom);

        const tdTo = document.createElement("td");
        tdTo.appendChild(buildIntInput("to", r.toPercent, v => { r.toPercent = v; }, i));
        tr.appendChild(tdTo);

        const tdColor = document.createElement("td");

        const colorWrap = document.createElement("div");
        colorWrap.className = "d-flex align-items-center";
        colorWrap.style.gap = "6px";

        const preview = document.createElement("div");
        preview.style.width = "18px";
        preview.style.height = "18px";
        preview.style.borderRadius = "4px";
        preview.style.border = "1px solid #ced4da";
        preview.style.flexShrink = "0";

        const inputColor = document.createElement("input");
        inputColor.type = "text";
        inputColor.className = "form-control form-control-sm";
        inputColor.style.maxWidth = "120px";
        inputColor.value = (r.color === null || r.color === undefined) ? "" : String(r.color);
        inputColor.id = `colorRule_${i}_color`;
        inputColor.name = `colorRule_${i}_color`;

        inputColor.addEventListener("input", () => {
            const sanitized = sanitizeHexColorInput(inputColor.value);
            inputColor.value = sanitized;

            r.color = sanitized === "" ? null : sanitized;

            const invalid = sanitized !== "" && !isValidHexColor(sanitized);
            setColorInvalid(inputColor, invalid);
            updatePreview(preview, sanitized);
        });

        const btnPick = document.createElement("button");
        btnPick.type = "button";
        btnPick.className = "btn btn-outline-secondary btn-sm";
        btnPick.textContent = "🎨";
        btnPick.addEventListener("click", () => {
            openSharedColorPicker(btnPick, inputColor.value, (v) => {
                inputColor.value = v;
                r.color = v;
                setColorInvalid(inputColor, false);
                updatePreview(preview, v);
            });
        });

        colorWrap.append(preview, inputColor, btnPick);
        updatePreview(preview, inputColor.value);
        tdColor.appendChild(colorWrap);
        tr.appendChild(tdColor);

        const tdDel = document.createElement("td");
        tdDel.className = "text-end";
        const btnDel = document.createElement("button");
        btnDel.type = "button";
        btnDel.className = "btn btn-outline-danger btn-sm";
        btnDel.textContent = "×";
        btnDel.addEventListener("click", () => {
            colorRules.splice(i, 1);
            renderColorRules();
        });
        tdDel.appendChild(btnDel);
        tr.appendChild(tdDel);

        tbody.appendChild(tr);
    }
}

function updatePreview(previewEl, val) {
    if (!previewEl) return;

    if (isValidHexColor(val)) {
        previewEl.style.backgroundColor = val;
    } else {
        previewEl.style.backgroundColor = "transparent";
    }
}

function buildIntInput(kind, value, onChange, rowIndex) {
    const input = document.createElement("input");
    input.type = "text";
    input.inputMode = "numeric";
    input.className = "form-control form-control-sm text-center";
    input.style.maxWidth = "120px";
    input.id = `colorRule_${rowIndex}_${kind}`;
    input.name = `colorRule_${rowIndex}_${kind}`;
    input.value = (value === null || value === undefined) ? "" : String(value);

    input.addEventListener("input", () => {
        const cleaned = (input.value || "").replace(/[^\d]/g, "");
        input.value = cleaned;
        const v = cleaned === "" ? 0 : Number(cleaned);
        if (Number.isInteger(v) && v >= 0) onChange(v);
    });

    return input;
}

async function saveColorRules() {
    try {
        const payload = { items: buildColorRulesPayload() };
        const resp = await sendJsonRequest("?handler=SavePlanColorRules", "POST", buildJsonHeaders(antiForgeryToken), payload);
        unwrapOrThrow(resp, "Ошибка сохранения цветовой схемы.");
        return true;
    } catch (e) {
        console.error(e);
        return false;
    }
}

function buildColorRulesPayload() {
    return colorRules.map((x, idx) => ({
        id: (x.id === null || x.id === undefined || x.id === "") ? null : String(x.id),
        fromPercent: Number(x.fromPercent || 0),
        toPercent: Number(x.toPercent || 0),
        color: (x.color === null || x.color === undefined || String(x.color).trim() === "") ? null : String(x.color).trim()
    }));
}

function showPageError(msg) {
    const el = document.getElementById("pageError");
    if (!el) return;
    el.textContent = msg || "Ошибка.";
    el.classList.remove("d-none");
}

function clearPageError() {
    const el = document.getElementById("pageError");
    if (!el) return;
    el.textContent = "";
    el.classList.add("d-none");
}

function isValidHexColor(v) {
    return /^#([0-9a-fA-F]{6})$/.test(v);
}

function sanitizeHexColorInput(raw) {
    let v = String(raw || "").trim();

    if (v === "") return "";

    if (v[0] !== "#") v = "#" + v;

    let out = "#";
    for (let i = 1; i < v.length && out.length < 7; i++) {
        const ch = v[i];
        if (/[0-9a-fA-F]/.test(ch)) out += ch.toUpperCase();
    }

    return out;
}

function setColorInvalid(input, isInvalid) {
    if (!input) return;
    if (isInvalid) input.classList.add("is-invalid");
    else input.classList.remove("is-invalid");
}

function validateColorRulesBeforeSave() {
    clearPageError();

    let ok = true;

    for (let i = 0; i < colorRules.length; i++) {
        const r = colorRules[i];

        const fromEl = document.getElementById(`colorRule_${i}_from`);
        const toEl = document.getElementById(`colorRule_${i}_to`);
        const colorEl = document.getElementById(`colorRule_${i}_color`);

        const from = Number(r.fromPercent);
        const to = Number(r.toPercent);        

        const badFrom = !Number.isInteger(from) || from < 0 || from > maxPercent;
        const badTo = !Number.isInteger(to) || to < 0 || to > maxPercent;

        setInvalid(fromEl, badFrom);
        setInvalid(toEl, badTo);

        if (badFrom || badTo) ok = false;

        if (!badFrom && !badTo) {
            const badRange = to <= from;
            setInvalid(fromEl, badRange);
            setInvalid(toEl, badRange);
            if (badRange) ok = false;
        }

        const colorRaw = colorEl ? (colorEl.value || "").trim() : "";
        const badColor = (colorRaw !== "" && !isValidHexColor(colorRaw));
        setColorInvalid(colorEl, badColor);
        if (badColor) ok = false;

        const missingColor = colorRaw === "";
        setColorInvalid(colorEl, missingColor || badColor);
        if (missingColor) ok = false;
    }

    const ranges = [];

    for (let i = 0; i < colorRules.length; i++) {
        const r = colorRules[i];
        const from = Number(r.fromPercent);
        const to = Number(r.toPercent);

        const fromEl = document.getElementById(`colorRule_${i}_from`);
        const toEl = document.getElementById(`colorRule_${i}_to`);

        const baseOk =
            Number.isInteger(from) && Number.isInteger(to) &&
            from >= 0 && from <= maxPercent &&
            to >= 0 && to <= maxPercent &&
            to > from;

        if (baseOk) ranges.push({ i, from, to });
    }

    ranges.sort((a, b) => (a.from === b.from) ? (a.to - b.to) : (a.from - b.from));

    for (let k = 0; k < ranges.length - 1; k++) {
        const a = ranges[k];
        const b = ranges[k + 1];

        if (b.from <= a.to) {
            const aFromEl = document.getElementById(`colorRule_${a.i}_from`);
            const aToEl = document.getElementById(`colorRule_${a.i}_to`);
            const bFromEl = document.getElementById(`colorRule_${b.i}_from`);
            const bToEl = document.getElementById(`colorRule_${b.i}_to`);

            setInvalid(aFromEl, true);
            setInvalid(aToEl, true);
            setInvalid(bFromEl, true);
            setInvalid(bToEl, true);

            ok = false;
        }
    }

    if (!ok) {
        showPageError("Цветовая схема: проверьте проценты (0–999), диапазон (До % должен быть больше От %) и цвет (#RRGGBB). Диапазоны не должны пересекаться.");
    }

    return ok;
}

function formatGroups(groups) {
    if (!Array.isArray(groups) || groups.length === 0) return "";
    return groups
        .map(g => (g && g.name) ? String(g.name).trim() : "")
        .filter(x => x.length > 0)
        .join(", ");
}

function getAllGroups() {
    const map = new Map();

    for (const r of (allRows || [])) {
        if (!Array.isArray(r.groups)) continue;

        for (const g of r.groups) {
            const id = Number(g?.id);
            if (!Number.isFinite(id)) continue;

            const name = (g?.name ? String(g.name).trim() : "");
            if (!map.has(id)) map.set(id, { id, name });
            else if (name && !map.get(id).name) map.get(id).name = name;
        }
    }

    return Array.from(map.values())
        .sort((a, b) => String(a.name || "").localeCompare(String(b.name || ""), "ru"));
}

function getAllGroupIds() {
    return getAllGroups().map(x => x.id);
}

function rebuildGroupFilterList() {
    const host = document.getElementById("groupFilterList");
    if (!host) return;

    host.textContent = "";

    const qEl = document.getElementById("groupFilterSearch");
    const q = (qEl ? qEl.value : "").trim().toLowerCase();

    const set = Array.isArray(selectedGroupIds) ? new Set(selectedGroupIds.map(Number)) : null;

    const groups = getAllGroups();
    const filtered = q
        ? groups.filter(x => String(x.name || "").toLowerCase().includes(q))
        : groups;

    const hint = document.getElementById("groupFilterHint");
    if (hint) {
        hint.classList.add("d-none");
        hint.textContent = "";
        if (q && filtered.length === 0) {
            hint.textContent = "Ничего не найдено";
            hint.classList.remove("d-none");
        }
    }

    for (const g of filtered) {
        const wrap = document.createElement("div");
        wrap.className = "form-check";
        wrap.style.marginLeft = "12px";

        const input = document.createElement("input");
        input.type = "checkbox";
        input.className = "form-check-input";
        input.id = `grpf_${g.id}`;
        input.name = input.id;
        input.checked = set ? set.has(g.id) : false;

        input.addEventListener("change", () => {
            const id = Number(g.id);

            let arr = Array.isArray(selectedGroupIds) ? [...selectedGroupIds] : [];
            if (input.checked) {
                if (!arr.includes(id)) arr.push(id);
            } else {
                arr = arr.filter(x => x !== id);
            }

            selectedGroupIds = arr.length > 0 ? arr : null;
            saveState();
        });

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = input.id;
        label.textContent = g.name || `(id:${g.id})`;

        wrap.append(input, label);
        host.appendChild(wrap);
    }
}

function setInvalid(input, isInvalid) {
    if (!input) return;
    if (isInvalid) input.classList.add("is-invalid");
    else input.classList.remove("is-invalid");
}

function sortColorRules() {
    if (!Array.isArray(colorRules)) return;

    colorRules.sort((a, b) => {
        const af = Number(a?.fromPercent ?? 0);
        const bf = Number(b?.fromPercent ?? 0);
        if (af !== bf) return af - bf;

        const at = Number(a?.toPercent ?? 0);
        const bt = Number(b?.toPercent ?? 0);
        return at - bt;
    });
}

function applyCollapseState() {
    const colorsBody = document.getElementById("colorRulesBody");
    const plansBody = document.getElementById("plansBody");

    const btnColors = document.getElementById("btnToggleColorRules");
    const btnPlans = document.getElementById("btnTogglePlans");

    if (colorsBody) colorsBody.classList.toggle("d-none", isColorRulesCollapsed);
    if (plansBody) plansBody.classList.toggle("d-none", isPlansCollapsed);

    if (btnColors) btnColors.textContent = isColorRulesCollapsed ? "+" : "—";
    if (btnPlans) btnPlans.textContent = isPlansCollapsed ? "+" : "—";
}

function hasActiveNameFilter() {
    return Array.isArray(selectedEmployeeIds) && selectedEmployeeIds.length > 0;
}

function hasActiveGroupFilter() {
    return Array.isArray(selectedGroupIds) && selectedGroupIds.length > 0;
}

function setFilterModalLocked(kind, locked) {
    if (kind === "name") {
        const hint = document.getElementById("nameFilterHint");
        const search = document.getElementById("nameFilterSearch");
        const selAll = document.getElementById("nameFilterSelectAll");
        const clrAll = document.getElementById("nameFilterClearAll");
        const host = document.getElementById("nameFilterList");

        if (hint) {
            if (locked) {
                hint.textContent = "Сейчас используется фильтр по группам. Чтобы использовать фильтр по ФИО, сначала очистите фильтр по группам.";
                hint.classList.remove("d-none");
            } else {
                hint.textContent = "";
                hint.classList.add("d-none");
            }
        }

        if (search) search.disabled = locked;
        if (selAll) selAll.disabled = locked;
        if (clrAll) clrAll.disabled = locked;

        if (host) {
            host.querySelectorAll("input[type='checkbox']").forEach(x => x.disabled = locked);
        }
    }

    if (kind === "group") {
        const hint = document.getElementById("groupFilterHint");
        const search = document.getElementById("groupFilterSearch");
        const selAll = document.getElementById("groupFilterSelectAll");
        const clrAll = document.getElementById("groupFilterClearAll");
        const host = document.getElementById("groupFilterList");

        if (hint) {
            if (locked) {
                hint.textContent = "Сейчас используется фильтр по ФИО. Чтобы использовать фильтр по группам, сначала очистите фильтр по ФИО.";
                hint.classList.remove("d-none");
            } else {
                hint.textContent = "";
                hint.classList.add("d-none");
            }
        }

        if (search) search.disabled = locked;
        if (selAll) selAll.disabled = locked;
        if (clrAll) clrAll.disabled = locked;

        if (host) {
            host.querySelectorAll("input[type='checkbox']").forEach(x => x.disabled = locked);
        }
    }
}

function initPlanTextColorUi() {
    const input = document.getElementById("planTextColorInput");
    const preview = document.getElementById("planTextColorPreview");
    const btnPick = document.getElementById("planTextColorPick");
    if (!input || !preview || !btnPick) return;

    const saved = String(localStorage.getItem(reportPlanTextColorKey) || "").trim();
    const start = isValidHexColor(saved) ? saved.toUpperCase() : "#FFC107";

    input.value = start;
    updatePreview(preview, input.value);

    input.addEventListener("input", () => {
        const sanitized = sanitizeHexColorInput(input.value);
        input.value = sanitized;

        const ok = isValidHexColor(sanitized);
        setColorInvalid(input, !ok);

        if (ok) updatePreview(preview, sanitized.toUpperCase());
        else updatePreview(preview, "");
    });

    btnPick.addEventListener("click", () => {
        openSharedColorPicker(btnPick, input.value, (v) => {
            input.value = v;
            setColorInvalid(input, false);
            updatePreview(preview, v);
        });
    });
}

function savePlanTextColorToStorage() {
    const input = document.getElementById("planTextColorInput");
    if (!input) return true;

    const v = String(input.value || "").trim().toUpperCase();
    if (!isValidHexColor(v)) {
        setColorInvalid(input, true);
        showPageError("Цвет колонки «План»: укажите #RRGGBB.");
        return false;
    }

    setColorInvalid(input, false);
    localStorage.setItem(reportPlanTextColorKey, v);
    return true;
}

function ensureSharedColorPicker() {
    if (sharedColorPicker) return sharedColorPicker;

    const p = document.createElement("input");
    p.type = "color";
    p.style.position = "fixed";
    p.style.left = "0px";
    p.style.top = "0px";
    p.style.width = "32px";
    p.style.height = "32px";
    p.style.opacity = "0.01";
    p.style.pointerEvents = "none";
    p.style.zIndex = "999999";
    p.style.border = "0";
    p.style.padding = "0";

    p.addEventListener("input", () => {
        const v = String(p.value || "").toUpperCase();
        if (sharedColorPickerOnPick) sharedColorPickerOnPick(v);
    });

    document.body.appendChild(p);
    sharedColorPicker = p;
    return p;
}

function openSharedColorPicker(btnEl, initialHex, onPick) {
    const p = ensureSharedColorPicker();

    sharedColorPickerOnPick = onPick;

    p.value = isValidHexColor(initialHex) ? initialHex.toUpperCase() : "#000000";

    const rect = btnEl.getBoundingClientRect();

    p.style.left = rect.left + "px";
    p.style.top = rect.bottom + "px";
    p.style.pointerEvents = "auto";

    requestAnimationFrame(() => {
        p.click();
        setTimeout(() => {
            p.style.pointerEvents = "none";
        }, 300);
    });
}