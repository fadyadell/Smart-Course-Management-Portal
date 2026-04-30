// ─────────────────────────────────────────────
//  API Configuration
// ─────────────────────────────────────────────
const BASE_URL = 'http://localhost:5202/api';

// ─────────────────────────────────────────────
//  Token Helpers
// ─────────────────────────────────────────────
const getToken = () => localStorage.getItem('accessToken');
const getRefreshToken = () => localStorage.getItem('refreshToken');
const setTokens = (accessToken, refreshToken) => {
  localStorage.setItem('accessToken', accessToken);
  if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
};
const clearTokens = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('user');
};

// ─────────────────────────────────────────────
//  Core Fetch Wrapper (with auto-refresh on 401)
// ─────────────────────────────────────────────
let isRefreshing = false;

async function apiFetch(endpoint, options = {}, retry = true) {
  const token = getToken();

  const headers = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const response = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  // Auto-refresh on 401
  if (response.status === 401 && retry && !isRefreshing) {
    isRefreshing = true;
    try {
      const refreshToken = getRefreshToken();
      if (!refreshToken) throw new Error('No refresh token');

      const refreshRes = await fetch(`${BASE_URL}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      });

      if (!refreshRes.ok) throw new Error('Token refresh failed');

      const data = await refreshRes.json();
      setTokens(data.accessToken, null);
      isRefreshing = false;

      // Retry original request with new token
      return apiFetch(endpoint, options, false);
    } catch {
      isRefreshing = false;
      clearTokens();
      window.location.href = '/';
      throw new Error('Session expired. Please log in again.');
    }
  }

  return response;
}

// ─────────────────────────────────────────────
//  Auth Endpoints
// ─────────────────────────────────────────────

/**
 * Login user.
 * POST /api/auth/login
 * Returns: { accessToken, refreshToken, accessTokenExpiry, user: { id, name, email, role } }
 */
export async function loginUser(email, password) {
  const res = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Invalid email or password.');
  }

  const data = await res.json();
  setTokens(data.accessToken, data.refreshToken);
  localStorage.setItem('user', JSON.stringify(data.user));
  return data;
}

/**
 * Register a new user.
 * POST /api/auth/register
 * Body: { name, email, password, role }
 */
export async function registerUser(name, email, password, role) {
  const res = await fetch(`${BASE_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, email, password, role }),
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Registration failed.');
  }

  return res.json();
}

/**
 * Logout — clears stored tokens.
 */
export function logoutUser() {
  clearTokens();
}

// ─────────────────────────────────────────────
//  Course Endpoints
// ─────────────────────────────────────────────

/**
 * Get all courses (no pagination).
 * GET /api/courses
 */
export async function getCourses() {
  const res = await apiFetch('/courses');
  if (!res.ok) throw new Error('Failed to fetch courses.');
  return res.json();
}

/**
 * Get courses with pagination and optional search.
 * GET /api/courses/search?page=1&pageSize=10&searchTerm=...
 */
export async function getCoursesPaged(page = 1, pageSize = 10, searchTerm = '') {
  const params = new URLSearchParams({ page, pageSize });
  if (searchTerm) params.set('searchTerm', searchTerm);
  const res = await apiFetch(`/courses/search?${params}`);
  if (!res.ok) throw new Error('Failed to fetch courses.');
  return res.json();
}

/**
 * Get single course by ID.
 * GET /api/courses/{id}
 */
export async function getCourseById(id) {
  const res = await apiFetch(`/courses/${id}`);
  if (!res.ok) throw new Error(`Course ${id} not found.`);
  return res.json();
}

/**
 * Create a new course (Admin/Instructor only).
 * POST /api/courses
 * Body: { title, description, credits, instructorId }
 */
export async function createCourse(courseData) {
  const res = await apiFetch('/courses', {
    method: 'POST',
    body: JSON.stringify(courseData),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Failed to create course.');
  }
  return res.json();
}

/**
 * Update a course (Admin/Instructor only).
 * PUT /api/courses/{id}
 */
export async function updateCourse(id, courseData) {
  const res = await apiFetch(`/courses/${id}`, {
    method: 'PUT',
    body: JSON.stringify(courseData),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Failed to update course.');
  }
  // 204 No Content
  return true;
}

/**
 * Delete a course (Admin only).
 * DELETE /api/courses/{id}
 */
export async function deleteCourse(id) {
  const res = await apiFetch(`/courses/${id}`, { method: 'DELETE' });
  if (!res.ok) throw new Error('Failed to delete course.');
  return true;
}

// ─────────────────────────────────────────────
//  Enrollment Endpoints
// ─────────────────────────────────────────────

/**
 * Get current student's enrollments.
 * GET /api/enrollments/my-enrollments  (Student role)
 */
export async function getMyEnrollments() {
  const res = await apiFetch('/enrollments/my-enrollments');
  if (!res.ok) throw new Error('Failed to fetch enrollments.');
  return res.json();
}

/**
 * Enroll current student in a course.
 * POST /api/enrollments
 * Body: { studentId, courseId }
 */
export async function enrollCourse(studentId, courseId) {
  const res = await apiFetch('/enrollments', {
    method: 'POST',
    body: JSON.stringify({ studentId, courseId }),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Enrollment failed.');
  }
  return res.json();
}

/**
 * Unenroll from a course.
 * DELETE /api/enrollments/{id}
 */
export async function unenrollCourse(enrollmentId) {
  const res = await apiFetch(`/enrollments/${enrollmentId}`, { method: 'DELETE' });
  if (!res.ok) throw new Error('Failed to unenroll.');
  return true;
}

/**
 * Get enrollments for a specific student (Admin/Instructor).
 * GET /api/enrollments/student/{studentId}
 */
export async function getStudentEnrollments(studentId) {
  const res = await apiFetch(`/enrollments/student/${studentId}`);
  if (!res.ok) throw new Error('Failed to fetch student enrollments.');
  return res.json();
}
