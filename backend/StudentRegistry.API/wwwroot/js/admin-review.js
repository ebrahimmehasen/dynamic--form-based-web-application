// Admin oversight page: global (cross-student) log of field edits / comments, and the only place
// delete requests can be approved (actually deletes the student) or rejected. Depends on
// editor-shared.js (escapeHtml/formatDate/displayValue/statusLabel/fetchJson) + app-modal.js.

document.addEventListener('DOMContentLoaded', () => {
  initAdminTabs();
  // Dashboard is the default-active tab; it loads itself (admin-dashboard.js).

  const statusFilter = document.getElementById('admin-delete-status-filter');
  if (statusFilter) statusFilter.addEventListener('change', loadDeleteRequests);

  const fromCommentOnly = document.getElementById('admin-edits-from-comment-only');
  if (fromCommentOnly) fromCommentOnly.addEventListener('change', loadFieldEdits);
});

function initAdminTabs() {
  const tabs = {
    dashboard: { btn: 'admin-tab-btn-dashboard', panel: 'admin-tab-dashboard', load: typeof loadDashboard === 'function' ? loadDashboard : () => {} },
    delete: { btn: 'admin-tab-btn-delete', panel: 'admin-tab-delete', load: loadDeleteRequests },
    edits: { btn: 'admin-tab-btn-edits', panel: 'admin-tab-edits', load: loadFieldEdits },
    comments: { btn: 'admin-tab-btn-comments', panel: 'admin-tab-comments', load: loadFieldComments },
    users: { btn: 'admin-tab-btn-users', panel: 'admin-tab-users', load: typeof loadUsers === 'function' ? loadUsers : () => {} }
  };

  Object.entries(tabs).forEach(([key, cfg]) => {
    const btn = document.getElementById(cfg.btn);
    if (!btn) return;
    btn.addEventListener('click', () => {
      Object.values(tabs).forEach(other => {
        document.getElementById(other.btn).classList.toggle('editor-tab-active', other === cfg);
        document.getElementById(other.panel).style.display = other === cfg ? '' : 'none';
      });
      cfg.load();
    });
  });
}

// ---------- Delete requests ----------

async function loadDeleteRequests() {
  const body = document.getElementById('admin-delete-body');
  if (!body) return;
  const status = document.getElementById('admin-delete-status-filter').value;
  body.innerHTML = '<p class="field-hint">جارِ التحميل...</p>';

  try {
    const requests = await fetchJson(`/api/admin/review/deleterequests?status=${encodeURIComponent(status)}`);
    renderDeleteRequests(requests);
  } catch (err) {
    body.innerHTML = `<p class="field-hint">${escapeHtml(err.message || 'تعذر تحميل طلبات الحذف.')}</p>`;
  }
}

function renderDeleteRequests(requests) {
  const body = document.getElementById('admin-delete-body');
  if (!requests || requests.length === 0) {
    body.innerHTML = '<p class="field-hint">لا توجد طلبات مطابقة.</p>';
    return;
  }

  body.innerHTML = `
    <div class="table-responsive">
      <table class="grades-table data-table">
        <thead>
          <tr>
            <th>الطالب</th><th>مقدّم الطلب</th><th>تاريخ الطلب</th><th>السبب</th>
            <th>الحالة</th><th>راجعه</th><th>تاريخ المراجعة</th><th></th>
          </tr>
        </thead>
        <tbody>
          ${requests.map(r => `
            <tr>
              <td>${r.studentId ? escapeHtml(r.studentName || ('طالب #' + r.studentId)) : '<em>تم حذفه</em>'}</td>
              <td>${escapeHtml(r.requestedBy)}</td>
              <td>${formatDate(r.requestedAt)}</td>
              <td>${displayValue(r.reason)}</td>
              <td>${escapeHtml(statusLabel(r.status))}</td>
              <td>${displayValue(r.reviewedBy)}</td>
              <td>${r.reviewedAt ? formatDate(r.reviewedAt) : '—'}</td>
              <td>
                ${r.status === 'pending' ? `
                  <div class="btn-group-row">
                    <button type="button" class="btn btn-secondary admin-approve-delete" data-id="${r.id}" style="width:auto;">موافقة وحذف</button>
                    <button type="button" class="btn btn-secondary admin-reject-delete" data-id="${r.id}" style="width:auto;">رفض</button>
                  </div>` : ''}
              </td>
            </tr>`).join('')}
        </tbody>
      </table>
    </div>`;

  body.querySelectorAll('.admin-approve-delete').forEach(btn => {
    btn.addEventListener('click', () => handleApproveDelete(btn.getAttribute('data-id')));
  });
  body.querySelectorAll('.admin-reject-delete').forEach(btn => {
    btn.addEventListener('click', () => handleRejectDelete(btn.getAttribute('data-id')));
  });
}

async function handleApproveDelete(id) {
  const { confirmed } = await showAppModal({
    title: 'تأكيد حذف الطالب',
    bodyHtml: '<p>سيتم حذف بيانات هذا الطالب نهائياً من النظام. لا يمكن التراجع عن هذا الإجراء. هل تريد المتابعة؟</p>',
    confirmText: 'حذف نهائي',
    cancelText: 'إلغاء',
    variant: 'danger'
  });
  if (!confirmed) return;

  try {
    await fetchJson(`/api/admin/review/deleterequests/${id}/approve`, { method: 'POST' });
    showToast('تم حذف الطالب واعتماد الطلب.', 'success');
    loadDeleteRequests();
  } catch (err) {
    showToast(err.message || 'تعذر اعتماد طلب الحذف.', 'danger');
  }
}

async function handleRejectDelete(id) {
  const { confirmed } = await showAppModal({
    title: 'رفض طلب الحذف',
    bodyHtml: '<p>سيتم رفض هذا الطلب مع الإبقاء على بيانات الطالب كما هي.</p>',
    confirmText: 'رفض الطلب',
    cancelText: 'تراجع',
    variant: 'primary'
  });
  if (!confirmed) return;

  try {
    await fetchJson(`/api/admin/review/deleterequests/${id}/reject`, { method: 'POST' });
    showToast('تم رفض طلب الحذف.', 'success');
    loadDeleteRequests();
  } catch (err) {
    showToast(err.message || 'تعذر رفض طلب الحذف.', 'danger');
  }
}

// ---------- Field edits ----------

async function loadFieldEdits() {
  const body = document.getElementById('admin-edits-body');
  if (!body) return;
  const fromCommentOnly = document.getElementById('admin-edits-from-comment-only').checked;
  body.innerHTML = '<p class="field-hint">جارِ التحميل...</p>';

  try {
    const edits = await fetchJson(`/api/admin/review/fieldedits?fromCommentOnly=${fromCommentOnly}`);
    renderFieldEdits(edits);
  } catch (err) {
    body.innerHTML = `<p class="field-hint">${escapeHtml(err.message || 'تعذر تحميل التعديلات.')}</p>`;
  }
}

function renderFieldEdits(edits) {
  const body = document.getElementById('admin-edits-body');
  if (!edits || edits.length === 0) {
    body.innerHTML = '<p class="field-hint">لا توجد تعديلات مطابقة.</p>';
    return;
  }

  body.innerHTML = `
    <div class="table-responsive">
      <table class="grades-table data-table">
        <thead>
          <tr>
            <th>الطالب</th><th>الحقل</th><th>القيمة القديمة</th><th>القيمة الجديدة</th>
            <th>المحرر</th><th>التاريخ</th><th>المصدر</th><th>ملاحظة</th>
          </tr>
        </thead>
        <tbody>
          ${edits.map(e => `
            <tr>
              <td>${escapeHtml(e.studentName || ('طالب #' + e.studentId))}</td>
              <td>${escapeHtml(e.fieldName)}</td>
              <td>${displayValue(e.oldValue)}</td>
              <td>${displayValue(e.newValue)}</td>
              <td>${escapeHtml(e.editor)}</td>
              <td>${formatDate(e.editedAt)}</td>
              <td>${escapeHtml(statusLabel(e.source))}</td>
              <td>${displayValue(e.note)}</td>
            </tr>`).join('')}
        </tbody>
      </table>
    </div>`;
}

// ---------- Field comments ----------

async function loadFieldComments() {
  const body = document.getElementById('admin-comments-body');
  if (!body) return;
  body.innerHTML = '<p class="field-hint">جارِ التحميل...</p>';

  try {
    const comments = await fetchJson('/api/admin/review/fieldcomments');
    renderFieldComments(comments);
  } catch (err) {
    body.innerHTML = `<p class="field-hint">${escapeHtml(err.message || 'تعذر تحميل التعليقات.')}</p>`;
  }
}

function renderFieldComments(comments) {
  const body = document.getElementById('admin-comments-body');
  if (!comments || comments.length === 0) {
    body.innerHTML = '<p class="field-hint">لا توجد تعليقات.</p>';
    return;
  }

  body.innerHTML = `
    <div class="table-responsive">
      <table class="grades-table data-table">
        <thead>
          <tr><th>الطالب</th><th>الحقل</th><th>التعليق</th><th>الكاتب</th><th>التاريخ</th><th>الحالة</th></tr>
        </thead>
        <tbody>
          ${comments.map(c => `
            <tr>
              <td>${escapeHtml(c.studentName || ('طالب #' + c.studentId))}</td>
              <td>${escapeHtml(c.fieldName)}</td>
              <td>${escapeHtml(c.commentText)}</td>
              <td>${escapeHtml(c.author)}</td>
              <td>${formatDate(c.createdAt)}</td>
              <td>${escapeHtml(statusLabel(c.status))}</td>
            </tr>`).join('')}
        </tbody>
      </table>
    </div>`;
}
