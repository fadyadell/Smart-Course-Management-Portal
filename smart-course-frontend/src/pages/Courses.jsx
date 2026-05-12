import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { getCoursesPaged, deleteCourse } from '../api/api';
import { useAuth } from '../context/AuthContext';
import CourseCard from '../components/CourseCard';

export default function Courses() {
  const { canManageCourses, isStudent, isInstructor, isAdmin } = useAuth();
  const navigate = useNavigate();

  // ── State ──────────────────────────────────
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [viewMode, setViewMode] = useState(isStudent ? 'catalog' : 'manage'); // 'catalog' or 'manage'

  // Pagination & search
  const [page, setPage] = useState(1);
  const [pageSize] = useState(9);
  const [totalPages, setTotalPages] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [searchInput, setSearchInput] = useState('');

  // ── Fetch ──────────────────────────────────
  const fetchCourses = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getCoursesPaged(page, pageSize, searchTerm);

      // API may return a paginated object OR a plain array
      if (Array.isArray(data)) {
        setCourses(data);
        setTotalPages(1);
      } else {
        setCourses(data.items ?? []);
        setTotalPages(data.totalPages ?? 1);
      }
    } catch (err) {
      setError(err.message || 'Failed to load courses.');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, searchTerm]);

  useEffect(() => {
    fetchCourses();
  }, [fetchCourses]);

  // ── Search ─────────────────────────────────
  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    setSearchTerm(searchInput.trim());
  };

  const handleClearSearch = () => {
    setSearchInput('');
    setSearchTerm('');
    setPage(1);
  };

  // ── Delete ─────────────────────────────────
  const handleDelete = async (id, title) => {
    if (!window.confirm(`Delete "${title}"? This cannot be undone.`)) return;
    try {
      await deleteCourse(id);
      fetchCourses();
    } catch (err) {
      alert(err.message);
    }
  };

  // Switch view mode
  const handleToggleView = (mode) => {
    setViewMode(mode);
    setPage(1);
    setSearchTerm('');
    setSearchInput('');
  };

  // CATALOG VIEW (for all users)
  if (viewMode === 'catalog') {
    return (
      <div className="page-wrapper">
        {/* Page header with toggle */}
        <div className="page-header">
          <div>
            <h1 className="page-title">Course Catalogue</h1>
            <p className="page-subtitle">
              Browse and enroll in available courses
            </p>
          </div>
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            {canManageCourses && (
              <button
                className="btn btn-secondary"
                onClick={() => handleToggleView('manage')}
              >
                📋 Manage Mode
              </button>
            )}
            <button
              className="btn btn-primary"
              onClick={() => navigate('/courses/new')}
              style={{ display: canManageCourses ? 'inline-block' : 'none' }}
            >
              + New Course
            </button>
          </div>
        </div>

        {/* Search bar */}
        <form onSubmit={handleSearch} className="search-bar">
          <div className="search-input-wrapper">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              placeholder="Search by title or description…"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              className="search-input"
              id="course-search"
            />
            {searchInput && (
              <button
                type="button"
                className="search-clear"
                onClick={handleClearSearch}
                aria-label="Clear search"
              >
                ✕
              </button>
            )}
          </div>
          <button type="submit" className="btn btn-primary">
            Search
          </button>
        </form>

        {/* Search context label */}
        {searchTerm && (
          <p className="search-context">
            Showing results for <strong>"{searchTerm}"</strong> —{' '}
            <button className="link-btn" onClick={handleClearSearch}>
              clear
            </button>
          </p>
        )}

        {/* Loading */}
        {loading && (
          <div className="loading-state">
            <div className="loading-spinner" />
            <p>Loading courses…</p>
          </div>
        )}

        {/* Error */}
        {!loading && error && (
          <div className="alert alert-error">
            <span>⚠</span> {error}{' '}
            <button className="link-btn" onClick={fetchCourses}>
              Retry
            </button>
          </div>
        )}

        {/* Empty state */}
        {!loading && !error && courses.length === 0 && (
          <div className="empty-state">
            <div className="empty-icon">📭</div>
            <h3>No courses found</h3>
            <p>
              {searchTerm
                ? 'Try a different search term.'
                : 'No courses are available right now.'}
            </p>
          </div>
        )}

        {/* Courses grid */}
        {!loading && !error && courses.length > 0 && (
          <>
            <div className="courses-grid">
              {courses.map((course) => (
                <CourseCard
                  key={course.id}
                  course={course}
                  canManage={canManageCourses}
                  onEnrollSuccess={fetchCourses}
                  onDelete={isAdmin ? handleDelete : null}
                />
              ))}
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

  // MANAGE VIEW (for admin/instructor only)
  return (
    <div className="page-wrapper">
      <div className="page-header">
        <div>
          <h1 className="page-title">Manage Courses</h1>
          <p className="page-subtitle">Create and manage course offerings</p>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button
            className="btn btn-secondary"
            onClick={() => handleToggleView('catalog')}
          >
            🔍 Browse Mode
          </button>
          <button
            className="btn btn-primary"
            onClick={() => navigate('/courses/new')}
          >
            + New Course
          </button>
        </div>
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
                <th>Actions</th>
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
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
