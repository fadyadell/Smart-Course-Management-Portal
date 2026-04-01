const Validators = {
    validateEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email).trim());
    },

    validatePassword(password) {
        return typeof password === 'string' && password.length >= 6;
    },

    validateName(name) {
        if (typeof name !== 'string') return false;
        const trimmed = name.trim();
        return trimmed.length >= 2 && trimmed.length <= 100;
    },

    showFieldError(inputId, message) {
        const input = document.getElementById(inputId);
        if (!input) return;
        input.classList.add('is-invalid');
        input.classList.remove('is-valid');
        let feedback = input.parentElement.querySelector('.field-error');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'field-error';
            input.parentElement.appendChild(feedback);
        }
        feedback.textContent = message;
    },

    clearFieldError(inputId) {
        const input = document.getElementById(inputId);
        if (!input) return;
        input.classList.remove('is-invalid');
        const feedback = input.parentElement.querySelector('.field-error');
        if (feedback) feedback.textContent = '';
    },

    clearAllErrors(formId) {
        const form = document.getElementById(formId);
        if (!form) return;
        form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
        form.querySelectorAll('.field-error').forEach(el => (el.textContent = ''));
    }
};
