const LOOKUP_PAGE_SIZE = 20;
const EQUIPMENTS_FILTERS_STORAGE_KEY = "crm_equipments_filters_v1";
const EQUIPMENTS_GRID_COLUMNS_STORAGE_KEY = "crm_equipments_grid_columns_v3";
const DEFAULT_COLUMN_MIN_WIDTH = 90;
const ID_COLUMN_MIN_WIDTH = 60;
const ID_COLUMN_MAX_WIDTH = 150;
const EQUIPMENT_INFO_COLUMN_WIDTH = 200;
const INVENTORY_AND_SERIAL_COLUMN_WIDTH = 200;
const COMPANY_AND_OBJECT_COLUMN_WIDTH = 300;
const GRID_LAST_COLUMN_INDEX = 6;
const LAST_RESIZABLE_COLUMN_INDEX = 5;
const AUTO_EXPANDING_COLUMN_INDEX = 6;
const AUTO_EXPANDING_COLUMN_MIN_WIDTH = 240;
const ACCESS_VALUE_SEPARATOR = "/";
const TERMINAL_ACCESS_BUTTONS = [
    { parameterCode: "AnyDesk", clearbatType: "anydesk", buttonText: "AD", iconPath: "/icon/equipments/anydesk.png?v=1", iconAlt: "AnyDesk", displayName: "ЭниДеск", requirePassword: true },
    { parameterCode: "AA", clearbatType: "ammyy", buttonText: "AA", iconPath: "/icon/equipments/ammyadmin.png?v=1", iconAlt: "АмиАдмин", displayName: "АмиАдмин", requirePassword: false },
    { parameterCode: "AC", clearbatType: "assistant", buttonText: "AC", iconPath: "/icon/equipments/assistant.png?v=1", iconAlt: "Ассистент", displayName: "Ассистент", requirePassword: false },
    { parameterCode: "rust", clearbatType: "rustdesk", buttonText: "RD", iconPath: "/icon/equipments/rustdesk.ico?v=1", iconAlt: "RustDesk", displayName: "Растдеск", requirePassword: false }
];
const IIKO_CREDENTIALS_PARAMETER_CODE = "0008";
const SERVER_ACCESS_BUTTONS = [
    { addressParameterCode: "srv_addr", iconPath: "/icon/equipments/iikoOffice_icon.ico?v=1", iconAlt: "RMS", displayName: "RMS" },
    { addressParameterCode: "0017", iconPath: "/icon/equipments/iikoChain_icon.ico?v=1", iconAlt: "Чейн", displayName: "Чейн" }
];

let antiForgeryToken = null;
let equipmentsState = createDefaultState();
let equipmentsListRequestId = 0;
let equipmentsExactCountRequestId = 0;
let equipmentsLookupStates = createLookupStates();
let equipmentsGridResizeCleanup = null;
let equipmentSearchWarningModal = null;

document.addEventListener("DOMContentLoaded", () => {
    initEquipmentsPage();
});

function createDefaultState() {
    return {
        page: 1,
        pageSize: 20,
        filters: createDefaultFilterState(),
        exactTotalCount: null,
        exactTotalPages: null,
        list: {
            items: [],
            page: 1,
            pageSize: 20,
            totalPages: 1,
            displayTotalCount: 0,
            isTotalCountCapped: false,
            hasNextPage: false
        }
    };
}

function createDefaultFilterState() {
    return {
        quickSearch: ""
    };
}

function createLookupStates() {
    return {
        company: createLookupState("CompanyLookup", "filterCompanySearch", "filterCompanyList", "filterCompanySelected"),
        maintenanceEntity: createLookupState("MaintenanceEntityLookup", "filterMaintenanceEntitySearch", "filterMaintenanceEntityList", "filterMaintenanceEntitySelected")
    };
}

function createLookupState(handler, searchId, listId, selectedId) {
    return {
        handler,
        searchId,
        listId,
        selectedId,
        items: [],
        selectedIds: new Set(),
        offset: 0,
        hasMore: true,
        loading: false,
        search: "",
        requestId: 0,
        debounceTimer: 0
    };
}

async function initEquipmentsPage() {
    antiForgeryToken = getRequestVerificationToken();
    initEquipmentSearchWarningModal();
    restoreEquipmentsFiltersState();
    bindEquipmentsEvents();
    restoreEquipmentsPageFromUrl();
    applyStateToFilters();
    initEquipmentsGridColumnResize();
    applyDefaultEquipmentsGridColumnWidths();
    restoreEquipmentsGridColumnWidths();
    await loadInitialLookups();
    await reloadEquipments(false);
}

function bindEquipmentsEvents() {
    const quickSearchInput = document.getElementById("filterQuickSearch");
    if (quickSearchInput) {
        quickSearchInput.addEventListener("keydown", async event => {
            if (event.key !== "Enter") {
                return;
            }

            event.preventDefault();
            const quickSearchValue = getTrimmedValue("filterQuickSearch");
            if (quickSearchValue && !isPositiveIntegerValue(quickSearchValue)) {
                showEquipmentSearchWarningModal();
                return;
            }

            equipmentsState.page = 1;
            equipmentsState.exactTotalCount = null;
            equipmentsState.exactTotalPages = null;
            syncEquipmentsFilterStateFromInputs();
            await reloadEquipments(true);
        });
    }

    const pageSize = document.getElementById("filterPageSize");
    if (pageSize) {
        pageSize.addEventListener("change", async () => {
            equipmentsState.page = 1;
            equipmentsState.exactTotalCount = null;
            equipmentsState.exactTotalPages = null;
            syncEquipmentsFilterStateFromInputs();
            saveEquipmentsFiltersState();
            await reloadEquipments(true);
        });
    }

    bindLookupEvents("company");
    bindLookupEvents("maintenanceEntity");

    const filtersCollapseElement = document.getElementById("equipmentFiltersCollapse");
    if (filtersCollapseElement) {
        filtersCollapseElement.addEventListener("shown.bs.collapse", () => {
            updateEquipmentFiltersToggleButton(true);
        });

        filtersCollapseElement.addEventListener("hidden.bs.collapse", () => {
            updateEquipmentFiltersToggleButton(false);
        });
    }

    const resetButton = document.getElementById("resetFiltersButton");
    if (resetButton) {
        resetButton.addEventListener("click", async () => {
            resetEquipmentsFilters();
            if (hasStoredEquipmentsGridColumnWidths()) {
                resetEquipmentsGridColumnWidths();
            }
            await loadInitialLookups();
            await reloadEquipments(true);
        });
    }
}

function bindLookupEvents(key) {
    const lookup = equipmentsLookupStates[key];
    const searchInput = document.getElementById(lookup.searchId);
    const list = document.getElementById(lookup.listId);
    const toggle = document.getElementById(`filter${capitalizeLookupKey(key)}Toggle`);

    if (searchInput) {
        searchInput.addEventListener("input", () => {
            if (lookup.debounceTimer) {
                window.clearTimeout(lookup.debounceTimer);
            }

            lookup.debounceTimer = window.setTimeout(async () => {
                lookup.search = normalizeLookupSearch(searchInput.value);
                saveEquipmentsFiltersState();
                await loadLookupOptions(key, true);
            }, 250);
        });
    }

    if (list) {
        list.addEventListener("scroll", async () => {
            if (lookup.loading || !lookup.hasMore) {
                return;
            }

            const threshold = 24;
            const isNearBottom = list.scrollTop + list.clientHeight >= list.scrollHeight - threshold;
            if (!isNearBottom) {
                return;
            }

            await loadLookupOptions(key, false);
        });
    }

    if (toggle) {
        toggle.addEventListener("shown.bs.dropdown", async () => {
            if (searchInput) {
                searchInput.focus();
                searchInput.select();
            }

            if (lookup.items.length === 0) {
                await loadLookupOptions(key, true);
            }
        });
    }
}

async function loadInitialLookups() {
    await Promise.all([
        loadLookupOptions("company", true),
        loadLookupOptions("maintenanceEntity", true)
    ]);
}

async function loadLookupOptions(key, reset) {
    const lookup = equipmentsLookupStates[key];
    if (!lookup || lookup.loading) {
        return;
    }

    if (reset) {
        lookup.offset = 0;
        lookup.items = [];
        lookup.hasMore = true;
    }

    if (!lookup.hasMore) {
        renderLookupList(key);
        return;
    }

    lookup.loading = true;
    const requestId = ++lookup.requestId;
    renderLookupList(key);

    try {
        const params = new URLSearchParams();
        params.append("offset", String(lookup.offset));
        params.append("limit", String(LOOKUP_PAGE_SIZE));

        if (lookup.search) {
            params.append("search", lookup.search);
        }

        const url = `?handler=${lookup.handler}&${params.toString()}`;
        const response = await sendJsonRequest(url, "GET", buildJsonHeaders(antiForgeryToken));

        if (requestId !== lookup.requestId) {
            return;
        }

        const newItems = Array.isArray(response) ? response : [];
        if (reset) {
            lookup.items = [];
        }

        for (const item of newItems) {
            if (!lookup.items.some(existing => existing.id === item.id)) {
                lookup.items.push(item);
            }
        }

        lookup.offset += newItems.length;
        lookup.hasMore = newItems.length === LOOKUP_PAGE_SIZE;
    } catch (error) {
        console.error(error);
        showPageError(error.message || "Не удалось загрузить элементы фильтра.");
    } finally {
        lookup.loading = false;
        renderLookupList(key);
    }
}

function renderLookupList(key) {
    const lookup = equipmentsLookupStates[key];
    const container = document.getElementById(lookup.listId);
    if (!container) {
        return;
    }

    container.textContent = "";

    if (lookup.items.length === 0) {
        const empty = document.createElement("div");
        empty.className = "px-2 py-2 small text-muted";
        empty.textContent = lookup.loading ? "Загрузка..." : "Ничего не найдено";
        container.appendChild(empty);
        updateLookupSelectedCounter(key);
        return;
    }

    for (const item of lookup.items) {
        const row = document.createElement("label");
        row.className = "d-flex align-items-start gap-2 px-2 py-2 border-bottom small";

        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.className = "form-check-input mt-1 flex-shrink-0";
        checkbox.checked = lookup.selectedIds.has(item.id);
        checkbox.addEventListener("change", async () => {
            if (checkbox.checked) {
                lookup.selectedIds.add(item.id);
            } else {
                lookup.selectedIds.delete(item.id);
            }

            updateLookupSelectedCounter(key);
            equipmentsState.page = 1;
            equipmentsState.exactTotalCount = null;
            equipmentsState.exactTotalPages = null;
            saveEquipmentsFiltersState();
            await reloadEquipments(true);
        });

        const content = document.createElement("span");
        content.className = "d-inline-flex align-items-start gap-2";

        if (key === "company") {
            const marker = document.createElement("span");
            marker.className = "rounded-circle flex-shrink-0 mt-1";
            marker.style.width = "0.65rem";
            marker.style.height = "0.65rem";
            marker.style.backgroundColor = normalizeHexColor(item.color) || "#6c757d";
            content.appendChild(marker);
        }

        const text = document.createElement("span");
        text.textContent = String(item.text || `#${item.id}`);

        row.appendChild(checkbox);
        content.appendChild(text);
        row.appendChild(content);
        container.appendChild(row);
    }

    if (lookup.loading) {
        const loading = document.createElement("div");
        loading.className = "px-2 py-2 small text-muted";
        loading.textContent = "Загрузка...";
        container.appendChild(loading);
    }

    updateLookupSelectedCounter(key);
}

function updateLookupSelectedCounter(key) {
    const lookup = equipmentsLookupStates[key];
    const counter = document.getElementById(lookup.selectedId);
    const toggleCount = document.getElementById(`filter${capitalizeLookupKey(key)}ToggleCount`);
    const toggleLabel = document.getElementById(`filter${capitalizeLookupKey(key)}ToggleLabel`);
    if (!counter) {
        return;
    }

    counter.textContent = `Выбрано: ${lookup.selectedIds.size}`;

    if (toggleCount) {
        toggleCount.textContent = String(lookup.selectedIds.size);
    }

    if (toggleLabel) {
        toggleLabel.textContent = getLookupToggleLabel(key, lookup.selectedIds.size);
    }
}

function normalizeLookupSearch(value) {
    const normalized = String(value || "").trim();
    const nonWhitespaceCount = normalized.replace(/\s+/g, "").length;
    return nonWhitespaceCount >= 2 ? normalized : "";
}

function applyStateToFilters() {
    const quickSearch = document.getElementById("filterQuickSearch");
    const pageSize = document.getElementById("filterPageSize");

    if (quickSearch) {
        quickSearch.value = equipmentsState.filters.quickSearch;
    }

    if (pageSize) {
        pageSize.value = String(equipmentsState.pageSize);
    }

    for (const key of Object.keys(equipmentsLookupStates)) {
        const lookup = equipmentsLookupStates[key];
        const searchInput = document.getElementById(lookup.searchId);
        if (searchInput) {
            searchInput.value = lookup.search;
        }

        updateLookupSelectedCounter(key);
    }

    updateEquipmentFiltersToggleButton(isEquipmentFiltersExpanded());
}

function resetEquipmentsFilters() {
    const ids = [
        "filterQuickSearch",
        "filterCompanySearch",
        "filterMaintenanceEntitySearch"
    ];

    for (const id of ids) {
        const input = document.getElementById(id);
        if (input) {
            input.value = "";
        }
    }

    for (const key of Object.keys(equipmentsLookupStates)) {
        const lookup = equipmentsLookupStates[key];
        lookup.selectedIds.clear();
        lookup.items = [];
        lookup.offset = 0;
        lookup.hasMore = true;
        lookup.search = "";
        updateLookupSelectedCounter(key);
        renderLookupList(key);
    }

    const pageSize = document.getElementById("filterPageSize");
    if (pageSize) {
        pageSize.value = "20";
    }

    equipmentsState.page = 1;
    equipmentsState.pageSize = 20;
    equipmentsState.filters = createDefaultFilterState();
    equipmentsState.exactTotalCount = null;
    equipmentsState.exactTotalPages = null;
    saveEquipmentsFiltersState();
}

async function reloadEquipments(resetPage) {
    const request = collectEquipmentsRequest();
    if (resetPage) {
        request.page = 1;
        equipmentsState.page = 1;
    }

    const requestId = ++equipmentsListRequestId;

    try {
        clearPageMessages();

        const url = buildEquipmentsRequestUrl("List", request);
        const response = await sendJsonRequest(url, "GET", buildJsonHeaders(antiForgeryToken));

        if (requestId !== equipmentsListRequestId) {
            return;
        }

        equipmentsState.page = request.page;
        equipmentsState.pageSize = request.pageSize;
        syncEquipmentsFilterStateFromInputs();
        saveEquipmentsFiltersState();
        equipmentsState.list = {
            items: ensureArray(response.items),
            page: response.page || request.page,
            pageSize: response.pageSize || request.pageSize,
            totalPages: response.totalPages || 1,
            displayTotalCount: response.displayTotalCount || 0,
            isTotalCountCapped: response.isTotalCountCapped === true,
            hasNextPage: response.hasNextPage === true
        };

        if (equipmentsState.exactTotalCount !== null && equipmentsState.exactTotalPages !== null) {
            equipmentsState.list.totalPages = equipmentsState.exactTotalPages;
        }

        updateEquipmentsPageUrl();
        renderEquipmentsTable();
        renderEquipmentsTotalCount();
        renderEquipmentsPagination();
    } catch (error) {
        console.error(error);
        showPageError(error.message || "Не удалось загрузить список оборудования.");
        renderEquipmentsTable();
        renderEquipmentsTotalCount();
        renderEquipmentsPagination();
    }
}

function collectEquipmentsRequest() {
    syncEquipmentsFilterStateFromInputs();
    const pageSize = Number(getTrimmedValue("filterPageSize") || "20");
    const equipmentId = parsePositiveIntegerOrNull(equipmentsState.filters.quickSearch);

    return {
        equipmentId,
        companyIds: Array.from(equipmentsLookupStates.company.selectedIds),
        maintenanceEntityIds: Array.from(equipmentsLookupStates.maintenanceEntity.selectedIds),
        page: equipmentsState.page,
        pageSize
    };
}

function buildEquipmentsRequestUrl(handler, request) {
    const params = new URLSearchParams();

    appendNumberParam(params, "equipmentId", request.equipmentId);
    appendListParam(params, "companyIds", request.companyIds);
    appendListParam(params, "maintenanceEntityIds", request.maintenanceEntityIds);
    appendNumberParam(params, "page", request.page);
    appendNumberParam(params, "pageSize", request.pageSize);

    return `?handler=${handler}&${params.toString()}`;
}

function appendNumberParam(params, key, value) {
    if (typeof value === "number" && !Number.isNaN(value)) {
        params.append(key, String(value));
    }
}

function appendListParam(params, key, values) {
    if (!Array.isArray(values) || values.length === 0) {
        return;
    }

    for (const value of values) {
        params.append(key, String(value));
    }
}

function renderEquipmentsTable() {
    const tbody = document.getElementById("equipmentsRows");
    if (!tbody) {
        return;
    }

    tbody.textContent = "";

    if (!Array.isArray(equipmentsState.list.items) || equipmentsState.list.items.length === 0) {
        const tr = document.createElement("tr");
        const td = document.createElement("td");
        td.colSpan = 7;
        td.className = "text-center text-muted py-4";
        td.textContent = "Оборудование не найдено";
        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }

    for (const item of equipmentsState.list.items) {
        const tr = document.createElement("tr");
        tr.className = "align-middle";
        tr.appendChild(buildCell(String(item.id || ""), 0, item.id));
        tr.appendChild(buildEquipmentInfoCell(item));
        tr.appendChild(buildCell(String(item.inventoryNumber || "Не указан"), 2));
        tr.appendChild(buildCell(String(item.serialNumber || "Не указан"), 3));
        tr.appendChild(buildCompanyCell(item, 4));
        tr.appendChild(buildCell(String(item.maintenanceEntityName || "Не указан"), 5));
        tr.appendChild(buildAccessesCell(item));
        tbody.appendChild(tr);
    }

    applyCurrentEquipmentsGridColumnWidths();
    applyIdColumnAutoWidth();
    applyEquipmentInfoColumnAutoWidth();

}

function renderEquipmentsTotalCount() {
    const container = document.getElementById("equipmentsTotalCount");
    if (!container) {
        return;
    }

    container.textContent = "";

    const label = document.createElement("span");
    label.textContent = "Всего оборудования: ";
    container.appendChild(label);

    if (equipmentsState.exactTotalCount !== null) {
        const value = document.createElement("span");
        value.textContent = String(equipmentsState.exactTotalCount);
        container.appendChild(value);
        return;
    }

    if (equipmentsState.list.isTotalCountCapped) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "btn btn-link p-0 align-baseline";
        button.style.fontSize = "inherit";
        button.style.lineHeight = "inherit";
        button.style.fontWeight = "inherit";
        button.style.textDecoration = "none";
        button.textContent = "1000+";
        button.addEventListener("click", async () => {
            await ensureExactCountLoaded();
        });
        container.appendChild(button);
        return;
    }

    const value = document.createElement("span");
    value.textContent = String(equipmentsState.list.displayTotalCount);
    container.appendChild(value);
}

function renderEquipmentsPagination() {
    const container = document.getElementById("equipmentsPagination");
    if (!container) {
        return;
    }

    container.textContent = "";

    const currentPage = equipmentsState.list.page || 1;
    const totalPages = getResolvedTotalPages();
    const hasNextPage = hasEquipmentsNextPage();

    if (currentPage > 1) {
        container.appendChild(createPageButton("<<", async () => {
            await goToEquipmentsPage(1);
        }, false));

        container.appendChild(createPageButton("<", async () => {
            await goToEquipmentsPage(currentPage - 1);
        }, false));
    }

    const range = getEquipmentsPageRange(currentPage, totalPages);
    for (const page of range) {
        container.appendChild(createPageButton(String(page), async () => {
            await goToEquipmentsPage(page);
        }, page === currentPage));
    }

    if (range.length > 0 && shouldShowTrailingDots(range[range.length - 1], totalPages, hasNextPage)) {
        const dots = document.createElement("span");
        dots.className = "px-2";
        dots.textContent = "...";
        container.appendChild(dots);
    }

    if (hasNextPage || currentPage < totalPages) {
        container.appendChild(createPageButton(">", async () => {
            await goToEquipmentsPage(currentPage + 1);
        }, false));

        container.appendChild(createPageButton(">>", async () => {
            await goToEquipmentsLastPage();
        }, false));
    }
}

function createPageButton(text, onClick, isActive) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = isActive ? "btn btn-primary" : "btn btn-outline-secondary";
    button.textContent = text;
    button.addEventListener("click", onClick);
    return button;
}

function getEquipmentsPageRange(currentPage, totalPages) {
    let start = 1;

    if (currentPage > 5) {
        start = currentPage - 2;
    }

    if (start + 4 > totalPages) {
        start = Math.max(1, totalPages - 4);
    }

    const end = Math.min(totalPages, start + 4);
    const pages = [];

    for (let page = start; page <= end; page += 1) {
        pages.push(page);
    }

    return pages;
}

function shouldShowTrailingDots(lastVisiblePage, totalPages, hasNextPage) {
    if (lastVisiblePage < totalPages) {
        return true;
    }

    return hasNextPage;
}

async function goToEquipmentsPage(page) {
    if (page <= 0) {
        return;
    }

    equipmentsState.page = page;
    await reloadEquipments(false);
}

async function goToEquipmentsLastPage() {
    if (equipmentsState.exactTotalPages === null && equipmentsState.list.isTotalCountCapped) {
        const loaded = await ensureExactCountLoaded();
        if (!loaded) {
            return;
        }
    }

    equipmentsState.page = getResolvedTotalPages();
    await reloadEquipments(false);
}

async function ensureExactCountLoaded() {
    if (equipmentsState.exactTotalCount !== null && equipmentsState.exactTotalPages !== null) {
        return true;
    }

    const requestId = ++equipmentsExactCountRequestId;

    try {
        const request = collectEquipmentsRequest();
        const url = buildEquipmentsRequestUrl("ExactCount", request);
        const response = await sendJsonRequest(url, "GET", buildJsonHeaders(antiForgeryToken));

        if (requestId !== equipmentsExactCountRequestId) {
            return false;
        }

        equipmentsState.exactTotalCount = response.totalCount || 0;
        equipmentsState.exactTotalPages = response.totalPages || 1;
        equipmentsState.list.totalPages = equipmentsState.exactTotalPages;

        renderEquipmentsTotalCount();
        renderEquipmentsPagination();
        return true;
    } catch (error) {
        console.error(error);
        showPageError(error.message || "Не удалось получить точное количество оборудования.");
        return false;
    }
}

function getResolvedTotalPages() {
    if (equipmentsState.exactTotalPages !== null) {
        return equipmentsState.exactTotalPages;
    }

    return equipmentsState.list.totalPages || 1;
}

function hasEquipmentsNextPage() {
    if (equipmentsState.exactTotalPages !== null) {
        return equipmentsState.list.page < equipmentsState.exactTotalPages;
    }

    return equipmentsState.list.hasNextPage === true || equipmentsState.list.page < equipmentsState.list.totalPages;
}

function buildCell(text, columnIndex, equipmentId = null) {
    const td = document.createElement("td");
    td.className = columnIndex === 0 ? "px-2 py-2" : "px-3 py-2";
    td.setAttribute("data-column-cell", String(columnIndex));

    const content = document.createElement("div");
    content.className = "text-truncate";
    content.style.width = "100%";
    content.textContent = text;
    td.appendChild(content);

    if (equipmentId !== null) {
        makeEquipmentCellNavigable(content, equipmentId);
    }

    return td;
}

function buildCompanyCell(item, columnIndex) {
    const td = document.createElement("td");
    td.className = "px-3 py-2";
    td.setAttribute("data-column-cell", String(columnIndex));

    const wrapper = document.createElement("div");
    wrapper.className = "d-flex align-items-center gap-2";
    wrapper.style.width = "100%";
    wrapper.style.minWidth = "0";

    const marker = document.createElement("span");
    marker.className = "rounded-circle flex-shrink-0";
    marker.style.width = "0.75rem";
    marker.style.height = "0.75rem";
    marker.style.backgroundColor = normalizeHexColor(item.companyCategoryColor) || "#6c757d";

    const text = document.createElement("div");
    text.className = "text-truncate";
    text.style.flexGrow = "1";
    text.style.minWidth = "0";
    text.style.width = "0";
    text.textContent = String(item.companyName || "Не указан");

    wrapper.appendChild(marker);
    wrapper.appendChild(text);
    td.appendChild(wrapper);

    return td;
}

function buildEquipmentInfoCell(item) {
    const typeName = String(item.typeName || "Не указан");
    const manufacturerName = String(item.manufacturerName || "Не указан");
    const modelName = String(item.modelName || "Не указан");
    const combinedText = `${typeName} ${manufacturerName} ${modelName}`;

    return buildCell(combinedText, 1, item.id);
}

function buildAccessesCell(item) {
    const td = document.createElement("td");
    td.className = "px-3 py-2";
    td.setAttribute("data-column-cell", String(AUTO_EXPANDING_COLUMN_INDEX));

    const wrapper = document.createElement("div");
    wrapper.className = "d-flex flex-wrap align-items-center gap-2";

    for (const accessButton of TERMINAL_ACCESS_BUTTONS) {
        const parameterValue = getEquipmentParameterValue(item, accessButton.parameterCode);
        const credentials = parseAccessCredentials(parameterValue, accessButton.requirePassword);
        if (credentials === null) {
            continue;
        }

        const button = document.createElement("button");
        button.type = "button";
        button.className = "btn btn-sm p-0 border-0 bg-transparent shadow-none d-inline-flex align-items-center justify-content-center overflow-hidden rounded-1";
        button.style.width = "31px";
        button.style.height = "31px";
        button.style.minWidth = "31px";
        button.style.minHeight = "31px";
        button.title = accessButton.displayName;
        button.setAttribute("aria-label", accessButton.iconAlt);
        button.setAttribute("data-bs-toggle", "tooltip");
        button.setAttribute("data-bs-placement", "top");

        const icon = document.createElement("img");
        icon.src = accessButton.iconPath;
        icon.alt = accessButton.iconAlt;
        icon.className = "d-block w-100 h-100";
        icon.style.objectFit = "fill";
        button.appendChild(icon);

        bindAccessButtonHoverState(button);
        button.addEventListener("click", () => {
            openClearbatLink(buildClearbatUrl(accessButton.clearbatType, credentials));
        });
        wrapper.appendChild(button);
    }

    const iikoCredentialsValue = getEquipmentParameterValue(item, IIKO_CREDENTIALS_PARAMETER_CODE);
    const iikoCredentials = parseAccessCredentials(iikoCredentialsValue, false);
    if (iikoCredentials !== null) {
        for (const accessButton of SERVER_ACCESS_BUTTONS) {
            const serverAccess = parseServerAccess(getEquipmentParameterValue(item, accessButton.addressParameterCode));
            if (serverAccess === null) {
                continue;
            }

            const button = buildAccessIconButton(accessButton);
            bindAccessButtonHoverState(button);
            button.addEventListener("click", () => {
                openClearbatLink(buildIikoClearbatUrl(serverAccess.address, {
                    login: iikoCredentials.login,
                    password: serverAccess.password || iikoCredentials.password
                }));
            });
            wrapper.appendChild(button);
        }
    }

    td.appendChild(wrapper);
    initializeAccessTooltips(td);
    return td;
}

function buildAccessIconButton(accessButton) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "btn btn-sm p-0 border-0 bg-transparent shadow-none d-inline-flex align-items-center justify-content-center overflow-hidden rounded-1";
    button.style.width = "31px";
    button.style.height = "31px";
    button.style.minWidth = "31px";
    button.style.minHeight = "31px";
    button.title = accessButton.displayName;
    button.setAttribute("aria-label", accessButton.iconAlt);
    button.setAttribute("data-bs-toggle", "tooltip");
    button.setAttribute("data-bs-placement", "top");

    const icon = document.createElement("img");
    icon.src = accessButton.iconPath;
    icon.alt = accessButton.iconAlt;
    icon.className = "d-block w-100 h-100";
    icon.style.objectFit = "fill";
    button.appendChild(icon);

    return button;
}

function bindAccessButtonHoverState(button) {
    const activate = () => {
        button.classList.add("rounded-1");
        button.style.outline = "2px solid var(--bs-primary)";
        button.style.outlineOffset = "1px";
    };

    const deactivate = () => {
        button.style.outline = "";
        button.style.outlineOffset = "";
    };

    button.addEventListener("mouseenter", activate);
    button.addEventListener("mouseleave", deactivate);
    button.addEventListener("focus", activate);
    button.addEventListener("blur", deactivate);
}

function initializeAccessTooltips(container) {
    if (!container || !window.bootstrap?.Tooltip) {
        return;
    }

    const tooltipElements = container.querySelectorAll('[data-bs-toggle="tooltip"]');
    for (const tooltipElement of tooltipElements) {
        new bootstrap.Tooltip(tooltipElement);
    }
}

function getEquipmentParameterValue(item, parameterCode) {
    if (!item || !Array.isArray(item.parameters) || !parameterCode) {
        return "";
    }

    const parameter = item.parameters.find(current =>
        typeof current?.code === "string"
        && current.code === parameterCode);

    return typeof parameter?.value === "string" ? parameter.value : "";
}

function parseServerAccess(value) {
    const normalizedValue = String(value || "").trim();
    if (!normalizedValue) {
        return null;
    }

    const compactValue = normalizedValue.replace(/\s+/g, "");
    const separatorIndex = compactValue.indexOf(ACCESS_VALUE_SEPARATOR);
    const rawAddress = separatorIndex >= 0
        ? compactValue.slice(0, separatorIndex)
        : compactValue;
    const password = separatorIndex >= 0
        ? compactValue.slice(separatorIndex + ACCESS_VALUE_SEPARATOR.length)
        : "";

    if (!rawAddress) {
        return null;
    }

    const address = ensureServerPort(rawAddress);
    if (!address) {
        return null;
    }

    return {
        address,
        password
    };
}

function ensureServerPort(address) {
    const normalizedAddress = String(address || "").trim();
    if (!normalizedAddress) {
        return "";
    }

    if (hasExplicitPort(normalizedAddress)) {
        return normalizedAddress;
    }

    return `${normalizedAddress}:443`;
}

function hasExplicitPort(address) {
    if (!address) {
        return false;
    }

    const lastColonIndex = address.lastIndexOf(":");
    if (lastColonIndex < 0 || lastColonIndex === address.length - 1) {
        return false;
    }

    const portPart = address.slice(lastColonIndex + 1);
    return /^\d+$/.test(portPart);
}

function parseAccessCredentials(value, requirePassword) {
    const normalizedValue = String(value || "").replace(/\s+/g, "");
    if (!normalizedValue) {
        return null;
    }

    const separatorIndex = normalizedValue.indexOf(ACCESS_VALUE_SEPARATOR);
    if (separatorIndex < 0) {
        return requirePassword
            ? null
            : {
                login: normalizedValue,
                password: ""
            };
    }

    const login = normalizedValue.slice(0, separatorIndex);
    const password = normalizedValue.slice(separatorIndex + ACCESS_VALUE_SEPARATOR.length);
    if (login.length === 0) {
        return null;
    }

    if (requirePassword && password.length === 0) {
        return null;
    }

    return {
        login,
        password
    };
}

function buildClearbatUrl(clearbatType, credentials) {
    const payloadParts = [
        `?type=${clearbatType}`,
        `?login=${credentials.login}`
    ];

    if (credentials.password) {
        payloadParts.push(`?password=${credentials.password}`);
    }

    const payload = payloadParts.join("");
    return `clearbat:/${toBase64Utf8(payload)}?encode=full`;
}

function buildIikoClearbatUrl(address, credentials) {
    const payloadParts = [
        "?type=iiko",
        `?url=${address}`
    ];

    if (credentials.login) {
        payloadParts.push(`?login=${credentials.login}`);
    }

    if (credentials.password) {
        payloadParts.push(`?password=${credentials.password}`);
    }

    const payload = payloadParts.join("");
    return `clearbat:/${toBase64Utf8(payload)}?encode=full`;
}

function toBase64Utf8(value) {
    const bytes = new TextEncoder().encode(String(value || ""));
    let binary = "";

    for (const byte of bytes) {
        binary += String.fromCharCode(byte);
    }

    return window.btoa(binary);
}

function openClearbatLink(url) {
    if (!url) {
        return;
    }

    window.location.assign(url);
}

function makeEquipmentCellNavigable(element, equipmentId) {
    element.style.cursor = "pointer";
    element.tabIndex = 0;
    element.classList.add("rounded-1");
    element.addEventListener("mouseenter", () => {
        element.classList.add("text-primary");
    });
    element.addEventListener("mouseleave", () => {
        element.classList.remove("text-primary");
    });
    element.addEventListener("focus", () => {
        element.classList.add("text-primary");
    });
    element.addEventListener("blur", () => {
        element.classList.remove("text-primary");
    });
    element.addEventListener("click", () => {
        openEquipmentDetails(equipmentId);
    });
    element.addEventListener("keydown", event => {
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            openEquipmentDetails(equipmentId);
        }
    });
}

function openEquipmentDetails(id) {
    if (!Number.isInteger(id) || id <= 0) {
        return;
    }

    const page = Math.max(1, Number.parseInt(String(equipmentsState.page || 1), 10) || 1);
    window.location.assign(`/equipments/${id}?page=${page}`);
}

function ensureArray(value) {
    return Array.isArray(value) ? value : [];
}

function getTrimmedValue(id) {
    const element = document.getElementById(id);
    if (!element) {
        return "";
    }

    return String(element.value || "").trim();
}

function parsePositiveIntegerOrNull(value) {
    if (!isPositiveIntegerValue(value)) {
        return null;
    }

    return Number.parseInt(String(value), 10);
}

function isPositiveIntegerValue(value) {
    return /^[1-9]\d*$/.test(String(value || "").trim());
}

function clearPageMessages() {
    hideMessage("pageError");
    hideMessage("pageSuccess");
}

function showPageError(message) {
    showMessage("pageError", message);
}

function showMessage(id, message) {
    const element = document.getElementById(id);
    if (!element) {
        return;
    }

    element.textContent = message;
    element.classList.remove("d-none");
}

function hideMessage(id) {
    const element = document.getElementById(id);
    if (!element) {
        return;
    }

    element.textContent = "";
    element.classList.add("d-none");
}

function updateEquipmentFiltersToggleButton(isExpanded) {
    const button = document.getElementById("toggleEquipmentFiltersButton");
    if (!button) {
        return;
    }

    button.textContent = isExpanded ? "Свернуть фильтры" : "Развернуть фильтры";
    button.setAttribute("aria-expanded", isExpanded ? "true" : "false");
}

function isEquipmentFiltersExpanded() {
    const collapse = document.getElementById("equipmentFiltersCollapse");
    return collapse ? collapse.classList.contains("show") : false;
}

function initEquipmentSearchWarningModal() {
    const modalElement = document.getElementById("equipmentSearchWarningModal");
    if (!modalElement || !window.bootstrap) {
        return;
    }

    equipmentSearchWarningModal = new bootstrap.Modal(modalElement);
}

function showEquipmentSearchWarningModal() {
    if (equipmentSearchWarningModal) {
        equipmentSearchWarningModal.show();
    }
}

function restoreEquipmentsFiltersState() {
    equipmentsState.filters = createDefaultFilterState();

    const raw = localStorage.getItem(EQUIPMENTS_FILTERS_STORAGE_KEY);
    if (!raw) {
        return;
    }

    try {
        const parsed = JSON.parse(raw);
        equipmentsState.pageSize = isAllowedPageSize(parsed.pageSize) ? parsed.pageSize : 20;
        equipmentsState.filters.quickSearch = parsed.quickSearch ? String(parsed.quickSearch) : "";

        restoreLookupSelection("company", parsed.companyIds, parsed.companySearch);
        restoreLookupSelection("maintenanceEntity", parsed.maintenanceEntityIds, parsed.maintenanceEntitySearch);
    } catch (error) {
        console.error(error);
        localStorage.removeItem(EQUIPMENTS_FILTERS_STORAGE_KEY);
    }
}

function restoreLookupSelection(key, ids, search) {
    const lookup = equipmentsLookupStates[key];
    lookup.selectedIds = new Set(Array.isArray(ids) ? ids.filter(id => Number.isInteger(id) && id > 0) : []);
    lookup.search = typeof search === "string" ? normalizeLookupSearch(search) : "";
}

function saveEquipmentsFiltersState() {
    syncEquipmentsFilterStateFromInputs();

    const payload = {
        pageSize: equipmentsState.pageSize,
        quickSearch: equipmentsState.filters.quickSearch,
        companyIds: Array.from(equipmentsLookupStates.company.selectedIds),
        maintenanceEntityIds: Array.from(equipmentsLookupStates.maintenanceEntity.selectedIds),
        companySearch: equipmentsLookupStates.company.search,
        maintenanceEntitySearch: equipmentsLookupStates.maintenanceEntity.search
    };

    localStorage.setItem(EQUIPMENTS_FILTERS_STORAGE_KEY, JSON.stringify(payload));
}

function syncEquipmentsFilterStateFromInputs() {
    if (!equipmentsState.filters) {
        equipmentsState.filters = createDefaultFilterState();
    }

    equipmentsState.filters.quickSearch = getTrimmedValue("filterQuickSearch");
    equipmentsState.pageSize = Number(getTrimmedValue("filterPageSize") || "20");
}

function isAllowedPageSize(value) {
    return value === 20 || value === 50 || value === 100;
}

function capitalizeLookupKey(key) {
    return key.charAt(0).toUpperCase() + key.slice(1);
}

function getLookupToggleLabel(key, count) {
    const baseLabels = {
        company: "Клиент",
        maintenanceEntity: "Объект обслуживания"
    };

    if (count <= 0) {
        return baseLabels[key] || "";
    }

    return `${baseLabels[key]} (${count})`;
}

function normalizeHexColor(value) {
    const normalizedValue = String(value || "").trim().toUpperCase();
    return /^#([0-9A-F]{6})$/.test(normalizedValue) ? normalizedValue : "";
}

function initEquipmentsGridColumnResize() {
    if (typeof equipmentsGridResizeCleanup === "function") {
        equipmentsGridResizeCleanup();
    }

    const handlers = [];
    const resizers = document.querySelectorAll("[data-column-resizer]");
    for (const resizer of resizers) {
        const onMouseDown = event => {
            event.preventDefault();

            const columnIndex = Number(resizer.getAttribute("data-column-resizer"));
            if (Number.isNaN(columnIndex)) {
                return;
            }

            const col = document.getElementById(`equipmentsCol${columnIndex}`);
            if (!col) {
                return;
            }

            syncEquipmentsGridColumnWidths();
            fixEquipmentsGridWidth();

            const startX = event.clientX;
            const startWidth = col.getBoundingClientRect().width;
            const minColumnWidth = getEquipmentsColumnMinWidth(columnIndex);
            const minDelta = minColumnWidth - startWidth;

            const onMouseMove = moveEvent => {
                const delta = moveEvent.clientX - startX;
                const clampedDelta = Math.max(delta, minDelta);
                const currentWidth = startWidth + clampedDelta;

                setEquipmentsGridColumnWidth(columnIndex, currentWidth);
                fixEquipmentsGridWidth();
            };

            const onMouseUp = () => {
                document.removeEventListener("mousemove", onMouseMove);
                document.removeEventListener("mouseup", onMouseUp);
                saveEquipmentsGridColumnWidths();
            };

            document.addEventListener("mousemove", onMouseMove);
            document.addEventListener("mouseup", onMouseUp);
        };

        resizer.addEventListener("mousedown", onMouseDown);
        handlers.push(() => resizer.removeEventListener("mousedown", onMouseDown));
    }

    equipmentsGridResizeCleanup = () => {
        for (const dispose of handlers) {
            dispose();
        }
    };
}

function restoreEquipmentsPageFromUrl() {
    const params = new URLSearchParams(window.location.search);
    const page = Number.parseInt(params.get("page") || "", 10);
    if (Number.isInteger(page) && page > 0) {
        equipmentsState.page = page;
    }
}

function updateEquipmentsPageUrl() {
    const url = new URL(window.location.href);
    url.searchParams.set("page", String(equipmentsState.page || 1));
    window.history.replaceState(null, "", `${url.pathname}${url.search}`);
}

function syncEquipmentsGridColumnWidths() {
    const table = document.getElementById("equipmentsGrid");
    if (!table) {
        return;
    }

    const headerCells = table.querySelectorAll("thead th[data-column-index]");
    for (const headerCell of headerCells) {
        const columnIndex = Number(headerCell.getAttribute("data-column-index"));
        if (Number.isNaN(columnIndex)) {
            continue;
        }

        const col = document.getElementById(`equipmentsCol${columnIndex}`);
        if (!col) {
            continue;
        }

        setEquipmentsGridColumnWidth(columnIndex, headerCell.getBoundingClientRect().width);
    }

    fixEquipmentsGridWidth();
}

function fixEquipmentsGridWidth() {
    const table = document.getElementById("equipmentsGrid");
    if (!table) {
        return;
    }

    table.style.tableLayout = "fixed";
    const wrap = table.closest(".table-responsive");
    const wrapWidth = wrap ? wrap.clientWidth : 0;

    let totalWidth = 0;

    for (let index = 0; index <= GRID_LAST_COLUMN_INDEX; index += 1) {
        if (index === AUTO_EXPANDING_COLUMN_INDEX) {
            continue;
        }

        const col = document.getElementById(`equipmentsCol${index}`);
        if (!col) {
            continue;
        }

        let width = Number.parseFloat(col.style.width || "0");
        if (!Number.isFinite(width) || width <= 0) {
            const headerCell = document.querySelector(`thead th[data-column-index="${index}"]`);
            width = headerCell ? headerCell.getBoundingClientRect().width : 0;
        }

        if (!Number.isFinite(width) || width <= 0) {
            totalWidth = 0;
            break;
        }

        totalWidth += width;
    }

    if (totalWidth <= 0) {
        const measuredWidth = table.getBoundingClientRect().width;
        if (!Number.isFinite(measuredWidth) || measuredWidth <= 0) {
            return;
        }

        const fallbackWidth = Math.max(measuredWidth, wrapWidth);
        table.style.width = `${fallbackWidth}px`;
        table.style.minWidth = `${fallbackWidth}px`;
        return;
    }

    const tableWidth = Math.max(totalWidth + AUTO_EXPANDING_COLUMN_MIN_WIDTH, wrapWidth);
    table.style.width = `${tableWidth}px`;
    table.style.minWidth = `${tableWidth}px`;
}

function setEquipmentsGridColumnWidth(columnIndex, width) {
    if (!Number.isFinite(width) || width <= 0) {
        return;
    }

    const minWidth = getEquipmentsColumnMinWidth(columnIndex);
    const maxWidth = getEquipmentsColumnMaxWidth(columnIndex);
    const normalizedWidth = maxWidth === null
        ? Math.max(width, minWidth)
        : Math.min(Math.max(width, minWidth), maxWidth);
    const widthValue = `${normalizedWidth}px`;
    const col = document.getElementById(`equipmentsCol${columnIndex}`);
    if (col) {
        col.style.width = widthValue;
        col.style.minWidth = widthValue;
        col.style.maxWidth = widthValue;
    }

    const headerCell = document.querySelector(`thead th[data-column-index="${columnIndex}"]`);
    if (headerCell) {
        headerCell.style.width = widthValue;
        headerCell.style.minWidth = widthValue;
        headerCell.style.maxWidth = widthValue;
    }

    const cells = document.querySelectorAll(`[data-column-cell="${columnIndex}"]`);
    for (const cell of cells) {
        cell.style.width = widthValue;
        cell.style.minWidth = widthValue;
        cell.style.maxWidth = widthValue;
    }
}

function applyCurrentEquipmentsGridColumnWidths() {
    for (let index = 0; index <= LAST_RESIZABLE_COLUMN_INDEX; index += 1) {
        const col = document.getElementById(`equipmentsCol${index}`);
        if (!col) {
            continue;
        }

        const width = Number.parseFloat(col.style.width || "0");
        if (!Number.isFinite(width) || width <= 0) {
            continue;
        }

        setEquipmentsGridColumnWidth(index, width);
    }
}

function applyIdColumnAutoWidth() {
    const minWidth = getEquipmentsColumnMinWidth(0);
    const maxWidthLimit = getEquipmentsColumnMaxWidth(0);
    let maxWidth = minWidth;
    const contents = document.querySelectorAll('[data-column-cell="0"] .text-truncate');

    for (const content of contents) {
        const cell = content.closest('[data-column-cell="0"]');
        const measuredWidth = Math.ceil(
            measureEquipmentsTextWidth(content.textContent || "", content)
            + getEquipmentsHorizontalInsets(content)
            + getEquipmentsHorizontalInsets(cell)
        );
        if (Number.isFinite(measuredWidth) && measuredWidth > maxWidth) {
            maxWidth = measuredWidth;
        }
    }

    if (maxWidthLimit !== null) {
        maxWidth = Math.min(maxWidth, maxWidthLimit);
    }

    setEquipmentsGridColumnWidth(0, maxWidth);
    fixEquipmentsGridWidth();
}

function measureEquipmentsTextWidth(text, sourceElement) {
    const probe = document.createElement("span");
    const computedStyle = window.getComputedStyle(sourceElement);
    probe.textContent = text;
    probe.style.position = "absolute";
    probe.style.visibility = "hidden";
    probe.style.pointerEvents = "none";
    probe.style.whiteSpace = "nowrap";
    probe.style.font = computedStyle.font;
    probe.style.fontSize = computedStyle.fontSize;
    probe.style.fontWeight = computedStyle.fontWeight;
    probe.style.fontFamily = computedStyle.fontFamily;
    probe.style.letterSpacing = computedStyle.letterSpacing;
    document.body.appendChild(probe);

    const measuredWidth = probe.getBoundingClientRect().width;
    probe.remove();
    return measuredWidth;
}

function getEquipmentsHorizontalInsets(element) {
    if (!element) {
        return 0;
    }

    const computedStyle = window.getComputedStyle(element);
    const paddingLeft = Number.parseFloat(computedStyle.paddingLeft || "0");
    const paddingRight = Number.parseFloat(computedStyle.paddingRight || "0");
    const borderLeft = Number.parseFloat(computedStyle.borderLeftWidth || "0");
    const borderRight = Number.parseFloat(computedStyle.borderRightWidth || "0");
    return paddingLeft + paddingRight + borderLeft + borderRight;
}

function applyEquipmentInfoColumnAutoWidth() {
    const minWidth = getEquipmentsColumnMinWidth(1);
    let maxWidth = minWidth;
    const contents = document.querySelectorAll('[data-column-cell="1"] .text-truncate');

    for (const content of contents) {
        const measuredWidth = Math.ceil(content.scrollWidth + 32);
        if (Number.isFinite(measuredWidth) && measuredWidth > maxWidth) {
            maxWidth = measuredWidth;
        }
    }

    setEquipmentsGridColumnWidth(1, maxWidth);
    fixEquipmentsGridWidth();
}

function getEquipmentsColumnMinWidth(columnIndex) {
    if (columnIndex === 0) {
        return ID_COLUMN_MIN_WIDTH;
    }

    if (columnIndex === 1) {
        return EQUIPMENT_INFO_COLUMN_WIDTH;
    }

    if (columnIndex === 2 || columnIndex === 3) {
        return INVENTORY_AND_SERIAL_COLUMN_WIDTH;
    }

    if (columnIndex === 4 || columnIndex === 5) {
        return COMPANY_AND_OBJECT_COLUMN_WIDTH;
    }

    return DEFAULT_COLUMN_MIN_WIDTH;
}

function getEquipmentsColumnMaxWidth(columnIndex) {
    if (columnIndex === 0) {
        return ID_COLUMN_MAX_WIDTH;
    }

    return null;
}

function getEquipmentsColumnDefaultWidth(columnIndex) {
    if (columnIndex === 1) {
        return EQUIPMENT_INFO_COLUMN_WIDTH;
    }

    if (columnIndex === 2 || columnIndex === 3) {
        return INVENTORY_AND_SERIAL_COLUMN_WIDTH;
    }

    if (columnIndex === 4 || columnIndex === 5) {
        return COMPANY_AND_OBJECT_COLUMN_WIDTH;
    }

    return null;
}

function applyDefaultEquipmentsGridColumnWidths() {
    for (let index = 0; index <= GRID_LAST_COLUMN_INDEX; index += 1) {
        const defaultWidth = getEquipmentsColumnDefaultWidth(index);
        if (defaultWidth === null) {
            continue;
        }

        setEquipmentsGridColumnWidth(index, defaultWidth);
    }

    fixEquipmentsGridWidth();
}

function restoreEquipmentsGridColumnWidths() {
    const raw = localStorage.getItem(EQUIPMENTS_GRID_COLUMNS_STORAGE_KEY);
    if (!raw) {
        return;
    }

    try {
        const parsed = JSON.parse(raw);
        if (!Array.isArray(parsed)) {
            return;
        }

        for (const item of parsed) {
            if (!item || !Number.isInteger(item.index) || typeof item.width !== "number") {
                continue;
            }

            if (item.width < getEquipmentsColumnMinWidth(item.index)) {
                continue;
            }

            const col = document.getElementById(`equipmentsCol${item.index}`);
            if (!col) {
                continue;
            }

            setEquipmentsGridColumnWidth(item.index, item.width);
        }

        fixEquipmentsGridWidth();
    } catch (error) {
        console.error(error);
        localStorage.removeItem(EQUIPMENTS_GRID_COLUMNS_STORAGE_KEY);
    }
}

function saveEquipmentsGridColumnWidths() {
    const widths = [];

    for (let index = 0; index <= LAST_RESIZABLE_COLUMN_INDEX; index += 1) {
        const col = document.getElementById(`equipmentsCol${index}`);
        if (!col) {
            continue;
        }

        const width = Number.parseFloat(col.style.width || "0");
        if (!Number.isFinite(width) || width <= 0) {
            continue;
        }

        widths.push({
            index,
            width
        });
    }

    localStorage.setItem(EQUIPMENTS_GRID_COLUMNS_STORAGE_KEY, JSON.stringify(widths));
}

function resetEquipmentsGridColumnWidths() {
    localStorage.removeItem(EQUIPMENTS_GRID_COLUMNS_STORAGE_KEY);

    const table = document.getElementById("equipmentsGrid");
    if (table) {
        table.style.width = "";
        table.style.minWidth = "";
        table.style.tableLayout = "";
    }

    for (let index = 0; index <= GRID_LAST_COLUMN_INDEX; index += 1) {
        const col = document.getElementById(`equipmentsCol${index}`);
        if (col) {
            col.style.width = "";
            col.style.minWidth = "";
            col.style.maxWidth = "";
        }

        const headerCell = document.querySelector(`thead th[data-column-index="${index}"]`);
        if (headerCell) {
            headerCell.style.width = "";
            headerCell.style.minWidth = "";
            headerCell.style.maxWidth = "";
        }

        const cells = document.querySelectorAll(`[data-column-cell="${index}"]`);
        for (const cell of cells) {
            cell.style.width = "";
            cell.style.minWidth = "";
            cell.style.maxWidth = "";
        }
    }

    applyDefaultEquipmentsGridColumnWidths();
}

function hasStoredEquipmentsGridColumnWidths() {
    return localStorage.getItem(EQUIPMENTS_GRID_COLUMNS_STORAGE_KEY) !== null;
}
