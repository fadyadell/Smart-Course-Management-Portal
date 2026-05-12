import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import Cookies from 'js-cookie';
import { loginUser, logoutUser } from '../api/api';

// ─────────────────────────────────────────────
//  Context Definition
// ─────────────────────────────────────────────
const AuthContext = createContext(null);

// ─────────────────────────────────────────────
//  Provider
// ─────────────────────────────────────────────
export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [loading, setLoading] = useState(true); // hydrating from storage

  // Hydrate auth state from storage on mount
  useEffect(() => {
    const storedToken = Cookies.get('accessToken');
    const storedUser = localStorage.getItem('user');

    if (storedToken && storedUser) {
      try {
        setToken(storedToken);
        setUser(JSON.parse(storedUser));
      } catch {
        // Corrupted data — clear it
        Cookies.remove('accessToken');
        Cookies.remove('refreshToken');
        localStorage.removeItem('user');
      }
    }
    setLoading(false);
  }, []);

  // ── Login ──────────────────────────────────
  const login = useCallback(async (email, password) => {
    const data = await loginUser(email, password);
    setToken(data.accessToken);
    setUser(data.user);
    return data;
  }, []);

  // ── Logout ─────────────────────────────────
  const logout = useCallback(() => {
    logoutUser();
    setToken(null);
    setUser(null);
  }, []);


  // ── Derived helpers ────────────────────────
  const isAuthenticated = !!token && !!user;
  const isInstructor = user?.role === 'Instructor';
  const isStudent = user?.role === 'Student';
  const canManageCourses = isInstructor;

  const value = {
    user,
    token,
    loading,
    isAuthenticated,
    isInstructor,
    isStudent,
    canManageCourses,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// ─────────────────────────────────────────────
//  Custom Hook
// ─────────────────────────────────────────────
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>');
  return ctx;
}
