import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Navbar from './components/Navbar';
import PrivateRoute from './components/PrivateRoute';

// Pages
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Courses from './pages/Courses';
import MyEnrollments from './pages/MyEnrollments';
import ManageCourses from './pages/ManageCourses';
import Students from './pages/Students';
import Instructors from './pages/Instructors';


// ─────────────────────────────────────────────
//  Public-only route — redirect authenticated users away from login
// ─────────────────────────────────────────────
function PublicRoute({ children }) {
  const { isAuthenticated, loading } = useAuth();
  if (loading) return null;
  return isAuthenticated ? <Navigate to="/dashboard" replace /> : children;
}

// ─────────────────────────────────────────────
//  App Shell with Navbar
// ─────────────────────────────────────────────
function AppShell() {
  return (
    <div className="app-shell">
      <Navbar />
      <main className="app-main">
        <Routes>
          {/* Public */}
          <Route
            path="/"
            element={
              <PublicRoute>
                <Login />
              </PublicRoute>
            }
          />

          {/* Private — any authenticated role */}
          <Route
            path="/dashboard"
            element={
              <PrivateRoute>
                <Dashboard />
              </PrivateRoute>
            }
          />
          <Route
            path="/courses"
            element={
              <PrivateRoute>
                <Courses />
              </PrivateRoute>
            }
          />

          {/* Private — Student only */}
          <Route
            path="/my-enrollments"
            element={
              <PrivateRoute roles={['Student']}>
                <MyEnrollments />
              </PrivateRoute>
            }
          />

          {/* Private — Admin or Instructor */}
          <Route
            path="/manage-courses"
            element={
              <PrivateRoute roles={['Admin', 'Instructor']}>
                <ManageCourses />
              </PrivateRoute>
            }
          />

          <Route
            path="/students"
            element={
              <PrivateRoute roles={['Admin', 'Instructor']}>
                <Students />
              </PrivateRoute>
            }
          />

          <Route
            path="/instructors"
            element={
              <PrivateRoute>
                <Instructors />
              </PrivateRoute>
            }
          />

          {/* Catch-all */}
          <Route path="*" element={<Navigate to="/" replace />} />

        </Routes>
      </main>
    </div>
  );
}

// ─────────────────────────────────────────────
//  Root
// ─────────────────────────────────────────────
export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppShell />
      </AuthProvider>
    </BrowserRouter>
  );
}
