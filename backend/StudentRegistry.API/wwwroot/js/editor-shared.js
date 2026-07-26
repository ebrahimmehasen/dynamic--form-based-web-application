// Shared helpers for the Student Records Editor page and its Notifications inbox. Vanilla JS, no
// framework/module system — same string-template + innerHTML convention as review-page.js.

function escapeHtml(value) {
  if (value === null || value === undefined) return '';
  return String(value)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

function formatDate(isoString) {
  if (!isoString) return '';
  const d = new Date(isoString);
  if (isNaN(d.getTime())) return escapeHtml(isoString);
  return d.toLocaleString('ar-EG', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
}

function displayValue(value) {
  if (value === null || value === undefined || value === '') return '—';
  return escapeHtml(value);
}

function cssEscape(value) {
  return typeof CSS !== 'undefined' && CSS.escape ? CSS.escape(value) : String(value).replace(/["\\]/g, '\\$&');
}

const EDITOR_STATUS_LABELS = {
  unreviewed: 'غير مراجع',
  approved_as_edit: 'تم اعتماده كتعديل',
  dismissed: 'تم الرفض',
  pending: 'قيد المراجعة',
  approved: 'تمت الموافقة',
  rejected: 'مرفوض',
  manual: 'يدوي',
  from_comment: 'من تعليق'
};

function statusLabel(status) {
  return EDITOR_STATUS_LABELS[status] || status || '';
}

// Mirrors StudentRegistry.Application.Editor.FieldPath on the server: "Group.Prop" or
// "Group#RowId.Prop" — the one canonical field address used across edits, comments and the audit log.
function parseFieldPath(fieldPath) {
  const dotIndex = fieldPath.lastIndexOf('.');
  const head = fieldPath.slice(0, dotIndex);
  const propertyName = fieldPath.slice(dotIndex + 1);
  const hashIndex = head.indexOf('#');
  if (hashIndex >= 0) {
    return { entityGroup: head.slice(0, hashIndex), entityRowId: parseInt(head.slice(hashIndex + 1), 10), propertyName };
  }
  return { entityGroup: head, entityRowId: null, propertyName };
}

function formatFieldPath(entityGroup, entityRowId, propertyName) {
  return entityRowId !== null && entityRowId !== undefined ? `${entityGroup}#${entityRowId}.${propertyName}` : `${entityGroup}.${propertyName}`;
}

async function fetchJson(url, options) {
  const response = await fetch(url, options);
  const payload = await response.json().catch(() => ({}));
  if (!response.ok || payload.status !== 'success') {
    throw new Error(payload.message || 'حدث خطأ غير متوقع.');
  }
  return payload.data;
}

// Covers a plain inline edit, "Approve as Edit" and "Write own edit" — all three save through this
// one endpoint, differing only in source/triggeringCommentId.
function applyFieldEdit({ studentId, entityGroup, entityRowId, propertyName, newValue, note, source, triggeringCommentId }) {
  return fetchJson('/api/editor/fieldedits', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      studentId, entityGroup, entityRowId: entityRowId ?? null, propertyName,
      newValue: newValue === undefined ? null : newValue, note: note || null,
      source: source || 'manual', triggeringCommentId: triggeringCommentId ?? null
    })
  });
}

function addFieldComment({ studentId, entityGroup, entityRowId, propertyName, fieldSnapshot, commentText }) {
  return fetchJson('/api/editor/fieldcomments', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ studentId, entityGroup, entityRowId: entityRowId ?? null, propertyName, fieldSnapshot, commentText })
  });
}

function dismissFieldComment(id) {
  return fetchJson(`/api/editor/fieldcomments/${id}/dismiss`, { method: 'POST' });
}

async function loadUnreviewedBadge() {
  const badge = document.getElementById('editor-unreviewed-badge');
  if (!badge) return;
  try {
    const count = await fetchJson('/api/editor/fieldcomments/unreviewed/count');
    if (count > 0) {
      badge.textContent = count > 99 ? '99+' : String(count);
      badge.style.display = '';
    } else {
      badge.style.display = 'none';
    }
  } catch {
    badge.style.display = 'none';
  }
}
