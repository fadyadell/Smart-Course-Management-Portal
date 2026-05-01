import { useEffect, useState } from 'react';
import { getStudents, getStudentEnrollments } from '../api/api';
import { useAuth } from '../context/AuthContext';

export default function Students() {
  const { isAdmin, isInstructor } = useAuth();
  const [students, setStudents] = useState([]);
  const [selectedStudent, setSelectedStudent] = useState(null);
  const [enrollments, setEnrollments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchStudents();
  }, []);

  const fetchStudents = async () => {
    try {
      setLoading(true);
      const data = await getStudents();
      setStudents(data);
    } catch (err) {
      setError('Failed to load students.');
    } finally {
      setLoading(false);
    }
  };

  const handleViewEnrollments = async (student) => {
    try {
      setSelectedStudent(student);
      const data = await getStudentEnrollments(student.userId);
      setEnrollments(data);
    } catch (err) {
      console.error('Failed to load enrollments', err);
    }
  };

  if (!isAdmin && !isInstructor) {
    return <div className="page-wrapper">Access Denied.</div>;
  }

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
                {students.map((student) => (
                  <tr key={student.userId}>
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
                ))}
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
