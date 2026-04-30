import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { registerUser } from '../api/api';

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();

  // ── Toggle between login / register ──────
  const [mode, setMode] = useState('login'); // 'login' | 'register'

  // ── Form state ────────────────────────────
  const [form, setForm] = useState({
    name: '',
    email: '',
    password: '',
    role: 'Student',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleChange = (e) =>
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));

  // ── Submit ─────────────────────────────────
  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      if (mode === 'login') {
        await login(form.email, form.password);
        navigate('/dashboard');
      } else {
        await registerUser(form.name, form.email, form.password, form.role);
        setSuccess('Account created! You can now sign in.');
        setMode('login');
        setForm((p) => ({ ...p, name: '', password: '', role: 'Student' }));
      }
    } catch (err) {
      setError(err.message || 'Something went wrong.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-wrapper">
      {/* Left decorative panel */}
      <div className="auth-panel">
        <div className="auth-panel-content">
          <div className="auth-logo">🎓</div>
          <h1 className="auth-panel-title">Smart Course Management</h1>
          <p className="auth-panel-subtitle">
            Your all-in-one platform for managing courses, students, and
            instructors — seamlessly.
          </p>
          <ul className="auth-features">
            <li>✦ Role-based access control</li>
            <li>✦ Real-time enrollment tracking</li>
            <li>✦ Instructor course management</li>
            <li>✦ Secure JWT authentication</li>
          </ul>
        </div>
        <div className="auth-panel-glow" />
      </div>

      {/* Right form panel */}
      <div className="auth-form-side">
        <div className="auth-card">
          <div className="auth-card-header">
            <h2>{mode === 'login' ? 'Welcome back' : 'Create account'}</h2>
            <p>
              {mode === 'login'
                ? 'Sign in to your account to continue'
                : 'Fill in the details below to get started'}
            </p>
          </div>

          {/* Mode tabs */}
          <div className="auth-tabs">
            <button
              className={`auth-tab ${mode === 'login' ? 'active' : ''}`}
              onClick={() => { setMode('login'); setError(''); setSuccess(''); }}
            >
              Sign In
            </button>
            <button
              className={`auth-tab ${mode === 'register' ? 'active' : ''}`}
              onClick={() => { setMode('register'); setError(''); setSuccess(''); }}
            >
              Register
            </button>
          </div>

          <form onSubmit={handleSubmit} className="auth-form" noValidate>
            {/* Name — register only */}
            {mode === 'register' && (
              <div className="form-group">
                <label htmlFor="name">Full Name</label>
                <input
                  id="name"
                  name="name"
                  type="text"
                  placeholder="Jane Smith"
                  value={form.name}
                  onChange={handleChange}
                  required
                  autoComplete="name"
                />
              </div>
            )}

            {/* Email */}
            <div className="form-group">
              <label htmlFor="email">Email Address</label>
              <input
                id="email"
                name="email"
                type="email"
                placeholder="you@example.com"
                value={form.email}
                onChange={handleChange}
                required
                autoComplete="email"
              />
            </div>

            {/* Password */}
            <div className="form-group">
              <label htmlFor="password">Password</label>
              <input
                id="password"
                name="password"
                type="password"
                placeholder={mode === 'register' ? 'Min. 6 characters' : '••••••••'}
                value={form.password}
                onChange={handleChange}
                required
                autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              />
            </div>

            {/* Role — register only */}
            {mode === 'register' && (
              <div className="form-group">
                <label htmlFor="role">Role</label>
                <select
                  id="role"
                  name="role"
                  value={form.role}
                  onChange={handleChange}
                >
                  <option value="Student">Student</option>
                  <option value="Instructor">Instructor</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>
            )}

            {/* Feedback */}
            {error && (
              <div className="alert alert-error">
                <span>⚠</span> {error}
              </div>
            )}
            {success && (
              <div className="alert alert-success">
                <span>✓</span> {success}
              </div>
            )}

            <button
              type="submit"
              className="btn btn-primary btn-full"
              disabled={loading}
            >
              {loading ? (
                <span className="loading-spinner-sm" />
              ) : mode === 'login' ? (
                'Sign In'
              ) : (
                'Create Account'
              )}
            </button>
          </form>

          {/* Demo credentials hint */}
          <div className="auth-hint">
            <p>🔑 Demo: use any registered account credentials</p>
          </div>
        </div>
      </div>
    </div>
  );
}
