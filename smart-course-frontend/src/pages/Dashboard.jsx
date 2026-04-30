import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getCourses, getMyEnrollments } from '../api/api';

export default function Dashboard() {
  const { user, isStudent, canManageCourses } = useAuth();

  const [stats, setStats] = useState({
    totalCourses: null,
    myEnrollments: null,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const [coursesData, enrollData] = await Promise.allSettled([
          getCourses(),
          isStudent ? getMyEnrollments() : Promise.resolve([]),
        ]);

        setStats({
          totalCourses:
            coursesData.status === 'fulfilled'
              ? Array.isArray(coursesData.value)
                ? coursesData.value.length
                : coursesData.value?.items?.length ?? '—'
              : '—',
          myEnrollments:
            enrollData.status === 'fulfilled'
              ? Array.isArray(enrollData.value)
                ? enrollData.value.length
                : '—'
              : '—',
        });
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [isStudent]);

  const roleColor = {
    Admin: '#f59e0b',
    Instructor: '#3b82f6',
    Student: '#10b981',
  };

  return (
    <div className="page-wrapper">
      {/* Hero welcome */}
      <div
        className="dashboard-hero"
        style={{ '--role-color': roleColor[user?.role] ?? '#6366f1' }}
      >
        <div className="hero-text">
          <h1>
            Welcome back, <span className="hero-name">{user?.name}</span> 👋
          </h1>
          <p>
            You are signed in as{' '}
            <span
              className={`role-badge role-${user?.role?.toLowerCase()}`}
              style={{ fontSize: '0.9rem' }}
            >
              {user?.role}
            </span>
          </p>
        </div>
        <div className="hero-illustration">🎓</div>
      </div>

      {/* Stats row */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">📚</div>
          <div className="stat-body">
            <span className="stat-value">
              {loading ? '…' : stats.totalCourses}
            </span>
            <span className="stat-label">Available Courses</span>
          </div>
        </div>

        {isStudent && (
          <div className="stat-card">
            <div className="stat-icon">✅</div>
            <div className="stat-body">
              <span className="stat-value">
                {loading ? '…' : stats.myEnrollments}
              </span>
              <span className="stat-label">My Enrollments</span>
            </div>
          </div>
        )}

        <div className="stat-card">
          <div className="stat-icon">🔐</div>
          <div className="stat-body">
            <span className="stat-value">{user?.role}</span>
            <span className="stat-label">Your Role</span>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">👤</div>
          <div className="stat-body">
            <span className="stat-value" style={{ fontSize: '1rem' }}>
              {user?.email}
            </span>
            <span className="stat-label">Account Email</span>
          </div>
        </div>
      </div>

      {/* Quick actions */}
      <section className="dashboard-section">
        <h2 className="section-title">Quick Actions</h2>
        <div className="quick-actions-grid">
          <Link to="/courses" className="quick-action-card">
            <div className="qa-icon">📖</div>
            <div className="qa-body">
              <h3>Browse Courses</h3>
              <p>Explore all available courses and enroll</p>
            </div>
            <span className="qa-arrow">→</span>
          </Link>

          {isStudent && (
            <Link to="/my-enrollments" className="quick-action-card">
              <div className="qa-icon">🗂</div>
              <div className="qa-body">
                <h3>My Enrollments</h3>
                <p>View courses you are enrolled in</p>
              </div>
              <span className="qa-arrow">→</span>
            </Link>
          )}

          {canManageCourses && (
            <Link to="/manage-courses" className="quick-action-card">
              <div className="qa-icon">⚙️</div>
              <div className="qa-body">
                <h3>Manage Courses</h3>
                <p>Create, edit, or delete courses</p>
              </div>
              <span className="qa-arrow">→</span>
            </Link>
          )}
        </div>
      </section>

      {/* Info banner */}
      <div className="info-banner">
        <span>💡</span>
        <p>
          {isStudent
            ? 'Tip: Navigate to Courses to find and enroll in new courses.'
            : canManageCourses
            ? 'Tip: Use Manage Courses to create new course offerings.'
            : 'Welcome to the Smart Course Management System.'}
        </p>
      </div>
    </div>
  );
}
