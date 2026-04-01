/**
 * api.js - Fetch wrapper with automatic JWT attachment and refresh token handling
 */

const Api = {
    _isRefreshing: false,
    _pendingRequests: [],

    async request(endpoint, method = 'GET', body = null, isRetry = false) {
        const token = Storage.getToken();
        const headers = { 'Content-Type': 'application/json' };

        if (token) {
            // Auto-refresh if token is expired (and we have a refresh token)
            if (Utils.isTokenExpired(token) && !isRetry) {
                const refreshed = await this._tryRefresh();
                if (!refreshed) {
                    Auth.logout();
                    return;
                }
                headers['Authorization'] = `Bearer ${Storage.getToken()}`;
            } else {
                headers['Authorization'] = `Bearer ${token}`;
            }
        }

        const options = { method, headers };
        if (body) options.body = JSON.stringify(body);

        const response = await fetch(`${CONFIG.API_BASE}${endpoint}`, options);

        // Handle 401: try refresh once
        if (response.status === 401 && !isRetry) {
            const refreshed = await this._tryRefresh();
            if (refreshed) {
                return this.request(endpoint, method, body, true);
            }
            Auth.logout();
            throw new Error('Session expired. Please login again.');
        }

        if (response.status === 403) {
            throw new Error('You do not have permission to perform this action.');
        }

        if (response.status === 429) {
            throw new Error('Too many requests. Please slow down and try again.');
        }

        if (!response.ok) {
            let msg = `Request failed (${response.status})`;
            try {
                const errData = await response.json();
                msg = errData.message || msg;
            } catch { /* ignore parse errors */ }
            throw new Error(msg);
        }

        // No content (204)
        if (response.status === 204) return null;

        const contentType = response.headers.get('content-type') || '';
        if (contentType.includes('application/json')) {
            return response.json();
        }
        return null;
    },

    async _tryRefresh() {
        const refreshToken = Storage.getRefreshToken();
        if (!refreshToken) return false;

        try {
            const data = await this.request('/auth/refresh', 'POST', { refreshToken }, true);
            if (data && data.token) {
                Storage.setToken(data.token);
                Storage.setRefreshToken(data.refreshToken);
                if (data.user) Storage.setUser(data.user);
                return true;
            }
        } catch {
            Storage.clearAuth();
        }
        return false;
    },

    get(endpoint) { return this.request(endpoint, 'GET'); },
    post(endpoint, body) { return this.request(endpoint, 'POST', body); },
    put(endpoint, body) { return this.request(endpoint, 'PUT', body); },
    delete(endpoint) { return this.request(endpoint, 'DELETE'); },
};
