/**
 * storage.js - Secure localStorage/sessionStorage management
 */

const Storage = {
    get(key) {
        try {
            const val = localStorage.getItem(key);
            return val ? JSON.parse(val) : null;
        } catch {
            return localStorage.getItem(key);
        }
    },

    set(key, value) {
        try {
            localStorage.setItem(key, typeof value === 'string' ? value : JSON.stringify(value));
        } catch (e) {
            console.warn('Storage set error:', e);
        }
    },

    remove(key) {
        localStorage.removeItem(key);
    },

    getToken() {
        return localStorage.getItem(CONFIG.TOKEN_KEY);
    },

    setToken(token) {
        localStorage.setItem(CONFIG.TOKEN_KEY, token);
    },

    getRefreshToken() {
        return localStorage.getItem(CONFIG.REFRESH_TOKEN_KEY);
    },

    setRefreshToken(token) {
        localStorage.setItem(CONFIG.REFRESH_TOKEN_KEY, token);
    },

    getUser() {
        try {
            const val = localStorage.getItem(CONFIG.USER_KEY);
            return val ? JSON.parse(val) : null;
        } catch {
            return null;
        }
    },

    setUser(user) {
        localStorage.setItem(CONFIG.USER_KEY, JSON.stringify(user));
    },

    clearAuth() {
        localStorage.removeItem(CONFIG.TOKEN_KEY);
        localStorage.removeItem(CONFIG.REFRESH_TOKEN_KEY);
        localStorage.removeItem(CONFIG.USER_KEY);
    }
};
