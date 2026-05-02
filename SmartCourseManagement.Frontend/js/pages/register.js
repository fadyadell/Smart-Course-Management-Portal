(async () => {
    const form = document.getElementById('register-form');
    const passwordInput = document.getElementById('password');
    const strengthBar = document.getElementById('strength-bar');
    const strengthText = document.getElementById('strength-text');

    function getStrength(pwd) {
        let score = 0;
        if (pwd.length >= 6) score++;
        if (pwd.length >= 10) score++;
        if (/[A-Z]/.test(pwd)) score++;
        if (/[0-9]/.test(pwd)) score++;
        if (/[^A-Za-z0-9]/.test(pwd)) score++;
        return score;
    }

    if (passwordInput && strengthBar) {
        passwordInput.addEventListener('input', () => {
            const score = getStrength(passwordInput.value);
            const levels = ['', 'Very Weak', 'Weak', 'Fair', 'Strong', 'Very Strong'];
            const colors = ['', '#ef4444', '#f97316', '#f59e0b', '#22c55e', '#16a34a'];
            const pct = (score / 5) * 100;
            strengthBar.style.width = pct + '%';
            strengthBar.style.backgroundColor = colors[score] || '';
            if (strengthText) strengthText.textContent = score > 0 ? levels[score] : '';
        });
    }

    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        Validators.clearAllErrors('register-form');

        const name = document.getElementById('name').value.trim();
        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirm-password').value;
        const roleEl = form.querySelector('input[name="role"]:checked');
        const role = roleEl ? roleEl.value : '';

        let valid = true;
        if (!Validators.validateName(name)) {
            Validators.showFieldError('name', 'Name must be 2–100 characters.');
            valid = false;
        }
        if (!Validators.validateEmail(email)) {
            Validators.showFieldError('email', 'Enter a valid email address.');
            valid = false;
        }
        if (!Validators.validatePassword(password)) {
            Validators.showFieldError('password', 'Password must be at least 6 characters.');
            valid = false;
        }
        if (password !== confirmPassword) {
            Validators.showFieldError('confirm-password', 'Passwords do not match.');
            valid = false;
        }
        if (!role) {
            showToast('Please select a role.', 'warning');
            valid = false;
        }
        if (!valid) return;

        showLoading();
        try {
            await api.auth.register(name, email, password, role);
            hideLoading();
            showToast('Account created! Please log in.', 'success');
            sessionStorage.setItem('register_email', email);
            setTimeout(() => navigateTo('login.html'), 1000);
        } catch (err) {
            hideLoading();
            showToast(err.message || 'Registration failed.', 'error');
        }
    });
})();
