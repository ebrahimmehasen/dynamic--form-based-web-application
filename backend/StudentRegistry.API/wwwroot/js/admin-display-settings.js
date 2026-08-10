// Admin "إعدادات العرض" tab (protected root admins only, server-gated in both the Razor page and the
// API). One toggle per certification type: turning it off hides the computed final score/total on
// the public registration success screen for that certification, without touching saved data. Every
// toggle requires confirmation + re-entering the account password (see AdminDisplaySettingsController).
// Depends on editor-shared.js (escapeHtml/fetchJson) + app-modal.js (showAppModal).

var adminDisplaySettingsState = {
  settings: []
};

async function loadDisplaySettings() {
  const body = document.getElementById('admin-display-settings-body');
  if (!body) return;
  body.innerHTML = '<p class="field-hint">جارِ التحميل...</p>';

  try {
    adminDisplaySettingsState.settings = await fetchJson('/api/admin/display-settings');
    renderDisplaySettings();
  } catch (err) {
    body.innerHTML = `<p class="field-hint">${escapeHtml(err.message || 'تعذر تحميل إعدادات العرض.')}</p>`;
  }
}

function renderDisplaySettings() {
  const body = document.getElementById('admin-display-settings-body');
  if (!body) return;

  const rows = adminDisplaySettingsState.settings.map(setting => {
    const statusLabel = setting.isResultVisible ? 'يظهر للطالب' : 'مخفي عن الطالب';
    const statusStyle = setting.isResultVisible
      ? 'background:#dcfce7; color:#15803d; border:1px solid #bbf7d0;'
      : 'background:#fee2e2; color:var(--danger-color); border:1px solid #fecaca;';
    const toggleLabel = setting.isResultVisible ? 'إخفاء النتيجة' : 'إظهار النتيجة';
    const updatedHint = setting.updatedAt
      ? `<span class="field-hint">آخر تعديل: ${formatDate(setting.updatedAt)} بواسطة ${escapeHtml(setting.updatedByUsername || '')}</span>`
      : '';
    return `
      <div class="dashboard-filter-actions" style="justify-content: space-between; align-items: center; border-bottom: 1px solid var(--border-color); padding: 12px 0;">
        <div>
          <strong>${escapeHtml(setting.label)}</strong>
          <span class="editor-pending-badge" style="margin-inline-start: 8px; ${statusStyle}">${statusLabel}</span>
          <br>${updatedHint}
        </div>
        <button type="button" class="btn btn-secondary admin-display-toggle-btn" style="width:auto;"
                data-cert-key="${escapeHtml(setting.certificationKey)}" data-next-visible="${!setting.isResultVisible}">
          ${toggleLabel}
        </button>
      </div>
    `;
  }).join('');

  body.innerHTML = `
    <div class="dashboard-widget-title" style="font-weight: 600; margin-bottom: 4px;">أنواع الشهادات</div>
    ${rows}
  `;

  body.querySelectorAll('.admin-display-toggle-btn').forEach(btn => {
    btn.addEventListener('click', () => onToggleDisplaySetting(btn.dataset.certKey, btn.dataset.nextVisible === 'true'));
  });
}

async function onToggleDisplaySetting(certificationKey, nextVisible) {
  const setting = adminDisplaySettingsState.settings.find(s => s.certificationKey === certificationKey);
  if (!setting) return;

  const actionLabel = nextVisible ? 'إظهار' : 'إخفاء';
  const { confirmed, value } = await showAppModal({
    title: `تأكيد ${actionLabel} النتيجة النهائية`,
    bodyHtml: `
      <p>هل أنت متأكد من ${actionLabel} النتيجة النهائية لشهادة "${escapeHtml(setting.label)}" أمام الطلاب؟</p>
      <label class="review-field-label" for="admin-display-settings-password">كلمة مرور الحساب لتأكيد التعديل</label>
      <input type="password" id="admin-display-settings-password" class="table-input" placeholder="أدخل كلمة المرور" autocomplete="current-password">
    `,
    confirmText: 'تأكيد',
    cancelText: 'إلغاء',
    variant: nextVisible ? 'primary' : 'warning',
    focusInputId: 'admin-display-settings-password'
  });

  if (!confirmed) return;

  const password = value || '';
  if (!password) {
    showToast('يجب إدخال كلمة مرور الحساب لتأكيد التعديل.', 'danger');
    return;
  }

  try {
    await fetchJson(`/api/admin/display-settings/${encodeURIComponent(certificationKey)}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ isVisible: nextVisible, password })
    });
    showToast(`تم ${actionLabel} النتيجة النهائية لشهادة "${setting.label}".`, 'success');
    await loadDisplaySettings();
  } catch (err) {
    showToast(err.message || 'تعذر حفظ التعديل.', 'danger');
  }
}
