/**
 * ui.js - Toast notifications, loading spinners, modal helpers
 */

const UI = {
    // Toast notifications
    toast(message, type = 'success', duration = CONFIG.TOAST_DURATION) {
        const container = this._getOrCreateToastContainer();
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;

        const icons = { success: '✓', error: '✗', warning: '⚠', info: 'ℹ' };
        toast.innerHTML = `<span class="toast-icon">${icons[type] || '•'}</span><span>${Utils.escapeHtml(message)}</span>`;

        container.appendChild(toast);
        // Trigger animation
        requestAnimationFrame(() => toast.classList.add('toast-visible'));

        setTimeout(() => {
            toast.classList.remove('toast-visible');
            toast.addEventListener('transitionend', () => toast.remove(), { once: true });
        }, duration);
    },

    _getOrCreateToastContainer() {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            document.body.appendChild(container);
        }
        return container;
    },

    // Loading spinner
    showSpinner(targetId) {
        const el = document.getElementById(targetId);
        if (el) el.innerHTML = '<div class="spinner"></div>';
    },

    // Confirm modal
    confirm(title, message) {
        return new Promise((resolve) => {
            const modal = document.createElement('div');
            modal.className = 'modal-overlay';
            modal.innerHTML = `
                <div class="modal-box glass-card">
                    <h3>${Utils.escapeHtml(title)}</h3>
                    <p>${Utils.escapeHtml(message)}</p>
                    <div class="modal-actions">
                        <button class="btn btn-outline" id="modal-cancel">Cancel</button>
                        <button class="btn btn-primary" id="modal-confirm">Confirm</button>
                    </div>
                </div>`;
            document.body.appendChild(modal);

            document.getElementById('modal-confirm').onclick = () => { modal.remove(); resolve(true); };
            document.getElementById('modal-cancel').onclick = () => { modal.remove(); resolve(false); };
        });
    },

    // Render pagination controls
    renderPagination(containerId, currentPage, totalPages, onPageChange) {
        const container = document.getElementById(containerId);
        if (!container || totalPages <= 1) {
            if (container) container.innerHTML = '';
            return;
        }

        const pages = [];
        const delta = 2;
        for (let i = Math.max(1, currentPage - delta); i <= Math.min(totalPages, currentPage + delta); i++) {
            pages.push(i);
        }

        container.innerHTML = `
            <div class="pagination">
                <button class="btn btn-sm ${currentPage === 1 ? 'btn-disabled' : 'btn-outline'}" 
                    ${currentPage === 1 ? 'disabled' : `onclick="(${onPageChange.toString()})(${currentPage - 1})"`}>
                    &laquo; Prev
                </button>
                ${pages.map(p => `
                    <button class="btn btn-sm ${p === currentPage ? 'btn-primary' : 'btn-outline'}" 
                        onclick="(${onPageChange.toString()})(${p})">${p}</button>
                `).join('')}
                <button class="btn btn-sm ${currentPage === totalPages ? 'btn-disabled' : 'btn-outline'}"
                    ${currentPage === totalPages ? 'disabled' : `onclick="(${onPageChange.toString()})(${currentPage + 1})"`}>
                    Next &raquo;
                </button>
                <span class="pagination-info">Page ${currentPage} of ${totalPages}</span>
            </div>`;
    }
};
