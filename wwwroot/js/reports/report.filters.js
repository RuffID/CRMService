const expandedKey = "crm_report_filters_expanded_v1";
const storageKey = "crm_report_filters_v1";

const reportEoModeKey = "crm_report_eo_mode_v1";
const reportPlanModeKey = "crm_report_plan_mode_v1";
const reportSelectedPlanKey = "crm_report_selected_plan_v1";

const reportAutoReloadMs = 5 * 60 * 1000;

let allEmployees = [];
let reportAutoReloadTimerId = null;
let reportPlanSwitchTimerId = null;
let reportPlanSwitchDelayMs = 10 * 1000;
let reportPlans = [];

let isAutoReloadInProgress = false;

document.addEventListener("DOMContentLoaded", async () => {
    await initReportFiltersState();
});

document.addEventListener("change", (e) => {
    const t = e.target;
    if (!t) return;

    const isFilter =
        t.classList?.contains("filter-groups")
        || t.classList?.contains("filter-priorities")
        || t.classList?.contains("filter-statuses")
        || t.classList?.contains("filter-types")
        || t.classList?.contains("filter-employees");

    if (!isFilter) return;

    if (t.classList.contains("filter-groups")) applyEmployeeVisibilityByGroups();
    if (t.classList.contains("filter-employees")) applyGroupsVisibilityByEmployees();

    saveFilters(storageKey);
    applyMutualExclusionUI();
    updateBadges();
});

document.addEventListener("crm-report-rendered", () => {
    if (!isPlanModeOn()) return;
    scheduleNextPlanSwitch();
});

async function initReportFiltersState() {
    const panel = document.getElementById("filtersPanel");
    if (!panel || !window.bootstrap) return;

    await loadAndRenderDictionaries();
    await loadPlanSettingsAndRenderSelect();

    ensureDefaultMonthDates(storageKey);
    restoreFilters(storageKey);

    loadModeFlags();
    applyPeriodModesUi();
    enforceModeDates();

    applyEmployeeVisibilityByGroups();
    applyGroupsVisibilityByEmployees();
    applyMutualExclusionUI();
    syncTypesTreeFolders();
    updateBadges();

    wireExpandedPersistence(expandedKey, panel);
    wireFiltersPersistence(storageKey);
    wirePeriodModes();

    await restartAutoReload(true);
}

function wireExpandedPersistence(expandedKey, panel) {
    panel.addEventListener("shown.bs.collapse", () => localStorage.setItem(expandedKey, "1"));
    panel.addEventListener("hidden.bs.collapse", () => localStorage.setItem(expandedKey, "0"));
}

async function loadPlanSettingsAndRenderSelect() {
    try {
        const [plansResponse, settingsResponse] = await Promise.all([
            sendJsonRequest("?handler=Plans", "GET"),
            sendJsonRequest("?handler=GeneralSettings", "GET")
        ]);

        const plansData = unwrapOrThrow(plansResponse, "Не удалось загрузить планы.");
        const settingsData = unwrapOrThrow(settingsResponse, "Не удалось загрузить общие настройки.");

        reportPlans = Array.isArray(plansData) ? plansData : [];

        const switchSeconds = Number(settingsData?.planSwitchSeconds ?? 10);
        reportPlanSwitchDelayMs = Number.isFinite(switchSeconds) && switchSeconds >= 10
            ? switchSeconds * 1000
            : 10 * 1000;
    } catch (error) {
        console.error(error);
        reportPlans = [];
        reportPlanSwitchDelayMs = 10 * 1000;
    }

    renderPlanSelect();
}

function wireFiltersPersistence(storageKey) {
    getEl("hideWithoutCurrent")?.addEventListener("change", () => saveFilters(storageKey));
    getEl("hideWithoutSolved")?.addEventListener("change", () => saveFilters(storageKey));
    getEl("hideWithoutTime")?.addEventListener("change", () => saveFilters(storageKey));

    getEl("dateFrom")?.addEventListener("change", () => saveFilters(storageKey));
    getEl("dateTo")?.addEventListener("change", () => saveFilters(storageKey));

    getEl("applyFilters")?.addEventListener("click", async () => {
        enforceModeDates();
        saveFilters(storageKey);
        if (typeof window.loadPerformanceReport === "function") await window.loadPerformanceReport();
    });

    getEl("resetFilters")?.addEventListener("click", async () => {
        resetFilters();
        saveFilters(storageKey);

        if (typeof window.resetReportSorting === "function") window.resetReportSorting();
        if (typeof window.loadPerformanceReport === "function") await window.loadPerformanceReport();
    });

    getEl("typesSelectAll")?.addEventListener("click", () => {
        const root = getEl("listTypes");
        if (!root) return;

        root.querySelectorAll("input.filter-types-folder, input.filter-types").forEach(x => {
            x.checked = true;
            if (x.classList.contains("filter-types-folder")) x.indeterminate = false;
        });

        saveFilters(storageKey);
        updateBadges();
    });

    getEl("typesClearAll")?.addEventListener("click", () => {
        const root = getEl("listTypes");
        if (!root) return;

        root.querySelectorAll("input.filter-types-folder, input.filter-types").forEach(x => {
            x.checked = false;
            if (x.classList.contains("filter-types-folder")) x.indeterminate = false;
        });

        saveFilters(storageKey);
        updateBadges();
    });

    initSearch("searchGroups", "listGroups");
    initSearch("searchEmployees", "listEmployees");
    initSearch("searchPriorities", "listPriorities");
    initSearch("searchStatuses", "listStatuses");

    wireSelectButtons("employeesSelectAll", "employeesClearAll", "listEmployees", "input.filter-employees");
    wireSelectButtons("groupsSelectAll", "groupsClearAll", "listGroups", "input.filter-groups");
    wireSelectButtons("prioritiesSelectAll", "prioritiesClearAll", "listPriorities", "input.filter-priorities");
    wireSelectButtons("statusesSelectAll", "statusesClearAll", "listStatuses", "input.filter-statuses");
    wireSelectButtons("typesSelectAll", "typesClearAll", "listTypes", "input.filter-types");

    getEl("reportPlanSelect")?.addEventListener("change", async (event) => {
        const select = event.target;
        localStorage.setItem(reportSelectedPlanKey, select?.value || "");

        saveFilters(storageKey);

        if (typeof window.loadPerformanceReport === "function") {
            await window.loadPerformanceReport();
        }
    });

}

function renderPlanSelect() {
    const select = getEl("reportPlanSelect");
    if (!select) return;

    const hasSaved = localStorage.getItem(reportSelectedPlanKey) !== null;
    const savedRaw = localStorage.getItem(reportSelectedPlanKey); // может быть ""
    const savedPlanId = normalizePlanId(savedRaw); // null для ""

    select.textContent = "";

    const emptyOption = document.createElement("option");
    emptyOption.value = "";
    emptyOption.textContent = "Без плана";
    select.appendChild(emptyOption);

    for (const plan of reportPlans) {
        const planId = normalizePlanId(plan?.id ?? plan?.Id);
        if (!planId) continue;

        const option = document.createElement("option");
        option.value = planId;
        option.textContent = String(plan?.name ?? plan?.Name ?? "").trim();
        select.appendChild(option);
    }

    const planIds = reportPlans
        .map((plan) => normalizePlanId(plan?.id ?? plan?.Id))
        .filter((id) => !!id);

    if (hasSaved) {
        if (savedRaw === "") {
            select.value = ""; // явно сохраняем "Без плана"
        } else if (savedPlanId && planIds.includes(savedPlanId)) {
            select.value = savedPlanId;
        } else {
            select.value = "";
        }
    } else {
        select.value = planIds.length > 0 ? planIds[0] : "";
    }

    localStorage.setItem(reportSelectedPlanKey, select.value || "");
}

function wirePeriodModes() {
    const eo = getEl("eoMode");
    const pm = getEl("planMode");
    if (!eo || !pm) return;

    eo.addEventListener("change", async () => {
        if (eo.checked) pm.checked = false;

        saveModeFlags();
        applyPeriodModesUi();
        enforceModeDates();

        await restartAutoReload(true);

        saveFilters(storageKey);

        if (typeof window.applySortAndRender === "function")
            window.applySortAndRender();
    });

    pm.addEventListener("change", async () => {
        if (pm.checked) eo.checked = false;

        saveModeFlags();
        applyPeriodModesUi();

        await restartAutoReload(true);

        saveFilters(storageKey);

        if (typeof window.applySortAndRender === "function") window.applySortAndRender();
    });
}

function stopPlanSwitchTimer() {
    if (reportPlanSwitchTimerId) {
        clearTimeout(reportPlanSwitchTimerId);
        reportPlanSwitchTimerId = null;
    }
}

function scheduleNextPlanSwitch() {
    stopPlanSwitchTimer();

    if (!isPlanModeOn()) return;
    if (reportPlans.length <= 1) return;

    reportPlanSwitchTimerId = setTimeout(() => {
        if (!isPlanModeOn()) return;
        if (document.hidden) {
            scheduleNextPlanSwitch();
            return;
        }

        const select = getEl("reportPlanSelect");
        if (!select) return;

        const planIds = reportPlans
            .map((plan) => normalizePlanId(plan?.id ?? plan?.Id))
            .filter((id) => !!id);

        if (planIds.length <= 1) return;

        const current = normalizePlanId(select.value);
        let currentIndex = planIds.indexOf(current);
        if (currentIndex < 0) currentIndex = 0;

        const nextIndex = (currentIndex + 1) % planIds.length;
        select.value = planIds[nextIndex];
        localStorage.setItem(reportSelectedPlanKey, select.value || "");
        saveFilters(storageKey);

        if (typeof window.loadPerformanceReport === "function") {
            window.loadPerformanceReport().finally(() => {
                scheduleNextPlanSwitch();
            });
            return;
        }

        scheduleNextPlanSwitch();
    }, reportPlanSwitchDelayMs);
}

async function restartAutoReload(forceNow = false) {
    if (reportAutoReloadTimerId) {
        clearInterval(reportAutoReloadTimerId);
        reportAutoReloadTimerId = null;
    }

    stopPlanSwitchTimer();

    reportAutoReloadTimerId = setInterval(async () => {
        if (document.hidden) return;
        if (isAutoReloadInProgress) return;
        if (typeof window.loadPerformanceReport !== "function") return;

        isAutoReloadInProgress = true;
        try {
            await window.loadPerformanceReport();
        } finally {
            isAutoReloadInProgress = false;
        }
    }, reportAutoReloadMs);

    if (forceNow && typeof window.loadPerformanceReport === "function") {
        await window.loadPerformanceReport();
        return;
    }
}

function applyPeriodModesUi() {
    const eo = getEl("eoMode");
    const pm = getEl("planMode");
    const planSelect = getEl("reportPlanSelect");
    if (!eo || !pm) return;

    if (eo.checked) {
        pm.checked = false;
        pm.disabled = true;
        eo.disabled = false;
    } else if (pm.checked) {
        eo.checked = false;
        eo.disabled = true;
        pm.disabled = false;
    } else {
        eo.disabled = false;
        pm.disabled = false;
    }

    if (planSelect) {
        planSelect.disabled = pm.checked === true;
    }

    syncDateInputsLock();
}

function syncDateInputsLock() {
    const df = getEl("dateFrom");
    const dt = getEl("dateTo");
    if (!df || !dt) return;

    const locked = isEoModeOn();

    df.disabled = locked;
    dt.disabled = locked;
}

function enforceModeDates() {
    if (isEoModeOn()) {
        setTodayRange();
    }
}

function setTodayRange() {
    const today = toDateInputValue(new Date());
    setValue("dateFrom", today);
    setValue("dateTo", today);
}

function setCurrentMonthRange() {
    const now = new Date();
    const first = new Date(now.getFullYear(), now.getMonth(), 1);
    const last = new Date(now.getFullYear(), now.getMonth() + 1, 0);

    setValue("dateFrom", toDateInputValue(first));
    setValue("dateTo", toDateInputValue(last));
}

function saveModeFlags() {
    const eo = getEl("eoMode");
    const pm = getEl("planMode");
    if (!eo || !pm) return;

    localStorage.setItem(reportEoModeKey, eo.checked ? "1" : "0");
    localStorage.setItem(reportPlanModeKey, pm.checked ? "1" : "0");
}

function loadModeFlags() {
    const eo = getEl("eoMode");
    const pm = getEl("planMode");
    if (!eo || !pm) return;

    eo.checked = localStorage.getItem(reportEoModeKey) === "1";
    pm.checked = localStorage.getItem(reportPlanModeKey) === "1";

    if (eo.checked && pm.checked) {
        pm.checked = false;
        localStorage.setItem(reportPlanModeKey, "0");
    }
}

function isEoModeOn() {
    const el = getEl("eoMode");
    return !!el && el.checked === true;
}

function isPlanModeOn() {
    const el = getEl("planMode");
    return !!el && el.checked === true;
}

function wireSelectButtons(selectAllId, clearAllId, listId, checkboxSelector) {
    const btnAll = getEl(selectAllId);
    const btnNone = getEl(clearAllId);
    const list = getEl(listId);

    if (btnAll) {
        btnAll.addEventListener("click", () => {
            if (!list) return;

            list.querySelectorAll(checkboxSelector).forEach(cb => {
                const wrap = cb.closest(".form-check");
                if (wrap && wrap.classList.contains("d-none")) return;
                cb.checked = true;
            });

            saveFilters(storageKey);
            updateBadges();
            applyMutualExclusionUI();
        });
    }

    if (btnNone) {
        btnNone.addEventListener("click", () => {
            if (!list) return;

            list.querySelectorAll(checkboxSelector).forEach(cb => {
                const wrap = cb.closest(".form-check");
                if (wrap && wrap.classList.contains("d-none")) return;
                cb.checked = false;
            });

            saveFilters(storageKey);
            updateBadges();
            applyEmployeeVisibilityByGroups();
            applyMutualExclusionUI();
        });
    }
}

function saveFilters(storageKey) {
    localStorage.setItem(storageKey, JSON.stringify(readState()));
}

function restoreFilters(storageKey) {
    const raw = localStorage.getItem(storageKey);
    if (!raw) return;

    let state;
    try { state = JSON.parse(raw); } catch { return; }
    if (!state) return;

    setChecked(".filter-groups", state.groups);
    setChecked(".filter-priorities", state.priorities);
    setChecked(".filter-statuses", state.statuses);
    setChecked(".filter-types", state.types);
    setChecked(".filter-employees", state.employees);

    setBool("hideWithoutCurrent", state.hideWithoutCurrent);
    setBool("hideWithoutSolved", state.hideWithoutSolved);
    setBool("hideWithoutTime", state.hideWithoutTime);

    setValue("dateFrom", state.dateFrom);
    setValue("dateTo", state.dateTo);

    const planSelect = getEl("reportPlanSelect");
    if (planSelect && Object.prototype.hasOwnProperty.call(state, "selectedPlanId")) {
        const savedPlanId = normalizePlanId(state.selectedPlanId);

        if (!savedPlanId) {
            planSelect.value = "";
            localStorage.setItem(reportSelectedPlanKey, "");
        } else {
            const exists = Array.from(planSelect.options).some(
                (opt) => normalizePlanId(opt.value) === savedPlanId
            );

            planSelect.value = exists ? savedPlanId : "";
            localStorage.setItem(reportSelectedPlanKey, planSelect.value || "");
        }
    }
}

function resetFilters() {
    setChecked(".filter-groups", []);
    setChecked(".filter-priorities", []);
    setChecked(".filter-statuses", []);
    setChecked(".filter-types", []);
    setChecked(".filter-employees", []);

    setBool("hideWithoutCurrent", false);
    setBool("hideWithoutSolved", false);
    setBool("hideWithoutTime", false);

    const { dateFrom, dateTo } = getCurrentMonthRangeForInputs();
    setValue("dateFrom", dateFrom);
    setValue("dateTo", dateTo);

    const eo = getEl("eoMode");
    const pm = getEl("planMode");
    if (eo) eo.checked = false;
    if (pm) pm.checked = false;

    const planSelect = getEl("reportPlanSelect");
    if (planSelect) {
        const firstOption = Array.from(planSelect.options).find((opt) => normalizePlanId(opt.value));
        planSelect.value = firstOption ? firstOption.value : "";
        localStorage.setItem(reportSelectedPlanKey, planSelect.value || "");
    }

    saveModeFlags();
    applyPeriodModesUi();

    applyEmployeeVisibilityByGroups();
    applyGroupsVisibilityByEmployees();
    applyMutualExclusionUI();
    updateBadges();
}

function readState() {
    return {
        groups: getChecked(".filter-groups"),
        employees: getChecked(".filter-employees"),
        priorities: getChecked(".filter-priorities"),
        statuses: getChecked(".filter-statuses"),
        types: getChecked(".filter-types"),
        hideWithoutCurrent: getBool("hideWithoutCurrent"),
        hideWithoutSolved: getBool("hideWithoutSolved"),
        hideWithoutTime: getBool("hideWithoutTime"),
        eoMode: getBool("eoMode"),
        planMode: getBool("planMode"),
        selectedPlanId: getValue("reportPlanSelect") ?? "",
        dateFrom: getValue("dateFrom"),
        dateTo: getValue("dateTo")
    };
}

window.readState = readState;
window.getSelectedReportPlanId = () => normalizePlanId(getValue("reportPlanSelect"));
window.hasReportPlans = () => reportPlans.length > 0;
window.getSelectedReportPlanMeta = () => {
    const selectedId = normalizePlanId(getValue("reportPlanSelect"));
    if (!selectedId) return null;

    for (const plan of reportPlans) {
        const id = normalizePlanId(plan?.id ?? plan?.Id);
        if (id !== selectedId) continue;

        const name = String(plan?.name ?? plan?.Name ?? "").trim();
        const colorRaw = String(plan?.planColor ?? plan?.PlanColor ?? "").trim().toUpperCase();
        const color = /^#([0-9A-F]{6})$/.test(colorRaw) ? colorRaw : null;

        return { id, name, color };
    }

    return null;
};

function getChecked(selector) {
    return Array.from(document.querySelectorAll(selector))
        .filter(x => x.checked)
        .map(x => x.value);
}

function setChecked(selector, values) {
    const set = new Set(values || []);
    document.querySelectorAll(selector).forEach(x => { x.checked = set.has(x.value); });
}

function updateBadges() {
    setBadge("badgeGroups", countChecked(".filter-groups"));
    setBadge("badgeEmployees", countChecked(".filter-employees"));
    setBadge("badgePriorities", countChecked(".filter-priorities"));
    setBadge("badgeStatuses", countChecked(".filter-statuses"));
    setBadge("badgeTypes", countChecked(".filter-types"));
}

function countChecked(selector) {
    return Array.from(document.querySelectorAll(selector)).filter(x => x.checked).length;
}

function setBadge(id, n) {
    const el = getEl(id);
    if (!el) return;
    el.textContent = String(n || 0);
}

function initSearch(searchId, listId) {
    const input = getEl(searchId);
    const list = getEl(listId);
    if (!input || !list) return;

    input.addEventListener("input", () => {
        const q = (input.value || "").trim().toLowerCase();
        list.querySelectorAll(".form-check").forEach(item => {
            const label = item.querySelector("label");
            const text = (label?.textContent || "").toLowerCase();
            item.classList.toggle("d-none-by-search", q.length > 0 && !text.includes(q));
            item.classList.toggle(
                "d-none",
                item.classList.contains("d-none-by-search")
                || item.classList.contains("d-none-by-groups")
                || item.classList.contains("d-none-by-employees")
            );
        });
    });
}

function getEl(id) {
    return document.getElementById(id);
}

function normalizePlanId(value) {
    const text = String(value || "").trim();
    return text.length > 0 ? text : null;
}

function getBool(id) {
    const el = getEl(id);
    return !!el?.checked;
}

function setBool(id, val) {
    const el = getEl(id);
    if (el) el.checked = !!val;
}

function getValue(id) {
    const el = getEl(id);
    return el?.value || "";
}

function setValue(id, val) {
    const el = getEl(id);
    if (el) el.value = val || "";
}

function ensureDefaultMonthDates(storageKey) {
    const raw = localStorage.getItem(storageKey);
    if (raw) {
        try {
            const st = JSON.parse(raw);
            if (st && st.dateFrom && st.dateTo) return;
        } catch { }
    }

    const { dateFrom, dateTo } = getCurrentMonthRangeForInputs();

    setValue("dateFrom", dateFrom);
    setValue("dateTo", dateTo);

    const state = readState();
    state.dateFrom = dateFrom;
    state.dateTo = dateTo;

    localStorage.setItem(storageKey, JSON.stringify(state));
}

function getCurrentMonthRangeForInputs() {
    const now = new Date();

    const y = now.getFullYear();
    const m = now.getMonth();

    const first = new Date(y, m, 1);
    const last = new Date(y, m + 1, 0);

    return {
        dateFrom: toDateInputValue(first),
        dateTo: toDateInputValue(last)
    };
}

function toDateInputValue(d) {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${y}-${m}-${day}`;
}

async function loadAndRenderDictionaries() {
    try {
        const token = getRequestVerificationToken();

        const [groups, priorities, statuses, typeGroups, types] = await Promise.all([
            fetchDict("EmployeeGroupList", "groupsLoading", "Не удалось загрузить группы."),
            fetchDict("IssuePriorityList", "prioritiesLoading", "Не удалось загрузить приоритеты."),
            fetchDict("IssueStatusList", "statusesLoading", "Не удалось загрузить статусы."),
            fetchDict("IssueTypeGroupList", null, "Не удалось загрузить папки типов."),
            fetchDict("IssueTypeList", "typesLoading", "Не удалось загрузить типы.")
        ]);

        const employees = await loadEmployeesByGroups(token);
        allEmployees = Array.isArray(employees) ? employees : [];

        renderEmployeeList(allEmployees);
        renderCheckboxList("listGroups", "filter-groups", groups, x => x.name ?? x.Name ?? "", "g");
        renderCheckboxList("listPriorities", "filter-priorities", priorities, x => x.name ?? x.Name ?? "", "p");
        renderCheckboxList("listStatuses", "filter-statuses", statuses, x => x.name ?? x.Name ?? "", "s");
        renderTypesTree("listTypes", typeGroups, types);
    }
    catch (e) {
        console.error(e);
    }
}

async function fetchDict(handlerName, loadingId, defaultErrorMessage) {
    const loadingEl = getEl(loadingId);
    if (loadingEl) loadingEl.textContent = "Загрузка...";

    const url = `${window.location.pathname}?handler=${encodeURIComponent(handlerName)}`;

    try {
        const resp = await sendJsonRequest(url, "GET");
        const data = unwrapOrThrow(resp, defaultErrorMessage);

        if (loadingEl) loadingEl.remove();

        return Array.isArray(data) ? data : [];
    }
    catch (e) {
        console.error(`Failed to load dict ${handlerName}`, e);

        if (loadingEl) {
            loadingEl.textContent = "Ошибка загрузки";
            loadingEl.classList.remove("text-muted");
            loadingEl.classList.add("text-danger");
        }

        return [];
    }
}

async function loadEmployeesByGroups(token, groupIds = null) {
    const payload = {
        groupIds: groupIds && groupIds.length > 0 ? groupIds.map(Number) : null
    };

    const resp = await sendJsonRequest(`?handler=EmployeeList`, "POST", buildJsonHeaders(token), payload);
    return unwrapOrThrow(resp, "Ошибка загрузки сотрудников.");
}

function renderCheckboxList(listId, checkboxClass, items, textSelector, idPrefix) {
    const list = getEl(listId);
    if (!list) return;

    list.innerHTML = "";

    if (!items || items.length === 0) {
        const div = document.createElement("div");
        div.className = "text-muted small px-1 py-2";
        div.textContent = "Нет данных";
        list.appendChild(div);
        return;
    }

    for (const x of items) {
        const id = Number(x.id ?? x.Id);
        if (!id || id <= 0) continue;

        const labelText = (textSelector(x) || "").trim();
        const inputId = `${idPrefix}${id}`;

        const wrap = document.createElement("div");
        wrap.className = "form-check";

        const input = document.createElement("input");
        input.className = `form-check-input ${checkboxClass}`;
        input.type = "checkbox";
        input.value = String(id);
        input.id = inputId;

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = inputId;
        label.textContent = labelText || `#${id}`;

        wrap.appendChild(input);
        wrap.appendChild(label);
        list.appendChild(wrap);
    }
}

function renderEmployeeList(items) {
    const list = getEl("listEmployees");
    if (!list) return;

    list.innerHTML = "";

    if (!items || items.length === 0) {
        const div = document.createElement("div");
        div.className = "text-muted small px-1 py-2";
        div.textContent = "Нет данных";
        list.appendChild(div);
        return;
    }

    for (const x of items) {
        const id = Number(x.id ?? x.Id);
        if (!id || id <= 0) continue;

        const groupIds = (x.groupIds ?? x.GroupIds ?? []).map(Number).filter(n => n > 0);
        const inputId = `e${id}`;

        const wrap = document.createElement("div");
        wrap.className = "form-check";
        wrap.dataset.groupIds = groupIds.join(",");

        const input = document.createElement("input");
        input.className = "form-check-input filter-employees";
        input.type = "checkbox";
        input.value = String(id);
        input.id = inputId;

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = inputId;
        label.textContent = employeeDisplayName(x) || `#${id}`;

        wrap.appendChild(input);
        wrap.appendChild(label);
        list.appendChild(wrap);
    }
}

function employeeDisplayName(x) {
    const ln = (x.lastName ?? x.LastName ?? "").trim();
    const fn = (x.firstName ?? x.FirstName ?? "").trim();
    const pn = (x.patronymic ?? x.Patronymic ?? "").trim();
    return [ln, fn, pn].filter(s => s && s.length > 0).join(" ");
}

function applyEmployeeVisibilityByGroups() {
    const selectedGroups = new Set(getChecked(".filter-groups").map(Number));

    const list = getEl("listEmployees");
    if (!list) return;

    const filterEnabled = selectedGroups.size > 0;

    list.querySelectorAll(".form-check").forEach(item => {
        const input = item.querySelector("input.filter-employees");
        if (!input) return;

        if (!filterEnabled) {
            item.classList.remove("d-none-by-groups");
            item.classList.toggle("d-none", item.classList.contains("d-none-by-search") || item.classList.contains("d-none-by-groups"));
            return;
        }

        const raw = item.dataset.groupIds || "";
        const empGroups = raw.length > 0 ? raw.split(",").map(Number).filter(n => n > 0) : [];

        const visible = empGroups.some(gid => selectedGroups.has(gid));
        item.classList.toggle("d-none-by-groups", !visible);
        item.classList.toggle("d-none", item.classList.contains("d-none-by-search") || item.classList.contains("d-none-by-groups"));

        if (!visible && input.checked) input.checked = false;
    });

    updateBadges();
}

function applyGroupsVisibilityByEmployees() {
    const hasEmployees = countChecked(".filter-employees") > 0;

    const list = getEl("listGroups");
    if (!list) return;

    list.querySelectorAll(".form-check").forEach(item => {
        item.classList.toggle("d-none-by-employees", hasEmployees);
        item.classList.toggle("d-none", item.classList.contains("d-none-by-search") || item.classList.contains("d-none-by-employees"));
    });

    const search = getEl("searchGroups");
    if (search) search.classList.toggle("d-none", hasEmployees);

    const actions = getEl("groupsActions");
    if (actions) actions.classList.toggle("d-none", hasEmployees);

    const hint = getEl("groupsHint");
    if (hint) hint.classList.toggle("d-none", !hasEmployees);
}

function applyMutualExclusionUI() {
    const hasGroups = countChecked(".filter-groups") > 0;
    const hasEmployees = countChecked(".filter-employees") > 0;

    const employeesBlocked = hasGroups;
    const groupsBlocked = hasEmployees;

    setDropdownBlocked(employeesBlocked, "employeesHint", "searchEmployees", "employeesActions", "listEmployees",
        "При выборе групп нельзя выбрать конкретного сотрудника. Снимите выбор групп, чтобы выбрать сотрудников."
    );

    setDropdownBlocked(groupsBlocked, "groupsHint", "searchGroups", "groupsActions", "listGroups",
        "При выборе сотрудников фильтр по группам недоступен. Снимите выбор сотрудников, чтобы выбрать группы."
    );
}

function setDropdownBlocked(isBlocked, hintId, searchId, actionsId, listId, hintText) {
    const hint = getEl(hintId);
    const search = getEl(searchId);
    const actions = getEl(actionsId);
    const list = getEl(listId);

    if (hint) {
        hint.textContent = hintText;
        hint.classList.toggle("d-none", !isBlocked);
    }

    if (search) search.classList.toggle("d-none", isBlocked);
    if (actions) actions.classList.toggle("d-none", isBlocked);
    if (list) list.classList.toggle("d-none", isBlocked);
}

function renderTypesTree(listId, folders, types) {
    const list = getEl(listId);
    if (!list) return;

    list.innerHTML = '';

    const folderItems = Array.isArray(folders) ? folders : [];
    const typeItems = Array.isArray(types) ? types : [];

    if (folderItems.length === 0 && typeItems.length === 0) {
        const div = document.createElement('div');
        div.className = 'text-muted small px-1 py-2';
        div.textContent = 'Нет данных';
        list.appendChild(div);
        return;
    }

    const nodes = buildFolderTree(folderItems);
    const typesByGroupId = buildTypesByGroup(typeItems);

    const root = document.createElement('div');
    root.id = 'typesTreeRoot';
    list.appendChild(root);

    nodes.forEach(n => {
        root.appendChild(renderFolderNode(n, typesByGroupId));
    });

    const ungrouped = typesByGroupId.get(0) || [];
    if (ungrouped.length > 0) {
        const leafWrap = document.createElement('div');
        leafWrap.className = 'types-root-leaves mb-2';

        ungrouped.forEach(t => {
            const row = document.createElement('div');
            row.className = 'form-check';

            const input = document.createElement('input');
            input.type = 'checkbox';
            input.className = 'form-check-input filter-types';
            input.value = String(t.id);
            input.id = `t${t.id}`;
            input.dataset.folderId = '0';

            const lb = document.createElement('label');
            lb.className = 'form-check-label';
            lb.htmlFor = input.id;
            lb.textContent = t.name || `#${t.id}`;

            row.appendChild(input);
            row.appendChild(lb);
            leafWrap.appendChild(row);
        });

        root.appendChild(leafWrap);
    }

    wireTypesTreeHandlers(list);
    initTypesTreeSearch('searchTypes', listId);
}

function syncTypesTreeFolders() {
    const root = getEl('listTypes');
    if (!root) return;

    Array.from(root.querySelectorAll('.types-folder')).forEach(folder => {
        updateFolderSelf(folder);
    });

    Array.from(root.querySelectorAll('.types-folder')).reverse().forEach(folder => {
        updateFolderUpwards(folder);
    });
}

function buildFolderTree(folderItems) {
    const byId = new Map();
    const roots = [];

    folderItems.forEach(x => {
        const id = Number(x.id ?? x.Id);
        if (!id || id <= 0) return;

        const name = String(x.name ?? x.Name ?? '').trim();
        const parentIdRaw = (x.parentId ?? x.ParentId);
        const parentId = parentIdRaw == null ? null : Number(parentIdRaw);

        byId.set(id, {
            id,
            name,
            parentId: (parentId && parentId > 0) ? parentId : null,
            children: []
        });
    });

    byId.forEach(n => {
        if (n.parentId && byId.has(n.parentId)) {
            byId.get(n.parentId).children.push(n);
        } else {
            roots.push(n);
        }
    });

    const sortRec = (arr) => {
        arr.sort((a, b) => (a.name || '').localeCompare(b.name || '', 'ru'));
        arr.forEach(x => sortRec(x.children));
    };

    sortRec(roots);
    return roots;
}

function buildTypesByGroup(typeItems) {
    const map = new Map();

    typeItems.forEach(x => {
        const id = Number(x.id ?? x.Id);
        if (!id || id <= 0) return;

        const name = String(x.name ?? x.Name ?? '').trim();
        const groupIdRaw = (x.groupId ?? x.GroupId);
        const groupId = (groupIdRaw == null) ? 0 : Number(groupIdRaw);

        const t = { id, name, groupId: (groupId && groupId > 0) ? groupId : 0 };

        if (!map.has(t.groupId)) map.set(t.groupId, []);
        map.get(t.groupId).push(t);
    });

    map.forEach(arr => arr.sort((a, b) => (a.name || '').localeCompare(b.name || '', 'ru')));
    return map;
}

function renderFolderNode(node, typesByGroupId) {
    const wrap = document.createElement('div');
    wrap.className = 'types-folder mb-2';
    wrap.dataset.folderId = String(node.id);

    const header = document.createElement('div');
    header.className = 'd-flex align-items-center gap-2';

    const toggle = document.createElement('button');
    toggle.type = 'button';
    toggle.className = 'btn btn-link p-0 text-decoration-none fs-5 d-inline-flex align-items-center justify-content-center';
    toggle.style.width = '22px';
    toggle.style.height = '22px';
    toggle.textContent = '▸';
    toggle.dataset.role = 'types-toggle';

    const cb = document.createElement('input');
    cb.type = 'checkbox';
    cb.className = 'form-check-input filter-types-folder';
    cb.dataset.folderId = String(node.id);

    const label = document.createElement('span');
    label.className = 'fw-semibold';
    label.textContent = node.name || `#${node.id}`;

    header.appendChild(toggle);
    header.appendChild(cb);
    header.appendChild(label);

    const body = document.createElement('div');
    body.className = 'types-body mt-1 ms-4 d-none';
    body.dataset.role = 'types-body';
    body.dataset.folderId = String(node.id);

    const childFolders = document.createElement('div');
    childFolders.className = 'types-children-folders';
    node.children.forEach(ch => childFolders.appendChild(renderFolderNode(ch, typesByGroupId)));
    body.appendChild(childFolders);

    const types = typesByGroupId.get(node.id) || [];
    if (types.length > 0) {
        const leafWrap = document.createElement('div');
        leafWrap.className = 'types-leaves';

        types.forEach(t => {
            const row = document.createElement('div');
            row.className = 'form-check';

            const input = document.createElement('input');
            input.type = 'checkbox';
            input.className = 'form-check-input filter-types';
            input.value = String(t.id);
            input.id = `t${t.id}`;
            input.dataset.folderId = String(node.id);

            const lb = document.createElement('label');
            lb.className = 'form-check-label';
            lb.htmlFor = input.id;
            lb.textContent = t.name || `#${t.id}`;

            row.appendChild(input);
            row.appendChild(lb);
            leafWrap.appendChild(row);
        });

        body.appendChild(leafWrap);
    }

    wrap.appendChild(header);
    wrap.appendChild(body);
    return wrap;
}

function wireTypesTreeHandlers(listRoot) {
    listRoot.addEventListener('click', (e) => {
        const btn = e.target;
        if (!btn || btn.dataset?.role !== 'types-toggle') return;

        const folder = btn.closest('.types-folder');
        if (!folder) return;

        const body = folder.querySelector('[data-role="types-body"]');
        if (!body) return;

        const isHidden = body.classList.contains('d-none');
        body.classList.toggle('d-none', !isHidden);
        btn.textContent = isHidden ? '▾' : '▸';
    });

    listRoot.addEventListener('change', (e) => {
        const el = e.target;
        if (!el) return;

        if (el.classList.contains('filter-types-folder')) {
            const folder = el.closest('.types-folder');
            if (!folder) return;

            const checked = el.checked === true;

            folder.querySelectorAll('input.filter-types-folder, input.filter-types').forEach(x => {
                x.checked = checked;
                if (x.classList.contains('filter-types-folder')) x.indeterminate = false;
            });

            updateFolderUpwards(folder);
            saveFilters(storageKey);
            updateBadges();
            return;
        }

        if (el.classList.contains('filter-types')) {
            const folder = el.closest('.types-folder');
            if (folder) {
                updateFolderSelf(folder);
                updateFolderUpwards(folder);
            }

            saveFilters(storageKey);
            updateBadges();
            return;
        }
    });
}

function updateFolderSelf(folder) {
    const folderCb = folder.querySelector('input.filter-types-folder');
    if (!folderCb) return;

    const cbs = Array.from(folder.querySelectorAll(':scope input.filter-types, :scope .types-children-folders input.filter-types-folder'));
    const total = cbs.length;
    const checked = cbs.filter(x => x.checked).length;
    const ind = cbs.filter(x => x.indeterminate).length;

    folderCb.indeterminate = (ind > 0) || (checked > 0 && checked < total);
    folderCb.checked = total > 0 && checked === total && ind === 0;
}

function updateFolderUpwards(folder) {
    let parent = folder.parentElement;

    while (parent) {
        const parentFolder = parent.closest('.types-folder');
        if (!parentFolder) break;

        updateFolderSelf(parentFolder);
        parent = parentFolder.parentElement;
    }
}

function initTypesTreeSearch(searchId, listId) {
    const input = getEl(searchId);
    const list = getEl(listId);
    if (!input || !list) return;

    input.addEventListener('input', () => {
        const q = String(input.value || '').trim().toLowerCase();

        const folders = Array.from(list.querySelectorAll('.types-folder'));

        if (q.length === 0) {
            folders.forEach(f => f.classList.remove('d-none'));
            list.querySelectorAll('.form-check').forEach(r => r.classList.remove('d-none'));
            return;
        }

        const rootLeaves = Array.from(list.querySelectorAll('.types-root-leaves .form-check'));
        rootLeaves.forEach(row => {
            const text = String(row.querySelector('label')?.textContent || '').toLowerCase();
            row.classList.toggle('d-none', !text.includes(q));
        });

        folders.forEach(folder => {
            const title = String(folder.querySelector('span.fw-semibold')?.textContent || '').toLowerCase();
            let anyMatch = false;

            folder.querySelectorAll(':scope .form-check').forEach(row => {
                const text = String(row.querySelector('label')?.textContent || '').toLowerCase();
                const ok = title.includes(q) || text.includes(q);
                row.classList.toggle('d-none', !ok);
                if (ok) anyMatch = true;
            });

            const childFolders = Array.from(folder.querySelectorAll(':scope .types-children-folders > .types-folder'));
            childFolders.forEach(ch => {
                if (!ch.classList.contains('d-none')) anyMatch = true;
            });

            folder.classList.toggle('d-none', !anyMatch);

            if (anyMatch) {
                let p = folder.parentElement?.closest('.types-folder');
                while (p) {
                    p.classList.remove('d-none');
                    p = p.parentElement?.closest('.types-folder');
                }
            }
        });
    });
}
