const api = (() => {
    let _isRefreshing = false;
    let _refreshQueue = [];

    function _processQueue(error, token) {
        _refreshQueue.forEach(({ resolve, reject }) =>
            error ? reject(error) : resolve(token)
        );
        _refreshQueue = [];
    }

    async function _tryRefresh() {
        const refreshToken = Storage.getRefreshToken();
        if (!refreshToken) throw new Error('No refresh token');
        const res = await fetch(`${CONFIG.API_URL}/auth/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken })
        });
        if (!res.ok) throw new Error('Refresh failed');
        const data = await res.json();
        Storage.setTokens(data.token, data.refreshToken);
        Storage.setUser(data.user || Storage.getUser());
        return data.token;
    }

    async function request(endpoint, options = {}, _retry = false) {
        const token = Storage.getToken();
        const headers = {
            'Content-Type': 'application/json',
            ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
            ...options.headers
        };

        const res = await fetch(`${CONFIG.API_URL}${endpoint}`, { ...options, headers });

        if (res.status === 401 && !_retry) {
            if (_isRefreshing) {
                return new Promise((resolve, reject) => {
                    _refreshQueue.push({ resolve, reject });
                }).then(newToken => {
                    options.headers = { ...options.headers, 'Authorization': `Bearer ${newToken}` };
                    return request(endpoint, options, true);
                });
            }

            _isRefreshing = true;
            try {
                const newToken = await _tryRefresh();
                _processQueue(null, newToken);
                options.headers = { ...options.headers, 'Authorization': `Bearer ${newToken}` };
                return request(endpoint, options, true);
            } catch (err) {
                _processQueue(err, null);
                Storage.clearAll();
                window.location.href = 'login.html';
                return;
            } finally {
                _isRefreshing = false;
            }
        }

        if (res.status === 204) return null;

        let data;
        const contentType = res.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            data = await res.json();
        } else {
            data = {};
        }

        if (!res.ok) {
            const msg = data.message || data.title || data.detail || `Error ${res.status}`;
            throw new Error(msg);
        }

        return data;
    }

    return {
        get: (endpoint) => request(endpoint, { method: 'GET' }),
        post: (endpoint, body) => request(endpoint, { method: 'POST', body: JSON.stringify(body) }),
        put: (endpoint, body) => request(endpoint, { method: 'PUT', body: JSON.stringify(body) }),
        del: (endpoint) => request(endpoint, { method: 'DELETE' }),

        auth: {
            async login(email, password) {
                const data = await request('/auth/login', {
                    method: 'POST',
                    body: JSON.stringify({ email, password })
                });
                Storage.setTokens(data.token, data.refreshToken);
                Storage.setUser(data.user);
                return data;
            },
            async register(name, email, password, role) {
                return request('/auth/register', {
                    method: 'POST',
                    body: JSON.stringify({ name, email, password, role })
                });
            },
            async refresh(refreshToken) {
                return request('/auth/refresh', {
                    method: 'POST',
                    body: JSON.stringify({ refreshToken })
                });
            }
        },

        courses: {
            getAll(params = {}) {
                const qs = new URLSearchParams(
                    Object.entries(params).filter(([, v]) => v !== '' && v !== null && v !== undefined)
                ).toString();
                return request(`/courses${qs ? '?' + qs : ''}`, { method: 'GET' });
            },
            getById: (id) => request(`/courses/${id}`, { method: 'GET' }),
            create: (data) => request('/courses', { method: 'POST', body: JSON.stringify(data) }),
            update: (id, data) => request(`/courses/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
            delete: (id) => request(`/courses/${id}`, { method: 'DELETE' })
        },

        enrollments: {
            getMine: () => request('/enrollments/my-enrollments', { method: 'GET' }),
            enroll: (studentId, courseId) => request('/enrollments', {
                method: 'POST',
                body: JSON.stringify({ studentId, courseId })
            }),
            unenroll: (id) => request(`/enrollments/${id}`, { method: 'DELETE' })
        },

        students: {
            getAll: () => request('/students', { method: 'GET' })
        },

        instructors: {
            getAll: () => request('/instructors', { method: 'GET' })
        },

        users: {
            getAll(params = {}) {
                const qs = new URLSearchParams(
                    Object.entries(params).filter(([, v]) => v !== '' && v !== null && v !== undefined)
                ).toString();
                return request(`/users${qs ? '?' + qs : ''}`, { method: 'GET' });
            },
            update: (id, data) => request(`/users/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
            delete: (id) => request(`/users/${id}`, { method: 'DELETE' })
        }
    };
})();
