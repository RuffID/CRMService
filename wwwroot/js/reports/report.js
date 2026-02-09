let antiForgeryToken = null;
const reportSortKey = "crm_report_sort_v1";
let lastReportItems = [];
window.resetReportSorting = resetReportSorting;
const reportAutoReloadMs = 5 * 60 * 1000; // 5 минут

document.addEventListener('DOMContentLoaded', () => {
    antiForgeryToken = getRequestVerificationToken();
    initReportSorting();
    startAutoReload();

    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el);
    });
});

function initReportSorting() {
    const grid = document.getElementById('grid');
    if (!grid) return;

    ensureSortIndicators();

    grid.querySelectorAll('th[data-sort-key]').forEach(th => {
        th.addEventListener('click', () => {
            const key = th.dataset.sortKey;
            if (!key) return;

            const st = loadSortState();

            let dir = 'asc';
            if (st && st.key === key) {
                dir = (st.dir === 'asc') ? 'desc' : 'asc';
            }

            saveSortState({ key, dir });
            updateSortIndicators();
            applySortAndRender();
        });
    });

    updateSortIndicators();
}

async function loadPerformanceReport() {
    enforceEoModeDates();
    const payload = buildReportPayload();

    try {
        const response = await sendJsonRequest(`?handler=Report`, 'POST', buildJsonHeaders(antiForgeryToken), payload);

        const data = unwrapOrThrow(response, 'Ошибка загрузки отчёта.');
        lastReportItems = Array.isArray(data) ? data : [];
        applySortAndRender();
    } catch (e) {
        console.error(e);
    }
}

function buildReportPayload() {
    const st = readState();

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

function renderTableRows(items) {
    const tbody = document.getElementById('rows');
    tbody.innerHTML = '';

    if (!items || items.length === 0) {
        const tr = document.createElement('tr');

        const td = document.createElement('td');
        td.colSpan = 4;
        td.className = 'text-center text-muted py-4';
        td.textContent = 'Нет данных';

        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }

    items.forEach(it => {
        const tr = document.createElement('tr');

        const tdName = document.createElement('td');
        tdName.textContent = buildFullName(it);

        const tdResolved = document.createElement('td');
        tdResolved.className = 'text-center';
        tdResolved.textContent = it.solvedIssues ?? 0;

        const tdCurrent = document.createElement('td');
        tdCurrent.className = 'text-center';
        tdCurrent.textContent = (it.issues ? it.issues.length : 0);

        const tdTime = document.createElement('td');
        const spent = (it.spentedTime ?? 0);
        tdTime.className = 'text-center';
        tdTime.textContent = formatHours(spent);
        tdTime.title = `${Math.round(spent * 60)} минут`;

        tr.appendChild(tdName);
        tr.appendChild(tdResolved);
        tr.appendChild(tdCurrent);
        tr.appendChild(tdTime);
        tbody.appendChild(tr);
    });
}

function formatHours(hours) {
    if (!hours || hours <= 0) return '0 минут';

    const totalMinutes = Math.round(hours * 60);
    const h = Math.floor(totalMinutes / 60);
    const m = totalMinutes % 60;

    if (h > 0 && m > 0) return `${h} час${h > 1 ? 'а' : ''} ${m} минут`;
    if (h > 0) return `${h} час${h > 1 ? 'а' : ''}`;
    return `${m} минут`;
}

function buildFullName(it) {
    const ln = it.lastName || '';
    const fn = it.firstName || '';
    const pn = it.patronymic || '';
    return [ln, fn, pn].filter(x => x && x.trim().length > 0).join(' ').trim();
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
        x.classList.add('opacity-0');
        x.textContent = '▲';
    });
}

function updateSortIndicators() {
    clearSortIndicators();

    const st = loadSortState();
    if (!st) return;

    const th = document.querySelector(`#grid th[data-sort-key="${st.key}"]`);
    if (!th) return;

    const ind = th.querySelector('.sort-indicator');
    if (!ind) return;

    ind.classList.remove('opacity-0');
    ind.textContent = (st.dir === 'asc') ? '▲' : '▼';
}

function applySortAndRender() {
    let items = Array.isArray(lastReportItems) ? [...lastReportItems] : [];

    items = applyClientFilters(items);

    const st = loadSortState();
    if (st) {
        sortReportItems(items, st.key, st.dir);
    }

    renderTableRows(items);
}

function sortReportItems(items, key, dir) {
    const sign = (dir === 'desc') ? -1 : 1;

    const getName = (it) => (buildFullName(it) || '').trim().toLowerCase();
    const getResolved = (it) => Number(it?.solvedIssues ?? 0);
    const getCurrent = (it) => Number(it?.issues?.length ?? 0);
    const getTime = (it) => Number(it?.spentedTime ?? 0);

    const cmpNum = (a, b) => (a === b ? 0 : (a < b ? -1 : 1));
    const cmpStr = (a, b) => a.localeCompare(b, 'ru');

    items.sort((a, b) => {
        let r = 0;

        switch (key) {
            case 'name': {
                r = cmpStr(getName(a), getName(b));
                break;
            }
            case 'resolved': {
                r = cmpNum(getResolved(a), getResolved(b));
                break;
            }
            case 'current': {
                r = cmpNum(getCurrent(a), getCurrent(b));
                break;
            }
            case 'time': {
                r = cmpNum(getTime(a), getTime(b));
                break;
            }
            default:
                r = 0;
                break;
        }

        if (r === 0 && key !== 'name') {
            r = cmpStr(getName(a), getName(b));
        }

        return r * sign;
    });
}

function resetReportSorting() {
    saveSortState(null);
    updateSortIndicators();
    applySortAndRender();
}

function ensureSortIndicators() {
    const grid = document.getElementById('grid');
    if (!grid) return;

    grid.querySelectorAll('th[data-sort-key]').forEach(th => {
        th.style.cursor = 'pointer';
        th.classList.add('user-select-none');

        if (th.querySelector('.sort-indicator')) return;

        const ind = document.createElement('span');
        ind.className = 'sort-indicator ms-1 opacity-0';
        ind.textContent = '▲';
        th.appendChild(ind);
    });
}

function startAutoReload() {
    setInterval(async () => {
        if (document.hidden)
            return;
        await loadPerformanceReport();
    }, reportAutoReloadMs);
}

function enforceEoModeDates() {
    const st = readState();
    if (!st || st.eoMode !== true) return;

    const today = toDateInputValue(new Date());

    const df = document.getElementById('dateFrom');
    const dt = document.getElementById('dateTo');

    if (df) df.value = today;
    if (dt) dt.value = today;
}

function toDateInputValue(d) {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${y}-${m}-${day}`;
}

function applyClientFilters(items) {
    const st = readState() || {};
    if (!st) return items;

    let res = items;

    if (st.hideWithoutSolved) {
        res = res.filter(x => Number(x?.solvedIssues ?? 0) > 0);
    }

    if (st.hideWithoutTime) {
        res = res.filter(x => Number(x?.spentedTime ?? 0) > 0);
    }

    if (st.hideWithoutCurrent) {
        res = res.filter(x => Number(x?.issues?.length ?? 0) > 0);
    }

    return res;
}