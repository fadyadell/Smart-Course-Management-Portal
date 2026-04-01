/**
 * auth.js - Login, register, logout, and token management
 */

const Auth = {
    async login(email, password) {
        const data = await Api.post('/auth/login', { email, password });
        this._saveSession(data);
        return data;
    },

    async register(name, email, password, role) {
        const data = await Api.post('/auth/register', { name, email, password, role });
        this._saveSession(data);
        return data;
    },

    logout() {
        Storage.clearAuth();
        window.location.href = '/login.html';
    },

    isAuthenticated() {
        const token = Storage.getToken();
        return !!token;
    },

    getCurrentUser() {
        const user = Storage.getUser();
        if (user) return user;

        const token = Storage.getToken();
        if (!token) return null;

        const decoded = Utils.getUserFromToken(token);
        if (decoded) Storage.setUser(decoded);
        return decoded;
    },

    requireAuth(allowedRoles = []) {
        if (!this.isAuthenticated()) {
            window.location.href = '/login.html';
            return null;
        }
        const user = this.getCurrentUser();
        if (allowedRoles.length > 0 && !allowedRoles.includes(user?.role)) {
            window.location.href = '/dashboard.html';
            return null;
        }
        return user;
    },

    _saveSession(data) {
        if (!data || !data.token) throw new Error('Invalid auth response');
        Storage.setToken(data.token);
        if (data.refreshToken) Storage.setRefreshToken(data.refreshToken);
        if (data.user) {
            Storage.setUser(data.user);
        } else {
            const decoded = Utils.getUserFromToken(data.token);
            if (decoded) Storage.setUser(decoded);
        }
    }
};
