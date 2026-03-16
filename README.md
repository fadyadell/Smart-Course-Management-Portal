# Smart Course Management Portal

A modern, full-stack application built with **ASP.NET Core Web API** and a **responsive Vanilla HTML/CSS/JS frontend**. This portal allows administrators, instructors, and students to manage and interact with educational courses efficiently.

## Technologies Used

- **Backend**: 
  - **ASP.NET Core 10.0 Web API**: Core framework for building RESTful services.
  - **Entity Framework Core**: ORM for database operations and migrations.
  - **SQL Server**: Relational database (using LocalDB).
  - **JWT Authentication**: Secure stateless authentication using JSON Web Tokens.
  - **BCrypt.Net**: Industry-standard password hashing.
  - **Swagger/OpenAPI**: API documentation and testing interface.
  - **LINQ**: Optimized data querying with `Select()` projections and `AsNoTracking()`.

- **Frontend**:
  - **Vanilla HTML5 & CSS3**: Clean, modern UI with Glassmorphism and responsive design.
  - **Modern JavaScript (ES6+)**: Dynamic content rendering using the `Fetch API`.
  - **Google Fonts (Inter)**: Premium typography for professional aesthetics.

## How to Run the Project

### Prerequisites
- .NET 10 SDK
- SQL Server LocalDB (standard with Visual Studio/SDK installation)

### Backend Setup
1. Open a terminal in `./SmartCourseManagement.API`.
2. Run database migrations:
   ```bash
   dotnet ef database update
   ```
3. Start the API:
   ```bash
   dotnet run
   ```
4. Note the URL (e.g., `https://localhost:7153`) and update it in `./SmartCourseManagement.Frontend/js/api.js` if necessary.

### Frontend Setup
1. Simply open `./SmartCourseManagement.Frontend/login.html` in any modern web browser.
2. Register a new account as a Student, Instructor, or Admin.

---

## Technical Discussion: HTTP-only Cookies

### Why use HTTP-only Cookies for Authentication?

In many modern web applications, **HTTP-only cookies** are preferred over local storage for storing sensitive authentication tokens (like JWTs) for the following reasons:

1. **Protection against XSS (Cross-Site Scripting)**: Cookies with the `HttpOnly` flag cannot be accessed by client-side JavaScript. This prevents malicious scripts from stealing the session token if the application has an XSS vulnerability.
2. **Built-in Browser Security**: Browsers automatically include cookies in requests to the originating domain, allowing for more secure and standardized state management.
3. **Defense in Depth**: While researchers often debate storage methods, `HttpOnly` combined with `SameSite=Strict` and `Secure` flags provides a robust multi-layered defense against both XSS and CSRF (Cross-Site Request Forgery).

*Note: In this specific implementation, for simplicity in a decoupled assignment environment, LocalStorage is used, but industry-standard production apps typically adopt HTTP-only cookies.*