const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
const ALLOWED_EXTENSIONS = [".jpg", ".jpeg", ".png", ".webp"];

let antiForgeryToken = null;
let isBusy = false;
let previewModal = null;
let currentBackground = null;

document.addEventListener("DOMContentLoaded", () => {
    antiForgeryToken = getRequestVerificationToken();

    initSettingsPage();
});

function initSettingsPage() {
    initPreviewModal();
    initButtons();
    initUploadZone();
    loadReportBackgrounds();
}

function initPreviewModal() {
    const modalElement = document.getElementById("backgroundPreviewModal");
    if (!modalElement || !window.bootstrap) {
        return;
    }

    previewModal = new bootstrap.Modal(modalElement);
}

function initButtons() {
    const reloadButton = document.getElementById("reloadBackgroundsButton");
    const fileDialogButton = document.getElementById("openFileDialogButton");
    const fileInput = document.getElementById("backgroundFileInput");

    if (reloadButton) {
        reloadButton.addEventListener("click", async () => {
            if (isBusy) {
                return;
            }

            await loadReportBackgrounds();
        });
    }

    if (fileDialogButton && fileInput) {
        fileDialogButton.addEventListener("click", () => {
            if (isBusy) {
                return;
            }

            fileInput.click();
        });
    }

    if (fileInput) {
        fileInput.addEventListener("change", async () => {
            if (isBusy || !fileInput.files || fileInput.files.length === 0) {
                return;
            }

            try {
                await uploadFileList(Array.from(fileInput.files));
            }
            finally {
                fileInput.value = "";
            }
        });
    }
}

function initUploadZone() {
    const dropZone = document.getElementById("uploadDropZone");
    if (!dropZone) {
        return;
    }

    const enterDragState = () => {
        dropZone.classList.add("border-primary", "bg-primary-subtle");
        dropZone.classList.remove("border-secondary-subtle", "bg-light-subtle");
    };

    const leaveDragState = () => {
        dropZone.classList.remove("border-primary", "bg-primary-subtle");
        dropZone.classList.add("border-secondary-subtle", "bg-light-subtle");
    };

    ["dragenter", "dragover"].forEach((eventName) => {
        dropZone.addEventListener(eventName, (event) => {
            event.preventDefault();
            if (isBusy) {
                return;
            }

            enterDragState();
        });
    });

    ["dragleave", "dragend", "drop"].forEach((eventName) => {
        dropZone.addEventListener(eventName, (event) => {
            event.preventDefault();
            leaveDragState();
        });
    });

    dropZone.addEventListener("drop", async (event) => {
        if (isBusy) {
            return;
        }

        const files = Array.from(event.dataTransfer?.files || []);
        if (files.length === 0) {
            return;
        }

        await uploadFileList(files);
    });
}

async function loadReportBackgrounds() {
    if (isBusy) {
        return;
    }

    clearMessages();
    setBusy(true, "Загрузка текущего фона...");

    try {
        const response = await sendJsonRequest("?handler=ReportBackgrounds", "GET", buildJsonHeaders(antiForgeryToken));
        const data = unwrapOrThrow(response, "Ошибка загрузки фона.");

        currentBackground = data || null;

        renderReportBackgrounds();
        setUploadStatus("");
    }
    catch (error) {
        showPageError(error?.message || "Ошибка загрузки фона.");
    }
    finally {
        setBusy(false);
    }
}

async function uploadFileList(files) {
    if (!Array.isArray(files) || files.length === 0) {
        return;
    }

    const file = validateSingleFileSelection(files);

    clearMessages();
    setBusy(true, "Загрузка фона...");

    try {
        await uploadSingleFile(file);
        await reloadReportBackgroundsAfterAction("");
    }
    catch (error) {
        showPageError(error?.message || "Ошибка загрузки фона.");
    }
    finally {
        setBusy(false);
    }
}

async function uploadSingleFile(file) {
    validateFileOrThrow(file);

    const formData = new FormData();
    formData.append("file", file);

    const response = await sendJsonRequest(
        "?handler=UploadReportBackground",
        "POST",
        buildMultipartHeaders(antiForgeryToken),
        formData
    );

    unwrapOrThrow(response, "Ошибка загрузки фона.");
}

async function deleteBackground() {
    clearMessages();
    setBusy(true, "Удаление фона...");

    try {
        const response = await sendJsonRequest(
            "?handler=DeleteReportBackground",
            "POST",
            buildJsonHeaders(antiForgeryToken)
        );

        unwrapOrThrow(response, "Ошибка удаления фона.");
        showPageSuccess("Фон удалён.");
        await reloadReportBackgroundsAfterAction("Фон удалён.");
    }
    catch (error) {
        showPageError(error?.message || "Ошибка удаления фона.");
    }
    finally {
        setBusy(false);
    }
}

async function reloadReportBackgroundsAfterAction(statusText) {
    const response = await sendJsonRequest("?handler=ReportBackgrounds", "GET", buildJsonHeaders(antiForgeryToken));
    const data = unwrapOrThrow(response, "Ошибка обновления данных о фоне.");

    currentBackground = data || null;

    renderReportBackgrounds();
    setUploadStatus(statusText);
}

function renderReportBackgrounds() {
    renderCurrentBackgroundCard();
}

function renderCurrentBackgroundCard() {
    const host = document.getElementById("currentBackgroundCard");
    if (!host) {
        return;
    }

    host.textContent = "";

    if (!currentBackground) {
        const emptyState = document.createElement("div");
        emptyState.className = "rounded-4 border border-secondary-subtle bg-light-subtle d-flex align-items-center justify-content-center text-muted";
        emptyState.style.minHeight = "320px";
        emptyState.textContent = "Фон пока не загружен.";
        host.appendChild(emptyState);
        return;
    }

    const preview = buildImagePreview(currentBackground, "Текущий фон");
    const meta = buildImageMeta(currentBackground);
    const actions = buildCurrentBackgroundActions(currentBackground);

    host.append(preview, meta, actions);
}

function buildImagePreview(item, altText) {
    const wrap = document.createElement("div");
    wrap.className = "rounded-4 overflow-hidden bg-light-subtle border border-secondary-subtle";

    const image = document.createElement("img");
    image.src = buildBackgroundImageUrl();
    image.alt = String(altText || "");
    image.className = "w-100 d-block";
    image.style.height = "320px";
    image.style.objectFit = "cover";
    image.style.cursor = "zoom-in";
    image.addEventListener("click", () => {
        openPreviewModal(item);
    });

    wrap.appendChild(image);
    return wrap;
}

function buildCurrentBackgroundActions(item) {
    const wrap = document.createElement("div");
    wrap.className = "d-flex gap-2 flex-wrap";

    const openButton = document.createElement("button");
    openButton.type = "button";
    openButton.className = "btn btn-outline-secondary";
    openButton.textContent = "Открыть полностью";
    openButton.addEventListener("click", () => {
        openPreviewModal(item);
    });

    const deleteButton = document.createElement("button");
    deleteButton.type = "button";
    deleteButton.className = "btn btn-outline-danger";
    deleteButton.textContent = "Удалить";
    deleteButton.disabled = isBusy;
    deleteButton.addEventListener("click", async () => {
        if (isBusy) {
            return;
        }

        await deleteBackground();
    });

    wrap.append(openButton, deleteButton);
    return wrap;
}

function buildImageMeta(item) {
    const wrap = document.createElement("div");
    wrap.className = "d-flex flex-column gap-1";

    const title = document.createElement("div");
    title.className = "fw-semibold text-break";
    title.textContent = String(item.displayName || "Без названия");

    const fileSize = document.createElement("div");
    fileSize.className = "small text-muted";
    fileSize.textContent = `Размер: ${formatSize(item.sizeBytes)}`;

    const uploaded = document.createElement("div");
    uploaded.className = "small text-muted";
    uploaded.textContent = `Загружено: ${formatUtcDate(item.uploadedAtUtc)}`;

    wrap.append(title, fileSize, uploaded);
    return wrap;
}

function openPreviewModal(item) {
    if (!item) {
        return;
    }

    const image = document.getElementById("backgroundPreviewImage");
    const title = document.getElementById("backgroundPreviewTitle");

    if (image) {
        image.src = buildBackgroundImageUrl();
        image.alt = String(item.displayName || "Просмотр изображения");
    }

    if (title) {
        title.textContent = String(item.displayName || "Просмотр изображения");
    }

    if (previewModal) {
        previewModal.show();
    }
}

function validateSingleFileSelection(files) {
    if (files.length !== 1) {
        throw new Error("Можно загрузить только один файл.");
    }

    return files[0];
}

function validateFileOrThrow(file) {
    if (!file) {
        throw new Error("Файл не выбран.");
    }

    if (file.size > MAX_FILE_SIZE_BYTES) {
        throw new Error(`Файл "${file.name}" превышает лимит 10 МБ.`);
    }

    const lowerName = String(file.name || "").toLowerCase();
    const isAllowed = ALLOWED_EXTENSIONS.some((extension) => lowerName.endsWith(extension));
    if (!isAllowed) {
        throw new Error(`Файл "${file.name}" имеет неподдерживаемый формат.`);
    }
}

function buildBackgroundImageUrl() {
    if (!currentBackground?.uploadedAtUtc) {
        return "?handler=ReportBackgroundFile";
    }

    const version = encodeURIComponent(currentBackground.uploadedAtUtc);
    return `?handler=ReportBackgroundFile&v=${version}`;
}

function buildMultipartHeaders(requestVerificationToken) {
    const headers = {
        "Accept": "application/json"
    };

    if (requestVerificationToken) {
        headers.RequestVerificationToken = requestVerificationToken;
    }

    return headers;
}

function setBusy(value, statusText = "") {
    isBusy = value === true;

    const reloadButton = document.getElementById("reloadBackgroundsButton");
    const fileDialogButton = document.getElementById("openFileDialogButton");
    const fileInput = document.getElementById("backgroundFileInput");

    if (reloadButton) {
        reloadButton.disabled = isBusy;
    }

    if (fileDialogButton) {
        fileDialogButton.disabled = isBusy;
    }

    if (fileInput) {
        fileInput.disabled = isBusy;
    }

    if (statusText) {
        setUploadStatus(statusText);
    }

    renderReportBackgrounds();
}

function setUploadStatus(message) {
    const status = document.getElementById("uploadStatus");
    if (!status) {
        return;
    }

    status.textContent = String(message || "");
}

function showPageError(message) {
    const error = document.getElementById("pageError");
    if (!error) {
        return;
    }

    error.textContent = String(message || "Ошибка.");
    error.classList.remove("d-none");
}

function showPageSuccess(message) {
    const success = document.getElementById("pageSuccess");
    if (!success) {
        return;
    }

    success.textContent = String(message || "");
    success.classList.remove("d-none");
}

function clearMessages() {
    const error = document.getElementById("pageError");
    const success = document.getElementById("pageSuccess");

    if (error) {
        error.textContent = "";
        error.classList.add("d-none");
    }

    if (success) {
        success.textContent = "";
        success.classList.add("d-none");
    }
}

function formatSize(sizeBytes) {
    const size = Number(sizeBytes || 0);
    if (size < 1024) {
        return `${size} Б`;
    }

    if (size < 1024 * 1024) {
        return `${(size / 1024).toFixed(1)} КБ`;
    }

    return `${(size / (1024 * 1024)).toFixed(2)} МБ`;
}

function formatUtcDate(value) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return "Неизвестно";
    }

    return date.toLocaleString("ru-RU", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
    });
}
