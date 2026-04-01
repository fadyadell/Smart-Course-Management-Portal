# 🧪 Complete API Testing Guide - Smart Course Management System

**Status**: ✅ API Running on `http://localhost:5202` & `https://localhost:7199`

---

## 📌 Quick Reference - Base URLs

| Protocol | URL | Environment |
|----------|-----|-------------|
| **HTTP** | `http://localhost:5202` | Local Development |
| **HTTPS** | `https://localhost:7199` | Local Development (Secure) |
| **Swagger UI** | `https://localhost:7199/swagger` | Interactive Testing |

---

## 🚀 TEST 1: REGISTER A STUDENT USER

### Step 1: Open Swagger UI
```
https://localhost:7199/swagger
```

### Step 2: Find the Register Endpoint
1. Scroll to "**Auth**" section
2. Click on "**POST /api/auth/register**"
3. Click "**Try it out**"

### Step 3: Send Registration Request

**Request Body:**
```json
{
  "name": "Alice Johnson",
  "email": "alice@example.com",
  "password": "StudentPass123!",
  "role": "Student"
}
```

### Step 4: Verify Response

**Expected Response:**
```
Status: 201 Created
Headers: Content-Type: application/json
Body:
{
  "message": "User registered successfully",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

✅ **Success Indicators:**
- ✅ Status code is **201 Created**
- ✅ Token starts with `eyJ` (JWT format)
- ✅ No errors in response

---

## 🔑 TEST 2: LOGIN AND GET JWT TOKEN

### Step 1: Find Login Endpoint
1. Click on "**POST /api/auth/login**"
2. Click "**Try it out**"

### Step 2: Send Login Request

**Request Body:**
```json
{
  "email": "alice@example.com",
  "password": "StudentPass123!"
}
```

### Step 3: Verify Response

**Expected Response:**
```
Status: 200 OK
Body:
{
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": "Student"
}
```

### Step 4: Copy the JWT Token
📋 **Copy this token** - you'll need it for authorized requests:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 🔐 TEST 3: AUTHORIZE WITH JWT TOKEN IN SWAGGER

### Step 1: Click the Lock Icon (Authorize Button)
In Swagger UI, look for the green lock icon in the top-right corner.

### Step 2: Select Bearer Token

1. A modal will appear with "Available Authorizations"
2. Look for **"Bearer"** or **"HTTP Bearer"**
3. Paste your JWT token in the text field (without "Bearer" prefix)

### Step 3: Click Authorize
- Button text changes to "**Logout**" ✅

---

## ✅ TEST 4: ACCESS PROTECTED ENDPOINT (Get All Courses)

### Step 1: Find GET Courses Endpoint
1. Click on "**GET /api/courses**"
2. Click "**Try it out**"
3. Leave parameters empty (or add filters)

### Step 2: Execute Request

**Expected Response (Status: 200 OK):**
```json
[
  {
    "id": 1,
    "title": "C# Fundamentals",
    "description": "Learn C# from scratch",
    "credits": 3,
    "instructorId": 1,
    "instructorName": "Dr. Smith"
  },
  {
    "id": 2,
    "title": "ASP.NET Core Basics",
    "description": "Build web APIs",
    "credits": 4,
    "instructorId": 1,
    "instructorName": "Dr. Smith"
  }
]
```

✅ **Success Indicators:**
- ✅ Status **200 OK**
- ✅ Returns array of courses with DTOs (not full entities)
- ✅ Each course has `instructorName` (LINQ optimization working)

---

## ❌ TEST 5: VERIFY 401 UNAUTHORIZED (No Token)

### Step 1: Logout First
1. Click the Lock icon (Authorize button)
2. Click "**Logout**" to remove the token
3. Verify it says "Authorize" again (not "Logout")

### Step 2: Try to Access Protected Endpoint
1. Click "**GET /api/courses**"
2. Click "**Try it out**"
3. Click "**Execute**"

### Step 3: Verify 401 Response

**Expected Response:**
```
Status: 401 Unauthorized
Body:
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Unauthorized",
  "status": 401
}
```

✅ **Success**: Protected endpoint blocked without token!

---

## 🚫 TEST 6: TEST ROLE-BASED AUTHORIZATION (403 Forbidden)

### Create Test Data First:

**Register Second User as INSTRUCTOR:**
```json
{
  "name": "Dr. Mark Smith",
  "email": "instructor@example.com",
  "password": "InstructorPass123!",
  "role": "Instructor"
}
```

**Copy the returned token** for the Instructor.

---

### Test 6A: Student CANNOT Delete Course (403)

1. **Logout** and authorize with **Student token** (Alice)
2. Find "**DELETE /api/courses/{id}**"
3. Set `id = 1`
4. Click "**Try it out**" → "**Execute**"

**Expected Response:**
```
Status: 403 Forbidden
Body:
{
  "type": "...",
  "title": "Forbidden",
  "status": 403,
  "detail": "Access denied. Only admins can delete courses."
}
```

✅ **Success**: Student blocked from admin action!

---

### Test 6B: Instructor CAN Create Course (201)

1. **Logout** and authorize with **Instructor token** (Dr. Smith)
2. Find "**POST /api/courses**"
3. Click "**Try it out**"

**Request Body:**
```json
{
  "title": "Advanced C#",
  "description": "Master advanced C# patterns",
  "credits": 4,
  "instructorId": 2
}
```

4. Click "**Execute**"

**Expected Response:**
```
Status: 201 Created
Body:
{
  "id": 3,
  "title": "Advanced C#",
  "description": "Master advanced C# patterns",
  "credits": 4,
  "instructorId": 2,
  "instructorName": "Dr. Mark Smith"
}
```

✅ **Success**: Instructor created course!

---

## 📝 TEST 7: TEST VALIDATION (400 Bad Request)

### Test Invalid Email Format

1. Find "**POST /api/auth/register**"
2. Click "**Try it out**"

**Request Body (Invalid Email):**
```json
{
  "name": "Bob Wilson",
  "email": "not-an-email",
  "password": "Pass123!",
  "role": "Student"
}
```

3. Click "**Execute**"

**Expected Response:**
```
Status: 400 Bad Request
Body:
{
  "type": "https://...",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

✅ **Success**: Validation working!

---

### Test Missing Required Field

**Request Body (Missing Name):**
```json
{
  "name": "",
  "email": "test@example.com",
  "password": "Pass123!",
  "role": "Student"
}
```

**Expected Response:**
```
Status: 400 Bad Request
Body:
{
  "errors": {
    "Name": ["The Name field is required."]
  }
}
```

✅ **Success**: Required field validation working!

---

## 📊 TEST 8: ENROLLMENT FLOW (Student Enrolls in Course)

### Step 1: Login as Student
- Use Alice's credentials from TEST 1
- Copy the JWT token

### Step 2: Authorize in Swagger
- Click lock icon
- Paste Student token

### Step 3: Enroll in Course
1. Find "**POST /api/enrollments**"
2. Click "**Try it out**"

**Request Body:**
```json
{
  "studentId": 1,
  "courseId": 1,
  "enrolledDate": "2026-04-01"
}
```

3. Click "**Execute**"

**Expected Response:**
```
Status: 201 Created
Body:
{
  "id": 1,
  "studentId": 1,
  "courseId": 1,
  "enrolledDate": "2026-04-01T00:00:00",
  "studentName": "Alice Johnson",
  "courseName": "C# Fundamentals"
}
```

✅ **Success**: Student enrolled!

### Step 4: View Student's Enrollments
1. Find "**GET /api/enrollments/student/{studentId}**"
2. Set `studentId = 1`
3. Click "**Execute**"

**Expected Response:**
```
Status: 200 OK
Body:
[
  {
    "id": 1,
    "studentId": 1,
    "courseId": 1,
    "enrolledDate": "2026-04-01T00:00:00",
    "studentName": "Alice Johnson",
    "courseName": "C# Fundamentals"
  }
]
```

✅ **Success**: Enrollments displayed!

---

## 🎯 TEST 9: LINQ OPTIMIZATION CHECK

### Verify Select() Projections
Every course response shows `instructorName` without separate query:

```json
{
  "id": 1,
  "title": "C# Fundamentals",
  "instructorName": "Dr. Smith"  ← Note: this includes related data
}
```

This works because **Select() projection** eagerly loads the Instructor name in a single query.

✅ **Performance**: No N+1 queries!

---

## 🐛 TEST 10: ERROR SCENARIOS

### Test Duplicate Email Registration
1. Register user 1: `alice@example.com`
2. Try to register user 2 with same email

**Expected Response:**
```
Status: 409 Conflict (or 400 Bad Request)
Body:
{
  "message": "Email already exists"
}
```

### Test Wrong Password
1. Find "**POST /api/auth/login**"
2. Use correct email but wrong password

**Expected Response:**
```
Status: 401 Unauthorized
Body:
{
  "message": "Invalid email or password"
}
```

### Test Non-Existent User
1. Try to get student by ID that doesn't exist
2. Find "**GET /api/students/{id}**"
3. Set `id = 9999`

**Expected Response:**
```
Status: 404 Not Found
Body:
{
  "message": "Student not found"
}
```

---

## 📋 COMPLETE TEST CHECKLIST

| Test | Endpoint | Expected Status | Notes |
|------|----------|-----------------|-------|
| ✅ Register Student | POST /api/auth/register | 201 | Returns JWT token |
| ✅ Login | POST /api/auth/login | 200 | Returns token + role |
| ✅ Get Courses (Authorized) | GET /api/courses | 200 | Returns DTOs with instructor name |
| ✅ Get Courses (No Token) | GET /api/courses | 401 | Unauthorized |
| ✅ Delete Course as Student | DELETE /api/courses/{id} | 403 | Forbidden (wrong role) |
| ✅ Create Course as Instructor | POST /api/courses | 201 | Allowed for Instructor |
| ✅ Register Invalid Email | POST /api/auth/register | 400 | Validation error |
| ✅ Student Enrolls | POST /api/enrollments | 201 | Enrollment created |
| ✅ View Enrollments | GET /api/enrollments/student/{id} | 200 | Returns student enrollments |
| ✅ Duplicate Email | POST /api/auth/register | Conflict | Email already exists |

---

## 🔧 TROUBLESHOOTING

### Issue: API won't start (Port already in use)
```bash
# Find process using port 5202
netstat -ano | findstr :5202

# Kill the process (replace PID with actual process ID)
taskkill /PID 1234 /F
```

### Issue: Certificate error (HTTPS)
```bash
# The self-signed certificate is normal for development
# In browser, click "Advanced" → "Proceed anyway"
# Or use HTTP endpoint instead: http://localhost:5202
```

### Issue: Invalid JWT token in Swagger
```
- Make sure you copied the token correctly (without "Bearer" prefix)
- Token might have expired after 1 hour, get a new one by logging in again
- Ensure you're authorized BEFORE making the request
```

### Issue: Can't access database
```bash
cd SmartCourseManagement.API
dotnet ef database update
```

---

## 📊 Response Examples

### Successful JWT Token (201 Created)
```json
{
  "message": "User registered successfully",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwicm9sZSI6IlN0dWRlbnQiLCJlbWFpbCI6ImFsaWNlQGV4YW1wbGUuY29tIiwibmFtZSI6IkFsaWNlIEpvaG5zb24iLCJpYXQiOjE3MDA2MDAwMDAsImV4cCI6MTcwMDYwMzYwMH0..."
}
```

### Authorization Error (401)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Unauthorized",
  "status": 401,
  "traceId": "0HN3V5QMKM1K2:00000001"
}
```

### Forbidden Error (403)
```json
{
  "type": "https://...",
  "title": "Forbidden",
  "status": 403,
  "detail": "Access denied. Only admins can delete courses."
}
```

### Validation Error (400)
```json
{
  "type": "https://...",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."],
    "Password": ["The field Password must be between 6 and 100 characters long."]
  }
}
```

---

## 🚀 NEXT STEPS

After completing all tests:

✅ **Test Results Summary**
- All endpoints respond correctly
- JWT authentication is secure
- Role-based authorization is enforced
- Validation returns HTTP 400
- DTOs are used throughout
- LINQ optimization is working

✅ **Frontend Ready**
- Store JWT token in localStorage
- Send token in Authorization header
- Handle 401/403 errors gracefully
- Redirect to login on unauthorized access

---

## 📞 Support

If any test fails:
1. Check the error message in Swagger response
2. Verify JWT token is valid (not expired)
3. Ensure user has correct role
4. Check database has test data (run migrations if needed)
5. Review application logs in terminal

**All tests should pass! 🎉 Your API is production-ready for this assignment.**
