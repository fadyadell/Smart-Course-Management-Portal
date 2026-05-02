(async () => {
    if (!Auth.requireAuth()) return;

    const user = Auth.getCurrentUser();
    const role = user ? user.role : null;

    // Populate navbar user info
    const userNameEl = document.getElementById('navbar-username');
    if (userNameEl) userNameEl.textContent = user ? user.name : '';
    const userRoleEl = document.getElementById('navbar-role');
    if (userRoleEl && Formatters) userRoleEl.innerHTML = Formatters.badgeForRole(role);

    // Welcome message
    const welcomeEl = document.getElementById('welcome-name');
    if (welcomeEl) welcomeEl.textContent = user ? user.name : '';

    // Show/hide role-based sections
    document.querySelectorAll('[data-role]').forEach(el => {
        const allowed = el.dataset.role.split(',').map(r => r.trim());
        el.style.display = allowed.includes(role) ? '' : 'none';
    });

    if (role === 'Student') await loadStudentDashboard(user);
    else if (role === 'Instructor') await loadInstructorDashboard(user);
    else if (role === 'Admin') await loadAdminDashboard();
})();

async function loadStudentDashboard(user) {
    const container = document.getElementById('student-courses');
    if (!container) return;
    showLoading();
    try {
        const enrollments = await api.enrollments.getMine();
        hideLoading();
        if (!enrollments || enrollments.length === 0) {
            container.innerHTML = `<div class="empty-state"><i class="fas fa-book-open fa-3x mb-3 text-muted"></i><p>You are not enrolled in any courses yet.</p><a href="courses.html" class="btn btn-primary mt-2">Browse Courses</a></div>`;
            return;
        }
        container.innerHTML = enrollments.map(e => `
            <div class="course-card">
                <div class="course-card-header">
                    <span class="credits-badge">${e.course ? e.course.credits : ''} Credits</span>
                </div>
                <h5 class="course-title">${e.course ? e.course.title : e.courseId}</h5>
                <p class="course-desc">${truncate(e.course ? e.course.description : '', 80)}</p>
                <div class="course-card-footer">
                    <span class="text-muted small"><i class="fas fa-calendar me-1"></i>${formatDate(e.enrollmentDate)}</span>
                    <button class="btn btn-sm btn-outline-danger" onclick="unenroll(${e.id})">Unenroll</button>
                </div>
            </div>`).join('');

        // Count
        const countEl = document.getElementById('enrolled-count');
        if (countEl) countEl.textContent = enrollments.length;
    } catch (err) {
        hideLoading();
        showToast('Failed to load enrollments.', 'error');
        container.innerHTML = '<p class="text-danger">Failed to load enrollments.</p>';
    }
}

async function unenroll(enrollmentId) {
    if (!confirm('Are you sure you want to unenroll from this course?')) return;
    showLoading();
    try {
        await api.enrollments.unenroll(enrollmentId);
        hideLoading();
        showToast('Unenrolled successfully.', 'success');
        const user = Auth.getCurrentUser();
        await loadStudentDashboard(user);
    } catch (err) {
        hideLoading();
        showToast(err.message || 'Failed to unenroll.', 'error');
    }
}

async function loadInstructorDashboard(user) {
    showLoading();
    try {
        const result = await api.courses.getAll({ instructorId: user.id, pageSize: 100 });
        hideLoading();
        const courses = result.items || result;

        const countEl = document.getElementById('my-courses-count');
        if (countEl) countEl.textContent = Array.isArray(courses) ? courses.length : 0;

        const container = document.getElementById('instructor-courses');
        if (!container) return;

        if (!courses || courses.length === 0) {
            container.innerHTML = `<div class="empty-state"><i class="fas fa-chalkboard-teacher fa-3x mb-3 text-muted"></i><p>You have not created any courses yet.</p></div>`;
            return;
        }

        container.innerHTML = courses.map(c => `
            <div class="course-card">
                <div class="course-card-header">
                    <span class="credits-badge">${c.credits} Credits</span>
                    <div class="course-actions">
                        <button class="btn btn-sm btn-outline-primary" onclick="editCourse(${c.id})"><i class="fas fa-edit"></i></button>
                        <button class="btn btn-sm btn-outline-danger" onclick="deleteCourse(${c.id})"><i class="fas fa-trash"></i></button>
                    </div>
                </div>
                <h5 class="course-title">${c.title}</h5>
                <p class="course-desc">${truncate(c.description, 80)}</p>
                <div class="course-card-footer text-muted small">
                    <span><i class="fas fa-users me-1"></i>${c.enrollmentCount || 0} enrolled</span>
                </div>
            </div>`).join('');
    } catch (err) {
        hideLoading();
        showToast('Failed to load your courses.', 'error');
    }

    // Create course form
    const createForm = document.getElementById('create-course-form');
    if (createForm) {
        createForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const title = document.getElementById('course-title').value.trim();
            const description = document.getElementById('course-description').value.trim();
            const credits = parseInt(document.getElementById('course-credits').value, 10);
            if (!title || !credits) { showToast('Title and credits are required.', 'warning'); return; }
            showLoading();
            try {
                const currentUser = Auth.getCurrentUser();
                await api.courses.create({ title, description, credits, instructorId: currentUser.id });
                hideLoading();
                showToast('Course created successfully!', 'success');
                createForm.reset();
                await loadInstructorDashboard(currentUser);
            } catch (err) {
                hideLoading();
                showToast(err.message || 'Failed to create course.', 'error');
            }
        });
    }
}

async function deleteCourse(courseId) {
    if (!confirm('Delete this course? This action cannot be undone.')) return;
    showLoading();
    try {
        await api.courses.delete(courseId);
        hideLoading();
        showToast('Course deleted.', 'success');
        const user = Auth.getCurrentUser();
        await loadInstructorDashboard(user);
    } catch (err) {
        hideLoading();
        showToast(err.message || 'Failed to delete course.', 'error');
    }
}

function editCourse(courseId) {
    navigateTo(`courses.html?edit=${courseId}`);
}

async function loadAdminDashboard() {
    showLoading();
    try {
        const [coursesRes, usersRes] = await Promise.all([
            api.courses.getAll({ page: 1, pageSize: 1 }),
            api.users.getAll({ page: 1, pageSize: 1 })
        ]);
        hideLoading();

        const totalCoursesEl = document.getElementById('total-courses');
        if (totalCoursesEl) totalCoursesEl.textContent = coursesRes.totalCount || 0;

        const totalUsersEl = document.getElementById('total-users');
        if (totalUsersEl) totalUsersEl.textContent = usersRes.totalCount || 0;
    } catch (err) {
        hideLoading();
        showToast('Failed to load statistics.', 'error');
    }
}
