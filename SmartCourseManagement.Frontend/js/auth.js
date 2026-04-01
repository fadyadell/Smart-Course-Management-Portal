const Auth = {
    isAuthenticated() {
        return !!Storage.getToken();
    },

    getCurrentUser() {
        return Storage.getUser();
    },

    getUserRole() {
        const user = Storage.getUser();
        return user ? user.role : null;
    },

    logout() {
        Storage.clearAll();
        window.location.href = 'login.html';
    },

    requireAuth() {
        if (!this.isAuthenticated()) {
            window.location.href = 'login.html';
            return false;
        }
        return true;
    },

    requireRole(roles) {
        if (!this.requireAuth()) return false;
        const role = this.getUserRole();
        const allowed = Array.isArray(roles) ? roles : [roles];
        if (!allowed.includes(role)) {
            window.location.href = 'dashboard.html';
            return false;
        }
        return true;
    }
};
