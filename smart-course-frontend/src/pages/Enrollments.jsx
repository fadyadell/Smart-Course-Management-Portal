import { useEffect, useMemo, useState } from 'react';
import {
  enrollCourse,
  getCourses,
  getMyEnrollments,
  getStudentById,
  getStudentEnrollments,
  unenrollCourse,
} from '../api/api';
import { useAuth } from '../context/AuthContext';

function normalizeList(data) {
  return Array.isArray(data) ? data : data?.items ?? [];
}

export default function Enrollments() {
  const { user, isStudent, isAdmin } = useAuth();

  const [courses, setCourses] = useState([]);
  const [coursesLoading, setCoursesLoading] = useState(false);
  const [coursesError, setCoursesError] = useState('');

  const [myEnrollments, setMyEnrollments] = useState([]);
  const [myEnrollmentsLoading, setMyEnrollmentsLoading] = useState(false);
  const [myEnrollmentsError, setMyEnrollmentsError] = useState('');

  const [selectedCourseId, setSelectedCourseId] = useState('');
  const [enrolling, setEnrolling] = useState(false);

  const [lookupStudentId, setLookupStudentId] = useState('');
  const [lookupLoading, setLookupLoading] = useState(false);
  const [lookupError, setLookupError] = useState('');
  const [lookupStudent, setLookupStudent] = useState(null);
  const [lookupEnrollments, setLookupEnrollments] = useState([]);

  const loadCourses = async () => {
    setCoursesLoading(true);
    setCoursesError('');

    try {
      const data = await getCourses();
      setCourses(normalizeList(data));
    } catch (err) {
      setCoursesError(err.message || 'Failed to load courses.');
    } finally {
      setCoursesLoading(false);
    }
  };

  const loadMyEnrollments = async () => {
    setMyEnrollmentsLoading(true);
    setMyEnrollmentsError('');

    try {
      const data = await getMyEnrollments();
      setMyEnrollments(normalizeList(data));
    } catch (err) {
      setMyEnrollmentsError(err.message || 'Failed to load enrollments.');
    } finally {
      setMyEnrollmentsLoading(false);
    }
  };

  const loadLookupEnrollments = async (studentId) => {
    if (!studentId) return;

    setLookupLoading(true);
    setLookupError('');

    try {
      const [student, enrollments] = await Promise.all([
        getStudentById(studentId),
        getStudentEnrollments(studentId),
      ]);

      setLookupStudent(student);
      setLookupEnrollments(normalizeList(enrollments));
    } catch (err) {
      setLookupStudent(null);
      setLookupEnrollments([]);
      setLookupError(err.message || 'Failed to load student enrollments.');
    } finally {
      setLookupLoading(false);
    }
  };

  useEffect(() => {
    if (isStudent) {
      loadCourses();
      loadMyEnrollments();
    }
  }, [isStudent]);

  const availableCourses = useMemo(() => {
    if (!isStudent) return courses;
    return courses.filter(
      (course) => !myEnrollments.some((enrollment) => enrollment.courseId === course.id)
    );
  }, [courses, isStudent, myEnrollments]);

  useEffect(() => {
    if (isStudent && availableCourses.length > 0 && !selectedCourseId) {
      setSelectedCourseId(String(availableCourses[0].id));
    }
  }, [availableCourses, isStudent, selectedCourseId]);

  const handleEnroll = async (event) => {
    event.preventDefault();
    if (!selectedCourseId) return;

    setEnrolling(true);
    setMyEnrollmentsError('');

    try {
      await enrollCourse(user.id, Number(selectedCourseId));
      await loadMyEnrollments();
    } catch (err) {
      setMyEnrollmentsError(err.message || 'Failed to enroll in course.');
    } finally {
      setEnrolling(false);
    }
  };

  const handleLookup = async (event) => {
    event.preventDefault();
    const studentId = Number(lookupStudentId);
    if (!studentId) return;

    await loadLookupEnrollments(studentId);
  };

  const handleUnenroll = async (enrollmentId) => {
    if (!window.confirm('Remove this enrollment?')) return;

    try {
      await unenrollCourse(enrollmentId);

      if (isStudent) {
        await loadMyEnrollments();
      } else if (lookupStudentId) {
        await loadLookupEnrollments(Number(lookupStudentId));
      }
    } catch (err) {
      alert(err.message);
    }
  };

  const activeEnrollments = isStudent ? myEnrollments : lookupEnrollments;

  return (
    <div className="page-wrapper">
      <div className="page-header">
        <div>
          <h1 className="page-title">Enrollments</h1>
          <p className="page-subtitle">
            {isStudent
              ? 'Enroll in courses and manage your active registrations'
              : 'Look up student enrollments and manage records'}
          </p>
        </div>
      </div>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
          gap: '1.5rem',
          alignItems: 'start',
        }}
      >
        <section className="manage-form-card">
          <h2 className="form-heading">
            {isStudent ? 'Enroll in a Course' : 'Find a Student'}
          </h2>

          {isStudent ? (
            <form onSubmit={handleEnroll} className="manage-form" noValidate>
              {coursesLoading && (
                <div className="loading-state">
                  <div className="loading-spinner" />
                  <p>Loading courses...</p>
                </div>
              )}

              {!coursesLoading && coursesError && (
                <div className="alert alert-error">
                  <span>⚠</span> {coursesError}
                  <button type="button" className="link-btn" onClick={loadCourses}>
                    Retry
                  </button>
                </div>
              )}

              {!coursesLoading && !coursesError && (
                <>
                  <div className="form-group">
                    <label htmlFor="courseId">Available Courses</label>
                    <select
                      id="courseId"
                      value={selectedCourseId}
                      onChange={(event) => setSelectedCourseId(event.target.value)}
                      disabled={availableCourses.length === 0}
                    >
                      {availableCourses.length === 0 ? (
                        <option value="">No courses available</option>
                      ) : (
                        <>
                          <option value="">Select a course</option>
                          {availableCourses.map((course) => (
                            <option key={course.id} value={course.id}>
                              {course.title} (#{course.id})
                            </option>
                          ))}
                        </>
                      )}
                    </select>
                  </div>

                  <p className="section-note">
                    {availableCourses.length === 0
                      ? 'You are already enrolled in every available course.'
                      : 'Pick a course to enroll yourself. The backend enforces the logged-in student.'}
                  </p>

                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={enrolling || availableCourses.length === 0}
                  >
                    {enrolling ? <span className="loading-spinner-sm" /> : 'Enroll'}
                  </button>
                </>
              )}
            </form>
          ) : (
            <form onSubmit={handleLookup} className="manage-form" noValidate>
              <div className="form-group">
                <label htmlFor="lookupStudentId">Student ID</label>
                <input
                  id="lookupStudentId"
                  type="number"
                  min={1}
                  value={lookupStudentId}
                  onChange={(event) => setLookupStudentId(event.target.value)}
                  placeholder="Enter a student ID"
                  required
                />
              </div>

              <p className="section-note">
                Use the Students page to find IDs quickly.
              </p>

              <button type="submit" className="btn btn-primary" disabled={lookupLoading}>
                {lookupLoading ? <span className="loading-spinner-sm" /> : 'Load Enrollments'}
              </button>
            </form>
          )}
        </section>

        <section className="manage-form-card">
          <h2 className="form-heading">
            {isStudent
              ? 'My Enrollments'
              : lookupStudent
              ? `Enrollments for ${lookupStudent.name}`
              : 'Enrollment Results'}
          </h2>

          {isStudent && myEnrollmentsLoading && (
            <div className="loading-state">
              <div className="loading-spinner" />
              <p>Loading your enrollments...</p>
            </div>
          )}

          {isStudent && myEnrollmentsError && (
            <div className="alert alert-error">
              <span>⚠</span> {myEnrollmentsError}
              <button type="button" className="link-btn" onClick={loadMyEnrollments}>
                Retry
              </button>
            </div>
          )}

          {!isStudent && lookupError && (
            <div className="alert alert-error">
              <span>⚠</span> {lookupError}
            </div>
          )}

          {!isStudent && lookupLoading && (
            <div className="loading-state">
              <div className="loading-spinner" />
              <p>Loading student enrollments...</p>
            </div>
          )}

          {!isStudent && lookupStudent && (
            <div className="info-banner" style={{ marginBottom: '1rem' }}>
              <span>👤</span>
              <p>
                {lookupStudent.name} ({lookupStudent.email})
              </p>
            </div>
          )}

          {!isStudent && !lookupStudent && !lookupLoading && !lookupError && (
            <div className="empty-state">
              <div className="empty-icon">🔎</div>
              <h3>No student selected</h3>
              <p>Enter a student ID to load their enrollment history.</p>
            </div>
          )}

          {activeEnrollments.length === 0 && (
            <div className="empty-state">
              <div className="empty-icon">🗂</div>
              <h3>No enrollments found</h3>
              <p>
                {isStudent
                  ? 'Head to the Courses page to enroll in a class.'
                  : 'Search for a student to review their enrollments.'}
              </p>
            </div>
          )}

          {activeEnrollments.length > 0 && (
            <div className="table-responsive">
              <table className="manage-table">
                <thead>
                  <tr>
                    <th>Course</th>
                    <th>Enrolled</th>
                    <th>Student</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {activeEnrollments.map((enrollment) => (
                    <tr key={enrollment.id}>
                      <td>
                        <strong>{enrollment.courseTitle || `Course #${enrollment.courseId}`}</strong>
                      </td>
                      <td className="td-muted">
                        {new Date(enrollment.enrollmentDate).toLocaleDateString()}
                      </td>
                      <td className="td-muted">
                        {enrollment.studentName || `#${enrollment.studentId}`}
                      </td>
                      <td>
                        {(isStudent || isAdmin) && (
                          <button
                            type="button"
                            className="btn btn-danger btn-sm"
                            onClick={() => handleUnenroll(enrollment.id)}
                          >
                            Drop
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
