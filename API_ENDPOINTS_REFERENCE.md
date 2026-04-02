# API Endpoints Reference

Complete documentation of all Smart Course Management Portal API endpoints with request/response examples.

**Base URL**: `http://localhost:5202/api`  
**Documentation**: http://localhost:5202/swagger

---

## 📋 Table of Contents

1. [Authentication](#-authentication)
2. [Courses](#-courses)
3. [Enrollments](#-enrollments)
4. [Students](#-students)
5. [Instructors](#-instructors)
6. [Error Responses](#-error-responses)
7. [Authentication Header](#-authentication-header)

---

## 🔐 Authentication

### POST /auth/register

Create a new user account.

**Endpoint**: `POST /api/auth/register`

**Request Body**:
```json
{
  "email": "student@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Student"
}
```

**Request Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| email | string | Yes | User email (must be unique) |
| password | string | Yes | Password (min 8 chars, 1 uppercase, 1 number, 1 special) |
| firstName | string | Yes | User's first name |
| lastName | string | Yes | User's last name |
| role | string | Yes | Role: "Student", "Instructor", or "Admin" |

**Response** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "student@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Student"
}
```

**Error Responses**:
- `400 Bad Request` - Invalid email format or password weak
- `409 Conflict` - Email already registered

---

### POST /auth/login

Authenticate user and receive JWT token.

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "email": "instructor@example.com",
  "password": "InstructorPass123!"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedtoken==",
  "accessTokenExpiry": "2026-04-01T10:30:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "email": "instructor@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "role": "Instructor"
  }
}
```

**Response Fields**:
| Field | Type | Description |
|-------|------|-------------|
| accessToken | string | JWT token for API requests (15 min expiry) |
| refreshToken | string | Base64 encoded refresh token (7 day expiry) |
| accessTokenExpiry | datetime | When access token expires |
| user | object | User profile information |

**Error Responses**:
- `401 Unauthorized` - Invalid credentials
- `400 Bad Request` - Email not found

**Usage**:
```bash
# Save tokens in localStorage
localStorage.setItem('accessToken', response.accessToken);
localStorage.setItem('refreshToken', response.refreshToken);

# Use in subsequent requests
Authorization: Bearer <accessToken>
```

---

### POST /auth/refresh

Get a new access token using refresh token.

**Endpoint**: `POST /api/auth/refresh`

**Request Body**:
```json
{
  "refreshToken": "base64encodedtoken=="
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accessTokenExpiry": "2026-04-01T10:45:00Z"
}
```

**Error Responses**:
- `401 Unauthorized` - Refresh token invalid or expired
- `400 Bad Request` - Token not provided

**Automatic Refresh Flow** (Frontend):
```javascript
// If API returns 401:
const newToken = await refreshAccessToken();
// Retry original request with new token
```

---

## 🎓 Courses

### GET /courses

Get all courses with pagination.

**Endpoint**: `GET /api/courses?page=1&pageSize=10`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | integer | 1 | Page number (1-based) |
| pageSize | integer | 10 | Items per page (1-100) |

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "title": "ASP.NET Core Advanced Techniques",
      "description": "Master async/await, EF Core, and Azure deployment",
      "credits": 3,
      "maxStudents": 30,
      "currentEnrollment": 18,
      "instructorId": "550e8400-e29b-41d4-a716-446655440001",
      "instructorName": "Dr. Jane Smith",
      "startDate": "2026-04-01T00:00:00Z",
      "endDate": "2026-05-20T00:00:00Z",
      "isEnrolled": false
    }
  ],
  "totalItems": 42,
  "totalPages": 5,
  "currentPage": 1,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Authentication**: Required  
**Authorization**: None (all authenticated users)

---

### GET /courses/search

Search and filter courses with advanced options.

**Endpoint**: `GET /api/courses/search?searchTerm=ASP&sortBy=Title&sortDescending=false&page=1&pageSize=10`

**Query Parameters**:
| Parameter | Type | Description | Example |
|-----------|------|-------------|----|
| searchTerm | string | Search in title & description | "ASP" |
| sortBy | string | Sort column: Title, StartDate, Credits, EnrollmentCount | "Title" |
| sortDescending | boolean | Descending order? true/false | "false" |
| page | integer | Page number | "1" |
| pageSize | integer | Items per page | "10" |

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "title": "ASP.NET Core Advanced Techniques",
      "description": "Master async/await, EF Core, and Azure deployment",
      "credits": 3,
      "maxStudents": 30,
      "currentEnrollment": 15,
      "instructorId": "550e8400-e29b-41d4-a716-446655440001",
      "instructorName": "Dr. Jane Smith",
      "startDate": "2026-04-01T00:00:00Z",
      "endDate": "2026-05-20T00:00:00Z",
      "isEnrolled": false
    }
  ],
  "totalItems": 1,
  "totalPages": 1,
  "currentPage": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Example Requests**:
```bash
# Search for ASP courses
GET /api/courses/search?searchTerm=ASP&page=1&pageSize=10

# Sort by credits descending
GET /api/courses/search?sortBy=Credits&sortDescending=true

# Find courses starting soon, sorted by start date
GET /api/courses/search?sortBy=StartDate&sortDescending=false&pageSize=5
```

**Authentication**: Required

---

### GET /courses/{id}

Get detailed information about a specific course.

**Endpoint**: `GET /api/courses/{courseId}`

**Path Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| courseId | GUID | Course ID |

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440010",
  "title": "ASP.NET Core Advanced Techniques",
  "description": "Master async/await, EF Core, and Azure deployment",
  "credits": 3,
  "maxStudents": 30,
  "currentEnrollment": 15,
  "instructorId": "550e8400-e29b-41d4-a716-446655440001",
  "instructorName": "Dr. Jane Smith",
  "startDate": "2026-04-01T00:00:00Z",
  "endDate": "2026-05-20T00:00:00Z",
  "isEnrolled": false,
  "enrolledStudents": 15,
  "availableSeats": 15
}
```

**Error Responses**:
- `404 Not Found` - Course doesn't exist

**Authentication**: Required

---

### POST /courses

Create a new course (Instructor/Admin only).

**Endpoint**: `POST /api/courses`

**Request Body**:
```json
{
  "title": "Web Development with React",
  "description": "Learn building modern web applications using React",
  "credits": 4,
  "maxStudents": 25,
  "startDate": "2026-04-15T00:00:00Z",
  "endDate": "2026-05-30T00:00:00Z"
}
```

**Response** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440050",
  "title": "Web Development with React",
  "description": "Learn building modern web applications using React",
  "credits": 4,
  "maxStudents": 25,
  "currentEnrollment": 0,
  "instructorId": "550e8400-e29b-41d4-a716-446655440001",
  "startDate": "2026-04-15T00:00:00Z",
  "endDate": "2026-05-30T00:00:00Z",
  "createdAt": "2026-04-01T09:00:00Z"
}
```

**Authentication**: Required  
**Authorization**: Instructor or Admin

**Error Responses**:
- `400 Bad Request` - Invalid input
- `403 Forbidden` - Not an instructor/admin

---

### PUT /courses/{id}

Update an existing course (Instructor/Admin only).

**Endpoint**: `PUT /api/courses/{courseId}`

**Request Body**:
```json
{
  "title": "Advanced React Development",
  "description": "Master React hooks, context, and state management",
  "credits": 4,
  "maxStudents": 30,
  "startDate": "2026-04-15T00:00:00Z",
  "endDate": "2026-05-30T00:00:00Z"
}
```

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440050",
  "title": "Advanced React Development",
  "description": "Master React hooks, context, and state management",
  "credits": 4,
  "maxStudents": 30,
  "currentEnrollment": 8,
  "instructorId": "550e8400-e29b-41d4-a716-446655440001",
  "startDate": "2026-04-15T00:00:00Z",
  "endDate": "2026-05-30T00:00:00Z",
  "updatedAt": "2026-04-01T09:30:00Z"
}
```

**Authentication**: Required  
**Authorization**: Course instructor or Admin

**Error Responses**:
- `403 Forbidden` - Not the instructor
- `404 Not Found` - Course not found

---

### DELETE /courses/{id}

Delete a course (Instructor/Admin only).

**Endpoint**: `DELETE /api/courses/{courseId}`

**Response** (204 No Content):
```
(empty response body)
```

**Authentication**: Required  
**Authorization**: Course instructor or Admin

**Note**: Soft delete - enrollments preserved for audit trail

**Error Responses**:
- `403 Forbidden` - Not the instructor
- `404 Not Found` - Course not found

---

## 📚 Enrollments

### GET /enrollments

Get user's course enrollments.

**Endpoint**: `GET /api/enrollments?page=1&pageSize=10`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | integer | 1 | Page number |
| pageSize | integer | 10 | Items per page |

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440100",
      "userId": "550e8400-e29b-41d4-a716-446655440002",
      "courseId": "550e8400-e29b-41d4-a716-446655440010",
      "courseName": "ASP.NET Core Advanced Techniques",
      "enrollmentDate": "2026-04-01T08:00:00Z",
      "status": "Active",
      "grade": null
    }
  ],
  "totalItems": 3,
  "totalPages": 1,
  "currentPage": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Authentication**: Required  
**Authorization**: Own enrollments or Admin

---

### GET /enrollments/{id}

Get enrollment details.

**Endpoint**: `GET /api/enrollments/{enrollmentId}`

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440100",
  "userId": "550e8400-e29b-41d4-a716-446655440002",
  "courseId": "550e8400-e29b-41d4-a716-446655440010",
  "courseName": "ASP.NET Core Advanced Techniques",
  "studentName": "John Doe",
  "enrollmentDate": "2026-04-01T08:00:00Z",
  "status": "Active",
  "grade": null
}
```

**Authentication**: Required

---

### POST /enrollments

Enroll in a course.

**Endpoint**: `POST /api/enrollments`

**Request Body**:
```json
{
  "courseId": "550e8400-e29b-41d4-a716-446655440010"
}
```

**Response** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440101",
  "userId": "550e8400-e29b-41d4-a716-446655440002",
  "courseId": "550e8400-e29b-41d4-a716-446655440010",
  "courseName": "ASP.NET Core Advanced Techniques",
  "enrollmentDate": "2026-04-01T09:45:00Z",
  "status": "Active",
  "grade": null
}
```

**Authentication**: Required

**Error Responses**:
- `400 Bad Request` - Already enrolled
- `404 Not Found` - Course not found
- `409 Conflict` - Course full

---

### PUT /enrollments/{id}

Update enrollment (grade, status).

**Endpoint**: `PUT /api/enrollments/{enrollmentId}`

**Request Body**:
```json
{
  "status": "Completed",
  "grade": "A"
}
```

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440100",
  "userId": "550e8400-e29b-41d4-a716-446655440002",
  "courseId": "550e8400-e29b-41d4-a716-446655440010",
  "courseName": "ASP.NET Core Advanced Techniques",
  "enrollmentDate": "2026-04-01T08:00:00Z",
  "status": "Completed",
  "grade": "A"
}
```

**Authentication**: Required  
**Authorization**: Admin only

---

### DELETE /enrollments/{id}

Unenroll from a course (drop course).

**Endpoint**: `DELETE /api/enrollments/{enrollmentId}`

**Response** (204 No Content):
```
(empty response body)
```

**Authentication**: Required  
**Authorization**: Student or Admin

---

## 👥 Students

### GET /students

Get all students (Admin only).

**Endpoint**: `GET /api/students?page=1&pageSize=10`

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "enrollmentCount": 3,
      "role": "Student",
      "joinDate": "2026-03-15T00:00:00Z"
    }
  ],
  "totalItems": 15,
  "totalPages": 2,
  "currentPage": 1,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Authentication**: Required  
**Authorization**: Admin only

---

### GET /students/{id}

Get student profile details.

**Endpoint**: `GET /api/students/{studentId}`

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "enrollmentCount": 3,
  "role": "Student",
  "joinDate": "2026-03-15T00:00:00Z",
  "courses": [
    {
      "courseId": "550e8400-e29b-41d4-a716-446655440010",
      "courseName": "ASP.NET Core Advanced Techniques",
      "enrollmentDate": "2026-04-01T08:00:00Z",
      "status": "Active"
    }
  ]
}
```

**Authentication**: Required

---

### POST /students

Create new student account (Admin only).

**Endpoint**: `POST /api/students`

**Request Body**:
```json
{
  "email": "newstudent@example.com",
  "password": "SecurePass123!",
  "firstName": "Jane",
  "lastName": "Smith"
}
```

**Response** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440051",
  "email": "newstudent@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "role": "Student",
  "joinDate": "2026-04-01T10:00:00Z"
}
```

**Authentication**: Required  
**Authorization**: Admin only

---

### PUT /students/{id}

Update student information (Admin only).

**Endpoint**: `PUT /api/students/{studentId}`

**Request Body**:
```json
{
  "firstName": "Jane",
  "lastName": "Smith"
}
```

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "email": "jane.smith@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "role": "Student",
  "joinDate": "2026-03-15T00:00:00Z"
}
```

---

### DELETE /students/{id}

Delete student account (Admin only).

**Endpoint**: `DELETE /api/students/{studentId}`

**Response** (204 No Content):
```
(empty response body)
```

**Note**: Soft delete - enrollments and history preserved

---

## 🎓 Instructors

### GET /instructors

Get all instructors.

**Endpoint**: `GET /api/instructors?page=1&pageSize=10`

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "email": "instructor@example.com",
      "firstName": "Jane",
      "lastName": "Smith",
      "department": "Computer Science",
      "courseCount": 3,
      "bio": "PhD in Computer Science, 10+ years teaching experience",
      "office": "Tech Building, Room 305"
    }
  ],
  "totalItems": 8,
  "totalPages": 1,
  "currentPage": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

**Authentication**: Required

---

### GET /instructors/{id}

Get instructor profile and courses.

**Endpoint**: `GET /api/instructors/{instructorId}`

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "email": "instructor@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "department": "Computer Science",
  "bio": "PhD in Computer Science, 10+ years teaching experience",
  "office": "Tech Building, Room 305",
  "courses": [
    {
      "courseId": "550e8400-e29b-41d4-a716-446655440010",
      "title": "ASP.NET Core Advanced Techniques",
      "credits": 3,
      "studentCount": 18
    }
  ]
}
```

**Authentication**: Required

---

### POST /instructors

Create instructor profile (Admin only).

**Endpoint**: `POST /api/instructors`

**Request Body**:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "department": "Computer Science",
  "bio": "PhD in CS, 10+ years experience",
  "office": "Tech Building, Room 305"
}
```

**Response** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440060",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "firstName": "Jane",
  "lastName": "Smith",
  "department": "Computer Science",
  "bio": "PhD in CS, 10+ years experience",
  "office": "Tech Building, Room 305",
  "courseCount": 0
}
```

**Authentication**: Required  
**Authorization**: Admin only

---

### PUT /instructors/{id}

Update instructor profile.

**Endpoint**: `PUT /api/instructors/{instructorId}`

**Request Body**:
```json
{
  "department": "Computer Science",
  "bio": "Updated biography",
  "office": "New Office Location"
}
```

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440060",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "instructor@example.com",
  "department": "Computer Science",
  "bio": "Updated biography",
  "office": "New Office Location",
  "courseCount": 3
}
```

**Authentication**: Required  
**Authorization**: Own profile or Admin

---

## ❌ Error Responses

### Standard Error Format

All error responses follow this format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The Email field is required.",
  "traceId": "0HN1GDLVPL0P3:00000001"
}
```

### Common Status Codes

| Code | Meaning | Example |
|------|---------|---------|
| **200** | OK | Request successful |
| **201** | Created | Resource created successfully |
| **204** | No Content | Success with no response body |
| **400** | Bad Request | Invalid input data |
| **401** | Unauthorized | Missing/invalid token |
| **403** | Forbidden | Insufficient permissions |
| **404** | Not Found | Resource doesn't exist |
| **409** | Conflict | Resource already exists |
| **500** | Server Error | Internal server error |

### Example Error Responses

**Invalid Email Format** (400):
```json
{
  "error": "Invalid email format"
}
```

**Missing Authorization** (401):
```json
{
  "error": "Authorization header missing or invalid"
}
```

**Insufficient Permissions** (403):
```json
{
  "error": "You don't have permission to perform this action"
}
```

**Resource Not Found** (404):
```json
{
  "error": "Course not found"
}
```

---

## 🔑 Authentication Header

All authenticated endpoints require the JWT token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiSm9obiBEb2UiLCJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDIiLCJlbWFpbCI6ImpvaG5AZXhhbXBsZS5jb20iLCJyb2xlIjoiU3R1ZGVudCIsImlhdCI6MTcxMTk2NDAwMCwiZXhwIjoxNzExOTY0OTAwfQ.signature
```

**Token Properties**:
- **Type**: Bearer
- **Format**: JWT (3 parts separated by dots)
- **Expiry**: 15 minutes from login
- **Storage**: localStorage (frontend) or HTTP-only cookie (production)

### Using Tokens

**JavaScript (Vanilla)**:
```javascript
const token = localStorage.getItem('accessToken');
const response = await fetch('http://localhost:5202/api/courses', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
```

**PowerShell**:
```powershell
$token = $loginResponse.accessToken
$headers = @{'Authorization'="Bearer $token"}
Invoke-RestMethod -Uri "http://localhost:5202/api/courses" -Headers $headers
```

**cURL**:
```bash
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5202/api/courses
```

---

## 📚 Complete Request/Response Examples

### Example 1: Full Login & Course Fetch Flow

```bash
# Step 1: Login
curl -X POST http://localhost:5202/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email":"instructor@example.com",
    "password":"InstructorPass123!"
  }'

# Response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64token==",
  "accessTokenExpiry": "2026-04-01T10:30:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "email": "instructor@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "role": "Instructor"
  }
}

# Step 2: Fetch courses with token
curl -X GET "http://localhost:5202/api/courses/search?searchTerm=ASP&page=1&pageSize=5" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."

# Response:
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "title": "ASP.NET Core Advanced Techniques",
      ...
    }
  ],
  "totalItems": 1,
  "totalPages": 1,
  "currentPage": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

### Example 2: Student Enrollment Flow

```bash
# Step 1: Get available courses
curl -X GET "http://localhost:5202/api/courses?page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"

# Step 2: Enroll in a course
curl -X POST http://localhost:5202/api/enrollments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "courseId": "550e8400-e29b-41d4-a716-446655440010"
  }'

# Step 3: View enrollments
curl -X GET "http://localhost:5202/api/enrollments?page=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 🔄 Pagination Details

All list endpoints follow this pagination format:

**Query Parameters**:
```
?page=1&pageSize=10
```

**Response Structure**:
```json
{
  "items": [...],              // Array of results
  "totalItems": 42,            // Total count
  "totalPages": 5,             // Total pages
  "currentPage": 1,            // Current page (1-based)
  "hasNextPage": true,         // More pages available?
  "hasPreviousPage": false     // Previous pages available?
}
```

**Limits**:
- Minimum pageSize: 1
- Maximum pageSize: 100
- Default page: 1
- Default pageSize: 10

---

## 🎯 Common Workflows

### Student Registration and Course Enrollment

```
1. POST /auth/register (as Student)
2. POST /auth/login
3. GET /courses/search (find courses)
4. POST /enrollments (enroll in course)
5. GET /enrollments (view status)
```

### Instructor Course Management

```
1. POST /auth/login (as Instructor)
2. POST /courses (create course)
3. GET /enrollments?courseId=xxx (view students)
4. PUT /enrollments/{id} (grade student)
```

### Admin Full Control

```
1. POST /auth/login (as Admin)
2. GET /students (view all students)
3. POST /students (create new student)
4. GET /courses (view all courses)
5. DELETE /courses/{id} (remove course)
```

---

**Last Updated**: April 1, 2026  
**API Version**: 1.0.0  
**Status**: Production Ready
