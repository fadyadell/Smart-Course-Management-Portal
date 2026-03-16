const API_URL = 'https://localhost:7153/api'; // Update port if different

const api = {
    async request(endpoint, options = {}) {
        const token = localStorage.getItem('token');
        const headers = {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` }),
            ...options.headers
        };

        const response = await fetch(`${API_URL}${endpoint}`, {
            ...options,
            headers
        });

        if (response.status === 401) {
            localStorage.removeItem('token');
            window.location.href = 'login.html';
            return;
        }

        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.message || 'Something went wrong');
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
