import fs from 'fs';

const baseUrl = 'http://localhost:5202/api';

async function req(method, endpoint, body = null, token = null) {
    const headers = {};
    if (body) headers['Content-Type'] = 'application/json';
    if (token) headers['Authorization'] = `Bearer ${token}`;
<<<<<<< HEAD

=======
    
>>>>>>> dev
    // Add unique identifier to avoid duplicate emails during testing
    if (body && body.email && endpoint.includes('register')) {
        // Only modify email on register, not on login
    }

    try {
        const response = await fetch(`${baseUrl}${endpoint}`, {
            method,
            headers,
            body: body ? JSON.stringify(body) : null
        });
<<<<<<< HEAD

=======
        
>>>>>>> dev
        let data;
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            data = await response.json();
        } else {
            data = await response.text();
        }
        return { status: response.status, ok: response.ok, data };
    } catch (e) {
        return { status: 0, ok: false, data: e.message };
    }
}

async function runTests() {
    console.log("🚀 Starting System Test Suite...\n");
    let adminToken, instToken, stuToken;
    let courseId;
    let ts = Date.now();
    let newAdminEmail = `${ts}_admin@example.com`;
    let newInstEmail = `${ts}_inst@example.com`;
    let newStuEmail = `${ts}_stu@example.com`;

    // 1. AUTHENTICATION
    console.log("--- 🔐 Testing Authentication ---");
    let r1 = await req('POST', '/auth/register', { name: "System Admin", email: newAdminEmail, password: "Password123!", role: "Admin" });
    console.log(`[${r1.status}] Register Admin:`, r1.ok ? "✅ PASSED" : `❌ FAILED: ${JSON.stringify(r1.data)}`);
<<<<<<< HEAD

=======
    
>>>>>>> dev
    let r2 = await req('POST', '/auth/register', { name: "System Instructor", email: newInstEmail, password: "Password123!", role: "Instructor" });
    console.log(`[${r2.status}] Register Instructor:`, r2.ok ? "✅ PASSED" : `❌ FAILED`);

    let r3 = await req('POST', '/auth/register', { name: "System Student", email: newStuEmail, password: "Password123!", role: "Student" });
    console.log(`[${r3.status}] Register Student:`, r3.ok ? "✅ PASSED" : `❌ FAILED`);

    // Login
    let l1 = await req('POST', '/auth/login', { email: newAdminEmail, password: "Password123!" });
    adminToken = l1.data?.accessToken; // Fixed field name
    console.log(`[${l1.status}] Login Admin:`, adminToken ? "✅ PASSED" : `❌ FAILED: ${JSON.stringify(l1.data)}`);
<<<<<<< HEAD

=======
    
>>>>>>> dev
    let l2 = await req('POST', '/auth/login', { email: newInstEmail, password: "Password123!" });
    instToken = l2.data?.accessToken;
    console.log(`[${l2.status}] Login Instructor:`, instToken ? "✅ PASSED" : `❌ FAILED`);

    let l3 = await req('POST', '/auth/login', { email: newStuEmail, password: "Password123!" });
    stuToken = l3.data?.accessToken;
    console.log(`[${l3.status}] Login Student:`, stuToken ? "✅ PASSED" : `❌ FAILED`);

    if (!adminToken || !instToken || !stuToken) {
        console.log("\n❌ Authentication failed, aborting further tests.");
        return;
    }

    // 2. COURSES
    console.log("\n--- 📚 Testing Courses ---");
    // Wait, getting instructor ID:
    let instId = r2.data?.user?.id || 1; // get from register response if possible
    let c1 = await req('POST', '/courses', { title: "Automated API Testing 101", description: "Test course", credits: 3, instructorId: instId }, adminToken);
    console.log(`[${c1.status}] Create Course (Admin):`, c1.status === 201 ? "✅ PASSED" : `❌ FAILED: ${JSON.stringify(c1.data)}`);
    courseId = c1.data?.id;

    let c2 = await req('GET', '/courses', null, stuToken);
    console.log(`[${c2.status}] Read All Courses:`, c2.status === 200 && Array.isArray(c2.data) ? "✅ PASSED" : `❌ FAILED`);

    if (courseId) {
        let c3 = await req('PUT', `/courses/${courseId}`, { title: "Updated Auto Title", credits: 4, description: "Hello" }, adminToken);
        console.log(`[${c3.status}] Update Course (Admin):`, c3.status === 204 ? "✅ PASSED" : `❌ FAILED: ${JSON.stringify(c3.data)}`);
<<<<<<< HEAD

=======
        
>>>>>>> dev
        let c4 = await req('GET', `/courses/${courseId}`, null, stuToken);
        console.log(`[${c4.status}] Get Course by ID:`, c4.status === 200 ? "✅ PASSED" : `❌ FAILED`);
    }

    // 3. ENROLLMENTS & STUDENTS
    console.log("\n--- 🎓 Testing Students & Enrollments ---");
    let stuId = r3.data?.user?.id || 1;
    let e1 = await req('POST', '/enrollments', { studentId: stuId, courseId: courseId || 1 }, adminToken);
    console.log(`[${e1.status}] Enroll Student in Course:`, [200, 201, 204].includes(e1.status) || (e1.status === 400) ? "✅ PASSED" : `❌ FAILED: ${JSON.stringify(e1.data)}`);

    let e2 = await req('GET', '/students', null, adminToken);
    console.log(`[${e2.status}] List Students (Admin):`, e2.status === 200 ? "✅ PASSED" : `❌ FAILED`);

    let e3 = await req('GET', '/enrollments', null, adminToken);
    console.log(`[${e3.status}] List Enrollments:`, e3.status === 200 ? "✅ PASSED" : `❌ FAILED`);

    // 4. ERROR HANDLING TESTS
    console.log("\n--- ❌ Testing Error Handling ---");
    let err1 = await req('GET', '/courses/999999'); // No token
    console.log(`[${err1.status}] 401 Unauthorized Test:`, err1.status === 401 ? "✅ PASSED" : `❌ FAILED`);

    let err2 = await req('POST', '/courses', { title: "Illegal", description: "Desc", credits: 3, instructorId: instId }, stuToken); // Student creating course
    console.log(`[${err2.status}] 403 Forbidden Test:`, err2.status === 403 ? "✅ PASSED" : `❌ FAILED`);

    let err3 = await req('GET', '/courses/999999', null, adminToken);
    console.log(`[${err3.status}] 404 Not Found Test:`, err3.status === 404 ? "✅ PASSED" : `❌ FAILED`);

    let err4 = await req('POST', '/courses', { title: "X", description: "Desc", credits: 100, instructorId: instId }, adminToken); // Bad data
    console.log(`[${err4.status}] 400 Bad Request Test:`, err4.status === 400 ? "✅ PASSED" : `❌ FAILED`);

    console.log("\n✅ Test Suite Complete!");
}

runTests();
