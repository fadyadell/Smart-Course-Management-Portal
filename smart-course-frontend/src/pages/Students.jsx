import { useEffect, useState } from 'react';
import { getStudents, getStudentEnrollments } from '../api/api';

export default function Students() {
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Expandable enrollments per student
  const [expandedId, setExpandedId] = useState(null);
  const [enrollments, setEnrollments] = useState({});
  const [enrollLoading, setEnrollLoading] = useState(false);

  // Search
  const [search, setSearch] = useState('');

  const fetchStudents = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getStudents();
      setStudents(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || 'Failed to load students.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchStudents();
  }, []);

  const handleToggleEnrollments = async (studentId) => {
    if (expandedId === studentId) {
      setExpandedId(null);
      return;
    }
    setExpandedId(studentId);
    if (enrollments[studentId]) return; // already loaded

    setEnrollLoading(true);
    try {
      const data = await getStudentEnrollments(studentId);
      setEnrollments((prev) => ({ ...prev, [studentId]: Array.isArray(data) ? data : [] }));
    } catch {
      setEnrollments((prev) => ({ ...prev, [studentId]: [] }));
    } finally {
      setEnrollLoading(false);
    }
  };

  const filtered = students.filter(
    (s) =>
      s.name?.toLowerCase().includes(search.toLowerCase()) ||
      s.email?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="page-wrapper">
      <div className="page-header">
        <div>
          <h1 className="page-title">Students</h1>
          <p className="page-subtitle">Browse all registered student accounts</p>
        </div>
        <div className="header-meta">
          {!loading && (
            <span className="count-chip">{students.length} student{students.length !== 1 ? 's' : ''}</span>
          )}
        </div>
      </div>

      {/* Search */}
      <div className="search-bar" style={{ marginBottom: '1.5rem' }}>
        <div className="search-input-wrapper">
          <span className="search-icon">🔍</span>
          <input
            type="text"
            placeholder="Search by name or email…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="search-input"
          />
          {search && (
            <button
              type="button"
              className="search-clear"
              onClick={() => setSearch('')}
              aria-label="Clear search"
            >
              ✕
            </button>
          )}
        </div>
      </div>

      {loading && (
        <div className="loading-state">
          <div className="loading-spinner" />
          <p>Loading students…</p>
        </div>
      )}

      {!loading && error && (
        <div className="alert alert-error">
          <span>⚠</span> {error}{' '}
          <button className="link-btn" onClick={fetchStudents}>
            Retry
          </button>
        </div>
      )}

      {!loading && !error && filtered.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon">👥</div>
          <h3>{search ? 'No students match your search' : 'No students registered yet'}</h3>
          {search && (
            <button className="btn btn-secondary" onClick={() => setSearch('')} style={{ marginTop: '1rem' }}>
              Clear search
            </button>
          )}
        </div>
      )}

      {!loading && !error && filtered.length > 0 && (
        <div className="manage-table-wrapper">
          <table className="manage-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Name</th>
                <th>Email</th>
                <th>Enrollments</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((student, idx) => (
                <>
                  <tr key={student.id}>
                    <td className="td-muted">{idx + 1}</td>
                    <td>
                      <div className="student-name-cell">
                        <span className="user-avatar" style={{ width: 32, height: 32, fontSize: '0.85rem' }}>
                          {student.name?.[0]?.toUpperCase() ?? '?'}
                        </span>
                        <strong>{student.name}</strong>
                      </div>
                    </td>
                    <td className="td-muted">{student.email}</td>
                    <td>
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() => handleToggleEnrollments(student.id)}
                      >
                        {expandedId === student.id ? '▲ Hide' : '▼ View Enrollments'}
                      </button>
                    </td>
                  </tr>
                  {expandedId === student.id && (
                    <tr key={`enroll-${student.id}`} className="enrollment-expand-row">
                      <td colSpan={4}>
                        {enrollLoading && !enrollments[student.id] ? (
                          <div style={{ padding: '0.75rem', color: 'var(--text-muted)' }}>
                            <span className="loading-spinner-sm" style={{ borderTopColor: 'var(--primary)', borderColor: 'var(--border)' }} /> Loading…
                          </div>
                        ) : enrollments[student.id]?.length === 0 ? (
                          <p style={{ padding: '0.75rem', color: 'var(--text-muted)' }}>
                            Not enrolled in any courses.
                          </p>
                        ) : (
                          <ul className="enrollment-expand-list">
                            {enrollments[student.id]?.map((en) => (
                              <li key={en.id}>
                                <span className="meta-chip">📖 {en.courseTitle || `Course #${en.courseId}`}</span>
                                <span className="meta-chip">📅 {new Date(en.enrollmentDate).toLocaleDateString()}</span>
                              </li>
                            ))}
                          </ul>
                        )}
                      </td>
                    </tr>
                  )}
                </>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
