(async () => {
    if (!Auth.requireRole(['Admin'])) return;

    const user = Auth.getCurrentUser();
    const av = document.getElementById('user-avatar-btn');
    if (av && user) av.textContent = user.name ? user.name.charAt(0).toUpperCase() : 'A';

    // State
    const usersState   = { page: 1, pageSize: 10, search: '', totalPages: 1 };
    const coursesState = { page: 1, pageSize: 10, search: '', totalPages: 1 };

    // ===== STATS =====
    async function loadStats() {
        try {
            const [coursesRes, usersRes, studentsRes, instructorsRes] = await Promise.all([
                api.courses.getAll({ page: 1, pageSize: 1 }),
                api.users.getAll({ page: 1, pageSize: 1 }),
                api.students.getAll(),
                api.instructors.getAll()
            ]);
            document.getElementById('stat-courses').textContent     = coursesRes.totalCount || 0;
            document.getElementById('stat-users').textContent       = usersRes.totalCount || 0;
            document.getElementById('stat-students').textContent    = Array.isArray(studentsRes) ? studentsRes.length : (studentsRes.totalCount || 0);
            document.getElementById('stat-instructors').textContent = Array.isArray(instructorsRes) ? instructorsRes.length : (instructorsRes.totalCount || 0);
        } catch { /* ignore stats errors */ }
    }

    // ===== USERS =====
    async function loadUsers() {
        showLoading();
        try {
            const result = await api.users.getAll({ page: usersState.page, pageSize: usersState.pageSize, search: usersState.search });
            hideLoading();
            const users = result.items || result || [];
            usersState.totalPages = result.totalPages || 1;
            renderUsersTable(users);
            renderUsersPagination(result);
        } catch (err) {
            hideLoading();
            showToast('Failed to load users.', 'error');
            document.getElementById('users-tbody').innerHTML = `<tr><td colspan="5" class="text-center text-danger">Failed to load users.</td></tr>`;
        }
    }

    function renderUsersTable(users) {
        const tbody = document.getElementById('users-tbody');
        if (!users.length) {
            tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted py-4">No users found.</td></tr>`;
            return;
        }
        tbody.innerHTML = users.map((u, i) => `
            <tr>
                <td data-label="#">${(usersState.page - 1) * usersState.pageSize + i + 1}</td>
                <td data-label="Name"><strong>${u.name}</strong></td>
                <td data-label="Email"><span class="text-muted">${u.email}</span></td>
                <td data-label="Role">${Formatters.badgeForRole(u.role)}</td>
                <td data-label="Actions">
                    <div class="table-actions">
                        <button class="btn btn-sm btn-outline-primary" onclick="openEditUser(${u.id},'${u.name.replace(/'/g,"\\'")}','${u.email}','${u.role}')"><i class="fas fa-edit"></i></button>
                        <button class="btn btn-sm btn-outline-danger" onclick="deleteUser(${u.id})"><i class="fas fa-trash"></i></button>
                    </div>
                </td>
            </tr>`).join('');
    }

    function renderUsersPagination(result) {
        const el = document.getElementById('users-pagination');
        if (!el || result.totalPages <= 1) { if (el) el.innerHTML = ''; return; }
        el.innerHTML = buildPagination(usersState.page, result.totalPages, 'changeUserPage');
    }

    window.changeUserPage = function (page) {
        if (page < 1 || page > usersState.totalPages) return;
        usersState.page = page;
        loadUsers();
    };

    window.openEditUser = function (id, name, email, role) {
        document.getElementById('edit-user-id').value    = id;
        document.getElementById('edit-user-name').value  = name;
        document.getElementById('edit-user-email').value = email;
        document.getElementById('edit-user-role').value  = role;
        new bootstrap.Modal(document.getElementById('editUserModal')).show();
    };

    window.deleteUser = async function (id) {
        if (!confirm('Delete this user? This action cannot be undone.')) return;
        showLoading();
        try {
            await api.users.delete(id);
            hideLoading();
            showToast('User deleted.', 'success');
            loadUsers();
            loadStats();
        } catch (err) {
            hideLoading();
            showToast(err.message || 'Failed to delete user.', 'error');
        }
    };

    const editUserForm = document.getElementById('edit-user-form');
    if (editUserForm) {
        editUserForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id    = document.getElementById('edit-user-id').value;
            const name  = document.getElementById('edit-user-name').value.trim();
            const email = document.getElementById('edit-user-email').value.trim();
            const role  = document.getElementById('edit-user-role').value;
            showLoading();
            try {
                await api.users.update(id, { name, email, role });
                hideLoading();
                showToast('User updated!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('editUserModal')).hide();
                loadUsers();
            } catch (err) {
                hideLoading();
                showToast(err.message || 'Failed to update user.', 'error');
            }
        });
    }

    const userSearch = document.getElementById('user-search');
    if (userSearch) {
        userSearch.addEventListener('input', debounce(() => {
            usersState.search = userSearch.value.trim();
            usersState.page = 1;
            loadUsers();
        }, 400));
    }

    // ===== COURSES =====
    async function loadAdminCourses() {
        showLoading();
        try {
            const result = await api.courses.getAll({ page: coursesState.page, pageSize: coursesState.pageSize, search: coursesState.search });
            hideLoading();
            const courses = result.items || [];
            coursesState.totalPages = result.totalPages || 1;
            renderCoursesTable(courses);
            renderCoursesPagination(result);
        } catch (err) {
            hideLoading();
            showToast('Failed to load courses.', 'error');
            document.getElementById('courses-tbody').innerHTML = `<tr><td colspan="6" class="text-center text-danger">Failed to load courses.</td></tr>`;
        }
    }

    function renderCoursesTable(courses) {
        const tbody = document.getElementById('courses-tbody');
        if (!courses.length) {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-4">No courses found.</td></tr>`;
            return;
        }
        tbody.innerHTML = courses.map((c, i) => `
            <tr>
                <td data-label="#">${(coursesState.page - 1) * coursesState.pageSize + i + 1}</td>
                <td data-label="Title"><strong>${c.title}</strong></td>
                <td data-label="Credits"><span class="credits-badge">${c.credits}</span></td>
                <td data-label="Instructor">${c.instructorName || '—'}</td>
                <td data-label="Enrolled">${c.enrollmentCount || 0}</td>
                <td data-label="Actions">
                    <div class="table-actions">
                        <button class="btn btn-sm btn-outline-primary" onclick="adminEditCourse(${c.id})"><i class="fas fa-edit"></i></button>
                        <button class="btn btn-sm btn-outline-danger" onclick="adminDeleteCourse(${c.id})"><i class="fas fa-trash"></i></button>
                    </div>
                </td>
            </tr>`).join('');
    }

    function renderCoursesPagination(result) {
        const el = document.getElementById('courses-pagination-admin');
        if (!el || result.totalPages <= 1) { if (el) el.innerHTML = ''; return; }
        el.innerHTML = buildPagination(coursesState.page, result.totalPages, 'changeCourseAdminPage');
    }

    window.changeCourseAdminPage = function (page) {
        if (page < 1 || page > coursesState.totalPages) return;
        coursesState.page = page;
        loadAdminCourses();
    };

    window.adminDeleteCourse = async function (id) {
        if (!confirm('Delete this course? This cannot be undone.')) return;
        showLoading();
        try {
            await api.courses.delete(id);
            hideLoading();
            showToast('Course deleted.', 'success');
            loadAdminCourses();
            loadStats();
        } catch (err) {
            hideLoading();
            showToast(err.message || 'Failed to delete course.', 'error');
        }
    };

    window.adminEditCourse = async function (id) {
        showLoading();
        try {
            const course = await api.courses.getById(id);
            hideLoading();
            document.getElementById('admin-edit-course-id').value = course.id;
            document.getElementById('admin-edit-title').value     = course.title;
            document.getElementById('admin-edit-desc').value      = course.description || '';
            document.getElementById('admin-edit-credits').value   = course.credits;
            new bootstrap.Modal(document.getElementById('adminEditCourseModal')).show();
        } catch (err) {
            hideLoading();
            showToast('Failed to load course.', 'error');
        }
    };

    const adminEditForm = document.getElementById('admin-edit-course-form');
    if (adminEditForm) {
        adminEditForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id          = document.getElementById('admin-edit-course-id').value;
            const title       = document.getElementById('admin-edit-title').value.trim();
            const description = document.getElementById('admin-edit-desc').value.trim();
            const credits     = parseInt(document.getElementById('admin-edit-credits').value, 10);
            showLoading();
            try {
                await api.courses.update(id, { title, description, credits });
                hideLoading();
                showToast('Course updated!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('adminEditCourseModal')).hide();
                loadAdminCourses();
            } catch (err) {
                hideLoading();
                showToast(err.message || 'Failed to update course.', 'error');
            }
        });
    }

    const courseSearchAdmin = document.getElementById('course-search-admin');
    if (courseSearchAdmin) {
        courseSearchAdmin.addEventListener('input', debounce(() => {
            coursesState.search = courseSearchAdmin.value.trim();
            coursesState.page = 1;
            loadAdminCourses();
        }, 400));
    }

    // Load instructors for create modal
    try {
        const instructors = await api.instructors.getAll();
        const select = document.getElementById('admin-course-instructor');
        if (select && instructors) {
            (Array.isArray(instructors) ? instructors : instructors.items || []).forEach(inst => {
                const opt = document.createElement('option');
                opt.value = inst.id;
                opt.textContent = inst.name;
                select.appendChild(opt);
            });
        }
    } catch { /* ignore */ }

    const adminCreateForm = document.getElementById('admin-create-course-form');
    if (adminCreateForm) {
        adminCreateForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const title        = document.getElementById('admin-course-title').value.trim();
            const description  = document.getElementById('admin-course-desc').value.trim();
            const credits      = parseInt(document.getElementById('admin-course-credits').value, 10);
            const instructorId = document.getElementById('admin-course-instructor').value || null;
            if (!title || !credits) { showToast('Title and credits are required.', 'warning'); return; }
            showLoading();
            try {
                await api.courses.create({ title, description, credits, instructorId: instructorId ? parseInt(instructorId) : null });
                hideLoading();
                showToast('Course created!', 'success');
                adminCreateForm.reset();
                bootstrap.Modal.getInstance(document.getElementById('adminCreateCourseModal')).hide();
                loadAdminCourses();
                loadStats();
            } catch (err) {
                hideLoading();
                showToast(err.message || 'Failed to create course.', 'error');
            }
        });
    }

    // Helper: build pagination HTML
    function buildPagination(current, total, fn) {
        if (total <= 1) return '';
        let html = `<nav><ul class="pagination justify-content-center flex-wrap">`;
        html += `<li class="page-item ${current === 1 ? 'disabled' : ''}"><button class="page-link" onclick="${fn}(${current - 1})"><i class="fas fa-chevron-left"></i></button></li>`;
        const s = Math.max(1, current - 2), e = Math.min(total, current + 2);
        if (s > 1) html += `<li class="page-item"><button class="page-link" onclick="${fn}(1)">1</button></li>${s > 2 ? '<li class="page-item disabled"><span class="page-link">…</span></li>' : ''}`;
        for (let i = s; i <= e; i++) html += `<li class="page-item ${i === current ? 'active' : ''}"><button class="page-link" onclick="${fn}(${i})">${i}</button></li>`;
        if (e < total) html += `${e < total - 1 ? '<li class="page-item disabled"><span class="page-link">…</span></li>' : ''}<li class="page-item"><button class="page-link" onclick="${fn}(${total})">${total}</button></li>`;
        html += `<li class="page-item ${current === total ? 'disabled' : ''}"><button class="page-link" onclick="${fn}(${current + 1})"><i class="fas fa-chevron-right"></i></button></li>`;
        html += `</ul></nav>`;
        return html;
    }

    // Init
    await Promise.all([loadStats(), loadUsers(), loadAdminCourses()]);
})();
