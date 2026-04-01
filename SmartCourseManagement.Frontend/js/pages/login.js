(async () => {
    // Pre-fill email from sessionStorage (redirect from register)
    const savedEmail = sessionStorage.getItem('register_email');
    if (savedEmail) {
        const emailInput = document.getElementById('email');
        if (emailInput) emailInput.value = savedEmail;
        sessionStorage.removeItem('register_email');
    }

    const form = document.getElementById('login-form');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        Validators.clearAllErrors('login-form');

        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value;

        let valid = true;
        if (!Validators.validateEmail(email)) {
            Validators.showFieldError('email', 'Enter a valid email address.');
            valid = false;
        }
        if (!password) {
            Validators.showFieldError('password', 'Password is required.');
            valid = false;
        }
        if (!valid) return;

        showLoading();
        try {
            const data = await api.auth.login(email, password);
            showToast('Login successful! Redirecting...', 'success');
            setTimeout(() => navigateTo('dashboard.html'), 800);
        } catch (err) {
            hideLoading();
            showToast(err.message || 'Invalid email or password.', 'error');
            Validators.showFieldError('password', 'Invalid credentials.');
        }
    });
})();
