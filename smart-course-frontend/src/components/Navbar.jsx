import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { user, isAuthenticated, logout, canManageCourses } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const isActive = (path) => location.pathname === path;

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <Link to="/" className="brand-link">
          <span className="brand-icon">🎓</span>
          <span className="brand-text">SmartCourse</span>
        </Link>
      </div>

      <div className="navbar-links">
        {/* Public Links */}
        <Link
          to="/"
          className={`nav-link ${isActive('/') ? 'active' : ''}`}
        >
          Home
        </Link>

        {isAuthenticated ? (
          <>
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
            <Link
              to="/instructors"
              className={`nav-link ${isActive('/instructors') ? 'active' : ''}`}
            >
              Instructors
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
              <>
                <Link
                  to="/students"
                  className={`nav-link ${isActive('/students') ? 'active' : ''}`}
                >
                  Students
                </Link>
                <Link
                  to="/manage-courses"
                  className={`nav-link ${isActive('/manage-courses') ? 'active' : ''}`}
                >
                  Manage Courses
                </Link>
              </>
            )}
          </>
        ) : (
          <Link
            to="/courses"
            className={`nav-link ${isActive('/courses') ? 'active' : ''}`}
          >
            All Courses
          </Link>
        )}
        {canManageCourses && (
          <Link
            to="/students"
            className={`nav-link ${isActive('/students') ? 'active' : ''}`}
          >
            Students
          </Link>
        )}
        <Link
          to="/instructors"
          className={`nav-link ${isActive('/instructors') ? 'active' : ''}`}
        >
          Instructors
        </Link>
      </div>

      <div className="navbar-user">
        {isAuthenticated ? (
          <>
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
          </>
        ) : (
          <Link to="/login" className="btn btn-primary">
            Sign In
          </Link>
        )}
      </div>
    </nav>
  );
}
