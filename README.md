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

## 🔒 Security: JWT vs HTTP-Only Cookies (Industry Best Practices)

### Current Implementation: JWT in Authorization Header
This project uses **JWT tokens in the Authorization header** for a learning environment and SPA architecture:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Advantages:**
- ✅ Stateless authentication (no server session storage needed)
- ✅ Works perfectly with SPAs (React, Vue, Angular)
- ✅ Cross-origin requests (CORS-friendly)
- ✅ Mobile-friendly (iOS, Android apps)

**Security Trade-off:**
- ⚠️ Vulnerable to XSS attacks if localStorage is compromised
- ⚠️ Requires developer discipline to prevent XSS vulnerabilities

---

### Industry Standard: HTTP-Only Cookies
Production systems typically use **HTTP-only cookies** for authentication tokens:

| Aspect | JWT (Current) | HTTP-Only Cookies |
|--------|---------------|-------------------|
| **XSS Risk** | ⚠️ Vulnerable to malicious JS accessing localStorage | ✅ Protected (browser-managed, JS cannot access) |
| **CSRF Risk** | ✅ Safe (stateless, token in header) | 🔧 Requires CSRF token middleware |
| **Theft Method** | JavaScript access or network interception | Network interception only |
| **Best For** | SPAs, Mobile apps, Microservices | Traditional server-rendered web apps |
| **Stateless** | ✅ Yes | ❌ Requires session validation |

---

### Why HTTP-Only Cookies are Industry Standard

1. **Automatic XSS Protection**: Even if attacker injects malicious JS, they cannot access HTTP-only cookies
2. **Browser Compliance**: Built-in cookie management by the browser
3. **OWASP Recommendation**: OWASP Security Testing Guide recommends HTTP-only + Secure + SameSite flags
4. **Automatic Transmission**: Browser automatically includes cookie in requests (no manual header management)

**Example HTTP-Only Cookie Response:**
```http
Set-Cookie: authToken=eyJhbGc...; 
           HttpOnly; 
           Secure; 
           SameSite=Strict; 
           Max-Age=3600; 
           Path=/api
```

---

### Migration Path (For Production)

If this project were going to production, we would:

1. **Use HTTP-Only Cookies**:
   ```csharp
   // Instead of returning token in JSON:
   // return Ok(new { token = jwtToken });
   
   // Set as HTTP-only cookie:
   HttpContext.Response.Cookies.Append("authToken", jwtToken, 
       new CookieOptions 
       { 
           HttpOnly = true,           // JS cannot access
           Secure = true,             // HTTPS only
           SameSite = SameSiteMode.Strict,  // CSRF protection
           MaxAge = TimeSpan.FromHours(1)
       });
   ```

2. **Add CSRF Protection**:
   ```csharp
   // services.AddAntiforgery();
   // Protect POST/PUT/DELETE endpoints with [ValidateAntiForgeryToken]
   ```

3. **Implement Refresh Token Rotation**:
   ```csharp
   // Short-lived access token (15 mins) + Long-lived refresh token (7 days)
   // Refresh endpoint to get new access token using refresh token
   ```

4. **Add Security Headers**:
   ```csharp
   // Content-Security-Policy
   // X-Content-Type-Options: nosniff
   // X-Frame-Options: DENY
   // Strict-Transport-Security
   ```

---

### Current Security Measures (This Project)

✅ **Already Implemented:**
- JWT signed tokens (cannot be tampered with)
- 32+ character secret key
- Role-based authorization
- Token expiry (1 hour)
- CORS properly configured
- Passwords hashed with BCrypt (in auth validation)
- Async/await (prevents blocking)

🔧 **Recommended for Production:**
- Implement HTTP-only cookies instead of localStorage
- Add refresh token mechanism
- Enable HTTPS only
- Add rate limiting
- Implement API key rotation
- Add comprehensive logging and monitoring

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