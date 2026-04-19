let issueDetailsState = {
    issueId: 0,
    details: null
};

document.addEventListener("DOMContentLoaded", () => {
    initIssueDetailsPage();
});

async function initIssueDetailsPage() {
    const root = document.getElementById("issueDetailsPage");
    if (!root) {
        return;
    }

    issueDetailsState.issueId = Number.parseInt(root.dataset.issueId || "", 10);
    if (!Number.isInteger(issueDetailsState.issueId) || issueDetailsState.issueId <= 0) {
        showPageError("Не удалось определить идентификатор заявки.");
        renderIssueDetailsContentError();
        return;
    }

    try {
        const response = await sendJsonRequest(`?handler=Details&id=${issueDetailsState.issueId}`, "GET", buildJsonHeaders(null));
        issueDetailsState.details = response;
        hidePageError();
        renderIssueDetails();
    } catch (error) {
        console.error(error);
        showPageError(error.message || "Не удалось загрузить карточку заявки.");
        renderIssueDetailsContentError();
    }
}

function renderIssueDetails() {
    const container = document.getElementById("issueDetailsContent");
    if (!container) {
        return;
    }

    container.textContent = "";

    const details = issueDetailsState.details;
    if (!details) {
        renderIssueDetailsContentError();
        return;
    }

    container.appendChild(buildIssueDetailsHeader(details));
    container.appendChild(buildIssueDetailsFields(details));
}

function buildIssueDetailsHeader(details) {
    const wrapper = document.createElement("div");
    wrapper.className = "d-flex flex-wrap align-items-center gap-2 mb-4";

    const marker = document.createElement("span");
    marker.className = "rounded-circle flex-shrink-0";
    marker.style.width = "0.75rem";
    marker.style.height = "0.75rem";
    marker.style.backgroundColor = normalizeHexColor(details.companyCategoryColor) || "#6c757d";

    const issueNumber = document.createElement("span");
    issueNumber.className = "text-muted";
    issueNumber.style.fontSize = "12px";
    issueNumber.textContent = `Заявка №${details.id}`;

    const title = document.createElement("span");
    title.className = "text-dark fw-semibold";
    title.textContent = getDisplayText(details.title);

    wrapper.appendChild(marker);
    wrapper.appendChild(issueNumber);
    wrapper.appendChild(title);

    return wrapper;
}

function buildIssueDetailsFields(details) {
    const fields = [
        createField("Тема", details.title),
        createField("Клиент", details.companyName),
        createField("Объект обслуживания", details.serviceObjectName),
        createField("Ответственный", details.assigneeName),
        createField("Автор", details.authorName),
        createField("Тип", details.typeName),
        createStatusField("Статус", details.statusName, details.statusColor),
        createPriorityField("Приоритет", details.priorityName, details.priorityColor),
        createField("Дата регистрации", formatDateTime(details.createdAt)),
        createField("Дата решения", formatDateTime(details.completedAt)),
        createField("Срок решения", formatDateTime(details.deadlineAt)),
        createField("Отложено до", formatDateTime(details.delayTo))
    ];

    const row = document.createElement("div");
    row.className = "row g-4";

    const chunks = splitFieldsIntoColumns(fields, 3);
    for (const chunk of chunks) {
        const column = document.createElement("div");
        column.className = "col-12 col-lg-4";

        const list = document.createElement("div");
        list.className = "d-flex flex-column gap-3";

        for (const field of chunk) {
            list.appendChild(buildIssueFieldCard(field));
        }

        column.appendChild(list);
        row.appendChild(column);
    }

    return row;
}

function createField(label, value) {
    return {
        label,
        value: getDisplayText(value),
        kind: "text"
    };
}

function createStatusField(label, value, color) {
    return {
        label,
        value: getDisplayText(value),
        color: normalizeHexColor(color),
        kind: "status"
    };
}

function createPriorityField(label, value, color) {
    return {
        label,
        value: getDisplayText(value),
        color: normalizeHexColor(color),
        kind: "priority"
    };
}

function splitFieldsIntoColumns(fields, columnsCount) {
    const normalizedColumnsCount = Math.max(1, columnsCount);
    const chunks = Array.from({ length: normalizedColumnsCount }, () => []);
    const chunkSize = fields.length === 0
        ? 0
        : Math.ceil(fields.length / normalizedColumnsCount);

    for (let index = 0; index < normalizedColumnsCount; index += 1) {
        if (chunkSize === 0) {
            break;
        }

        const start = index * chunkSize;
        chunks[index] = fields.slice(start, start + chunkSize);
    }

    return chunks;
}

function buildIssueFieldCard(field) {
    const wrapper = document.createElement("div");
    wrapper.className = "border rounded-3 bg-light-subtle px-3 py-2";

    const label = document.createElement("div");
    label.className = "text-muted mb-1";
    label.style.fontSize = "12px";
    label.textContent = field.label;

    const value = document.createElement("div");
    value.className = "text-dark";
    value.style.fontSize = "14px";

    if (field.kind === "status" || field.kind === "priority") {
        const badge = document.createElement("span");
        badge.className = "badge rounded-pill text-bg-light border";
        badge.style.padding = "0.45rem 0.9rem";
        badge.style.minHeight = "17px";
        badge.textContent = field.value;

        if (field.color) {
            badge.style.backgroundColor = field.color;
            badge.style.color = "#ffffff";
            badge.style.borderColor = field.color;
        } else {
            badge.style.color = "#6A747C";
        }

        value.appendChild(badge);
    } else {
        value.textContent = field.value;
    }

    wrapper.appendChild(label);
    wrapper.appendChild(value);
    return wrapper;
}

function renderIssueDetailsContentError() {
    const container = document.getElementById("issueDetailsContent");
    if (!container) {
        return;
    }

    container.textContent = "";

    const message = document.createElement("div");
    message.className = "text-muted";
    message.textContent = "Не удалось отобразить данные заявки.";
    container.appendChild(message);
}

function getDisplayText(value) {
    const normalizedValue = String(value || "").trim();
    return normalizedValue ? normalizedValue : "Не указано";
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

function showPageError(message) {
    const element = document.getElementById("pageError");
    if (!element) {
        return;
    }

    element.textContent = message;
    element.classList.remove("d-none");
}

function hidePageError() {
    const element = document.getElementById("pageError");
    if (!element) {
        return;
    }

    element.textContent = "";
    element.classList.add("d-none");
}

function normalizeHexColor(value) {
    const normalizedValue = String(value || "").trim().toUpperCase();
    return /^#([0-9A-F]{6})$/.test(normalizedValue) ? normalizedValue : "";
}
