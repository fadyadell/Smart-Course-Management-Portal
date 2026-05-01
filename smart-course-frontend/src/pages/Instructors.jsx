import { useEffect, useState } from 'react';
import { getInstructors, updateInstructorProfile } from '../api/api';
import { useAuth } from '../context/AuthContext';

export default function Instructors() {
  const { user, isInstructor } = useAuth();
  const [instructors, setInstructors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  // Profile edit state (for the logged-in instructor)
  const [isEditing, setIsEditing] = useState(false);
  const [profileForm, setProfileForm] = useState({
    biography: '',
    officeLocation: ''
  });
  const [updateLoading, setUpdateLoading] = useState(false);

  useEffect(() => {
    fetchInstructors();
  }, []);

  const fetchInstructors = async () => {
    try {
      setLoading(true);
      const data = await getInstructors();
      setInstructors(data);
      
      // If the current user is an instructor, pre-fill their profile data for editing
      if (isInstructor) {
        const myProfile = data.find(i => i.userId === user.id);
        if (myProfile) {
          setProfileForm({
            biography: myProfile.biography || '',
            officeLocation: myProfile.officeLocation || ''
          });
        }
      }
    } catch (err) {
      setError('Failed to load instructors.');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateProfile = async (e) => {
    e.preventDefault();
    try {
      setUpdateLoading(true);
      await updateInstructorProfile(profileForm);
      setIsEditing(false);
      fetchInstructors(); // Refresh list
    } catch (err) {
      alert('Failed to update profile.');
    } finally {
      setUpdateLoading(false);
    }
  };

  return (
    <div className="page-wrapper">
      <div className="page-header">
        <h1>Instructor Directory</h1>
        <p>Explore our qualified teaching staff.</p>
        {isInstructor && !isEditing && (
          <button 
            className="btn btn-primary"
            onClick={() => setIsEditing(true)}
          >
            Edit My Profile
          </button>
        )}
      </div>

      {isEditing && (
        <div className="card animation-slide-in" style={{ marginBottom: '2rem' }}>
          <h3>Update Your Profile</h3>
          <form onSubmit={handleUpdateProfile} className="profile-form">
            <div className="form-group">
              <label>Office Location</label>
              <input 
                type="text"
                value={profileForm.officeLocation}
                onChange={(e) => setProfileForm({...profileForm, officeLocation: e.target.value})}
                placeholder="e.g. Building A, Room 101"
              />
            </div>
            <div className="form-group">
              <label>Biography</label>
              <textarea 
                value={profileForm.biography}
                onChange={(e) => setProfileForm({...profileForm, biography: e.target.value})}
                placeholder="Tell us about your background..."
                rows={4}
              />
            </div>
            <div className="form-actions">
              <button type="submit" className="btn btn-success" disabled={updateLoading}>
                {updateLoading ? 'Saving...' : 'Save Changes'}
              </button>
              <button 
                type="button" 
                className="btn btn-secondary" 
                onClick={() => setIsEditing(false)}
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {loading ? (
        <div className="loading-spinner" />
      ) : error ? (
        <div className="alert alert-error">{error}</div>
      ) : (
        <div className="instructor-grid">
          {instructors.map((inst) => (
            <div key={inst.userId} className="instructor-card card">
              <div className="inst-avatar">
                {inst.name.charAt(0)}
              </div>
              <div className="inst-info">
                <h3>{inst.name}</h3>
                <p className="inst-email">{inst.email}</p>
                {inst.officeLocation && (
                  <p className="inst-office">📍 {inst.officeLocation}</p>
                )}
                {inst.biography && (
                  <p className="inst-bio">{inst.biography}</p>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
