/**
 * SMART COURSE MANAGEMENT - CORE APPLICATION LOGIC
 */

const app = {
    // STATE
    user: null,
    token: localStorage.getItem('token') || null,
    apiBase: '/api',

    // INITIALIZE
    init() {
        console.log("App Initializing...");
        if (this.token) {
            this.validateToken();
        } else {
            this.showPage('login');
        }
    },

    // AUTHENTICATION
    async handleLogin(e) {
        e.preventDefault();
        const email = document.getElementById('login-email').value;
        const password = document.getElementById('login-password').value;

        try {
            const data = await this.fetchWrapper('/Auth/login', 'POST', { email, password });
            this.token = data.token;
            localStorage.setItem('token', this.token);
            this.notify('Success', 'Welcome back!');
            this.validateToken();
        } catch (err) {
            this.notify('Error', err.message, 'error');
        }
    },

    async handleRegister(e) {
        e.preventDefault();
        const name = document.getElementById('reg-name').value;
        const email = document.getElementById('reg-email').value;
        const password = document.getElementById('reg-password').value;
        const role = document.getElementById('reg-role').value;

        try {
            await this.fetchWrapper('/Auth/register', 'POST', { name, email, password, role });
            this.notify('Success', 'Account created! Please login.');
            this.showPage('login');
        } catch (err) {
            this.notify('Error', err.message, 'error');
        }
    },

    logout() {
        this.token = null;
        this.user = null;
        localStorage.removeItem('token');
        this.showPage('login');
        document.getElementById('main-header').classList.add('hidden');
    },

    // TOKEN VALIDATION (Simplified - extraction of user info from JWT)
    validateToken() {
        if (!this.token) return;
        
        try {
            const base64Url = this.token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(c => 
                '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
            ).join(''));

            const payload = JSON.parse(jsonPayload);
            this.user = {
                id: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
                name: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
                role: payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
            };

            this.setupUI();
            this.showPage('dashboard');
        } catch (e) {
            this.logout();
        }
    },

    // UI CONTROLS
    setupUI() {
        document.getElementById('main-header').classList.remove('hidden');
        if (this.user.role === 'Admin' || this.user.role === 'Instructor') {
            document.getElementById('add-course-btn').style.display = 'block';
        }
    },

    showPage(pageId) {
        const sections = document.querySelectorAll('section');
        sections.forEach(s => s.classList.add('hidden'));
        document.getElementById(`${pageId}-page`).classList.remove('hidden');
        
        if (pageId === 'courses') this.loadCourses();
        if (pageId === 'dashboard') this.loadDashboardData();
    },

    // API WRAPPER
    async fetchWrapper(endpoint, method = 'GET', body = null) {
        const headers = { 'Content-Type': 'application/json' };
        if (this.token) headers['Authorization'] = `Bearer ${this.token}`;

        const options = { method, headers };
        if (body) options.body = JSON.stringify(body);

        const response = await fetch(`${this.apiBase}${endpoint}`, options);
        
        if (response.status === 401) {
            this.notify('Auth Required', 'Please login again.', 'error');
            this.logout();
            throw new Error('Unauthorized');
        }

        const data = await response.json();
        if (!response.ok) throw new Error(data.message || 'API request failed');
        return data;
    },

    // DATA LOADING
    async loadCourses() {
        const grid = document.getElementById('courses-grid');
        grid.innerHTML = '<p>Loading courses...</p>';
        try {
            const courses = await this.fetchWrapper('/Courses');
            grid.innerHTML = courses.map(c => `
                <div class="glass-card animate-fade-in">
                    <h3>${c.title}</h3>
                    <p class="text-muted">${c.description.substring(0, 100)}...</p>
                    <div class="mt-4" style="display:flex; justify-content: space-between; align-items: center;">
                        <span><i class="fas fa-graduation-cap"></i> ${c.credits} Credits</span>
                        ${this.user.role === 'Student' ? 
                            `<button class="btn btn-primary btn-sm" onclick="app.enroll(${c.id})">Enroll</button>` : 
                            `<span class="text-muted">ID: ${c.id}</span>`}
                    </div>
                </div>
            `).join('');
        } catch (err) {
            grid.innerHTML = `<p class="error">Error: ${err.message}</p>`;
        }
    },

    async loadDashboardData() {
        // Mocking dashboard stats - in real app fetch from /api/Stats
        document.getElementById('stats-courses').innerText = '12 Courses Available';
        document.getElementById('stats-enrollments').innerText = '4 Active Sessions';
    },

    async enroll(courseId) {
        try {
            await this.fetchWrapper('/Enrollments', 'POST', { courseId });
            this.notify('Success', 'Enrolled successfully!');
            this.loadDashboardData();
        } catch (err) {
            this.notify('Enrollment Error', err.message, 'error');
        }
    },

    // UTILS
    notify(title, message, type = 'success') {
        const area = document.getElementById('notification-area');
        const note = document.createElement('div');
        note.className = `glass-card mt-4 animate-fade-in notification ${type}`;
        note.style.borderLeft = `5px solid ${type === 'success' ? 'var(--success-color)' : 'var(--error-color)'}`;
        note.innerHTML = `<strong>${title}:</strong> ${message}`;
        area.appendChild(note);
        setTimeout(() => note.remove(), 4000);
    }
};

window.onload = () => app.init();
