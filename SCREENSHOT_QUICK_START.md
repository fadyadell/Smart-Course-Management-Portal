# 🎬 Quick Start Guide - Screenshot Capture Instructions

**Objective**: Capture all 33 screenshots of Smart Course Management Portal API  
**Duration**: ~30-45 minutes  
**Difficulty**: Easy (just click, copy-paste, capture!)

---

## ✅ Pre-Flight Checklist

Before you start, verify:

- [ ] API is running on http://localhost:5202
- [ ] Browser can access: http://localhost:5202/swagger
- [ ] `screenshots/` folder exists in your workspace
- [ ] SCREENSHOT_CHECKLIST.md is open in your editor
- [ ] Windows Snipping Tool is installed (Win + Shift + S)

---

## 🎯 Step-by-Step Example (Screenshots 1-3: Registration)

### How to Capture Screenshots 1-3

#### Screenshot 1️⃣: Register as Student

**Step 1**: Open Swagger UI
```
Navigate to: http://localhost:5202/swagger
```
You'll see a page with blue "GET" buttons, green "POST" buttons, etc.

**Step 2**: Find the Register Endpoint
- Scroll down to the **Auth** section (blue box labeled "auth")
- Click **POST /api/auth/register** (green button)
- The endpoint expands to show input/output fields

**Step 3**: Click "Try it out"
- Look for the blue **"Try it out"** button
- Click it to enable the input field

**Step 4**: Copy the JSON
- Click in the **Request Body** text area
- Delete any placeholder text
- Copy this JSON and paste it:

```json
{
  "email": "student1@example.com",
  "password": "StudentPass123!",
  "firstName": "Alice",
  "lastName": "Johnson",
  "role": "Student"
}
```

**Step 5**: Execute the Request
- Click the blue **"Execute"** button
- Wait ~2 seconds for response

**Step 6**: Capture Screenshot
- Press **Win + Shift + S** (Windows Snipping Tool opens)
- Click and drag to select:
  - The endpoint name (POST /api/auth/register)
  - The request JSON
  - The entire response (scroll down if needed)
  - The status code (201 Created)
- Release mouse - image is copied
- Paste in Paint or File Explorer: `Ctrl + V`
- Save as: **`01_Register_Student.png`** in `screenshots/` folder

✅ **Screenshot 1 Complete!**

---

#### Screenshot 2️⃣: Register as Instructor

**Repeat the same steps**, but:
1. Use the **same endpoint** (POST /api/auth/register)
2. Use **different JSON**:

```json
{
  "email": "instructor2@example.com",
  "password": "InstructorPass123!",
  "firstName": "Bob",
  "lastName": "Smith",
  "role": "Instructor"
}
```

4. Click Execute
5. Capture screenshot (same process as before)
6. Save as: **`02_Register_Instructor.png`**

✅ **Screenshot 2 Complete!**

---

#### Screenshot 3️⃣: Register as Admin

**Repeat again with**:

```json
{
  "email": "admin@example.com",
  "password": "AdminPass123!",
  "firstName": "Charlie",
  "lastName": "Brown",
  "role": "Admin"
}
```

Save as: **`03_Register_Admin.png`**

✅ **Screenshot 3 Complete!**

---

## 🔑 How to Add Authorization Token (Screenshots 9-25)

Many endpoints need authentication. Here's how:

**One time setup** (do this once):

1. From **Screenshot 4** (Login Student), copy the `accessToken` value
   - Look in the response JSON
   - Find `"accessToken": "eyJhbGc..."`
   - Copy everything between the quotes

2. In Swagger UI, click **"Authorize"** button (top-right corner, above the pink "GET" buttons)

3. Paste this:
   ```
   bearer YOUR_TOKEN_HERE
   ```
   (Replace YOUR_TOKEN_HERE with the token you copied)

4. Click "Authorize" button in the popup

5. Click "Close"

**Now** all subsequent requests will include this token automatically! 

---

## 📸 Quick Reference - All 33 Screenshots

### Shots 1-8: Authentication (Register & Login)
- No auth needed
- Just fill JSON and execute
- All return 2xx status codes

### Shots 9-14: Courses (CRUD operations)
- **Need auth token from Screenshot 4**
- Screenshot 11: Save the courseId (you'll need it for shots 12-13)

### Shots 15-16: Students
- **Need Admin token from Screenshot 6**
- Use it in Authorize

### Shots 17-19: Instructors
- **Instructor token** or no token (GET endpoints work without auth)

### Shots 20-25: Enrollments
- **Need Student token from Screenshot 4**
- Screenshot 21: Save enrollmentId for shots 22-24

### Shots 26-29: Errors
- **These intentionally fail**
- Show 401, 403, 404, 400 responses
- Screenshot 26: DO NOT add token (to get 401)

### Shots 30-33: Bonus
- Shots 30-31: Scroll down to "Schemas" section
- Shot 32: New tab to /hangfire dashboard
- Shot 33: Run rate limiting script (see RATE_LIMIT_TEST.md)

---

## 🚀 Full Workflow

1. **Open both windows side-by-side:**
   - Window 1: SCREENSHOT_CHECKLIST.md (this file)
   - Window 2: http://localhost:5202/swagger

2. **For each screenshot:**
   - ☐ Read the checklist line for that screenshot
   - ☐ Find the endpoint in Swagger
   - ☐ Copy-paste the JSON from SWAGGER_TESTING_GUIDE.md
   - ☐ Click "Try it out" → Fill JSON → Click "Execute"
   - ☐ Capture with Snipping Tool (Win + Shift + S)
   - ☐ Save with exact filename to `screenshots/` folder
   - ☐ Mark ✅ in SCREENSHOT_CHECKLIST.md

3. **When all 33 are done:**
   - ☐ Verify folder has exactly 33 PNG files
   - ☐ Check filenames match (01_, 02_, 03_... 33_)
   - ☐ Create SUBMISSION_REPORT.md (copy from SUBMISSION_TEMPLATE.md)
   - ☐ You're done! 🎉

---

## 💡 Pro Tips

### Capture Better Screenshots
- **Expand sections**: Click the response area arrow to fill screen
- **Show full JSON**: Scroll if needed to show all data
- **Frame it right**: Include method, endpoint, and status code in shot
- **Consistent style**: Same framing for all 33 shots looks professional

### Save Time
- **Use Ctrl+C / Ctrl+V** for copy-pasting JSON between shots
- **Batch similar shots**: Do all Auth shots (1-8) together, then all Course shots, etc.
- **Keep Snipping Tool open**: Press Win+Shift+S between each shot (stays open)
- **Organize files**: Save to correct folder immediately (not desktop, then move later)

### Troubleshoot Common Issues

**Q: Swagger page won't load?**  
A: API might not be running. Run: `dotnet run` in PowerShell in the API folder

**Q: Can't find endpoint?**  
A: Swagger is organized by sections. Scroll or use search icon (magnifying glass)

**Q: Response is cut off?**  
A: Scroll down in the response box to see full JSON

**Q: Snipping Tool won't open?**  
A: Try `Win + Print Screen` instead, or use Paint's Crop tool

**Q: Token gets rejected?**  
A: Use Refresh Token endpoint (shot 7) to get a new one

---

## ⏱️ Time Estimate

- Shots 1-8 (Auth): **~5 minutes**
- Shots 9-14 (Courses): **~6 minutes**
- Shots 15-16 (Students): **~2 minutes**
- Shots 17-19 (Instructors): **~3 minutes**
- Shots 20-25 (Enrollments): **~7 minutes**
- Shots 26-29 (Errors): **~4 minutes**
- Shots 30-33 (Bonus): **~5 minutes**
- **Total: ~30-40 minutes**

---

## 🎯 Final Checklist

When all done:

- [ ] 33 PNG files in `/screenshots/` folder
- [ ] Files named: 01_*, 02_*, ... 33_*
- [ ] Each screenshot shows endpoint + request + response
- [ ] Status codes clearly visible
- [ ] No duplicate screenshots
- [ ] File sizes between 50KB-500KB
- [ ] SCREENSHOT_CHECKLIST.md fully marked ✅
- [ ] SUBMISSION_TEMPLATE.md completed with metadata
- [ ] Ready to zip and submit! 📦

---

## 🎓 You're Ready!

Everything is set up. Here's what you have:

| File | Purpose |
|------|---------|
| SWAGGER_TESTING_GUIDE.md | Detailed endpoint-by-endpoint instructions |
| SCREENSHOT_CHECKLIST.md | Tracker for all 33 screenshots |
| RATE_LIMIT_TEST.md | PowerShell script for bonus rate-limiting test |
| SUBMISSION_TEMPLATE.md | Package template for when you're done |
| `/screenshots/` folder | Where to save your PNG files |

**Next Step**: Open http://localhost:5202/swagger and start with Screenshot 1! 🚀

---

**Time to shine! Go capture those screenshots! ✨**
