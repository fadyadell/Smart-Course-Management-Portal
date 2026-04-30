import { useEffect, useState, useCallback } from 'react';
import { getCoursesPaged, deleteCourse } from '../api/api';
import { useAuth } from '../context/AuthContext';
import CourseCard from '../components/CourseCard';

export default function Courses() {
  const { canManageCourses, isAdmin } = useAuth();

  // ── State ──────────────────────────────────
  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

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
  const handleDelete = async (id) => {
    if (!window.confirm('Delete this course? This cannot be undone.')) return;
    try {
      await deleteCourse(id);
      fetchCourses();
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="page-wrapper">
      {/* Page header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Course Catalogue</h1>
          <p className="page-subtitle">
            Browse and enroll in available courses
          </p>
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
