(async () => {
    if (!Auth.requireAuth()) return;

    const user = Auth.getCurrentUser();
    const role = user ? user.role : null;

    let state = {
        page: 1,
        pageSize: 9,
        search: '',
        credits: '',
        sortBy: 'title',
        sortDesc: false,
        totalPages: 1,
        enrolledIds: new Set()
    };

    // Load enrolled course IDs for students
    if (role === 'Student') {
        try {
            const enrollments = await api.enrollments.getMine();
            state.enrolledIds = new Set((enrollments || []).map(e => e.courseId));
        } catch { /* ignore */ }
    }

    const grid = document.getElementById('courses-grid');
    const paginationEl = document.getElementById('pagination');
    const searchInput = document.getElementById('search-input');
    const creditsFilter = document.getElementById('credits-filter');
    const sortSelect = document.getElementById('sort-select');
    const totalCountEl = document.getElementById('total-count');

    async function loadCourses() {
        showLoading();
        try {
            const params = {
                page: state.page,
                pageSize: state.pageSize,
                ...(state.search && { search: state.search }),
                ...(state.credits && { credits: state.credits }),
                sortBy: state.sortBy,
                sortDesc: state.sortDesc
            };
            const result = await api.courses.getAll(params);
            hideLoading();

            const courses = result.items || [];
            state.totalPages = result.totalPages || 1;

            if (totalCountEl) totalCountEl.textContent = result.totalCount || 0;

            renderCourses(courses);
            renderPagination(result);
        } catch (err) {
            hideLoading();
            showToast('Failed to load courses.', 'error');
            if (grid) grid.innerHTML = '<p class="text-danger text-center py-4">Failed to load courses.</p>';
        }
    }

    function renderCourses(courses) {
        if (!grid) return;
        if (!courses.length) {
            grid.innerHTML = `<div class="col-12 empty-state"><i class="fas fa-search fa-3x mb-3 text-muted"></i><p>No courses found.</p></div>`;
            return;
        }
        grid.innerHTML = courses.map(c => {
            const isEnrolled = state.enrolledIds.has(c.id);
            const canEnroll = role === 'Student';
            const canManage = role === 'Admin' || (role === 'Instructor' && c.instructorId === user.id);
            return `
            <div class="col-md-6 col-lg-4">
                <div class="course-card h-100">
                    <div class="course-card-header">
                        <span class="credits-badge"><i class="fas fa-star me-1"></i>${c.credits} Credits</span>
                        ${canManage ? `<div class="course-actions">
                            <button class="btn btn-sm btn-outline-primary" onclick="openEditModal(${c.id})" title="Edit"><i class="fas fa-edit"></i></button>
                            <button class="btn btn-sm btn-outline-danger" onclick="deleteCourse(${c.id})" title="Delete"><i class="fas fa-trash"></i></button>
                        </div>` : ''}
                    </div>
                    <h5 class="course-title">${c.title}</h5>
                    <p class="course-desc">${truncate(c.description || '', 100)}</p>
                    <div class="course-card-footer">
                        <span class="text-muted small"><i class="fas fa-chalkboard-teacher me-1"></i>${c.instructorName || 'N/A'}</span>
                        ${canEnroll ? (isEnrolled
                            ? `<button class="btn btn-sm btn-success" disabled><i class="fas fa-check me-1"></i>Enrolled</button>`
                            : `<button class="btn btn-sm btn-primary" onclick="enrollCourse(${c.id})"><i class="fas fa-plus me-1"></i>Enroll</button>`)
                        : ''}
                    </div>
                </div>
            </div>`;
        }).join('');
    }

    function renderPagination(result) {
        if (!paginationEl) return;
        if (result.totalPages <= 1) { paginationEl.innerHTML = ''; return; }

        let html = `<nav><ul class="pagination justify-content-center flex-wrap">`;
        html += `<li class="page-item ${!result.hasPreviousPage ? 'disabled' : ''}">
            <button class="page-link" onclick="changePage(${state.page - 1})"><i class="fas fa-chevron-left"></i></button></li>`;

        const start = Math.max(1, state.page - 2);
        const end = Math.min(result.totalPages, state.page + 2);
        if (start > 1) html += `<li class="page-item"><button class="page-link" onclick="changePage(1)">1</button></li>${start > 2 ? '<li class="page-item disabled"><span class="page-link">…</span></li>' : ''}`;
        for (let i = start; i <= end; i++) {
            html += `<li class="page-item ${i === state.page ? 'active' : ''}"><button class="page-link" onclick="changePage(${i})">${i}</button></li>`;
        }
        if (end < result.totalPages) html += `${end < result.totalPages - 1 ? '<li class="page-item disabled"><span class="page-link">…</span></li>' : ''}<li class="page-item"><button class="page-link" onclick="changePage(${result.totalPages})">${result.totalPages}</button></li>`;

        html += `<li class="page-item ${!result.hasNextPage ? 'disabled' : ''}">
            <button class="page-link" onclick="changePage(${state.page + 1})"><i class="fas fa-chevron-right"></i></button></li>`;
        html += `</ul></nav>`;
        paginationEl.innerHTML = html;
    }

    window.changePage = function (page) {
        if (page < 1 || page > state.totalPages) return;
        state.page = page;
        loadCourses();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    window.enrollCourse = async function (courseId) {
        showLoading();
        try {
            await api.enrollments.enroll(user.id, courseId);
            state.enrolledIds.add(courseId);
            hideLoading();
            showToast('Enrolled successfully!', 'success');
            loadCourses();
        } catch (err) {
            hideLoading();
            showToast(err.message || 'Failed to enroll.', 'error');
        }
    };

    window.deleteCourse = async function (courseId) {
        if (!confirm('Delete this course? This cannot be undone.')) return;
        showLoading();
        try {
            await api.courses.delete(courseId);
            hideLoading();
            showToast('Course deleted.', 'success');
            loadCourses();
        } catch (err) {
            hideLoading();
            showToast(err.message || 'Failed to delete.', 'error');
        }
    };

    window.openEditModal = async function (courseId) {
        showLoading();
        try {
            const course = await api.courses.getById(courseId);
            hideLoading();
            document.getElementById('edit-course-id').value = course.id;
            document.getElementById('edit-title').value = course.title;
            document.getElementById('edit-description').value = course.description || '';
            document.getElementById('edit-credits').value = course.credits;
            const modal = new bootstrap.Modal(document.getElementById('editCourseModal'));
            modal.show();
        } catch (err) {
            hideLoading();
            showToast('Failed to load course details.', 'error');
        }
    };

    const editForm = document.getElementById('edit-course-form');
    if (editForm) {
        editForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('edit-course-id').value;
            const title = document.getElementById('edit-title').value.trim();
            const description = document.getElementById('edit-description').value.trim();
            const credits = parseInt(document.getElementById('edit-credits').value, 10);
            if (!title || !credits) { showToast('Title and credits are required.', 'warning'); return; }
            showLoading();
            try {
                await api.courses.update(id, { title, description, credits, instructorId: user.id });
                hideLoading();
                showToast('Course updated!', 'success');
                bootstrap.Modal.getInstance(document.getElementById('editCourseModal')).hide();
                loadCourses();
            } catch (err) {
                hideLoading();
                showToast(err.message || 'Failed to update course.', 'error');
            }
        });
    }

    // Event listeners
    if (searchInput) {
        searchInput.addEventListener('input', debounce(() => {
            state.search = searchInput.value.trim();
            state.page = 1;
            loadCourses();
        }, 400));
    }

    if (creditsFilter) {
        creditsFilter.addEventListener('change', () => {
            state.credits = creditsFilter.value;
            state.page = 1;
            loadCourses();
        });
    }

    if (sortSelect) {
        sortSelect.addEventListener('change', () => {
            const [field, dir] = sortSelect.value.split(':');
            state.sortBy = field;
            state.sortDesc = dir === 'desc';
            state.page = 1;
            loadCourses();
        });
    }

    // Show create form for instructors/admins
    const createSection = document.getElementById('create-course-section');
    if (createSection) {
        createSection.style.display = (role === 'Instructor' || role === 'Admin') ? '' : 'none';
    }

    const createForm = document.getElementById('create-course-form');
    if (createForm) {
        createForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const title = document.getElementById('new-title').value.trim();
            const description = document.getElementById('new-description').value.trim();
            const credits = parseInt(document.getElementById('new-credits').value, 10);
            if (!title || !credits) { showToast('Title and credits are required.', 'warning'); return; }
            showLoading();
            try {
                await api.courses.create({ title, description, credits, instructorId: user.id });
                hideLoading();
                showToast('Course created!', 'success');
                createForm.reset();
                bootstrap.Modal.getInstance(document.getElementById('createCourseModal')).hide();
                loadCourses();
            } catch (err) {
                hideLoading();
                showToast(err.message || 'Failed to create course.', 'error');
            }
        });
    }

    await loadCourses();
})();
