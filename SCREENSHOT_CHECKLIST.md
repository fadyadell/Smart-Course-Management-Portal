# 📸 Screenshot Capture Checklist

**Total Screenshots**: 33 | **Status**: Ready to Capture  
**Start Date**: April 2, 2026 | **Updated**: April 2, 2026

---

## Instructions

For each screenshot below:
1. Open http://localhost:5202/swagger in your browser
2. Follow the exact steps listed
3. Click "Execute" to run the endpoint
4. **Press PrtScn or use Snipping Tool** to capture
5. Save in `screenshots/` folder with the filename listed
6. Mark ✅ in the Status column when saved

---

## ✅ SECTION 1: Authentication (8 Screenshots)

| # | Name | Test | Status | Filename |
|---|------|------|--------|----------|
| 1 | Register Student | POST /auth/register with student1@example.com | ☐ | 01_Register_Student.png |
| 2 | Register Instructor | POST /auth/register with instructor2@example.com | ☐ | 02_Register_Instructor.png |
| 3 | Register Admin | POST /auth/register with admin@example.com | ☐ | 03_Register_Admin.png |
| 4 | Login Student | POST /auth/login as Student | ☐ | 04_Login_Student.png |
| 5 | Login Instructor | POST /auth/login as Instructor | ☐ | 05_Login_Instructor.png |
| 6 | Login Admin | POST /auth/login as Admin | ☐ | 06_Login_Admin.png |
| 7 | Refresh Token (Valid) | POST /auth/refresh with valid token | ☐ | 07_Refresh_Token_Valid.png |
| 8 | Refresh Token (Invalid) | POST /auth/refresh with invalid token (401) | ☐ | 08_Refresh_Token_Invalid.png |

### Key Points:
- **Screenshot 4-6**: Make sure to capture the full token response (accessToken, refreshToken, expiry)
- **Screenshot 7**: Copy refreshToken from Screenshot 5 response
- **Screenshot 8**: Expected status: 401 Unauthorized

---

## ✅ SECTION 2: Courses (6 Screenshots)

| # | Name | Test | Status | Filename |
|---|------|------|--------|----------|
| 9 | Get Courses Paginated | GET /courses with page=1, pageSize=10 | ☐ | 09_Get_Courses_Paginated.png |
| 10 | Search Courses | GET /courses/search with searchTerm="ASP" | ☐ | 10_Search_Courses.png |
| 11 | Create Course | POST /courses with Instructor token | ☐ | 11_Create_Course.png |
| 12 | Get Course by ID | GET /courses/{id} (use ID from #11) | ☐ | 12_Get_Course_By_ID.png |
| 13 | Update Course | PUT /courses/{id} (update title) | ☐ | 13_Update_Course.png |
| 14 | Delete Course | DELETE /courses/{id} | ☐ | 14_Delete_Course.png |

### Key Points:
- **Screenshot 9**: Highlight the pagination fields: `totalItems`, `totalPages`, `hasNextPage`
- **Screenshot 10**: Use "ASP" to find the ASP.NET course pre-seeded in DB
- **Screenshot 11**: Save the `courseId` from response for later tests
- **Screenshot 14**: Status should be 204 No Content

---

## ✅ SECTION 3: Students (2 Screenshots)

| # | Name | Test | Status | Filename |
|---|------|------|--------|----------|
| 15 | Get All Students | GET /students with Admin token | ☐ | 15_Get_All_Students.png |
| 16 | Get Student by ID | GET /students/{id} | ☐ | 16_Get_Student_By_ID.png |

### Key Points:
- **Screenshot 15**: Use admin token; highlight enrollment counts
- **Screenshot 16**: Use student ID from response of #15

---

## ✅ SECTION 4: Instructors (3 Screenshots)

| # | Name | Test | Status | Filename |
|---|------|------|--------|----------|
| 17 | Get All Instructors | GET /instructors | ☐ | 17_Get_All_Instructors.png |
| 18 | Get Instructor by ID | GET /instructors/{id} | ☐ | 18_Get_Instructor_By_ID.png |
| 19 | Update Instructor | PUT /instructors/{id} with new bio | ☐ | 19_Update_Instructor_Profile.png |

### Key Points:
- **Screenshot 17**: Show department and course count fields
- **Screenshot 18**: Use ID from #17 response
- **Screenshot 19**: Update department field; highlight the change

---

## ✅ SECTION 5: Enrollments (6 Screenshots)

| # | Name | Test | Status | Filename |
|---|------|------|--------|----------|
| 20 | Get Enrollments | GET /enrollments with Student token | ☐ | 20_Get_Enrollments.png |
| 21 | Enroll in Course | POST /enrollments with Student token | ☐ | 21_Enroll_In_Course.png |
| 22 | Get Enrollment Details | GET /enrollments/{id} | ☐ | 22_Get_Enrollment_Details.png |
| 23 | Update Grade | PUT /enrollments/{id} with grade="A" (Admin token) | ☐ | 23_Update_Enrollment_Grade.png |
| 24 | Unenroll | DELETE /enrollments/{id} with Student token | ☐ | 24_Unenroll_From_Course.png |
| 25 | Verify Unenroll | GET /enrollments (verify course gone) | ☐ | 25_Verify_Unenrollment.png |

### Key Points:
- **Screenshot 21**: Save the `enrollmentId` for #22-24
- **Screenshot 23**: Use Admin token; highlight Grade field changed
- **Screenshot 24**: Status 204 No Content
- **Screenshot 25**: Compare to #20; enrollment should be removed

---

## ✅ SECTION 6: Error Handling (4 Screenshots)

| # | Name | Test | Status | Filename |
|---|------|------|--------|----------|
| 26 | 401 Unauthorized | GET /courses with NO token | ☐ | 26_Error_401_Unauthorized.png |
| 27 | 403 Forbidden | POST /enrollments with Student role on restricted course | ☐ | 27_Error_403_Forbidden.png |
| 28 | 404 Not Found | GET /courses/{fake-id} | ☐ | 28_Error_404_Not_Found.png |
| 29 | 400 Bad Request | POST /auth/register with invalid email | ☐ | 29_Error_400_Bad_Request.png |

### Key Points:
- **Screenshot 26**: DO NOT add Authorization header
- **Screenshot 28**: Use fake ID: `00000000-0000-0000-0000-000000000000`
- **Screenshot 29**: Use email like "invalid-email" without @

---

## ✅ SECTION 7: Bonus Features (4 Screenshots)

| # | Name | Test | Status | Filename |
|---|------|------|--------|----------|
| 30 | Schemas - Auth | Scroll to Schemas section → LoginRequest | ☐ | 30_Schema_Auth_Request.png |
| 31 | Schemas - Course | Scroll to Schemas section → CourseReadDto | ☐ | 31_Schema_Course_Response.png |
| 32 | Hangfire Dashboard | Open http://localhost:5202/hangfire → Recurring Jobs | ☐ | 32_Hangfire_Recurring_Jobs.png |
| 33 | Rate Limiting | See RATE_LIMIT_TEST.md | ☐ | 33_Rate_Limiting_429.png |

### Key Points:
- **Screenshot 30-31**: Just show the schema definitions
- **Screenshot 32**: Highlight the 3 background jobs with their schedules
- **Screenshot 33**: See separate rate limiting test script below

---

## 🔑 Important Tokens & IDs

Save these from your tests:

```
STUDENT_TOKEN: [Copy from Screenshot 4 -> accessToken]
INSTRUCTOR_TOKEN: [Copy from Screenshot 5 -> accessToken]
ADMIN_TOKEN: [Copy from Screenshot 6 -> accessToken]
REFRESH_TOKEN: [Copy from Screenshot 5 -> refreshToken]
COURSE_ID: [Copy from Screenshot 11 -> id]
STUDENT_ID: [Copy from Screenshot 15 response]
INSTRUCTOR_ID: [Copy from Screenshot 17 response]
ENROLLMENT_ID: [Copy from Screenshot 21 -> id]
```

---

## 📋 Authorization Header Setup

For screenshots that need authentication:

1. Click **"Authorize"** button (top-right corner of Swagger UI)
2. Paste this in the field:
   ```
   bearer YOUR_TOKEN_HERE
   ```
3. Click "Authorize"
4. Click "Close"
5. Now all requests will include this token

---

## 💡 Capture Tips

### Using Windows Snipping Tool:
- Press **Win + Shift + S**
- Select the area containing the endpoint and response
- Save to `screenshots/` folder with the exact filename

### What to Capture:
✅ The HTTP Method and Endpoint path  
✅ The Request JSON (input)  
✅ The Response Status Code  
✅ The Response JSON (output)  
✅ Any error messages  

### Highlight in Each Screenshot:
- Status code (201, 200, 400, etc.)
- Key response fields (tokenexpiry, enrollmentId, etc.)
- Email/role/grade fields to show role-based differences

---

## ✨ Submission Checklist

When all 33 screenshots captured:

- [ ] All files named correctly (01_02_03... etc.)
- [ ] All files in `screenshots/` folder
- [ ] Each screenshot shows full Request + Response
- [ ] Status codes clearly visible
- [ ] No duplicates or blank captures
- [ ] File sizes reasonable (50KB - 500KB each)
- [ ] `SUBMISSION_REPORT.md` created with file list
- [ ] Ready to submit! 🎉

---

## 📞 Troubleshooting

**Q: Can't find Authorize button?**  
A: It's in the top-right corner of Swagger UI, next to the GitHub icon

**Q: My response is cut off?**  
A: Scroll down in the response section after clicking Execute

**Q: Token expires during testing?**  
A: Use the Refresh Token endpoint (Screenshot 7) to get a new one

**Q: Why is my Screenshot 10 not finding courses?**  
A: Make sure you're searching for "ASP" (uppercase) to find ASP.NET Core course

**Q: How do I get different user roles?**  
A: Register/Login with different "role" values in the JSON

---

## 🚀 Next Steps

1. Keep this checklist open in one window
2. Open Swagger UI in another window  
3. Go through Section 1 screenshots 1-8
4. Mark each as ☐ complete
5. Continue to Sections 2-7
6. When all 33 are captured, create `SUBMISSION_REPORT.md`

**Good Luck! You've got this! 🎓**
