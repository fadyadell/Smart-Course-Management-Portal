import axios from 'axios';
import Cookies from 'js-cookie';

// ─────────────────────────────────────────────
//  API Configuration
// ─────────────────────────────────────────────
const BASE_URL = 'http://localhost:5202/api/v1';


// Axios Instance
const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// ─────────────────────────────────────────────
//  Token Helpers (using Cookies)
// ─────────────────────────────────────────────
const getToken = () => Cookies.get('accessToken');
const getRefreshToken = () => Cookies.get('refreshToken');

const setTokens = (accessToken, refreshToken) => {
  // Set cookies with a 7-day expiration for the refresh token, and session-based for access token
  Cookies.set('accessToken', accessToken, { expires: 1 / 96 }); // ~15 mins
  if (refreshToken) {
    Cookies.set('refreshToken', refreshToken, { expires: 7 });
  }
};

const clearTokens = () => {
  Cookies.remove('accessToken');
  Cookies.remove('refreshToken');
  localStorage.removeItem('user');
};

// ─────────────────────────────────────────────
//  Request Interceptor
// ─────────────────────────────────────────────
api.interceptors.request.use(
  (config) => {
    const token = getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ─────────────────────────────────────────────
//  Response Interceptor (Auto-refresh on 401)
// ─────────────────────────────────────────────
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Handle Network Error explicitly
    if (!error.response) {
      console.error('Network Error: Check if backend is running at ' + BASE_URL);
      return Promise.reject(new Error('Network Error: Backend is unreachable. Please ensure the API is started.'));
    }

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return api(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = getRefreshToken();
        if (!refreshToken) throw new Error('No refresh token');

        const res = await axios.post(`${BASE_URL}/auth/refresh`, { refreshToken });
        const { accessToken } = res.data;

        setTokens(accessToken, null);
        processQueue(null, accessToken);
        isRefreshing = false;

        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        isRefreshing = false;
        clearTokens();
        window.location.href = '/';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

// ─────────────────────────────────────────────
//  Auth Endpoints
// ─────────────────────────────────────────────

export async function loginUser(email, password) {
  const res = await api.post('/auth/login', { email, password });
  const data = res.data;
  setTokens(data.accessToken, data.refreshToken);
  localStorage.setItem('user', JSON.stringify(data.user));
  return data;
}

export async function registerUser(name, email, password, role) {
  const res = await api.post('/auth/register', { name, email, password, role });
  return res.data;
}

export function logoutUser() {
  clearTokens();
}

// ─────────────────────────────────────────────
//  Course Endpoints
// ─────────────────────────────────────────────

export async function getCourses() {
  const res = await api.get('/courses');
  return res.data;
}

export async function getCoursesPaged(page = 1, pageSize = 10, searchTerm = '') {
  const params = { page, pageSize };
  if (searchTerm) params.searchTerm = searchTerm;
  const res = await api.get('/courses/search', { params });
  return res.data;
}

export async function getCourseById(id) {
  const res = await api.get(`/courses/${id}`);
  return res.data;
}

export async function createCourse(courseData) {
  const res = await api.post('/courses', courseData);
  return res.data;
}

export async function updateCourse(id, courseData) {
  const res = await api.put(`/courses/${id}`, courseData);
  return res.data;
}

export async function deleteCourse(id) {
  const res = await api.delete(`/courses/${id}`);
  return res.data;
}

// ─────────────────────────────────────────────
//  Enrollment Endpoints
// ─────────────────────────────────────────────

export async function getMyEnrollments() {
  const res = await api.get('/enrollments/my-enrollments');
  return res.data;
}

export async function enrollCourse(studentId, courseId) {
  const res = await api.post('/enrollments', { studentId, courseId });
  return res.data;
}

export async function unenrollCourse(enrollmentId) {
  const res = await api.delete(`/enrollments/${enrollmentId}`);
  return res.data;
}

export async function getStudentEnrollments(studentId) {
  const res = await api.get(`/enrollments/student/${studentId}`);
  return res.data;
}

export async function getInstructorCoursesEnrollments(page = 1, pageSize = 20) {
  const params = { page, pageSize };
  const res = await api.get('/enrollments/instructor/my-courses-enrollments', { params });
  return res.data;
}

// ─────────────────────────────────────────────
//  Instructor & Student Endpoints
// ─────────────────────────────────────────────

export async function getInstructors() {
  const res = await api.get('/instructors');
  return res.data;
}

export async function getInstructorById(id) {
  const res = await api.get(`/instructors/${id}`);
  return res.data;
}

export async function updateInstructorProfile(profileData) {
  const res = await api.put('/instructors/profile', profileData);
  return res.data;
}

export async function getStudents() {
  const res = await api.get('/students');
  return res.data;
}

export async function getStudentById(id) {
  const res = await api.get(`/students/${id}`);
  return res.data;
}

export async function updateCourseById(id, courseData) {
  return updateCourse(id, courseData);
}

