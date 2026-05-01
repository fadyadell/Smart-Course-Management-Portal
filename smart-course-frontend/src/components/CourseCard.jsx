import { useState } from 'react';
import { Link } from 'react-router-dom';
import { enrollCourse } from '../api/api';
import { useAuth } from '../context/AuthContext';

export default function CourseCard({ course, onEnrollSuccess, onDelete, canManage }) {
  const { user, isStudent } = useAuth();
  const [enrolling, setEnrolling] = useState(false);
  const [error, setError] = useState('');
  const [enrolled, setEnrolled] = useState(false);

  const handleEnroll = async () => {
    if (!user?.id) return;
    setEnrolling(true);
    setError('');
    try {
      await enrollCourse(user.id, course.id);
      setEnrolled(true);
      onEnrollSuccess?.();
    } catch (err) {
      setError(err.message);
    } finally {
      setEnrolling(false);
    }
  };

  const creditBadgeColor =
    course.credits >= 4 ? '#f59e0b' : course.credits >= 3 ? '#3b82f6' : '#10b981';

  return (
    <div className="course-card">
      {/* Header */}
      <div className="course-card-header">
        <div className="course-credits-badge" style={{ background: creditBadgeColor }}>
          {course.credits} {course.credits === 1 ? 'Credit' : 'Credits'}
        </div>
        <h3 className="course-title">{course.title}</h3>
      </div>

      {/* Body */}
      <div className="course-card-body">
        <p className="course-description">
          {course.description || 'No description available.'}
        </p>

        <div className="course-meta">
          <div className="meta-item">
            <span className="meta-icon">👨‍🏫</span>
            <span>{course.instructorName || `Instructor #${course.instructorId}`}</span>
          </div>
          {course.createdAt && (
            <div className="meta-item">
              <span className="meta-icon">📅</span>
              <span>{new Date(course.createdAt).toLocaleDateString()}</span>
            </div>
          )}
        </div>
      </div>

      {/* Footer */}
      <div className="course-card-footer">
        {error && <p className="error-text">{error}</p>}

        <div className="card-actions">
          <Link to={`/courses/${course.id}`} className="btn btn-secondary">
            View Details
          </Link>

          {isStudent && !enrolled && (
            <button
              className="btn btn-primary"
              onClick={handleEnroll}
              disabled={enrolling}
            >
              {enrolling ? (
                <span className="loading-spinner-sm" />
              ) : (
                '✚ Enroll'
              )}
            </button>
          )}

          {enrolled && (
            <span className="enrolled-badge">✓ Enrolled</span>
          )}

          {canManage && (
            <>
              <button
                className="btn btn-secondary"
                onClick={() => onDelete?.(course.id)}
              >
                🗑 Delete
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
