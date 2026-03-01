const usersShowInactiveKey = "crm_users_show_inactive_v1";

let antiForgeryToken = null;
let users = [];
let roles = [];
let isLoading = false;
let isSaving = false;
let userModal = null;
let isEditMode = false;

document.addEventListener("DOMContentLoaded", () => {
    initUsersPage();
});

async function initUsersPage() {
    antiForgeryToken = getRequestVerificationToken();

    const modalEl = document.getElementById("editUserModal");
    if (modalEl) userModal = new bootstrap.Modal(modalEl);

    const showInactive = document.getElementById("showInactiveUsers");
    if (showInactive) {
        showInactive.checked = localStorage.getItem(usersShowInactiveKey) === "1";
        showInactive.addEventListener("change", async () => {
            localStorage.setItem(usersShowInactiveKey, showInactive.checked ? "1" : "0");
            await reloadUsers();
        });
    }

    const btnOpenCreate = document.getElementById("btnOpenCreateUser");
    if (btnOpenCreate) {
        btnOpenCreate.addEventListener("click", async () => {
            if (isSaving || isLoading) return;
            clearModalError();
            await ensureRolesLoaded();
            openCreateModal();
        });
    }

    const btnSave = document.getElementById("btnSaveUser");
    if (btnSave) {
        btnSave.addEventListener("click", async () => {
            if (isSaving) return;
            await saveUser();
        });
    }

    await reloadUsers();
}

async function ensureRolesLoaded() {
    if (Array.isArray(roles) && roles.length > 0) return true;

    try {
        const resp = await sendJsonRequest("?handler=Roles", "GET", buildJsonHeaders(antiForgeryToken));
        const data = unwrapOrThrow(resp, "Ошибка загрузки ролей.");
        roles = Array.isArray(data) ? data : [];
        return true;
    } catch (e) {
        console.error(e);
        showPageError(e.message || "Ошибка загрузки ролей.");
        roles = [];
        return false;
    }
}

async function reloadUsers() {
    try {
        isLoading = true;
        clearPageMessages();

        const includeInactive = isShowInactiveOn();
        const url = `?handler=List&includeInactive=${includeInactive ? "true" : "false"}`;
        const resp = await sendJsonRequest(url, "GET", buildJsonHeaders(antiForgeryToken));
        const data = unwrapOrThrow(resp, "Ошибка загрузки пользователей.");

        users = Array.isArray(data) ? data : [];
        renderUsers();
        return true;
    } catch (e) {
        console.error(e);
        showPageError(e.message || "Ошибка загрузки пользователей.");
        return false;
    } finally {
        isLoading = false;
    }
}

function renderUsers() {
    const tbody = document.getElementById("usersRows");
    if (!tbody) return;

    tbody.textContent = "";

    if (!Array.isArray(users) || users.length === 0) {
        const tr = document.createElement("tr");
        const td = document.createElement("td");
        td.colSpan = 5;
        td.className = "text-center text-muted py-4";
        td.textContent = "Нет пользователей";
        tr.appendChild(td);
        tbody.appendChild(tr);
        return;
    }

    for (const user of users) {
        const tr = document.createElement("tr");

        tr.appendChild(buildCell(String(user.name || "")));
        tr.appendChild(buildCell(String(user.login || "")));
        tr.appendChild(buildCell(formatRoles(user.roles)));

        const tdStatus = document.createElement("td");
        tdStatus.className = "text-center";
        tdStatus.appendChild(buildStatusBadge(Boolean(user.active)));
        tr.appendChild(tdStatus);

        const tdActions = document.createElement("td");
        tdActions.className = "text-end";

        const btnEdit = document.createElement("button");
        btnEdit.type = "button";
        btnEdit.className = "btn btn-sm btn-outline-primary me-2";
        btnEdit.textContent = "Редактировать";
        btnEdit.disabled = isSaving;
        btnEdit.addEventListener("click", async () => {
            if (isSaving) return;
            clearModalError();
            const ok = await ensureRolesLoaded();
            if (!ok) return;
            openEditModal(user);
        });

        const btnToggleActive = buildToggleActiveButton(user);

        tdActions.appendChild(btnEdit);
        tdActions.appendChild(btnToggleActive);
        tr.appendChild(tdActions);

        tbody.appendChild(tr);
    }
}

function buildStatusBadge(isActive) {
    const badge = document.createElement("span");
    badge.classList.add("badge");
    badge.style.color = "#fff";

    if (isActive) {
        badge.style.backgroundColor = "#0D6EFD";
        badge.textContent = "Активен";
        return badge;
    }

    badge.style.backgroundColor = "#DC3545";
    badge.textContent = "Неактивен";
    return badge;
}

function openCreateModal() {
    isEditMode = false;
    setModalTitle("Новый пользователь");
    setModalSaveText("Создать");
    setPasswordHint("Обязателен для нового пользователя.");
    setFormValues({ id: "", name: "", login: "", password: "", roleIds: [] });
    renderRoles([]);
    if (userModal) userModal.show();
}

function openEditModal(user) {
    isEditMode = true;
    setModalTitle("Редактирование пользователя");
    setModalSaveText("Сохранить");
    setPasswordHint("Оставьте пустым, если не нужно менять пароль.");

    const roleIds = Array.isArray(user.roles) ? user.roles.map(r => String(r.id || "")) : [];
    setFormValues({
        id: String(user.id || ""),
        name: String(user.name || ""),
        login: String(user.login || ""),
        password: "",
        roleIds
    });

    renderRoles(roleIds);
    if (userModal) userModal.show();
}

function renderRoles(selectedRoleIds) {
    const box = document.getElementById("userRoles");
    if (!box) return;

    box.textContent = "";

    if (!Array.isArray(roles) || roles.length === 0) {
        const hint = document.createElement("div");
        hint.className = "text-muted small";
        hint.textContent = "Роли не найдены.";
        box.appendChild(hint);
        return;
    }

    const selected = new Set((selectedRoleIds || []).map(String));

    for (const role of roles) {
        const roleId = String(role.id || "");
        const line = document.createElement("div");
        line.className = "form-check";

        const input = document.createElement("input");
        input.className = "form-check-input";
        input.type = "checkbox";
        input.value = roleId;
        input.id = `role_${roleId}`;
        input.checked = selected.has(roleId);

        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = input.id;
        label.textContent = String(role.name || "");

        line.appendChild(input);
        line.appendChild(label);
        box.appendChild(line);
    }
}

async function saveUser() {
    const btnSave = document.getElementById("btnSaveUser");

    const userId = String(document.getElementById("userId")?.value || "").trim();
    const name = String(document.getElementById("userName")?.value || "").trim();
    const login = String(document.getElementById("userLogin")?.value || "").trim();
    const password = String(document.getElementById("userPassword")?.value || "");
    const roleIds = Array.from(document.querySelectorAll("#userRoles input[type='checkbox']:checked")).map(x => x.value);

    if (!name) {
        showModalError("Укажите имя.");
        return;
    }

    if (!login) {
        showModalError("Укажите логин.");
        return;
    }

    if (!isEditMode && !password) {
        showModalError("Укажите пароль.");
        return;
    }

    if (roleIds.length === 0) {
        showModalError("Выберите хотя бы одну роль.");
        return;
    }

    try {
        isSaving = true;
        if (btnSave) btnSave.disabled = true;
        clearModalError();
        clearPageMessages();

        const payload = isEditMode
            ? { userId, name, login, password, roleIds }
            : { name, login, password, roleIds };

        const handler = isEditMode ? "Update" : "Create";

        const resp = await sendJsonRequest(
            `?handler=${handler}`,
            "POST",
            buildJsonHeaders(antiForgeryToken),
            payload
        );

        unwrapOrThrow(resp, isEditMode ? "Ошибка обновления пользователя." : "Ошибка создания пользователя.");

        if (userModal) userModal.hide();
        isSaving = false;
        if (btnSave) btnSave.disabled = false;

        showPageSuccess(isEditMode ? "Пользователь обновлён." : "Пользователь создан.");
        await reloadUsers();
    } catch (e) {
        console.error(e);
        showModalError(e.message || (isEditMode ? "Ошибка обновления пользователя." : "Ошибка создания пользователя."));
    } finally {
        isSaving = false;
        if (btnSave) btnSave.disabled = false;
    }
}

function buildToggleActiveButton(user) {
    const btn = document.createElement("button");
    btn.type = "button";
    btn.className = user.active ? "btn btn-sm btn-outline-danger" : "btn btn-sm btn-outline-success";
    btn.textContent = user.active ? "Деактивировать" : "Восстановить";
    btn.disabled = isSaving;
    btn.addEventListener("click", async () => {
        await setUserActive(String(user.id || ""), !Boolean(user.active));
    });
    return btn;
}

async function setUserActive(userId, isActive) {
    if (!userId) return;

    const actionText = isActive ? "восстановить" : "деактивировать";
    if (!confirm(`Подтвердите действие: ${actionText} пользователя?`)) return;

    try {
        isSaving = true;
        clearPageMessages();

        const resp = await sendJsonRequest(
            "?handler=SetActive",
            "POST",
            buildJsonHeaders(antiForgeryToken),
            { userId, isActive }
        );

        unwrapOrThrow(resp, isActive ? "Ошибка восстановления пользователя." : "Ошибка деактивации пользователя.");

        isSaving = false;
        showPageSuccess(isActive ? "Пользователь восстановлен." : "Пользователь деактивирован.");
        await reloadUsers();
    } catch (e) {
        console.error(e);
        showPageError(e.message || (isActive ? "Ошибка восстановления пользователя." : "Ошибка деактивации пользователя."));
    } finally {
        isSaving = false;
    }
}

function buildCell(text) {
    const td = document.createElement("td");
    td.textContent = text;
    return td;
}

function formatRoles(items) {
    if (!Array.isArray(items) || items.length === 0) return "-";

    const names = items
        .map(x => String(x.name || "").trim())
        .filter(x => x.length > 0);

    return names.length > 0 ? names.join(", ") : "-";
}

function setFormValues(values) {
    const id = document.getElementById("userId");
    const name = document.getElementById("userName");
    const login = document.getElementById("userLogin");
    const password = document.getElementById("userPassword");

    if (id) id.value = values.id || "";
    if (name) name.value = values.name || "";
    if (login) login.value = values.login || "";
    if (password) password.value = values.password || "";
}

function setModalTitle(text) {
    const el = document.getElementById("userModalTitle");
    if (el) el.textContent = text;
}

function setModalSaveText(text) {
    const el = document.getElementById("btnSaveUser");
    if (el) el.textContent = text;
}

function setPasswordHint(text) {
    const el = document.getElementById("userPasswordHint");
    if (el) el.textContent = text;
}

function isShowInactiveOn() {
    const cb = document.getElementById("showInactiveUsers");
    return !!cb && cb.checked;
}

function clearPageMessages() {
    const err = document.getElementById("pageError");
    const ok = document.getElementById("pageSuccess");

    if (err) {
        err.textContent = "";
        err.classList.add("d-none");
    }

    if (ok) {
        ok.textContent = "";
        ok.classList.add("d-none");
    }
}

function showPageError(message) {
    const el = document.getElementById("pageError");
    if (!el) return;
    el.textContent = message;
    el.classList.remove("d-none");
}

function showPageSuccess(message) {
    const el = document.getElementById("pageSuccess");
    if (!el) return;
    el.textContent = message;
    el.classList.remove("d-none");
}

function clearModalError() {
    const el = document.getElementById("userModalError");
    if (!el) return;
    el.textContent = "";
    el.classList.add("d-none");
}

function showModalError(message) {
    const el = document.getElementById("userModalError");
    if (!el) return;
    el.textContent = message;
    el.classList.remove("d-none");
}
