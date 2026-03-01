const planSettingsSelectedPlanKey = "crm_plan_settings_selected_plan_v2";
const planSettingsNameFilterKey = "crm_plan_settings_name_filter_v1";
const planSettingsGroupFilterKey = "crm_plan_settings_group_filter_v1";
const defaultPickerColor = "#000000";

let antiForgeryToken = null;
let isSaving = false;
let isReloading = false;

let plans = [];
let selectedPlanId = null;
let generalSettings = null;
let employeeRows = [];
let colorRules = [];
let selectedEmployeeIds = null;
let selectedGroupIds = null;

document.addEventListener("DOMContentLoaded", () => {
    antiForgeryToken = getRequestVerificationToken();

    initButtons();
    initInputs();
    initNameFilter();
    initGroupFilter();
    loadInitial();
    initCollapseToggleButtons();
});

function initButtons() {
    const btnReload = document.getElementById("btnReloadEmployees");
    const btnSaveAll = document.getElementById("btnSaveAll");
    const btnAddPlan = document.getElementById("btnAddPlan");
    const btnAddColorRule = document.getElementById("btnAddColorRule");
    const selectPlan = document.getElementById("activePlanSelect");

    if (btnReload) {
        btnReload.addEventListener("click", async () => {
            if (isReloading || isSaving) return;
            await reloadCurrentPlanData();
        });
    }

    if (btnSaveAll) {
        btnSaveAll.addEventListener("click", async () => {
            if (isSaving || isReloading) return;
            await saveAll();
        });
    }

    if (btnAddPlan) {
        btnAddPlan.addEventListener("click", () => {
            plans.push({ id: null, name: "", planColor: "" });
            renderPlansTable();
            renderPlanSelect();
        });
    }

    if (btnAddColorRule) {
        btnAddColorRule.addEventListener("click", () => {
            if (!selectedPlanId) return;

            colorRules.push({
                id: null,
                planId: selectedPlanId,
                fromPercent: 0,
                toPercent: null,
                color: "#198754"
            });

            renderColorRulesTable();
        });
    }

    if (selectPlan) {
        selectPlan.addEventListener("change", async () => {
            selectedPlanId = normalizeGuid(selectPlan.value);
            saveSelectedPlanState();
            await reloadCurrentPlanData();
        });
    }
}

function initInputs() {
    const inputSeconds = document.getElementById("planSwitchSecondsInput");
    if (!inputSeconds) return;

    inputSeconds.addEventListener("input", () => {
        inputSeconds.value = (inputSeconds.value || "").replace(/[^\d]/g, "");
    });
}

function initCollapseToggleButtons() {
    bindCollapseToggleText("plansSectionBody", "btnTogglePlansSection");
    bindCollapseToggleText("colorRulesSectionBody", "btnToggleColorRulesSection");
    bindCollapseToggleText("employeePlansSectionBody", "btnToggleEmployeePlansSection");
}

function bindCollapseToggleText(collapseId, buttonId) {
    const collapseEl = document.getElementById(collapseId);
    const buttonEl = document.getElementById(buttonId);
    if (!collapseEl || !buttonEl) return;

    const applyText = () => {
        buttonEl.textContent = collapseEl.classList.contains("show")
            ? "Свернуть"
            : "Развернуть";
    };

    collapseEl.addEventListener("shown.bs.collapse", applyText);
    collapseEl.addEventListener("hidden.bs.collapse", applyText);

    applyText();
}

async function loadInitial() {
    clearPageError();
    loadFilterState();

    const okGeneral = await reloadGeneralSettings();
    const okPlans = await reloadPlans();

    if (!okGeneral || !okPlans) return;

    applySelectedPlanFromState();
    await reloadCurrentPlanData();
}

async function reloadGeneralSettings() {
    try {
        const response = await sendJsonRequest("?handler=GeneralSettings", "GET", buildJsonHeaders(antiForgeryToken));
        const data = unwrapOrThrow(response, "Ошибка загрузки общих настроек.");

        generalSettings = data;
        const inputSeconds = document.getElementById("planSwitchSecondsInput");
        if (inputSeconds) {
            inputSeconds.value = data && Number.isInteger(Number(data.planSwitchSeconds))
                ? String(data.planSwitchSeconds)
                : "10";
        }

        return true;
    } catch (error) {
        showPageError(error?.message || "Ошибка загрузки общих настроек.");
        return false;
    }
}

async function reloadPlans() {
    try {
        const response = await sendJsonRequest("?handler=Plans", "GET", buildJsonHeaders(antiForgeryToken));
        const data = unwrapOrThrow(response, "Ошибка загрузки планов.");
        plans = Array.isArray(data) ? data.map(mapPlanItem) : [];

        renderPlansTable();
        renderPlanSelect();
        return true;
    } catch (error) {
        showPageError(error?.message || "Ошибка загрузки планов.");
        return false;
    }
}

async function reloadCurrentPlanData() {
    if (isReloading) return;

    isReloading = true;
    clearPageError();

    if (!selectedPlanId) {
        employeeRows = [];
        colorRules = [];
        renderEmployeeRowsTable();
        renderColorRulesTable();
        isReloading = false;
        return;
    }

    try {
        const employeeResponse = await sendJsonRequest(
            `?handler=EmployeePlanRows&planId=${encodeURIComponent(selectedPlanId)}`,
            "GET",
            buildJsonHeaders(antiForgeryToken)
        );

        const employeeData = unwrapOrThrow(employeeResponse, "Ошибка загрузки планов сотрудников.");
        employeeRows = Array.isArray(employeeData) ? employeeData : [];

        const rulesResponse = await sendJsonRequest(
            `?handler=PlanColorRules&planId=${encodeURIComponent(selectedPlanId)}`,
            "GET",
            buildJsonHeaders(antiForgeryToken)
        );

        const rulesData = unwrapOrThrow(rulesResponse, "Ошибка загрузки цветовых правил.");
        colorRules = Array.isArray(rulesData) ? rulesData.map(mapColorRuleItem) : [];

        renderEmployeeRowsTable();
        renderColorRulesTable();
    } catch (error) {
        showPageError(error?.message || "Ошибка загрузки данных плана.");
    } finally {
        isReloading = false;
    }
}

function renderPlansTable() {
    const tbody = document.getElementById("plansTableBody");
    if (!tbody) return;

    tbody.textContent = "";

    if (!Array.isArray(plans) || plans.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");
        cell.colSpan = 3;
        cell.className = "text-center text-muted py-3";
        cell.textContent = "Планы не добавлены";
        row.appendChild(cell);
        tbody.appendChild(row);
        return;
    }

    for (let index = 0; index < plans.length; index++) {
        const plan = plans[index];
        const row = document.createElement("tr");

        const nameCell = document.createElement("td");
        const nameInput = document.createElement("input");
        nameInput.type = "text";
        nameInput.id = `plan_name_${index}`;
        nameInput.name = `plan_name_${index}`;
        nameInput.className = "form-control form-control-sm";
        nameInput.value = String(plan.name || "");
        nameInput.addEventListener("input", () => {
            plan.name = nameInput.value;
        });
        nameCell.appendChild(nameInput);

        const colorCell = document.createElement("td");
        const colorWrap = document.createElement("div");
        colorWrap.className = "d-flex align-items-center gap-2";

        const colorInput = document.createElement("input");
        colorInput.type = "text";
        colorInput.id = `plan_color_${index}`;
        colorInput.name = `plan_color_${index}`;
        colorInput.className = "form-control form-control-sm";
        colorInput.style.maxWidth = "130px";
        colorInput.value = String(plan.planColor || "");
        colorInput.addEventListener("input", () => {
            const normalized = sanitizeHex(colorInput.value);
            colorInput.value = normalized;
            plan.planColor = normalized;
            const isHex = normalized !== "" && isValidHex(normalized);
            if (isHex) {
                picker.value = normalized;
            } else if (normalized === "") {
                picker.value = defaultPickerColor;
            }
            setInvalid(colorInput, normalized !== "" && !isValidHex(normalized));
        });

        const picker = document.createElement("input");
        picker.type = "color";
        picker.id = `plan_color_picker_${index}`;
        picker.name = `plan_color_picker_${index}`;
        picker.className = "form-control form-control-color p-1";
        picker.value = isValidHex(plan.planColor) ? String(plan.planColor).toUpperCase() : defaultPickerColor;
        picker.addEventListener("input", () => {
            const value = String(picker.value || "").toUpperCase();
            colorInput.value = value;
            plan.planColor = value;
            setInvalid(colorInput, false);
        });

        colorWrap.append(colorInput, picker);
        colorCell.appendChild(colorWrap);

        const actionCell = document.createElement("td");
        actionCell.className = "text-end";
        const deleteBtn = document.createElement("button");
        deleteBtn.type = "button";
        deleteBtn.className = "btn btn-outline-danger btn-sm";
        deleteBtn.textContent = "Удалить";
        deleteBtn.addEventListener("click", () => {
            const removedId = normalizeGuid(plan.id);
            plans.splice(index, 1);

            if (removedId && selectedPlanId === removedId) {
                selectedPlanId = null;
            }

            renderPlansTable();
            renderPlanSelect();
            if (!selectedPlanId) {
                employeeRows = [];
                colorRules = [];
                renderEmployeeRowsTable();
                renderColorRulesTable();
            }
        });
        actionCell.appendChild(deleteBtn);

        row.append(nameCell, colorCell, actionCell);
        tbody.appendChild(row);
    }
}

function renderPlanSelect() {
    const select = document.getElementById("activePlanSelect");
    if (!select) return;

    const previousValue = selectedPlanId;
    select.textContent = "";

    for (const plan of plans) {
        const planId = normalizeGuid(plan.id);
        if (!planId) continue;

        const option = document.createElement("option");
        option.value = planId;
        option.textContent = String(plan.name || "");
        select.appendChild(option);
    }

    if (previousValue && plans.some(x => normalizeGuid(x.id) === previousValue)) {
        selectedPlanId = previousValue;
    } else {
        const firstPlan = plans.find(x => !!normalizeGuid(x.id));
        selectedPlanId = firstPlan ? normalizeGuid(firstPlan.id) : null;
    }

    select.value = selectedPlanId || "";
    saveSelectedPlanState();
}

function renderEmployeeRowsTable() {
    const tbody = document.getElementById("employeePlansBody");
    if (!tbody) return;

    tbody.textContent = "";

    if (!selectedPlanId) {
        appendEmptyRow(tbody, 3, "Сначала выберите план");
        updateFilterButtonsState();
        rebuildNameFilterList();
        rebuildGroupFilterList();
        return;
    }

    if (!Array.isArray(employeeRows) || employeeRows.length === 0) {
        appendEmptyRow(tbody, 3, "Нет данных");
        updateFilterButtonsState();
        rebuildNameFilterList();
        rebuildGroupFilterList();
        return;
    }

    const filteredRows = getFilteredEmployeeRows(employeeRows);
    const sortedRows = [...filteredRows].sort((a, b) =>
        String(a.fullName || "").localeCompare(String(b.fullName || ""), "ru")
    );

    if (sortedRows.length === 0) {
        appendEmptyRow(tbody, 3, "Нет данных по выбранным фильтрам");
        updateFilterButtonsState();
        rebuildNameFilterList();
        rebuildGroupFilterList();
        return;
    }

    for (const rowData of sortedRows) {
        const row = document.createElement("tr");

        const nameCell = document.createElement("td");
        nameCell.textContent = String(rowData.fullName || "");

        const groupsCell = document.createElement("td");
        groupsCell.textContent = formatGroups(rowData.groups);

        const planCell = document.createElement("td");
        planCell.className = "text-center";

        const input = document.createElement("input");
        input.type = "text";
        input.id = `employee_plan_${rowData.employeeId}`;
        input.name = `employee_plan_${rowData.employeeId}`;
        input.inputMode = "numeric";
        input.className = "form-control form-control-sm text-center";
        input.style.maxWidth = "140px";
        input.style.margin = "0 auto";
        input.value = rowData.planValue === null || rowData.planValue === undefined ? "" : String(rowData.planValue);
        input.addEventListener("input", () => {
            const cleaned = (input.value || "").replace(/[^\d]/g, "");
            input.value = cleaned;
            rowData.planValue = cleaned === "" ? null : Number(cleaned);
        });

        planCell.appendChild(input);
        row.append(nameCell, groupsCell, planCell);
        tbody.appendChild(row);
    }

    updateFilterButtonsState();
    rebuildNameFilterList();
    rebuildGroupFilterList();
}

function renderColorRulesTable() {
    const tbody = document.getElementById("colorRulesBody");
    if (!tbody) return;

    tbody.textContent = "";

    if (!selectedPlanId) {
        appendEmptyRow(tbody, 4, "Сначала выберите план");
        return;
    }

    if (!Array.isArray(colorRules) || colorRules.length === 0) {
        appendEmptyRow(tbody, 4, "Правила не добавлены");
        return;
    }

    for (let index = 0; index < colorRules.length; index++) {
        const rule = colorRules[index];
        const row = document.createElement("tr");

        const fromCell = document.createElement("td");
        fromCell.appendChild(buildPercentInput(rule.fromPercent, (val) => { rule.fromPercent = val; }, `rule_from_${index}`));

        const toCell = document.createElement("td");
        toCell.appendChild(buildNullablePercentInput(rule.toPercent, (val) => { rule.toPercent = val; }, `rule_to_${index}`));

        const colorCell = document.createElement("td");
        colorCell.appendChild(buildRuleColorEditor(rule, index));

        const actionCell = document.createElement("td");
        actionCell.className = "text-end";
        const deleteBtn = document.createElement("button");
        deleteBtn.type = "button";
        deleteBtn.className = "btn btn-outline-danger btn-sm";
        deleteBtn.textContent = "Удалить";
        deleteBtn.addEventListener("click", () => {
            colorRules.splice(index, 1);
            renderColorRulesTable();
        });
        actionCell.appendChild(deleteBtn);

        row.append(fromCell, toCell, colorCell, actionCell);
        tbody.appendChild(row);
    }
}

async function saveAll() {
    isSaving = true;
    clearPageError();

    const saveBtn = document.getElementById("btnSaveAll");
    if (saveBtn) saveBtn.disabled = true;

    try {
        const generalSettingsPayload = buildGeneralSettingsPayload();
        if (!generalSettingsPayload) {
            throw new Error("Укажите корректный интервал переключения планов (минимум 10 секунд).");
        }

        const plansPayload = buildPlansPayload();
        await savePlans(plansPayload);

        const selectedBeforeReload = selectedPlanId;
        await reloadPlans();

        selectedPlanId = selectedBeforeReload && plans.some(x => normalizeGuid(x.id) === selectedBeforeReload)
            ? selectedBeforeReload
            : (plans.length > 0 ? normalizeGuid(plans[0].id) : null);
        renderPlanSelect();

        await saveGeneralSettings(generalSettingsPayload);

        if (selectedPlanId) {
            await saveEmployeePlanValues(buildPlanSettingsPayload());
            await saveColorRules(buildColorRulesPayload());
            await reloadCurrentPlanData();
        }
    } catch (error) {
        showPageError(error?.message || "Ошибка сохранения.");
    } finally {
        isSaving = false;
        if (saveBtn) saveBtn.disabled = false;
    }
}

function buildGeneralSettingsPayload() {
    const inputSeconds = document.getElementById("planSwitchSecondsInput");
    const seconds = Number((inputSeconds ? inputSeconds.value : "").trim());

    if (!Number.isInteger(seconds) || seconds < 10) {
        if (inputSeconds) setInvalid(inputSeconds, true);
        return null;
    }

    if (inputSeconds) setInvalid(inputSeconds, false);

    return {
        item: {
            id: generalSettings && generalSettings.id ? String(generalSettings.id) : null,
            planSwitchSeconds: seconds
        }
    };
}

function buildPlansPayload() {
    const normalized = [];

    for (const plan of plans) {
        const trimmedName = String(plan.name || "").trim();
        if (trimmedName.length === 0) {
            throw new Error("Название плана не может быть пустым.");
        }

        const normalizedColor = String(plan.planColor || "").trim().toUpperCase();
        if (normalizedColor !== "" && !isValidHex(normalizedColor)) {
            throw new Error(`РЈРєР°Р¶РёС‚Рµ РєРѕСЂСЂРµРєС‚РЅС‹Р№ С†РІРµС‚ РїР»Р°РЅР° (${trimmedName}) РІ С„РѕСЂРјР°С‚Рµ #RRGGBB.`);
        }

        normalized.push({
            id: normalizeGuid(plan.id),
            name: trimmedName,
            planColor: normalizedColor === "" ? null : normalizedColor
        });
    }

    const duplicates = hasDuplicateNames(normalized.map(x => x.name));
    if (duplicates) {
        throw new Error("Названия планов должны быть уникальными.");
    }

    return { items: normalized };
}

function buildPlanSettingsPayload() {
    if (!selectedPlanId) return { items: [] };

    const items = [];
    for (const rowData of employeeRows) {
        items.push({
            planId: selectedPlanId,
            employeeId: Number(rowData.employeeId),
            planValue: rowData.planValue === null || rowData.planValue === undefined ? null : Number(rowData.planValue)
        });
    }

    return { items };
}

function buildColorRulesPayload() {
    if (!selectedPlanId) return { planId: null, items: [] };

    validateColorRulesOrThrow();

    const items = colorRules.map((rule) => ({
        id: normalizeGuid(rule.id),
        planId: selectedPlanId,
        fromPercent: Number(rule.fromPercent),
        toPercent: rule.toPercent === null || rule.toPercent === undefined || rule.toPercent === "" ? null : Number(rule.toPercent),
        color: String(rule.color || "").trim().toUpperCase()
    }));

    return {
        planId: selectedPlanId,
        items
    };
}

async function savePlans(payload) {
    const response = await sendJsonRequest("?handler=SavePlans", "POST", buildJsonHeaders(antiForgeryToken), payload);
    unwrapOrThrow(response, "Ошибка сохранения планов.");
}

async function saveGeneralSettings(payload) {
    const response = await sendJsonRequest("?handler=SaveGeneralSettings", "POST", buildJsonHeaders(antiForgeryToken), payload);
    unwrapOrThrow(response, "Ошибка сохранения общих настроек.");
}

async function saveEmployeePlanValues(payload) {
    const response = await sendJsonRequest("?handler=SavePlanSettings", "POST", buildJsonHeaders(antiForgeryToken), payload);
    unwrapOrThrow(response, "Ошибка сохранения значений плана сотрудников.");
}

async function saveColorRules(payload) {
    const response = await sendJsonRequest("?handler=SavePlanColorRules", "POST", buildJsonHeaders(antiForgeryToken), payload);
    unwrapOrThrow(response, "Ошибка сохранения цветовых правил.");
}

function validateColorRulesOrThrow() {
    const normalizedRules = [];

    for (const rule of colorRules) {
        const from = Number(rule.fromPercent);
        const to = rule.toPercent === null || rule.toPercent === undefined || rule.toPercent === ""
            ? null
            : Number(rule.toPercent);
        const color = String(rule.color || "").trim().toUpperCase();

        if (!Number.isInteger(from) || from < 0) {
            throw new Error("Поле «От %» должно быть целым числом >= 0.");
        }

        if (to !== null && (!Number.isInteger(to) || to < from)) {
            throw new Error("Поле «До %» должно быть пустым или целым числом >= «От %».");
        }

        if (!isValidHex(color)) {
            throw new Error("Цвет правила должен быть в формате #RRGGBB.");
        }

        normalizedRules.push({
            from,
            to: to === null ? Number.MAX_SAFE_INTEGER : to
        });
    }

    normalizedRules.sort((a, b) => {
        if (a.from !== b.from) return a.from - b.from;
        return a.to - b.to;
    });

    for (let index = 0; index < normalizedRules.length - 1; index++) {
        const current = normalizedRules[index];
        const next = normalizedRules[index + 1];
        if (next.from <= current.to) {
            throw new Error("Диапазоны процентов не должны пересекаться.");
        }
    }
}

function buildPercentInput(value, onChanged, key) {
    const input = document.createElement("input");
    input.type = "text";
    input.id = key;
    input.name = key;
    input.inputMode = "numeric";
    input.className = "form-control form-control-sm text-center";
    input.value = value === null || value === undefined ? "0" : String(value);
    input.addEventListener("input", () => {
        const cleaned = (input.value || "").replace(/[^\d]/g, "");
        input.value = cleaned;
        onChanged(cleaned === "" ? 0 : Number(cleaned));
    });
    return input;
}

function buildNullablePercentInput(value, onChanged, key) {
    const input = document.createElement("input");
    input.type = "text";
    input.id = key;
    input.name = key;
    input.inputMode = "numeric";
    input.className = "form-control form-control-sm text-center";
    input.placeholder = "Без ограничения";
    input.value = value === null || value === undefined ? "" : String(value);
    input.addEventListener("input", () => {
        const cleaned = (input.value || "").replace(/[^\d]/g, "");
        input.value = cleaned;
        onChanged(cleaned === "" ? null : Number(cleaned));
    });
    return input;
}

function buildRuleColorEditor(rule, index) {
    const wrap = document.createElement("div");
    wrap.className = "d-flex align-items-center gap-2";

    const input = document.createElement("input");
    input.type = "text";
    input.id = `rule_color_${index}`;
    input.name = `rule_color_${index}`;
    input.className = "form-control form-control-sm";
    input.style.maxWidth = "130px";
    input.value = String(rule.color || "#198754");
    input.addEventListener("input", () => {
        const normalized = sanitizeHex(input.value);
        input.value = normalized;
        rule.color = normalized;
        setInvalid(input, normalized !== "" && !isValidHex(normalized));
    });

    const picker = document.createElement("input");
    picker.type = "color";
    picker.id = `rule_color_picker_${index}`;
    picker.name = `rule_color_picker_${index}`;
    picker.className = "form-control form-control-color p-1";
    picker.value = isValidHex(rule.color) ? String(rule.color) : "#198754";
    picker.addEventListener("input", () => {
        const value = String(picker.value || "").toUpperCase();
        input.value = value;
        rule.color = value;
        setInvalid(input, false);
    });

    wrap.append(input, picker);
    return wrap;
}

function applySelectedPlanFromState() {
    const savedPlanId = normalizeGuid(localStorage.getItem(planSettingsSelectedPlanKey));
    if (savedPlanId && plans.some(x => normalizeGuid(x.id) === savedPlanId)) {
        selectedPlanId = savedPlanId;
    } else {
        const firstPlan = plans.find(x => !!normalizeGuid(x.id));
        selectedPlanId = firstPlan ? normalizeGuid(firstPlan.id) : null;
    }

    const select = document.getElementById("activePlanSelect");
    if (select) {
        select.value = selectedPlanId || "";
    }
}

function saveSelectedPlanState() {
    if (selectedPlanId) {
        localStorage.setItem(planSettingsSelectedPlanKey, selectedPlanId);
    } else {
        localStorage.removeItem(planSettingsSelectedPlanKey);
    }
}

function mapPlanItem(item) {
    return {
        id: normalizeGuid(item?.id),
        name: String(item?.name || ""),
        planColor: String(item?.planColor || "").toUpperCase()
    };
}

function mapColorRuleItem(item) {
    return {
        id: normalizeGuid(item?.id),
        planId: normalizeGuid(item?.planId),
        fromPercent: Number(item?.fromPercent || 0),
        toPercent: item?.toPercent === null || item?.toPercent === undefined ? null : Number(item.toPercent),
        color: String(item?.color || "#198754").toUpperCase()
    };
}

function normalizeGuid(value) {
    const text = String(value || "").trim();
    if (text.length === 0) return null;
    return text;
}

function formatGroups(groups) {
    if (!Array.isArray(groups) || groups.length === 0) return "";
    return groups
        .map((group) => String(group?.name || "").trim())
        .filter((name) => name.length > 0)
        .join(", ");
}

function initNameFilter() {
    const btn = document.getElementById("btnNameFilter");
    const modalEl = document.getElementById("nameFilterModal");
    if (!btn || !modalEl || !window.bootstrap) return;

    const modal = new bootstrap.Modal(modalEl);

    btn.addEventListener("click", (event) => {
        event.stopPropagation();
        rebuildNameFilterList();
        modal.show();
    });

    const search = document.getElementById("nameFilterSearch");
    if (search) {
        search.addEventListener("input", () => rebuildNameFilterList());
    }

    const selectAll = document.getElementById("nameFilterSelectAll");
    if (selectAll) {
        selectAll.addEventListener("click", () => {
            selectedEmployeeIds = employeeRows.map((row) => Number(row.employeeId));
            saveFilterState();
            rebuildNameFilterList();
            updateFilterButtonsState();
        });
    }

    const clearAll = document.getElementById("nameFilterClearAll");
    if (clearAll) {
        clearAll.addEventListener("click", () => {
            selectedEmployeeIds = null;
            saveFilterState();
            rebuildNameFilterList();
            updateFilterButtonsState();
        });
    }

    const apply = document.getElementById("nameFilterApply");
    if (apply) {
        apply.addEventListener("click", () => {
            renderEmployeeRowsTable();
        });
    }
}

function initGroupFilter() {
    const btn = document.getElementById("btnGroupFilter");
    const modalEl = document.getElementById("groupFilterModal");
    if (!btn || !modalEl || !window.bootstrap) return;

    const modal = new bootstrap.Modal(modalEl);

    btn.addEventListener("click", (event) => {
        event.stopPropagation();
        rebuildGroupFilterList();
        modal.show();
    });

    const search = document.getElementById("groupFilterSearch");
    if (search) {
        search.addEventListener("input", () => rebuildGroupFilterList());
    }

    const selectAll = document.getElementById("groupFilterSelectAll");
    if (selectAll) {
        selectAll.addEventListener("click", () => {
            selectedGroupIds = getAllGroupIds();
            saveFilterState();
            rebuildGroupFilterList();
            updateFilterButtonsState();
        });
    }

    const clearAll = document.getElementById("groupFilterClearAll");
    if (clearAll) {
        clearAll.addEventListener("click", () => {
            selectedGroupIds = null;
            saveFilterState();
            rebuildGroupFilterList();
            updateFilterButtonsState();
        });
    }

    const apply = document.getElementById("groupFilterApply");
    if (apply) {
        apply.addEventListener("click", () => {
            renderEmployeeRowsTable();
        });
    }
}

function getFilteredEmployeeRows(rows) {
    let filteredRows = Array.isArray(rows) ? rows : [];

    if (Array.isArray(selectedEmployeeIds) && selectedEmployeeIds.length > 0) {
        const selectedEmployeesSet = new Set(selectedEmployeeIds.map((id) => Number(id)));
        filteredRows = filteredRows.filter((row) => selectedEmployeesSet.has(Number(row.employeeId)));
    }

    if (Array.isArray(selectedGroupIds) && selectedGroupIds.length > 0) {
        const selectedGroupsSet = new Set(selectedGroupIds.map((id) => Number(id)));
        filteredRows = filteredRows.filter((row) => {
            if (!Array.isArray(row.groups) || row.groups.length === 0) return false;
            return row.groups.some((group) => selectedGroupsSet.has(Number(group?.id)));
        });
    }

    return filteredRows;
}

function updateFilterButtonsState() {
    const nameFilterButton = document.getElementById("btnNameFilter");
    if (nameFilterButton) {
        const isNameFilterActive = Array.isArray(selectedEmployeeIds) && selectedEmployeeIds.length > 0;
        nameFilterButton.classList.toggle("btn-secondary", isNameFilterActive);
        nameFilterButton.classList.toggle("btn-outline-secondary", !isNameFilterActive);
    }

    const groupFilterButton = document.getElementById("btnGroupFilter");
    if (groupFilterButton) {
        const isGroupFilterActive = Array.isArray(selectedGroupIds) && selectedGroupIds.length > 0;
        groupFilterButton.classList.toggle("btn-secondary", isGroupFilterActive);
        groupFilterButton.classList.toggle("btn-outline-secondary", !isGroupFilterActive);
    }
}

function rebuildNameFilterList() {
    const host = document.getElementById("nameFilterList");
    if (!host) return;

    host.textContent = "";

    const searchInput = document.getElementById("nameFilterSearch");
    const query = String(searchInput?.value || "").trim().toLowerCase();

    const selectedSet = Array.isArray(selectedEmployeeIds)
        ? new Set(selectedEmployeeIds.map((id) => Number(id)))
        : null;

    const rows = Array.isArray(employeeRows) ? [...employeeRows] : [];
    rows.sort((left, right) => String(left.fullName || "").localeCompare(String(right.fullName || ""), "ru"));

    const filteredRows = query
        ? rows.filter((row) => String(row.fullName || "").toLowerCase().includes(query))
        : rows;

    const hint = document.getElementById("nameFilterHint");
    if (hint) {
        hint.classList.add("d-none");
        hint.textContent = "";
        if (query && filteredRows.length === 0) {
            hint.textContent = "Ничего не найдено";
            hint.classList.remove("d-none");
        }
    }

    for (const row of filteredRows) {
        const wrap = document.createElement("div");
        wrap.className = "form-check";

        const input = document.createElement("input");
        input.type = "checkbox";
        input.className = "form-check-input";
        input.id = `empf_${row.employeeId}`;
        input.name = `empf_${row.employeeId}`;
        input.checked = selectedSet ? selectedSet.has(Number(row.employeeId)) : false;

        input.addEventListener("change", () => {
            const employeeId = Number(row.employeeId);
            if (!Number.isFinite(employeeId)) return;

            let selectedIds = Array.isArray(selectedEmployeeIds) ? [...selectedEmployeeIds] : [];
            if (input.checked) {
                if (!selectedIds.includes(employeeId)) {
                    selectedIds.push(employeeId);
                }
            } else {
                selectedIds = selectedIds.filter((id) => id !== employeeId);
            }

            selectedEmployeeIds = selectedIds.length > 0 ? selectedIds : null;
            saveFilterState();
            updateFilterButtonsState();
        });

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = input.id;
        label.textContent = String(row.fullName || "");

        wrap.append(input, label);
        host.appendChild(wrap);
    }
}

function getAllGroups() {
    const groupsById = new Map();

    for (const row of employeeRows) {
        if (!Array.isArray(row.groups)) continue;

        for (const group of row.groups) {
            const groupId = Number(group?.id);
            if (!Number.isFinite(groupId)) continue;

            const groupName = String(group?.name || "").trim();
            if (!groupsById.has(groupId)) {
                groupsById.set(groupId, { id: groupId, name: groupName });
            } else if (groupName && !groupsById.get(groupId).name) {
                groupsById.get(groupId).name = groupName;
            }
        }
    }

    return Array.from(groupsById.values())
        .sort((left, right) => String(left.name || "").localeCompare(String(right.name || ""), "ru"));
}

function getAllGroupIds() {
    return getAllGroups().map((group) => group.id);
}

function rebuildGroupFilterList() {
    const host = document.getElementById("groupFilterList");
    if (!host) return;

    host.textContent = "";

    const searchInput = document.getElementById("groupFilterSearch");
    const query = String(searchInput?.value || "").trim().toLowerCase();

    const selectedSet = Array.isArray(selectedGroupIds)
        ? new Set(selectedGroupIds.map((id) => Number(id)))
        : null;

    const groups = getAllGroups();
    const filteredGroups = query
        ? groups.filter((group) => String(group.name || "").toLowerCase().includes(query))
        : groups;

    const hint = document.getElementById("groupFilterHint");
    if (hint) {
        hint.classList.add("d-none");
        hint.textContent = "";
        if (query && filteredGroups.length === 0) {
            hint.textContent = "Ничего не найдено";
            hint.classList.remove("d-none");
        }
    }

    for (const group of filteredGroups) {
        const wrap = document.createElement("div");
        wrap.className = "form-check";

        const input = document.createElement("input");
        input.type = "checkbox";
        input.className = "form-check-input";
        input.id = `grpf_${group.id}`;
        input.name = `grpf_${group.id}`;
        input.checked = selectedSet ? selectedSet.has(group.id) : false;

        input.addEventListener("change", () => {
            const groupId = Number(group.id);
            if (!Number.isFinite(groupId)) return;

            let selectedIds = Array.isArray(selectedGroupIds) ? [...selectedGroupIds] : [];
            if (input.checked) {
                if (!selectedIds.includes(groupId)) {
                    selectedIds.push(groupId);
                }
            } else {
                selectedIds = selectedIds.filter((id) => id !== groupId);
            }

            selectedGroupIds = selectedIds.length > 0 ? selectedIds : null;
            saveFilterState();
            updateFilterButtonsState();
        });

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = input.id;
        label.textContent = String(group.name || `(id:${group.id})`);

        wrap.append(input, label);
        host.appendChild(wrap);
    }
}

function loadFilterState() {
    const nameFilterValue = localStorage.getItem(planSettingsNameFilterKey);
    if (nameFilterValue) {
        try {
            const parsed = JSON.parse(nameFilterValue);
            selectedEmployeeIds = Array.isArray(parsed) ? parsed : null;
        } catch {
            selectedEmployeeIds = null;
        }
    }

    const groupFilterValue = localStorage.getItem(planSettingsGroupFilterKey);
    if (groupFilterValue) {
        try {
            const parsed = JSON.parse(groupFilterValue);
            selectedGroupIds = Array.isArray(parsed) ? parsed : null;
        } catch {
            selectedGroupIds = null;
        }
    }
}

function saveFilterState() {
    if (Array.isArray(selectedEmployeeIds) && selectedEmployeeIds.length > 0) {
        localStorage.setItem(planSettingsNameFilterKey, JSON.stringify(selectedEmployeeIds));
    } else {
        localStorage.removeItem(planSettingsNameFilterKey);
    }

    if (Array.isArray(selectedGroupIds) && selectedGroupIds.length > 0) {
        localStorage.setItem(planSettingsGroupFilterKey, JSON.stringify(selectedGroupIds));
    } else {
        localStorage.removeItem(planSettingsGroupFilterKey);
    }
}
function hasDuplicateNames(names) {
    const set = new Set();
    for (const name of names) {
        const key = String(name || "").trim().toLowerCase();
        if (set.has(key)) return true;
        set.add(key);
    }
    return false;
}

function sanitizeHex(raw) {
    let value = String(raw || "").trim();
    if (value === "") return "";
    if (!value.startsWith("#")) value = `#${value}`;

    let out = "#";
    for (let index = 1; index < value.length && out.length < 7; index++) {
        const symbol = value[index];
        if (/[0-9a-fA-F]/.test(symbol)) out += symbol.toUpperCase();
    }

    return out;
}

function isValidHex(value) {
    return /^#([0-9A-F]{6})$/.test(String(value || "").toUpperCase());
}

function setInvalid(input, isInvalid) {
    if (!input) return;
    if (isInvalid) input.classList.add("is-invalid");
    else input.classList.remove("is-invalid");
}

function appendEmptyRow(tbody, colSpan, text) {
    const row = document.createElement("tr");
    const cell = document.createElement("td");
    cell.colSpan = colSpan;
    cell.className = "text-center text-muted py-3";
    cell.textContent = text;
    row.appendChild(cell);
    tbody.appendChild(row);
}

function showPageError(message) {
    const el = document.getElementById("pageError");
    if (!el) return;
    el.textContent = String(message || "Ошибка.");
    el.classList.remove("d-none");
}

function clearPageError() {
    const el = document.getElementById("pageError");
    if (!el) return;
    el.textContent = "";
    el.classList.add("d-none");
}

