/**
 * SMART COURSE MANAGEMENT - CORE APPLICATION LOGIC
 * Depends on: config.js, storage.js, utils.js, ui.js, api.js, auth.js
 */

const app = {
    // STATE
    get user() { return Auth.getCurrentUser(); },
    get token() { return Storage.getToken(); },

    // INITIALIZE
    init() {
        try {
            const loader = document.getElementById('loader');
            if (loader) loader.classList.add('hidden');

            if (this.token) {
                const user = this.user;
                if (user) {
                    this.setupUI();
                    this.showPage('dashboard');
                } else {
                    this.showPage('login');
                }
            } else {
                this.showPage('login');
            }
        } catch (err) {
            console.error('Initialization error:', err);
            this.showPage('login');
        }
    },

    // AUTHENTICATION
    async handleLogin(e) {
        e.preventDefault();
        const email = document.getElementById('login-email').value;
        const password = document.getElementById('login-password').value;
        const btn = e.target.querySelector('button[type=submit]');
        if (btn) btn.disabled = true;

        try {
            await Auth.login(email, password);
            UI.toast('Welcome back!', 'success');
            setTimeout(() => {
                this.setupUI();
                this.showPage('dashboard');
            }, 500);
        } catch (err) {
            UI.toast(err.message || 'Invalid credentials', 'error');
        } finally {
            if (btn) btn.disabled = false;
        }
    },

    async handleRegister(e) {
        e.preventDefault();
        const name = document.getElementById('reg-name').value;
        const email = document.getElementById('reg-email').value;
        const password = document.getElementById('reg-password').value;
        const role = document.getElementById('reg-role').value;
        const btn = e.target.querySelector('button[type=submit]');
        if (btn) btn.disabled = true;

        try {
            await Auth.register(name, email, password, role);
            UI.toast('Account created! Welcome!', 'success');
            setTimeout(() => {
                this.setupUI();
                this.showPage('dashboard');
            }, 500);
        } catch (err) {
            UI.toast(err.message || 'Registration failed', 'error');
        } finally {
            if (btn) btn.disabled = false;
        }
    },

    logout() {
        Auth.logout();
    },

    // UI CONTROLS
    setupUI() {
        try {
            const header = document.getElementById('main-header');
            if (header) header.classList.remove('hidden');

            const user = this.user;
            if (user) {
                const userNameEl = document.getElementById('user-display-name');
                if (userNameEl) userNameEl.textContent = user.name;

                const roleEl = document.getElementById('user-role-badge');
                if (roleEl) roleEl.textContent = user.role;

                if (user.role === 'Admin' || user.role === 'Instructor') {
                    const addCourseBtn = document.getElementById('add-course-btn');
                    if (addCourseBtn) addCourseBtn.style.display = 'inline-block';
                }
            }
        } catch (err) {
            console.error('Setup UI error:', err);
        }
    },

    showPage(pageId) {
        try {
            const loader = document.getElementById('loader');
            if (loader) loader.classList.add('hidden');

            document.querySelectorAll('section').forEach(s => s.classList.add('hidden'));

            const targetPage = document.getElementById(`${pageId}-page`);
            if (targetPage) targetPage.classList.remove('hidden');

            if (pageId === 'courses') this.loadCourses();
            if (pageId === 'dashboard') this.loadDashboardData();
            if (pageId === 'profile' && this.user) {
                const nameEl = document.getElementById('profile-name');
                const emailEl = document.getElementById('profile-email');
                const roleEl = document.getElementById('profile-role');
                if (nameEl) nameEl.value = this.user.name || '';
                if (emailEl) emailEl.value = this.user.email || '';
                if (roleEl) roleEl.value = this.user.role || '';
            }
        } catch (err) {
            console.error('Error showing page:', err);
        }
    },

    // DATA LOADING
    async loadCourses(page = 1, search = '') {
        const grid = document.getElementById('courses-grid');
        if (!grid) return;

        UI.showSpinner('courses-grid');
        try {
            const query = search
                ? `/courses/paged?page=${page}&pageSize=${CONFIG.PAGE_SIZE}&searchTerm=${encodeURIComponent(search)}`
                : `/courses/paged?page=${page}&pageSize=${CONFIG.PAGE_SIZE}`;

            const result = await Api.get(query);
            const courses = result.data || [];

            if (!courses.length) {
                grid.innerHTML = '<p class="text-muted">No courses found.</p>';
                return;
            }

            grid.innerHTML = courses.map(c => `
                <div class="glass-card animate-fade-in">
                    <h3>${Utils.escapeHtml(c.title || 'Course')}</h3>
                    <p class="text-muted mb-4">${Utils.escapeHtml(Utils.truncate(c.description))}</p>
                    <span class="badge"><i class="fas fa-graduation-cap"></i> ${c.credits || 0} Credits</span>
                    <p class="text-muted" style="font-size:0.8rem;margin-top:0.5rem;">
                        Instructor: ${Utils.escapeHtml(c.instructorName || 'TBA')}
                    </p>
                    <div class="course-meta mt-3">
                        ${this.user && this.user.role === 'Student'
                            ? `<button class="btn btn-primary btn-sm" onclick="app.enroll(${c.id})">Enroll Now</button>`
                            : `<span class="badge">ID: ${c.id}</span>`}
                    </div>
                </div>
            `).join('');

            if (result.totalPages > 1) {
                UI.renderPagination('pagination-container', result.page, result.totalPages,
                    (p) => app.loadCourses(p, search));
            }
        } catch (err) {
            console.error('Course load error:', err);
            grid.innerHTML = `<p class="text-muted" style="color:var(--error-color)">Error: ${Utils.escapeHtml(err.message)}</p>`;
        }
    },

    async loadDashboardData() {
        const content = document.getElementById('dashboard-content');
        if (!content) return;

        UI.showSpinner('dashboard-content');
        try {
            const coursesResult = await Api.get('/courses/paged?page=1&pageSize=3');
            const courses = coursesResult.data || [];

            if (this.user && this.user.role === 'Student') {
                let enrollments = [];
                try {
                    enrollments = await Api.get('/enrollments/my-enrollments');
                } catch { /* not enrolled yet */ }

                const statsEl = document.getElementById('stats-enrollments');
                if (statsEl) statsEl.textContent = `${(enrollments || []).length} Active Enrollments`;

                content.innerHTML = `
                    <h3 class="mb-4">My Enrolled Courses</h3>
                    ${!enrollments || !enrollments.length
                        ? '<p class="text-muted">You are not enrolled in any courses yet. <a href="#" onclick="app.showPage(\'courses\')">Browse courses</a></p>'
                        : '<div class="grid grid-cols-3">' + enrollments.map(e => `
                            <div class="glass-card">
                                <h4>${Utils.escapeHtml(e.courseTitle || 'Course')}</h4>
                                <p class="text-muted mt-2" style="font-size:0.85rem">
                                    Enrolled: ${Utils.formatDate(e.enrollmentDate)}
                                </p>
                            </div>`).join('') + '</div>'
                    }
                    <h3 class="mt-6 mb-4">Available Courses</h3>
                    <div class="grid grid-cols-3">
                        ${courses.map(c => `
                            <div class="glass-card">
                                <h4>${Utils.escapeHtml(c.title)}</h4>
                                <p class="text-muted" style="font-size:0.85rem">${c.credits} Credits</p>
                                <button class="btn btn-primary btn-sm mt-2" onclick="app.enroll(${c.id})">Enroll</button>
                            </div>`).join('')}
                    </div>`;
            } else {
                content.innerHTML = `
                    <h3 class="mb-4">Recent Courses</h3>
                    <div class="grid grid-cols-3">
                        ${courses.map(c => `
                            <div class="glass-card">
                                <h4>${Utils.escapeHtml(c.title)}</h4>
                                <p class="text-muted" style="font-size:0.85rem">${c.credits} Credits</p>
                            </div>`).join('')}
                    </div>`;
            }
        } catch (err) {
            console.error('Dashboard error:', err);
            content.innerHTML = `<p class="text-muted">Failed to load data: ${Utils.escapeHtml(err.message)}</p>`;
        }
    },

    async enroll(courseId) {
        try {
            if (!this.user || !this.user.id) throw new Error('User not authenticated');
            await Api.post('/enrollments', { studentId: parseInt(this.user.id), courseId });
            UI.toast('Enrolled successfully!', 'success');
            this.loadDashboardData();
        } catch (err) {
            UI.toast(err.message || 'Failed to enroll', 'error');
        }
    },

    // Legacy notify support
    notify(title, message, type = 'success') {
        UI.toast(`${title}: ${message}`, type);
    }
};

window.onload = () => app.init();

