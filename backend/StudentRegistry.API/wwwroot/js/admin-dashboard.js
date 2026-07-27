// Admin Statistics Dashboard: KPI cards, per-certificate breakdowns, and the "other/external
// certificate" drill-down, all backed by a single live query (api/admin/dashboard/stats) with
// optional date-range + certificate filters. Depends on editor-shared.js (escapeHtml/fetchJson).

document.addEventListener('DOMContentLoaded', () => {
  loadDashboard();

  const applyBtn = document.getElementById('dashboard-apply-filters');
  const resetBtn = document.getElementById('dashboard-reset-filters');
  if (applyBtn) applyBtn.addEventListener('click', loadDashboard);
  if (resetBtn) {
    resetBtn.addEventListener('click', () => {
      document.getElementById('dashboard-start-date').value = '';
      document.getElementById('dashboard-end-date').value = '';
      document.getElementById('dashboard-cert-filter').value = '';
      loadDashboard();
    });
  }
});

async function loadDashboard() {
  const kpiGrid = document.getElementById('dashboard-kpi-grid');
  if (!kpiGrid) return; // dashboard markup not on this page

  kpiGrid.innerHTML = '<p class="field-hint">جارِ تحميل الإحصائيات...</p>';

  const startDate = document.getElementById('dashboard-start-date').value;
  const endDate = document.getElementById('dashboard-end-date').value;
  const certification = document.getElementById('dashboard-cert-filter').value;

  const params = new URLSearchParams();
  if (startDate) params.set('startDate', startDate);
  if (endDate) params.set('endDate', endDate);
  if (certification) params.set('certification', certification);

  try {
    const stats = await fetchJson(`/api/admin/dashboard/stats?${params.toString()}`);
    renderDashboard(stats);
  } catch (err) {
    kpiGrid.innerHTML = `<p class="field-hint">${escapeHtml(err.message || 'تعذر تحميل الإحصائيات.')}</p>`;
  }
}

function renderDashboard(stats) {
  populateCertificateFilter(stats.availableCertifications);
  renderKpiCards(stats);
  renderBarList(
    'dashboard-students-per-cert',
    (stats.studentsPerCertificate || []).map(x => ({ label: x.certification, count: x.count }))
  );
  renderBarList(
    'dashboard-comments-per-cert',
    (stats.commentsPerCertificate || []).map(x => ({ label: x.certification, count: x.count }))
  );
  renderOtherCertificatesTable(stats.otherCertificates);
}

function populateCertificateFilter(list) {
  const select = document.getElementById('dashboard-cert-filter');
  if (!select || !list) return;
  const currentValue = select.value;
  select.innerHTML = '<option value="">كل الشهادات</option>' +
    list.map(c => `<option value="${escapeHtml(c)}">${escapeHtml(c)}</option>`).join('');
  select.value = currentValue;
}

function renderKpiCards(stats) {
  const grid = document.getElementById('dashboard-kpi-grid');
  if (!grid) return;

  const cards = [
    { label: 'إجمالي الطلاب المسجلين', value: stats.totalStudents },
    {
      label: 'إجمالي التعليقات',
      value: stats.totalComments,
      sub: `${stats.reviewNotesCount} ملاحظة مراجعة + ${stats.fieldCommentsCount} تعليق محرر`
    },
    { label: 'طلاب الشهادات الخارجية ("أخرى")', value: stats.totalOtherCertificateStudents },
    { label: 'عدد الشهادات الخارجية المختلفة', value: stats.distinctOtherCertificates }
  ];

  grid.innerHTML = cards.map(c => `
    <div class="dashboard-kpi-card">
      <div class="dashboard-kpi-value">${(c.value ?? 0).toLocaleString('ar-EG')}</div>
      <div class="dashboard-kpi-label">${escapeHtml(c.label)}</div>
      ${c.sub ? `<div class="dashboard-kpi-sub">${escapeHtml(c.sub)}</div>` : ''}
    </div>`).join('');
}

function renderBarList(containerId, items) {
  const container = document.getElementById(containerId);
  if (!container) return;

  if (!items || items.length === 0) {
    container.innerHTML = '<p class="field-hint">لا توجد بيانات مطابقة.</p>';
    return;
  }

  const max = Math.max(...items.map(i => i.count), 1);
  container.innerHTML = `
    <div class="dashboard-bar-list">
      ${items.map(i => `
        <div class="dashboard-bar-row">
          <span class="dashboard-bar-label" title="${escapeHtml(i.label)}">${escapeHtml(i.label || '—')}</span>
          <div class="dashboard-bar-track">
            <div class="dashboard-bar-fill" style="width:${Math.max(2, (i.count / max) * 100)}%;"></div>
          </div>
          <span class="dashboard-bar-count">${i.count}</span>
        </div>`).join('')}
    </div>`;
}

function renderOtherCertificatesTable(items) {
  const container = document.getElementById('dashboard-other-certs');
  if (!container) return;

  if (!items || items.length === 0) {
    container.innerHTML = '<p class="field-hint">لا يوجد طلاب مسجلون بشهادات خارجية ("أخرى") ضمن الفلاتر الحالية.</p>';
    return;
  }

  container.innerHTML = `
    <div class="table-responsive">
      <table class="grades-table data-table">
        <thead><tr><th>اسم الشهادة</th><th>عدد الطلاب</th></tr></thead>
        <tbody>
          ${items.map(i => `<tr><td>${escapeHtml(i.certificateName || '—')}</td><td>${i.count}</td></tr>`).join('')}
        </tbody>
      </table>
    </div>`;
}
