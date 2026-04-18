const LOOKUP_PAGE_SIZE = 20;
const ISSUES_FILTERS_STORAGE_KEY = "crm_issues_filters_v2";
const ISSUES_GRID_COLUMNS_STORAGE_KEY = "crm_issues_grid_columns_v1";

let antiForgeryToken = null;
let issuesState = createDefaultState();
let issuesListRequestId = 0;
let issuesExactCountRequestId = 0;
let issuesFilterDebounceTimer = 0;
let issuesLookupStates = createLookupStates();
let issuesGridResizeCleanup = null;
let issueSearchWarningModal = null;

document.addEventListener("DOMContentLoaded", () => {
    initIssuesPage();
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

function createLookupStates() {
    return {
        assignee: createLookupState("AssigneeLookup", "filterAssigneeSearch", "filterAssigneeList", "filterAssigneeSelected"),
        author: createLookupState("AuthorLookup", "filterAuthorSearch", "filterAuthorList", "filterAuthorSelected"),
        type: createLookupState("TypeLookup", "filterTypeSearch", "filterTypeList", "filterTypeSelected"),
        status: createLookupState("StatusLookup", "filterStatusSearch", "filterStatusList", "filterStatusSelected"),
        priority: createLookupState("PriorityLookup", "filterPrioritySearch", "filterPriorityList", "filterPrioritySelected"),
        company: createLookupState("CompanyLookup", "filterCompanySearch", "filterCompanyList", "filterCompanySelected"),
        group: createLookupState("GroupLookup", "filterGroupSearch", "filterGroupList", "filterGroupSelected")
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

async function initIssuesPage() {
    antiForgeryToken = getRequestVerificationToken();
    initIssueSearchWarningModal();
    restoreIssuesFiltersState();
    bindIssuesEvents();
    applyStateToFilters();
    initIssuesGridColumnResize();
    restoreIssuesGridColumnWidths();
    await loadInitialLookups();
    await reloadIssues(true);
}

function bindIssuesEvents() {
    const quickSearchInput = document.getElementById("filterQuickSearch");
    if (quickSearchInput) {
        quickSearchInput.addEventListener("keydown", async event => {
            if (event.key !== "Enter") {
                return;
            }

            event.preventDefault();
            const quickSearchValue = getTrimmedValue("filterQuickSearch");
            if (quickSearchValue && getNonWhitespaceCount(quickSearchValue) < 2) {
                showIssueSearchWarningModal();
                return;
            }

            issuesState.page = 1;
            issuesState.exactTotalCount = null;
            issuesState.exactTotalPages = null;
            syncIssuesFilterStateFromInputs();
            await reloadIssues(true);
        });
    }

    const numberInputIds = [
        "filterIssueNumberFrom",
        "filterIssueNumberTo"
    ];

    for (const id of numberInputIds) {
        const input = document.getElementById(id);
        if (!input) {
            continue;
        }

        input.addEventListener("input", () => {
            scheduleIssuesReload();
        });
    }

    const pageSize = document.getElementById("filterPageSize");
    if (pageSize) {
        pageSize.addEventListener("change", async () => {
            issuesState.page = 1;
            issuesState.exactTotalCount = null;
            issuesState.exactTotalPages = null;
            syncIssuesFilterStateFromInputs();
            saveIssuesFiltersState();
            await reloadIssues(true);
        });
    }

    const dateIds = [
        "filterRegistrationDateFrom",
        "filterRegistrationDateTo",
        "filterResolutionDateFrom",
        "filterResolutionDateTo"
    ];

    for (const id of dateIds) {
        const element = document.getElementById(id);
        if (!element) {
            continue;
        }

        element.addEventListener("change", async () => {
            issuesState.page = 1;
            issuesState.exactTotalCount = null;
            issuesState.exactTotalPages = null;
            syncIssuesFilterStateFromInputs();
            saveIssuesFiltersState();
            await reloadIssues(true);
        });
    }

    bindLookupEvents("assignee");
    bindLookupEvents("author");
    bindLookupEvents("type");
    bindLookupEvents("status");
    bindLookupEvents("priority");
    bindLookupEvents("company");
    bindLookupEvents("group");

    const filtersCollapseElement = document.getElementById("issueFiltersCollapse");
    if (filtersCollapseElement) {
        filtersCollapseElement.addEventListener("shown.bs.collapse", () => {
            updateIssueFiltersToggleButton(true);
        });

        filtersCollapseElement.addEventListener("hidden.bs.collapse", () => {
            updateIssueFiltersToggleButton(false);
        });
    }

    const resetButton = document.getElementById("resetFiltersButton");
    if (resetButton) {
        resetButton.addEventListener("click", async () => {
            resetIssuesFilters();
            await loadInitialLookups();
            await reloadIssues(true);
        });
    }
}

function bindLookupEvents(key) {
    const lookup = issuesLookupStates[key];
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
                saveIssuesFiltersState();
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
        loadLookupOptions("assignee", true),
        loadLookupOptions("author", true),
        loadLookupOptions("type", true),
        loadLookupOptions("status", true),
        loadLookupOptions("priority", true),
        loadLookupOptions("company", true),
        loadLookupOptions("group", true)
    ]);
}

async function loadLookupOptions(key, reset) {
    const lookup = issuesLookupStates[key];
    if (!lookup) {
        return;
    }

    if (lookup.loading) {
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
    const lookup = issuesLookupStates[key];
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
        checkbox.className = "form-check-input mt-1";
        checkbox.checked = lookup.selectedIds.has(item.id);
        checkbox.addEventListener("change", async () => {
            if (checkbox.checked) {
                lookup.selectedIds.add(item.id);
            } else {
                lookup.selectedIds.delete(item.id);
            }

            updateLookupSelectedCounter(key);
            issuesState.page = 1;
            issuesState.exactTotalCount = null;
            issuesState.exactTotalPages = null;
            saveIssuesFiltersState();
            await reloadIssues(true);
        });

        const text = document.createElement("span");
        text.textContent = String(item.text || `#${item.id}`);

        row.appendChild(checkbox);
        row.appendChild(text);
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
    const lookup = issuesLookupStates[key];
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
    const issueNumberFrom = document.getElementById("filterIssueNumberFrom");
    const issueNumberTo = document.getElementById("filterIssueNumberTo");
    const pageSize = document.getElementById("filterPageSize");
    const registrationDateFrom = document.getElementById("filterRegistrationDateFrom");
    const registrationDateTo = document.getElementById("filterRegistrationDateTo");
    const resolutionDateFrom = document.getElementById("filterResolutionDateFrom");
    const resolutionDateTo = document.getElementById("filterResolutionDateTo");

    if (quickSearch) {
        quickSearch.value = issuesState.filters.quickSearch;
    }

    if (issueNumberFrom) {
        issueNumberFrom.value = issuesState.filters.issueNumberFrom;
    }

    if (issueNumberTo) {
        issueNumberTo.value = issuesState.filters.issueNumberTo;
    }

    if (pageSize) {
        pageSize.value = String(issuesState.pageSize);
    }

    if (registrationDateFrom) {
        registrationDateFrom.value = issuesState.filters.registrationDateFrom;
    }

    if (registrationDateTo) {
        registrationDateTo.value = issuesState.filters.registrationDateTo;
    }

    if (resolutionDateFrom) {
        resolutionDateFrom.value = issuesState.filters.resolutionDateFrom;
    }

    if (resolutionDateTo) {
        resolutionDateTo.value = issuesState.filters.resolutionDateTo;
    }

    for (const key of Object.keys(issuesLookupStates)) {
        const lookup = issuesLookupStates[key];
        const searchInput = document.getElementById(lookup.searchId);
        if (searchInput) {
            searchInput.value = lookup.search;
        }

        updateLookupSelectedCounter(key);
    }

    updateIssueFiltersToggleButton(isIssueFiltersExpanded());
}

function resetIssuesFilters() {
    const ids = [
        "filterIssueNumberFrom",
        "filterIssueNumberTo",
        "filterQuickSearch",
        "filterRegistrationDateFrom",
        "filterRegistrationDateTo",
        "filterResolutionDateFrom",
        "filterResolutionDateTo",
        "filterAssigneeSearch",
        "filterAuthorSearch",
        "filterTypeSearch",
        "filterStatusSearch",
        "filterPrioritySearch",
        "filterCompanySearch",
        "filterGroupSearch"
    ];

    for (const id of ids) {
        const input = document.getElementById(id);
        if (input) {
            input.value = "";
        }
    }

    for (const key of Object.keys(issuesLookupStates)) {
        const lookup = issuesLookupStates[key];
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

    issuesState.page = 1;
    issuesState.pageSize = 20;
    issuesState.filters = createDefaultFilterState();
    issuesState.exactTotalCount = null;
    issuesState.exactTotalPages = null;
    saveIssuesFiltersState();
}

function scheduleIssuesReload() {
    if (issuesFilterDebounceTimer) {
        window.clearTimeout(issuesFilterDebounceTimer);
    }

    issuesFilterDebounceTimer = window.setTimeout(async () => {
        issuesState.page = 1;
        issuesState.exactTotalCount = null;
        issuesState.exactTotalPages = null;
        syncIssuesFilterStateFromInputs();
        saveIssuesFiltersState();
        await reloadIssues(true);
    }, 350);
}

async function reloadIssues(resetPage) {
    const request = collectIssuesRequest();
    if (resetPage) {
        request.page = 1;
        issuesState.page = 1;
    }

    const requestId = ++issuesListRequestId;

    try {
        clearPageMessages();

        const url = buildIssuesRequestUrl("List", request);
        const response = await sendJsonRequest(url, "GET", buildJsonHeaders(antiForgeryToken));

        if (requestId !== issuesListRequestId) {
            return;
        }

        issuesState.page = request.page;
        issuesState.pageSize = request.pageSize;
        syncIssuesFilterStateFromInputs();
        saveIssuesFiltersState();
        issuesState.list = {
            items: ensureArray(response.items),
            page: response.page || request.page,
            pageSize: response.pageSize || request.pageSize,
            totalPages: response.totalPages || 1,
            displayTotalCount: response.displayTotalCount || 0,
            isTotalCountCapped: response.isTotalCountCapped === true,
            hasNextPage: response.hasNextPage === true
        };

        if (issuesState.exactTotalCount !== null && issuesState.exactTotalPages !== null) {
            issuesState.list.totalPages = issuesState.exactTotalPages;
        }

        renderIssuesTable();
        renderIssuesTotalCount();
        renderIssuesPagination();
    } catch (error) {
        console.error(error);
        showPageError(error.message || "Не удалось загрузить список заявок.");
        renderIssuesTable();
        renderIssuesTotalCount();
        renderIssuesPagination();
    }
}

function collectIssuesRequest() {
    syncIssuesFilterStateFromInputs();
    const pageSize = Number(getTrimmedValue("filterPageSize") || "20");

    return {
        numberFrom: issuesState.filters.issueNumberFrom ? Number(issuesState.filters.issueNumberFrom) : null,
        numberTo: issuesState.filters.issueNumberTo ? Number(issuesState.filters.issueNumberTo) : null,
        search: issuesState.filters.quickSearch,
        assigneeIds: Array.from(issuesLookupStates.assignee.selectedIds),
        authorIds: Array.from(issuesLookupStates.author.selectedIds),
        typeIds: Array.from(issuesLookupStates.type.selectedIds),
        statusIds: Array.from(issuesLookupStates.status.selectedIds),
        priorityIds: Array.from(issuesLookupStates.priority.selectedIds),
        companyIds: Array.from(issuesLookupStates.company.selectedIds),
        groupIds: Array.from(issuesLookupStates.group.selectedIds),
        registrationDateFrom: issuesState.filters.registrationDateFrom,
        registrationDateTo: issuesState.filters.registrationDateTo,
        resolutionDateFrom: issuesState.filters.resolutionDateFrom,
        resolutionDateTo: issuesState.filters.resolutionDateTo,
        page: issuesState.page,
        pageSize
    };
}

function buildIssuesRequestUrl(handler, request) {
    const params = new URLSearchParams();

    appendNumberParam(params, "numberFrom", request.numberFrom);
    appendNumberParam(params, "numberTo", request.numberTo);
    appendTextParam(params, "search", request.search);
    appendListParam(params, "assigneeIds", request.assigneeIds);
    appendListParam(params, "authorIds", request.authorIds);
    appendListParam(params, "typeIds", request.typeIds);
    appendListParam(params, "statusIds", request.statusIds);
    appendListParam(params, "priorityIds", request.priorityIds);
    appendListParam(params, "companyIds", request.companyIds);
    appendListParam(params, "groupIds", request.groupIds);
    appendTextParam(params, "registrationDateFrom", request.registrationDateFrom);
    appendTextParam(params, "registrationDateTo", request.registrationDateTo);
    appendTextParam(params, "resolutionDateFrom", request.resolutionDateFrom);
    appendTextParam(params, "resolutionDateTo", request.resolutionDateTo);
    appendNumberParam(params, "page", request.page);
    appendNumberParam(params, "pageSize", request.pageSize);

    return `?handler=${handler}&${params.toString()}`;
}

function appendNumberParam(params, key, value) {
    if (typeof value === "number" && !Number.isNaN(value)) {
        params.append(key, String(value));
    }
}

function appendTextParam(params, key, value) {
    if (value) {
        params.append(key, value);
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

function renderIssuesTable() {
    const tbody = document.getElementById("issuesRows");
    if (!tbody) return;

    tbody.textContent = "";

    if (!Array.isArray(issuesState.list.items) || issuesState.list.items.length === 0) {
        const tr = document.createElement("tr");
        const td = document.createElement("td");
        td.colSpan = 7;
        td.className = "text-center text-muted py-4";
        td.textContent = "Заявки не найдены";
        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }

    for (const item of issuesState.list.items) {
        const tr = document.createElement("tr");
        tr.appendChild(buildIssueIdCell(item, 0));
        tr.appendChild(buildCell(String(item.title || ""), 1));
        tr.appendChild(buildCompanyCell(item, 2));
        tr.appendChild(buildCell(String(item.assigneeName || "Не указан"), 3));
        tr.appendChild(buildCell(formatDateTime(item.createdAt), 4));
        tr.appendChild(buildCell(formatDateTime(item.completedAt), 5));
        tr.appendChild(buildStatusCell(item, 6));
        tbody.appendChild(tr);
    }
}

function renderIssuesTotalCount() {
    const container = document.getElementById("issuesTotalCount");
    if (!container) return;

    container.textContent = "";

    const label = document.createElement("span");
    label.textContent = "Всего заявок: ";
    container.appendChild(label);

    if (issuesState.exactTotalCount !== null) {
        const value = document.createElement("span");
        value.textContent = String(issuesState.exactTotalCount);
        container.appendChild(value);
        return;
    }

    if (issuesState.list.isTotalCountCapped) {
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
    value.textContent = String(issuesState.list.displayTotalCount);
    container.appendChild(value);
}

function renderIssuesPagination() {
    const container = document.getElementById("issuesPagination");
    if (!container) return;

    container.textContent = "";

    const currentPage = issuesState.list.page || 1;
    const totalPages = getResolvedTotalPages();
    const hasNextPage = hasIssuesNextPage();

    if (currentPage > 1) {
        container.appendChild(createPageButton("<<", async () => {
            await goToIssuesPage(1);
        }, false));

        container.appendChild(createPageButton("<", async () => {
            await goToIssuesPage(currentPage - 1);
        }, false));
    }

    const range = getIssuesPageRange(currentPage, totalPages);
    for (const page of range) {
        container.appendChild(createPageButton(String(page), async () => {
            await goToIssuesPage(page);
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
            await goToIssuesPage(currentPage + 1);
        }, false));

        container.appendChild(createPageButton(">>", async () => {
            await goToIssuesLastPage();
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

function getIssuesPageRange(currentPage, totalPages) {
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

async function goToIssuesPage(page) {
    if (page <= 0) {
        return;
    }

    issuesState.page = page;
    await reloadIssues(false);
}

async function goToIssuesLastPage() {
    if (issuesState.exactTotalPages === null && issuesState.list.isTotalCountCapped) {
        const loaded = await ensureExactCountLoaded();
        if (!loaded) {
            return;
        }
    }

    issuesState.page = getResolvedTotalPages();
    await reloadIssues(false);
}

async function ensureExactCountLoaded() {
    if (issuesState.exactTotalCount !== null && issuesState.exactTotalPages !== null) {
        return true;
    }

    const requestId = ++issuesExactCountRequestId;

    try {
        const request = collectIssuesRequest();
        const url = buildIssuesRequestUrl("ExactCount", request);
        const response = await sendJsonRequest(url, "GET", buildJsonHeaders(antiForgeryToken));

        if (requestId !== issuesExactCountRequestId) {
            return false;
        }

        issuesState.exactTotalCount = response.totalCount || 0;
        issuesState.exactTotalPages = response.totalPages || 1;
        issuesState.list.totalPages = issuesState.exactTotalPages;

        renderIssuesTotalCount();
        renderIssuesPagination();
        return true;
    } catch (error) {
        console.error(error);
        showPageError(error.message || "Не удалось получить точное количество заявок.");
        return false;
    }
}

function getResolvedTotalPages() {
    if (issuesState.exactTotalPages !== null) {
        return issuesState.exactTotalPages;
    }

    return issuesState.list.totalPages || 1;
}

function hasIssuesNextPage() {
    if (issuesState.exactTotalPages !== null) {
        return issuesState.list.page < issuesState.exactTotalPages;
    }

    return issuesState.list.hasNextPage === true || issuesState.list.page < issuesState.list.totalPages;
}

function getTrimmedValue(id) {
    const element = document.getElementById(id);
    if (!element) {
        return "";
    }

    return String(element.value || "").trim();
}

function formatDateTime(value) {
    if (!value) {
        return "Не указана";
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return "Не указана";
    }

    return date.toLocaleString("ru-RU", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
    });
}

function ensureArray(value) {
    return Array.isArray(value) ? value : [];
}

function buildCell(text, columnIndex, isCentered = false) {
    const td = document.createElement("td");
    td.style.fontSize = "1rem";
    td.className = "pe-3 ps-3";
    td.setAttribute("data-column-cell", String(columnIndex));

    const content = document.createElement("div");
    content.className = "text-truncate";
    content.style.width = "100%";
    if (isCentered) {
        content.classList.add("text-center");
    }
    content.textContent = text;
    td.appendChild(content);

    return td;
}

function buildIssueIdCell(item, columnIndex) {
    const td = document.createElement("td");
    td.style.fontSize = "1rem";
    td.className = "pe-3 ps-0";
    td.setAttribute("data-column-cell", String(columnIndex));
    td.style.position = "relative";

    const priorityName = String(item.priorityName || "").trim();
    if (priorityName) {
        td.title = `Приоритет: ${priorityName}`;
    }

    const wrapper = document.createElement("div");
    wrapper.className = "d-flex align-items-stretch";
    wrapper.style.minHeight = "1.75rem";

    const priorityIndicator = document.createElement("span");
    priorityIndicator.className = "position-absolute top-0 bottom-0 flex-shrink-0";
    priorityIndicator.style.left = "0";
    priorityIndicator.style.width = "6px";
    priorityIndicator.style.backgroundColor = normalizeHexColor(item.priorityColor) || "transparent";

    const content = document.createElement("div");
    content.className = "text-truncate flex-grow-1";
    content.style.paddingLeft = "18px";
    content.style.width = "100%";
    content.textContent = String(item.id || "");

    wrapper.appendChild(priorityIndicator);
    wrapper.appendChild(content);
    td.appendChild(wrapper);
    return td;
}

function buildStatusCell(item, columnIndex) {
    const td = document.createElement("td");
    td.style.fontSize = "1rem";
    td.className = "pe-3 ps-3";
    td.setAttribute("data-column-cell", String(columnIndex));

    const wrapper = document.createElement("div");
    wrapper.className = "d-flex justify-content-center";

    const badge = document.createElement("span");
    badge.style.padding = "0.45rem 0.9rem";
    badge.style.minWidth = "95px";
    badge.style.minHeight = "17px";
    badge.className = "badge rounded-pill text-bg-light border";
    badge.textContent = String(item.statusName || "Не указан");
    badge.style.color = "#6A747C";

    const statusColor = normalizeHexColor(item.statusColor);
    if (statusColor) {
        badge.style.backgroundColor = statusColor;
        badge.style.color = "#ffffff";
        badge.style.borderColor = statusColor;
    }

    const priorityName = String(item.priorityName || "").trim();
    if (priorityName) {
        badge.title = `Приоритет: ${priorityName}`;
    }

    wrapper.appendChild(badge);
    td.appendChild(wrapper);
    return td;
}

function buildCompanyCell(item, columnIndex) {
    const td = document.createElement("td");
    td.style.fontSize = "1rem";
    td.className = "pe-3 ps-3";
    td.setAttribute("data-column-cell", String(columnIndex));

    const wrapper = document.createElement("div");
    wrapper.className = "d-flex align-items-center gap-2";
    wrapper.style.minWidth = "0";

    const marker = document.createElement("span");
    marker.className = "rounded-circle flex-shrink-0";
    marker.style.width = "0.75rem";
    marker.style.height = "0.75rem";
    marker.style.backgroundColor = normalizeHexColor(item.companyCategoryColor) || "#6c757d";

    const text = document.createElement("div");
    text.className = "text-truncate";
    text.style.width = "100%";
    text.textContent = String(item.companyName || "Не указан");

    wrapper.appendChild(marker);
    wrapper.appendChild(text);
    td.appendChild(wrapper);
    return td;
}

function clearPageMessages() {
    hideMessage("pageError");
    hideMessage("pageSuccess");
}

function normalizeHexColor(value) {
    const normalizedValue = String(value || "").trim().toUpperCase();
    return /^#([0-9A-F]{6})$/.test(normalizedValue) ? normalizedValue : "";
}

function initIssuesGridColumnResize() {
    if (typeof issuesGridResizeCleanup === "function") {
        issuesGridResizeCleanup();
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

            const col = document.getElementById(`issuesCol${columnIndex}`);
            const nextCol = document.getElementById(`issuesCol${columnIndex + 1}`);
            if (!col) {
                return;
            }

            if (!nextCol) {
                return;
            }

            syncIssuesGridColumnWidths();

            const startX = event.clientX;
            const startWidth = col.getBoundingClientRect().width;
            const nextStartWidth = nextCol.getBoundingClientRect().width;
            const MIN_COLUMN_WIDTH = 90;

            const onMouseMove = moveEvent => {
                const delta = moveEvent.clientX - startX;
                const currentWidth = startWidth + delta;
                const adjacentWidth = nextStartWidth - delta;

                if (currentWidth < MIN_COLUMN_WIDTH || adjacentWidth < MIN_COLUMN_WIDTH) {
                    return;
                }

                col.style.width = `${currentWidth}px`;
                nextCol.style.width = `${adjacentWidth}px`;
            };

            const onMouseUp = () => {
                document.removeEventListener("mousemove", onMouseMove);
                document.removeEventListener("mouseup", onMouseUp);
                saveIssuesGridColumnWidths();
            };

            document.addEventListener("mousemove", onMouseMove);
            document.addEventListener("mouseup", onMouseUp);
        };

        resizer.addEventListener("mousedown", onMouseDown);
        handlers.push(() => resizer.removeEventListener("mousedown", onMouseDown));
    }

    issuesGridResizeCleanup = () => {
        for (const dispose of handlers) {
            dispose();
        }
    };
}

function syncIssuesGridColumnWidths() {
    const table = document.getElementById("issuesGrid");
    if (!table) {
        return;
    }

    const headerCells = table.querySelectorAll("thead th[data-column-index]");
    for (const headerCell of headerCells) {
        const columnIndex = Number(headerCell.getAttribute("data-column-index"));
        if (Number.isNaN(columnIndex)) {
            continue;
        }

        const col = document.getElementById(`issuesCol${columnIndex}`);
        if (!col) {
            continue;
        }

        col.style.width = `${headerCell.getBoundingClientRect().width}px`;
    }
}

function restoreIssuesGridColumnWidths() {
    const raw = localStorage.getItem(ISSUES_GRID_COLUMNS_STORAGE_KEY);
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

            if (item.width < 90) {
                continue;
            }

            const col = document.getElementById(`issuesCol${item.index}`);
            if (!col) {
                continue;
            }

            col.style.width = `${item.width}px`;
        }
    } catch (error) {
        console.error(error);
        localStorage.removeItem(ISSUES_GRID_COLUMNS_STORAGE_KEY);
    }
}

function saveIssuesGridColumnWidths() {
    const widths = [];

    for (let index = 0; index <= 6; index += 1) {
        const col = document.getElementById(`issuesCol${index}`);
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

    localStorage.setItem(ISSUES_GRID_COLUMNS_STORAGE_KEY, JSON.stringify(widths));
}

function restoreIssuesFiltersState() {
    issuesState.filters = createDefaultFilterState();

    const raw = localStorage.getItem(ISSUES_FILTERS_STORAGE_KEY);
    if (!raw) {
        return;
    }

    try {
        const parsed = JSON.parse(raw);
        issuesState.pageSize = isAllowedPageSize(parsed.pageSize) ? parsed.pageSize : 20;
        issuesState.filters.issueNumberFrom = parsed.issueNumberFrom ? String(parsed.issueNumberFrom) : "";
        issuesState.filters.issueNumberTo = parsed.issueNumberTo ? String(parsed.issueNumberTo) : "";
        issuesState.filters.registrationDateFrom = parsed.registrationDateFrom ? String(parsed.registrationDateFrom) : "";
        issuesState.filters.registrationDateTo = parsed.registrationDateTo ? String(parsed.registrationDateTo) : "";
        issuesState.filters.resolutionDateFrom = parsed.resolutionDateFrom ? String(parsed.resolutionDateFrom) : "";
        issuesState.filters.resolutionDateTo = parsed.resolutionDateTo ? String(parsed.resolutionDateTo) : "";

        restoreLookupSelection("assignee", parsed.assigneeIds, parsed.assigneeSearch);
        restoreLookupSelection("author", parsed.authorIds, parsed.authorSearch);
        restoreLookupSelection("type", parsed.typeIds, parsed.typeSearch);
        restoreLookupSelection("status", parsed.statusIds, parsed.statusSearch);
        restoreLookupSelection("priority", parsed.priorityIds, parsed.prioritySearch);
        restoreLookupSelection("company", parsed.companyIds, parsed.companySearch);
        restoreLookupSelection("group", parsed.groupIds, parsed.groupSearch);
    } catch (error) {
        console.error(error);
        localStorage.removeItem(ISSUES_FILTERS_STORAGE_KEY);
    }
}

function restoreLookupSelection(key, ids, search) {
    const lookup = issuesLookupStates[key];
    lookup.selectedIds = new Set(Array.isArray(ids) ? ids.filter(id => Number.isInteger(id) && id > 0) : []);
    lookup.search = typeof search === "string" ? normalizeLookupSearch(search) : "";
}

function saveIssuesFiltersState() {
    syncIssuesFilterStateFromInputs();

    const payload = {
        pageSize: issuesState.pageSize,
        issueNumberFrom: issuesState.filters.issueNumberFrom,
        issueNumberTo: issuesState.filters.issueNumberTo,
        registrationDateFrom: issuesState.filters.registrationDateFrom,
        registrationDateTo: issuesState.filters.registrationDateTo,
        resolutionDateFrom: issuesState.filters.resolutionDateFrom,
        resolutionDateTo: issuesState.filters.resolutionDateTo,
        assigneeIds: Array.from(issuesLookupStates.assignee.selectedIds),
        authorIds: Array.from(issuesLookupStates.author.selectedIds),
        typeIds: Array.from(issuesLookupStates.type.selectedIds),
        statusIds: Array.from(issuesLookupStates.status.selectedIds),
        priorityIds: Array.from(issuesLookupStates.priority.selectedIds),
        companyIds: Array.from(issuesLookupStates.company.selectedIds),
        groupIds: Array.from(issuesLookupStates.group.selectedIds),
        assigneeSearch: issuesLookupStates.assignee.search,
        authorSearch: issuesLookupStates.author.search,
        typeSearch: issuesLookupStates.type.search,
        statusSearch: issuesLookupStates.status.search,
        prioritySearch: issuesLookupStates.priority.search,
        companySearch: issuesLookupStates.company.search,
        groupSearch: issuesLookupStates.group.search
    };

    localStorage.setItem(ISSUES_FILTERS_STORAGE_KEY, JSON.stringify(payload));
}

function syncIssuesFilterStateFromInputs() {
    if (!issuesState.filters) {
        issuesState.filters = createDefaultFilterState();
    }

    issuesState.filters.issueNumberFrom = getTrimmedValue("filterIssueNumberFrom");
    issuesState.filters.issueNumberTo = getTrimmedValue("filterIssueNumberTo");
    issuesState.filters.quickSearch = getTrimmedValue("filterQuickSearch");
    issuesState.filters.registrationDateFrom = getTrimmedValue("filterRegistrationDateFrom");
    issuesState.filters.registrationDateTo = getTrimmedValue("filterRegistrationDateTo");
    issuesState.filters.resolutionDateFrom = getTrimmedValue("filterResolutionDateFrom");
    issuesState.filters.resolutionDateTo = getTrimmedValue("filterResolutionDateTo");
    issuesState.pageSize = Number(getTrimmedValue("filterPageSize") || "20");
}

function createDefaultFilterState() {
    return {
        quickSearch: "",
        issueNumberFrom: "",
        issueNumberTo: "",
        registrationDateFrom: "",
        registrationDateTo: "",
        resolutionDateFrom: "",
        resolutionDateTo: ""
    };
}

function isAllowedPageSize(value) {
    return value === 20 || value === 50 || value === 100;
}

function capitalizeLookupKey(key) {
    return key.charAt(0).toUpperCase() + key.slice(1);
}

function getLookupToggleLabel(key, count) {
    const baseLabels = {
        assignee: "Ответственный",
        author: "Автор",
        type: "Тип",
        status: "Статус",
        priority: "Приоритет",
        company: "Клиент",
        group: "Группа"
    };

    if (count <= 0) {
        return baseLabels[key] || "";
    }

    return `${baseLabels[key]} (${count})`;
}

function showPageError(message) {
    showMessage("pageError", message);
}

function showMessage(id, message) {
    const element = document.getElementById(id);
    if (!element) return;

    element.textContent = message;
    element.classList.remove("d-none");
}

function hideMessage(id) {
    const element = document.getElementById(id);
    if (!element) return;

    element.textContent = "";
    element.classList.add("d-none");
}

function updateIssueFiltersToggleButton(isExpanded) {
    const button = document.getElementById("toggleIssueFiltersButton");
    if (!button) {
        return;
    }

    button.textContent = isExpanded ? "Свернуть фильтры" : "Развернуть фильтры";
    button.setAttribute("aria-expanded", isExpanded ? "true" : "false");
}

function isIssueFiltersExpanded() {
    const collapse = document.getElementById("issueFiltersCollapse");
    return collapse ? collapse.classList.contains("show") : false;
}

function initIssueSearchWarningModal() {
    const modalElement = document.getElementById("issueSearchWarningModal");
    if (!modalElement || !window.bootstrap) {
        return;
    }

    issueSearchWarningModal = new bootstrap.Modal(modalElement);
}

function showIssueSearchWarningModal() {
    if (issueSearchWarningModal) {
        issueSearchWarningModal.show();
    }
}

function getNonWhitespaceCount(value) {
    return String(value || "").replace(/\s+/g, "").length;
}
