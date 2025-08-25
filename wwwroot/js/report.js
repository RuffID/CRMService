document.addEventListener('DOMContentLoaded', () => {
    // Загружает данные при входе на страницу
    loadData().catch(err => console.error(err));
});

// Извлекает антифорджери-токен из скрытой формы для текущего запроса
function getRequestVerificationToken() {
    const inp = document.querySelector('#af input[name="__RequestVerificationToken"]');
    return inp ? inp.value : '';
}

// Загружает данные POST-запросом к хендлеру с токеном в заголовке
async function loadData() {
    const token = getRequestVerificationToken();

    const data = await sendPostJson('?handler=Data', null, token);

    renderRows(data);
}

// Форматирует минуты в строку "X ч. Y м." / "Y м."
function fmtMinutes(total) {
    const hours = Math.floor(total / 60);
    const mins = total % 60;
    if (hours > 0 && mins > 0) return `${hours} ч. ${mins} м.`;
    if (hours > 0) return `${hours} ч.`;
    return `${mins} м.`;
}

// Рендерит таблицу
function renderRows(items) {
    if (!items || items.length === 0) return;

    const tbody = document.getElementById('rows');
    tbody.innerHTML = '';

    items.forEach(it => {
        const tr = document.createElement('tr');

        const tdName = document.createElement('td');
        tdName.textContent = it.fullName;

        const tdResolved = document.createElement('td');
        tdResolved.className = 'text-center';
        tdResolved.textContent = it.resolvedCount;

        const tdCurrent = document.createElement('td');
        tdCurrent.className = 'text-center';
        tdCurrent.textContent = it.currentCount;

        const tdTime = document.createElement('td');
        tdTime.className = 'text-center';
        tdTime.textContent = fmtMinutes(it.loggedMinutes);
        tdTime.title = `${it.loggedMinutes} мин.`;

        tr.appendChild(tdName);
        tr.appendChild(tdResolved);
        tr.appendChild(tdCurrent);
        tr.appendChild(tdTime);
        tbody.appendChild(tr);
    });
}