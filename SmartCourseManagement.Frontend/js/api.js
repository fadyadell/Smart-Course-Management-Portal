const API_URL = (window.__API_BASE_URL__ || 'http://localhost:5202/api').replace(/\/$/, '');

const api = {
    async request(endpoint, options = {}) {
        const token = localStorage.getItem('token');
        const headers = {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` }),
            ...options.headers
        };

        let response;
        try {
            response = await fetch(`${API_URL}${endpoint}`, {
                ...options,
                headers
            });
        } catch (error) {
            throw new Error(`Unable to reach the API at ${API_URL}. Make sure the backend is running.`);
        }

        if (response.status === 401) {
            localStorage.removeItem('token');
            window.location.href = 'login.html';
            return;
        }

        const contentType = response.headers.get('content-type') || '';
        const data = contentType.includes('application/json') ? await response.json() : null;
        
        if (!response.ok) {
            throw new Error(data?.message || 'Something went wrong');
        }

        return data;
    },

    auth: {
        async login(credentials) {
            const data = await api.request('/auth/login', {
                method: 'POST',
                body: JSON.stringify(credentials)
            });
            localStorage.setItem('token', data.token);
            localStorage.setItem('user', JSON.stringify(data.user));
            return data;
        },
        async register(userData) {
            return api.request('/auth/register', {
                method: 'POST',
                body: JSON.stringify(userData)
            });
        },
        logout() {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = 'login.html';
        },
        getUser() {
            return JSON.parse(localStorage.getItem('user'));
        },
        isAuthenticated() {
            return !!localStorage.getItem('token');
        }
    },

    courses: {
        async getAll() {
            return api.request('/courses');
        },
        async getById(id) {
            return api.request(`/courses/${id}`);
        },
        async create(courseData) {
            return api.request('/courses', {
                method: 'POST',
                body: JSON.stringify(courseData)
            });
        }
    },

    enrollments: {
        async getMyEnrollments() {
            return api.request('/enrollments/my-enrollments');
        },
        async enroll(courseId) {
            const user = api.auth.getUser();
            return api.request('/enrollments', {
                method: 'POST',
                body: JSON.stringify({ studentId: user.id, courseId })
            });
        }
    }
};
