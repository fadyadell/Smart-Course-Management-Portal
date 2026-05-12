import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { getCourseById, enrollCourse, unenrollCourse, getMyEnrollments } from '../api/api';
import { useAuth } from '../context/AuthContext';

export default function CourseDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, isStudent } = useAuth();

  const [course, setCourse] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Enrollment state
  const [enrollmentId, setEnrollmentId] = useState(null); // null = not enrolled
  const [enrolling, setEnrolling] = useState(false);
  const [actionError, setActionError] = useState('');
  const [actionSuccess, setActionSuccess] = useState('');

  // Load course and check enrollment
  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setError('');
      try {
        const courseData = await getCourseById(id);
        setCourse(courseData);

        // For students, check if already enrolled
        if (isStudent) {
          try {
            const myEnrollments = await getMyEnrollments();
            const existing = myEnrollments.find((e) => e.courseId === courseData.id);
            if (existing) setEnrollmentId(existing.id);
          } catch {
            // non-fatal — enrollment check failure shouldn't block page
          }
        }
      } catch (err) {
        setError(err.message || 'Course not found.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [id, isStudent]);

  const handleEnroll = async () => {
    if (!user?.id) return;
    setEnrolling(true);
    setActionError('');
    setActionSuccess('');
    try {
      const result = await enrollCourse(user.id, course.id);
      setEnrollmentId(result.id);
      setActionSuccess('You have successfully enrolled in this course!');
    } catch (err) {
      setActionError(err.message || 'Enrollment failed.');
    } finally {
      setEnrolling(false);
    }
  };

  const handleUnenroll = async () => {
    if (!enrollmentId) return;
    if (!window.confirm('Drop this course? You can re-enroll later.')) return;
    setEnrolling(true);
    setActionError('');
    setActionSuccess('');
    try {
      await unenrollCourse(enrollmentId);
      setEnrollmentId(null);
      setActionSuccess('You have been unenrolled from this course.');
    } catch (err) {
      setActionError(err.message || 'Unenroll failed.');
    } finally {
      setEnrolling(false);
    }
  };

  if (loading) {
    return (
      <div className="page-wrapper">
        <div className="loading-state">
          <div className="loading-spinner" />
          <p>Loading course…</p>
        </div>
      </div>
    );
  }

  if (error || !course) {
    return (
      <div className="page-wrapper">
        <div className="alert alert-error">
          <span>⚠</span> {error || 'Course not found.'}
        </div>
        <button className="btn btn-secondary" onClick={() => navigate('/courses')} style={{ marginTop: '1rem' }}>
          ← Back to Courses
        </button>
      </div>
    );
  }

  const creditColor =
    course.credits >= 4 ? '#f59e0b' : course.credits >= 3 ? '#3b82f6' : '#10b981';

  return (
    <div className="page-wrapper">
      {/* Breadcrumb */}
      <nav className="breadcrumb" aria-label="breadcrumb">
        <Link to="/courses" className="breadcrumb-link">Courses</Link>
        <span className="breadcrumb-sep">›</span>
        <span>{course.title}</span>
      </nav>

      <div className="course-detail-layout">
        {/* Main content */}
        <div className="course-detail-main">
          <div className="course-detail-card">
            <div className="course-detail-header">
              <div className="course-credits-badge" style={{ background: creditColor, fontSize: '0.95rem' }}>
                {course.credits} {course.credits === 1 ? 'Credit' : 'Credits'}
              </div>
              <h1 className="course-detail-title">{course.title}</h1>
            </div>

            <div className="course-detail-body">
              <section className="detail-section">
                <h2 className="detail-section-title">About This Course</h2>
                <p className="course-detail-description">
                  {course.description || 'No description available for this course.'}
                </p>
              </section>

              <section className="detail-section">
                <h2 className="detail-section-title">Course Details</h2>
                <dl className="detail-list">
                  <div className="detail-row">
                    <dt>Instructor</dt>
                    <dd>{course.instructorName || `Instructor #${course.instructorId}`}</dd>
                  </div>
                  <div className="detail-row">
                    <dt>Credits</dt>
                    <dd>{course.credits}</dd>
                  </div>
                  <div className="detail-row">
                    <dt>Created</dt>
                    <dd>{new Date(course.createdAt).toLocaleDateString()}</dd>
                  </div>
                  {course.updatedAt && course.updatedAt !== course.createdAt && (
                    <div className="detail-row">
                      <dt>Last updated</dt>
                      <dd>{new Date(course.updatedAt).toLocaleDateString()}</dd>
                    </div>
                  )}
                </dl>
              </section>
            </div>
          </div>
        </div>

        {/* Sidebar */}
        <aside className="course-detail-sidebar">
          <div className="sidebar-card">
            <h2 className="sidebar-card-title">Enrollment</h2>

            {actionError && (
              <div className="alert alert-error" style={{ marginBottom: '1rem' }}>
                <span>⚠</span> {actionError}
              </div>
            )}
            {actionSuccess && (
              <div className="alert alert-success" style={{ marginBottom: '1rem' }}>
                <span>✓</span> {actionSuccess}
              </div>
            )}

            {isStudent ? (
              enrollmentId ? (
                <>
                  <div className="enrolled-status">
                    <span className="enrolled-badge" style={{ fontSize: '1rem', padding: '0.5rem 1rem' }}>
                      ✓ Enrolled
                    </span>
                  </div>
                  <button
                    className="btn btn-danger btn-full"
                    onClick={handleUnenroll}
                    disabled={enrolling}
                    style={{ marginTop: '1rem' }}
                  >
                    {enrolling ? <span className="loading-spinner-sm" /> : 'Drop Course'}
                  </button>
                </>
              ) : (
                <button
                  className="btn btn-primary btn-full"
                  onClick={handleEnroll}
                  disabled={enrolling}
                >
                  {enrolling ? <span className="loading-spinner-sm" /> : '✚ Enroll in Course'}
                </button>
              )
            ) : (
              <p className="td-muted" style={{ fontSize: '0.9rem' }}>
                {user?.role === 'Instructor'
                  ? 'Enrollment is available to students only.'
                  : 'Log in as a student to enroll.'}
              </p>
            )}

            <hr style={{ margin: '1.25rem 0', borderColor: 'var(--border)' }} />

            <Link to="/courses" className="btn btn-secondary btn-full">
              ← Back to Courses
            </Link>
          </div>
        </aside>
      </div>
    </div>
  );
}
