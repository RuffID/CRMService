async function sendJsonRequest(url, method = "GET", headers = {}, body = null) {
    const options = {
        method: method,
        headers: headers
    };

    if (body !== null) {
        if (typeof body === "object" && !(body instanceof FormData)) {
            options.body = JSON.stringify(body);

            if (!options.headers["Content-Type"]) {
                options.headers["Content-Type"] = "application/json";
            }
        } else {
            options.body = body;
        }
    }

    let response = await fetch(url, options);

    if (!response.ok) {
        let errorText = await response.text();
        let message;

        try {
            const parsed = JSON.parse(errorText);
            message = (parsed && parsed.message) ? parsed.message : errorText;
        }
        catch {
            message = errorText || ("HTTP error " + response.status);
        }

        throw new Error(message);
    }

    let text = await response.text();
    if (!text || text.trim() === "") return {};

    try {
        return JSON.parse(text);
    }
    catch (error) {
        console.error("Failed to parse JSON", error);
        throw error;
    }
}

function buildJsonHeaders(antiforgeryToken) {
    let headers = {
        "Accept": "application/json",
        "Content-Type": "application/json"
    };

    if (antiforgeryToken) {
        headers["RequestVerificationToken"] = antiforgeryToken;
    }

    return headers;
}

function unwrapServiceResult(resp) {
    if (resp && typeof resp === 'object' && ('success' in resp)) {
        const success = (resp.success) === true;
        const data = resp.data;
        const message = resp.message ?? '';
        return { success, data, message, raw: resp };
    }

    return { success: true, data: resp, message: '', raw: resp };
}

function unwrapOrThrow(resp, defaultMessage) {
    const r = unwrapServiceResult(resp);
    if (!r.success) {
        throw new Error(r.message || defaultMessage || 'Ошибка операции.');
    }
    return r.data;
}