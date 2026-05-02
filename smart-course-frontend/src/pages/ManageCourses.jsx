import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { getCourses, deleteCourse, getInstructors } from '../api/api';
import { useAuth } from '../context/AuthContext';

export default function ManageCourses() {
  const { isAdmin } = useAuth();
  const navigate = useNavigate();

  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchCourses = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const coursesData = await getCourses();
      setCourses(Array.isArray(coursesData) ? coursesData : coursesData.items ?? []);
    } catch (err) {
      setError(err.message || 'Failed to load courses.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchCourses(); }, [fetchCourses]);

  const handleDelete = async (id, title) => {
    if (!window.confirm(`Delete "${title}"? This cannot be undone.`)) return;
    try {
      await deleteCourse(id);
      fetchCourses();
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="page-wrapper">
      <div className="page-header">
        <div>
          <h1 className="page-title">Manage Courses</h1>
          <p className="page-subtitle">Create and manage course offerings</p>
        </div>
        <button
          className="btn btn-primary"
          onClick={() => navigate('/courses/new')}
        >
          + New Course
        </button>
      </div>

      {/* Course table */}
      {loading && (
        <div className="loading-state"><div className="loading-spinner" /><p>Loading…</p></div>
      )}

      {!loading && error && (
        <div className="alert alert-error"><span>⚠</span> {error}</div>
      )}

      {!loading && !error && courses.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon">📭</div>
          <h3>No courses yet</h3>
          <p>Click "+ New Course" to create the first one.</p>
        </div>
      )}

      {!loading && !error && courses.length > 0 && (
        <div className="manage-table-wrapper">
          <table className="manage-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Title</th>
                <th>Credits</th>
                <th>Instructor</th>
                <th>Created</th>
                {isAdmin && <th>Actions</th>}
              </tr>
            </thead>
            <tbody>
              {courses.map((c, i) => (
                <tr key={c.id}>
                  <td className="td-muted">{i + 1}</td>
                  <td>
                    <strong>{c.title}</strong>
                    {c.description && (
                      <p className="td-desc">{c.description.slice(0, 70)}{c.description.length > 70 ? '…' : ''}</p>
                    )}
                  </td>
                  <td>
                    <span className="credit-chip">{c.credits}</span>
                  </td>
                  <td className="td-muted">
                    {c.instructorName || `#${c.instructorId}`}
                  </td>
                  <td className="td-muted">
                    {new Date(c.createdAt).toLocaleDateString()}
                  </td>
                  {isAdmin && (
                    <td>
                      <button
                        className="btn btn-secondary btn-sm"
                        style={{ marginRight: '0.5rem' }}
                        onClick={() => navigate(`/courses/edit/${c.id}`)}
                      >
                        Edit
                      </button>
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() => handleDelete(c.id, c.title)}
                      >
                        Delete
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
