# 🎓 Smart Course Management Portal

![.NET Core](https://img.shields.io/badge/.NET%2010-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)
![Entity Framework Core](https://img.shields.io/badge/EF_Core_10-5C2D91?style=for-the-badge&logo=nuget&logoColor=white)
![JWT Auth](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=JSON%20web%20tokens&logoColor=white)

A modern, full-stack educational platform built to demonstrate enterprise-grade architecture. It features a high-performance **ASP.NET Core 10 Web API** backend and a dynamic **React (Vite)** frontend.

This project is built with a focus on clean architecture, security, and performance, making it a perfect template for modern educational systems.

---

## ✨ Key Features

- **Role-Based Access Control (RBAC):** Distinct experiences, dashboards, and permissions for Students, Instructors, and Administrators.
- **Secure Authentication:** Industry-standard JWT (JSON Web Tokens) with refresh token rotation and military-grade BCrypt password hashing.
- **Advanced Data Management:** Pagination, filtering, and sorting out-of-the-box using highly optimized EF Core projections (`.AsNoTracking()`).
- **Audit Trails & Soft Deletes:** Automatic tracking of `CreatedAt`, `UpdatedAt`, and soft-deletion mechanisms via custom EF Core context hooks.
- **Background Processing:** Scheduled cron jobs (like auto-unenrollment and token cleanup) handled gracefully by **Hangfire**.
- **Rate Limiting & Reliability:** Built-in rate limiting (100 requests/minute) and global exception handling middleware.

---

## 🛠️ Technology Stack

### Backend (.NET Web API)
- **Framework:** ASP.NET Core 10 Web API
- **ORM:** Entity Framework Core 10 (SQL Server / LocalDB)
- **Authentication:** JWT Bearer Authentication
- **Security:** BCrypt.Net-Next
- **Background Jobs:** Hangfire
- **API Documentation:** Swagger / OpenAPI

### Frontend (Single Page Application)
- **Framework:** React 18 (Bootstrapped with Vite)
- **Routing:** React Router DOM
- **HTTP Client:** Axios (with auth interceptors)
- **Styling:** Vanilla CSS (Glassmorphism & Dark Mode Support)

---

## 🚀 Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js & npm](https://nodejs.org/)
- SQL Server or LocalDB

### 1. Backend Setup

```bash
# Clone the repository
git clone https://github.com/fadyadell/Smart-Course-Management-Portal.git
cd Smart-Course-Management-Portal/SmartCourseManagement.API

# Restore dependencies
dotnet restore

# Apply database migrations to create the SQL schema
dotnet ef database update

# Run the API
dotnet run
```
The API will be available at `http://localhost:5202`. You can view the interactive **Swagger documentation** at `http://localhost:5202/swagger`.

### 2. Frontend Setup

```bash
# Open a new terminal and navigate to the frontend directory
cd Smart-Course-Management-Portal/smart-course-frontend

# Install dependencies
npm install

# Start the development server
npm run dev
```
The React frontend will be available at `http://localhost:5173`.

---

## 🔑 Test Credentials

You can use the following default credentials to explore the platform without registering a new account:

| Role | Email | Password |
|------|-------|----------|
| **Instructor** | instructor@example.com | `InstructorPass123!` |
| **Student** | student@example.com | `StudentPass123!` |

### Entity Relationships

## 📂 Architecture Overview

The backend strictly follows an **N-Tier Architecture** to maintain separation of concerns:

* **Controllers (`/Controllers`)**: Thin routers that handle incoming HTTP requests, validate JWT claims, and route data to services.
* **Services (`/Services`)**: The core business logic layer. All data processing, validation, and EF Core database calls live here.
* **DTOs (`/DTOs`)**: Data Transfer Objects ensure that database entities are never directly exposed to the client, preventing over-posting attacks and hiding sensitive data.
* **Models (`/Models`)**: EF Core entity configurations, table mappings, and database relationships (1-to-1, 1-to-Many, Many-to-Many).

<details>
<summary><b>🛡️ Click to read about our Security & Auth Implementation</b></summary>
<br>

This project uses **JWT tokens in the Authorization header** for stateless authentication, which is the industry standard for Single Page Applications (SPAs). 

**Security Measures Implemented:**
- **Stateless Tokens**: JWT signed tokens ensure the server doesn't need to look up session IDs.
- **Token Rotation**: Access tokens expire quickly (e.g., 15 minutes) for security. Long-lived refresh tokens are used to seamlessly mint new access tokens without requiring the user to log in again.
- **Role-Based Authorization**: Endpoints are strictly locked down using `[Authorize(Roles = "...")]`.
- **Password Security**: Passwords are never stored in plain text. They are hashed and salted with BCrypt.
- **Global Exception Handling**: Custom middleware catches any fatal crashes and returns standard JSON error messages, preventing stack traces from leaking server infrastructure details to the client.

</details>

---


---

## 🛣️ API Endpoints

The frontend communicates with the backend via the following RESTful endpoints:

### Authentication
| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| `POST` | `/api/auth/register` | Create a new account | Public |
| `POST` | `/api/auth/login` | Authenticate and get JWT | Public |
| `POST` | `/api/auth/refresh` | Refresh expired access tokens | Public |

### Courses
| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| `GET` | `/api/courses` | List all available courses | Authenticated |
| `GET` | `/api/courses/search` | Search & Paginated courses | Authenticated |
| `GET` | `/api/courses/{id}` | Get specific course details | Authenticated |
| `POST` | `/api/courses` | Create a new course | Admin, Instructor |
| `PUT` | `/api/courses/{id}` | Update existing course | Admin, Instructor |
| `DELETE` | `/api/courses/{id}` | Remove a course | Admin |

### Enrollments
| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| `GET` | `/api/enrollments/my-enrollments` | View your course list | Student |
| `POST` | `/api/enrollments` | Enroll in a new course | Student |
| `DELETE` | `/api/enrollments/{id}` | Unenroll from a course | Student, Admin |

---

## 📸 Application Screenshots

### 🏠 Home & Authentication
![Home](website%20screenshoots/home%20and%20login/Screenshot%202026-05-02%20191548.png)
![Login](website%20screenshoots/home%20and%20login/Screenshot%202026-05-02%20191603.png)

### 🎓 Student Experience
![Student Dashboard](website%20screenshoots/student/Screenshot%202026-05-02%20191641.png)
![Student Courses](website%20screenshoots/student/Screenshot%202026-05-02%20191658.png)
![Student Enrollments](website%20screenshoots/student/Screenshot%202026-05-02%20191712.png)
![Student Profile](website%20screenshoots/student/Screenshot%202026-05-02%20191724.png)

### 👨‍🏫 Instructor Experience
![Instructor Dashboard](website%20screenshoots/instructor/Screenshot%202026-05-02%20191920.png)
![Course Management](website%20screenshoots/instructor/Screenshot%202026-05-02%20191936.png)
![Student List](website%20screenshoots/instructor/Screenshot%202026-05-02%20191947.png)
![Add Course](website%20screenshoots/instructor/Screenshot%202026-05-02%20192543.png)
![Instructor Tools](website%20screenshoots/instructor/Screenshot%202026-05-02%20192609.png)
![Instructor Profile](website%20screenshoots/instructor/Screenshot%202026-05-02%20192617.png)
![Settings](website%20screenshoots/instructor/Screenshot%202026-05-02%20192715.png)

### 🛡️ Administrator Experience
![Admin Overview](website%20screenshoots/admin/Screenshot%202026-05-02%20191802.png)
![User Management](website%20screenshoots/admin/Screenshot%202026-05-02%20191815.png)
![System Statistics](website%20screenshoots/admin/Screenshot%202026-05-02%20191830.png)
![Global Settings](website%20screenshoots/admin/Screenshot%202026-05-02%20191839.png)
![Audit Logs](website%20screenshoots/admin/Screenshot%202026-05-02%20191852.png)


---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! 
Feel free to check the [issues page](https://github.com/fadyadell/Smart-Course-Management-Portal/issues) to see what we are currently working on.

## 📝 License

This project is licensed under the **ISC License**.