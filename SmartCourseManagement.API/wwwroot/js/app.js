/**
 * SMART COURSE MANAGEMENT - CORE APPLICATION LOGIC
 */

const app = {
    // STATE
    user: null,
    token: localStorage.getItem('token') || null,
    apiBase: (window.__API_BASE_URL__ || 'http://localhost:5202/api').replace(/\/$/, ''),

    // INITIALIZE
    init() {
        try {
            const loader = document.getElementById('loader');
            if (loader) {
                loader.classList.add('hidden');
            }
            
            if (this.token) {
                this.validateToken();
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

        try {
            const data = await this.fetchWrapper('/Auth/login', 'POST', { email, password });
            this.token = data.accessToken || data.token;
            localStorage.setItem('token', this.token);
            if (data.refreshToken) {
                localStorage.setItem('refreshToken', data.refreshToken);
            }
            localStorage.setItem('user', JSON.stringify(data.user || {}));
            this.notify('Success', 'Welcome back!');
            setTimeout(() => this.validateToken(), 500);
        } catch (err) {
            this.notify('Login Error', err.message || 'Invalid credentials', 'error');
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
            setTimeout(() => this.showPage('login'), 500);
        } catch (err) {
            this.notify('Registration Error', err.message || 'Registration failed', 'error');
        }
    },

    logout() {
        this.token = null;
        this.user = null;
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        const header = document.getElementById('main-header');
        if (header) {
            header.classList.add('hidden');
        }
        this.showPage('login');
    },

    // TOKEN VALIDATION (Simplified - extraction of user info from JWT)
    validateToken() {
        if (!this.token) {
            this.showPage('login');
            return;
        }
        
        try {
            const base64Url = this.token.split('.')[1];
            if (!base64Url) {
                throw new Error('Invalid token format');
            }

            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(c => 
                '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
            ).join(''));

            const payload = JSON.parse(jsonPayload);
            this.user = {
                id: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || '',
                name: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || 'User',
                email: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || '',
                role: payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || 'Student'
            };

            this.setupUI();
            this.showPage('dashboard');
        } catch (e) {
            console.error('Token validation error:', e);
            this.logout();
        }
    },

    // UI CONTROLS
    setupUI() {
        try {
            const header = document.getElementById('main-header');
            if (header) {
                header.classList.remove('hidden');
            }
            
            if (this.user && (this.user.role === 'Admin' || this.user.role === 'Instructor')) {
                const addCourseBtn = document.getElementById('add-course-btn');
                if (addCourseBtn) {
                    addCourseBtn.style.display = 'block';
                }
            }
        } catch (err) {
            console.error('Setup UI error:', err);
        }
    },

    showPage(pageId) {
        try {
            const loader = document.getElementById('loader');
            if (loader) {
                loader.classList.add('hidden');
            }
            
            const sections = document.querySelectorAll('section');
            sections.forEach(s => s.classList.add('hidden'));
            
            const targetPage = document.getElementById(`${pageId}-page`);
            if (targetPage) {
                targetPage.classList.remove('hidden');
            }
            
            if (pageId === 'courses') {
                this.loadCourses();
            }
            if (pageId === 'dashboard') {
                this.loadDashboardData();
            }
            if (pageId === 'profile' && this.user) {
                document.getElementById('profile-name').value = this.user.name || '';
                document.getElementById('profile-email').value = this.user.email || '';
                document.getElementById('profile-role').value = this.user.role || '';
            }
        } catch (err) {
            console.error('Error showing page:', err);
        }
    },

    // API WRAPPER
    async fetchWrapper(endpoint, method = 'GET', body = null) {
        try {
            const headers = { 'Content-Type': 'application/json' };
            if (this.token) {
                headers['Authorization'] = `Bearer ${this.token}`;
            }

            const options = { method, headers };
            if (body) options.body = JSON.stringify(body);

            let response;
            try {
                response = await fetch(`${this.apiBase}${endpoint}`, options);
            } catch (error) {
                throw new Error(`Unable to reach the API at ${this.apiBase}. Make sure the backend is running.`);
            }
            
            if (response.status === 401) {
                localStorage.removeItem('token');
                this.logout();
                throw new Error('Session expired. Please login again.');
            }

            if (response.status === 403) {
                throw new Error('You do not have permission to access this resource.');
            }

            if (response.status === 404) {
                throw new Error('Resource not found.');
            }

            if (!response.ok && response.status !== 200) {
                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || `API Error: ${response.status}`);
                } else {
                    throw new Error(`API Error: ${response.status} ${response.statusText}`);
                }
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                const data = await response.json();
                if (!response.ok) {
                    throw new Error(data.message || 'API request failed');
                }
                return data;
            } else {
                return response.ok ? { success: true } : null;
            }
        } catch (error) {
            console.error('Fetch error:', error);
            throw error;
        }
    },

    // DATA LOADING
    async loadCourses() {
        const grid = document.getElementById('courses-grid');
        if (!grid) return;
        
        grid.innerHTML = '<p>Loading courses...</p>';
        try {
            const courses = await this.fetchWrapper('/Courses');
            if (!Array.isArray(courses) || courses.length === 0) {
                grid.innerHTML = '<p class="text-muted">No courses available.</p>';
                return;
            }
            
            grid.innerHTML = courses.map(c => `
                <div class="glass-card animate-fade-in">
                    <h3>${c.title || 'Course'}</h3>
                    <p class="text-muted mb-4">${(c.description || '').substring(0, 100)}...</p>
                    <span class="badge"><i class="fas fa-graduation-cap"></i> ${c.credits || 0} Credits</span>
                    <div class="course-meta">
                        ${this.user && this.user.role === 'Student' ? 
                            `<button class="btn btn-primary btn-sm" onclick="app.enroll(${c.id})">Enroll Now</button>` : 
                            `<span class="text-muted" style="font-size: 0.85rem;">Course ID: ${c.id}</span>`}
                    </div>
                </div>
            `).join('');
        } catch (err) {
            console.error('Course load error:', err);
            grid.innerHTML = `<p class="error">Error: ${err.message}</p>`;
        }
    },

    async loadDashboardData() {
        const content = document.getElementById('dashboard-content');
        if (!content) return;
        
        content.innerHTML = '<p class="text-center"><i class="fas fa-spinner fa-spin"></i> Loading dashboard...</p>';
        try {
            const courses = await this.fetchWrapper('/Courses');
            const courseCount = Array.isArray(courses) ? courses.length : 0;
            
            const statsCoursesEl = document.getElementById('stats-courses');
            if (statsCoursesEl) {
                statsCoursesEl.innerText = `${courseCount} Courses Available`;
            }
            
            if (this.user && this.user.role === 'Student') {
                try {
                    const enrollments = await this.fetchWrapper('/Enrollments/my-enrollments');
                    const enrollmentCount = Array.isArray(enrollments) ? enrollments.length : 0;
                    
                    const statsEnrollmentsEl = document.getElementById('stats-enrollments');
                    if (statsEnrollmentsEl) {
                        statsEnrollmentsEl.innerText = `${enrollmentCount} Active Sessions`;
                    }
                    
                    content.innerHTML = `
                        <h3 class="mb-4">My Enrolled Courses</h3>
                        ${enrollmentCount === 0 ? '<p class="text-muted">You are not enrolled in any courses yet.</p>' : ''}
                        <div class="grid grid-cols-3">
                            ${Array.isArray(enrollments) ? enrollments.map(e => `
                                <div class="glass-card" style="padding: 1.5rem;">
                                    <h4>${e.courseTitle || 'Course'}</h4>
                                    <p class="text-muted mt-2" style="font-size: 0.85rem;">Enrolled on: ${new Date(e.enrollmentDate).toLocaleDateString()}</p>
                                </div>
                            `).join('') : ''}
                        </div>
                    `;
                } catch (enrollErr) {
                    console.warn('Error loading enrollments:', enrollErr);
                    content.innerHTML = '<p class="text-muted">You are not enrolled in any courses yet.</p>';
                }
            } else {
                const statsEnrollmentsEl = document.getElementById('stats-enrollments');
                if (statsEnrollmentsEl) {
                    statsEnrollmentsEl.innerText = 'Manage all students';
                }
                content.innerHTML = `<p class="text-muted">Welcome to the Instructor Dashboard. Use the Courses tab to manage your courses and students.</p>`;
            }
        } catch (err) {
            console.error('Dashboard load error:', err);
            content.innerHTML = `<p class="text-muted" style="color: var(--error-color);">Failed to load data: ${err.message}</p>`;
        }
    },

    async enroll(courseId) {
        try {
            if (!this.user || !this.user.id) {
                throw new Error('User not authenticated');
            }
            
            await this.fetchWrapper('/Enrollments', 'POST', { 
                studentId: parseInt(this.user.id), 
                courseId: courseId 
            });
            
            this.notify('Success', 'Enrolled successfully!');
            this.loadDashboardData();
        } catch (err) {
            this.notify('Enrollment Error', err.message || 'Failed to enroll', 'error');
        }
    },

    // UTILS
    notify(title, message, type = 'success') {
        try {
            const area = document.getElementById('notification-area');
            if (!area) return;
            
            const note = document.createElement('div');
            note.className = `glass-card mt-4 animate-fade-in notification ${type}`;
            note.style.borderLeft = `5px solid ${type === 'success' ? 'var(--success-color)' : 'var(--error-color)'}`;
            note.innerHTML = `<strong>${title}:</strong> ${message}`;
            area.appendChild(note);
            
            setTimeout(() => {
                if (note.parentNode) {
                    note.remove();
                }
            }, 4000);
        } catch (err) {
            console.error('Notification error:', err);
        }
    }
};

window.onload = () => app.init();
