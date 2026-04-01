# 🎓 Smart Course Management Portal

A professional-grade ASP.NET Core 10 Web API for managing courses, students, instructors, and enrollments. Built with enterprise patterns including JWT authentication with refresh tokens, pagination, soft delete, audit trails, global exception handling, structured logging, and rate limiting.

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client / Swagger UI                   │
└─────────────────────────┬───────────────────────────────────┘
                          │ HTTP/HTTPS
┌─────────────────────────▼───────────────────────────────────┐
│                  ASP.NET Core 10 Pipeline                    │
│  ExceptionHandlingMiddleware → Serilog → RateLimiter →       │
│  CORS → Authentication (JWT) → Authorization → Controllers   │
└─────────────────────────┬───────────────────────────────────┘
                          │ DI / Service Layer
┌─────────────────────────▼───────────────────────────────────┐
│   AuthService │ CourseService │ EnrollmentService │          │
│   StudentService │ InstructorService │ EmailService          │
└─────────────────────────┬───────────────────────────────────┘
                          │ Entity Framework Core 10
┌─────────────────────────▼───────────────────────────────────┐
│                    SQL Server (LocalDB)                       │
│  Users │ Courses │ Enrollments │ InstructorProfiles │        │
│  RefreshTokens                                               │
└─────────────────────────────────────────────────────────────┘
```

### Entity Relationships

```
User (1) ──────────── (0..1) InstructorProfile (1) ──── (N) Course
 │                                                            │
 └── (N) Enrollment ──────────────────────────────────── (1) ┘
 │
 └── (N) RefreshToken
```

---

## ✨ Feature Highlights

| Feature | Description |
|---|---|
| **JWT + Refresh Tokens** | Short-lived access tokens (1h) + long-lived refresh tokens (7d). Auto-rotated on each use. |
| **Pagination & Filtering** | All GET endpoints support `?page=1&pageSize=10&searchTerm=x&sortBy=y&sortDirection=asc` |
| **Soft Delete** | Records set `IsDeleted=true` instead of being removed. Hard-delete admin endpoint available. |
| **Global Exception Handling** | Middleware catches all exceptions and returns standardised JSON error responses. |
| **Serilog Logging** | Structured logging to console + rolling file (`logs/app-YYYYMMDD.txt`). Every request logged. |
| **Rate Limiting** | 100 req/min general policy. 5 req/min on login endpoint. Returns HTTP 429 when exceeded. |
| **Email Notifications** | Welcome email on registration. Enrollment confirmation on course join. (Mock logger in dev.) |
| **Audit Fields** | `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` auto-populated via `SaveChangesAsync` override. |
| **API Versioning** | All routes prefixed with `/api/v1/`. Forward-compatible design. |
| **Unit Tests** | 19 xUnit tests covering AuthService (9 tests) and CourseService (10 tests). |

---

## 📋 API Endpoints

### Authentication — `/api/v1/auth`

| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| POST | `/api/v1/auth/register` | ❌ | — | Register new user (Admin/Instructor/Student) |
| POST | `/api/v1/auth/login` | ❌ | — | Login, receive JWT + refresh token |
| POST | `/api/v1/auth/refresh` | ❌ | — | Exchange refresh token for new token pair |

### Courses — `/api/v1/courses`

| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/v1/courses` | ✅ | Any | Get paginated/filtered courses |
| GET | `/api/v1/courses/{id}` | ✅ | Any | Get course by ID |
| POST | `/api/v1/courses` | ✅ | Admin, Instructor | Create course |
| PUT | `/api/v1/courses/{id}` | ✅ | Admin, Instructor | Update course |
| DELETE | `/api/v1/courses/{id}` | ✅ | Admin, Instructor | Soft-delete course |
| DELETE | `/api/v1/courses/{id}/hard` | ✅ | Admin | Hard-delete course permanently |

### Students — `/api/v1/students`

| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/v1/students` | ✅ | Admin, Instructor | Get paginated students |
| GET | `/api/v1/students/{id}` | ✅ | Admin, Instructor | Get student by ID |

### Enrollments — `/api/v1/enrollments`

| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/v1/enrollments` | ✅ | Admin, Instructor | Get all enrollments (filtered) |
| GET | `/api/v1/enrollments/my-enrollments` | ✅ | Student | Get own enrollments |
| GET | `/api/v1/enrollments/student/{id}` | ✅ | Admin, Instructor | Get enrollments for a student |
| POST | `/api/v1/enrollments` | ✅ | Student | Enroll in a course (self-only) |
| DELETE | `/api/v1/enrollments/{id}` | ✅ | Admin, Student | Soft-delete (unenroll) |

### Instructors — `/api/v1/instructors`

| Method | Route | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/v1/instructors` | ✅ | Any | Get all instructor profiles |
| GET | `/api/v1/instructors/{id}` | ✅ | Any | Get instructor by ID |
| PUT | `/api/v1/instructors/profile` | ✅ | Instructor | Update own profile |

---

## 🗄️ Database Schema

### Users
| Column | Type | Notes |
|---|---|---|
| Id | int PK | Auto-increment |
| Name | nvarchar(100) | |
| Email | nvarchar(200) | Unique index |
| PasswordHash | nvarchar(max) | BCrypt hash |
| Role | nvarchar(20) | Admin / Instructor / Student |
| IsDeleted | bit | Soft delete flag |
| CreatedAt | datetime2 | Auto-set by SaveChangesAsync |
| UpdatedAt | datetime2 | Auto-updated by SaveChangesAsync |
| CreatedBy | nvarchar(max) | Email of creator |
| UpdatedBy | nvarchar(max) | Email of last updater |

### Courses, Enrollments, InstructorProfiles
Same audit columns as Users plus their entity-specific columns.

### RefreshTokens
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Token | nvarchar(max) | Random 512-bit base64 |
| UserId | int FK | Cascades on User delete |
| ExpiresAt | datetime2 | 7 days from issue |
| IsUsed | bit | True after single use |
| IsRevoked | bit | True if manually revoked |
| CreatedAt | datetime2 | |

---

## 🚀 Setup Instructions

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB, Express, or full)

### 1. Clone & Configure
```bash
git clone https://github.com/fadyadell/Smart-Course-Management-Portal
cd Smart-Course-Management-Portal/SmartCourseManagement.API
```

Edit `appsettings.json` to point to your SQL Server instance:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartCourseManagementDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "YourSecretKeyAtLeast32CharactersLong!",
    "Issuer": "SmartCourseManagement",
    "Audience": "SmartCourseManagementUsers"
  }
}
```

### 2. Run the API
```bash
dotnet run
```

Migrations are applied automatically on startup. The API starts at `http://localhost:5202`.

### 3. Open Swagger UI
Navigate to `http://localhost:5202/swagger` to explore and test all endpoints interactively.

---

## 🧪 Testing Instructions

### Run all unit tests
```bash
cd SmartCourseManagement.Tests
dotnet test
```

Expected output:
```
Passed! - Failed: 0, Passed: 19, Skipped: 0, Total: 19
```

### Test via Swagger UI

1. **Register** — POST `/api/v1/auth/register`
```json
{
  "name": "Alice Smith",
  "email": "alice@example.com",
  "password": "Password123",
  "role": "Student"
}
```

2. **Copy** the `token` from the response, click **Authorize** at the top, paste it.

3. **Test paginated courses** — GET `/api/v1/courses?page=1&pageSize=2&searchTerm=asp`

4. **Test refresh** — POST `/api/v1/auth/refresh`
```json
{ "refreshToken": "<refreshToken from login response>" }
```

---

## ⚙️ Environment Configuration

| Setting | Description | Default |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | LocalDB |
| `Jwt:Key` | HMAC-SHA256 signing key (≥32 chars) | — |
| `Jwt:Issuer` | JWT issuer claim | SmartCourseManagement |
| `Jwt:Audience` | JWT audience claim | SmartCourseManagementUsers |

Logs are written to `logs/app-YYYYMMDD.txt` with daily rotation.

---

## 📁 Project Structure

```
SmartCourseManagement.API/
├── Controllers/          # HTTP endpoints (versioned /api/v1/)
├── Data/                 # AppDbContext with soft-delete filters + audit override
├── DTOs/                 # Input/output data shapes incl. PagedRequest/PagedResponse
├── Middleware/           # ExceptionHandlingMiddleware
├── Migrations/           # EF Core database migrations
├── Models/               # BaseEntity + domain entities
├── Services/             # Business logic + interfaces
├── logs/                 # Serilog rolling log files (created at runtime)
└── Program.cs            # App bootstrap: Serilog, JWT, Rate Limiting, CORS, DI

SmartCourseManagement.Tests/
├── AuthServiceTests.cs   # 9 tests for registration, login, token refresh
└── CourseServiceTests.cs # 10 tests for pagination, filtering, CRUD, soft delete
```
