import { useEffect, useState } from 'react';
import { getStudents, getStudentEnrollments, getInstructorCoursesEnrollments, unenrollCourse } from '../api/api';
import { useAuth } from '../context/AuthContext';

export default function Students() {
  const { isAdmin, isInstructor } = useAuth();
  const [students, setStudents] = useState([]);
  const [selectedStudent, setSelectedStudent] = useState(null);
  const [enrollments, setEnrollments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dropping, setDropping] = useState(false);

  useEffect(() => {
    if (isInstructor) {
      fetchInstructorCourseEnrollments();
    } else if (isAdmin) {
      fetchStudents();
    }
  }, [isInstructor, isAdmin]);

  const fetchStudents = async () => {
    try {
      setLoading(true);
      const data = await getStudents();
      setStudents(data.items);
    } catch (err) {
      setError('Failed to load students.');
    } finally {
      setLoading(false);
    }
  };

  const fetchInstructorCourseEnrollments = async () => {
    try {
      setLoading(true);
      const data = await getInstructorCoursesEnrollments();
      const items = Array.isArray(data) ? data : data.items || [];
      setEnrollments(items);
      setError('');
    } catch (err) {
      setError('Failed to load enrollments.');
    } finally {
      setLoading(false);
    }
  };

  const handleViewEnrollments = async (student) => {
    try {
      setSelectedStudent(student);
      const data = await getStudentEnrollments(student.userId);
      setEnrollments(data.items || data);
    } catch (err) {
      console.error('Failed to load enrollments', err);
    }
  };

  const handleDropStudent = async (enrollmentId, studentName, courseName) => {
    if (!window.confirm(`Drop ${studentName} from ${courseName}?`)) return;
    
    setDropping(true);
    try {
      await unenrollCourse(enrollmentId);
      // Refresh the enrollments list
      await fetchInstructorCourseEnrollments();
    } catch (err) {
      alert(err.message || 'Failed to drop student.');
    } finally {
      setDropping(false);
    }
  };

  if (!isAdmin && !isInstructor) {
    return <div className="page-wrapper">Access Denied.</div>;
  }

  // Instructor view - show their course enrollments
  if (isInstructor && !isAdmin) {
    return (
      <div className="page-wrapper">
        <div className="page-header">
          <h1>My Course Students</h1>
          <p>Manage students enrolled in your courses</p>
        </div>

        {loading && (
          <div className="loading-state">
            <div className="loading-spinner" />
            <p>Loading enrollments…</p>
          </div>
        )}

        {!loading && error && (
          <div className="alert alert-error">
            <span>⚠</span> {error}
          </div>
        )}

        {!loading && !error && enrollments.length === 0 && (
          <div className="empty-state">
            <div className="empty-icon">📭</div>
            <h3>No students enrolled yet</h3>
            <p>Students will appear here once they enroll in your courses.</p>
          </div>
        )}

        {!loading && !error && enrollments.length > 0 && (
          <div className="manage-table-wrapper">
            <table className="manage-table">
              <thead>
                <tr>
                  <th>Student Name</th>
                  <th>Course</th>
                  <th>Enrolled Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {enrollments.map((enrollment) => (
                  <tr key={enrollment.id}>
                    <td>{enrollment.studentName}</td>
                    <td>{enrollment.courseTitle}</td>
                    <td>{new Date(enrollment.enrollmentDate).toLocaleDateString()}</td>
                    <td>
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() => handleDropStudent(enrollment.id, enrollment.studentName, enrollment.courseTitle)}
                        disabled={dropping}
                      >
                        {dropping ? '...' : '🗑 Drop'}
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

  // Admin view - show all students directory
  return (
    <div className="page-wrapper">
      <div className="page-header">
        <h1>Student Directory</h1>
        <p>Manage and view all registered students.</p>
      </div>

      {loading ? (
        <div className="loading-spinner" />
      ) : error ? (
        <div className="alert alert-error">{error}</div>
      ) : (
        <div className="content-grid">
          <div className="table-container card">
            <table className="data-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {students && students.length > 0 ? (
                  students.map((student, index) => (
                    <tr key={index}>
                      <td>{student.userId}</td>
                      <td>{student.name}</td>
                      <td>{student.email}</td>
                      <td>
                        <button 
                          className="btn btn-secondary btn-sm"
                          onClick={() => handleViewEnrollments(student)}
                        >
                          View Enrollments
                        </button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="4" style={{textAlign: 'center'}}>No students found</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {selectedStudent && (
            <div className="side-panel card animation-slide-in">
              <div className="panel-header">
                <h3>Enrollments for {selectedStudent.name}</h3>
                <button 
                  className="btn-close"
                  onClick={() => setSelectedStudent(null)}
                >
                  ×
                </button>
              </div>
              <div className="panel-body">
                {enrollments.length === 0 ? (
                  <p className="empty-state">No active enrollments found.</p>
                ) : (
                  <ul className="enrollment-list">
                    {enrollments.map((en) => (
                      <li key={en.id} className="enrollment-item">
                        <div className="en-info">
                          <strong>{en.courseTitle}</strong>
                          <span>Enrolled on: {new Date(en.enrollmentDate).toLocaleDateString()}</span>
                        </div>
                        <span className="status-badge status-active">
                          Active
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
