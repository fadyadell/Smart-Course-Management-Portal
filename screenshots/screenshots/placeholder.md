# Complete Swagger Testing Guide - All 30 Screenshots
**Smart Course Management API - Step-by-Step Instructions with Expected Outputs**

---

## 🚀 SETUP

**Before starting:**
1. Open terminal: `cd D:\Smart-Course-Management-Portal\SmartCourseManagement.API`
2. Run API: `dotnet run`
3. Wait for: "Now listening on: http://localhost:5202"
4. Open browser: `http://localhost:5202/swagger`

**Important IDs to note:**
- Instructor ID: **1** (Dr. Jane Smith)
- Student User ID: **37** (will vary - check GET /api/students)
- Course IDs: Will be created as you go

---

# SECTION 1: SETUP & AUTHORIZATION (Screenshots 1-2)

---

## 📸 SCREENSHOT 1: Swagger UI Home Page

### **Steps:**
1. Navigate to `http://localhost:5202/swagger`
2. Page loads showing all endpoints

### **What to screenshot:**
- Full Swagger page
- Title: "Smart Course Management API"
- Green "Authorize" button visible
- All 5 controller sections collapsed

### **Expected Output:**
```
Smart Course Management API v1
[Authorize button 🔓]

Auth
  POST /api/auth/register
  POST /api/auth/login
  POST /api/auth/refresh

Courses
  GET /api/courses
  GET /api/courses/search
  GET /api/courses/{id}
  POST /api/courses
  PUT /api/courses/{id}
  DELETE /api/courses/{id}

Enrollments
  [endpoints listed]

Instructors
  [endpoints listed]

Students
  [endpoints listed]
```

**Save as:** `01_swagger_home.png`

---

## 📸 SCREENSHOT 2: Authorization Modal

### **Steps:**
1. Click the green **"Authorize"** button (🔓)
2. Modal popup appears

### **What to screenshot:**
- Authorization popup window
- "Available authorizations" title
- Bearer input field (empty)
- Description about JWT
- Authorize and Close buttons

### **Expected Output:**
```
Available authorizations

Bearer (apiKey)
Value: [empty text box]

Description: JWT Authorization header using the Bearer scheme.
Example: "Authorization: Bearer {token}"

[Authorize] [Close]
```

### **After screenshot:**
- Click "Close"

**Save as:** `02_authorization_modal.png`

---

# SECTION 2: AUTHENTICATION (Screenshots 3-8)

---

## 📸 SCREENSHOT 3: Register Admin

### **Steps:**
1. Expand **POST /api/auth/register**
2. Click **"Try it out"**
3. Clear Request body
4. Paste JSON below
5. Click **"Execute"**

### **Request Body:**
```json
{
  "name": "Admin User",
  "email": "admin@example.com",
  "password": "Admin123",
  "role": "Admin"
}
```

### **What to screenshot:**
- Request body with JSON
- Response section

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "SGVsbG8gV29ybGQgUzIgVG9rZW4h...",
  "accessTokenExpiry": "2026-04-02T02:00:00Z",
  "user": {
    "id": 1,
    "name": "Admin User",
    "email": "admin@example.com",
    "role": "Admin"
  }
}
```

### **IMPORTANT - Save the token:**
- Copy the **entire `accessToken`** value
- Save in Notepad as: **ADMIN TOKEN**

**Save as:** `03_register_admin.png`

---

## 📸 SCREENSHOT 4: Register Instructor

### **Steps:**
1. Same endpoint: **POST /api/auth/register**
2. Already in "Try it out" mode
3. Clear Request body
4. Paste JSON below
5. Click **"Execute"**

### **Request Body:**
```json
{
  "name": "Dr. Sarah Williams",
  "email": "sarah.williams@example.com",
  "password": "Instructor123",
  "role": "Instructor"
}
```

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "accessTokenExpiry": "2026-04-02T02:00:00Z",
  "user": {
    "id": 38,
    "name": "Dr. Sarah Williams",
    "email": "sarah.williams@example.com",
    "role": "Instructor"
  }
}
```

### **IMPORTANT - Save the token:**
- Copy the **`accessToken`**
- Save in Notepad as: **INSTRUCTOR TOKEN**

**Save as:** `04_register_instructor.png`

---

## 📸 SCREENSHOT 5: Register Student

### **Steps:**
1. Same endpoint: **POST /api/auth/register**
2. Clear Request body
3. Paste JSON below
4. Click **"Execute"**

### **Request Body:**
```json
{
  "name": "Alice Johnson",
  "email": "alice@example.com",
  "password": "Student123",
  "role": "Student"
}
```

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "accessTokenExpiry": "2026-04-02T02:00:00Z",
  "user": {
    "id": 37,
    "name": "Alice Johnson",
    "email": "alice@example.com",
    "role": "Student"
  }
}
```

### **IMPORTANT - Save the token:**
- Copy the **`accessToken`**
- Save in Notepad as: **STUDENT TOKEN**
- **Note the user.id** (e.g., 37) - this is your **STUDENT USER ID**

**Save as:** `05_register_student.png`

---

## 📸 SCREENSHOT 6: Login as Admin

### **Steps:**
1. Expand **POST /api/auth/login**
2. Click **"Try it out"**
3. Paste JSON below
4. Click **"Execute"**

### **Request Body:**
```json
{
  "email": "admin@example.com",
  "password": "Admin123"
}
```

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "SGVsbG8gV29ybGQgUzIgVG9rZW4h...",
  "accessTokenExpiry": "2026-04-02T02:00:00Z",
  "user": {
    "id": 1,
    "name": "Admin User",
    "email": "admin@example.com",
    "role": "Admin"
  }
}
```

### **Action:**
- Copy the **`accessToken`** (you'll use it next)

**Save as:** `06_login_admin.png`

---

## 📸 SCREENSHOT 7: Authorize with JWT Token

### **Steps:**
1. Click **"Authorize"** button at TOP of page
2. In the popup, find "Value" text box
3. Type: `Bearer ` (word Bearer + space)
4. Paste your ADMIN TOKEN after the space
5. Click **"Authorize"** button in popup
6. **TAKE SCREENSHOT NOW** (before closing)
7. Click "Close"

### **What to type in Value field:**
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
(Your actual admin token)

### **What to screenshot:**
- Authorization popup
- Token entered in Value field (can blur part of it)
- "Authorized" confirmation

### **Expected Output:**
```
Available authorizations

Bearer (apiKey)
Value: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
       [Logout]

✓ Authorized

[Close]
```

### **After screenshot:**
- Lock icon 🔓 becomes 🔒 (closed)

**Save as:** `07_authorize_jwt.png`

---

## 📸 SCREENSHOT 8: Refresh Token

### **Steps:**
1. Expand **POST /api/auth/refresh**
2. Click **"Try it out"**
3. Look at Screenshot 6 response - find the **`refreshToken`**
4. Paste JSON below (replace with YOUR refresh token)
5. Click **"Execute"**

### **Request Body:**
```json
{
  "refreshToken": "YOUR_REFRESH_TOKEN_FROM_SCREENSHOT_6"
}
```

Example:
```json
{
  "refreshToken": "SGVsbG8gV29ybGQgUzIgVG9rZW4h..."
}
```

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accessTokenExpiry": "2026-04-02T03:00:00Z"
}
```

**Save as:** `08_refresh_token.png`

---

# SECTION 3: COURSES MANAGEMENT (Screenshots 9-14)

**NOTE:** Make sure you're authorized as Admin or Instructor for this section!

---

## 📸 SCREENSHOT 9: Create Course 1

### **Steps:**
1. Scroll to **Courses** section
2. Expand **POST /api/courses**
3. Click **"Try it out"**
4. Clear Request body
5. Paste JSON below
6. Click **"Execute"**

### **Request Body:**
```json
{
  "title": "Introduction to Computer Science",
  "description": "Programming basics",
  "credits": 3,
  "instructorId": 1
}
```

### **Expected Output:**
```
Code: 201 Created

Response body:
{
  "id": 1,
  "title": "Introduction to Computer Science",
  "description": "Programming basics",
  "credits": 3,
  "instructorId": 1,
  "instructorName": "Dr. Jane Smith",
  "createdAt": "2026-04-02T00:25:00Z",
  "createdBy": "Admin User",
  "updatedAt": "2026-04-02T00:25:00Z",
  "updatedBy": "Admin User",
  "isDeleted": false
}
```

### **IMPORTANT:**
- Note the **`id`** value (probably 1) - this is COURSE ID 1

**Save as:** `09_create_course_1.png`

---

## 📸 SCREENSHOT 10: Create Course 2

### **Steps:**
1. Same endpoint: **POST /api/courses**
2. Clear Request body
3. Paste JSON below
4. Click **"Execute"**

### **Request Body:**
```json
{
  "title": "Web Development Bootcamp",
  "description": "Full-stack web development with HTML CSS JavaScript and .NET",
  "credits": 4,
  "instructorId": 1
}
```

### **Expected Output:**
```
Code: 201 Created

Response body:
{
  "id": 2,
  "title": "Web Development Bootcamp",
  "description": "Full-stack web development with HTML CSS JavaScript and .NET",
  "credits": 4,
  "instructorId": 1,
  "instructorName": "Dr. Jane Smith",
  "createdAt": "2026-04-02T00:28:00Z",
  "createdBy": "Admin User",
  "updatedAt": "2026-04-02T00:28:00Z",
  "updatedBy": "Admin User",
  "isDeleted": false
}
```

### **IMPORTANT:**
- Note the **`id`** value (probably 2) - this is COURSE ID 2

**Save as:** `10_create_course_2.png`

---

## 📸 SCREENSHOT 11: Get All Courses

### **Steps:**
1. Expand **GET /api/courses** (without {id})
2. Click **"Try it out"**
3. Click **"Execute"**

### **No parameters needed**

### **Expected Output:**
```
Code: 200 OK

Response body:
[
  {
    "id": 1,
    "title": "Introduction to Computer Science",
    "description": "Programming basics",
    "credits": 3,
    "instructorId": 1,
    "instructorName": "Dr. Jane Smith",
    ...
  },
  {
    "id": 2,
    "title": "Web Development Bootcamp",
    "description": "Full-stack web development...",
    "credits": 4,
    "instructorId": 1,
    "instructorName": "Dr. Jane Smith",
    ...
  }
  // ... possibly more courses from previous testing
]
```

**Save as:** `11_get_all_courses.png`

---

## 📸 SCREENSHOT 12: Get Course by ID

### **Steps:**
1. Expand **GET /api/courses/{id}**
2. Click **"Try it out"**
3. In **id** parameter field, enter: `1`
4. Click **"Execute"**

### **Parameters:**
- **id**: `1`

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "id": 1,
  "title": "Introduction to Computer Science",
  "description": "Programming basics",
  "credits": 3,
  "instructorId": 1,
  "instructorName": "Dr. Jane Smith",
  "createdAt": "2026-04-02T00:25:00Z",
  "createdBy": "Admin User",
  "updatedAt": "2026-04-02T00:25:00Z",
  "updatedBy": "Admin User",
  "isDeleted": false
}
```

**Save as:** `12_get_course_by_id.png`

---

## 📸 SCREENSHOT 13: Search with Pagination

### **Steps:**
1. Expand **GET /api/courses/search**
2. Click **"Try it out"**
3. Fill in parameters:
   - **page**: `1`
   - **pageSize**: `10`
   - **searchTerm**: `web`
4. Click **"Execute"**

### **Parameters:**
- **page**: `1`
- **pageSize**: `10`
- **searchTerm**: `web`

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "totalCount": 1,
  "totalPages": 1,
  "currentPage": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 2,
      "title": "Web Development Bootcamp",
      "description": "Full-stack web development...",
      "credits": 4,
      "instructorId": 1,
      "instructorName": "Dr. Jane Smith",
      ...
    }
  ]
}
```

**Save as:** `13_search_pagination.png`

---

## 📸 SCREENSHOT 14: Update Course

### **Steps:**
1. Expand **PUT /api/courses/{id}**
2. Click **"Try it out"**
3. In **id** parameter field, enter: `1`
4. Clear Request body
5. Paste JSON below
6. Click **"Execute"**

### **Parameters:**
- **id**: `1`

### **Request Body:**
```json
{
  "title": "Introduction to Computer Science - Updated",
  "description": "Updated course with Python and Java programming fundamentals",
  "credits": 4
}
```

### **Expected Output:**
```
Code: 204 No Content

(No response body - this is normal for 204)
```

**Save as:** `14_update_course.png`

---

# SECTION 4: STUDENTS MANAGEMENT (Screenshots 15-16)

**NOTE:** Requires Admin or Instructor role!

---

## 📸 SCREENSHOT 15: Get All Students

### **Steps:**
1. Scroll to **Students** section
2. Expand **GET /api/students**
3. Click **"Try it out"**
4. Click **"Execute"**

### **No parameters needed**

### **Expected Output:**
```
Code: 200 OK

Response body:
[
  {
    "userId": 37,
    "name": "Alice Johnson",
    "email": "alice@example.com",
    "enrolledCoursesCount": 0
  }
  // ... possibly more students
]
```

### **IMPORTANT:**
- Note the **userId** of Alice Johnson (e.g., 37) - this is your STUDENT USER ID
- You'll need this for later screenshots!

**Save as:** `15_get_all_students.png`

---

## 📸 SCREENSHOT 16: Get Student by ID

### **Steps:**
1. Expand **GET /api/students/{id}**
2. Click **"Try it out"**
3. In **id** parameter field, enter your STUDENT USER ID (e.g., `37`)
4. Click **"Execute"**

### **Parameters:**
- **id**: `37` (use YOUR student user ID from Screenshot 15)

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "userId": 37,
  "name": "Alice Johnson",
  "email": "alice@example.com",
  "enrollments": []
}
```

**Save as:** `16_get_student_by_id.png`

---

# SECTION 5: INSTRUCTORS MANAGEMENT (Screenshots 17-19)

---

## 📸 SCREENSHOT 17: Get All Instructors

### **Steps:**
1. Scroll to **Instructors** section
2. Expand **GET /api/instructors**
3. Click **"Try it out"**
4. Click **"Execute"**

### **No parameters needed**

### **Expected Output:**
```
Code: 200 OK

Response body:
[
  {
    "id": 1,
    "userId": 1,
    "userName": "Dr. Jane Smith",
    "biography": "Professor of Computer Science with 15 years experience.",
    "officeLocation": "Science Building, Lab 404",
    "createdAt": "0001-01-01T00:00:00",
    ...
  },
  {
    "id": 13,
    "userId": 38,
    "userName": "Dr. Sarah Williams",
    "biography": "",
    "officeLocation": "",
    ...
  }
  // ... more instructors
]
```

**Save as:** `17_get_all_instructors.png`

---

## 📸 SCREENSHOT 18: Get Instructor by ID

### **Steps:**
1. Expand **GET /api/instructors/{id}**
2. Click **"Try it out"**
3. In **id** parameter field, enter: `1`
4. Click **"Execute"**

### **Parameters:**
- **id**: `1`

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "id": 1,
  "userId": 1,
  "userName": "Dr. Jane Smith",
  "biography": "Professor of Computer Science with 15 years experience.",
  "officeLocation": "Science Building, Lab 404",
  "createdAt": "0001-01-01T00:00:00",
  "createdBy": null,
  "updatedAt": "0001-01-01T00:00:00",
  "updatedBy": null
}
```

**Save as:** `18_get_instructor_by_id.png`

---

## 📸 SCREENSHOT 19: Update Instructor Profile

**IMPORTANT:** This requires INSTRUCTOR role only!

### **Steps:**

**A. Login as Instructor:**
1. Go to **POST /api/auth/login**
2. Click "Try it out"
3. Enter:
```json
{
  "email": "sarah.williams@example.com",
  "password": "Instructor123"
}
```
4. Click "Execute"
5. Copy the **`accessToken`**

**B. Re-authorize:**
1. Click **"Authorize"** button at top
2. Clear old token
3. Enter: `Bearer ` + paste instructor token
4. Click "Authorize", then "Close"

**C. Update Profile:**
1. Expand **PUT /api/instructors/profile**
2. Click **"Try it out"**
3. Clear Request body
4. Paste JSON below
5. Click **"Execute"**

### **Request Body:**
```json
{
  "biography": "PhD in Computer Science with 15 years of teaching experience",
  "officeLocation": "Engineering Building Room 305"
}
```

### **Expected Output:**
```
Code: 204 No Content

(No response body - this is normal)
```

**Save as:** `19_update_instructor_profile.png`

---

# SECTION 6: ENROLLMENTS (Screenshots 20-26)

---

## 📸 SCREENSHOT 20: Re-authorize as Student

### **Steps:**

**A. Login as Student:**
1. Go to **POST /api/auth/login**
2. Click "Try it out"
3. Enter:
```json
{
  "email": "alice@example.com",
  "password": "Student123"
}
```
4. Click "Execute"
5. Copy the **`accessToken`**
6. **Also note the `user.id`** in response (this is your STUDENT USER ID)

**B. Re-authorize:**
1. Click **"Authorize"** button at top
2. Clear old token
3. Enter: `Bearer ` + paste student token
4. **TAKE SCREENSHOT NOW** (of the authorization popup)
5. Click "Authorize"
6. Click "Close"

### **What to screenshot:**
- Authorization popup with student token entered

### **Expected in popup:**
```
Available authorizations

Bearer (apiKey)
Value: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
       [Logout]

[Authorize] [Close]
```

**Save as:** `20_reauthorize_student.png`

---

## 📸 SCREENSHOT 21: Enroll in Course 1

### **Steps:**
1. Scroll to **Enrollments** section
2. Expand **POST /api/enrollments**
3. Click **"Try it out"**
4. Clear Request body
5. Paste JSON below (USE YOUR STUDENT USER ID!)
6. Click **"Execute"**

### **Request Body:**
```json
{
  "studentId": 37,
  "courseId": 1
}
```

**CRITICAL:** Replace `37` with YOUR actual student user ID from Screenshot 20!

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "enrollmentId": 1,
  "studentId": 37,
  "studentName": "Alice Johnson",
  "courseId": 1,
  "courseName": "Introduction to Computer Science - Updated",
  "enrolledAt": "2026-04-02T00:52:00Z"
}
```

### **IMPORTANT:**
- Note the **`enrollmentId`** (e.g., 1) - you'll need this for Screenshot 26

**Save as:** `21_enroll_course_1.png`

---

## 📸 SCREENSHOT 22: Enroll in Course 2

### **Steps:**
1. Same endpoint: **POST /api/enrollments**
2. Clear Request body
3. Paste JSON below (USE YOUR STUDENT USER ID!)
4. Click **"Execute"**

### **Request Body:**
```json
{
  "studentId": 37,
  "courseId": 2
}
```

**CRITICAL:** Replace `37` with YOUR actual student user ID!

### **Expected Output:**
```
Code: 200 OK

Response body:
{
  "enrollmentId": 2,
  "studentId": 37,
  "studentName": "Alice Johnson",
  "courseId": 2,
  "courseName": "Web Development Bootcamp",
  "enrolledAt": "2026-04-02T00:54:00Z"
}
```

**Save as:** `22_enroll_course_2.png`

---

## 📸 SCREENSHOT 23: Get My Enrollments

### **Steps:**
1. Expand **GET /api/enrollments/my-enrollments**
2. Click **"Try it out"**
3. Click **"Execute"**

### **No parameters needed** (gets current student's enrollments from JWT)

### **Expected Output:**
```
Code: 200 OK

Response body:
[
  {
    "enrollmentId": 1,
    "studentId": 37,
    "studentName": "Alice Johnson",
    "courseId": 1,
    "courseName": "Introduction to Computer Science - Updated",
    "enrolledAt": "2026-04-02T00:52:00Z"
  },
  {
    "enrollmentId": 2,
    "studentId": 37,
    "studentName": "Alice Johnson",
    "courseId": 2,
    "courseName": "Web Development Bootcamp",
    "enrolledAt": "2026-04-02T00:54:00Z"
  }
]
```

**Save as:** `23_get_my_enrollments.png`

---

## 📸 SCREENSHOT 24: Re-authorize as Admin

### **Steps:**

**A. Login as Admin:**
1. Go to **POST /api/auth/login**
2. Click "Try it out"
3. Enter:
```json
{
  "email": "admin@example.com",
  "password": "Admin123"
}
```
4. Click "Execute"
5. Copy the **`accessToken`**

**B. Re-authorize:**
1. Click **"Authorize"** button at top
2. Clear old token
3. Enter: `Bearer ` + paste admin token
4. **TAKE SCREENSHOT NOW** (of the authorization popup)
5. Click "Authorize"
6. Click "Close"

### **What to screenshot:**
- Authorization popup with admin token entered

**Save as:** `24_reauthorize_admin.png`

---

## 📸 SCREENSHOT 25: View Student's Enrollments (Admin)

### **Steps:**
1. Expand **GET /api/enrollments/student/{studentId}**
2. Click **"Try it out"**
3. In **studentId** parameter field, enter YOUR STUDENT USER ID (e.g., `37`)
4. Click **"Execute"**

### **Parameters:**
- **studentId**: `37` (use YOUR actual student user ID)

### **Expected Output:**
```
Code: 200 OK

Response body:
[
  {
    "enrollmentId": 1,
    "studentId": 37,
    "studentName": "Alice Johnson",
    "courseId": 1,
    "courseName": "Introduction to Computer Science - Updated",
    "enrolledAt": "2026-04-02T00:52:00Z"
  },
  {
    "enrollmentId": 2,
    "studentId": 37,
    "studentName": "Alice Johnson",
    "courseId": 2,
    "courseName": "Web Development Bootcamp",
    "enrolledAt": "2026-04-02T00:54:00Z"
  }
]
```

**Save as:** `25_view_student_enrollments_admin.png`

---

## 📸 SCREENSHOT 26: Unenroll from Course

### **Steps:**
1. Expand **DELETE /api/enrollments/{id}**
2. Click **"Try it out"**
3. In **id** parameter field, enter first enrollment ID (e.g., `1`)
4. Click **"Execute"**

### **Parameters:**
- **id**: `1` (use the enrollmentId from Screenshot 21)

### **Expected Output:**
```
Code: 204 No Content

(No response body - this is normal)
```

**Save as:** `26_unenroll.png`

---

# SECTION 7: ERROR HANDLING (Screenshots 27-30)

---

## 📸 SCREENSHOT 27: 401 Unauthorized Error

### **Steps:**

**A. Logout (clear authorization):**
1. Click **"Authorize"** button at top
2. Click **"Logout"** button in popup
3. Click "Close"
4. Lock icon should be open 🔓

**B. Try protected endpoint:**
1. Go to **GET /api/courses**
2. Click **"Try it out"**
3. Click **"Execute"**

### **Expected Output:**
```
Code: 401 Unauthorized

Response headers:
www-authenticate: Bearer

(Empty or minimal response body)
```

**Save as:** `27_error_401_unauthorized.png`

---

## 📸 SCREENSHOT 28: 403 Forbidden Error

### **Steps:**

**A. Login as Student:**
1. Go to **POST /api/auth/login**
2. Login with:
```json
{
  "email": "alice@example.com",
  "password": "Student123"
}
```
3. Copy accessToken

**B. Re-authorize as Student:**
1. Click "Authorize"
2. Enter: `Bearer ` + student token
3. Click "Authorize", then "Close"

**C. Try Admin-only endpoint:**
1. Go to **DELETE /api/courses/{id}**
2. Click **"Try it out"**
3. Enter id: `1`
4. Click **"Execute"**

### **Parameters:**
- **id**: `1`

### **Expected Output:**
```
Code: 403 Forbidden

Response body:
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.4",
  "title": "Forbidden",
  "status": 403
}
```

**Save as:** `28_error_403_forbidden.png`

---

## 📸 SCREENSHOT 29: 404 Not Found Error

### **Steps:**
1. Go to **GET /api/courses/{id}**
2. Click **"Try it out"**
3. In **id** parameter field, enter: `999`
4. Click **"Execute"**

### **Parameters:**
- **id**: `999`

### **Expected Output:**
```
Code: 404 Not Found

Response body:
{
  "message": "Course 999 not found."
}
```

**Save as:** `29_error_404_notfound.png`

---

## 📸 SCREENSHOT 30: 400 Bad Request - Validation Error

### **Steps:**
1. Go to **POST /api/auth/register**
2. Click **"Try it out"**
3. Clear Request body
4. Paste INVALID JSON below
5. Click **"Execute"**

### **Request Body (INTENTIONALLY INVALID):**
```json
{
  "name": "",
  "email": "not-an-email",
  "password": "123",
  "role": "InvalidRole"
}
```

### **Expected Output:**
```
Code: 400 Bad Request

Response body:
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "Name is required",
      "Name must be at least 2 characters"
    ],
    "Email": [
      "Invalid email format"
    ],
    "Password": [
      "Password must be at least 6 characters"
    ],
    "Role": [
      "Role must be Admin, Instructor, or Student"
    ]
  }
}
```

**Save as:** `30_error_400_validation.png`

---

# 🎉 CONGRATULATIONS! ALL 30 SCREENSHOTS COMPLETE!

---

## 📋 FINAL CHECKLIST

### ✅ Section 1: Setup (2 screenshots)
- [ ] 01_swagger_home.png
- [ ] 02_authorization_modal.png

### ✅ Section 2: Authentication (6 screenshots)
- [ ] 03_register_admin.png
- [ ] 04_register_instructor.png
- [ ] 05_register_student.png
- [ ] 06_login_admin.png
- [ ] 07_authorize_jwt.png
- [ ] 08_refresh_token.png

### ✅ Section 3: Courses (6 screenshots)
- [ ] 09_create_course_1.png
- [ ] 10_create_course_2.png
- [ ] 11_get_all_courses.png
- [ ] 12_get_course_by_id.png
- [ ] 13_search_pagination.png
- [ ] 14_update_course.png

### ✅ Section 4: Students (2 screenshots)
- [ ] 15_get_all_students.png
- [ ] 16_get_student_by_id.png

### ✅ Section 5: Instructors (3 screenshots)
- [ ] 17_get_all_instructors.png
- [ ] 18_get_instructor_by_id.png
- [ ] 19_update_instructor_profile.png

### ✅ Section 6: Enrollments (7 screenshots)
- [ ] 20_reauthorize_student.png
- [ ] 21_enroll_course_1.png
- [ ] 22_enroll_course_2.png
- [ ] 23_get_my_enrollments.png
- [ ] 24_reauthorize_admin.png
- [ ] 25_view_student_enrollments_admin.png
- [ ] 26_unenroll.png

### ✅ Section 7: Error Handling (4 screenshots)
- [ ] 27_error_401_unauthorized.png
- [ ] 28_error_403_forbidden.png
- [ ] 29_error_404_notfound.png
- [ ] 30_error_400_validation.png

---

## 🎯 KEY FEATURES DEMONSTRATED

✅ **JWT Authentication**
- User registration (Admin, Instructor, Student roles)
- Login and token generation
- Authorization with Bearer tokens
- Token refresh mechanism

✅ **Role-Based Access Control**
- Admin-only operations (delete courses)
- Instructor operations (update profile, create courses)
- Student operations (enroll in courses, view own enrollments)
- Multi-role operations (Admin/Instructor can view students)

✅ **CRUD Operations**
- Create: Courses, Enrollments
- Read: All resources (courses, students, instructors, enrollments)
- Update: Courses, Instructor profiles
- Delete: Courses, Enrollments

✅ **Advanced Features**
- Pagination and search (courses)
- Filtering by role and user
- Relationship management (enrollments link students and courses)

✅ **Error Handling**
- 401 Unauthorized (no token)
- 403 Forbidden (wrong role)
- 404 Not Found (resource doesn't exist)
- 400 Bad Request (validation errors)

---

## 💡 IMPORTANT NOTES

1. **Token Expiration**: Tokens expire after ~1 hour. If you get 401 errors, re-login and get a fresh token.

2. **User IDs**: The student user ID (e.g., 37) will vary based on your database. Always check GET /api/students to get the correct ID.

3. **Instructor ID**: Use instructor ID 1 (Dr. Jane Smith) for creating courses.

4. **Screenshot Naming**: Use the naming convention shown (01_, 02_, etc.) for easy organization.

5. **Blur Sensitive Data**: When submitting publicly, consider blurring actual JWT tokens.

---

## 📝 SUBMISSION TIPS

1. **Organize screenshots** in folders by section
2. **Create a summary document** explaining what each screenshot demonstrates
3. **Highlight key features** with annotations if needed
4. **Include this guide** as documentation reference
5. **Test systematically** - follow the order to ensure dependencies are met

---

**Good luck with your submission! 🎓**
