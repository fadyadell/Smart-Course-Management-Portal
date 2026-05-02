import { useEffect, useState, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getCourseById, createCourse, updateCourse, getInstructors } from '../api/api';
import { useAuth } from '../context/AuthContext';

const EMPTY_FORM = { title: '', description: '', credits: 3, instructorId: '' };

export default function CourseForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [instructors, setInstructors] = useState([]);
  const [loading, setLoading] = useState(isEdit);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const fetchData = useCallback(async () => {
    try {
      const instData = await getInstructors();
      setInstructors(instData);

      if (isEdit) {
        const courseData = await getCourseById(id);
        setForm({
          title: courseData.title,
          description: courseData.description || '',
          credits: courseData.credits,
          instructorId: courseData.instructorId,
        });
      }
    } catch (err) {
      setError(err.message || 'Failed to load data.');
    } finally {
      setLoading(false);
    }
  }, [id, isEdit]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleChange = (e) =>
    setForm((p) => ({ ...p, [e.target.name]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setSubmitting(true);

    try {
      const payload = {
        title: form.title,
        description: form.description,
        credits: parseInt(form.credits, 10),
        instructorId: parseInt(form.instructorId, 10),
      };

      if (isEdit) {
        await updateCourse(id, payload);
        setSuccess('Course updated successfully!');
      } else {
        await createCourse(payload);
        setSuccess('Course created successfully!');
      }

      setTimeout(() => navigate('/manage-courses'), 1500);
    } catch (err) {
      setError(err.message || 'Action failed.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="full-page-loading">
        <div className="loading-spinner" />
      </div>
    );
  }

  return (
    <div className="page-wrapper">
      <div className="page-header">
        <div>
          <h1 className="page-title">{isEdit ? 'Edit Course' : 'Create New Course'}</h1>
          <p className="page-subtitle">
            {isEdit ? `Modifying course #${id}` : 'Fill in the details to offer a new course'}
          </p>
        </div>
        <button className="btn btn-secondary" onClick={() => navigate('/manage-courses')}>
          ← Back to List
        </button>
      </div>

      <div className="manage-form-card" style={{ maxWidth: '800px', margin: '0 auto' }}>
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
              rows={5}
              placeholder="Provide a comprehensive course overview…"
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
                  {inst.name} (ID: {inst.id})
                </option>
              ))}
            </select>
          </div>

          {error && <div className="alert alert-error"><span>⚠</span> {error}</div>}
          {success && <div className="alert alert-success"><span>✓</span> {success}</div>}

          <div className="form-actions" style={{ marginTop: '2rem' }}>
            <button type="submit" className="btn btn-primary btn-lg" disabled={submitting}>
              {submitting ? (
                <span className="loading-spinner-sm" />
              ) : isEdit ? (
                'Update Course'
              ) : (
                'Create Course'
              )}
            </button>
            <button
              type="button"
              className="btn btn-secondary btn-lg"
              onClick={() => navigate('/manage-courses')}
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
