// Student Records Editor page logic. Cloned from review-page.js as the required starting point,
// then extended with inline editing, the comment approve/write-own/dismiss workflow, an audit log
// tab, pagination, and the Request Deletion flow. Depends on editor-shared.js + app-modal.js.

var editorState = {
  currentStudentId: null,
  currentNationalId: null,
  editedFieldPaths: new Set(),      // fieldPath -> has at least one FieldEdit
  commentedFieldPaths: new Set(),   // fieldPath -> has at least one *unreviewed* FieldComment
  commentsByField: new Map(),       // fieldPath -> FieldCommentResponseDto[]
  pendingDeleteRequest: null,
  currentFieldPath: null,
  currentFieldSnapshot: null,
  activeEdit: null,                 // { originalEl, wrap, fieldPath } while an inline edit input is open
  currentQuery: '',
  currentPage: 1,
  pageSize: 50
};

document.addEventListener('DOMContentLoaded', () => {
  initEditorSearch();
  initEditorModals();
  loadStudents('', 1);
  loadUnreviewedBadge();

  // Deep link from the Notifications page ("/editor?studentId=123") — open that student's modal
  // directly once the page has loaded.
  const params = new URLSearchParams(window.location.search);
  const deepLinkStudentId = params.get('studentId');
  if (deepLinkStudentId) {
    openStudentDetail(parseInt(deepLinkStudentId, 10));
  }
});

// ---------- Search, table & pagination ----------

function initEditorSearch() {
  const input = document.getElementById('editor-search-input');
  if (!input) return;
  let debounceTimer = null;
  input.addEventListener('input', () => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => loadStudents(input.value.trim(), 1), 300);
  });

  const tbody = document.getElementById('editor-table-body');
  if (tbody) {
    tbody.addEventListener('click', (e) => {
      const row = e.target.closest('tr[data-student-id]');
      if (!row) return;
      openStudentDetail(parseInt(row.getAttribute('data-student-id'), 10));
    });
  }

  const pagination = document.getElementById('editor-pagination');
  if (pagination) {
    pagination.addEventListener('click', (e) => {
      const btn = e.target.closest('button[data-page]');
      if (!btn || btn.disabled) return;
      loadStudents(editorState.currentQuery, parseInt(btn.getAttribute('data-page'), 10));
    });
  }
}

async function loadStudents(query, page) {
  const tbody = document.getElementById('editor-table-body');
  const banner = document.getElementById('editor-results-banner');
  const emptyState = document.getElementById('editor-empty-state');
  if (!tbody) return;

  editorState.currentQuery = query;
  editorState.currentPage = page;

  try {
    const url = `/api/editor/students/search?q=${encodeURIComponent(query || '')}&page=${page}&pageSize=${editorState.pageSize}`;
    const result = await fetchJson(url);
    const students = result.items || [];
    tbody.innerHTML = students.map(renderStudentRow).join('');

    if (banner) {
      banner.textContent = result.totalCount > 0
        ? `عدد النتائج: ${result.totalCount}`
        : '';
    }
    if (emptyState) {
      emptyState.style.display = students.length === 0 ? 'block' : 'none';
    }

    renderPagination(result.totalCount, result.page, result.pageSize);
  } catch (err) {
    tbody.innerHTML = '';
    showToast(err.message || 'حدث خطأ أثناء البحث.', 'danger');
  }
}

function renderStudentRow(student) {
  // Blue left-border: any edit activity or a pending delete request. Yellow background: reuses
  // the same highlight as an individually-commented field, and is driven purely by whether ANY
  // field on this student has a comment — independent of the edit/pending-delete indicator.
  const flagged = student.hasFieldEdits || student.hasPendingDeleteRequest;
  const commented = student.hasFieldComments;
  const rowClasses = [
    flagged ? 'editor-row-flagged' : '',
    commented ? 'row-has-comment' : '',
    student.hasPendingReview ? 'row-pending-review' : ''
  ].filter(Boolean).join(' ');
  const pendingBadge = student.hasPendingDeleteRequest
    ? '<span class="editor-pending-badge">حذف معلق</span>'
    : '';
  const reviewBadge = student.hasPendingReview ? '<span class="pending-review-badge">قيد المراجعة</span>' : '';
  const eligibilityBadge = eligibilityBadgeHtml(student.eligibilityStatus);
  return `
    <tr data-student-id="${student.id}" class="${rowClasses}">
      <td>${displayValue(student.studentName)}</td>
      <td>${displayValue(student.studentNameEn)}</td>
      <td>${displayValue(student.nationalId)}</td>
      <td>${displayValue(student.phone)}</td>
      <td>${displayValue(student.email)}</td>
      <td>${displayValue(student.addressGov)}</td>
      <td>${displayValue(student.certification)}</td>
      <td>${displayValue(student.track)}</td>
      <td>${displayValue(student.graduationYear)}</td>
      <td>${formatDate(student.submittedAt)} ${pendingBadge}</td>
      <td class="col-pending-review">${reviewBadge}</td>
      <td class="col-eligibility">${eligibilityBadge}</td>
    </tr>`;
}

// "Eligible" -> green "مستوفي", "NotEligible" -> red "غير مستوفي", null/undefined -> empty (not yet
// confirmed by an Editor).
function eligibilityBadgeHtml(status) {
  if (status === 'Eligible') return '<span class="eligibility-badge eligible">مستوفي</span>';
  if (status === 'NotEligible') return '<span class="eligibility-badge not-eligible">غير مستوفي</span>';
  return '';
}

function renderPagination(totalCount, page, pageSize) {
  const el = document.getElementById('editor-pagination');
  if (!el) return;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  if (totalPages <= 1) {
    el.innerHTML = '';
    return;
  }
  el.innerHTML = `
    <button type="button" class="btn btn-secondary" data-page="${page - 1}" ${page <= 1 ? 'disabled' : ''}>السابق</button>
    <span class="editor-pagination-label">صفحة ${page} من ${totalPages}</span>
    <button type="button" class="btn btn-secondary" data-page="${page + 1}" ${page >= totalPages ? 'disabled' : ''}>التالي</button>`;
}

// ---------- Detail modal ----------

async function openStudentDetail(studentId) {
  try {
    const [student, edits, comments, deleteRequests] = await Promise.all([
      fetchJson(`/api/editor/students/${studentId}`),
      fetchJson(`/api/editor/fieldedits/student/${studentId}`),
      fetchJson(`/api/editor/fieldcomments/student/${studentId}`),
      fetchJson(`/api/editor/deleterequests/student/${studentId}`)
    ]);

    editorState.currentStudentId = studentId;
    editorState.currentNationalId = student.nationalId;
    editorState.editedFieldPaths = new Set(edits.map(e => e.fieldName));
    editorState.commentsByField = new Map();
    comments.forEach(c => {
      if (!editorState.commentsByField.has(c.fieldName)) editorState.commentsByField.set(c.fieldName, []);
      editorState.commentsByField.get(c.fieldName).push(c);
    });
    editorState.commentedFieldPaths = new Set(
      comments.filter(c => c.status === 'unreviewed').map(c => c.fieldName)
    );
    editorState.pendingDeleteRequest = deleteRequests.find(d => d.status === 'pending') || null;

    renderStudentDetail(student);
    renderDeleteRequestBadge();
    updatePendingReviewUi(student.hasPendingReview);
    updateEligibilityUi(student.eligibilityStatus);
    showDetailTab('details');

    const overlay = document.getElementById('editor-detail-overlay');
    if (overlay) overlay.style.display = 'flex';
  } catch (err) {
    showToast(err.message || 'حدث خطأ أثناء تحميل بيانات الطالب.', 'danger');
  }
}

async function refreshCurrentStudentDetail() {
  if (!editorState.currentStudentId) return;
  const studentId = editorState.currentStudentId;
  const [student, edits, comments] = await Promise.all([
    fetchJson(`/api/editor/students/${studentId}`),
    fetchJson(`/api/editor/fieldedits/student/${studentId}`),
    fetchJson(`/api/editor/fieldcomments/student/${studentId}`)
  ]);
  editorState.editedFieldPaths = new Set(edits.map(e => e.fieldName));
  editorState.commentsByField = new Map();
  comments.forEach(c => {
    if (!editorState.commentsByField.has(c.fieldName)) editorState.commentsByField.set(c.fieldName, []);
    editorState.commentsByField.get(c.fieldName).push(c);
  });
  editorState.commentedFieldPaths = new Set(
    comments.filter(c => c.status === 'unreviewed').map(c => c.fieldName)
  );
  renderStudentDetail(student);
  updatePendingReviewUi(student.hasPendingReview);
  updateEligibilityUi(student.eligibilityStatus);
  if (document.getElementById('editor-detail-tab-audit').classList.contains('active')) {
    loadAuditLog(studentId);
  }
}

function updatePendingReviewUi(hasPendingReview) {
  const badge = document.getElementById('editor-pending-review-badge');
  const btn = document.getElementById('editor-resolve-pending-review-btn');
  if (badge) badge.style.display = hasPendingReview ? 'inline-block' : 'none';
  if (btn) btn.style.display = hasPendingReview ? 'inline-flex' : 'none';
}

// Live-patches the table row's class and status badge in place — no full page/list reload needed
// so the table always reflects the latest state right after the modal action.
function setRowPendingReviewState(studentId, isPending) {
  const row = document.querySelector(`#editor-table-body tr[data-student-id="${studentId}"]`);
  if (!row) return;
  row.classList.toggle('row-pending-review', isPending);
  const statusCell = row.querySelector('.col-pending-review');
  if (statusCell) {
    statusCell.innerHTML = isPending ? '<span class="pending-review-badge">قيد المراجعة</span>' : '';
  }
}

// Same live-patch approach as setRowPendingReviewState, for the "مستوفي/غير مستوفي" column.
function setRowEligibilityState(studentId, status) {
  const row = document.querySelector(`#editor-table-body tr[data-student-id="${studentId}"]`);
  if (!row) return;
  const cell = row.querySelector('.col-eligibility');
  if (cell) cell.innerHTML = eligibilityBadgeHtml(status);
}

function updateEligibilityUi(status) {
  const badge = document.getElementById('editor-eligibility-badge');
  const eligibleBtn = document.getElementById('editor-mark-eligible-btn');
  const notEligibleBtn = document.getElementById('editor-mark-not-eligible-btn');

  if (badge) {
    if (status === 'Eligible') {
      badge.textContent = 'مستوفي';
      badge.className = 'eligibility-badge eligible';
      badge.style.display = 'inline-block';
    } else if (status === 'NotEligible') {
      badge.textContent = 'غير مستوفي';
      badge.className = 'eligibility-badge not-eligible';
      badge.style.display = 'inline-block';
    } else {
      badge.style.display = 'none';
    }
  }

  if (eligibleBtn) eligibleBtn.classList.toggle('active', status === 'Eligible');
  if (notEligibleBtn) notEligibleBtn.classList.toggle('active', status === 'NotEligible');
}

async function setCurrentStudentEligibility(status) {
  if (!editorState.currentStudentId) return;
  const eligibleBtn = document.getElementById('editor-mark-eligible-btn');
  const notEligibleBtn = document.getElementById('editor-mark-not-eligible-btn');
  if (eligibleBtn) eligibleBtn.disabled = true;
  if (notEligibleBtn) notEligibleBtn.disabled = true;
  try {
    await fetchJson(`/api/editor/students/${editorState.currentStudentId}/eligibility`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status })
    });
    updateEligibilityUi(status);
    setRowEligibilityState(editorState.currentStudentId, status);
    showToast(status === 'Eligible' ? 'تم تأكيد أن الطالب مستوفي.' : 'تم تأكيد أن الطالب غير مستوفي.', 'success');
  } catch (err) {
    showToast(err.message || 'حدث خطأ أثناء تحديث حالة الاستيفاء.', 'danger');
  } finally {
    if (eligibleBtn) eligibleBtn.disabled = false;
    if (notEligibleBtn) notEligibleBtn.disabled = false;
  }
}

async function resolveCurrentStudentPendingReview() {
  if (!editorState.currentStudentId) return;
  const btn = document.getElementById('editor-resolve-pending-review-btn');
  if (btn) btn.disabled = true;
  try {
    await fetchJson(`/api/editor/students/${editorState.currentStudentId}/resolve-pending-review`, { method: 'POST' });
    showToast('تم إزالة علامة قيد المراجعة.', 'success');
    updatePendingReviewUi(false);
    setRowPendingReviewState(editorState.currentStudentId, false);
  } catch (err) {
    showToast(err.message || 'حدث خطأ أثناء إزالة علامة قيد المراجعة.', 'danger');
  } finally {
    if (btn) btn.disabled = false;
  }
}

function renderStudentDetail(student) {
  const titleEl = document.getElementById('editor-detail-title');
  const bodyEl = document.getElementById('editor-detail-body');
  if (titleEl) titleEl.textContent = `${student.studentName || ''} — ${student.certification || ''}`;
  if (!bodyEl) return;

  const sections = [
    renderFieldSection('البيانات الشخصية', [
      ['Student.StudentName', 'اسم الطالب', student.studentName, 'text'],
      ['Student.StudentNameEn', 'الاسم بالإنجليزية', student.studentNameEn, 'text'],
      ['Student.NationalId', 'الرقم القومي', student.nationalId, 'text'],
      ['Student.Gender', 'النوع', student.gender, 'text'],
      ['Student.Phone', 'الهاتف', student.phone, 'text'],
      ['Student.Email', 'البريد الإلكتروني', student.email, 'text']
    ]),
    renderFieldSection('بيانات ولي الأمر', [
      ['Student.GuardianName', 'اسم ولي الأمر', student.guardianName, 'text'],
      ['Student.GuardianNationalId', 'الرقم القومي لولي الأمر', student.guardianNationalId, 'text'],
      ['Student.GuardianPhone', 'هاتف ولي الأمر', student.guardianPhone, 'text'],
      ['Student.GuardianRelation', 'صلة القرابة', student.guardianRelation, 'text'],
      ['Student.GuardianOccupation', 'مهنة ولي الأمر', student.guardianOccupation, 'text']
    ]),
    renderFieldSection('العنوان', [
      ['Student.AddressGov', 'المحافظة', student.addressGov, 'text'],
      ['Student.AddressCenter', 'المركز/القسم', student.addressCenter, 'text'],
      ['Student.AddressVillage', 'القرية', student.addressVillage, 'text'],
      ['Student.AddressStreet', 'الشارع', student.addressStreet, 'text'],
      ['Student.AddressBuilding', 'رقم المبنى', student.addressBuilding, 'text'],
      ['Student.AddressFloor', 'الطابق', student.addressFloor, 'text']
    ]),
    renderFieldSection('البيانات الأكاديمية', [
      ['Student.Certification', 'الشهادة', student.certification, 'text'],
      ['Student.Track', 'المسار', student.track, 'text'],
      ['Student.GraduationYear', 'سنة التخرج', student.graduationYear, 'number'],
      ['Student.WishCollege', 'الكلية المرغوبة', student.wishCollege, 'text'],
      ['Student.WishProgram', 'البرنامج المرغوب', student.wishProgram, 'text']
    ]),
    renderUploadedFilesSection(student),
    renderCertificateSection(student)
  ];

  bodyEl.innerHTML = sections.join('');
  wireEditableFields(bodyEl);
}

function renderFieldSection(title, fields) {
  const items = fields.map(([fieldPath, label, value, kind]) => renderFieldItem(fieldPath, label, value, kind)).join('');
  return `
    <div class="review-section">
      <div class="review-section-title">${escapeHtml(title)}</div>
      <div class="review-field-grid">${items}</div>
    </div>`;
}

function renderFieldItem(fieldPath, label, value, kind) {
  return `
    <div class="review-field-item">
      <span class="review-field-item-label">${escapeHtml(label)}</span>
      ${renderEditableFieldHtml(fieldPath, label, value, kind)}
    </div>`;
}

function renderEditableFieldHtml(fieldPath, label, value, kind) {
  const edited = editorState.editedFieldPaths.has(fieldPath);
  const commented = editorState.commentedFieldPaths.has(fieldPath);
  const cls = ['review-clickable-field', 'editor-editable-field'];
  if (edited) cls.push('editor-field-edited');
  if (commented) cls.push('editor-field-commented');
  const rawValue = value === null || value === undefined ? '' : String(value);
  return `
    <div class="editor-field-wrap">
      <span class="${cls.join(' ')}"
            data-field-path="${escapeHtml(fieldPath)}"
            data-field-label="${escapeHtml(label)}"
            data-field-value="${escapeHtml(rawValue)}"
            data-value-kind="${kind || 'text'}">${displayValue(value)}</span>
      <button type="button" class="editor-comment-icon" title="إضافة تعليق" aria-label="إضافة تعليق"
              data-field-path="${escapeHtml(fieldPath)}" data-field-label="${escapeHtml(label)}"
              data-field-value="${escapeHtml(rawValue)}">💬</button>
    </div>`;
}

function renderUploadedFilesSection(student) {
  const photoBlock = student.photoPath
    ? `<div class="review-photo-frame"><img src="/${escapeHtml(student.photoPath)}" alt="الصورة الشخصية"></div>`
    : '<p class="field-hint">لا توجد صورة مرفوعة.</p>';

  return `
    <div class="review-section">
      <div class="review-section-title">الملفات المرفوعة</div>
      <div class="review-field-item">
        <span class="review-field-item-label">الصورة الشخصية (عرض فقط)</span>
        ${photoBlock}
      </div>
    </div>`;
}

// ---------- Certificate details dispatch (dynamic per certification type) ----------

function renderCertificateSection(student) {
  let body = '<p class="field-hint">لا توجد بيانات شهادة مسجلة.</p>';

  if (student.saudiTotals) {
    body = renderTotalsGrid('SaudiTotals', student.saudiTotals, [
      ['YearsCount', 'عدد سنوات الدراسة', 'text'], ['SchoolPercentage', 'نسبة المدرسة', 'number'],
      ['AptitudeScore', 'درجة القدرات', 'number'], ['FinalPercentage', 'النسبة النهائية', 'number'],
      ['EquivalentTotal', 'المجموع الاعتباري', 'number']
    ]) + renderGradeTable('SaudiGrades', student.saudiGrades, [
      ['YearLabel', 'السنة', 'text'], ['SubjectName', 'المادة', 'text'],
      ['Achieved', 'المتحصل', 'number'], ['Weighted', 'الموزون', 'number'], ['Coefficient', 'المعامل', 'number']
    ]);
  } else if (student.igGrades) {
    body = renderTotalsGrid('IgGrades', student.igGrades, [
      ['IgProgram', 'البرنامج', 'text'], ['Factor', 'المعامل', 'number'],
      ['SportsBonus', 'مكافأة رياضية', 'number'], ['ScorePercentage', 'نسبة الدرجة', 'number'],
      ['GovernmentScore', 'الدرجة الحكومية', 'number']
    ]) + renderGradeTable('IgGradeCounts', student.igGrades.gradeCounts, [
      ['GradeType', 'نوع الدرجة', 'text'], ['Grade', 'الدرجة', 'text'], ['Count', 'العدد', 'number']
    ]);
  } else if (student.kuwaitiTotals) {
    body = renderTotalsGrid('KuwaitiTotals', student.kuwaitiTotals, [
      ['YearsCount', 'عدد سنوات الدراسة', 'text'], ['Grade10Percentage', 'نسبة الصف العاشر', 'number'],
      ['Grade11Percentage', 'نسبة الصف الحادي عشر', 'number'], ['Grade12Percentage', 'نسبة الصف الثاني عشر', 'number'],
      ['FinalPercentage', 'النسبة النهائية', 'number'], ['EquivalentTotal', 'المجموع الاعتباري', 'number'],
      ['HasSecondAttempt', 'محاولة ثانية', 'bool']
    ]) + renderGradeTable('StandardGrades', student.kuwaitiGrades, standardGradeColumns());
  } else if (student.qatariTotals) {
    body = renderSingleYearCert('QatariTotals', student.qatariTotals, student.qatariGrades);
  } else if (student.omaniTotals) {
    body = renderSingleYearCert('OmaniTotals', student.omaniTotals, student.omaniGrades);
  } else if (student.yemeniTotals) {
    body = renderSingleYearCert('YemeniTotals', student.yemeniTotals, student.yemeniGrades);
  } else if (student.bahrainiTotals) {
    body = renderTotalsGrid('BahrainiTotals', student.bahrainiTotals, [
      ['Track', 'المسار', 'text'], ['FinalTotal', 'المجموع النهائي', 'number'],
      ['TotalMax', 'الحد الأقصى', 'number'], ['Percentage', 'النسبة المئوية', 'number'],
      ['EquivalentTotal', 'المجموع الاعتباري', 'number']
    ]) + renderGradeTable('StandardGrades', student.bahrainiGrades, standardGradeColumns());
  } else if (student.palestinianTotals) {
    body = renderTotalsGrid('PalestinianTotals', student.palestinianTotals, [
      ['Percentage', 'النسبة المئوية', 'number'], ['EquivalentTotal', 'المجموع الاعتباري', 'number'],
      ['Branch', 'الفرع', 'text']
    ]);
  } else if (student.otherTotals) {
    body = renderTotalsGrid('OtherTotals', student.otherTotals, [
      ['CertificateName', 'اسم الشهادة', 'text'], ['Percentage', 'النسبة المئوية', 'number']
    ]);
  } else if (student.egyptianTotals) {
    body = renderTotalsGrid('EgyptianTotals', student.egyptianTotals, [
      ['Track', 'المسار', 'text'], ['SubjectSystem', 'نظام المواد', 'text'],
      ['FinalTotal', 'المجموع النهائي', 'number'], ['Denominator', 'المجموع الكلي', 'number'],
      ['Percentage', 'النسبة المئوية', 'number']
    ]) + renderGradeTable('StandardGrades', student.egyptianGrades, standardGradeColumns());
  } else if (student.azharTotals) {
    body = renderTotalsGrid('AzharTotals', student.azharTotals, [
      ['Section', 'القسم', 'text'], ['FinalTotal', 'المجموع النهائي', 'number'],
      ['Denominator', 'المجموع الكلي', 'number'], ['Percentage', 'النسبة المئوية', 'number'],
      ['EquivalentTotal', 'المجموع الاعتباري', 'number']
    ]) + renderGradeTable('StandardGrades', student.azharGrades, standardGradeColumns());
  } else if (student.emiratiTotals) {
    body = renderSingleYearCert('EmiratiTotals', student.emiratiTotals, student.emiratiGrades);
  } else if (student.americanDiplomaTotals) {
    body = renderTotalsGrid('AmericanDiplomaTotals', student.americanDiplomaTotals, [
      ['AverageScore', 'متوسط الدرجات', 'number'], ['BasePercentage', 'النسبة الأساسية', 'number'],
      ['TestType1', 'نوع اختبار المستوى الأول', 'text'], ['SatI', 'SAT I', 'number'],
      ['ActComposite', 'ACT الكلي', 'number'], ['TestType2', 'نوع اختبار المستوى الثاني', 'text'],
      ['SatII', 'SAT II', 'number'], ['ActMath', 'ACT رياضيات', 'number'],
      ['SatIISubject1', 'مادة SAT II الأولى', 'text'], ['SatIISubject2', 'مادة SAT II الثانية', 'text'],
      ['StudiedAdvancedMath', 'درس رياضيات متقدمة', 'bool']
    ]) + renderGradeTable('StandardGrades', student.americanDiplomaGrades, standardGradeColumns());
  }

  const hasCertData = body !== '<p class="field-hint">لا توجد بيانات شهادة مسجلة.</p>';
  const recalcSection = hasCertData ? `
    <div class="editor-recalc-bar" id="editor-recalc-bar">
      <button type="button" class="btn btn-secondary" id="editor-edit-grades-btn">تعديل درجات الطالب</button>
      <div class="editor-recalc-panel" id="editor-recalc-panel" style="display:none;">
        <p class="field-hint">عدّل أي درجة لأي مادة أعلاه بالضغط عليها، ثم اضغط الزر التالي لإعادة حساب النتيجة النهائية بنفس طريقة الحساب المستخدمة في الصفحة الرئيسية.</p>
        <button type="button" class="btn btn-primary" id="editor-recalculate-btn">إعادة حساب النتيجة النهائية</button>
      </div>
    </div>` : '';

  return `
    <div class="review-section">
      <div class="review-section-title">تفاصيل الشهادة</div>
      ${body}
      ${recalcSection}
    </div>`;
}

function renderSingleYearCert(totalsGroup, totals, grades) {
  return renderTotalsGrid(totalsGroup, totals, [
    ['FinalTotal', 'المجموع النهائي', 'number'], ['Percentage', 'النسبة المئوية', 'number'],
    ['EquivalentTotal', 'المجموع الاعتباري', 'number']
  ]) + renderGradeTable('StandardGrades', grades, [
    ['SubjectName', 'المادة', 'text'], ['Mark', 'الدرجة', 'number']
  ]);
}

function standardGradeColumns() {
  return [
    ['SubjectName', 'المادة', 'text'], ['Grade', 'الدرجة', 'number'],
    ['MaxMark', 'الحد الأقصى', 'number'], ['WeightedPercentage', 'النسبة الموزونة', 'number']
  ];
}

// totalsFields: [[prop, label, kind], ...] read off `totals` (camelCase from JSON).
function renderTotalsGrid(entityGroup, totals, totalsFields) {
  const items = totalsFields.map(([prop, label, kind]) => {
    const jsProp = prop.charAt(0).toLowerCase() + prop.slice(1);
    const value = totals[jsProp];
    const fieldPath = formatFieldPath(entityGroup, null, prop);
    const displayed = kind === 'bool' ? (value ? 'نعم' : 'لا') : value;
    return renderFieldItem(fieldPath, label, displayed, kind);
  }).join('');
  return `<div class="review-field-grid">${items}</div>`;
}

// rows: array of grade DTOs (each now carries its own `id`, see StudentDto.cs). columns: [[prop, label, kind], ...]
function renderGradeTable(entityGroup, rows, columns) {
  if (!rows || rows.length === 0) {
    return '<p class="field-hint">لا توجد صفوف درجات.</p>';
  }
  const head = columns.map(([, label]) => `<th>${escapeHtml(label)}</th>`).join('');
  const body = rows.map(row => {
    const cells = columns.map(([prop, label, kind]) => {
      const jsProp = prop.charAt(0).toLowerCase() + prop.slice(1);
      const fieldPath = formatFieldPath(entityGroup, row.id, prop);
      return `<td>${renderEditableFieldHtml(fieldPath, label, row[jsProp], kind)}</td>`;
    }).join('');
    return `<tr>${cells}</tr>`;
  }).join('');
  return `
    <div class="table-responsive" style="margin-top:0.85rem;">
      <table class="grades-table review-cert-table"><thead><tr>${head}</tr></thead><tbody>${body}</tbody></table>
    </div>`;
}

// ---------- Inline click-to-edit ----------

function wireEditableFields(container) {
  container.querySelectorAll('.editor-editable-field').forEach(el => {
    el.addEventListener('click', () => startInlineEdit(el));
  });
  container.querySelectorAll('.editor-comment-icon').forEach(el => {
    el.addEventListener('click', (e) => {
      e.stopPropagation();
      openCommentModal({
        fieldPath: el.getAttribute('data-field-path'),
        label: el.getAttribute('data-field-label'),
        value: el.getAttribute('data-field-value')
      });
    });
  });

  const editGradesBtn = container.querySelector('#editor-edit-grades-btn');
  if (editGradesBtn) {
    editGradesBtn.addEventListener('click', () => {
      const panel = container.querySelector('#editor-recalc-panel');
      if (panel) panel.style.display = panel.style.display === 'none' ? 'block' : 'none';
    });
  }
  const recalcBtn = container.querySelector('#editor-recalculate-btn');
  if (recalcBtn) {
    recalcBtn.addEventListener('click', recalculateCurrentStudentGrades);
  }
}

// Same PDF the student downloads right after registering (GET /api/students/{id}/export/pdf) —
// reused as-is here so the Editor always gets the current, up-to-date record (including any edits
// or recalculations applied since registration), not a stale copy from the student's own download.
function downloadCurrentStudentPdf() {
  if (!editorState.currentStudentId || !editorState.currentNationalId) return;
  const url = `/api/students/${editorState.currentStudentId}/export/pdf?nationalId=${encodeURIComponent(editorState.currentNationalId)}`;
  window.location.href = url;
}

// Triggers the server-side recalculation (POST /api/editor/students/{id}/recalculate), which
// re-derives the certificate's totals from whatever raw grade rows currently exist — the same rows
// the inline edits above just changed — using the exact same formula as the main registration page,
// and logs every changed total field in the edits sheet under this editor's own username.
async function recalculateCurrentStudentGrades() {
  if (!editorState.currentStudentId) return;
  const btn = document.getElementById('editor-recalculate-btn');
  const originalText = btn ? btn.textContent : '';
  if (btn) {
    btn.disabled = true;
    btn.textContent = 'جاري إعادة الحساب...';
  }
  try {
    await fetchJson(`/api/editor/students/${editorState.currentStudentId}/recalculate`, { method: 'POST' });
    showToast('تم إعادة حساب النتيجة النهائية بنجاح.', 'success');
    await refreshCurrentStudentDetail();
  } catch (err) {
    showToast(err.message || 'حدث خطأ أثناء إعادة الحساب.', 'danger');
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.textContent = originalText;
    }
  }
}

// Editing any of these raw underlying grade fields changes what the certificate's totals SHOULD
// be, so saving one of them auto-triggers a recalculation (same math as the main registration page)
// instead of requiring a separate manual click — see recalculateCurrentStudentGrades(). Totals groups
// themselves are deliberately excluded: overwriting a Totals field the editor just typed with a fresh
// recalculation would silently discard their edit.
const GRADE_AFFECTING_GROUPS = new Set(['SaudiGrades', 'StandardGrades', 'IgGradeCounts', 'IgGrades']);

function startInlineEdit(el) {
  if (editorState.activeEdit) return; // one inline edit at a time

  const fieldPath = el.getAttribute('data-field-path');
  const kind = el.getAttribute('data-value-kind') || 'text';
  const currentValue = el.getAttribute('data-field-value') || '';

  const wrap = document.createElement('span');
  wrap.className = 'editor-inline-edit';

  let inputHtml;
  if (kind === 'bool') {
    const isTrue = currentValue === 'true' || currentValue === 'True';
    inputHtml = `<select class="table-input editor-inline-input">
        <option value="true" ${isTrue ? 'selected' : ''}>نعم</option>
        <option value="false" ${!isTrue ? 'selected' : ''}>لا</option>
      </select>`;
  } else if (kind === 'date') {
    inputHtml = `<input type="date" class="table-input editor-inline-input" value="${escapeHtml(currentValue.substring(0, 10))}">`;
  } else if (kind === 'number') {
    inputHtml = `<input type="number" step="any" class="table-input editor-inline-input" value="${escapeHtml(currentValue)}">`;
  } else {
    inputHtml = `<input type="text" class="table-input editor-inline-input" value="${escapeHtml(currentValue)}">`;
  }

  wrap.innerHTML = `${inputHtml}
    <button type="button" class="editor-inline-save" title="حفظ" aria-label="حفظ">✓</button>
    <button type="button" class="editor-inline-cancel" title="إلغاء" aria-label="إلغاء">✗</button>`;

  el.replaceWith(wrap);
  const inputEl = wrap.querySelector('.editor-inline-input');
  inputEl.focus();
  if (inputEl.select) inputEl.select();

  editorState.activeEdit = { originalEl: el, wrap, fieldPath };

  const cancel = () => {
    wrap.replaceWith(el);
    editorState.activeEdit = null;
  };

  const save = async () => {
    const newValue = inputEl.value;
    try {
      const { entityGroup, entityRowId, propertyName } = parseFieldPath(fieldPath);
      await applyFieldEdit({ studentId: editorState.currentStudentId, entityGroup, entityRowId, propertyName, newValue });

      editorState.editedFieldPaths.add(fieldPath);
      editorState.activeEdit = null;

      if (GRADE_AFFECTING_GROUPS.has(entityGroup)) {
        // A raw grade/subject value changed — the certificate's totals are now stale, so recalculate
        // immediately instead of leaving it to a separate manual step. This re-fetches and re-renders
        // the whole detail body (including this field), so just restore the DOM position first.
        wrap.replaceWith(el);
        showToast('تم حفظ التعديل، جاري إعادة حساب النتيجة النهائية...', 'success');
        await recalculateCurrentStudentGrades();
        return;
      }

      el.setAttribute('data-field-value', newValue);
      el.textContent = kind === 'bool' ? (newValue === 'true' ? 'نعم' : 'لا') : (newValue === '' ? '—' : newValue);
      el.classList.add('editor-field-edited');
      wrap.replaceWith(el);
      // Keep the comment icon's snapshot in sync so a comment added right after an edit captures
      // the new value, not the stale pre-edit one.
      const icon = el.parentElement ? el.parentElement.querySelector('.editor-comment-icon') : null;
      if (icon) icon.setAttribute('data-field-value', newValue);
      showToast('تم حفظ التعديل بنجاح.', 'success');
    } catch (err) {
      showToast(err.message || 'تعذر حفظ التعديل.', 'danger');
    }
  };

  wrap.querySelector('.editor-inline-save').addEventListener('click', save);
  wrap.querySelector('.editor-inline-cancel').addEventListener('click', cancel);
  inputEl.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') { e.preventDefault(); save(); }
    if (e.key === 'Escape') { e.preventDefault(); cancel(); }
  });
}

function getLiveFieldValue(fieldPath) {
  const el = document.querySelector(`.editor-editable-field[data-field-path="${cssEscape(fieldPath)}"]`);
  return el ? el.getAttribute('data-field-value') : null;
}

// ---------- Field-comment (small) modal ----------

function openCommentModal({ fieldPath, label, value }) {
  editorState.currentFieldPath = fieldPath;
  editorState.currentFieldSnapshot = value;

  const titleEl = document.getElementById('editor-field-title');
  const currentEl = document.getElementById('editor-field-current');
  const historyWrap = document.getElementById('editor-field-history-wrap');
  const historyEl = document.getElementById('editor-field-history');
  const input = document.getElementById('editor-field-comment-input');

  if (titleEl) titleEl.textContent = label || 'تعليق على الحقل';
  if (currentEl) currentEl.textContent = value || '—';
  if (input) input.value = '';

  const comments = editorState.commentsByField.get(fieldPath) || [];
  if (comments.length > 0 && historyWrap && historyEl) {
    historyWrap.style.display = '';
    historyEl.innerHTML = comments.map(renderCommentHistoryItem).join('');
    wireCommentActions(historyEl);
  } else if (historyWrap) {
    historyWrap.style.display = 'none';
  }

  const overlay = document.getElementById('editor-field-overlay');
  if (overlay) overlay.style.display = 'flex';
  if (input) setTimeout(() => input.focus(), 50);
}

function renderCommentHistoryItem(c) {
  const actions = c.status === 'unreviewed' ? `
    <div class="editor-comment-actions">
      <button type="button" class="btn btn-secondary editor-comment-approve" data-comment-id="${c.id}">اعتماد كتعديل</button>
      <button type="button" class="btn btn-secondary editor-comment-write" data-comment-id="${c.id}">كتابة تعديل خاص</button>
      <button type="button" class="btn btn-secondary editor-comment-dismiss" data-comment-id="${c.id}">تجاهل</button>
    </div>
    <div class="editor-comment-edit-area" id="editor-comment-edit-${c.id}"></div>` : '';
  return `
    <div class="review-field-history-item">
      <div class="review-field-history-meta">${escapeHtml(c.author)} — ${formatDate(c.createdAt)} — ${escapeHtml(statusLabel(c.status))}</div>
      <div>${escapeHtml(c.commentText)}</div>
      ${actions}
    </div>`;
}

function wireCommentActions(container) {
  container.querySelectorAll('.editor-comment-approve').forEach(btn => {
    btn.addEventListener('click', () => openCommentEditArea(btn.getAttribute('data-comment-id'), 'from_comment'));
  });
  container.querySelectorAll('.editor-comment-write').forEach(btn => {
    btn.addEventListener('click', () => openCommentEditArea(btn.getAttribute('data-comment-id'), 'manual'));
  });
  container.querySelectorAll('.editor-comment-dismiss').forEach(btn => {
    btn.addEventListener('click', () => handleDismissComment(btn.getAttribute('data-comment-id')));
  });
}

function openCommentEditArea(commentId, source) {
  const area = document.getElementById(`editor-comment-edit-${commentId}`);
  if (!area) return;
  const fieldPath = editorState.currentFieldPath;
  const currentLiveValue = getLiveFieldValue(fieldPath) ?? editorState.currentFieldSnapshot ?? '';

  area.innerHTML = `
    <label class="review-field-label">${source === 'from_comment' ? 'القيمة المعتمدة' : 'التعديل الخاص بك'}</label>
    <textarea class="table-input editor-comment-edit-input" rows="2">${escapeHtml(currentLiveValue)}</textarea>
    <div class="btn-group-row" style="margin-top:0.5rem;">
      <button type="button" class="btn btn-primary editor-comment-edit-save" style="flex:1;">حفظ</button>
      <button type="button" class="btn btn-secondary editor-comment-edit-cancel" style="flex:1;">إلغاء</button>
    </div>`;

  const input = area.querySelector('.editor-comment-edit-input');
  input.focus();

  area.querySelector('.editor-comment-edit-cancel').addEventListener('click', () => { area.innerHTML = ''; });
  area.querySelector('.editor-comment-edit-save').addEventListener('click', async () => {
    try {
      const { entityGroup, entityRowId, propertyName } = parseFieldPath(fieldPath);
      await applyFieldEdit({
        studentId: editorState.currentStudentId, entityGroup, entityRowId, propertyName,
        newValue: input.value, source, triggeringCommentId: parseInt(commentId, 10)
      });
      showToast('تم حفظ التعديل واعتماد التعليق.', 'success');
      closeFieldModal();
      await refreshCurrentStudentDetail();
      loadStudents(editorState.currentQuery, editorState.currentPage);
      loadUnreviewedBadge();
    } catch (err) {
      showToast(err.message || 'تعذر حفظ التعديل.', 'danger');
    }
  });
}

async function handleDismissComment(commentId) {
  try {
    await dismissFieldComment(commentId);
    showToast('تم تجاهل التعليق.', 'success');
    closeFieldModal();
    await refreshCurrentStudentDetail();
    loadUnreviewedBadge();
  } catch (err) {
    showToast(err.message || 'تعذر تجاهل التعليق.', 'danger');
  }
}

async function saveNewComment() {
  const input = document.getElementById('editor-field-comment-input');
  const text = input ? input.value.trim() : '';
  if (!text) {
    showToast('يرجى كتابة تعليق قبل الحفظ.', 'danger');
    return;
  }
  if (!editorState.currentStudentId || !editorState.currentFieldPath) return;

  try {
    const { entityGroup, entityRowId, propertyName } = parseFieldPath(editorState.currentFieldPath);
    const comment = await addFieldComment({
      studentId: editorState.currentStudentId, entityGroup, entityRowId, propertyName,
      fieldSnapshot: editorState.currentFieldSnapshot, commentText: text
    });

    const fieldPath = editorState.currentFieldPath;
    if (!editorState.commentsByField.has(fieldPath)) editorState.commentsByField.set(fieldPath, []);
    editorState.commentsByField.get(fieldPath).unshift(comment);
    editorState.commentedFieldPaths.add(fieldPath);
    document.querySelectorAll(`.editor-editable-field[data-field-path="${cssEscape(fieldPath)}"]`)
      .forEach(el => el.classList.add('editor-field-commented'));

    showToast('تم حفظ التعليق بنجاح.', 'success');
    closeFieldModal();
    loadUnreviewedBadge();
  } catch (err) {
    showToast(err.message || 'تعذر حفظ التعليق.', 'danger');
  }
}

// ---------- Audit log tab ----------

function showDetailTab(tab) {
  const detailsTab = document.getElementById('editor-detail-tab-details');
  const auditTab = document.getElementById('editor-detail-tab-audit');
  const detailsBtn = document.getElementById('editor-tab-btn-details');
  const auditBtn = document.getElementById('editor-tab-btn-audit');
  if (!detailsTab || !auditTab) return;

  const showAudit = tab === 'audit';
  detailsTab.style.display = showAudit ? 'none' : '';
  auditTab.style.display = showAudit ? '' : 'none';
  detailsTab.classList.toggle('active', !showAudit);
  auditTab.classList.toggle('active', showAudit);
  if (detailsBtn) detailsBtn.classList.toggle('editor-tab-active', !showAudit);
  if (auditBtn) auditBtn.classList.toggle('editor-tab-active', showAudit);

  if (showAudit && editorState.currentStudentId) {
    loadAuditLog(editorState.currentStudentId);
  }
}

let auditLogCache = [];

async function loadAuditLog(studentId) {
  const body = document.getElementById('editor-audit-body');
  if (!body) return;
  try {
    auditLogCache = await fetchJson(`/api/editor/students/${studentId}/audit`);
    renderAuditLog();
  } catch (err) {
    body.innerHTML = `<p class="field-hint">${escapeHtml(err.message || 'تعذر تحميل سجل التعديلات.')}</p>`;
  }
}

function renderAuditLog() {
  const body = document.getElementById('editor-audit-body');
  const typeFilter = document.getElementById('editor-audit-type-filter');
  const fieldFilter = document.getElementById('editor-audit-field-filter');
  if (!body) return;

  const typeVal = typeFilter ? typeFilter.value : '';
  const fieldVal = fieldFilter ? fieldFilter.value.trim().toLowerCase() : '';

  const rows = auditLogCache.filter(e =>
    (!typeVal || e.type === typeVal) &&
    (!fieldVal || e.fieldName.toLowerCase().includes(fieldVal))
  );

  if (rows.length === 0) {
    body.innerHTML = '<p class="field-hint">لا توجد سجلات مطابقة.</p>';
    return;
  }

  const typeLabels = { Edit: 'تعديل', Comment: 'تعليق', DeleteRequest: 'طلب حذف' };

  body.innerHTML = `
    <div class="table-responsive">
      <table class="grades-table data-table">
        <thead>
          <tr>
            <th>النوع</th><th>الحقل</th><th>القيمة القديمة</th><th>القيمة الجديدة</th>
            <th>المستخدم</th><th>التاريخ</th><th>ملاحظة</th><th>الحالة</th>
          </tr>
        </thead>
        <tbody>
          ${rows.map(e => `
            <tr>
              <td>${escapeHtml(typeLabels[e.type] || e.type)}</td>
              <td>${escapeHtml(e.fieldName)}</td>
              <td>${displayValue(e.oldValue)}</td>
              <td>${displayValue(e.newValue)}</td>
              <td>${escapeHtml(e.actor)}</td>
              <td>${formatDate(e.timestamp)}</td>
              <td>${displayValue(e.note)}</td>
              <td>${escapeHtml(statusLabel(e.status))}</td>
            </tr>`).join('')}
        </tbody>
      </table>
    </div>`;
}

// ---------- Request Deletion ----------

function renderDeleteRequestBadge() {
  const badge = document.getElementById('editor-delete-badge');
  if (!badge) return;
  badge.style.display = editorState.pendingDeleteRequest ? '' : 'none';
}

async function handleRequestDeletion() {
  if (editorState.pendingDeleteRequest) {
    showToast('يوجد بالفعل طلب حذف قيد المراجعة لهذا الطالب.', 'danger');
    return;
  }

  const { confirmed, value } = await showAppModal({
    title: 'طلب حذف الطالب',
    bodyHtml: `<p>سيتم إرسال طلب حذف إلى المسؤول (Admin) للمراجعة. لن يتم حذف أي بيانات الآن.</p>
      <label class="review-field-label" for="editor-delete-reason">سبب الحذف (اختياري)</label>
      <textarea id="editor-delete-reason" class="table-input review-field-textarea" rows="3"></textarea>`,
    confirmText: 'إرسال الطلب',
    cancelText: 'إلغاء',
    variant: 'danger',
    focusInputId: 'editor-delete-reason'
  });
  if (!confirmed) return;

  try {
    const request = await fetchJson('/api/editor/deleterequests', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ studentId: editorState.currentStudentId, reason: value || null })
    });
    editorState.pendingDeleteRequest = request;
    renderDeleteRequestBadge();
    loadStudents(editorState.currentQuery, editorState.currentPage);
    showToast('تم إرسال طلب الحذف — بانتظار مراجعة المسؤول.', 'success');
  } catch (err) {
    showToast(err.message || 'تعذر إرسال طلب الحذف.', 'danger');
  }
}

// ---------- Modal wiring / lifecycle ----------

function initEditorModals() {
  const detailClose = document.getElementById('editor-detail-close');
  const detailOverlay = document.getElementById('editor-detail-overlay');
  if (detailClose) detailClose.addEventListener('click', requestCloseDetailModal);
  if (detailOverlay) {
    detailOverlay.addEventListener('click', (e) => {
      if (e.target === detailOverlay) requestCloseDetailModal();
    });
  }

  const tabDetailsBtn = document.getElementById('editor-tab-btn-details');
  const tabAuditBtn = document.getElementById('editor-tab-btn-audit');
  if (tabDetailsBtn) tabDetailsBtn.addEventListener('click', () => showDetailTab('details'));
  if (tabAuditBtn) tabAuditBtn.addEventListener('click', () => showDetailTab('audit'));

  const auditTypeFilter = document.getElementById('editor-audit-type-filter');
  const auditFieldFilter = document.getElementById('editor-audit-field-filter');
  if (auditTypeFilter) auditTypeFilter.addEventListener('change', renderAuditLog);
  if (auditFieldFilter) auditFieldFilter.addEventListener('input', renderAuditLog);

  const requestDeleteBtn = document.getElementById('editor-request-delete-btn');
  if (requestDeleteBtn) requestDeleteBtn.addEventListener('click', handleRequestDeletion);

  const downloadPdfBtn = document.getElementById('editor-download-pdf-btn');
  if (downloadPdfBtn) downloadPdfBtn.addEventListener('click', downloadCurrentStudentPdf);

  const resolvePendingReviewBtn = document.getElementById('editor-resolve-pending-review-btn');
  if (resolvePendingReviewBtn) resolvePendingReviewBtn.addEventListener('click', resolveCurrentStudentPendingReview);

  const markEligibleBtn = document.getElementById('editor-mark-eligible-btn');
  if (markEligibleBtn) markEligibleBtn.addEventListener('click', () => setCurrentStudentEligibility('Eligible'));
  const markNotEligibleBtn = document.getElementById('editor-mark-not-eligible-btn');
  if (markNotEligibleBtn) markNotEligibleBtn.addEventListener('click', () => setCurrentStudentEligibility('NotEligible'));

  const fieldClose = document.getElementById('editor-field-close');
  const fieldCancel = document.getElementById('editor-field-cancel');
  const fieldOverlay = document.getElementById('editor-field-overlay');
  const fieldSave = document.getElementById('editor-field-save');
  if (fieldClose) fieldClose.addEventListener('click', closeFieldModal);
  if (fieldCancel) fieldCancel.addEventListener('click', closeFieldModal);
  if (fieldOverlay) {
    fieldOverlay.addEventListener('click', (e) => {
      if (e.target === fieldOverlay) closeFieldModal();
    });
  }
  if (fieldSave) fieldSave.addEventListener('click', saveNewComment);

  document.addEventListener('keydown', (e) => {
    if (e.key !== 'Escape') return;
    const fieldOpen = fieldOverlay && fieldOverlay.style.display === 'flex';
    if (fieldOpen) {
      closeFieldModal();
      return;
    }
    if (detailOverlay && detailOverlay.style.display === 'flex') requestCloseDetailModal();
  });
}

function requestCloseDetailModal() {
  if (editorState.activeEdit) {
    showAppModal({
      title: 'تعديل غير محفوظ',
      bodyHtml: '<p>لديك تعديل مفتوح لم يتم حفظه. هل تريد إغلاق النافذة والتخلي عن هذا التعديل؟</p>',
      confirmText: 'إغلاق وتجاهل',
      cancelText: 'العودة للتعديل',
      variant: 'danger'
    }).then(({ confirmed }) => {
      if (confirmed) {
        editorState.activeEdit = null;
        closeDetailModal();
      }
    });
    return;
  }
  closeDetailModal();
}

function closeDetailModal() {
  const overlay = document.getElementById('editor-detail-overlay');
  if (overlay) overlay.style.display = 'none';
}

function closeFieldModal() {
  const overlay = document.getElementById('editor-field-overlay');
  if (overlay) overlay.style.display = 'none';
}
