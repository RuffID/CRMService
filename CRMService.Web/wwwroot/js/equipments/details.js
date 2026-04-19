const ACCESS_VALUE_SEPARATOR = "/";
const INVALID_PLACEHOLDER_VALUES = new Set(["-", "\u043D\u0435\u0443\u043A\u0430\u0437\u0430\u043D\u043E"]);
const TERMINAL_ACCESS_BUTTONS = [
    { parameterCode: "AnyDesk", clearbatType: "anydesk", iconPath: "/icon/equipments/anydesk.png?v=1", iconAlt: "AnyDesk", displayName: "ЭниДеск", requirePassword: true },
    { parameterCode: "AA", clearbatType: "ammyy", iconPath: "/icon/equipments/ammyadmin.png?v=1", iconAlt: "АмиАдмин", displayName: "АмиАдмин", requirePassword: false },
    { parameterCode: "AC", clearbatType: "assistant", iconPath: "/icon/equipments/assistant.png?v=1", iconAlt: "Ассистент", displayName: "Ассистент", requirePassword: false },
    { parameterCode: "rust", clearbatType: "rustdesk", iconPath: "/icon/equipments/rustdesk.ico?v=1", iconAlt: "RustDesk", displayName: "Растдеск", requirePassword: false }
];
const WEB_LINK_PARAMETER_CODE = "1212";
const IIKO_CREDENTIALS_PARAMETER_CODE = "0008";
const SERVER_ACCESS_BUTTONS = [
    { parameterCode: "srv_addr", iconPath: "/icon/equipments/iikoOffice_icon.ico?v=1", iconAlt: "RMS", displayName: "RMS" },
    { parameterCode: "0017", iconPath: "/icon/equipments/iikoChain_icon.ico?v=1", iconAlt: "Чейн", displayName: "Чейн" }
];

let equipmentDetailsState = {
    equipmentId: 0,
    details: null
};

document.addEventListener("DOMContentLoaded", () => {
    initEquipmentDetailsPage();
});

async function initEquipmentDetailsPage() {
    const root = document.getElementById("equipmentDetailsPage");
    if (!root) {
        return;
    }

    equipmentDetailsState.equipmentId = Number.parseInt(root.dataset.equipmentId || "", 10);
    if (!Number.isInteger(equipmentDetailsState.equipmentId) || equipmentDetailsState.equipmentId <= 0) {
        showPageError("Не удалось определить идентификатор оборудования.");
        renderEquipmentDetailsContentError();
        return;
    }

    try {
        const response = await sendJsonRequest(`?handler=Details&id=${equipmentDetailsState.equipmentId}`, "GET", buildJsonHeaders(null));
        equipmentDetailsState.details = response;
        hidePageError();
        renderEquipmentDetails();
    } catch (error) {
        console.error(error);
        showPageError(error.message || "Не удалось загрузить карточку оборудования.");
        renderEquipmentDetailsContentError();
    }
}

function renderEquipmentDetails() {
    const container = document.getElementById("equipmentDetailsContent");
    if (!container) {
        return;
    }

    container.textContent = "";

    const details = equipmentDetailsState.details;
    if (!details) {
        renderEquipmentDetailsContentError();
        return;
    }

    container.appendChild(buildEquipmentDetailsHeader(details));
    container.appendChild(buildEquipmentDetailsFields(details.fields));
    initializeAccessTooltips(container);
}

function buildEquipmentDetailsHeader(details) {
    const wrapper = document.createElement("div");
    wrapper.className = "d-flex flex-wrap align-items-center gap-2 mb-4";

    const marker = document.createElement("span");
    marker.className = "rounded-circle flex-shrink-0";
    marker.style.width = "0.75rem";
    marker.style.height = "0.75rem";
    marker.style.backgroundColor = normalizeHexColor(details.companyCategoryColor) || "#6c757d";

    const companyName = document.createElement("span");
    companyName.className = "text-dark";
    companyName.style.fontSize = "12px";
    companyName.textContent = getDisplayText(details.companyName);

    wrapper.appendChild(marker);
    wrapper.appendChild(companyName);

    return wrapper;
}

function buildEquipmentDetailsFields(fields) {
    const normalizedFields = Array.isArray(fields) ? fields : [];
    const row = document.createElement("div");
    row.className = "row g-4";

    const chunks = splitFieldsIntoColumns(normalizedFields, 3);
    for (const chunk of chunks) {
        const column = document.createElement("div");
        column.className = "col-12 col-lg-4";

        const list = document.createElement("div");
        list.className = "d-flex flex-column gap-3";

        for (const field of chunk) {
            list.appendChild(buildEquipmentFieldCard(field));
        }

        column.appendChild(list);
        row.appendChild(column);
    }

    return row;
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

function buildEquipmentFieldCard(field) {
    const wrapper = document.createElement("div");
    wrapper.className = "border rounded-3 bg-light-subtle px-3 py-2";

    const contentRow = document.createElement("div");
    contentRow.className = "d-flex justify-content-between align-items-start gap-3";

    const textBlock = document.createElement("div");
    textBlock.className = "flex-grow-1";

    const label = document.createElement("div");
    label.className = "text-muted mb-1";
    label.style.fontSize = "12px";
    label.textContent = getDisplayText(field?.label);

    const value = document.createElement("div");
    value.className = "text-dark";
    value.style.fontSize = "14px";
    appendEquipmentFieldValue(value, field);

    textBlock.appendChild(label);
    textBlock.appendChild(value);
    contentRow.appendChild(textBlock);

    const accessButtons = buildAccessButtonsForField(field);
    if (accessButtons !== null) {
        contentRow.appendChild(accessButtons);
    }

    wrapper.appendChild(contentRow);
    return wrapper;
}

function appendEquipmentFieldValue(container, field) {
    const parameterCode = typeof field?.parameterCode === "string" ? field.parameterCode : "";
    const valueText = getDisplayText(field?.value);

    if (parameterCode === WEB_LINK_PARAMETER_CODE) {
        const webLink = normalizeEquipmentWebLink(field?.value);
        if (webLink) {
            const link = document.createElement("a");
            link.href = webLink;
            link.target = "_blank";
            link.rel = "noopener";
            link.className = "text-primary";
            link.style.textDecoration = "none";
            link.textContent = valueText;
            container.appendChild(link);
            return;
        }
    }

    container.textContent = valueText;
}

function buildAccessButtonsForField(field) {
    const parameterCode = typeof field?.parameterCode === "string" ? field.parameterCode : "";
    if (!parameterCode) {
        return null;
    }

    for (const accessButton of TERMINAL_ACCESS_BUTTONS) {
        if (accessButton.parameterCode !== parameterCode) {
            continue;
        }

        const credentials = parseAccessCredentials(getEquipmentParameterValue(parameterCode), accessButton.requirePassword);
        if (credentials === null) {
            return null;
        }

        const wrapper = createAccessButtonsWrapper();
        const button = buildAccessIconButton(accessButton.iconPath, accessButton.iconAlt, accessButton.displayName);
        bindAccessButtonHoverState(button);
        button.addEventListener("click", () => {
            openClearbatLink(buildClearbatUrl(accessButton.clearbatType, credentials));
        });
        wrapper.appendChild(button);
        return wrapper;
    }

    for (const accessButton of SERVER_ACCESS_BUTTONS) {
        if (accessButton.parameterCode !== parameterCode) {
            continue;
        }

        const iikoCredentials = parseAccessCredentials(getEquipmentParameterValue(IIKO_CREDENTIALS_PARAMETER_CODE), false);
        if (iikoCredentials === null) {
            return null;
        }

        const serverAccess = parseServerAccess(getEquipmentParameterValue(parameterCode));
        if (serverAccess === null) {
            return null;
        }

        const wrapper = createAccessButtonsWrapper();
        const button = buildAccessIconButton(accessButton.iconPath, accessButton.iconAlt, accessButton.displayName);
        bindAccessButtonHoverState(button);
        button.addEventListener("click", () => {
            openClearbatLink(buildIikoClearbatUrl(serverAccess.address, {
                login: iikoCredentials.login,
                password: serverAccess.password || iikoCredentials.password
            }));
        });
        wrapper.appendChild(button);
        return wrapper;
    }

    return null;
}

function createAccessButtonsWrapper() {
    const wrapper = document.createElement("div");
    wrapper.className = "d-flex flex-wrap align-items-start justify-content-end gap-2 flex-shrink-0";
    return wrapper;
}

function buildAccessIconButton(iconPath, iconAlt, displayName) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "btn btn-sm p-0 border-0 bg-transparent shadow-none d-inline-flex align-items-center justify-content-center overflow-hidden rounded-1";
    button.style.width = "31px";
    button.style.height = "31px";
    button.style.minWidth = "31px";
    button.style.minHeight = "31px";
    button.title = displayName;
    button.setAttribute("aria-label", iconAlt);
    button.setAttribute("data-bs-toggle", "tooltip");
    button.setAttribute("data-bs-placement", "top");

    const icon = document.createElement("img");
    icon.src = iconPath;
    icon.alt = iconAlt;
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

function getEquipmentParameterValue(parameterCode) {
    const details = equipmentDetailsState.details;
    if (!details || !Array.isArray(details.parameters) || !parameterCode) {
        return "";
    }

    const parameter = details.parameters.find(current =>
        typeof current?.code === "string"
        && current.code === parameterCode);

    return typeof parameter?.value === "string" ? parameter.value : "";
}

function normalizeEquipmentWebLink(value) {
    const normalizedValue = String(value || "").trim();
    if (!isMeaningfulAccessValue(normalizedValue) || /[\u0400-\u04FF]/u.test(normalizedValue) || /\s/.test(normalizedValue)) {
        return "";
    }

    try {
        const url = new URL(normalizedValue);
        if ((url.protocol !== "http:" && url.protocol !== "https:") || !url.hostname || /[^\x00-\x7F]/.test(url.hostname)) {
            return "";
        }

        return url.toString();
    }
    catch {
        return "";
    }
}

function parseServerAccess(value) {
    const normalizedValue = String(value || "").trim();
    if (!isMeaningfulAccessValue(normalizedValue) || /\s/.test(normalizedValue)) {
        return null;
    }

    const parsedValue = splitServerAccessValue(normalizedValue);
    if (parsedValue === null || !isMeaningfulAccessValue(parsedValue.address)) {
        return null;
    }

    const address = ensureServerPort(parsedValue.address);
    if (!address) {
        return null;
    }

    return {
        address,
        password: parsedValue.password
    };
}

function ensureServerPort(address) {
    const normalizedAddress = String(address || "").trim();
    if (!isMeaningfulAccessValue(normalizedAddress) || /\s/.test(normalizedAddress)) {
        return "";
    }

    if (/^https?:\/\//i.test(normalizedAddress)) {
        try {
            const url = new URL(normalizedAddress);
            if (url.protocol !== "http:" && url.protocol !== "https:") {
                return "";
            }

            if (!url.hostname) {
                return "";
            }

            if (url.username || url.password || url.pathname !== "/" || url.search || url.hash) {
                return "";
            }

            if (url.port) {
                return url.toString().replace(/\/$/, "");
            }

            url.port = "443";
            return url.toString().replace(/\/$/, "");
        }
        catch {
            return "";
        }
    }

    if (normalizedAddress.includes("://") || normalizedAddress.includes("?") || normalizedAddress.includes("#")) {
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

function splitServerAccessValue(value) {
    const protocolMatch = value.match(/^https?:\/\//i);
    const searchStartIndex = protocolMatch !== null ? protocolMatch[0].length : 0;
    const separatorIndex = value.indexOf(ACCESS_VALUE_SEPARATOR, searchStartIndex);
    if (separatorIndex < 0) {
        return {
            address: value,
            password: ""
        };
    }

    const address = value.slice(0, separatorIndex);
    const password = value.slice(separatorIndex + ACCESS_VALUE_SEPARATOR.length);
    if (!address || !password) {
        return null;
    }

    return {
        address,
        password
    };
}

function isMeaningfulAccessValue(value) {
    const normalizedValue = String(value || "").trim();
    if (!normalizedValue) {
        return false;
    }

    const collapsedValue = normalizedValue.replace(/\s+/g, "").toLowerCase();
    return !INVALID_PLACEHOLDER_VALUES.has(collapsedValue);
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

function renderEquipmentDetailsContentError() {
    const container = document.getElementById("equipmentDetailsContent");
    if (!container) {
        return;
    }

    container.textContent = "";

    const message = document.createElement("div");
    message.className = "text-muted";
    message.textContent = "Не удалось отобразить данные оборудования.";
    container.appendChild(message);
}

function getDisplayText(value) {
    const normalizedValue = String(value || "").trim();
    return normalizedValue ? normalizedValue : "Не указано";
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
