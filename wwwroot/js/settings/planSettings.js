const psSortKey = "crm_plan_settings_sort_v1";
const psFilterKey = "crm_plan_settings_filter_v1";
const psGroupFilterKey = "crm_plan_settings_group_filter_v1";
let selectedGroupIds = null;
let antiForgeryToken = null;

let allRows = [];
let visibleRows = [];

let sortState = { key: "name", dir: "asc" };
let selectedEmployeeIds = null;

let isReloading = false;
let isSaving = false;

let colorRules = [];

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
    await reloadEmployees();
    await reloadColorRules();
}

function initButtons() {
    const btnReload = document.getElementById("btnReloadEmployees");
    const btnSave = document.getElementById("btnSavePlans");
    const btnAddRule = document.getElementById("btnAddColorRule");

    if (btnReload) {
        btnReload.addEventListener("click", async () => {
            if (isReloading || isSaving) return;
            isReloading = true;
            btnReload.disabled = true;

            await reloadEmployees();

            btnReload.disabled = false;
            isReloading = false;
        });
    }

    if (btnSave) {
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

            const okPlans = await savePlanSettings();
            const okColors = await saveColorRules();

            if (okPlans && okColors) {
                await reloadEmployees();
                await reloadColorRules();
            }

            btnSave.disabled = false;
            isSaving = false;
        });
    }

    if (btnAddRule) {
        btnAddRule.addEventListener("click", () => {
            colorRules.push({
                id: null,
                fromPercent: 0,
                toPercent: 0,
                color: null
            });
            renderColorRules();
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
        const inputColor = document.createElement("input");
        inputColor.type = "text";
        inputColor.className = "form-control form-control-sm";
        inputColor.value = (r.color === null || r.color === undefined) ? "" : String(r.color);
        inputColor.id = `colorRule_${i}_color`;
        inputColor.name = `colorRule_${i}_color`;

        inputColor.addEventListener("input", () => {
            const v = (inputColor.value || "").trim();
            r.color = v === "" ? null : v;
            setColorInvalid(inputColor, !isValidHexColor(v) && v !== "");
        });

        tdColor.appendChild(inputColor);
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

        const badFrom = !Number.isInteger(from) || from < 0 || from > 100;
        const badTo = !Number.isInteger(to) || to < 0 || to > 100;

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

    if (!ok) {
        showPageError("Цветовая схема: проверь проценты (0–100) и диапазон (До % должен быть больше От %). Цвет — #RRGGBB или пусто для дефолта.");
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
