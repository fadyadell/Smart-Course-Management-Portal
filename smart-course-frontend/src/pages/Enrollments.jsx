import { useEffect, useState, useCallback } from 'react';
import {
  getInstructorCoursesEnrollments,
  unenrollCourse,
} from '../api/api';
import { useAuth } from '../context/AuthContext';

function normalizeList(data) {
  return Array.isArray(data) ? data : data?.items ?? [];
}

export default function Enrollments() {
  const { user, isInstructor } = useAuth();

  const [enrollments, setEnrollments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Pagination
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [totalPages, setTotalPages] = useState(1);

  // ── Fetch enrollments for the instructor's courses ──
  const fetchEnrollments = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getInstructorCoursesEnrollments(page, pageSize);
      if (Array.isArray(data)) {
        setEnrollments(data);
        setTotalPages(1);
      } else {
        setEnrollments(data.items ?? []);
        setTotalPages(data.totalPages ?? 1);
      }
    } catch (err) {
      setError(err.message || 'Failed to load enrollments.');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    if (isInstructor) {
      fetchEnrollments();
    }
  }, [isInstructor, fetchEnrollments]);

  // ── Drop a student ──
  const handleDrop = async (enrollmentId, studentName, courseTitle) => {
    if (
      !window.confirm(
        `Drop "${studentName}" from "${courseTitle}"? This action can be undone by re-enrolling.`
      )
    )
      return;

    try {
      await unenrollCourse(enrollmentId);
      fetchEnrollments();
    } catch (err) {
      alert(err.message || 'Failed to drop the student.');
    }
  };

  return (
    <div className="page-wrapper">
      <div className="page-header">
        <div>
          <h1 className="page-title">Students & Enrollments</h1>
          <p className="page-subtitle">
            Manage students enrolled in your courses
          </p>
        </div>
      </div>

      {/* Loading */}
      {loading && (
        <div className="loading-state">
          <div className="loading-spinner" />
          <p>Loading enrollments…</p>
        </div>
      )}

      {/* Error */}
      {!loading && error && (
        <div className="alert alert-error">
          <span>⚠</span> {error}{' '}
          <button className="link-btn" onClick={fetchEnrollments}>
            Retry
          </button>
        </div>
      )}

      {/* Empty state */}
      {!loading && !error && enrollments.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon">🗂</div>
          <h3>No enrollments found</h3>
          <p>No students are currently enrolled in any of your courses.</p>
        </div>
      )}

      {/* Enrollments table */}
      {!loading && !error && enrollments.length > 0 && (
        <>
          <div className="manage-table-wrapper">
            <table className="manage-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Student</th>
                  <th>Course</th>
                  <th>Enrolled On</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {enrollments.map((e, i) => (
                  <tr key={e.id}>
                    <td className="td-muted">{(page - 1) * pageSize + i + 1}</td>
                    <td>
                      <strong>{e.studentName || `Student #${e.studentId}`}</strong>
                    </td>
                    <td>{e.courseTitle || `Course #${e.courseId}`}</td>
                    <td className="td-muted">
                      {new Date(e.enrollmentDate).toLocaleDateString()}
                    </td>
                    <td>
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() =>
                          handleDrop(
                            e.id,
                            e.studentName || `Student #${e.studentId}`,
                            e.courseTitle || `Course #${e.courseId}`
                          )
                        }
                      >
                        Drop Student
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="pagination">
              <button
                className="btn btn-secondary"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                ← Prev
              </button>
              <span className="page-info">
                Page {page} of {totalPages}
              </span>
              <button
                className="btn btn-secondary"
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
              >
                Next →
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
