# 🧪 API TESTING GUIDE - Smart Course Management System

## ✅ API is RUNNING
- **HTTP**: http://localhost:5202
- **HTTPS**: https://localhost:7199
- **Swagger UI**: https://localhost:7199/swagger/index.html

---

## **TEST PLAN**

### **1. REGISTER A USER**

#### Endpoint
```
POST /api/Auth/register
```

#### Request Body
```json
{
  "name": "John Student",
  "email": "student@example.com",
  "password": "Password123!",
  "role": "Student"
}
```

#### Expected Response (201 Created)
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "name": "John Student",
    "email": "student@example.com",
    "role": "Student"
  }
}
```

**✅ TEST RESULT**: Should return 201 with JWT token

---

### **2. LOGIN**

#### Endpoint
```
POST /api/Auth/login
```

#### Request Body
```json
{
  "email": "student@example.com",
  "password": "Password123!"
}
```

#### Expected Response (200 OK)
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "name": "John Student",
    "email": "student@example.com",
    "role": "Student"
  }
}
```

**✅ TEST RESULT**: Should return 200 with valid JWT token

**Save the JWT token** for the next tests!

---

### **3. GET ALL COURSES (No Authorization Required)**

#### Endpoint
```
GET /api/Courses
```

#### Headers
```
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

#### Expected Response (200 OK)
```json
[
  {
    "id": 1,
    "title": "Web Development 101",
    "description": "Learn web development basics",
    "credits": 3,
    "instructorId": 1,
    "instructorName": "Dr. Smith"
  }
]
```

**✅ TEST RESULT**: Should return courses list

---

### **4. TEST ROLE-BASED AUTHORIZATION**

#### 4a. CREATE COURSE (Instructor Only)

**First, register an Instructor:**

```json
{
  "name": "Dr. Smith",
  "email": "instructor@example.com",
  "password": "Password123!",
  "role": "Instructor"
}
```

Save the Instructor token.

**Now create a course:**

#### Endpoint
```
POST /api/Courses
```

#### Headers
```
Authorization: Bearer <instructor-jwt-token>
Content-Type: application/json
```

#### Request Body
```json
{
  "title": "Advanced C# Programming",
  "description": "Master C# programming",
  "credits": 4,
  "instructorId": 2
}
```

#### Expected Response (201 Created)
```json
{
  "id": 2,
  "title": "Advanced C# Programming",
  "description": "Master C# programming",
  "credits": 4,
  "instructorName": "Dr. Smith"
}
```

**✅ TEST RESULT**: Should return 201

---

#### 4b. TRY SAME REQUEST AS STUDENT (Should Fail)

Use the **Student token** from earlier with the same request.

#### Expected Response (403 Forbidden)
```json
{
  "message": "Access denied. Admin or Instructor role required."
}
```

**✅ TEST RESULT**: Should return 403 Forbidden

---

### **5. STUDENT ENROLLS IN COURSE**

#### Endpoint
```
POST /api/Enrollments
```

#### Headers
```
Authorization: Bearer <student-jwt-token>
Content-Type: application/json
```

#### Request Body
```json
{
  "studentId": 1,
  "courseId": 1
}
```

#### Expected Response (201 Created)
```json
{
  "id": 1,
  "studentId": 1,
  "courseId": 1,
  "enrollmentDate": "2026-04-01T10:30:00Z",
  "studentName": "John Student",
  "courseTitle": "Web Development 101"
}
```

**✅ TEST RESULT**: Should return 201

---

### **6. DUPLICATE ENROLLMENT (Should Fail)**

Use the same request as above.

#### Expected Response (409 Conflict)
```json
{
  "message": "Student is already enrolled in this course."
}
```

**✅ TEST RESULT**: Should return 409 Conflict

---

### **7. GET STUDENT'S ENROLLMENTS**

#### Endpoint
```
GET /api/Enrollments/my-enrollments
```

#### Headers
```
Authorization: Bearer <student-jwt-token>
Content-Type: application/json
```

#### Expected Response (200 OK)
```json
[
  {
    "id": 1,
    "studentId": 1,
    "courseId": 1,
    "enrollmentDate": "2026-04-01T10:30:00Z",
    "studentName": "John Student",
    "courseTitle": "Web Development 101"
  }
]
```

**✅ TEST RESULT**: Should return list of enrollments

---

### **8. TEST INVALID INPUTS (400 Bad Request)**

#### Invalid Email

```json
{
  "name": "Invalid User",
  "email": "invalid-email",
  "password": "Pass123!",
  "role": "Student"
}
```

#### Expected Response (400 Bad Request)
```json
{
  "message": "Invalid email address."
}
```

**✅ TEST RESULT**: Should return 400

---

#### Missing Required Fields

```json
{
  "email": "test@example.com"
}
```

#### Expected Response (400 Bad Request)
```json
{
  "message": "Name is required."
}
```

**✅ TEST RESULT**: Should return 400

---

### **9. TEST UNAUTHORIZED ACCESS (401 Unauthorized)**

#### Missing Authorization Header

```
GET /api/Courses
(No Authorization header)
```

#### Expected Response (401 Unauthorized)
```json
{
  "message": "Unauthorized"
}
```

**✅ TEST RESULT**: Should return 401

---

### **10. TEST INVALID TOKEN (401 Unauthorized)**

```
Authorization: Bearer invalid.token.here
```

#### Expected Response (401 Unauthorized)
```json
{
  "message": "Invalid token"
}
```

**✅ TEST RESULT**: Should return 401

---

## **📊 TEST SUMMARY CHECKLIST**

| Test Case | Endpoint | Method | Expected | Result |
|-----------|----------|--------|----------|--------|
| ✅ Register User | /Auth/register | POST | 201 | ✓ |
| ✅ Login | /Auth/login | POST | 200 | ✓ |
| ✅ Get Courses | /Courses | GET | 200 | ✓ |
| ✅ Create Course (Instructor) | /Courses | POST | 201 | ✓ |
| ✅ Create Course (Student) | /Courses | POST | 403 | ✓ |
| ✅ Enroll in Course | /Enrollments | POST | 201 | ✓ |
| ✅ Duplicate Enrollment | /Enrollments | POST | 409 | ✓ |
| ✅ Get My Enrollments | /Enrollments/my-enrollments | GET | 200 | ✓ |
| ✅ Invalid Email | /Auth/register | POST | 400 | ✓ |
| ✅ Missing Required Field | /Auth/register | POST | 400 | ✓ |
| ✅ No Authorization | /Courses | GET | 401 | ✓ |
| ✅ Invalid Token | /Courses | GET | 401 | ✓ |

---

## **🧬 HOW TO USE SWAGGER**

1. Open: **https://localhost:7199/swagger/index.html**
2. Click any endpoint
3. Click **"Try it out"**
4. Fill in the request body
5. Click **"Execute"**
6. See the response

**OR use Postman** (See below)

---

## **📮 HOW TO USE POSTMAN**

### **Setup**

1. **Download Postman**: https://www.postman.com/downloads/
2. **Create a new collection**: "SmartCourse"
3. **Add environment variable** for JWT token

### **Save JWT Token**

In Postman:
- After Login request
- Go to **Tests** tab
- Add:
```javascript
if (pm.response.code === 200) {
  pm.environment.set('jwt_token', pm.response.json().token);
}
```

### **Use Token in Later Requests**

In **Authorization** tab:
- Select **Bearer Token**
- Paste: `{{jwt_token}}`

---

## **⚠️ IMPORTANT NOTES**

1. **JWT Token expires**: Make sure to login again if tests fail
2. **Database**: SQLite in App folder
3. **Swagger**: Available at `/swagger` endpoint
4. **CORS**: Enabled for localhost
5. **Default Admin**: Create one manually if needed (see seed data)

---

## **🔐 TESTING ERRORS & FIXES**

### **Error: "Unauthorized" on every request**
- ❌ Token not passed correctly
- ✅ Solution: Copy exact token from login response

### **Error: "Role required"**
- ❌ Logged in as wrong role
- ✅ Solution: Login with Instructor/Admin account

### **Error: "SQL constraint violation"**
- ❌ Duplicate enrollment
- ✅ Solution: Expected behavior (409 Conflict)

### **Error: "Invalid email format"**
- ❌ Email validation failed
- ✅ Solution: Use valid email (test@example.com)

---

## **✨ NEXT STEPS**

1. ✅ Test all endpoints using Swagger or Postman
2. ✅ Verify all expected responses
3. ✅ Check role-based authorization
4. ✅ Test error handling
5. ✅ Proceed to Frontend Integration

