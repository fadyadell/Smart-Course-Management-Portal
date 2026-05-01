import { useEffect, useState, useCallback } from 'react';
import { getCourses, createCourse, updateCourse, deleteCourse } from '../api/api';
import { useAuth } from '../context/AuthContext';

const EMPTY_FORM = { title: '', description: '', credits: 3, instructorId: '' };

export default function ManageCourses() {
  const { user, isAdmin, canManageCourses } = useAuth();

  const [courses, setCourses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Create/Edit form
  const [showForm, setShowForm] = useState(false);
  const [editingCourse, setEditingCourse] = useState(null); // null = create, object = edit
  const [form, setForm] = useState(EMPTY_FORM);
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');

  const fetchCourses = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getCourses();
      setCourses(Array.isArray(data) ? data : data.items ?? []);
    } catch (err) {
      setError(err.message || 'Failed to load courses.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchCourses(); }, [fetchCourses]);

  const handleChange = (e) =>
    setForm((p) => ({ ...p, [e.target.name]: e.target.value }));

  const openCreateForm = () => {
    setEditingCourse(null);
    setForm(EMPTY_FORM);
    setFormError('');
    setFormSuccess('');
    setShowForm(true);
  };

  const openEditForm = (course) => {
    setEditingCourse(course);
    setForm({
      title: course.title,
      description: course.description || '',
      credits: course.credits,
      instructorId: course.instructorId,
    });
    setFormError('');
    setFormSuccess('');
    setShowForm(true);
  };

  const closeForm = () => {
    setShowForm(false);
    setEditingCourse(null);
    setForm(EMPTY_FORM);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError('');
    setFormSuccess('');
    setSubmitting(true);
    try {
      if (editingCourse) {
        // Update existing course
        const payload = {
          title: form.title,
          description: form.description,
          credits: parseInt(form.credits, 10),
        };
        await updateCourse(editingCourse.id, payload);
        setFormSuccess('Course updated successfully!');
        setEditingCourse(null);
        setShowForm(false);
      } else {
        // Create new course
        const payload = {
          title: form.title,
          description: form.description,
          credits: parseInt(form.credits, 10),
          instructorId: parseInt(form.instructorId || user.id, 10),
        };
        await createCourse(payload);
        setFormSuccess('Course created successfully!');
        setForm(EMPTY_FORM);
      }
      fetchCourses();
    } catch (err) {
      setFormError(err.message || 'Failed to save course.');
    } finally {
      setSubmitting(false);
    }
  };

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
          <p className="page-subtitle">Create, edit, and manage course offerings</p>
        </div>
        {!showForm && (
          <button className="btn btn-primary" onClick={openCreateForm}>
            + New Course
          </button>
        )}
      </div>

      {/* Global form success (shown after close) */}
      {!showForm && formSuccess && (
        <div className="alert alert-success"><span>✓</span> {formSuccess}</div>
      )}

      {/* Create / Edit form */}
      {showForm && (
        <div className="manage-form-card">
          <h2 className="form-heading">
            {editingCourse ? `Edit: ${editingCourse.title}` : 'Create New Course'}
          </h2>
          <form onSubmit={handleSubmit} className="manage-form" noValidate>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="title">Course Title *</label>
                <input
                  id="title"
                  name="title"
                  value={form.title}
                  onChange={handleChange}
                  placeholder="e.g. Introduction to React"
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="credits">Credits *</label>
                <input
                  id="credits"
                  name="credits"
                  type="number"
                  min={1}
                  max={10}
                  value={form.credits}
                  onChange={handleChange}
                  required
                />
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="description">Description</label>
              <textarea
                id="description"
                name="description"
                value={form.description}
                onChange={handleChange}
                rows={3}
                placeholder="Brief course overview…"
              />
            </div>

            {/* InstructorId only shown when creating (can't change instructor on update) */}
            {!editingCourse && (
              <div className="form-group">
                <label htmlFor="instructorId">Instructor ID</label>
                <input
                  id="instructorId"
                  name="instructorId"
                  type="number"
                  min={1}
                  value={form.instructorId}
                  onChange={handleChange}
                  placeholder={`Default: your ID (${user?.id})`}
                />
              </div>
            )}

            {formError && <div className="alert alert-error"><span>⚠</span> {formError}</div>}
            {formSuccess && <div className="alert alert-success"><span>✓</span> {formSuccess}</div>}

            <div className="form-actions">
              <button type="submit" className="btn btn-primary" disabled={submitting}>
                {submitting ? (
                  <span className="loading-spinner-sm" />
                ) : editingCourse ? (
                  'Save Changes'
                ) : (
                  'Create Course'
                )}
              </button>
              <button type="button" className="btn btn-secondary" onClick={closeForm}>
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

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
                {canManageCourses && <th>Actions</th>}
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
                  {canManageCourses && (
                    <td>
                      <div className="action-buttons">
                        <button
                          className="btn btn-secondary btn-sm"
                          onClick={() => openEditForm(c)}
                        >
                          ✏ Edit
                        </button>
                        {isAdmin && (
                          <button
                            className="btn btn-danger btn-sm"
                            onClick={() => handleDelete(c.id, c.title)}
                          >
                            🗑 Delete
                          </button>
                        )}
                      </div>
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
