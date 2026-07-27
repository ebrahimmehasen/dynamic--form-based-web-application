// Admin Users Management tab: list/add/edit/delete users and change passwords. Passwords are never
// fetched, shown, or pre-filled anywhere here — only ever written via Add/Change Password, and only
// as a one-way hash on the server (see UserManagementService). Depends on editor-shared.js
// (escapeHtml/formatDate/fetchJson) + app-modal.js (used for the two confirm dialogs, not the form).

var adminUsersState = {
  users: [],
  modalMode: null, // 'create' | 'edit' | 'password'
  modalUserId: null
};

document.addEventListener('DOMContentLoaded', () => {
  const addBtn = document.getElementById('admin-add-user-btn');
  if (addBtn) addBtn.addEventListener('click', openCreateUserModal);

  const cancelBtn = document.getElementById('admin-user-modal-cancel');
  const overlay = document.getElementById('admin-user-modal-overlay');
  const saveBtn = document.getElementById('admin-user-modal-save');
  if (cancelBtn) cancelBtn.addEventListener('click', closeUserModal);
  if (overlay) overlay.addEventListener('click', (e) => { if (e.target === overlay) closeUserModal(); });
  if (saveBtn) saveBtn.addEventListener('click', submitUserModal);

  const searchInput = document.getElementById('admin-users-search-input');
  const statusFilter = document.getElementById('admin-users-status-filter');
  if (searchInput) searchInput.addEventListener('input', renderUsers);
  if (statusFilter) statusFilter.addEventListener('change', renderUsers);
});

async function loadUsers() {
  const body = document.getElementById('admin-users-body');
  if (!body) return;
  body.innerHTML = '<p class="field-hint">جارِ التحميل...</p>';

  try {
    adminUsersState.users = await fetchJson('/api/admin/users');
    renderUsers();
  } catch (err) {
    body.innerHTML = `<p class="field-hint">${escapeHtml(err.message || 'تعذر تحميل قائمة المستخدمين.')}</p>`;
  }
}

const ROLE_LABELS = { Viewer: 'مُراجع (Viewer)', Editor: 'محرر (Editor)', Admin: 'مسؤول (Admin)' };

function renderUsers() {
  const body = document.getElementById('admin-users-body');
  if (!body) return;

  if (adminUsersState.users.length === 0) {
    body.innerHTML = '<p class="field-hint">لا يوجد مستخدمون.</p>';
    return;
  }

  const searchInput = document.getElementById('admin-users-search-input');
  const statusFilter = document.getElementById('admin-users-status-filter');
  const searchVal = searchInput ? searchInput.value.trim().toLowerCase() : '';
  const statusVal = statusFilter ? statusFilter.value : '';

  const filtered = adminUsersState.users.filter(user => {
    const matchesSearch = !searchVal || user.username.toLowerCase().includes(searchVal);
    const matchesStatus = !statusVal || (statusVal === 'active' ? user.isActive : !user.isActive);
    return matchesSearch && matchesStatus;
  });

  if (filtered.length === 0) {
    body.innerHTML = '<p class="field-hint">لا يوجد مستخدمون مطابقون لبحثك.</p>';
    return;
  }

  body.innerHTML = `
    <div class="table-responsive">
      <table class="grades-table data-table">
        <thead>
          <tr><th>اسم المستخدم</th><th>الصلاحية</th><th>تاريخ الإنشاء</th><th>الحالة</th><th></th></tr>
        </thead>
        <tbody>
          ${filtered.map(renderUserRow).join('')}
        </tbody>
      </table>
    </div>`;

  body.querySelectorAll('.admin-edit-user').forEach(btn => {
    btn.addEventListener('click', () => openEditUserModal(parseInt(btn.getAttribute('data-id'), 10)));
  });
  body.querySelectorAll('.admin-change-password-user').forEach(btn => {
    btn.addEventListener('click', () => openChangePasswordModal(parseInt(btn.getAttribute('data-id'), 10)));
  });
  body.querySelectorAll('.admin-delete-user').forEach(btn => {
    btn.addEventListener('click', () => handleDeleteUser(parseInt(btn.getAttribute('data-id'), 10)));
  });
}

function renderUserRow(user) {
  const statusBadge = user.isActive
    ? '<span class="editor-notif-status">نشط</span>'
    : '<span class="editor-pending-badge">معطّل</span>';
  const protectedBadge = user.isProtected
    ? '<span class="editor-pending-badge" title="حساب محمي — لا يمكن تعديله أو حذفه">محمي 🔒</span>'
    : '';

  const actions = user.isProtected
    ? protectedBadge
    : `
      <div class="btn-group-row">
        <button type="button" class="btn btn-secondary admin-edit-user" data-id="${user.id}" style="width:auto;">تعديل</button>
        <button type="button" class="btn btn-secondary admin-change-password-user" data-id="${user.id}" style="width:auto;">تغيير كلمة المرور</button>
        <button type="button" class="btn btn-secondary admin-delete-user" data-id="${user.id}" style="width:auto;">حذف</button>
      </div>`;

  return `
    <tr>
      <td>${escapeHtml(user.username)}</td>
      <td>${escapeHtml(ROLE_LABELS[user.role] || user.role)}</td>
      <td>${formatDate(user.createdAt)}</td>
      <td>${statusBadge}</td>
      <td>${actions}</td>
    </tr>`;
}

// ---------- Add / Edit / Change Password modal ----------

function roleOptionsHtml(selectedRole) {
  return Object.entries(ROLE_LABELS)
    .map(([value, label]) => `<option value="${value}" ${value === selectedRole ? 'selected' : ''}>${label}</option>`)
    .join('');
}

function openCreateUserModal() {
  adminUsersState.modalMode = 'create';
  adminUsersState.modalUserId = null;

  document.getElementById('admin-user-modal-title').textContent = 'إضافة مستخدم جديد';
  document.getElementById('admin-user-modal-body').innerHTML = `
    <label class="review-field-label" for="admin-user-username">اسم المستخدم</label>
    <input type="text" id="admin-user-username" class="table-input" autocomplete="off">
    <label class="review-field-label" for="admin-user-password">كلمة المرور</label>
    <input type="password" id="admin-user-password" class="table-input" autocomplete="new-password">
    <label class="review-field-label" for="admin-user-role">الصلاحية</label>
    <select id="admin-user-role" class="table-input">${roleOptionsHtml('Viewer')}</select>`;

  showUserModal();
}

function openEditUserModal(id) {
  const user = adminUsersState.users.find(u => u.id === id);
  if (!user) return;

  adminUsersState.modalMode = 'edit';
  adminUsersState.modalUserId = id;

  document.getElementById('admin-user-modal-title').textContent = 'تعديل بيانات المستخدم';
  document.getElementById('admin-user-modal-body').innerHTML = `
    <label class="review-field-label" for="admin-user-username">اسم المستخدم</label>
    <input type="text" id="admin-user-username" class="table-input" value="${escapeHtml(user.username)}" autocomplete="off">
    <label class="review-field-label" for="admin-user-role">الصلاحية</label>
    <select id="admin-user-role" class="table-input">${roleOptionsHtml(user.role)}</select>
    <label class="editor-admin-checkbox" style="margin-top:1rem;">
      <input type="checkbox" id="admin-user-active" ${user.isActive ? 'checked' : ''}>
      حساب نشط
    </label>`;

  showUserModal();
}

function openChangePasswordModal(id) {
  const user = adminUsersState.users.find(u => u.id === id);
  if (!user) return;

  adminUsersState.modalMode = 'password';
  adminUsersState.modalUserId = id;

  document.getElementById('admin-user-modal-title').textContent = `تغيير كلمة مرور: ${user.username}`;
  document.getElementById('admin-user-modal-body').innerHTML = `
    <label class="review-field-label" for="admin-user-new-password">كلمة المرور الجديدة</label>
    <input type="password" id="admin-user-new-password" class="table-input" autocomplete="new-password">
    <label class="review-field-label" for="admin-user-new-password-confirm">تأكيد كلمة المرور</label>
    <input type="password" id="admin-user-new-password-confirm" class="table-input" autocomplete="new-password">`;

  showUserModal();
}

function showUserModal() {
  const overlay = document.getElementById('admin-user-modal-overlay');
  if (overlay) overlay.style.display = 'flex';
  const firstInput = document.querySelector('#admin-user-modal-body input, #admin-user-modal-body select');
  if (firstInput) setTimeout(() => firstInput.focus(), 50);
}

function closeUserModal() {
  const overlay = document.getElementById('admin-user-modal-overlay');
  if (overlay) overlay.style.display = 'none';
  adminUsersState.modalMode = null;
  adminUsersState.modalUserId = null;
}

async function submitUserModal() {
  const mode = adminUsersState.modalMode;
  try {
    if (mode === 'create') {
      const username = document.getElementById('admin-user-username').value.trim();
      const password = document.getElementById('admin-user-password').value;
      const role = document.getElementById('admin-user-role').value;
      await fetchJson('/api/admin/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password, role })
      });
      showToast('تم إضافة المستخدم بنجاح.', 'success');
    } else if (mode === 'edit') {
      const username = document.getElementById('admin-user-username').value.trim();
      const role = document.getElementById('admin-user-role').value;
      const isActive = document.getElementById('admin-user-active').checked;
      await fetchJson(`/api/admin/users/${adminUsersState.modalUserId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, role, isActive })
      });
      showToast('تم حفظ التعديلات بنجاح.', 'success');
    } else if (mode === 'password') {
      const newPassword = document.getElementById('admin-user-new-password').value;
      const confirmPassword = document.getElementById('admin-user-new-password-confirm').value;
      if (newPassword !== confirmPassword) {
        showToast('كلمتا المرور غير متطابقتين.', 'danger');
        return;
      }
      await fetchJson(`/api/admin/users/${adminUsersState.modalUserId}/change-password`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ newPassword })
      });
      showToast('تم تغيير كلمة المرور بنجاح.', 'success');
    } else {
      return;
    }

    closeUserModal();
    loadUsers();
  } catch (err) {
    showToast(err.message || 'حدث خطأ غير متوقع.', 'danger');
  }
}

// ---------- Delete ----------

async function handleDeleteUser(id) {
  const user = adminUsersState.users.find(u => u.id === id);
  if (!user) return;

  const { confirmed } = await showAppModal({
    title: 'تأكيد حذف المستخدم',
    bodyHtml: `<p>سيتم حذف المستخدم <strong>${escapeHtml(user.username)}</strong> نهائياً. هل تريد المتابعة؟</p>`,
    confirmText: 'حذف نهائي',
    cancelText: 'إلغاء',
    variant: 'danger'
  });
  if (!confirmed) return;

  try {
    await fetchJson(`/api/admin/users/${id}`, { method: 'DELETE' });
    showToast('تم حذف المستخدم بنجاح.', 'success');
    loadUsers();
  } catch (err) {
    showToast(err.message || 'تعذر حذف المستخدم.', 'danger');
  }
}
