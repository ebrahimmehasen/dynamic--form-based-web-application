// Editor "Notifications" inbox: unreviewed field comments across all students, with the same
// Approve-as-Edit / Write-own-edit / Dismiss actions available from the main Editor page's comment
// popup. Depends on editor-shared.js + app-modal.js.

var notifState = { tab: 'unreviewed', commentsById: {} };

document.addEventListener('DOMContentLoaded', () => {
  initNotifTabs();
  loadNotifications();
});

function initNotifTabs() {
  const unreviewedBtn = document.getElementById('notif-tab-btn-unreviewed');
  const resolvedBtn = document.getElementById('notif-tab-btn-resolved');
  if (unreviewedBtn) unreviewedBtn.addEventListener('click', () => switchTab('unreviewed'));
  if (resolvedBtn) resolvedBtn.addEventListener('click', () => switchTab('resolved'));
}

function switchTab(tab) {
  notifState.tab = tab;
  document.getElementById('notif-tab-btn-unreviewed').classList.toggle('editor-tab-active', tab === 'unreviewed');
  document.getElementById('notif-tab-btn-resolved').classList.toggle('editor-tab-active', tab === 'resolved');
  loadNotifications();
}

async function loadNotifications() {
  const list = document.getElementById('notif-list');
  if (!list) return;
  list.innerHTML = '<p class="field-hint">جارِ التحميل...</p>';
  try {
    const url = notifState.tab === 'unreviewed' ? '/api/editor/fieldcomments/unreviewed' : '/api/editor/fieldcomments/resolved';
    const comments = await fetchJson(url);
    renderNotifications(comments);
  } catch (err) {
    list.innerHTML = `<p class="field-hint">${escapeHtml(err.message || 'تعذر تحميل الإشعارات.')}</p>`;
  }
}

function renderNotifications(comments) {
  const list = document.getElementById('notif-list');
  if (!list) return;

  notifState.commentsById = {};
  (comments || []).forEach(c => { notifState.commentsById[c.id] = c; });

  if (!comments || comments.length === 0) {
    list.innerHTML = '<p class="field-hint">لا توجد عناصر.</p>';
    return;
  }

  list.innerHTML = comments.map(renderNotificationItem).join('');
  wireNotificationActions(list);
}

function renderNotificationItem(c) {
  const actions = c.status === 'unreviewed' ? `
    <div class="editor-comment-actions">
      <button type="button" class="btn btn-secondary notif-approve" data-comment-id="${c.id}">اعتماد كتعديل</button>
      <button type="button" class="btn btn-secondary notif-write" data-comment-id="${c.id}">كتابة تعديل خاص</button>
      <button type="button" class="btn btn-secondary notif-dismiss" data-comment-id="${c.id}">تجاهل</button>
    </div>
    <div class="editor-comment-edit-area" id="notif-comment-edit-${c.id}"></div>`
    : `<div class="editor-notif-status">${escapeHtml(statusLabel(c.status))}</div>`;

  return `
    <div class="editor-notif-item">
      <div class="editor-notif-header">
        <a href="/editor?studentId=${c.studentId}" class="editor-notif-student-link">${escapeHtml(c.studentName || ('طالب #' + c.studentId))}</a>
        <span class="editor-notif-field">${escapeHtml(c.fieldName)}</span>
        <span class="editor-notif-meta">${escapeHtml(c.author)} — ${formatDate(c.createdAt)}</span>
      </div>
      <div class="editor-notif-body">${escapeHtml(c.commentText)}</div>
      ${actions}
    </div>`;
}

function wireNotificationActions(container) {
  container.querySelectorAll('.notif-approve').forEach(btn => {
    btn.addEventListener('click', () => openNotifEditArea(btn, 'from_comment'));
  });
  container.querySelectorAll('.notif-write').forEach(btn => {
    btn.addEventListener('click', () => openNotifEditArea(btn, 'manual'));
  });
  container.querySelectorAll('.notif-dismiss').forEach(btn => {
    btn.addEventListener('click', () => handleNotifDismiss(btn.getAttribute('data-comment-id')));
  });
}

function openNotifEditArea(btn, source) {
  const commentId = btn.getAttribute('data-comment-id');
  const comment = notifState.commentsById[commentId];
  const area = document.getElementById(`notif-comment-edit-${commentId}`);
  if (!area || !comment) return;

  area.innerHTML = `
    <label class="review-field-label">${source === 'from_comment' ? 'القيمة المعتمدة' : 'التعديل الخاص بك'}</label>
    <textarea class="table-input editor-comment-edit-input" rows="2">${escapeHtml(comment.fieldSnapshot || '')}</textarea>
    <div class="btn-group-row" style="margin-top:0.5rem;">
      <button type="button" class="btn btn-primary notif-edit-save" style="flex:1;">حفظ</button>
      <button type="button" class="btn btn-secondary notif-edit-cancel" style="flex:1;">إلغاء</button>
    </div>`;

  const input = area.querySelector('.editor-comment-edit-input');
  input.focus();

  area.querySelector('.notif-edit-cancel').addEventListener('click', () => { area.innerHTML = ''; });
  area.querySelector('.notif-edit-save').addEventListener('click', async () => {
    try {
      const { entityGroup, entityRowId, propertyName } = parseFieldPath(comment.fieldName);
      await applyFieldEdit({
        studentId: comment.studentId, entityGroup, entityRowId, propertyName,
        newValue: input.value, source, triggeringCommentId: comment.id
      });
      showToast('تم حفظ التعديل واعتماد التعليق.', 'success');
      loadNotifications();
    } catch (err) {
      showToast(err.message || 'تعذر حفظ التعديل.', 'danger');
    }
  });
}

async function handleNotifDismiss(commentId) {
  try {
    await dismissFieldComment(commentId);
    showToast('تم تجاهل التعليق.', 'success');
    loadNotifications();
  } catch (err) {
    showToast(err.message || 'تعذر تجاهل التعليق.', 'danger');
  }
}
