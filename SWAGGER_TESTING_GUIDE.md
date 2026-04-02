# 🎓 Swagger API Testing Guide

Complete step-by-step guide to test all Smart Course Management Portal API endpoints using Swagger UI.

**Start Date**: April 2, 2026  
**Total Screenshots**: 33  
**Total Test Cases**: 29

---

## 🚀 Setup

### 1. Start the API
```powershell
cd D:\Smart-Course-Management-Portal\SmartCourseManagement.API
dotnet run
```

Wait until you see:
```
Now listening on: http://localhost:5202
Application started.
```

### 2. Open Swagger UI
Navigate to: **http://localhost:5202/swagger**

You'll see all endpoints organized by category.

---

## 📋 Testing Plan (33 Screenshots)

### **SECTION 1: Authentication (8 Screenshots)**

#### Screenshot 1️⃣: Register as Student
1. Click `POST /api/auth/register`
2. Click "Try it out"
3. Paste this JSON:
```json
{
  "email": "student1@example.com",
  "password": "StudentPass123!",
  "firstName": "Alice",
  "lastName": "Johnson",
  "role": "Student"
}
```
4. Click "Execute"
5. **Capture Response**: Status 201 Created with user profile
6. **Highlight**: Email and Role fields

#### Screenshot 2️⃣: Register as Instructor
1. Same endpoint
2. Use this JSON:
```json
{
  "email": "instructor2@example.com",
  "password": "InstructorPass123!",
  "firstName": "Bob",
  "lastName": "Smith",
  "role": "Instructor"
}
```
3. **Capture**: Status 201 Created

#### Screenshot 3️⃣: Register as Admin
1. Same endpoint
2. Use this JSON:
```json
{
  "email": "admin@example.com",
  "password": "AdminPass123!",
  "firstName": "Charlie",
  "lastName": "Brown",
  "role": "Admin"
}
```
3. **Capture**: Status 201 Created

#### Screenshot 4️⃣: Login as Student
1. Click `POST /api/auth/login`
2. Click "Try it out"
3. Paste:
```json
{
  "email": "student1@example.com",
  "password": "StudentPass123!"
}
```
4. Click "Execute"
5. **Capture**: Status 200 OK with:
   - accessToken (JWT)
   - refreshToken
   - accessTokenExpiry
   - user object
6. **Highlight**: The token structure

#### Screenshot 5️⃣: Login as Instructor
1. Same endpoint
2. Use:
```json
{
  "email": "instructor@example.com",
  "password": "InstructorPass123!"
}
```
3. **Capture**: Status 200 OK
4. **Highlight**: Different role in response

#### Screenshot 6️⃣: Login as Admin
1. Same endpoint
2. Use:
```json
{
  "email": "admin@example.com",
  "password": "AdminPass123!"
}
```
3. **Capture**: Status 200 OK

#### Screenshot 7️⃣: Refresh Token Success
1. Click `POST /api/auth/refresh`
2. Click "Try it out"
3. **First login** as instructor (Screenshot 5) to get refreshToken
4. Copy the refreshToken value
5. Paste in refresh endpoint:
```json
{
  "refreshToken": "paste-token-here"
}
```
6. Click "Execute"
7. **Capture**: Status 200 OK with new accessToken

#### Screenshot 8️⃣: Refresh Token Failure
1. Same endpoint
2. Use invalid token:
```json
{
  "refreshToken": "invalid-token-here"
}
```
3. Click "Execute"
4. **Capture**: Status 401 Unauthorized

---

### **SECTION 2: Courses (6 Screenshots)**

#### Screenshot 9️⃣: Get All Courses (Paginated)
1. Click `GET /api/courses`
2. Click "Try it out"
3. Set parameters:
   - page: 1
   - pageSize: 10
4. Add Authorization Header:
   - Click "Authorize" button (top right)
   - Paste: `bearer <accessToken-from-login>`
   - Click "Authorize"
5. Click "Execute"
6. **Capture**: Status 200 OK with:
   - items array (3 courses)
   - totalItems
   - totalPages
   - hasNextPage
7. **Highlight**: Pagination structure

#### Screenshot 🔟: Search Courses
1. Click `GET /api/courses/search`
2. Click "Try it out"
3. Set parameters:
   - searchTerm: ASP
   - sortBy: Title
   - sortDescending: false
   - page: 1
   - pageSize: 10
4. Click "Execute"
5. **Capture**: Status 200 OK with search results
6. **Highlight**: Found 1 course matching "ASP"

#### Screenshot 1️⃣1️⃣: Create Course (Instructor)
1. Click `POST /api/courses`
2. Click "Try it out"
3. Use instructor token in Authorization
4. Paste:
```json
{
  "title": "Advanced JavaScript",
  "description": "Master async/await, promises, and modern JS",
  "credits": 3,
  "maxStudents": 25,
  "startDate": "2026-04-15T00:00:00Z",
  "endDate": "2026-05-30T00:00:00Z"
}
```
5. Click "Execute"
6. **Capture**: Status 201 Created with course ID
7. **Save the courseId for later tests**

#### Screenshot 1️⃣2️⃣: Get Course by ID
1. Click `GET /api/courses/{id}`
2. Click "Try it out"
3. Enter the courseId from Screenshot 11
4. Click "Execute"
5. **Capture**: Status 200 OK with full course details

#### Screenshot 1️⃣3️⃣: Update Course (Instructor)
1. Click `PUT /api/courses/{id}`
2. Click "Try it out"
3. Enter courseId
4. Paste:
```json
{
  "title": "Advanced JavaScript Mastery",
  "description": "Updated description",
  "credits": 4,
  "maxStudents": 30,
  "startDate": "2026-04-15T00:00:00Z",
  "endDate": "2026-05-30T00:00:00Z"
}
```
5. Click "Execute"
6. **Capture**: Status 200 OK with updated fields

#### Screenshot 1️⃣4️⃣: Delete Course (Instructor)
1. Click `DELETE /api/courses/{id}`
2. Click "Try it out"
3. Enter courseId from previous tests
4. Click "Execute"
5. **Capture**: Status 204 No Content
6. **Note**: Soft delete (record preserved in DB)

---

### **SECTION 3: Students (2 Screenshots)**

#### Screenshot 1️⃣5️⃣: Get All Students (Admin)
1. Click `GET /api/students`
2. Click "Try it out"
3. Use admin token in Authorization
4. Set page: 1, pageSize: 10
5. Click "Execute"
6. **Capture**: Status 200 OK with paginated student list
7. **Highlight**: totalItems, enrollment counts

#### Screenshot 1️⃣6️⃣: Get Student by ID
1. Click `GET /api/students/{id}`
2. Click "Try it out"
3. **Get ID from previous response** or use known student ID
4. Click "Execute"
5. **Capture**: Status 200 OK with:
   - Student profile
   - Enrolled courses list
   - Enrollment dates

---

### **SECTION 4: Instructors (3 Screenshots)**

#### Screenshot 1️⃣7️⃣: Get All Instructors
1. Click `GET /api/instructors`
2. Click "Try it out"
3. Set page: 1, pageSize: 10
4. Click "Execute"
5. **Capture**: Status 200 OK with:
   - All instructors
   - Department info
   - Course count
   - Bio

#### Screenshot 1️⃣8️⃣: Get Instructor by ID
1. Click `GET /api/instructors/{id}`
2. Click "Try it out"
3. **Use ID from previous response** (instructor@example.com = dr-jane-smith)
4. Click "Execute"
5. **Capture**: Status 200 OK with:
   - Full instructor profile
   - Office location
   - Courses taught array

#### Screenshot 1️⃣9️⃣: Update Instructor Profile
1. Click `PUT /api/instructors/{id}`
2. Click "Try it out"
3. Enter instructor ID
4. Paste:
```json
{
  "department": "Computer Science",
  "bio": "PhD in CS, 15+ years experience in software development",
  "office": "Tech Building, Room 410"
}
```
5. Click "Execute"
6. **Capture**: Status 200 OK with updated profile

---

### **SECTION 5: Enrollments (6 Screenshots)**

#### Screenshot 2️⃣0️⃣: Get User's Enrollments
1. Click `GET /api/enrollments`
2. Click "Try it out"
3. Use student token in Authorization
4. Set page: 1, pageSize: 10
5. Click "Execute"
6. **Capture**: Status 200 OK with paginated enrollments

#### Screenshot 2️⃣1️⃣: Enroll in Course (Student)
1. Click `POST /api/enrollments`
2. Click "Try it out"
3. Use student token
4. Paste (use existing courseId):
```json
{
  "courseId": "550e8400-e29b-41d4-a716-446655440010"
}
```
5. Click "Execute"
6. **Capture**: Status 201 Created with:
   - Enrollment ID
   - Course name
   - Enrollment date
7. **Save enrollmentId**

#### Screenshot 2️⃣2️⃣: Get Enrollment Details
1. Click `GET /api/enrollments/{id}`
2. Click "Try it out"
3. Enter enrollmentId from Screenshot 21
4. Click "Execute"
5. **Capture**: Status 200 OK with full details

#### Screenshot 2️⃣3️⃣: Update Enrollment Grade (Admin)
1. Click `PUT /api/enrollments/{id}`
2. Click "Try it out"
3. Use admin token
4. Enter enrollmentId
5. Paste:
```json
{
  "status": "Completed",
  "grade": "A"
}
```
6. Click "Execute"
7. **Capture**: Status 200 OK with updated grade

#### Screenshot 2️⃣4️⃣: Unenroll from Course (Student)
1. Click `DELETE /api/enrollments/{id}`
2. Click "Try it out"
3. Use student token
4. Enter enrollmentId
5. Click "Execute"
6. **Capture**: Status 204 No Content

#### Screenshot 2️⃣5️⃣: Verify Unenrollment
1. Click `GET /api/enrollments`
2. Click "Try it out"
3. Use student token
4. Click "Execute"
5. **Capture**: Status 200 OK showing course removed from list

---

### **SECTION 6: Error Handling (4 Screenshots)**

#### Screenshot 2️⃣6️⃣: 401 Unauthorized
1. Click `GET /api/courses`
2. Click "Try it out"
3. **DO NOT add Authorization header**
4. Click "Execute"
5. **Capture**: Status 401 Unauthorized with message

#### Screenshot 2️⃣7️⃣: 403 Forbidden
1. Click `POST /api/enrollments`
2. Click "Try it out"
3. Use student token
4. Try using courseId that's already enrolled or maxed out
5. Click "Execute"
6. **Capture**: Status 403 Forbidden

#### Screenshot 2️⃣8️⃣: 404 Not Found
1. Click `GET /api/courses/{id}`
2. Click "Try it out"
3. Enter fake ID: `00000000-0000-0000-0000-000000000000`
4. Click "Execute"
5. **Capture**: Status 404 Not Found with error message

#### Screenshot 2️⃣9️⃣: 400 Bad Request
1. Click `POST /api/auth/register`
2. Click "Try it out"
3. Paste invalid email:
```json
{
  "email": "invalid-email",
  "password": "WeakPass1!",
  "firstName": "Test",
  "lastName": "User",
  "role": "Student"
}
```
4. Click "Execute"
5. **Capture**: Status 400 Bad Request with validation errors

---

### **SECTION 7: Bonus Features (4 Screenshots)**

#### Screenshot 3️⃣0️⃣: Schemas - Auth Requests
1. Scroll down to "Schemas" section at bottom
2. Click on `LoginRequest`
3. **Capture**: Show the schema definition with all fields and types

#### Screenshot 3️⃣1️⃣: Schemas - Course Response
1. In Schemas section
2. Click on `CourseReadDto`
3. **Capture**: Show the complete response schema

#### Screenshot 3️⃣2️⃣: Hangfire Dashboard
1. Open new tab: `http://localhost:5202/hangfire`
2. Click on "Recurring Jobs"
3. **Capture**: Show the 3 scheduled background jobs:
   - Auto-unenroll from expired courses (Cron: 0 0 * * *)
   - Weekly enrollment report (Cron: 0 8 ? * MON)
   - Revoke old refresh tokens (Cron: 0 2 * * *)

#### Screenshot 3️⃣3️⃣: Rate Limiting
1. Write a script to make 101+ requests in 1 minute
2. After 100 requests, you'll get 429 Too Many Requests
3. **Capture**: The 429 response with message

---

## 📊 Test Summary Table

| Feature | Endpoint | Method | Status | Screenshot |
|---------|----------|--------|--------|-----------|
| Register Student | /auth/register | POST | 201 | 1 |
| Register Instructor | /auth/register | POST | 201 | 2 |
| Register Admin | /auth/register | POST | 201 | 3 |
| Login Student | /auth/login | POST | 200 | 4 |
| Login Instructor | /auth/login | POST | 200 | 5 |
| Login Admin | /auth/login | POST | 200 | 6 |
| Refresh Token | /auth/refresh | POST | 200 | 7 |
| Invalid Refresh | /auth/refresh | POST | 401 | 8 |
| Get Courses | /courses | GET | 200 | 9 |
| Search Courses | /courses/search | GET | 200 | 10 |
| Create Course | /courses | POST | 201 | 11 |
| Get Course | /courses/{id} | GET | 200 | 12 |
| Update Course | /courses/{id} | PUT | 200 | 13 |
| Delete Course | /courses/{id} | DELETE | 204 | 14 |
| Get Students | /students | GET | 200 | 15 |
| Get Student | /students/{id} | GET | 200 | 16 |
| Get Instructors | /instructors | GET | 200 | 17 |
| Get Instructor | /instructors/{id} | GET | 200 | 18 |
| Update Instructor | /instructors/{id} | PUT | 200 | 19 |
| Get Enrollments | /enrollments | GET | 200 | 20 |
| Enroll Course | /enrollments | POST | 201 | 21 |
| Get Enrollment | /enrollments/{id} | GET | 200 | 22 |
| Update Enrollment | /enrollments/{id} | PUT | 200 | 23 |
| Unenroll | /enrollments/{id} | DELETE | 204 | 24 |
| Verify Unenroll | /enrollments | GET | 200 | 25 |
| 401 Error | /courses | GET | 401 | 26 |
| 403 Error | /enrollments | POST | 403 | 27 |
| 404 Error | /courses/{id} | GET | 404 | 28 |
| 400 Error | /auth/register | POST | 400 | 29 |
| Auth Schema | Schemas | - | - | 30 |
| Course Schema | Schemas | - | - | 31 |
| Hangfire Jobs | /hangfire | - | - | 32 |
| Rate Limit | * | - | 429 | 33 |

---

## 🎬 Screenshot Tips

### Before Each Screenshot:
1. Expand the response section fully
2. Scroll to show all important fields
3. Make sure Status code is visible
4. Show the JSON response clearly
5. Highlight key fields with annotations

### What to Include in Captions:
```
Screenshot X: [Feature Name]
- Endpoint: [METHOD /api/...]
- Status: [200/201/401, etc.]
- Key Highlight: [What to focus on]
```

---

## 📝 Expected Outcomes

### Authentication ✅
- 3 users created with different roles
- JWT tokens properly formatted
- Refresh token mechanism works
- 401 on invalid credentials

### Courses ✅
- Create read, update, delete operations
- Pagination works correctly
- Search filters results
- Soft delete implemented

### Relationships ✅
- Students can enroll/unenroll
- Enrollments linked to users and courses
- Instructors have multiple courses
- Audit fields populated

### Security ✅
- 401 without token
- 403 on insufficient permissions
- 404 for non-existent resources
- 400 for invalid input
- Role-based access control

### Advanced ✅
- Hangfire jobs visible
- Rate limiting enforced
- Schemas properly documented

---

## 💾 Saving Your Work

1. Take all 33 screenshots
2. Rename them: `01_Register_Student.png`, `02_Register_Instructor.png`, etc.
3. Create a folder: `screenshots/`
4. Place all images there
5. Generate summary document

---

## ✨ Ready to Start!

Follow the screenshots in order. Each one builds on previous tests. You'll have a comprehensive demonstration of all API features by the end.

**Happy Testing!** 🎓
