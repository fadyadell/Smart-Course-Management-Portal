import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Home() {
  const { isAuthenticated } = useAuth();

  return (
    <div className="home-container">
      {/* Hero Section */}
      <section className="hero">
        <div className="hero-content">
          <div className="hero-badge">Next Gen Education 🚀</div>
          <h1 className="hero-title">
            Master Your Future with <br />
            <span className="gradient-text">Smart Course Management</span>
          </h1>
          <p className="hero-description">
            A powerful, all-in-one platform designed for students, instructors, and administrators. 
            Streamline your learning journey with enterprise-grade tools and seamless collaboration.
          </p>
          <div className="hero-actions">
            {isAuthenticated ? (
              <Link to="/dashboard" className="btn btn-primary btn-lg">
                Go to Dashboard →
              </Link>
            ) : (
              <>
                <Link to="/login" className="btn btn-primary btn-lg">
                  Get Started for Free
                </Link>
                <Link to="/courses" className="btn btn-secondary btn-lg">
                  Browse Courses
                </Link>
              </>
            )}
          </div>
        </div>
        <div className="hero-visual">
          <div className="abstract-shape shape-1"></div>
          <div className="abstract-shape shape-2"></div>
          <div className="hero-icon-large">🎓</div>
        </div>
      </section>

      {/* Features Section */}
      <section className="features">
        <div className="section-header">
          <h2>Why Choose Our Platform?</h2>
          <p>Built with cutting-edge technology for the best educational experience.</p>
        </div>
        <div className="features-grid">
          <div className="feature-card">
            <div className="feature-icon">🛡️</div>
            <h3>Secure RBAC</h3>
            <p>Role-Based Access Control ensures that Students, Instructors, and Admins have exactly the tools they need.</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">📊</div>
            <h3>Real-time Analytics</h3>
            <p>Track enrollments, course popularity, and student progress with dynamic dashboard statistics.</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">⚡</div>
            <h3>Blazing Fast</h3>
            <p>Optimized with React and .NET 10 to ensure a smooth, lag-free experience on any device.</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">🔄</div>
            <h3>Seamless Sync</h3>
            <p>Instant updates across the platform whenever a course is created, updated, or enrolled.</p>
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="stats-banner">
        <div className="stat-item">
          <span className="stat-num">50+</span>
          <span className="stat-name">Expert Instructors</span>
        </div>
        <div className="stat-item">
          <span className="stat-num">1.2k+</span>
          <span className="stat-name">Active Students</span>
        </div>
        <div className="stat-item">
          <span className="stat-num">120+</span>
          <span className="stat-name">Verified Courses</span>
        </div>
        <div className="stat-item">
          <span className="stat-num">98%</span>
          <span className="stat-name">Success Rate</span>
        </div>
      </section>

      {/* CTA Section */}
      <section className="final-cta">
        <div className="cta-card">
          <h2>Ready to transform your learning?</h2>
          <p>Join thousands of users already excelling on our platform.</p>
          <Link to="/login" className="btn btn-light btn-lg">Create Your Account Now</Link>
        </div>
      </section>

      {/* Minimal Footer */}
      <footer className="home-footer">
        <p>&copy; {new Date().getFullYear()} Smart Course Management Portal. All rights reserved.</p>
      </footer>
    </div>
  );
}
