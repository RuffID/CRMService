async function sendPostJson(url, body, requestVerificationToken) {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'RequestVerificationToken': requestVerificationToken
        },
        credentials: 'same-origin',
        body: JSON.stringify(body)
    });

    if (!response.ok) {
        throw new Error(`Error sending post json request. HTTP: ${response.status}`);
    }

    return await response.json();
}