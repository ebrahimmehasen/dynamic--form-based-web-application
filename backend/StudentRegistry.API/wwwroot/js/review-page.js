// Student Records Review (admin, read-only) page logic.
// Vanilla JS, no framework — same string-template + innerHTML convention as form-handler.js.

var reviewState = {
  currentStudentId: null,
  notesByField: new Map(), // fieldName -> ReviewNoteResponseDto[]
  currentFieldName: null,
  currentFieldSnapshot: null,
  currentQuery: '',
  currentHasPendingReview: false
};

document.addEventListener('DOMContentLoaded', () => {
  initReviewSearch();
  initReviewModals();
  loadStudents('');
});

// ---------- Helpers ----------

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

// ---------- Search & table ----------

function initReviewSearch() {
  const input = document.getElementById('review-search-input');
  if (!input) return;
  let debounceTimer = null;
  input.addEventListener('input', () => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => loadStudents(input.value.trim()), 300);
  });

  const tbody = document.getElementById('review-table-body');
  if (tbody) {
    tbody.addEventListener('click', (e) => {
      const row = e.target.closest('tr[data-student-id]');
      if (!row) return;
      openStudentDetail(parseInt(row.getAttribute('data-student-id'), 10));
    });
  }
}

async function loadStudents(query) {
  const tbody = document.getElementById('review-table-body');
  const banner = document.getElementById('review-results-banner');
  const emptyState = document.getElementById('review-empty-state');
  if (!tbody) return;

  reviewState.currentQuery = query || '';

  try {
    const response = await fetch('/api/students/search?q=' + encodeURIComponent(query || ''));
    const payload = await response.json();
    if (!response.ok || payload.status !== 'success') {
      throw new Error(payload.message || 'تعذر تحميل بيانات الطلاب.');
    }

    const students = payload.data || [];
    tbody.innerHTML = students.map(renderStudentRow).join('');

    if (banner) {
      banner.textContent = students.length >= 500
        ? `عرض أول 500 نتيجة — يرجى تحسين البحث للوصول لنتائج أكثر تحديدًا.`
        : `عدد النتائج: ${students.length}`;
    }
    if (emptyState) {
      emptyState.style.display = students.length === 0 ? 'block' : 'none';
    }
  } catch (err) {
    tbody.innerHTML = '';
    showToast(err.message || 'حدث خطأ أثناء البحث.', 'danger');
  }
}

function renderStudentRow(student) {
  const rowClasses = [
    student.hasReviewNotes ? 'row-has-comment' : '',
    student.hasPendingReview ? 'row-pending-review' : ''
  ].filter(Boolean).join(' ');
  const statusBadge = student.hasPendingReview ? '<span class="pending-review-badge">قيد المراجعة</span>' : '';
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
      <td>${formatDate(student.submittedAt)}</td>
      <td>${statusBadge}</td>
    </tr>`;
}

// ---------- Detail modal ----------

async function openStudentDetail(studentId) {
  try {
    const [studentRes, notesRes] = await Promise.all([
      fetch(`/api/students/${studentId}`),
      fetch(`/api/reviewnotes/student/${studentId}`)
    ]);
    const studentPayload = await studentRes.json();
    const notesPayload = await notesRes.json();

    if (!studentRes.ok || studentPayload.status !== 'success') {
      throw new Error(studentPayload.message || 'تعذر تحميل بيانات الطالب.');
    }

    const student = studentPayload.data;
    const notes = notesRes.ok && notesPayload.status === 'success' ? (notesPayload.data || []) : [];

    reviewState.currentStudentId = studentId;
    reviewState.notesByField = new Map();
    notes.forEach(n => {
      if (!reviewState.notesByField.has(n.fieldName)) reviewState.notesByField.set(n.fieldName, []);
      reviewState.notesByField.get(n.fieldName).push(n);
    });

    renderStudentDetail(student);
    updatePendingReviewUi(student.hasPendingReview);

    const overlay = document.getElementById('review-detail-overlay');
    if (overlay) overlay.style.display = 'flex';
  } catch (err) {
    showToast(err.message || 'حدث خطأ أثناء تحميل بيانات الطالب.', 'danger');
  }
}

function updatePendingReviewUi(hasPendingReview) {
  reviewState.currentHasPendingReview = !!hasPendingReview;
  const badge = document.getElementById('review-pending-badge');
  const btn = document.getElementById('review-mark-pending-btn');
  if (badge) badge.style.display = reviewState.currentHasPendingReview ? 'inline-block' : 'none';
  if (btn) btn.style.display = reviewState.currentHasPendingReview ? 'none' : 'inline-flex';
}

// Live-patches the table row's class and status badge in place — no full page/list reload needed
// so the table always reflects the latest state right after the modal action, matching whatever the
// Editor's own table shows for the same student (same class/badge markup on both pages).
function setRowPendingReviewState(studentId, isPending) {
  const row = document.querySelector(`#review-table-body tr[data-student-id="${studentId}"]`);
  if (!row) return;
  row.classList.toggle('row-pending-review', isPending);
  const statusCell = row.lastElementChild;
  if (statusCell) {
    statusCell.innerHTML = isPending ? '<span class="pending-review-badge">قيد المراجعة</span>' : '';
  }
}

async function markCurrentStudentPendingReview() {
  if (!reviewState.currentStudentId) return;
  const btn = document.getElementById('review-mark-pending-btn');
  if (btn) btn.disabled = true;
  try {
    const response = await fetch(`/api/students/${reviewState.currentStudentId}/pending-review`, { method: 'POST' });
    const payload = await response.json();
    if (!response.ok || payload.status !== 'success') {
      throw new Error(payload.message || 'حدث خطأ أثناء وضع الطالب قيد المراجعة.');
    }
    updatePendingReviewUi(true);
    setRowPendingReviewState(reviewState.currentStudentId, true);
    showToast('تم وضع الطالب قيد المراجعة.', 'success');
  } catch (err) {
    showToast(err.message || 'حدث خطأ أثناء وضع الطالب قيد المراجعة.', 'danger');
  } finally {
    if (btn) btn.disabled = false;
  }
}

function renderStudentDetail(student) {
  const titleEl = document.getElementById('review-detail-title');
  const bodyEl = document.getElementById('review-detail-body');
  if (titleEl) titleEl.textContent = `${student.studentName || ''} — ${student.certification || ''}`;
  if (!bodyEl) return;

  const sections = [
    renderFieldSection('البيانات الشخصية', [
      ['Student.StudentName', 'اسم الطالب', student.studentName],
      ['Student.StudentNameEn', 'الاسم بالإنجليزية', student.studentNameEn],
      ['Student.NationalId', 'الرقم القومي', student.nationalId],
      ['Student.Gender', 'النوع', student.gender],
      ['Student.Phone', 'الهاتف', student.phone],
      ['Student.Email', 'البريد الإلكتروني', student.email]
    ]),
    renderFieldSection('بيانات ولي الأمر', [
      ['Student.GuardianName', 'اسم ولي الأمر', student.guardianName],
      ['Student.GuardianNationalId', 'الرقم القومي لولي الأمر', student.guardianNationalId],
      ['Student.GuardianPhone', 'هاتف ولي الأمر', student.guardianPhone],
      ['Student.GuardianLandlinePhone', 'رقم الهاتف الأرضي', student.guardianLandlinePhone],
      ['Student.GuardianRelation', 'صلة القرابة', student.guardianRelation],
      ['Student.GuardianOccupation', 'مهنة ولي الأمر', student.guardianOccupation]
    ]),
    renderFieldSection('العنوان', [
      ['Student.AddressGov', 'المحافظة', student.addressGov],
      ['Student.AddressCenter', 'المركز/القسم', student.addressCenter],
      ['Student.AddressVillage', 'القرية', student.addressVillage],
      ['Student.AddressStreet', 'الشارع', student.addressStreet],
      ['Student.AddressBuilding', 'رقم المبنى', student.addressBuilding],
      ['Student.AddressFloor', 'الطابق', student.addressFloor]
    ]),
    renderFieldSection('البيانات الأكاديمية', [
      ['Student.Certification', 'الشهادة', student.certification],
      ['Student.Track', 'المسار', student.track],
      ['Student.GraduationYear', 'سنة التخرج', student.graduationYear],
      ['Student.WishCollege', 'الكلية المرغوبة', student.wishCollege],
      ['Student.WishProgram', 'البرنامج المرغوب', student.wishProgram]
    ]),
    renderUploadedFilesSection(student),
    renderCertificateSection(student)
  ];

  bodyEl.innerHTML = sections.join('');
  wireClickableFields(bodyEl);
}

function renderFieldSection(title, fields) {
  const items = fields.map(([fieldName, label, value]) => `
      <div class="review-field-item">
        <span class="review-field-item-label">${escapeHtml(label)}</span>
        <span class="review-clickable-field${reviewState.notesByField.has(fieldName) ? ' field-reviewed' : ''}"
              data-field-name="${escapeHtml(fieldName)}"
              data-field-label="${escapeHtml(label)}"
              data-field-value="${escapeHtml(value)}"
              data-field-type="text">${displayValue(value)}</span>
      </div>`).join('');

  return `
    <div class="review-section">
      <div class="review-section-title">${escapeHtml(title)}</div>
      <div class="review-field-grid">${items}</div>
    </div>`;
}

function renderUploadedFilesSection(student) {
  const fieldName = 'Student.PhotoPath';
  const highlighted = reviewState.notesByField.has(fieldName) ? ' field-reviewed' : '';
  const photoBlock = student.photoPath
    ? `<div class="review-clickable-field${highlighted} review-photo-frame"
            data-field-name="${escapeHtml(fieldName)}"
            data-field-label="الصورة الشخصية"
            data-field-value="${escapeHtml(student.photoPath)}"
            data-field-type="image">
         <img src="/${escapeHtml(student.photoPath)}" alt="الصورة الشخصية">
       </div>`
    : '<p class="field-hint">لا توجد صورة مرفوعة.</p>';

  return `
    <div class="review-section">
      <div class="review-section-title">الملفات المرفوعة</div>
      <div class="review-field-item">
        <span class="review-field-item-label">الصورة الشخصية</span>
        ${photoBlock}
      </div>
    </div>`;
}

// ---------- Certificate details dispatch ----------

function renderCertificateSection(student) {
  let body = '<p class="field-hint">لا توجد بيانات شهادة مسجلة.</p>';

  if (student.saudiTotals) {
    body = renderCertTotalsAndGrades(student.saudiTotals, student.saudiGrades, 'SaudiTotals', [
      ['YearsCount', 'عدد سنوات الدراسة'],
      ['SchoolPercentage', 'نسبة المدرسة'],
      ['AptitudeScore', 'درجة القدرات'],
      ['FinalPercentage', 'النسبة النهائية'],
      ['EquivalentTotal', 'المجموع الاعتباري']
    ], (g) => `${g.yearLabel} — ${g.subjectName}: متحصل ${g.achieved} / موزون ${g.weighted} (معامل ${g.coefficient})`);
  } else if (student.igGrades) {
    body = renderCertTotalsAndGrades(student.igGrades, student.igGrades.gradeCounts, 'IgGrades', [
      ['IgProgram', 'البرنامج'],
      ['Factor', 'المعامل'],
      ['SportsBonus', 'مكافأة رياضية'],
      ['ScorePercentage', 'نسبة الدرجة'],
      ['GovernmentScore', 'الدرجة الحكومية']
    ], (g) => `${g.gradeType} — ${g.grade}: العدد ${g.count}`);
  } else if (student.kuwaitiTotals) {
    body = renderCertTotalsAndGrades(student.kuwaitiTotals, student.kuwaitiGrades, 'KuwaitiTotals', [
      ['YearsCount', 'عدد سنوات الدراسة'],
      ['Grade10Percentage', 'نسبة الصف العاشر'],
      ['Grade11Percentage', 'نسبة الصف الحادي عشر'],
      ['Grade12Percentage', 'نسبة الصف الثاني عشر'],
      ['FinalPercentage', 'النسبة النهائية'],
      ['EquivalentTotal', 'المجموع الاعتباري'],
      ['HasSecondAttempt', 'محاولة ثانية']
    ], (g) => `الصف ${g.gradeLevel} — ${g.subjectName}: ${g.obtained} / ${g.maxMark}`);
  } else if (student.qatariTotals) {
    body = renderSingleYearCert(student.qatariTotals, student.qatariGrades, 'QatariTotals');
  } else if (student.omaniTotals) {
    body = renderSingleYearCert(student.omaniTotals, student.omaniGrades, 'OmaniTotals');
  } else if (student.yemeniTotals) {
    body = renderSingleYearCert(student.yemeniTotals, student.yemeniGrades, 'YemeniTotals');
  } else if (student.bahrainiTotals) {
    body = renderCertTotalsAndGrades(student.bahrainiTotals, student.bahrainiGrades, 'BahrainiTotals', [
      ['Track', 'المسار'],
      ['FinalTotal', 'المجموع النهائي'],
      ['TotalMax', 'الحد الأقصى'],
      ['Percentage', 'النسبة المئوية'],
      ['EquivalentTotal', 'المجموع الاعتباري']
    ], (g) => `${g.subjectName}: ${g.mark}`);
  } else if (student.palestinianTotals) {
    body = renderCertTotalsAndGrades(student.palestinianTotals, null, 'PalestinianTotals', [
      ['Percentage', 'النسبة المئوية'],
      ['EquivalentTotal', 'المجموع الاعتباري'],
      ['Branch', 'الفرع']
    ], null);
  } else if (student.otherTotals) {
    body = renderCertTotalsAndGrades(student.otherTotals, null, 'OtherTotals', [
      ['CertificateName', 'اسم الشهادة'],
      ['Percentage', 'النسبة المئوية']
    ], null);
  } else if (student.egyptianTotals) {
    body = renderCertTotalsAndGrades(student.egyptianTotals, student.egyptianGrades, 'EgyptianTotals', [
      ['Track', 'المسار'],
      ['SubjectSystem', 'نظام المواد'],
      ['FinalTotal', 'المجموع النهائي'],
      ['Denominator', 'المجموع الكلي'],
      ['Percentage', 'النسبة المئوية']
    ], (g) => `${g.subjectName}: ${g.mark}`);
  } else if (student.azharTotals) {
    body = renderCertTotalsAndGrades(student.azharTotals, student.azharGrades, 'AzharTotals', [
      ['Section', 'القسم'],
      ['FinalTotal', 'المجموع النهائي'],
      ['Denominator', 'المجموع الكلي'],
      ['Percentage', 'النسبة المئوية'],
      ['EquivalentTotal', 'المجموع الاعتباري']
    ], (g) => `${g.subjectName}: ${g.mark}`);
  } else if (student.emiratiTotals) {
    body = renderSingleYearCert(student.emiratiTotals, student.emiratiGrades, 'EmiratiTotals');
  } else if (student.americanDiplomaTotals) {
    body = renderCertTotalsAndGrades(student.americanDiplomaTotals, student.americanDiplomaGrades, 'AmericanDiplomaTotals', [
      ['AverageScore', 'متوسط الدرجات'],
      ['BasePercentage', 'النسبة الأساسية'],
      ['TestType1', 'نوع اختبار المستوى الأول'],
      ['SatI', 'SAT I (معادل)'],
      ['ActComposite', 'ACT (الكلية، خام)'],
      ['TestType2', 'نوع اختبار المستوى الثاني'],
      ['SatII', 'SAT II (معادل)'],
      ['ActMath', 'ACT Math (خام)'],
      ['SatIISubject1', 'مادة SAT II الأولى'],
      ['SatIISubject2', 'مادة SAT II الثانية'],
      ['StudiedAdvancedMath', 'درس رياضيات متقدمة'],
      ['EquivalentPercentage', 'النسبة النهائية المعادلة']
    ], (g) => `${g.subjectName}: ${g.mark}`);
  }

  const hasCertData = body !== '<p class="field-hint">لا توجد بيانات شهادة مسجلة.</p>';
  const editHintSection = hasCertData ? `
    <div class="editor-recalc-bar">
      <button type="button" class="btn btn-secondary" id="review-edit-grades-btn">تعديل درجات الطالب</button>
      <div class="editor-recalc-panel" id="review-edit-grades-panel" style="display:none;">
        <p class="field-hint">هذه الصفحة للعرض فقط ولا تسمح بتعديل الدرجات. اضغط على أي درجة أو نتيجة أعلاه لإضافة ملاحظة مراجعة عليها.</p>
      </div>
    </div>` : '';

  return `
    <div class="review-section">
      <div class="review-section-title">تفاصيل الشهادة</div>
      ${body}
      ${editHintSection}
    </div>`;
}

function renderSingleYearCert(totals, grades, totalsFieldPrefix) {
  return renderCertTotalsAndGrades(totals, grades, totalsFieldPrefix, [
    ['FinalTotal', 'المجموع النهائي'],
    ['Percentage', 'النسبة المئوية'],
    ['EquivalentTotal', 'المجموع الاعتباري']
  ], (g) => `${g.subjectName}: ${g.mark}`);
}

// totalsFields: [[propName, label], ...] read off `totals` (camelCase from JSON).
// gradeFormatter(gradeItem) -> display string, or null to skip the grades list entirely.
function renderCertTotalsAndGrades(totals, grades, fieldPrefix, totalsFields, gradeFormatter) {
  const totalsItems = totalsFields.map(([prop, label]) => {
    const jsProp = prop.charAt(0).toLowerCase() + prop.slice(1);
    const value = totals[jsProp];
    const displayed = typeof value === 'boolean' ? (value ? 'نعم' : 'لا') : value;
    const fieldName = `${fieldPrefix}.${prop}`;
    return `
      <div class="review-field-item">
        <span class="review-field-item-label">${escapeHtml(label)}</span>
        <span class="review-clickable-field${reviewState.notesByField.has(fieldName) ? ' field-reviewed' : ''}"
              data-field-name="${escapeHtml(fieldName)}"
              data-field-label="${escapeHtml(label)}"
              data-field-value="${escapeHtml(displayed)}"
              data-field-type="text">${displayValue(displayed)}</span>
      </div>`;
  }).join('');

  let gradesHtml = '';
  if (gradeFormatter && grades && grades.length > 0) {
    const rows = grades.map((g, idx) => {
      const label = gradeFormatter(g);
      const fieldName = `${fieldPrefix}.Grades[${idx}]`;
      return `
        <div class="review-field-item">
          <span class="review-clickable-field${reviewState.notesByField.has(fieldName) ? ' field-reviewed' : ''}"
                data-field-name="${escapeHtml(fieldName)}"
                data-field-label="سجل درجة"
                data-field-value="${escapeHtml(label)}"
                data-field-type="text">${escapeHtml(label)}</span>
        </div>`;
    }).join('');
    gradesHtml = `<div class="review-field-grid" style="margin-top:0.85rem;">${rows}</div>`;
  }

  return `<div class="review-field-grid">${totalsItems}</div>${gradesHtml}`;
}

// ---------- Field-note (small) modal ----------

function wireClickableFields(container) {
  container.querySelectorAll('.review-clickable-field').forEach(el => {
    el.addEventListener('click', () => {
      openFieldReviewModal({
        fieldName: el.getAttribute('data-field-name'),
        label: el.getAttribute('data-field-label'),
        value: el.getAttribute('data-field-value'),
        type: el.getAttribute('data-field-type')
      });
    });
  });

  const editGradesBtn = container.querySelector('#review-edit-grades-btn');
  if (editGradesBtn) {
    editGradesBtn.addEventListener('click', () => {
      const panel = container.querySelector('#review-edit-grades-panel');
      if (panel) panel.style.display = panel.style.display === 'none' ? 'block' : 'none';
    });
  }
}

function initReviewModals() {
  const detailClose = document.getElementById('review-detail-close');
  const detailOverlay = document.getElementById('review-detail-overlay');
  if (detailClose) detailClose.addEventListener('click', closeDetailModal);
  if (detailOverlay) {
    detailOverlay.addEventListener('click', (e) => {
      if (e.target === detailOverlay) closeDetailModal();
    });
  }

  const markPendingBtn = document.getElementById('review-mark-pending-btn');
  if (markPendingBtn) markPendingBtn.addEventListener('click', markCurrentStudentPendingReview);

  const fieldClose = document.getElementById('review-field-close');
  const fieldCancel = document.getElementById('review-field-cancel');
  const fieldOverlay = document.getElementById('review-field-overlay');
  const fieldSave = document.getElementById('review-field-save');
  if (fieldClose) fieldClose.addEventListener('click', closeFieldModal);
  if (fieldCancel) fieldCancel.addEventListener('click', closeFieldModal);
  if (fieldOverlay) {
    fieldOverlay.addEventListener('click', (e) => {
      if (e.target === fieldOverlay) closeFieldModal();
    });
  }
  if (fieldSave) fieldSave.addEventListener('click', saveFieldNote);

  document.addEventListener('keydown', (e) => {
    if (e.key !== 'Escape') return;
    const fieldOpen = fieldOverlay && fieldOverlay.style.display === 'flex';
    if (fieldOpen) {
      closeFieldModal();
      return;
    }
    if (detailOverlay && detailOverlay.style.display === 'flex') closeDetailModal();
  });
}

function closeDetailModal() {
  const overlay = document.getElementById('review-detail-overlay');
  if (overlay) overlay.style.display = 'none';
}

function closeFieldModal() {
  const overlay = document.getElementById('review-field-overlay');
  if (overlay) overlay.style.display = 'none';
}

function openFieldReviewModal({ fieldName, label, value, type }) {
  reviewState.currentFieldName = fieldName;
  reviewState.currentFieldSnapshot = value;

  const titleEl = document.getElementById('review-field-title');
  const currentWrap = document.getElementById('review-field-current-wrap');
  const currentEl = document.getElementById('review-field-current');
  const historyWrap = document.getElementById('review-field-history-wrap');
  const historyEl = document.getElementById('review-field-history');
  const input = document.getElementById('review-field-note-input');

  if (titleEl) titleEl.textContent = label || 'ملاحظة مراجعة';
  if (input) input.value = '';

  if (type === 'image') {
    if (currentWrap) currentWrap.style.display = 'none';
    if (historyWrap) {
      // Prepend an image preview above the history list for the file-variant modal.
      historyWrap.insertAdjacentHTML('beforebegin', `
        <div id="review-field-image-preview">
          <label class="review-field-label">معاينة الملف</label>
          <div class="review-field-current"><img src="/${escapeHtml(value)}" alt="معاينة"></div>
        </div>`);
    }
  } else {
    if (currentWrap) currentWrap.style.display = '';
    if (currentEl) currentEl.textContent = value || '—';
    const stalePreview = document.getElementById('review-field-image-preview');
    if (stalePreview) stalePreview.remove();
  }

  const notes = reviewState.notesByField.get(fieldName) || [];
  if (notes.length > 0 && historyWrap && historyEl) {
    historyWrap.style.display = '';
    historyEl.innerHTML = notes.map(n => `
      <div class="review-field-history-item">
        <div class="review-field-history-meta">${escapeHtml(n.author)} — ${formatDate(n.createdAt)}</div>
        <div>${escapeHtml(n.reviewerNote)}</div>
      </div>`).join('');
  } else if (historyWrap) {
    historyWrap.style.display = 'none';
  }

  const overlay = document.getElementById('review-field-overlay');
  if (overlay) overlay.style.display = 'flex';
  if (input) setTimeout(() => input.focus(), 50);
}

async function saveFieldNote() {
  const input = document.getElementById('review-field-note-input');
  const noteText = input ? input.value.trim() : '';
  if (!noteText) {
    showToast('يرجى كتابة ملاحظة قبل الحفظ.', 'danger');
    return;
  }
  if (!reviewState.currentStudentId || !reviewState.currentFieldName) return;

  try {
    const response = await fetch('/api/reviewnotes', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        studentId: reviewState.currentStudentId,
        fieldName: reviewState.currentFieldName,
        fieldValueSnapshot: reviewState.currentFieldSnapshot,
        reviewerNote: noteText
      })
    });
    const payload = await response.json();
    if (!response.ok || payload.status !== 'success') {
      throw new Error(payload.message || 'تعذر حفظ الملاحظة.');
    }

    const fieldName = reviewState.currentFieldName;
    if (!reviewState.notesByField.has(fieldName)) reviewState.notesByField.set(fieldName, []);
    reviewState.notesByField.get(fieldName).push(payload.data);

    document.querySelectorAll(`.review-clickable-field[data-field-name="${cssEscape(fieldName)}"]`)
      .forEach(el => el.classList.add('field-reviewed'));

    // Live-update the main table row's yellow highlight — this student now has at least one
    // review note, regardless of which field it was added to.
    const row = document.querySelector(`#review-table-body tr[data-student-id="${reviewState.currentStudentId}"]`);
    if (row) row.classList.add('row-has-comment');

    showToast('تم حفظ الملاحظة بنجاح.', 'success');
    closeFieldModal();
  } catch (err) {
    showToast(err.message || 'حدث خطأ أثناء حفظ الملاحظة.', 'danger');
  }
}

function cssEscape(value) {
  return typeof CSS !== 'undefined' && CSS.escape ? CSS.escape(value) : value.replace(/["\\]/g, '\\$&');
}
