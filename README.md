# Smart Course Management System

A complete ASP.NET Core Web API and Frontend project for managing courses, instructors, students, and enrollments.

## 🚀 Features

### Backend (ASP.NET Core 10 Web API)
- **Entity Relationships**: Full implementation of One-to-One, One-to-Many, and Many-to-Many relationships using EF Core Fluent API.
- **Service Layer**: Decoupled business logic with Dependency Injection (5+ services).
- **JWT Authentication**: Secure stateless authentication with role-based claims.
- **Role-Based Authorization**: Policies for Admin, Instructor, and Student.
- **LINQ Optimization**: Performance-tuned queries using `AsNoTracking()` and `Select()` projections.
- **Swagger Documentation**: Interactive API testing with JWT Bearer support.

### Frontend (Vanilla HTML5/CSS3/JS)
- **Premium UI**: Modern dark-mode design with glassmorphism and micro-animations.
- **SPA Architecture**: Smooth navigation without page reloads.
- **JWT Integration**: Automatic token management and secure API fetch wrapper.
- **Dynamic Dashboard**: Personalized view based on user roles.

## 🛠️ Setup & Installation

1. **Clone the repository**:
   ```bash
   git clone [repository-url]
   cd Smart-Course-Management-Portal
   ```

2. **Database Setup**:
   The project uses SQL Server LocalDB. Run the migrations to create the database:
   ```bash
   cd SmartCourseManagement.API
   dotnet ef database update
   ```

3. **Run the Application**:
   ```bash
   dotnet run
   ```
   - **Frontend/Swagger**: [http://localhost:5202](http://localhost:5202)
   - **API Swagger Documentation**: [http://localhost:5202/swagger](http://localhost:5202/swagger)

## 🔑 Authentication Flow

1. **Register**: Navigate to the Register page and create an account as a "Student" or "Instructor".
2. **Login**: Sign in with your credentials to receive a JWT token.
3. **Explore**:
   - **Students**: Browse and enroll in courses.
   - **Instructors/Admins**: Access course management tools.

## 📂 Project Structure

- `SmartCourseManagement.API/`
  - `Controllers/`: API endpoints.
  - `Services/`: Business logic implementations.
  - `Data/`: DBContext and Migrations.
  - `DTOs/`: Data Transfer Objects for API contracts.
  - `Models/`: EF Core Entity models.
  - `wwwroot/`: Frontend static files (HTML, CSS, JS).

## ✅ Requirements Checklist

- [x] One-to-One relationship (User <-> InstructorProfile)
- [x] One-to-Many relationship (InstructorProfile <-> Course)
- [x] Many-to-Many relationship (Student <-> Course via Enrollment)
- [x] Service Layer + Dependency Injection
- [x] DTOs for Create/Update/Read operations
- [x] DTO Validation ([Required], [EmailAddress], etc.)
- [x] JWT Authentication
- [x] Role-Based Authorization ([Authorize(Policy = "...")])
- [x] LINQ Optimization (Select + AsNoTracking)
- [x] EF Core Migrations
- [x] Swagger UI with JWT Support
- [x] Premium Frontend UI