import { useEffect, useState } from 'react';
import { getMyEnrollments, unenrollCourse } from '../api/api';

export default function MyEnrollments() {
  const [enrollments, setEnrollments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchEnrollments = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getMyEnrollments();
      setEnrollments(Array.isArray(data) ? data : data.items ?? []);
    } catch (err) {
      setError(err.message || 'Failed to load enrollments.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEnrollments();
  }, []);

  const handleUnenroll = async (enrollmentId, courseTitle) => {
    if (!window.confirm(`Drop "${courseTitle}"? You can re-enroll later.`)) return;
    try {
      await unenrollCourse(enrollmentId);
      fetchEnrollments();
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="page-wrapper">
      <div className="page-header">
        <div>
          <h1 className="page-title">My Enrollments</h1>
          <p className="page-subtitle">Courses you are currently enrolled in</p>
        </div>
      </div>

      {loading && (
        <div className="loading-state">
          <div className="loading-spinner" />
          <p>Loading enrollments…</p>
        </div>
      )}

      {!loading && error && (
        <div className="alert alert-error">
          <span>⚠</span> {error}{' '}
          <button className="link-btn" onClick={fetchEnrollments}>
            Retry
          </button>
        </div>
      )}

      {!loading && !error && enrollments.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon">🗂</div>
          <h3>No enrollments yet</h3>
          <p>Head to the Courses page to enroll in a course.</p>
        </div>
      )}

      {!loading && !error && enrollments.length > 0 && (
        <div className="enrollments-list">
          {enrollments.map((en) => (
            <div key={en.id} className="enrollment-row">
              <div className="enrollment-info">
                <h3 className="enrollment-course">{en.courseTitle || `Course #${en.courseId}`}</h3>
                <div className="enrollment-meta">
                  <span className="meta-chip">📅 {new Date(en.enrollmentDate).toLocaleDateString()}</span>
                  <span className="meta-chip">🆔 Enrollment #{en.id}</span>
                  {en.studentName && (
                    <span className="meta-chip">👤 {en.studentName}</span>
                  )}
                </div>
              </div>
              <button
                className="btn btn-danger"
                onClick={() => handleUnenroll(en.id, en.courseTitle)}
              >
                Drop Course
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
