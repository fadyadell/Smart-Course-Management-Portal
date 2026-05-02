(async () => {
    if (!Auth.requireAuth()) return;

    const user = Auth.getCurrentUser();
    const role = user ? user.role : null;

    // Populate user info
    const fields = {
        'profile-name': user ? user.name : '',
        'profile-email': user ? user.email : '',
        'profile-role': user ? Formatters.formatRole(user.role) : ''
    };
    Object.entries(fields).forEach(([id, val]) => {
        const el = document.getElementById(id);
        if (el) el.textContent = val;
    });

    const roleBadgeEl = document.getElementById('profile-role-badge');
    if (roleBadgeEl && Formatters) roleBadgeEl.innerHTML = Formatters.badgeForRole(role);

    // Load enrolled courses for students
    const enrolledSection = document.getElementById('enrolled-section');
    if (enrolledSection) enrolledSection.style.display = role === 'Student' ? '' : 'none';

    if (role === 'Student') {
        const container = document.getElementById('profile-enrolled-courses');
        showLoading();
        try {
            const enrollments = await api.enrollments.getMine();
            hideLoading();
            if (!enrollments || enrollments.length === 0) {
                if (container) container.innerHTML = `<p class="text-muted">You have no enrolled courses. <a href="courses.html">Browse courses</a></p>`;
                return;
            }
            if (container) {
                container.innerHTML = `<div class="list-group">${enrollments.map(e => `
                    <div class="list-group-item d-flex justify-content-between align-items-center">
                        <div>
                            <strong>${e.course ? e.course.title : `Course #${e.courseId}`}</strong>
                            <div class="text-muted small">${e.course ? Formatters.formatCredits(e.course.credits) : ''} &bull; Enrolled ${Formatters.formatDate(e.enrollmentDate)}</div>
                        </div>
                        <button class="btn btn-sm btn-outline-danger" onclick="unenrollCourse(${e.id})">Unenroll</button>
                    </div>`).join('')}
                </div>`;
            }
        } catch (err) {
            hideLoading();
            showToast('Failed to load enrollments.', 'error');
        }
    }

    window.unenrollCourse = async function (enrollmentId) {
        if (!confirm('Unenroll from this course?')) return;
        showLoading();
        try {
            await api.enrollments.unenroll(enrollmentId);
            hideLoading();
            showToast('Unenrolled successfully.', 'success');
            location.reload();
        } catch (err) {
            hideLoading();
            showToast(err.message || 'Failed to unenroll.', 'error');
        }
    };

    // Logout button
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) logoutBtn.addEventListener('click', () => Auth.logout());
})();
