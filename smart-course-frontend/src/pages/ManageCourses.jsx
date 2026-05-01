import { useEffect, useState, useCallback } from 'react';
import { getCourses, createCourse, updateCourse, deleteCourse, getInstructors } from '../api/api';
import { useAuth } from '../context/AuthContext';

const EMPTY_FORM = { title: '', description: '', credits: 3, instructorId: '' };

export default function ManageCourses() {
  const { user, isAdmin } = useAuth();

  const [courses, setCourses] = useState([]);
  const [instructors, setInstructors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Create/Edit form
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');

  const fetchCourses = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [coursesData, instructorsData] = await Promise.all([
        getCourses(),
        getInstructors()
      ]);
      setCourses(Array.isArray(coursesData) ? coursesData : coursesData.items ?? []);
      setInstructors(instructorsData);
    } catch (err) {
      setError(err.message || 'Failed to load courses.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchCourses(); }, [fetchCourses]);

  const handleChange = (e) =>
    setForm((p) => ({ ...p, [e.target.name]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError('');
    setFormSuccess('');
    setSubmitting(true);
    try {
      const payload = {
        title: form.title,
        description: form.description,
        credits: parseInt(form.credits, 10),
        instructorId: parseInt(form.instructorId, 10),
      };

      if (editingId) {
        await updateCourse(editingId, payload);
        setFormSuccess('Course updated successfully!');
      } else {
        await createCourse(payload);
        setFormSuccess('Course created successfully!');
      }
      
      setForm(EMPTY_FORM);
      setEditingId(null);
      setTimeout(() => setShowForm(false), 1500);
      fetchCourses();
    } catch (err) {
      setFormError(err.message || 'Action failed.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleEdit = (course) => {
    setForm({
      title: course.title,
      description: course.description || '',
      credits: course.credits,
      instructorId: course.instructorId,
    });
    setEditingId(course.id);
    setShowForm(true);
    setFormError('');
    setFormSuccess('');
    window.scrollTo({ top: 0, behavior: 'smooth' });
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
          <p className="page-subtitle">Create and manage course offerings</p>
        </div>
        <button
          className="btn btn-primary"
          onClick={() => { 
            if (showForm) {
              setEditingId(null);
              setForm(EMPTY_FORM);
            }
            setShowForm((v) => !v); 
            setFormError(''); 
            setFormSuccess(''); 
          }}
        >
          {showForm ? '✕ Cancel' : '+ New Course'}
        </button>
      </div>

      {/* Create form */}
      {showForm && (
        <div className="manage-form-card">
          <h2 className="form-heading">{editingId ? 'Edit Course' : 'Create New Course'}</h2>
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

            <div className="form-group">
              <label htmlFor="instructorId">Instructor *</label>
              <select
                id="instructorId"
                name="instructorId"
                value={form.instructorId}
                onChange={handleChange}
                required
              >
                <option value="">-- Select an Instructor --</option>
                {instructors.map((inst) => (
                  <option key={inst.id} value={inst.id}>
                    {inst.name} (Profile ID: {inst.id})
                  </option>
                ))}
              </select>
            </div>

            {formError && <div className="alert alert-error"><span>⚠</span> {formError}</div>}
            {formSuccess && <div className="alert alert-success"><span>✓</span> {formSuccess}</div>}

            <div className="form-actions">
              <button type="submit" className="btn btn-primary" disabled={submitting}>
                {submitting ? <span className="loading-spinner-sm" /> : editingId ? 'Update Course' : 'Create Course'}
              </button>
              <button type="button" className="btn btn-secondary" onClick={() => { setShowForm(false); setEditingId(null); setForm(EMPTY_FORM); }}>
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
                        onClick={() => handleEdit(c)}
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
