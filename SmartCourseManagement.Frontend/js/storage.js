const Storage = {
    setUser(user) {
        localStorage.setItem(CONFIG.USER_KEY, JSON.stringify(user));
    },

    getUser() {
        const raw = localStorage.getItem(CONFIG.USER_KEY);
        try { return raw ? JSON.parse(raw) : null; } catch { return null; }
    },

    setTokens(jwt, refresh) {
        localStorage.setItem(CONFIG.TOKEN_KEY, jwt);
        if (refresh) localStorage.setItem(CONFIG.REFRESH_TOKEN_KEY, refresh);
    },

    getToken() {
        return localStorage.getItem(CONFIG.TOKEN_KEY);
    },

    getRefreshToken() {
        return localStorage.getItem(CONFIG.REFRESH_TOKEN_KEY);
    },

    clearAll() {
        localStorage.removeItem(CONFIG.TOKEN_KEY);
        localStorage.removeItem(CONFIG.REFRESH_TOKEN_KEY);
        localStorage.removeItem(CONFIG.USER_KEY);
    }
};
