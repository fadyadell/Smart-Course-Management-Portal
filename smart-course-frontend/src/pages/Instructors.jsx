import { useEffect, useState } from 'react';
import { getInstructors, updateInstructorProfile } from '../api/api';
import { useAuth } from '../context/AuthContext';

export default function Instructors() {
  const { isInstructor, user } = useAuth();

  const [instructors, setInstructors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Edit own profile (Instructor only)
  const [editMode, setEditMode] = useState(false);
  const [profileForm, setProfileForm] = useState({ biography: '', officeLocation: '' });
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState('');
  const [saveSuccess, setSaveSuccess] = useState('');

  const fetchInstructors = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getInstructors();
      setInstructors(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || 'Failed to load instructors.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchInstructors();
  }, []);

  // Pre-fill edit form with current profile data
  const handleEditClick = (instructor) => {
    setProfileForm({
      biography: instructor.biography || '',
      officeLocation: instructor.officeLocation || '',
    });
    setSaveError('');
    setSaveSuccess('');
    setEditMode(true);
  };

  const handleProfileSave = async (e) => {
    e.preventDefault();
    setSaveError('');
    setSaveSuccess('');
    setSaving(true);
    try {
      await updateInstructorProfile(profileForm);
      setSaveSuccess('Profile updated successfully!');
      setEditMode(false);
      fetchInstructors();
    } catch (err) {
      setSaveError(err.message || 'Failed to update profile.');
    } finally {
      setSaving(false);
    }
  };

  // Find current instructor's profile (matched by userId)
  const myProfile = instructors.find((i) => i.userId === user?.id);

  return (
    <div className="page-wrapper">
      <div className="page-header">
        <div>
          <h1 className="page-title">Instructors</h1>
          <p className="page-subtitle">Browse instructor profiles</p>
        </div>
        {isInstructor && myProfile && !editMode && (
          <button className="btn btn-primary" onClick={() => handleEditClick(myProfile)}>
            ✏ Edit My Profile
          </button>
        )}
      </div>

      {/* Edit Profile Form — Instructor only */}
      {isInstructor && editMode && (
        <div className="manage-form-card">
          <h2 className="form-heading">Update My Profile</h2>
          <form onSubmit={handleProfileSave} className="manage-form" noValidate>
            <div className="form-group">
              <label htmlFor="biography">Biography</label>
              <textarea
                id="biography"
                value={profileForm.biography}
                onChange={(e) => setProfileForm((p) => ({ ...p, biography: e.target.value }))}
                rows={4}
                placeholder="Tell students about yourself…"
                maxLength={500}
              />
            </div>
            <div className="form-group">
              <label htmlFor="officeLocation">Office Location</label>
              <input
                id="officeLocation"
                type="text"
                value={profileForm.officeLocation}
                onChange={(e) => setProfileForm((p) => ({ ...p, officeLocation: e.target.value }))}
                placeholder="e.g. Room 204, Building B"
                maxLength={100}
              />
            </div>

            {saveError && <div className="alert alert-error"><span>⚠</span> {saveError}</div>}
            {saveSuccess && <div className="alert alert-success"><span>✓</span> {saveSuccess}</div>}

            <div className="form-actions">
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? <span className="loading-spinner-sm" /> : 'Save Changes'}
              </button>
              <button type="button" className="btn btn-secondary" onClick={() => setEditMode(false)}>
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {loading && (
        <div className="loading-state">
          <div className="loading-spinner" />
          <p>Loading instructors…</p>
        </div>
      )}

      {!loading && error && (
        <div className="alert alert-error">
          <span>⚠</span> {error}{' '}
          <button className="link-btn" onClick={fetchInstructors}>
            Retry
          </button>
        </div>
      )}

      {!loading && !error && instructors.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon">👨‍🏫</div>
          <h3>No instructors found</h3>
          <p>No instructor profiles are available yet.</p>
        </div>
      )}

      {!loading && !error && instructors.length > 0 && (
        <div className="instructors-grid">
          {instructors.map((instructor) => (
            <div key={instructor.id} className="instructor-card">
              <div className="instructor-card-header">
                <div className="instructor-avatar">
                  {instructor.userName?.[0]?.toUpperCase() ?? '?'}
                </div>
                <div>
                  <h3 className="instructor-name">{instructor.userName}</h3>
                  <span className="role-badge role-instructor">Instructor</span>
                </div>
              </div>

              <div className="instructor-card-body">
                {instructor.biography ? (
                  <p className="instructor-bio">{instructor.biography}</p>
                ) : (
                  <p className="instructor-bio td-muted" style={{ fontStyle: 'italic' }}>
                    No biography provided.
                  </p>
                )}

                {instructor.officeLocation && (
                  <div className="meta-item" style={{ marginTop: '0.75rem' }}>
                    <span className="meta-icon">🏢</span>
                    <span>{instructor.officeLocation}</span>
                  </div>
                )}
              </div>

              <div className="instructor-card-footer">
                <span className="td-muted" style={{ fontSize: '0.8rem' }}>
                  📅 Joined {new Date(instructor.createdAt).toLocaleDateString()}
                </span>
                {isInstructor && instructor.userId === user?.id && (
                  <button
                    className="btn btn-secondary btn-sm"
                    onClick={() => handleEditClick(instructor)}
                  >
                    ✏ Edit
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
