const Formatters = {
    formatDate(date) {
        if (!date) return '—';
        return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
    },

    formatRole(role) {
        const map = { Admin: 'Administrator', Instructor: 'Instructor', Student: 'Student' };
        return map[role] || role || 'Unknown';
    },

    formatCredits(n) {
        const num = parseInt(n, 10);
        return isNaN(num) ? '—' : `${num} Credit${num !== 1 ? 's' : ''}`;
    },

    badgeForRole(role) {
        const classes = { Admin: 'badge-admin', Instructor: 'badge-instructor', Student: 'badge-student' };
        const cls = classes[role] || 'badge-secondary';
        return `<span class="badge ${cls}">${this.formatRole(role)}</span>`;
    }
};
