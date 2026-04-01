/**
 * utils.js - Helper functions (formatDate, truncate, debounce, etc.)
 */

const Utils = {
    formatDate(dateStr) {
        if (!dateStr) return 'N/A';
        return new Date(dateStr).toLocaleDateString('en-US', {
            year: 'numeric', month: 'short', day: 'numeric'
        });
    },

    truncate(str, len = 100) {
        if (!str) return '';
        return str.length > len ? str.substring(0, len) + '...' : str;
    },

    debounce(fn, delay = CONFIG.DEBOUNCE_DELAY) {
        let timer;
        return function (...args) {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), delay);
        };
    },

    decodeJwt(token) {
        try {
            const base64Url = token.split('.')[1];
            if (!base64Url) return null;
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(
                atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
            );
            return JSON.parse(jsonPayload);
        } catch {
            return null;
        }
    },

    isTokenExpired(token) {
        const payload = this.decodeJwt(token);
        if (!payload || !payload.exp) return true;
        return Date.now() >= payload.exp * 1000;
    },

    getUserFromToken(token) {
        const payload = this.decodeJwt(token);
        if (!payload) return null;
        return {
            id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '',
            name: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User',
            email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || '',
            role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Student'
        };
    },

    escapeHtml(str) {
        if (!str) return '';
        return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
                  .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
    },

    getQueryParam(name) {
        const params = new URLSearchParams(window.location.search);
        return params.get(name);
    }
};
