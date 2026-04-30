import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { user, isAuthenticated, logout, isAdmin, canManageCourses } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const isActive = (path) => location.pathname === path;

  if (!isAuthenticated) return null;

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <Link to="/dashboard" className="brand-link">
          <span className="brand-icon">🎓</span>
          <span className="brand-text">SmartCourse</span>
        </Link>
      </div>

      <div className="navbar-links">
        <Link
          to="/dashboard"
          className={`nav-link ${isActive('/dashboard') ? 'active' : ''}`}
        >
          Dashboard
        </Link>
        <Link
          to="/courses"
          className={`nav-link ${isActive('/courses') ? 'active' : ''}`}
        >
          Courses
        </Link>
        {user?.role === 'Student' && (
          <Link
            to="/my-enrollments"
            className={`nav-link ${isActive('/my-enrollments') ? 'active' : ''}`}
          >
            My Enrollments
          </Link>
        )}
        {canManageCourses && (
          <Link
            to="/manage-courses"
            className={`nav-link ${isActive('/manage-courses') ? 'active' : ''}`}
          >
            Manage Courses
          </Link>
        )}
      </div>

      <div className="navbar-user">
        <div className="user-info">
          <span className="user-avatar">{user?.name?.[0]?.toUpperCase() ?? '?'}</span>
          <div className="user-details">
            <span className="user-name">{user?.name}</span>
            <span className={`role-badge role-${user?.role?.toLowerCase()}`}>
              {user?.role}
            </span>
          </div>
        </div>
        <button onClick={handleLogout} className="btn btn-logout">
          Sign Out
        </button>
      </div>
    </nav>
  );
}
