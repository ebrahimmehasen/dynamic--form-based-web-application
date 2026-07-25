// Global state to store form inputs and base64 photo
let uploadedPhotoBase64 = '';

// Initialize Form Handlers
function initFormHandlers() {
  initImageUpload();
  setupTableCalculationListeners();
  setupIGCalculatorListeners();
  setupKuwaitiCalculatorListeners();
  setupQatariCalculatorListeners();
  setupOmaniCalculatorListeners();
  setupYemeniCalculatorListeners();
  setupBahrainiCalculatorListeners();
  setupPalestinianCalculatorListeners();
  setupOtherCalculatorListeners();
  setupEgyptianCalculatorListeners();
  setupAzharCalculatorListeners();
  setupEmiratiCalculatorListeners();
  setupAmericanDiplomaCalculatorListeners();
  setupSubmissionHandler();
}

// 1. Photo Upload Handler & Validation
function initImageUpload() {
  const photoInput = document.getElementById('photo-input');
  const uploadWrapper = document.getElementById('upload-wrapper');
  const previewContainer = document.getElementById('photo-preview-container');
  const previewImg = document.getElementById('photo-preview');
  const photoMeta = document.getElementById('photo-meta');
  const photoFeedback = document.getElementById('photo-feedback');

  // Drag and drop events
  ['dragenter', 'dragover'].forEach(eventName => {
    uploadWrapper.addEventListener(eventName, (e) => {
      e.preventDefault();
      uploadWrapper.style.borderColor = 'var(--primary-color)';
      uploadWrapper.style.backgroundColor = 'var(--primary-light)';
    }, false);
  });

  ['dragleave', 'drop'].forEach(eventName => {
    uploadWrapper.addEventListener(eventName, (e) => {
      e.preventDefault();
      uploadWrapper.style.borderColor = 'var(--border-color)';
      uploadWrapper.style.backgroundColor = 'var(--bg-main)';
    }, false);
  });

  uploadWrapper.addEventListener('drop', (e) => {
    const dt = e.dataTransfer;
    const files = dt.files;
    if (files.length) {
      photoInput.files = files;
      handlePhotoSelected(files[0]);
    }
  });

  photoInput.addEventListener('change', function() {
    if (this.files.length) {
      handlePhotoSelected(this.files[0]);
    }
  });

  function handlePhotoSelected(file) {
    // Check file format
    const validFormats = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!validFormats.includes(file.type)) {
      showPhotoError('عذراً، يجب أن تكون الصورة بصيغة JPG أو PNG أو WebP.');
      return;
    }

    // Check size (Max 5MB)
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      showPhotoError('حجم الصورة كبير جداً. الحد الأقصى هو 5 ميجابايت.');
      return;
    }

    // Read file as Base64
    const reader = new FileReader();
    reader.onload = function(e) {
      const base64Data = e.target.result;

      // Check Aspect Ratio
      const img = new Image();
      img.onload = function() {
        const width = img.width;
        const height = img.height;
        const ratio = height / width;

        // Target: 2:3 ratio -> height/width is ~1.5
        // Acceptable normal ratio range is 1.35 to 1.65
        let feedbackText = '';
        let feedbackClass = '';

        if (ratio >= 1.35 && ratio <= 1.65) {
          feedbackText = 'أبعاد الصورة مناسبة (نسبة 2:3)';
          feedbackClass = 'success';
        } else {
          feedbackText = 'تنبيه: أبعاد الصورة ليست قريبة من نسبة 2:3 (4×6). يمكنك المتابعة ولكن يُفضل تعديلها.';
          feedbackClass = 'warning';
        }

        // Show image and meta info
        previewImg.src = base64Data;
        photoMeta.textContent = `حجم الصورة: ${(file.size / (1024 * 1024)).toFixed(2)} MB | الأبعاد: ${width} × ${height}px`;
        photoFeedback.textContent = feedbackText;
        photoFeedback.className = 'photo-feedback ' + feedbackClass;
        previewContainer.style.display = 'flex';

        uploadedPhotoBase64 = base64Data;
        hideAlert('form-alert');
        updateProgressIndicator();
      };
      img.src = base64Data;
    };
    reader.readAsDataURL(file);
  }

  function showPhotoError(message) {
    uploadedPhotoBase64 = '';
    previewContainer.style.display = 'none';
    showAlert('form-alert', message, 'danger');
  }
}

// 2. Grades Table Generator
function generateGradesTable(yearVal) {
  const standardWrapper = document.getElementById('standard-table-wrapper');
  const saudiMultiContainer = document.getElementById('saudi-multi-tables-container');
  const tableBody = document.getElementById('grades-table-body');
  tableBody.innerHTML = '';
  saudiMultiContainer.innerHTML = '';

  const certKey = document.getElementById('cert-select').value;
  const isSaudi = (certKey === 'saudi');

  // Toggle Saudi Summary Box visibility
  const saudiSummaryBox = document.getElementById('saudi-summary-box');
  if (saudiSummaryBox) {
    saudiSummaryBox.style.display = isSaudi ? 'block' : 'none';
  }

  if (isSaudi) {
    standardWrapper.style.display = 'none';
    saudiMultiContainer.style.display = 'block';

    const blocks = typeof getSaudiBlocks === 'function' ? getSaudiBlocks(yearVal) : [];

    blocks.forEach((block, blockIndex) => {
      const card = document.createElement('div');
      card.className = 'saudi-year-card';
      card.style.cssText = 'background: var(--card-bg, #fff); border: 1px solid var(--border-color, #e2e8f0); border-radius: var(--radius-md, 8px); padding: 1.5rem; margin-bottom: 1.5rem; box-shadow: var(--shadow-sm, 0 1px 3px rgba(0,0,0,0.05));';

      let tableRowsHtml = '';
      block.subjects.forEach((subjectObj) => {
        const subjectName = subjectObj.name;
        tableRowsHtml += `
          <tr>
            <td class="col-num"></td>
            <td class="col-subject"></td>
            <td class="col-grade">
              <input type="number" min="0" step="any" required
                     placeholder="المتحصلة" class="table-input saudi-achieved-input"
                     data-subject="${subjectName}">
            </td>
            <td class="col-weight">
              <input type="number" min="0" step="any" required
                     placeholder="الموزونة" class="table-input saudi-weighted-input"
                     data-subject="${subjectName}">
            </td>
            <td class="col-achieved saudi-coefficient-cell">-</td>
            <td class="col-action"><button type="button" class="saudi-row-delete-btn" title="حذف المادة">✕</button></td>
          </tr>
        `;
      });

      card.setAttribute('data-year-key', block.key);
      card.innerHTML = `
        <h3 class="saudi-year-title" style="margin-top: 0; margin-bottom: 1rem; color: var(--primary-color); font-size: 1.15rem; font-weight: 600; border-bottom: 2px solid var(--primary-light); padding-bottom: 0.5rem;">
          📚 ${block.label}
        </h3>
        <div class="table-responsive">
          <table class="grades-table">
            <thead>
              <tr>
                <th class="col-num">#</th>
                <th class="col-subject">اسم المادة</th>
                <th class="col-grade">الدرجة المتحصلة</th>
                <th class="col-weight">الدرجة الموزونة</th>
                <th class="col-achieved">المعامل</th>
                <th class="col-action"></th>
              </tr>
            </thead>
            <tbody>
              ${tableRowsHtml}
            </tbody>
          </table>
        </div>

        <div class="saudi-add-subject-row">
          <button type="button" class="saudi-add-subject-btn">إضافة مادة</button>
        </div>

        <!-- Year Subtotal Bar -->
        <div class="saudi-subtotal-bar" id="subtotal-${block.key.replace(' ', '-')}" style="margin-top: 1rem; background: var(--light-bg, #f8fafc); border: 1px solid var(--border-color); padding: 0.75rem 1rem; border-radius: var(--radius-sm, 6px); display: flex; flex-wrap: wrap; justify-content: space-between; font-size: 0.9rem; font-weight: 600;">
          <div>مجموع الدرجات المحرزة: <span class="sub-achieved" style="color: var(--primary-color);">0.00</span></div>
          <div>مجموع المعاملات: <span class="sub-coefficients" style="color: var(--primary-color);">0</span></div>
          <div>المجموع الموزون: <span class="sub-weighted" style="color: var(--primary-color);">0.00</span></div>
          <div>نسبة السنة: <span class="sub-year-percent" style="color: var(--primary-color);">0.00%</span></div>
          <div>المساهمة الموزونة: <span class="sub-contribution" style="color: var(--primary-color);">0.00%</span></div>
        </div>
      `;
      saudiMultiContainer.appendChild(card);
      renumberSaudiCardRows(card);
      attachSaudiCardControls(card);
    });

  } else {
    standardWrapper.style.display = 'block';
    saudiMultiContainer.style.display = 'none';

    // Set table headers dynamically
    const thGrade = document.getElementById('th-grade');
    const thWeight = document.getElementById('th-weight');
    const thAchieved = document.getElementById('th-achieved');

    if (thGrade) thGrade.textContent = 'الدرجة';
    if (thWeight) thWeight.textContent = 'النسبة الموزونة (%)';
    if (thAchieved) thAchieved.textContent = 'الدرجة المتحصلة';

    const subjects = typeof getActiveSubjects === 'function' ? getActiveSubjects(certKey, yearVal) : [];

    subjects.forEach((subjectObj, index) => {
      const subjectName = subjectObj.name;
      const row = document.createElement('tr');
      row.innerHTML = `
        <td class="col-num">${index + 1}</td>
        <td class="col-subject">${subjectName}</td>
        <td class="col-grade">
          <input type="number" min="0" max="100" step="any" required
                 placeholder="0-100" class="table-input grade-input"
                 data-subject="${subjectName}">
        </td>
        <td class="col-weight">
          <input type="number" min="0" max="100" step="any" required
                 placeholder="0-100" class="table-input weight-input">
        </td>
        <td class="col-achieved">0.00</td>
      `;
      tableBody.appendChild(row);
    });
  }

  // Re-bind listeners for table calculation
  setupTableCalculationListeners();
}

// Official Saudi formula: per row, Coefficient = Weighted / Achieved (must be a whole
// number — otherwise the row is flagged as an error and the final grade is blocked).
// Per block: yearPercentage = (Σ Weighted) / (Σ Coefficient), weighted by the block's
// position via getSaudiYearWeights(yearsCount). School total = Σ weighted year percentages.
// Final = (schoolTotal + درجة القدرات) / 2. Mirrors StudentService.ProcessSaudiCertificate.
// Hoisted to module scope (not nested in setupTableCalculationListeners) so that dynamically
// added/removed subject rows can call the exact same function reference without re-binding
// duplicate listeners on every add/delete.
function recalculateSaudi() {
      const saudiMultiContainer = document.getElementById('saudi-multi-tables-container');
      const cards = saudiMultiContainer ? saudiMultiContainer.querySelectorAll('.saudi-year-card') : [];
      const yearsCountVal = document.getElementById('year-select').value;
      const yearWeights = typeof getSaudiYearWeights === 'function' ? getSaudiYearWeights(yearsCountVal) : {};

      let overallAchieved = 0;
      let overallWeighted = 0;
      let overallCoefficients = 0;
      let schoolPercentage = 0;
      let hasError = false;

      cards.forEach(card => {
        let cardAchieved = 0;
        let cardWeighted = 0;
        let cardCoefficients = 0;
        let cardHasError = false;

        const rows = card.querySelectorAll('tbody tr');
        rows.forEach(row => {
          const achievedInput = row.querySelector('.saudi-achieved-input');
          const weightedInput = row.querySelector('.saudi-weighted-input');
          const coefficientCell = row.querySelector('.saudi-coefficient-cell');

          const achievedRaw = achievedInput.value;
          const weightedRaw = weightedInput.value;
          const achieved = parseFloat(achievedRaw) || 0;
          const weighted = parseFloat(weightedRaw) || 0;

          achievedInput.style.borderColor = '';
          weightedInput.style.borderColor = '';
          coefficientCell.style.color = '';

          let coefficient = 0;

          if (achievedRaw === '' || weightedRaw === '') {
            coefficientCell.textContent = '-';
            cardHasError = true;
          } else if (achieved <= 0) {
            coefficientCell.textContent = '⚠️';
            coefficientCell.style.color = 'var(--danger-color)';
            achievedInput.style.borderColor = 'var(--danger-color)';
            cardHasError = true;
          } else {
            const rawCoefficient = weighted / achieved;
            const rounded = Math.round(rawCoefficient);
            if (Math.abs(rawCoefficient - rounded) < 0.01) {
              coefficient = rounded;
              coefficientCell.textContent = String(coefficient);
            } else {
              coefficientCell.textContent = `⚠️ ${rawCoefficient.toFixed(2)}`;
              coefficientCell.style.color = 'var(--danger-color)';
              achievedInput.style.borderColor = 'var(--danger-color)';
              weightedInput.style.borderColor = 'var(--danger-color)';
              cardHasError = true;
            }
          }

          cardAchieved += achieved;
          cardWeighted += weighted;
          cardCoefficients += coefficient;
        });

        const yearKey = card.getAttribute('data-year-key');
        const weightPercent = yearWeights[yearKey] || 0;
        const yearPercentage = cardCoefficients > 0 ? (cardWeighted / cardCoefficients) : 0;
        const contribution = yearPercentage * (weightPercent / 100);

        const subAchievedEl = card.querySelector('.sub-achieved');
        const subCoefficientsEl = card.querySelector('.sub-coefficients');
        const subWeightedEl = card.querySelector('.sub-weighted');
        const subYearPercentEl = card.querySelector('.sub-year-percent');
        const subContributionEl = card.querySelector('.sub-contribution');

        if (subAchievedEl) subAchievedEl.textContent = cardAchieved.toFixed(2);
        if (subCoefficientsEl) subCoefficientsEl.textContent = cardCoefficients;
        if (subWeightedEl) subWeightedEl.textContent = cardWeighted.toFixed(2);
        if (subYearPercentEl) subYearPercentEl.textContent = yearPercentage.toFixed(2) + '%';
        if (subContributionEl) subContributionEl.textContent = contribution.toFixed(2) + `% (وزن ${weightPercent}%)`;

        overallAchieved += cardAchieved;
        overallWeighted += cardWeighted;
        overallCoefficients += cardCoefficients;
        if (!cardHasError) {
          schoolPercentage += contribution;
        } else {
          hasError = true;
        }
      });

      const elTotalAchieved = document.getElementById('saudi-total-achieved');
      const elTotalCoefficients = document.getElementById('saudi-total-coefficients');
      const elTotalWeighted = document.getElementById('saudi-total-weighted');
      const elSchoolPercentage = document.getElementById('saudi-school-percentage');
      const elFinalGPA = document.getElementById('saudi-final-gpa');

      if (elTotalAchieved) elTotalAchieved.textContent = overallAchieved.toFixed(2);
      if (elTotalCoefficients) elTotalCoefficients.textContent = overallCoefficients;
      if (elTotalWeighted) elTotalWeighted.textContent = overallWeighted.toFixed(2);
      if (elSchoolPercentage) elSchoolPercentage.textContent = schoolPercentage.toFixed(2) + '%';

      const aptitudeInput = document.getElementById('saudi-aptitude-score');
      const aptitudeRaw = aptitudeInput ? aptitudeInput.value : '';
      const aptitudeVal = parseFloat(aptitudeRaw);
      const hasValidAptitude = aptitudeRaw !== '' && !isNaN(aptitudeVal) && aptitudeVal >= 0 && aptitudeVal <= 100;

      if (aptitudeInput) {
        aptitudeInput.style.borderColor = (aptitudeRaw !== '' && !hasValidAptitude) ? 'var(--danger-color)' : '';
      }

      const elEquivalentTotal = document.getElementById('saudi-equivalent-total');

      if (elFinalGPA) {
        if (hasError) {
          elFinalGPA.textContent = '⚠️ يوجد أخطاء في الدرجات المدخلة (تأكد أن المعامل رقم صحيح لكل مادة)';
          elFinalGPA.style.color = 'var(--danger-color)';
          if (elEquivalentTotal) { elEquivalentTotal.textContent = '—'; elEquivalentTotal.style.color = ''; }
        } else if (!hasValidAptitude) {
          elFinalGPA.textContent = '— (أدخل درجة القدرات)';
          elFinalGPA.style.color = '';
          if (elEquivalentTotal) { elEquivalentTotal.textContent = '— (أدخل درجة القدرات)'; elEquivalentTotal.style.color = ''; }
        } else {
          // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the
          // equivalent total (المجموع الاعتباري / المجموع المصري) must be derived from it, never
          // from the raw unrounded value. Mirrors StudentService.
          const finalGrade = Math.round(((schoolPercentage + aptitudeVal) / 2) * 100) / 100;
          elFinalGPA.textContent = finalGrade.toFixed(2) + '%';
          elFinalGPA.style.color = 'var(--success-color)';
          if (elEquivalentTotal) {
            const equivalentTotal = (finalGrade / 100) * 410;
            elEquivalentTotal.textContent = equivalentTotal.toFixed(2) + ' / 410';
            elEquivalentTotal.style.color = 'var(--success-color)';
          }
        }
      }

      updateProgressIndicator();
}

// Attach live-recalculation listeners to a single row's achieved/weighted inputs. Reuses the
// module-scope recalculateSaudi so repeated calls (e.g. after adding a row) never stack
// duplicate listeners referencing different closures.
function attachSaudiRowListeners(scope) {
  const inputs = scope.querySelectorAll('.saudi-achieved-input, .saudi-weighted-input');
  inputs.forEach(input => {
    input.addEventListener('input', recalculateSaudi);
    input.addEventListener('change', recalculateSaudi);
  });
}

// 2b. Saudi subject management (add / delete) — each year card manages its own table
// independently, per the "منح كل سنة دراسية جدول مستقل" requirement.

// Subjects that are collected on the certificate but excluded from the Egyptian equivalency
// (تنسيق) calculation. Mirrors StudentValidator's server-side denylist — keep both in sync.
const SAUDI_DENIED_SUBJECTS_EXACT = [
  'الفقه', 'القرآن الكريم والتفسير', 'الحديث', 'التوحيد',
  'التربية الصحية والبدنية', 'اللياقة والثقافة الصحية'
];
// Note: 'رياضه'/'رياضيه' (sport, noun/adjective) deliberately do NOT match 'الرياضيات'
// (Mathematics) after normalization — verify with any new keyword before adding it here.
const SAUDI_DENIED_KEYWORDS = [
  'قرآن', 'تفسير', 'حديث', 'توحيد', 'فقه', 'اسلام', 'اسلاميه', 'عقيده', 'شريعه', 'دينيه',
  'رياضه', 'رياضيه', 'بدني', 'بدنيه', 'لياقه', 'دفاع عن النفس',
  'اختياري', 'اختيار حر', 'ماده حره', 'نشاط حر'
];

// Normalizes Arabic text for comparison: trims/collapses whitespace, unifies alef/taa-marbuta/
// yaa variants, strips diacritics and tatweel, lowercases (for any incidental Latin chars).
function normalizeArabicSubject(text) {
  if (!text) return '';
  return text
    .trim()
    .replace(/\s+/g, ' ')
    .replace(/[أإآا]/g, 'ا')
    .replace(/ة/g, 'ه')
    .replace(/ى/g, 'ي')
    .replace(/[ًٌٍَُِّْـ]/g, '')
    .toLowerCase();
}

function checkSaudiSubjectAllowed(rawName) {
  const normalized = normalizeArabicSubject(rawName);
  if (!normalized) return { allowed: false, reason: 'الرجاء إدخال اسم المادة.' };

  const deniedExact = SAUDI_DENIED_SUBJECTS_EXACT.some(d => normalizeArabicSubject(d) === normalized);
  if (deniedExact) {
    return { allowed: false, reason: 'لا يمكن إضافة هذه المادة لأنها غير داخلة في حساب التنسيق المصري.' };
  }

  const deniedKeyword = SAUDI_DENIED_KEYWORDS.some(k => normalized.includes(k));
  if (deniedKeyword) {
    return { allowed: false, reason: 'لا يمكن إضافة هذه المادة لأنها غير داخلة في حساب التنسيق المصري.' };
  }

  return { allowed: true, reason: '' };
}

// Recomputes the "#" and displayed subject-name (with duplicate-occurrence suffixes) for a
// single card's rows, without touching any already-entered achieved/weighted values.
function renumberSaudiCardRows(card) {
  const rows = card.querySelectorAll('tbody tr');
  const occurrenceCounts = {};
  rows.forEach(row => {
    const subject = row.querySelector('.saudi-achieved-input').getAttribute('data-subject');
    occurrenceCounts[subject] = (occurrenceCounts[subject] || 0) + 1;
  });

  const currentOccurrence = {};
  rows.forEach((row, index) => {
    row.querySelector('.col-num').textContent = index + 1;
    const subject = row.querySelector('.saudi-achieved-input').getAttribute('data-subject');
    let displayName = subject;
    if (occurrenceCounts[subject] > 1) {
      currentOccurrence[subject] = (currentOccurrence[subject] || 0) + 1;
      displayName = `${subject} (${currentOccurrence[subject]})`;
    }
    row.querySelector('.col-subject').textContent = displayName;
  });
}

function buildSaudiSubjectRow(subjectName) {
  const row = document.createElement('tr');
  row.innerHTML = `
    <td class="col-num"></td>
    <td class="col-subject"></td>
    <td class="col-grade">
      <input type="number" min="0" step="any" required
             placeholder="المتحصلة" class="table-input saudi-achieved-input"
             data-subject="${subjectName}">
    </td>
    <td class="col-weight">
      <input type="number" min="0" step="any" required
             placeholder="الموزونة" class="table-input saudi-weighted-input"
             data-subject="${subjectName}">
    </td>
    <td class="col-achieved saudi-coefficient-cell">-</td>
    <td class="col-action"><button type="button" class="saudi-row-delete-btn" title="حذف المادة">✕</button></td>
  `;
  return row;
}

async function handleAddSaudiSubject(card) {
  const inputId = 'saudi-add-subject-input';
  const yearTitle = card.querySelector('.saudi-year-title') ? card.querySelector('.saudi-year-title').textContent.trim() : 'هذه السنة';

  const step1 = await showAppModal({
    title: `إضافة مادة جديدة — ${yearTitle}`,
    bodyHtml: `
      <div class="alert alert-danger">
        تنبيه: يرجى عدم إضافة أي مادة غير داخلة في حساب التنسيق المصري، مثل: الفقه، القرآن الكريم والتفسير،
        الحديث، التوحيد، التربية الصحية والبدنية، اللياقة والثقافة الصحية، أو أي مواد دينية أو تربية بدنية
        أو مواد اختيار حر. يتم احتساب المواد الأكاديمية فقط وفقاً لقواعد التنسيق المصري.
      </div>
      <div class="form-group" style="margin-bottom:0;">
        <label for="${inputId}">اسم المادة</label>
        <input type="text" id="${inputId}" class="form-control" placeholder="مثال: الأحياء" autocomplete="off">
      </div>
    `,
    confirmText: 'التالي',
    cancelText: 'إلغاء',
    variant: 'warning',
    focusInputId: inputId
  });

  if (!step1.confirmed) return;

  const rawName = (step1.value || '').trim();
  const check = checkSaudiSubjectAllowed(rawName);
  if (!check.allowed) {
    showToast(check.reason, 'danger');
    return;
  }

  const step2 = await showAppModal({
    title: 'تأكيد الإضافة',
    bodyHtml: `<p>هل أنت متأكد من إضافة مادة "<strong>${rawName}</strong>" إلى ${yearTitle}؟</p>`,
    confirmText: 'إضافة',
    cancelText: 'تراجع',
    variant: 'primary'
  });

  if (!step2.confirmed) return;

  const row = buildSaudiSubjectRow(rawName);
  card.querySelector('tbody').appendChild(row);
  attachSaudiRowListeners(row);
  row.querySelector('.saudi-row-delete-btn').addEventListener('click', () => {
    handleDeleteSaudiSubject(card, row);
  });
  renumberSaudiCardRows(card);
  recalculateSaudi();
  showToast(`تمت إضافة مادة "${rawName}" بنجاح.`, 'success');
}

async function handleDeleteSaudiSubject(card, row) {
  const subjectDisplay = row.querySelector('.col-subject').textContent.trim();
  const yearTitle = card.querySelector('.saudi-year-title') ? card.querySelector('.saudi-year-title').textContent.trim() : 'هذه السنة';

  const result = await showAppModal({
    title: 'تأكيد الحذف',
    bodyHtml: `<p>هل أنت متأكد من حذف مادة "<strong>${subjectDisplay}</strong>" من ${yearTitle}؟ لا يمكن التراجع عن هذا الإجراء.</p>`,
    confirmText: 'حذف',
    cancelText: 'تراجع',
    variant: 'danger'
  });

  if (!result.confirmed) return;

  row.remove();
  renumberSaudiCardRows(card);
  recalculateSaudi();
  showToast(`تم حذف مادة "${subjectDisplay}".`, 'success');
}

// Wires the "+ إضافة مادة" button and every row's delete button for one card. Safe to call
// repeatedly (buttons are freshly created per generateGradesTable() run, no stale handlers).
function attachSaudiCardControls(card) {
  const addBtn = card.querySelector('.saudi-add-subject-btn');
  if (addBtn) {
    addBtn.addEventListener('click', () => handleAddSaudiSubject(card));
  }

  card.querySelectorAll('.saudi-row-delete-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      const row = btn.closest('tr');
      handleDeleteSaudiSubject(card, row);
    });
  });
}

// Generic promise-based confirmation/input modal (replaces alert()/confirm()).
// variant: 'primary' | 'warning' | 'danger'. Pass focusInputId to collect a text value.
function showAppModal({ title, bodyHtml, confirmText = 'تأكيد', cancelText = 'إلغاء', variant = 'primary', focusInputId = null }) {
  return new Promise((resolve) => {
    const overlay = document.getElementById('app-modal-overlay');
    const box = document.getElementById('app-modal-box');
    const titleEl = document.getElementById('app-modal-title');
    const bodyEl = document.getElementById('app-modal-body');
    const confirmBtn = document.getElementById('app-modal-confirm');
    const cancelBtn = document.getElementById('app-modal-cancel');
    if (!overlay || !box || !titleEl || !bodyEl || !confirmBtn || !cancelBtn) {
      resolve({ confirmed: false, value: null });
      return;
    }

    titleEl.textContent = title;
    bodyEl.innerHTML = bodyHtml;
    confirmBtn.textContent = confirmText;
    cancelBtn.textContent = cancelText;
    box.className = 'app-modal-box app-modal-' + variant;

    overlay.style.display = 'flex';

    const cleanup = () => {
      overlay.style.display = 'none';
      confirmBtn.removeEventListener('click', onConfirm);
      cancelBtn.removeEventListener('click', onCancel);
      overlay.removeEventListener('click', onOverlayClick);
      document.removeEventListener('keydown', onKeydown);
    };

    const onConfirm = () => {
      const inputEl = focusInputId ? document.getElementById(focusInputId) : null;
      const value = inputEl ? inputEl.value : null;
      cleanup();
      resolve({ confirmed: true, value });
    };
    const onCancel = () => {
      cleanup();
      resolve({ confirmed: false, value: null });
    };
    const onOverlayClick = (e) => {
      if (e.target === overlay) onCancel();
    };
    const onKeydown = (e) => {
      if (e.key === 'Escape') onCancel();
      if (e.key === 'Enter' && focusInputId) onConfirm();
    };

    confirmBtn.addEventListener('click', onConfirm);
    cancelBtn.addEventListener('click', onCancel);
    overlay.addEventListener('click', onOverlayClick);
    document.addEventListener('keydown', onKeydown);

    if (focusInputId) {
      setTimeout(() => {
        const el = document.getElementById(focusInputId);
        if (el) el.focus();
      }, 50);
    }
  });
}

function showToast(message, variant = 'success') {
  const container = document.getElementById('app-toast-container');
  if (!container) return;
  const toast = document.createElement('div');
  toast.className = 'app-toast app-toast-' + variant;
  toast.textContent = message;
  container.appendChild(toast);
  setTimeout(() => {
    toast.classList.add('app-toast-hide');
    setTimeout(() => toast.remove(), 300);
  }, 3000);
}

// 3. Real-time Calculation Handler
function setupTableCalculationListeners() {
  const tableBody = document.getElementById('grades-table-body');
  if (!tableBody) return;

  const isSaudi = (document.getElementById('cert-select').value === 'saudi');

  if (isSaudi) {
    const saudiMultiContainer = document.getElementById('saudi-multi-tables-container');
    const cards = saudiMultiContainer.querySelectorAll('.saudi-year-card');

    cards.forEach(card => {
      attachSaudiRowListeners(card);
    });

    const aptitudeInput = document.getElementById('saudi-aptitude-score');
    if (aptitudeInput) {
      aptitudeInput.addEventListener('input', recalculateSaudi);
      aptitudeInput.addEventListener('change', recalculateSaudi);
    }

    recalculateSaudi();
  } else {
    const rows = tableBody.querySelectorAll('tr');

    const recalculateStandard = () => {
      rows.forEach(row => {
        const gradeInput = row.querySelector('.grade-input');
        const weightInput = row.querySelector('.weight-input');
        const achievedDisplay = row.querySelector('.col-achieved');

        if (gradeInput && weightInput && achievedDisplay) {
          const grade = parseFloat(gradeInput.value) || 0;
          const weight = parseFloat(weightInput.value) || 0;

          if (gradeInput.value !== '' && (grade < 0 || grade > 100)) {
            gradeInput.style.borderColor = 'var(--danger-color)';
          } else {
            gradeInput.style.borderColor = '';
          }

          if (weightInput.value !== '' && (weight < 0 || weight > 100)) {
            weightInput.style.borderColor = 'var(--danger-color)';
          } else {
            weightInput.style.borderColor = '';
          }

          const achieved = (grade * weight) / 100;
          achievedDisplay.textContent = achieved.toFixed(2);
        }
      });

      updateProgressIndicator();
    };

    rows.forEach(row => {
      const gradeInput = row.querySelector('.grade-input');
      const weightInput = row.querySelector('.weight-input');
      if (gradeInput && weightInput) {
        gradeInput.addEventListener('input', recalculateStandard);
        gradeInput.addEventListener('blur', recalculateStandard);
        weightInput.addEventListener('input', recalculateStandard);
        weightInput.addEventListener('blur', recalculateStandard);
      }
    });

    recalculateStandard();
  }
}

// 3b. IG Calculator Calculation and Helpers
const igPointsMapping = {
  "igcse-legacy": {
    "A_STAR": 8,
    "A": 7,
    "B": 6,
    "C": 5
  },
  "igcse-numeric": {
    "9": 8,
    "8": 7,
    "7": 6,
    "6": 5,
    "5": 4,
    "4": 3
  },
  "as-level": {
    "A": 5,
    "B": 4,
    "C": 3,
    "D": 2
  },
  "a-level": {
    "A_STAR": 6,
    "A": 5,
    "B": 4,
    "C": 3,
    "D": 2
  }
};

function setupIGCalculatorListeners() {
  const inputs = document.querySelectorAll('.ig-grade-input, #ig-factor-check, #ig-factor-val, #ig-sports-bonus');
  inputs.forEach(el => {
    el.addEventListener('change', calculateIGScore);
    el.addEventListener('input', calculateIGScore);
  });

  const factorCheck = document.getElementById('ig-factor-check');
  const factorContainer = document.getElementById('ig-factor-input-container');
  if (factorCheck && factorContainer) {
    factorCheck.addEventListener('change', function() {
      factorContainer.style.display = this.checked ? 'block' : 'none';
      calculateIGScore();
    });
  }
}

function resetIGCalculator() {
  document.querySelectorAll('.ig-grade-input').forEach(el => {
    el.value = '0';
  });
  const factorCheck = document.getElementById('ig-factor-check');
  if (factorCheck) factorCheck.checked = false;
  const factorVal = document.getElementById('ig-factor-val');
  if (factorVal) factorVal.value = '1.2';
  const factorContainer = document.getElementById('ig-factor-input-container');
  if (factorContainer) factorContainer.style.display = 'none';
  const sportsBonus = document.getElementById('ig-sports-bonus');
  if (sportsBonus) sportsBonus.value = '0';

  calculateIGScore();
}

function calculateIGScore() {
  const trackSelect = document.getElementById('track-select');
  if (!trackSelect) return;
  const trackVal = trackSelect.value;

  let subsystemKey = '';
  let maxPointVal = 8;

  if (trackVal.includes('IGCSE')) {
    subsystemKey = 'igcse';
    maxPointVal = 8;
  } else if (trackVal.includes('AS-Levels')) {
    subsystemKey = 'as-level';
    maxPointVal = 5;
  } else if (trackVal.includes('A-Levels')) {
    subsystemKey = 'a-level';
    maxPointVal = 6;
  } else {
    return;
  }

  let totalPoints = 0;
  let totalSubjects = 0;

  if (subsystemKey === 'igcse') {
    // Legacy
    document.querySelectorAll('.ig-grade-input[data-grade-type="igcse-legacy"]').forEach(el => {
      const count = parseInt(el.value) || 0;
      const grade = el.getAttribute('data-grade');
      const points = igPointsMapping['igcse-legacy'][grade] || 0;
      totalPoints += count * points;
      totalSubjects += count;
    });
    // Numeric
    document.querySelectorAll('.ig-grade-input[data-grade-type="igcse-numeric"]').forEach(el => {
      const count = parseInt(el.value) || 0;
      const grade = el.getAttribute('data-grade');
      const points = igPointsMapping['igcse-numeric'][grade] || 0;
      totalPoints += count * points;
      totalSubjects += count;
    });
  } else if (subsystemKey === 'as-level') {
    document.querySelectorAll('.ig-grade-input[data-grade-type="as-level"]').forEach(el => {
      const count = parseInt(el.value) || 0;
      const grade = el.getAttribute('data-grade');
      const points = igPointsMapping['as-level'][grade] || 0;
      totalPoints += count * points;
      totalSubjects += count;
    });
  } else if (subsystemKey === 'a-level') {
    document.querySelectorAll('.ig-grade-input[data-grade-type="a-level"]').forEach(el => {
      const count = parseInt(el.value) || 0;
      const grade = el.getAttribute('data-grade');
      const points = igPointsMapping['a-level'][grade] || 0;
      totalPoints += count * points;
      totalSubjects += count;
    });
  }

  const maxPoints = totalSubjects * maxPointVal;
  let scorePercentage = maxPoints > 0 ? (totalPoints / maxPoints) * 100 : 0;

  // Factor
  const factorCheck = document.getElementById('ig-factor-check');
  if (factorCheck && factorCheck.checked) {
    const factor = parseFloat(document.getElementById('ig-factor-val').value) || 1.2;
    scorePercentage *= factor;
  }

  // Sports Bonus
  const sportsBonus = parseFloat(document.getElementById('ig-sports-bonus').value) || 0;
  scorePercentage += sportsBonus;

  // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the equivalent
  // total (المجموع الاعتباري / المجموع المصري) must be derived from it. Mirrors StudentService.
  scorePercentage = Math.round(scorePercentage * 100) / 100;
  const governmentScore = (scorePercentage / 100) * 410;

  // Display Results
  document.getElementById('ig-percentage-val').textContent = scorePercentage.toFixed(2) + '%';
  document.getElementById('ig-gov-val').textContent = governmentScore.toFixed(2) + ' / 410';
}

// 3c. Kuwaiti Calculator (§1.3 of the certificate rules — mirrors StudentService.ProcessKuwaitiCertificate exactly)
// Max marks are fixed per subject (server-authoritative, taken from an official Kuwaiti Ministry of
// Education certificate sample) — the student only enters the obtained mark. The weight (%) each year
// contributes to the cumulative average is entered by the student themselves, since it is printed on
// their own certificate (e.g. 10% / 20% / 70%, or 20% / 80% when grade 10 wasn't studied).
const KUWAITI_EGYPTIAN_SCIENTIFIC_TOTAL = 410;

function generateKuwaitiGradesUI() {
  const container = document.getElementById('kuwaiti-grade-blocks');
  const yearsCountSelect = document.getElementById('kuwaiti-years-count');
  if (!container || !kuwaitiConfig || !yearsCountSelect) return;

  container.innerHTML = '';

  const yearsCount = yearsCountSelect.value;
  const includedLevels = getKuwaitiIncludedLevels();
  const isOneYear = yearsCount === 'One Year';

  const allBlocks = [
    { level: 10, label: 'الصف العاشر', subjects: kuwaitiConfig.grade_10 || [] },
    { level: 11, label: 'الصف الحادي عشر', subjects: kuwaitiConfig.grade_11 || [] },
    { level: 12, label: 'الصف الثاني عشر', subjects: kuwaitiConfig.grade_12 || [] }
  ];
  const blocks = allBlocks.filter(b => includedLevels.includes(b.level));

  blocks.forEach(block => {
    const card = document.createElement('div');
    card.className = 'saudi-year-card';
    card.style.cssText = 'background: var(--card-bg, #fff); border: 1px solid var(--border-color, #e2e8f0); border-radius: var(--radius-md, 8px); padding: 1.5rem; margin-bottom: 1.5rem; box-shadow: var(--shadow-sm, 0 1px 3px rgba(0,0,0,0.05));';

    let rowsHtml = '';
    block.subjects.forEach((subject, index) => {
      rowsHtml += `
        <tr>
          <td class="col-num">${index + 1}</td>
          <td class="col-subject">${subject.name}</td>
          <td class="col-grade">
            <input type="number" min="0" max="${subject.maxMark}" step="any" required placeholder="0-${subject.maxMark}"
                   class="table-input kuwaiti-obtained-input"
                   data-grade-level="${block.level}" data-subject="${subject.name}" data-max-mark="${subject.maxMark}">
          </td>
          <td class="col-weight">${subject.maxMark}</td>
        </tr>
      `;
    });

    const weightFieldHtml = isOneYear ? '' : `
      <div class="form-group" style="max-width: 260px;">
        <label for="kuwaiti-weight-${block.level}">نسبة ${block.label} من المعدل التراكمي (%) كما هي مدونة بالشهادة</label>
        <input type="number" id="kuwaiti-weight-${block.level}" class="form-control kuwaiti-weight-input"
               data-grade-level="${block.level}" min="0.01" max="100" step="any" placeholder="مثال: 70">
      </div>
    `;

    card.innerHTML = `
      <h3 class="saudi-year-title" style="margin-top: 0; margin-bottom: 1rem; color: var(--primary-color); font-size: 1.15rem; font-weight: 600; border-bottom: 2px solid var(--primary-light); padding-bottom: 0.5rem;">
        📚 ${block.label}
      </h3>
      ${weightFieldHtml}
      <div class="table-responsive">
        <table class="grades-table">
          <thead>
            <tr>
              <th class="col-num">#</th>
              <th class="col-subject">اسم المادة</th>
              <th class="col-grade">الدرجة المتحصلة</th>
              <th class="col-weight">الدرجة العظمى</th>
            </tr>
          </thead>
          <tbody>
            ${rowsHtml}
          </tbody>
        </table>
      </div>
      <div class="saudi-subtotal-bar" style="margin-top: 1rem; background: var(--light-bg, #f8fafc); border: 1px solid var(--border-color); padding: 0.75rem 1rem; border-radius: var(--radius-sm, 6px); font-size: 0.9rem; font-weight: 600;">
        نسبة ${block.label}: <span class="kuwaiti-grade-percentage" data-grade-level="${block.level}" style="color: var(--primary-color);">0.00%</span>
      </div>
    `;
    container.appendChild(card);
  });

  container.querySelectorAll('.kuwaiti-obtained-input, .kuwaiti-weight-input').forEach(input => {
    input.addEventListener('input', recalculateKuwaiti);
    input.addEventListener('change', recalculateKuwaiti);
  });

  recalculateKuwaiti();
}

function setupKuwaitiCalculatorListeners() {
  const yearsCountSelect = document.getElementById('kuwaiti-years-count');
  const secondAttempt = document.getElementById('kuwaiti-second-attempt');

  if (yearsCountSelect) {
    yearsCountSelect.addEventListener('change', generateKuwaitiGradesUI);
  }
  if (secondAttempt) {
    secondAttempt.addEventListener('change', recalculateKuwaiti);
  }
}

function kuwaitiYearsCountLabel(yearsCount) {
  if (yearsCount === 'One Year') return 'سنة واحدة';
  if (yearsCount === 'Three Years') return 'ثلاث سنوات';
  return 'سنتان';
}

function getKuwaitiIncludedLevels() {
  const yearsCountSelect = document.getElementById('kuwaiti-years-count');
  const yearsCount = yearsCountSelect ? yearsCountSelect.value : '';
  if (yearsCount === 'One Year') return [12];
  if (yearsCount === 'Three Years') return [10, 11, 12];
  return [11, 12];
}

function recalculateKuwaiti() {
  const container = document.getElementById('kuwaiti-grade-blocks');
  if (!container) return;

  const secondAttemptCheck = document.getElementById('kuwaiti-second-attempt');
  const secondAttemptAlert = document.getElementById('kuwaiti-second-attempt-alert');
  if (secondAttemptAlert) {
    secondAttemptAlert.style.display = (secondAttemptCheck && secondAttemptCheck.checked) ? 'flex' : 'none';
  }

  const includedLevels = getKuwaitiIncludedLevels();
  const isOneYear = includedLevels.length === 1;
  const percentages = {};
  const weights = {};

  includedLevels.forEach(level => {
    let totalObtained = 0;
    let totalMax = 0;

    container.querySelectorAll(`.kuwaiti-obtained-input[data-grade-level="${level}"]`).forEach(input => {
      const obtained = parseFloat(input.value) || 0;
      const maxMark = parseFloat(input.getAttribute('data-max-mark')) || 0;
      totalObtained += obtained;
      totalMax += maxMark;
    });

    const percentage = totalMax > 0 ? (totalObtained / totalMax) * 100 : 0;
    percentages[level] = percentage;

    const badge = container.querySelector(`.kuwaiti-grade-percentage[data-grade-level="${level}"]`);
    if (badge) badge.textContent = percentage.toFixed(2) + '%';

    if (isOneYear) {
      weights[level] = 100; // grade 12 alone carries 100% when only one year is studied
    } else {
      const weightInput = document.getElementById('kuwaiti-weight-' + level);
      weights[level] = weightInput ? (parseFloat(weightInput.value) || 0) : 0;
    }
  });

  const weightSum = includedLevels.reduce((sum, level) => sum + weights[level], 0);
  const weightSumAlert = document.getElementById('kuwaiti-weight-sum-alert');
  const weightsEntered = isOneYear || includedLevels.every(level => weights[level] > 0);
  if (weightSumAlert) {
    weightSumAlert.style.display = (!isOneYear && weightsEntered && Math.abs(weightSum - 100) > 0.01) ? 'flex' : 'none';
  }

  let finalPercentage = 0;
  if (weightsEntered && Math.abs(weightSum - 100) <= 0.01) {
    finalPercentage = includedLevels.reduce((sum, level) => sum + (percentages[level] * weights[level] / 100), 0);
  }
  // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the equivalent
  // total (المجموع الاعتباري / المجموع المصري) must be derived from it. Mirrors StudentService.
  finalPercentage = Math.round(finalPercentage * 100) / 100;
  const equivalentTotal = (finalPercentage / 100) * KUWAITI_EGYPTIAN_SCIENTIFIC_TOTAL;

  const finalEl = document.getElementById('kuwaiti-final-percentage');
  const totalEl = document.getElementById('kuwaiti-equivalent-total');
  if (finalEl) finalEl.textContent = finalPercentage.toFixed(2) + '%';
  if (totalEl) totalEl.textContent = equivalentTotal.toFixed(2) + ' / 410 (' + finalPercentage.toFixed(2) + '%)';

  updateProgressIndicator();
}

// 3d. Single-year fixed-total Calculator (shared by Qatari, Omani and Yemeni — mirrors
// StudentService's shared ProcessSingleYearFixedTotalCertificate exactly). Single grade level, max
// mark fixed at 100 per subject. The subject list, denominator and excluded subject (if any) are
// all read from that certificate's /api/config/subjects-* response — never hardcoded here — so a
// cert with a different subject count (Yemeni: 6 subjects / 600) or no excluded subject (Yemeni has
// none) is handled by the exact same functions as Qatari/Omani.
// Bahraini's subject list depends on the selected track (علمي/أدبي) — set by generateBahrainiGradesUI
// before getSingleYearConfig('bahraini') is consulted by the shared single-year-fixed-total functions.
let bahrainiSelectedTrack = '';

function getSingleYearConfig(prefix) {
  if (prefix === 'bahraini') {
    if (!bahrainiConfig) return null;
    let subjects = [];
    if (bahrainiSelectedTrack === bahrainiConfig.scientific_track_name) {
      subjects = bahrainiConfig.scientific;
    } else if (bahrainiSelectedTrack === bahrainiConfig.literary_track_name) {
      subjects = bahrainiConfig.literary;
    }
    return {
      subjects: subjects,
      max_mark_per_subject: bahrainiConfig.max_mark_per_subject,
      total_max: subjects.length * bahrainiConfig.max_mark_per_subject,
      excluded_subject: null
    };
  }
  if (prefix === 'qatari') {
    if (!qatariConfig) return null;
    return {
      subjects: qatariConfig.scientific,
      max_mark_per_subject: qatariConfig.max_mark_per_subject,
      total_max: qatariConfig.total_max,
      excluded_subject: qatariConfig.excluded_subject
    };
  }
  if (prefix === 'omani') {
    if (!omaniConfig) return null;
    return {
      subjects: omaniConfig.subjects,
      max_mark_per_subject: omaniConfig.max_mark_per_subject,
      total_max: omaniConfig.total_max,
      excluded_subject: omaniConfig.excluded_subject
    };
  }
  if (prefix === 'yemeni') {
    if (!yemeniConfig) return null;
    return {
      subjects: yemeniConfig.subjects,
      max_mark_per_subject: yemeniConfig.max_mark_per_subject,
      total_max: yemeniConfig.total_max,
      excluded_subject: yemeniConfig.excluded_subject
    };
  }
  return null;
}

// Builds the flat subject-mark table for a given prefix ('qatari', 'omani' or 'yemeni') into
// `${prefix}-subjects-body`, wiring live recalculation.
function generateSingleYearFixedTotalGradesUI(prefix) {
  const tbody = document.getElementById(prefix + '-subjects-body');
  const config = getSingleYearConfig(prefix);
  if (!tbody || !config) return;

  const maxMark = config.max_mark_per_subject;
  tbody.innerHTML = '';
  (config.subjects || []).forEach((subjectName, index) => {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td class="col-num">${index + 1}</td>
      <td class="col-subject">${subjectName}</td>
      <td class="col-grade">
        <input type="number" min="0" max="${maxMark}" step="any" required placeholder="0-${maxMark}"
               class="table-input ${prefix}-mark-input" data-subject="${subjectName}">
      </td>
      <td class="col-weight">${maxMark}</td>
    `;
    tbody.appendChild(row);
  });

  tbody.querySelectorAll('.' + prefix + '-mark-input').forEach(input => {
    input.addEventListener('input', () => recalculateSingleYearFixedTotal(prefix));
    input.addEventListener('change', () => recalculateSingleYearFixedTotal(prefix));
  });

  recalculateSingleYearFixedTotal(prefix);
}

function setupSingleYearFixedTotalListeners(prefix) {
  [prefix + '-printed-total', prefix + '-printed-percentage'].forEach(id => {
    const el = document.getElementById(id);
    if (el) {
      el.addEventListener('input', () => recalculateSingleYearFixedTotal(prefix));
      el.addEventListener('change', () => recalculateSingleYearFixedTotal(prefix));
    }
  });
}

function recalculateSingleYearFixedTotal(prefix) {
  const tbody = document.getElementById(prefix + '-subjects-body');
  const config = getSingleYearConfig(prefix);
  if (!tbody || !config) return;

  let finalTotal = 0;
  tbody.querySelectorAll('.' + prefix + '-mark-input').forEach(input => {
    finalTotal += parseFloat(input.value) || 0;
  });

  const totalMax = config.total_max;
  const rawPercentage = totalMax > 0 ? (finalTotal / totalMax) * 100 : 0;
  // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the equivalent
  // total (المجموع الاعتباري / المجموع المصري) must be derived from it, mirroring StudentService.
  const percentage = Math.round(rawPercentage * 100) / 100;

  const totalEl = document.getElementById(prefix + '-final-total');
  const percentageEl = document.getElementById(prefix + '-percentage');
  if (totalEl) totalEl.textContent = finalTotal.toFixed(2) + ' / ' + totalMax;
  if (percentageEl) percentageEl.textContent = percentage.toFixed(2) + '%';

  // Qatari, Omani, Bahraini and Yemeni all render an equivalent-total element.
  const equivalentEl = document.getElementById(prefix + '-equivalent-total');
  if (equivalentEl) {
    const equivalentTotal = (percentage / 100) * 410;
    equivalentEl.textContent = equivalentTotal.toFixed(2) + ' / 410';
  }

  const printedTotalInput = document.getElementById(prefix + '-printed-total');
  const printedPercentageInput = document.getElementById(prefix + '-printed-percentage');
  const noteEl = document.getElementById(prefix + '-comparison-note');
  if (noteEl) {
    const printedTotal = printedTotalInput ? parseFloat(printedTotalInput.value) : NaN;
    const printedPercentage = printedPercentageInput ? parseFloat(printedPercentageInput.value) : NaN;
    if (!isNaN(printedTotal) && !isNaN(printedPercentage)) {
      const totalDiff = printedTotal - finalTotal;
      const percentageDiff = printedPercentage - percentage;
      if (Math.abs(totalDiff) <= 0.01 && Math.abs(percentageDiff) <= 0.01) {
        noteEl.textContent = '';
      } else if (config.excluded_subject) {
        // e.g. Qatari/Omani — a gap is expected because the excluded subject is on the certificate.
        noteEl.textContent = `المجموع المطبوع على الشهادة (${printedTotal}) يشمل مادة ${config.excluded_subject}. ` +
          `الفرق عن المجموع المحتسب هنا هو ${totalDiff.toFixed(2)} درجة (${percentageDiff.toFixed(2)}%) ` +
          `بسبب استبعاد هذه المادة من حساب المعادلة.`;
      } else {
        // e.g. Yemeni — nothing is excluded, so a mismatch likely means a data-entry mistake.
        noteEl.textContent = `تنبيه: المجموع المطبوع على الشهادة (${printedTotal}) يختلف عن المجموع المحتسب هنا ` +
          `بمقدار ${totalDiff.toFixed(2)} درجة (${percentageDiff.toFixed(2)}%) — يرجى التأكد من صحة الدرجات المدخلة.`;
      }
    } else {
      noteEl.textContent = '';
    }
  }

  updateProgressIndicator();
}

// Qatari-specific: only المسار العلمي has a defined subject list; other tracks are blocked.
function generateQatariGradesUI(trackVal) {
  const blockedAlert = document.getElementById('qatari-track-blocked-alert');
  const gradeBlock = document.getElementById('qatari-grade-block');
  if (!blockedAlert || !gradeBlock || !qatariConfig) return;

  const isScientific = trackVal === qatariConfig.scientific_track_name;

  if (!isScientific) {
    blockedAlert.style.display = 'flex';
    gradeBlock.style.display = 'none';
    return;
  }

  blockedAlert.style.display = 'none';
  gradeBlock.style.display = 'block';

  generateSingleYearFixedTotalGradesUI('qatari');
}

function setupQatariCalculatorListeners() {
  setupSingleYearFixedTotalListeners('qatari');
}

// Omani-specific: single track, always rendered.
function generateOmaniGradesUI() {
  if (!omaniConfig) return;
  generateSingleYearFixedTotalGradesUI('omani');
}

function setupOmaniCalculatorListeners() {
  setupSingleYearFixedTotalListeners('omani');
}

// Yemeni-specific: single track, always rendered, no excluded subject.
function generateYemeniGradesUI() {
  if (!yemeniConfig) return;
  generateSingleYearFixedTotalGradesUI('yemeni');
}

function setupYemeniCalculatorListeners() {
  setupSingleYearFixedTotalListeners('yemeni');
}

// Bahraini-specific: track-dependent subject list. المسار العلمي is grouped by semester (الفصل
// 3/4/5/6 — 30 subject rows total, real course-code data with intentional duplicates); المسار
// الأدبي uses the shared flat single-year renderer (8 unique subjects, no semester grouping data
// published yet). مهني/فني has no defined subject list and is blocked, mirroring the Qatari pattern.
function generateBahrainiGradesUI(trackVal) {
  const blockedAlert = document.getElementById('bahraini-track-blocked-alert');
  const gradeBlock = document.getElementById('bahraini-grade-block');
  if (!blockedAlert || !gradeBlock || !bahrainiConfig) return;

  bahrainiSelectedTrack = trackVal;
  const isSupported = trackVal === bahrainiConfig.scientific_track_name || trackVal === bahrainiConfig.literary_track_name;

  if (!isSupported) {
    blockedAlert.style.display = 'flex';
    gradeBlock.style.display = 'none';
    return;
  }

  blockedAlert.style.display = 'none';
  gradeBlock.style.display = 'block';

  if (trackVal === bahrainiConfig.scientific_track_name && bahrainiConfig.scientific_semesters) {
    generateBahrainiScientificGradesUI();
  } else {
    generateSingleYearFixedTotalGradesUI('bahraini');
  }
}

// Renders المسار العلمي's subject table grouped by semester, with a bold separator row per
// semester label. Rows still use the shared '.bahraini-mark-input' class + data-subject attribute,
// so recalculateSingleYearFixedTotal / validateSingleYearFixedTotalMarks / collectSingleYearFixedTotalPayload
// all keep working unchanged — they just read every '.bahraini-mark-input' regardless of grouping.
function generateBahrainiScientificGradesUI() {
  const tbody = document.getElementById('bahraini-subjects-body');
  const semesters = bahrainiConfig.scientific_semesters;
  const maxMark = bahrainiConfig.max_mark_per_subject;
  if (!tbody || !semesters) return;

  tbody.innerHTML = '';
  let rowNum = 0;

  Object.keys(semesters).forEach(semesterLabel => {
    const headerRow = document.createElement('tr');
    headerRow.className = 'grades-group-header';
    headerRow.innerHTML = `<td colspan="4">${semesterLabel}</td>`;
    tbody.appendChild(headerRow);

    (semesters[semesterLabel] || []).forEach(course => {
      rowNum += 1;
      const subjectName = course.subjectName;
      const row = document.createElement('tr');
      row.innerHTML = `
        <td class="col-num">${rowNum}</td>
        <td class="col-subject"><span class="course-code">${course.code}</span> ${subjectName}</td>
        <td class="col-grade">
          <input type="number" min="0" max="${maxMark}" step="any" required placeholder="0-${maxMark}"
                 class="table-input bahraini-mark-input" data-subject="${subjectName}">
        </td>
        <td class="col-weight">${maxMark}</td>
      `;
      tbody.appendChild(row);
    });
  });

  tbody.querySelectorAll('.bahraini-mark-input').forEach(input => {
    input.addEventListener('input', () => recalculateSingleYearFixedTotal('bahraini'));
    input.addEventListener('change', () => recalculateSingleYearFixedTotal('bahraini'));
  });

  recalculateSingleYearFixedTotal('bahraini');
}

function setupBahrainiCalculatorListeners() {
  setupSingleYearFixedTotalListeners('bahraini');
}

// 3f. Palestinian Tawjihi Calculator — percentage-in only, no subjects/grades grid. Mirrors
// StudentService.ProcessPalestinianCertificate: the typed percentage is rounded to 2dp and
// converted via (percentage / 100) * 410.
function recalculatePalestinian() {
  const input = document.getElementById('palestinian-percentage');
  const percentageEl = document.getElementById('palestinian-percentage-val');
  const equivalentEl = document.getElementById('palestinian-equivalent-total');
  if (!input) return;

  const raw = parseFloat(input.value);
  const valid = input.value !== '' && !isNaN(raw) && raw >= 0 && raw <= 100;
  input.style.borderColor = (input.value !== '' && !valid) ? 'var(--danger-color)' : '';

  const percentage = valid ? Math.round(raw * 100) / 100 : 0;
  const equivalentTotal = (percentage / 100) * 410;

  if (percentageEl) percentageEl.textContent = percentage.toFixed(2) + '%';
  if (equivalentEl) equivalentEl.textContent = equivalentTotal.toFixed(2) + ' / 410';

  updateProgressIndicator();
}

function setupPalestinianCalculatorListeners() {
  const input = document.getElementById('palestinian-percentage');
  if (input) {
    input.addEventListener('input', recalculatePalestinian);
    input.addEventListener('change', recalculatePalestinian);
  }
}

// 3g. "أخرى" (Other) Calculator — percentage-in only, free-text certificate name, no track. The
// certificate name never affects the number.
function recalculateOther() {
  const input = document.getElementById('other-percentage');
  const percentageEl = document.getElementById('other-percentage-val');
  if (!input) return;

  const raw = parseFloat(input.value);
  const valid = input.value !== '' && !isNaN(raw) && raw >= 0 && raw <= 100;
  input.style.borderColor = (input.value !== '' && !valid) ? 'var(--danger-color)' : '';

  const percentage = valid ? Math.round(raw * 100) / 100 : 0;

  if (percentageEl) percentageEl.textContent = percentage.toFixed(2) + '%';

  updateProgressIndicator();
}

function setupOtherCalculatorListeners() {
  const input = document.getElementById('other-percentage');
  if (input) {
    input.addEventListener('input', recalculateOther);
    input.addEventListener('change', recalculateOther);
  }
}

// 3h. Egyptian Thanaweya Amma Calculator — track (from the shared track-select) + a nested
// "نظام المواد" select together determine the exact subject set and each subject's fixed max
// mark (mirrors StudentService.ProcessEgyptianCertificate / EgyptianConstants exactly). The
// denominator is fixed by subject system alone (320 حديث / 410 قديم) — never derived from the
// sum of the visible fields' own max marks.
function generateEgyptianTrackUI(trackVal) {
  const systemGroup = document.getElementById('egyptian-system-group');
  const systemSelect = document.getElementById('egyptian-system-select');
  const subjectsBlock = document.getElementById('egyptian-subjects-block');
  if (!systemGroup || !systemSelect || !subjectsBlock) return;

  systemSelect.value = '';
  systemGroup.style.display = trackVal ? 'block' : 'none';
  subjectsBlock.style.display = 'none';
  document.getElementById('egyptian-subjects-body').innerHTML = '';

  recalculateEgyptian();
}

function generateEgyptianSubjectsUI() {
  const trackSelect = document.getElementById('track-select');
  const systemSelect = document.getElementById('egyptian-system-select');
  const subjectsBlock = document.getElementById('egyptian-subjects-block');
  const tbody = document.getElementById('egyptian-subjects-body');
  if (!trackSelect || !systemSelect || !subjectsBlock || !tbody) return;

  const track = trackSelect.value;
  const system = systemSelect.value;

  if (!track || !system || !egyptianConfig) {
    subjectsBlock.style.display = 'none';
    tbody.innerHTML = '';
    recalculateEgyptian();
    return;
  }

  const subjects = (egyptianConfig.subjects_by_track_and_system[track] || {})[system] || [];
  tbody.innerHTML = '';
  subjects.forEach((subject, idx) => {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td class="col-num">${idx + 1}</td>
      <td class="col-subject">${subject.name}</td>
      <td class="col-grade">
        <input type="number" min="0" max="${subject.maxMark}" step="any" required placeholder="0-${subject.maxMark}"
               class="table-input egyptian-mark-input" data-subject="${subject.name}" data-max="${subject.maxMark}">
      </td>
      <td class="col-weight">${subject.maxMark}</td>
    `;
    tbody.appendChild(row);
  });

  subjectsBlock.style.display = 'block';

  tbody.querySelectorAll('.egyptian-mark-input').forEach(input => {
    input.addEventListener('input', recalculateEgyptian);
    input.addEventListener('change', recalculateEgyptian);
  });

  recalculateEgyptian();
}

function recalculateEgyptian() {
  const systemSelect = document.getElementById('egyptian-system-select');
  const system = systemSelect ? systemSelect.value : '';
  const denominator = egyptianConfig && system
    ? (system === egyptianConfig.new_system_name ? egyptianConfig.denominators.new_system : egyptianConfig.denominators.old_system)
    : 0;

  let finalTotal = 0;
  document.querySelectorAll('.egyptian-mark-input').forEach(input => {
    finalTotal += parseFloat(input.value) || 0;
  });

  const rawPercentage = denominator > 0 ? (finalTotal / denominator) * 100 : 0;
  const percentage = Math.round(rawPercentage * 100) / 100;

  const totalEl = document.getElementById('egyptian-final-total');
  const percentageEl = document.getElementById('egyptian-percentage');
  if (totalEl) totalEl.textContent = finalTotal.toFixed(2) + ' / ' + denominator;
  if (percentageEl) percentageEl.textContent = percentage.toFixed(2) + '%';

  updateProgressIndicator();
}

function setupEgyptianCalculatorListeners() {
  const systemSelect = document.getElementById('egyptian-system-select');
  if (systemSelect) {
    systemSelect.addEventListener('change', generateEgyptianSubjectsUI);
  }
}

// 3i. Azhar Thanaweya Calculator — القسم (from the shared track-select) selects a fixed subject
// list directly (mirrors StudentService.ProcessAzharCertificate / AzharConstants exactly). No
// secondary system select, unlike Egyptian. المجموع الاعتباري uses the same (Percentage × 4.1)
// formula as every other foreign certificate.
function generateAzharGradesUI(sectionVal) {
  const tbody = document.getElementById('azhar-subjects-body');
  if (!tbody) return;

  if (!sectionVal || !azharConfig) {
    tbody.innerHTML = '';
    recalculateAzhar();
    return;
  }

  const subjects = azharConfig.subjects_by_section[sectionVal] || [];
  tbody.innerHTML = '';
  subjects.forEach((subject, idx) => {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td class="col-num">${idx + 1}</td>
      <td class="col-subject">${subject.name}</td>
      <td class="col-grade">
        <input type="number" min="0" max="${subject.maxMark}" step="any" required placeholder="0-${subject.maxMark}"
               class="table-input azhar-mark-input" data-subject="${subject.name}" data-max="${subject.maxMark}">
      </td>
      <td class="col-weight">${subject.maxMark}</td>
    `;
    tbody.appendChild(row);
  });

  tbody.querySelectorAll('.azhar-mark-input').forEach(input => {
    input.addEventListener('input', recalculateAzhar);
    input.addEventListener('change', recalculateAzhar);
  });

  recalculateAzhar();
}

function recalculateAzhar() {
  const trackSelect = document.getElementById('track-select');
  const sectionVal = trackSelect ? trackSelect.value : '';
  const denominator = azharConfig && sectionVal ? (azharConfig.denominators[sectionVal] || 0) : 0;

  let finalTotal = 0;
  document.querySelectorAll('.azhar-mark-input').forEach(input => {
    finalTotal += parseFloat(input.value) || 0;
  });

  const rawPercentage = denominator > 0 ? (finalTotal / denominator) * 100 : 0;
  const percentage = Math.round(rawPercentage * 100) / 100;
  const equivalentTotal = percentage * 4.1;

  const totalEl = document.getElementById('azhar-final-total');
  const percentageEl = document.getElementById('azhar-percentage');
  const equivalentEl = document.getElementById('azhar-equivalent-total');
  if (totalEl) totalEl.textContent = finalTotal.toFixed(2) + ' / ' + denominator;
  if (percentageEl) percentageEl.textContent = percentage.toFixed(2) + '%';
  if (equivalentEl) equivalentEl.textContent = equivalentTotal.toFixed(2) + ' / 410';

  updateProgressIndicator();
}

function setupAzharCalculatorListeners() {
  // Subject rows are generated dynamically by generateAzharGradesUI (called from the track-select
  // trigger in conditional.js), which wires up its own input listeners — nothing static to bind here.
}

// 3j. Emirati Calculator — single track today (no track-selection UI). Core subjects (5) are
// always required; optional subjects (الكيمياء/العلوم الصحية/الأحياء) are counted — added to BOTH
// the numerator and the denominator — only if the student fills them in (mirrors
// StudentService.ProcessEmiratiCertificate exactly). المجموع الاعتباري = (Percentage / 100) × 410.
function generateEmiratiGradesUI() {
  const tbody = document.getElementById('emirati-subjects-body');
  if (!tbody || !emiratiConfig) return;

  const maxMark = emiratiConfig.max_mark_per_subject;
  tbody.innerHTML = '';
  let rowNum = 0;

  (emiratiConfig.core_subjects || []).forEach(subjectName => {
    rowNum += 1;
    const row = document.createElement('tr');
    row.innerHTML = `
      <td class="col-num">${rowNum}</td>
      <td class="col-subject">${subjectName}</td>
      <td class="col-grade">
        <input type="number" min="0" max="${maxMark}" step="any" required placeholder="0-${maxMark}"
               class="table-input emirati-mark-input" data-subject="${subjectName}">
      </td>
      <td class="col-weight">${maxMark}</td>
    `;
    tbody.appendChild(row);
  });

  (emiratiConfig.optional_subjects || []).forEach(subjectName => {
    rowNum += 1;
    const row = document.createElement('tr');
    row.innerHTML = `
      <td class="col-num">${rowNum}</td>
      <td class="col-subject"><span class="optional-badge">اختياري</span> ${subjectName}</td>
      <td class="col-grade">
        <input type="number" min="0" max="${maxMark}" step="any" placeholder="اختياري"
               class="table-input emirati-mark-input emirati-optional-input" data-subject="${subjectName}">
      </td>
      <td class="col-weight">${maxMark}</td>
    `;
    tbody.appendChild(row);
  });

  tbody.querySelectorAll('.emirati-mark-input').forEach(input => {
    input.addEventListener('input', recalculateEmirati);
    input.addEventListener('change', recalculateEmirati);
  });

  recalculateEmirati();
  if (typeof updateEmiratiMedicalWarning === 'function') {
    updateEmiratiMedicalWarning();
  }
}

// Only inputs with a non-empty value are counted — an empty optional field simply means "not on
// my certificate", not zero. The denominator therefore varies (500-800) instead of being fixed.
function recalculateEmirati() {
  let finalTotal = 0;
  let countedSubjects = 0;

  document.querySelectorAll('.emirati-mark-input').forEach(input => {
    if (input.value === '') return;
    const val = parseFloat(input.value);
    if (isNaN(val)) return;
    finalTotal += val;
    countedSubjects += 1;
  });

  const maxMark = (emiratiConfig && emiratiConfig.max_mark_per_subject) || 100;
  const denominator = countedSubjects * maxMark;
  const percentage = denominator > 0 ? (finalTotal / denominator) * 100 : 0;
  const equivalentTotal = (percentage / 100) * 410;

  const totalEl = document.getElementById('emirati-final-total');
  const percentageEl = document.getElementById('emirati-percentage');
  const equivalentEl = document.getElementById('emirati-equivalent-total');
  if (totalEl) totalEl.textContent = finalTotal.toFixed(2) + ' / ' + denominator;
  if (percentageEl) percentageEl.textContent = percentage.toFixed(2) + '%';
  if (equivalentEl) equivalentEl.textContent = equivalentTotal.toFixed(2) + ' / 410';

  updateProgressIndicator();
}

function setupEmiratiCalculatorListeners() {
  // Subject rows are generated once by generateEmiratiGradesUI (called directly from the
  // cert-select handler in conditional.js, since this cert has no track-selection step), which
  // wires up its own input listeners — nothing static to bind here.
}

// 3k. American Diploma Calculator — no track selector at all (mirrors "أخرى"/Emirati). Renders 8
// "best subject" rows (no fixed names), computes the GPA-based BasePercentage (§2), and wires SAT
// I/II inputs to the live, non-blocking admission-minimum warnings (§6, defined in conditional.js
// since they're driven by the same Wish-college config as refreshAmericanDiplomaSatIIFields).
function generateAmericanDiplomaGradesUI() {
  const tbody = document.getElementById('american-diploma-subjects-body');
  if (!tbody || !americanDiplomaConfig) return;

  const maxMark = americanDiplomaConfig.max_mark_per_subject;
  tbody.innerHTML = '';

  for (let i = 1; i <= americanDiplomaConfig.best_subjects_count; i++) {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td class="col-num">${i}</td>
      <td class="col-subject">
        <input type="text" maxlength="100" required placeholder="اسم المادة"
               class="table-input american-diploma-subject-input">
      </td>
      <td class="col-grade">
        <input type="number" min="0" max="${maxMark}" step="any" required placeholder="0-${maxMark}"
               class="table-input american-diploma-mark-input">
      </td>
      <td class="col-weight">${maxMark}</td>
    `;
    tbody.appendChild(row);
  }

  tbody.querySelectorAll('.american-diploma-mark-input').forEach(input => {
    input.addEventListener('input', recalculateAmericanDiploma);
    input.addEventListener('change', recalculateAmericanDiploma);
  });

  recalculateAmericanDiploma();
  if (typeof refreshAmericanDiplomaSatIIFields === 'function') {
    refreshAmericanDiplomaSatIIFields();
  }
}

// §2 — المعدل = مجموع أفضل 8 مواد ÷ 8؛ النسبة الأساسية = المعدل × 40 ÷ 100 (من 40، وليس من 100 —
// مطابق تمامًا لصيغة الباك اند في ProcessAmericanDiplomaCertificate).
function recalculateAmericanDiploma() {
  const inputs = document.querySelectorAll('.american-diploma-mark-input');
  let sum = 0;
  inputs.forEach(input => {
    sum += parseFloat(input.value) || 0;
  });

  const count = (americanDiplomaConfig && americanDiplomaConfig.best_subjects_count) || inputs.length || 1;
  const average = sum / count;
  const weight = (americanDiplomaConfig && americanDiplomaConfig.base_percentage_weight) || 40;
  const basePercentage = (average * weight) / 100;

  const averageEl = document.getElementById('american-diploma-average');
  const baseEl = document.getElementById('american-diploma-base-percentage');
  if (averageEl) averageEl.textContent = average.toFixed(2);
  if (baseEl) baseEl.textContent = basePercentage.toFixed(2) + ' / ' + weight;

  updateProgressIndicator();
}

function setupAmericanDiplomaCalculatorListeners() {
  const sat1 = document.getElementById('american-diploma-sat1');
  const sat2 = document.getElementById('american-diploma-sat2');
  if (sat1) {
    sat1.addEventListener('input', () => {
      if (typeof updateAmericanDiplomaWarnings === 'function') updateAmericanDiplomaWarnings();
    });
  }
  if (sat2) {
    sat2.addEventListener('input', () => {
      if (typeof updateAmericanDiplomaWarnings === 'function') updateAmericanDiplomaWarnings();
    });
  }
  // "Studied advanced math" only ever flips the engineering group's SAT II mandatory/optional
  // state — re-run the same college-driven refresh so the label/hint/required attribute update live.
  const advancedMathCheckbox = document.getElementById('american-diploma-advanced-math-checkbox');
  if (advancedMathCheckbox) {
    advancedMathCheckbox.addEventListener('change', () => {
      if (typeof refreshAmericanDiplomaSatIIFields === 'function') refreshAmericanDiplomaSatIIFields();
    });
  }
  // Subject rows are generated once by generateAmericanDiplomaGradesUI (called directly from the
  // cert-select handler in conditional.js, since this cert has no track-selection step), which
  // wires up its own input listeners.
}

// 4. Form Submission and Validation
function setupSubmissionHandler() {
  const mainForm = document.getElementById('student-reg-form');
  const submitBtn = document.getElementById('btn-submit');

  mainForm.addEventListener('submit', function(e) {
    e.preventDefault();
    hideAlert('form-alert');

    // Perform validation
    const validationResult = validateForm();
    if (!validationResult.valid) {
      showAlert('form-alert', validationResult.message, 'danger');
      if (validationResult.element) {
        validationResult.element.scrollIntoView({ behavior: 'smooth', block: 'center' });
        validationResult.element.focus();
      }
      return;
    }

    // Prepare payload
    const payload = compilePayload();

    // Show loading state
    submitBtn.disabled = true;
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = 'جاري الحفظ وإرسال البيانات...';

    // Submit payload
    sendData(payload, submitBtn, originalText);
  });
}

// Full Form Fields Validation
// Shared by Qatari, Omani and Yemeni: validates the flat subject-mark table + optional printed
// total/percentage documentation fields for a given prefix.
function validateSingleYearFixedTotalMarks(prefix, missingTableMessage, fallbackElement) {
  const markInputs = document.querySelectorAll('.' + prefix + '-mark-input');
  if (markInputs.length === 0) {
    return {
      valid: false,
      message: missingTableMessage,
      element: fallbackElement
    };
  }

  for (let i = 0; i < markInputs.length; i++) {
    const markVal = parseFloat(markInputs[i].value);
    if (isNaN(markVal) || markVal < 0 || markVal > 100) {
      return {
        valid: false,
        message: 'الرجاء إدخال درجة صحيحة (بين 0 و100) لجميع المواد.',
        element: markInputs[i]
      };
    }
  }

  const config = getSingleYearConfig(prefix);
  const totalMax = config ? config.total_max : 100 * markInputs.length;

  const printedTotalInput = document.getElementById(prefix + '-printed-total');
  if (printedTotalInput && printedTotalInput.value !== '' && (isNaN(parseFloat(printedTotalInput.value)) || parseFloat(printedTotalInput.value) < 0 || parseFloat(printedTotalInput.value) > totalMax)) {
    return {
      valid: false,
      message: `المجموع المطبوع على الشهادة يجب أن يكون بين 0 و${totalMax}.`,
      element: printedTotalInput
    };
  }

  const printedPercentageInput = document.getElementById(prefix + '-printed-percentage');
  if (printedPercentageInput && printedPercentageInput.value !== '' && (isNaN(parseFloat(printedPercentageInput.value)) || parseFloat(printedPercentageInput.value) < 0 || parseFloat(printedPercentageInput.value) > 100)) {
    return {
      valid: false,
      message: 'النسبة المطبوعة على الشهادة يجب أن تكون بين 0 و100.',
      element: printedPercentageInput
    };
  }

  return { valid: true };
}

function validateForm() {
  // 1. Photo Upload
  if (!uploadedPhotoBase64) {
    return {
      valid: false,
      message: 'الرجاء إضافة الصورة الشخصية للطالب قبل الإرسال.',
      element: document.getElementById('photo-input')
    };
  }

  // 2. Full Name
  const studentNameArInput = document.getElementById('student-name-ar');
  if (!studentNameArInput.value.trim()) {
    return {
      valid: false,
      message: 'الرجاء إدخال الاسم بالكامل باللغة العربية.',
      element: studentNameArInput
    };
  }
  if (studentNameArInput.value.length > 100) {
    return {
      valid: false,
      message: 'يجب ألا يزيد اسم الطالب عن 100 حرف.',
      element: studentNameArInput
    };
  }

  const studentNameEnInput = document.getElementById('student-name-en');
  if (!studentNameEnInput.value.trim()) {
    return {
      valid: false,
      message: 'الرجاء إدخال الاسم بالكامل باللغة الانجليزية.',
      element: studentNameEnInput
    };
  }

  // Personal Info & Address Validation
  const requiredFields = [
    { id: 'wish-college', name: 'الكلية' },
    { id: 'graduation-year', name: 'سنة التخرج' },
    { id: 'student-gender', name: 'النوع' },
    { id: 'student-phone', name: 'رقم هاتف الطالب' },
    { id: 'student-email', name: 'ايميل الشخصي للطالب' },
    { id: 'guardian-name', name: 'اسم ولي الامر' },
    { id: 'guardian-national-id', name: 'الرقم القومي لولي الامر' },
    { id: 'guardian-occupation', name: 'وظيفة ولي الامر' },
    { id: 'guardian-phone', name: 'رقم هاتف ولي الامر' },
    { id: 'guardian-relation', name: 'صلة قرابة ولي الامر' },
    { id: 'address-gov', name: 'المحافظه' },
    { id: 'address-center', name: 'المركز' },
    { id: 'address-street', name: 'شارع' },
    { id: 'address-building', name: 'رقم العماره' }
  ];

  for (let field of requiredFields) {
    const el = document.getElementById(field.id);
    if (!el.value.trim()) {
      return {
        valid: false,
        message: 'الرجاء إدخال ' + field.name + '.',
        element: el
      };
    }
  }

  // Wish Program (only required when the selected college has a program list)
  const wishProgramInput = document.getElementById('wish-program');
  if (!wishProgramInput.disabled && wishProgramInput.closest('.form-group').style.display !== 'none' && !wishProgramInput.value.trim()) {
    return {
      valid: false,
      message: 'الرجاء اختيار البرنامج.',
      element: wishProgramInput
    };
  }

  // Email format check
  const studentEmailInput = document.getElementById('student-email');
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailPattern.test(studentEmailInput.value.trim())) {
    return {
      valid: false,
      message: 'الرجاء إدخال بريد إلكتروني صحيح.',
      element: studentEmailInput
    };
  }

  // 3. National ID
  const nationalIdInput = document.getElementById('national-id');
  if (!nationalIdInput.value.trim()) {
    return {
      valid: false,
      message: 'الرجاء إدخال الرقم القومي.',
      element: nationalIdInput
    };
  }
  // Standard digits & letters length validation (e.g. between 8 and 20)
  if (nationalIdInput.value.length < 8 || nationalIdInput.value.length > 20) {
    return {
      valid: false,
      message: 'الرجاء إدخال رقم قومي صحيح (بين 8 و 20 خانة).',
      element: nationalIdInput
    };
  }

  const guardianNationalIdInput = document.getElementById('guardian-national-id');
  if (guardianNationalIdInput.value.length < 8 || guardianNationalIdInput.value.length > 20) {
    return {
      valid: false,
      message: 'الرجاء إدخال رقم قومي صحيح لولي الأمر (بين 8 و 20 خانة).',
      element: guardianNationalIdInput
    };
  }

  // 4. Certification
  const certSelect = document.getElementById('cert-select');
  if (!certSelect.value) {
    return {
      valid: false,
      message: 'الرجاء اختيار نوع الشهادة المعادلة.',
      element: certSelect
    };
  }

  // "أخرى" has NO track selector at all — validate its own two fields and return early,
  // bypassing the generic track requirement below.
  if (certSelect.value === 'other') {
    const nameInput = document.getElementById('other-certificate-name');
    if (!nameInput.value.trim()) {
      return {
        valid: false,
        message: 'الرجاء إدخال اسم الشهادة.',
        element: nameInput
      };
    }
    const percentageInput = document.getElementById('other-percentage');
    const val = parseFloat(percentageInput.value);
    if (percentageInput.value === '' || isNaN(val) || val < 0 || val > 100) {
      return {
        valid: false,
        message: 'الرجاء إدخال نسبة مئوية صحيحة (بين 0 و100).',
        element: percentageInput
      };
    }
    return { valid: true };
  }

  // Emirati has NO track selector at all either — validate its own subject table and return
  // early, bypassing the generic track requirement below (mirrors "أخرى" above).
  if (certSelect.value === 'emirati') {
    const markInputs = document.querySelectorAll('.emirati-mark-input');
    if (markInputs.length === 0) {
      return {
        valid: false,
        message: 'الرجاء توليد جدول مواد الشهادة الإماراتية أولاً.',
        element: certSelect
      };
    }

    for (let i = 0; i < markInputs.length; i++) {
      const isOptional = markInputs[i].classList.contains('emirati-optional-input');
      if (isOptional && markInputs[i].value === '') continue;   // optional subjects may stay empty

      const val = parseFloat(markInputs[i].value);
      if (markInputs[i].value === '' || isNaN(val) || val < 0 || val > 100) {
        return {
          valid: false,
          message: `الرجاء إدخال درجة صحيحة (بين 0 و100) لمادة "${markInputs[i].getAttribute('data-subject')}".`,
          element: markInputs[i]
        };
      }
    }

    return { valid: true };
  }

  // American Diploma has NO track selector at all either — validate its own fields and return
  // early, bypassing the generic track requirement below (mirrors "أخرى"/Emirati above). Note:
  // the 1050/1100 admission minimums are NEVER checked here — they're a non-blocking warning only.
  if (certSelect.value === 'americanDiploma') {
    const subjectInputs = document.querySelectorAll('.american-diploma-subject-input');
    const markInputs = document.querySelectorAll('.american-diploma-mark-input');
    if (markInputs.length === 0) {
      return {
        valid: false,
        message: 'الرجاء توليد جدول المواد أولاً.',
        element: certSelect
      };
    }

    for (let i = 0; i < subjectInputs.length; i++) {
      if (!subjectInputs[i].value.trim()) {
        return {
          valid: false,
          message: `الرجاء إدخال اسم المادة رقم ${i + 1}.`,
          element: subjectInputs[i]
        };
      }
    }

    for (let i = 0; i < markInputs.length; i++) {
      const val = parseFloat(markInputs[i].value);
      const subjectName = subjectInputs[i] ? subjectInputs[i].value.trim() : `رقم ${i + 1}`;
      if (markInputs[i].value === '' || isNaN(val) || val < 0 || val > 100) {
        return {
          valid: false,
          message: `الرجاء إدخال درجة صحيحة (بين 0 و100) لـ "${subjectName}".`,
          element: markInputs[i]
        };
      }
    }

    const sat1Input = document.getElementById('american-diploma-sat1');
    const sat1Val = parseFloat(sat1Input.value);
    if (sat1Input.value === '' || isNaN(sat1Val) || sat1Val < 400 || sat1Val > 1600) {
      return {
        valid: false,
        message: 'الرجاء إدخال درجة SAT I صحيحة (بين 400 و1600).',
        element: sat1Input
      };
    }

    // SAT II: shown for medical + engineering colleges (never تجارة). Medical is always optional;
    // engineering is mandatory unless the student checked "studied advanced math". Whenever a
    // value IS entered — mandatory or optionally filled in — its range and both subjects are
    // still validated.
    const collegeVal = document.getElementById('wish-college').value;
    const isSatIIApplicable = americanDiplomaConfig && americanDiplomaConfig.sat_ii_applicable_colleges.includes(collegeVal);
    if (isSatIIApplicable) {
      const isMedical = americanDiplomaConfig.medical_colleges.includes(collegeVal);
      const studiedAdvancedMath = document.getElementById('american-diploma-advanced-math-checkbox').checked;
      const mandatory = !isMedical && !studiedAdvancedMath;

      const sat2Input = document.getElementById('american-diploma-sat2');
      const sat2Provided = sat2Input.value !== '';

      if (mandatory && !sat2Provided) {
        return {
          valid: false,
          message: 'الرجاء إدخال درجة SAT II (مطلوبة لهذه الكلية، إلا إذا أكدت دراسة الرياضيات المتقدمة).',
          element: sat2Input
        };
      }

      if (sat2Provided) {
        const sat2Val = parseFloat(sat2Input.value);
        if (isNaN(sat2Val) || sat2Val < 400 || sat2Val > 1600) {
          return {
            valid: false,
            message: 'الرجاء إدخال درجة SAT II صحيحة (بين 400 و1600).',
            element: sat2Input
          };
        }

        const subject2Select = document.getElementById('american-diploma-sat2-subject2');
        if (!subject2Select.value) {
          return {
            valid: false,
            message: 'الرجاء اختيار مادة SAT II الثانية.',
            element: subject2Select
          };
        }
      }
    }

    return { valid: true };
  }

  // 5. Track
  const trackSelect = document.getElementById('track-select');
  if (!trackSelect.value) {
    return {
      valid: false,
      message: 'الرجاء اختيار المسار الأكاديمي.',
      element: trackSelect
    };
  }

  // Check if IG Cert is active
  if (certSelect.value === 'ig') {
    const trackVal = trackSelect.value;
    let targetSelectors = '';
    if (trackVal.includes('IGCSE')) {
      targetSelectors = '.ig-grade-input[data-grade-type="igcse-legacy"], .ig-grade-input[data-grade-type="igcse-numeric"]';
    } else if (trackVal.includes('AS-Levels')) {
      targetSelectors = '.ig-grade-input[data-grade-type="as-level"]';
    } else if (trackVal.includes('A-Levels')) {
      targetSelectors = '.ig-grade-input[data-grade-type="a-level"]';
    }

    let totalIGSubjects = 0;
    if (targetSelectors) {
      document.querySelectorAll(targetSelectors).forEach(el => {
        totalIGSubjects += parseInt(el.value) || 0;
      });
    }

    if (totalIGSubjects === 0) {
      return {
        valid: false,
        message: 'الرجاء تحديد عدد المواد وتوزيع الدرجات الأكاديمية لحساب المجموع.',
        element: document.querySelector(targetSelectors) || trackSelect
      };
    }

    // Validate Factor input if checked
    const factorCheck = document.getElementById('ig-factor-check');
    if (factorCheck && factorCheck.checked) {
      const factorVal = parseFloat(document.getElementById('ig-factor-val').value);
      if (isNaN(factorVal) || factorVal <= 0) {
        return {
          valid: false,
          message: 'الرجاء إدخال قيمة معامل صحيحة أكبر من الصفر.',
          element: document.getElementById('ig-factor-val')
        };
      }
    }

    // Validate Sports Bonus input
    const sportsBonusVal = parseFloat(document.getElementById('ig-sports-bonus').value);
    if (isNaN(sportsBonusVal) || sportsBonusVal < 0 || sportsBonusVal > 30) {
      return {
        valid: false,
        message: 'الرجاء إدخال نسبة حافز رياضي صحيحة بين 0 و 30%.',
        element: document.getElementById('ig-sports-bonus')
      };
    }

    return { valid: true };
  }

  // Check if Kuwaiti Cert is active
  if (certSelect.value === 'kuwaiti') {
    const yearsCountSelect = document.getElementById('kuwaiti-years-count');
    if (!yearsCountSelect.value) {
      return {
        valid: false,
        message: 'الرجاء اختيار عدد سنوات الدراسة.',
        element: yearsCountSelect
      };
    }

    const includedLevels = getKuwaitiIncludedLevels();
    const obtainedInputs = document.querySelectorAll('.kuwaiti-obtained-input');
    if (obtainedInputs.length === 0) {
      return {
        valid: false,
        message: 'الرجاء توليد جدول مواد الشهادة الكويتية أولاً.',
        element: yearsCountSelect
      };
    }

    for (let i = 0; i < obtainedInputs.length; i++) {
      const obtainedVal = parseFloat(obtainedInputs[i].value);
      const maxMarkVal = parseFloat(obtainedInputs[i].getAttribute('data-max-mark'));

      if (isNaN(obtainedVal) || obtainedVal < 0 || obtainedVal > maxMarkVal) {
        return {
          valid: false,
          message: 'الرجاء إدخال درجة متحصلة صحيحة (بين 0 و' + maxMarkVal + ') لجميع المواد.',
          element: obtainedInputs[i]
        };
      }
    }

    const isOneYear = includedLevels.length === 1;
    if (!isOneYear) {
      let weightSum = 0;
      for (const level of includedLevels) {
        const weightInput = document.getElementById('kuwaiti-weight-' + level);
        const weightVal = parseFloat(weightInput.value);
        if (isNaN(weightVal) || weightVal <= 0 || weightVal > 100) {
          return {
            valid: false,
            message: 'الرجاء إدخال نسبة صحيحة (بين 0 و100) لكل سنة كما هي مدونة بالشهادة.',
            element: weightInput
          };
        }
        weightSum += weightVal;
      }

      if (Math.abs(weightSum - 100) > 0.01) {
        return {
          valid: false,
          message: 'مجموع نسب السنوات المدخلة يجب أن يساوي 100%.',
          element: document.getElementById('kuwaiti-weight-' + includedLevels[0])
        };
      }
    }

    return { valid: true };
  }

  // Check if Qatari Cert is active
  if (certSelect.value === 'qatari') {
    if (trackSelect.value !== (qatariConfig ? qatariConfig.scientific_track_name : 'المسار العلمي')) {
      return {
        valid: false,
        message: 'قائمة مواد هذا المسار غير معتمدة بعد في النظام — يرجى مراجعة مكتب تنسيق القبول بالجامعات والمعاهد المصرية.',
        element: trackSelect
      };
    }

    return validateSingleYearFixedTotalMarks('qatari', 'الرجاء توليد جدول مواد الشهادة القطرية أولاً.', trackSelect);
  }

  // Check if Omani Cert is active
  if (certSelect.value === 'omani') {
    return validateSingleYearFixedTotalMarks('omani', 'الرجاء توليد جدول مواد الشهادة العمانية أولاً.', trackSelect);
  }

  // Check if Yemeni Cert is active
  if (certSelect.value === 'yemeni') {
    return validateSingleYearFixedTotalMarks('yemeni', 'الرجاء توليد جدول مواد الشهادة اليمنية أولاً.', trackSelect);
  }

  // Check if Bahraini Cert is active
  if (certSelect.value === 'bahraini') {
    const isSupported = trackSelect.value === (bahrainiConfig ? bahrainiConfig.scientific_track_name : 'علمي')
      || trackSelect.value === (bahrainiConfig ? bahrainiConfig.literary_track_name : 'أدبي');
    if (!isSupported) {
      return {
        valid: false,
        message: 'قائمة مواد هذا المسار غير معتمدة بعد في النظام — يرجى مراجعة مكتب تنسيق القبول بالجامعات والمعاهد المصرية.',
        element: trackSelect
      };
    }

    return validateSingleYearFixedTotalMarks('bahraini', 'الرجاء توليد جدول مواد الشهادة البحرينية أولاً.', trackSelect);
  }

  // Check if Palestinian Cert is active — percentage-in only, no subjects.
  if (certSelect.value === 'palestinian') {
    const percentageInput = document.getElementById('palestinian-percentage');
    const val = parseFloat(percentageInput.value);
    if (percentageInput.value === '' || isNaN(val) || val < 0 || val > 100) {
      return {
        valid: false,
        message: 'الرجاء إدخال نسبة مئوية صحيحة (بين 0 و100).',
        element: percentageInput
      };
    }
    return { valid: true };
  }

  // Check if Saudi Cert is active
  if (certSelect.value === 'saudi') {
    const yearSelect = document.getElementById('year-select');
    if (!yearSelect.value) {
      return {
        valid: false,
        message: 'الرجاء اختيار عدد سنوات الدراسة التراكمية.',
        element: yearSelect
      };
    }

    const achievedInputs = document.querySelectorAll('.saudi-achieved-input');
    const weightedInputs = document.querySelectorAll('.saudi-weighted-input');
    if (achievedInputs.length === 0) {
      return {
        valid: false,
        message: 'الرجاء توليد جدول المواد أولاً.',
        element: yearSelect
      };
    }

    for (let i = 0; i < achievedInputs.length; i++) {
      const subjectName = achievedInputs[i].getAttribute('data-subject');
      const achievedVal = parseFloat(achievedInputs[i].value);
      const weightedVal = parseFloat(weightedInputs[i].value);

      if (achievedInputs[i].value === '' || isNaN(achievedVal) || achievedVal <= 0) {
        return {
          valid: false,
          message: `الرجاء إدخال الدرجة المتحصلة لمادة "${subjectName}".`,
          element: achievedInputs[i]
        };
      }
      if (weightedInputs[i].value === '' || isNaN(weightedVal) || weightedVal <= 0) {
        return {
          valid: false,
          message: `الرجاء إدخال الدرجة الموزونة لمادة "${subjectName}".`,
          element: weightedInputs[i]
        };
      }

      const rawCoefficient = weightedVal / achievedVal;
      const rounded = Math.round(rawCoefficient);
      if (Math.abs(rawCoefficient - rounded) > 0.01) {
        return {
          valid: false,
          message: `درجات مادة "${subjectName}" غير صحيحة: المعامل الناتج (${rawCoefficient.toFixed(2)}) ليس رقماً صحيحاً. تأكد أن الدرجة الموزونة من مضاعفات الدرجة المتحصلة.`,
          element: weightedInputs[i]
        };
      }
    }

    const aptitudeInput = document.getElementById('saudi-aptitude-score');
    const aptitudeVal = parseFloat(aptitudeInput.value);
    if (aptitudeInput.value === '' || isNaN(aptitudeVal) || aptitudeVal < 0 || aptitudeVal > 100) {
      return {
        valid: false,
        message: 'الرجاء إدخال درجة القدرات الكلية بشكل صحيح (بين 0 و 100).',
        element: aptitudeInput
      };
    }

    return { valid: true };
  }

  // Check if Egyptian Thanaweya Amma Cert is active
  if (certSelect.value === 'egyptian') {
    const systemSelect = document.getElementById('egyptian-system-select');
    if (!systemSelect.value) {
      return {
        valid: false,
        message: 'الرجاء اختيار نظام المواد (قديم أو حديث).',
        element: systemSelect
      };
    }

    const markInputs = document.querySelectorAll('.egyptian-mark-input');
    if (markInputs.length === 0) {
      return {
        valid: false,
        message: 'الرجاء توليد جدول مواد الثانوية العامة المصرية أولاً.',
        element: systemSelect
      };
    }

    for (let i = 0; i < markInputs.length; i++) {
      const markVal = parseFloat(markInputs[i].value);
      const maxVal = parseFloat(markInputs[i].getAttribute('data-max'));
      if (markInputs[i].value === '' || isNaN(markVal) || markVal < 0 || markVal > maxVal) {
        return {
          valid: false,
          message: 'الرجاء إدخال درجة صحيحة (بين 0 و' + maxVal + ') لجميع المواد.',
          element: markInputs[i]
        };
      }
    }

    return { valid: true };
  }

  // Check if Azhar Thanaweya Cert is active
  if (certSelect.value === 'azhar') {
    const markInputs = document.querySelectorAll('.azhar-mark-input');
    if (markInputs.length === 0) {
      return {
        valid: false,
        message: 'الرجاء توليد جدول مواد الثانوية الأزهرية أولاً.',
        element: trackSelect
      };
    }

    for (let i = 0; i < markInputs.length; i++) {
      const markVal = parseFloat(markInputs[i].value);
      const maxVal = parseFloat(markInputs[i].getAttribute('data-max'));
      if (markInputs[i].value === '' || isNaN(markVal) || markVal < 0 || markVal > maxVal) {
        return {
          valid: false,
          message: 'الرجاء إدخال درجة صحيحة (بين 0 و' + maxVal + ') لجميع المواد.',
          element: markInputs[i]
        };
      }
    }

    return { valid: true };
  }

  // 6. Year of Study (Non-IG, Non-Saudi)
  const yearSelect = document.getElementById('year-select');
  if (!yearSelect.value) {
    return {
      valid: false,
      message: 'الرجاء اختيار السنة الدراسية.',
      element: yearSelect
    };
  }

  // 7. Table Inputs (Non-IG, Non-Saudi)
  const gradeInputs = document.querySelectorAll('.grade-input');
  const weightInputs = document.querySelectorAll('.weight-input');

  for (let i = 0; i < gradeInputs.length; i++) {
    const gradeVal = parseFloat(gradeInputs[i].value);
    const weightVal = parseFloat(weightInputs[i].value);

    if (isNaN(gradeVal) || gradeVal < 0 || gradeVal > 100) {
      return {
        valid: false,
        message: 'الرجاء إدخال درجة صحيحة بين 0 و 100 لجميع المواد.',
        element: gradeInputs[i]
      };
    }

    if (isNaN(weightVal) || weightVal < 0 || weightVal > 100) {
      return {
        valid: false,
        message: 'الرجاء إدخال نسبة موزونة صحيحة بين 0 و 100 لجميع المواد.',
        element: weightInputs[i]
      };
    }
  }

  return { valid: true };
}

// Compile Form Inputs into JSON Object
// Shared by Qatari, Omani and Yemeni: reads the flat subject-mark table + optional documentation
// fields for a given prefix into the shape StudentCreateDto.QatariData/OmaniData/YemeniData expect.
function collectSingleYearFixedTotalPayload(prefix) {
  const subjects = [];
  document.querySelectorAll('.' + prefix + '-mark-input').forEach(input => {
    subjects.push({
      subjectName: input.getAttribute('data-subject'),
      mark: parseFloat(input.value) || 0
    });
  });

  const printedTotalInput = document.getElementById(prefix + '-printed-total');
  const printedPercentageInput = document.getElementById(prefix + '-printed-percentage');

  const printedTotal = printedTotalInput && printedTotalInput.value !== '' ? parseFloat(printedTotalInput.value) : null;
  const printedPercentage = printedPercentageInput && printedPercentageInput.value !== '' ? parseFloat(printedPercentageInput.value) : null;

  const finalTotal = parseFloat(document.getElementById(prefix + '-final-total').textContent) || 0;
  const percentage = parseFloat(document.getElementById(prefix + '-percentage').textContent) || 0;

  return {
    data: {
      subjects: subjects,
      printedTotal: printedTotal,
      printedPercentage: printedPercentage
    },
    finalTotal: finalTotal,
    percentage: percentage
  };
}

function compilePayload() {
  const certSelect = document.getElementById('cert-select');
  const trackSelect = document.getElementById('track-select');
  const trackVal = trackSelect.value;

  const personalInfo = {
    studentName: document.getElementById('student-name-ar').value.trim(),
    studentNameAr: document.getElementById('student-name-ar').value.trim(),
    studentNameEn: document.getElementById('student-name-en').value.trim(),
    wishCollege: document.getElementById('wish-college').value.trim(),
    wishProgram: document.getElementById('wish-program').value.trim(),
    graduationYear: document.getElementById('graduation-year').value.trim(),
    gender: document.getElementById('student-gender').value.trim(),
    studentPhone: document.getElementById('student-phone').value.trim(),
    studentEmail: document.getElementById('student-email').value.trim(),
    guardianName: document.getElementById('guardian-name').value.trim(),
    guardianNationalId: document.getElementById('guardian-national-id').value.trim(),
    guardianOccupation: document.getElementById('guardian-occupation').value.trim(),
    guardianPhone: document.getElementById('guardian-phone').value.trim(),
    guardianRelation: document.getElementById('guardian-relation').value.trim(),
    addressGov: document.getElementById('address-gov').value.trim(),
    addressCenter: document.getElementById('address-center').value.trim(),
    addressVillage: document.getElementById('address-village').value.trim(),
    addressStreet: document.getElementById('address-street').value.trim(),
    addressBuilding: document.getElementById('address-building').value.trim(),
    addressFloor: document.getElementById('address-floor').value.trim()
  };

  if (certSelect.value === 'qatari') {
    const collected = collectSingleYearFixedTotalPayload('qatari');
    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      qatariData: collected.data,
      finalTotal: collected.finalTotal,
      percentage: collected.percentage,
      equivalentTotal: (collected.percentage / 100) * 410,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'omani') {
    const collected = collectSingleYearFixedTotalPayload('omani');
    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      omaniData: collected.data,
      finalTotal: collected.finalTotal,
      percentage: collected.percentage,
      equivalentTotal: (collected.percentage / 100) * 410,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'yemeni') {
    const collected = collectSingleYearFixedTotalPayload('yemeni');
    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      yemeniData: collected.data,
      finalTotal: collected.finalTotal,
      percentage: collected.percentage,
      equivalentTotal: (collected.percentage / 100) * 410,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'bahraini') {
    const collected = collectSingleYearFixedTotalPayload('bahraini');
    const config = getSingleYearConfig('bahraini');
    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      bahrainiData: collected.data,
      finalTotal: collected.finalTotal,
      totalMax: config ? config.total_max : 0,
      percentage: collected.percentage,
      equivalentTotal: (collected.percentage / 100) * 410,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'palestinian') {
    const percentage = Math.round((parseFloat(document.getElementById('palestinian-percentage').value) || 0) * 100) / 100;
    const equivalentTotal = (percentage / 100) * 410;
    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      palestinianData: { percentage: percentage, branch: trackVal },
      percentage: percentage,
      equivalentTotal: equivalentTotal,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'egyptian') {
    const subjectSystem = document.getElementById('egyptian-system-select').value;
    const subjects = [];
    document.querySelectorAll('.egyptian-mark-input').forEach(input => {
      subjects.push({
        subjectName: input.getAttribute('data-subject'),
        mark: parseFloat(input.value) || 0
      });
    });

    const finalTotal = parseFloat(document.getElementById('egyptian-final-total').textContent) || 0;
    const percentage = parseFloat(document.getElementById('egyptian-percentage').textContent) || 0;
    const denominator = egyptianConfig && subjectSystem
      ? (subjectSystem === egyptianConfig.new_system_name ? egyptianConfig.denominators.new_system : egyptianConfig.denominators.old_system)
      : 0;

    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      egyptianData: { subjectSystem: subjectSystem, subjects: subjects },
      finalTotal: finalTotal,
      denominator: denominator,
      percentage: percentage,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'azhar') {
    const subjects = [];
    document.querySelectorAll('.azhar-mark-input').forEach(input => {
      subjects.push({
        subjectName: input.getAttribute('data-subject'),
        mark: parseFloat(input.value) || 0
      });
    });

    const finalTotal = parseFloat(document.getElementById('azhar-final-total').textContent) || 0;
    const percentage = parseFloat(document.getElementById('azhar-percentage').textContent) || 0;
    const equivalentTotal = parseFloat(document.getElementById('azhar-equivalent-total').textContent) || 0;
    const denominator = azharConfig && trackVal ? (azharConfig.denominators[trackVal] || 0) : 0;

    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      azharData: { subjects: subjects },
      finalTotal: finalTotal,
      denominator: denominator,
      percentage: percentage,
      equivalentTotal: equivalentTotal,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'emirati') {
    const subjects = [];
    document.querySelectorAll('.emirati-mark-input').forEach(input => {
      if (input.value === '') return;   // empty optional subject — not on this student's certificate
      subjects.push({
        subjectName: input.getAttribute('data-subject'),
        mark: parseFloat(input.value) || 0
      });
    });

    const finalTotal = parseFloat(document.getElementById('emirati-final-total').textContent) || 0;
    const denominatorText = (document.getElementById('emirati-final-total').textContent || '').split('/')[1];
    const denominator = parseFloat(denominatorText) || 0;
    const percentage = parseFloat(document.getElementById('emirati-percentage').textContent) || 0;
    const equivalentTotal = parseFloat(document.getElementById('emirati-equivalent-total').textContent) || 0;

    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: emiratiConfig ? emiratiConfig.single_track_name : 'المسار العام',   // no track selector — fixed placeholder satisfies the shared Track column.
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      emiratiData: { subjects: subjects },
      finalTotal: finalTotal,
      denominator: denominator,
      percentage: percentage,
      equivalentTotal: equivalentTotal,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'americanDiploma') {
    const subjects = [];
    const subjectInputs = document.querySelectorAll('.american-diploma-subject-input');
    const markInputs = document.querySelectorAll('.american-diploma-mark-input');
    markInputs.forEach((input, i) => {
      subjects.push({
        subjectName: subjectInputs[i] ? subjectInputs[i].value.trim() : '',
        mark: parseFloat(input.value) || 0
      });
    });

    const collegeVal = document.getElementById('wish-college').value;
    const isSatIIApplicable = americanDiplomaConfig && americanDiplomaConfig.sat_ii_applicable_colleges.includes(collegeVal);
    const studiedAdvancedMath = document.getElementById('american-diploma-advanced-math-checkbox').checked;
    const satI = parseInt(document.getElementById('american-diploma-sat1').value, 10) || 0;

    const sat2Input = document.getElementById('american-diploma-sat2');
    const satIIProvided = isSatIIApplicable && sat2Input.value !== '';
    const satII = satIIProvided ? (parseInt(sat2Input.value, 10) || 0) : null;
    const satIISubject1 = satIIProvided ? document.getElementById('american-diploma-sat2-subject1').value : null;
    const satIISubject2 = satIIProvided ? document.getElementById('american-diploma-sat2-subject2').value : null;

    const averageScore = parseFloat(document.getElementById('american-diploma-average').textContent) || 0;
    const basePercentage = parseFloat(document.getElementById('american-diploma-base-percentage').textContent) || 0;

    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: 'الدبلومة الأمريكية',   // no track selector — fixed placeholder satisfies the shared Track column.
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      americanDiplomaData: {
        subjects: subjects,
        satI: satI,
        satII: satII,
        satIISubject1: satIISubject1,
        satIISubject2: satIISubject2,
        studiedAdvancedMath: studiedAdvancedMath
      },
      averageScore: averageScore,
      basePercentage: basePercentage,
      satIBelowMinimum: americanDiplomaConfig ? satI < americanDiplomaConfig.sat_i_minimum_threshold : false,
      satIIBelowMinimum: !!(satIIProvided && americanDiplomaConfig && satII < americanDiplomaConfig.sat_ii_minimum_threshold),
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'other') {
    const certificateName = document.getElementById('other-certificate-name').value.trim();
    const percentage = Math.round((parseFloat(document.getElementById('other-percentage').value) || 0) * 100) / 100;
    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: 'أخرى',   // "أخرى" has no track selector — a fixed placeholder satisfies the shared Track column.
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      otherData: { certificateName: certificateName, percentage: percentage },
      percentage: percentage,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'kuwaiti') {
    const yearsCount = document.getElementById('kuwaiti-years-count').value;
    const isOneYear = yearsCount === 'One Year';
    const isThreeYears = yearsCount === 'Three Years';
    const hasSecondAttempt = document.getElementById('kuwaiti-second-attempt').checked;

    const getWeight = (level) => {
      const input = document.getElementById('kuwaiti-weight-' + level);
      return input ? (parseFloat(input.value) || 0) : null;
    };

    const collectGradeLevel = (level) => {
      const rows = [];
      document.querySelectorAll(`.kuwaiti-obtained-input[data-grade-level="${level}"]`).forEach(input => {
        rows.push({
          subjectName: input.getAttribute('data-subject'),
          obtained: parseFloat(input.value) || 0
        });
      });
      return rows;
    };

    const finalPercentage = parseFloat(document.getElementById('kuwaiti-final-percentage').textContent) || 0;
    const equivalentTotal = parseFloat(document.getElementById('kuwaiti-equivalent-total').textContent) || 0;

    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      photo: uploadedPhotoBase64,
      kuwaitiData: {
        yearsCount: yearsCount,
        hasSecondAttempt: hasSecondAttempt,
        grade10Weight: isThreeYears ? getWeight(10) : null,
        grade11Weight: isOneYear ? null : getWeight(11),
        grade12Weight: isOneYear ? 100 : getWeight(12),
        grade10Subjects: isThreeYears ? collectGradeLevel(10) : null,
        grade11Subjects: isOneYear ? null : collectGradeLevel(11),
        grade12Subjects: collectGradeLevel(12)
      },
      finalPercentage: finalPercentage,
      equivalentTotal: equivalentTotal,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'ig') {
    let igProgram = 'IGCSE';
    let gradesData = {};
    let totalSubjects = 0;

    if (trackVal.includes('IGCSE')) {
      igProgram = 'IGCSE';
      const igcseGrades = {};
      document.querySelectorAll('.ig-grade-input[data-grade-type="igcse-legacy"], .ig-grade-input[data-grade-type="igcse-numeric"]').forEach(el => {
        const count = parseInt(el.value) || 0;
        if (count > 0) {
          igcseGrades[el.getAttribute('data-grade')] = count;
          totalSubjects += count;
        }
      });
      igcseGrades.totalSubjects = totalSubjects;
      gradesData = { igcse: igcseGrades };
    } else if (trackVal.includes('AS-Levels')) {
      igProgram = 'AS-Levels';
      const asGrades = {};
      document.querySelectorAll('.ig-grade-input[data-grade-type="as-level"]').forEach(el => {
        const count = parseInt(el.value) || 0;
        if (count > 0) {
          asGrades[el.getAttribute('data-grade')] = count;
          totalSubjects += count;
        }
      });
      asGrades.totalSubjects = totalSubjects;
      gradesData = { as_level: asGrades };
    } else if (trackVal.includes('A-Levels')) {
      igProgram = 'A-Levels';
      const aGrades = {};
      document.querySelectorAll('.ig-grade-input[data-grade-type="a-level"]').forEach(el => {
        const count = parseInt(el.value) || 0;
        if (count > 0) {
          aGrades[el.getAttribute('data-grade')] = count;
          totalSubjects += count;
        }
      });
      aGrades.totalSubjects = totalSubjects;
      gradesData = { a_level: aGrades };
    }

    const factorCheck = document.getElementById('ig-factor-check');
    const factor = factorCheck && factorCheck.checked ? parseFloat(document.getElementById('ig-factor-val').value) || 1.2 : 1.0;
    const sportsBonus = parseFloat(document.getElementById('ig-sports-bonus').value) || 0.0;

    const scorePercentage = parseFloat(document.getElementById('ig-percentage-val').textContent) || 0.0;
    const governmentScore = parseFloat(document.getElementById('ig-gov-val').textContent) || 0.0;

    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackVal,
      yearOfStudy: '',
      igProgram: igProgram,
      photo: uploadedPhotoBase64,
      grades: gradesData,
      factor: factor,
      sportsBonus: sportsBonus,
      scorePercentage: scorePercentage,
      governmentScore: governmentScore,
      submittedAt: new Date().toISOString()
    };
  }

  if (certSelect.value === 'saudi') {
    const yearsCountVal = document.getElementById('year-select').value;
    const yearWeights = typeof getSaudiYearWeights === 'function' ? getSaudiYearWeights(yearsCountVal) : {};
    const yearsData = [];
    const cards = document.querySelectorAll('#saudi-multi-tables-container .saudi-year-card');
    let overallAchieved = 0;
    let overallWeighted = 0;
    let overallCoefficients = 0;
    let schoolPercentage = 0;

    cards.forEach(card => {
      const yearLabelEl = card.querySelector('.saudi-year-title');
      const labelText = yearLabelEl ? yearLabelEl.textContent.trim().replace('📚 ', '') : 'السنة الدراسية';
      const yearKey = card.getAttribute('data-year-key') || 'Year 1';

      const gradesData = [];
      const rows = card.querySelectorAll('tbody tr');
      let cardAchieved = 0;
      let cardWeighted = 0;
      let cardCoefficients = 0;

      rows.forEach(row => {
        const achievedInput = row.querySelector('.saudi-achieved-input');
        const weightedInput = row.querySelector('.saudi-weighted-input');
        const subjectName = achievedInput.getAttribute('data-subject');
        const achieved = parseFloat(achievedInput.value) || 0;
        const weighted = parseFloat(weightedInput.value) || 0;
        const coefficient = achieved > 0 ? Math.round(weighted / achieved) : 0;

        gradesData.push({
          subjectName,
          coefficient,
          achieved,
          weighted
        });

        cardAchieved += achieved;
        cardWeighted += weighted;
        cardCoefficients += coefficient;
      });

      const weightPercent = yearWeights[yearKey] || 0;
      const yearPercentage = cardCoefficients > 0 ? (cardWeighted / cardCoefficients) : 0;
      const contribution = yearPercentage * (weightPercent / 100);

      yearsData.push({
        yearLabel: yearKey,
        yearLabelAr: labelText,
        grades: gradesData,
        subtotal: {
          totalAchieved: parseFloat(cardAchieved.toFixed(2)),
          totalWeighted: parseFloat(cardWeighted.toFixed(2)),
          totalCoefficients: cardCoefficients,
          yearPercentage: parseFloat(yearPercentage.toFixed(2)),
          weightPercent,
          contribution: parseFloat(contribution.toFixed(2))
        }
      });

      overallAchieved += cardAchieved;
      overallWeighted += cardWeighted;
      overallCoefficients += cardCoefficients;
      schoolPercentage += contribution;
    });

    const aptitudeScore = parseFloat(document.getElementById('saudi-aptitude-score').value) || 0;
    // Rounded to 2dp FIRST — matches the value shown on the calculator (saudi-final-gpa) and what
    // StudentService stores; the equivalent total must be derived from that, not a raw value.
    const finalPercentage = Math.round(((schoolPercentage + aptitudeScore) / 2) * 100) / 100;
    const equivalentTotal = (finalPercentage / 100) * 410;

    return {
      ...personalInfo,
      nationalId: document.getElementById('national-id').value.trim(),
      certification: certSelect.options[certSelect.selectedIndex].text,
      track: trackSelect.value,
      yearsCount: yearsCountVal,
      photo: uploadedPhotoBase64,
      years: yearsData,
      aptitudeScore: aptitudeScore,
      overallTotals: {
        totalAchieved: parseFloat(overallAchieved.toFixed(2)),
        totalWeighted: parseFloat(overallWeighted.toFixed(2)),
        totalCoefficients: overallCoefficients,
        schoolPercentage: parseFloat(schoolPercentage.toFixed(2)),
        aptitudeScore: aptitudeScore,
        finalPercentage: parseFloat(finalPercentage.toFixed(2)),
        equivalentTotal: parseFloat(equivalentTotal.toFixed(2))
      },
      submittedAt: new Date().toISOString()
    };
  }

  const grades = [];
  const rows = document.querySelectorAll('#grades-table-body tr');

  rows.forEach(row => {
    const subjectName = row.querySelector('.col-subject').textContent;
    const grade = parseFloat(row.querySelector('.grade-input').value);
    const weighted = parseFloat(row.querySelector('.weight-input').value);
    const achieved = parseFloat(row.querySelector('.col-achieved').textContent);

    grades.push({
      subjectName,
      grade,
      weighted,
      achieved
    });
  });

  return {
    ...personalInfo,
    nationalId: document.getElementById('national-id').value.trim(),
    certification: certSelect.options[certSelect.selectedIndex].text,
    track: trackSelect.value,
    yearOfStudy: document.getElementById('year-select').value,
    photo: uploadedPhotoBase64,
    grades: grades,
    submittedAt: new Date().toISOString()
  };
}

// Send Data to the ASP.NET Core API (same-origin, served by this app) with local backup fallback
function sendData(payload, submitBtn, originalText) {
  // Convert payload to ASP.NET Core DTO structure
  const apiPayload = {
    studentName: payload.studentName,
    studentNameEn: payload.studentNameEn,
    nationalId: payload.nationalId,
    wishCollege: payload.wishCollege,
    wishProgram: payload.wishProgram,
    graduationYear: parseInt(payload.graduationYear, 10),
    gender: payload.gender,
    phone: payload.studentPhone,
    email: payload.studentEmail,
    guardianName: payload.guardianName,
    guardianNationalId: payload.guardianNationalId,
    guardianOccupation: payload.guardianOccupation,
    guardianPhone: payload.guardianPhone,
    guardianRelation: payload.guardianRelation,
    addressGov: payload.addressGov,
    addressCenter: payload.addressCenter,
    addressVillage: payload.addressVillage,
    addressStreet: payload.addressStreet,
    addressBuilding: payload.addressBuilding,
    addressFloor: payload.addressFloor,
    certification: payload.certification === 'شهادة سعودية' ? 'Saudi Certificate' : (payload.certification.includes('IG') ? 'IG' : payload.certification),
    track: payload.track,
    photo: payload.photo
  };

  if (payload.yearsCount) {
    apiPayload.yearsCount = payload.yearsCount;
    apiPayload.aptitudeScore = payload.aptitudeScore;
    apiPayload.saudiGrades = [];
    payload.years.forEach(yr => {
      yr.grades.forEach(g => {
        apiPayload.saudiGrades.push({
          yearLabel: yr.yearLabel,
          subjectName: g.subjectName,
          achieved: g.achieved,
          weighted: g.weighted
        });
      });
    });
  } else if (payload.igProgram) {
    apiPayload.igProgram = payload.igProgram;
    apiPayload.factor = payload.factor;
    apiPayload.sportsBonus = payload.sportsBonus;
    apiPayload.igGradeCounts = [];

    const activeSubkey = payload.igProgram === 'IGCSE' ? 'igcse' : (payload.igProgram === 'AS-Levels' ? 'as_level' : 'a_level');
    const gradesObj = payload.grades[activeSubkey] || {};

    Object.keys(gradesObj).forEach(gradeKey => {
      let gradeType = 'igcse-legacy';
      if (payload.igProgram === 'IGCSE') {
        const isNumeric = ['9', '8', '7', '6', '5', '4'].includes(gradeKey);
        gradeType = isNumeric ? 'igcse-numeric' : 'igcse-legacy';
      } else if (payload.igProgram === 'AS-Levels') {
        gradeType = 'as-level';
      } else if (payload.igProgram === 'A-Levels') {
        gradeType = 'a-level';
      }

      apiPayload.igGradeCounts.push({
        gradeType: gradeType,
        grade: gradeKey,
        count: gradesObj[gradeKey]
      });
    });
  } else if (payload.kuwaitiData) {
    apiPayload.kuwaitiData = {
      yearsCount: payload.kuwaitiData.yearsCount,
      hasSecondAttempt: payload.kuwaitiData.hasSecondAttempt,
      grade10Weight: payload.kuwaitiData.grade10Weight,
      grade11Weight: payload.kuwaitiData.grade11Weight,
      grade12Weight: payload.kuwaitiData.grade12Weight,
      grade10Subjects: payload.kuwaitiData.grade10Subjects,
      grade11Subjects: payload.kuwaitiData.grade11Subjects,
      grade12Subjects: payload.kuwaitiData.grade12Subjects
    };
  } else if (payload.qatariData) {
    apiPayload.qatariData = {
      subjects: payload.qatariData.subjects
    };
  } else if (payload.omaniData) {
    apiPayload.omaniData = {
      subjects: payload.omaniData.subjects
    };
  } else if (payload.yemeniData) {
    apiPayload.yemeniData = {
      subjects: payload.yemeniData.subjects
    };
  } else if (payload.bahrainiData) {
    apiPayload.bahrainiData = {
      subjects: payload.bahrainiData.subjects
    };
  } else if (payload.palestinianData) {
    apiPayload.palestinianData = {
      percentage: payload.palestinianData.percentage,
      branch: payload.palestinianData.branch
    };
  } else if (payload.otherData) {
    apiPayload.otherData = {
      certificateName: payload.otherData.certificateName,
      percentage: payload.otherData.percentage
    };
  } else if (payload.egyptianData) {
    apiPayload.egyptianData = {
      subjectSystem: payload.egyptianData.subjectSystem,
      subjects: payload.egyptianData.subjects
    };
  } else if (payload.azharData) {
    apiPayload.azharData = {
      subjects: payload.azharData.subjects
    };
  } else if (payload.emiratiData) {
    apiPayload.emiratiData = {
      subjects: payload.emiratiData.subjects
    };
  } else if (payload.americanDiplomaData) {
    apiPayload.americanDiplomaData = {
      subjects: payload.americanDiplomaData.subjects,
      satI: payload.americanDiplomaData.satI,
      satII: payload.americanDiplomaData.satII,
      satIISubject1: payload.americanDiplomaData.satIISubject1,
      satIISubject2: payload.americanDiplomaData.satIISubject2,
      studiedAdvancedMath: payload.americanDiplomaData.studiedAdvancedMath
    };
  } else {
    apiPayload.yearOfStudy = payload.yearOfStudy;
    apiPayload.standardGrades = payload.grades.map(g => ({
      yearOfStudy: payload.yearOfStudy,
      subjectName: g.subjectName,
      grade: g.grade,
      weightedPercentage: g.weighted
    }));
  }

  fetch('/api/students/register', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(apiPayload)
  })
  .then(async response => {
    let result = {};
    try {
      result = await response.json();
    } catch(e) {}

    if (response.ok && result.status === 'success') {
      showSuccessScreen(payload, 'server', result.file_path || '');
    } else {
      throw new Error(result.message || 'Server error occurred');
    }
  })
  .catch(error => {
    console.warn('Backend submission failed. Storing in localStorage instead.', error);

    // Save to localStorage as secondary backup
    try {
      const existingSubmissions = JSON.parse(localStorage.getItem('student_submissions') || '[]');
      existingSubmissions.push(payload);
      localStorage.setItem('student_submissions', JSON.stringify(existingSubmissions));

      showSuccessScreen(payload, 'local');
    } catch(storageError) {
      console.error('Failed to store in localStorage', storageError);
      showAlert('form-alert', 'حدث خطأ أثناء الاتصال بالخادم ولم نتمكن من الحفظ محلياً. يرجى تنزيل الملف لحفظ بياناتك.', 'danger');
      showSuccessScreen(payload, 'local_failed');
    }
  })
  .finally(() => {
    submitBtn.disabled = false;
    submitBtn.innerHTML = originalText;
  });
}

// Show Success Receipt Screen
function showSuccessScreen(payload, mode, serverPath = '') {
  document.getElementById('student-reg-form').style.display = 'none';
  document.getElementById('step-bar-container').style.display = 'none';

  const successScreen = document.getElementById('success-screen');
  successScreen.style.display = 'block';

  // Fill receipt data
  document.getElementById('receipt-name').textContent = payload.studentName;
  document.getElementById('receipt-id').textContent = payload.nationalId;
  document.getElementById('receipt-cert').textContent = payload.certification;

  const programRow = document.getElementById('receipt-program-row');
  const yearRow = document.getElementById('receipt-year-row');
  const yearLabel = document.getElementById('receipt-year-label');
  const saudiGpaRow = document.getElementById('receipt-saudi-gpa-row');
  const saudiGpaVal = document.getElementById('receipt-saudi-gpa');

  if (payload.yearsCount) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) yearRow.style.display = 'flex';
    if (yearLabel) yearLabel.textContent = 'عدد سنوات الدراسة:';

    let yearsText = payload.yearsCount;
    if (payload.yearsCount === 'One Year') yearsText = 'سنة واحدة';
    else if (payload.yearsCount === 'Two Years') yearsText = 'سنتان';
    else if (payload.yearsCount === 'Three Years') yearsText = 'ثلاث سنوات';
    document.getElementById('receipt-year').textContent = yearsText;

    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = payload.overallTotals.finalPercentage.toFixed(2) + '%';
    }
  } else if (payload.igProgram) {
    if (programRow) {
      programRow.style.display = 'flex';
      const programLabel = document.getElementById('receipt-program-label');
      if (programLabel) programLabel.textContent = 'برنامج الـ IG:';
      document.getElementById('receipt-program').textContent = payload.track;
    }
    if (yearRow) yearRow.style.display = 'none';
    if (saudiGpaRow) saudiGpaRow.style.display = 'none';
  } else if (payload.kuwaitiData) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'عدد سنوات الدراسة:';
      document.getElementById('receipt-year').textContent = kuwaitiYearsCountLabel(payload.kuwaitiData.yearsCount);
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.finalPercentage || 0).toFixed(2) + '%';
    }
  } else if (payload.qatariData) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'المجموع الاعتباري (من 410):';
      document.getElementById('receipt-year').textContent = (payload.equivalentTotal || 0).toFixed(2) + ' / 410';
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.omaniData) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'المجموع الاعتباري (من 410):';
      document.getElementById('receipt-year').textContent = (payload.equivalentTotal || 0).toFixed(2) + ' / 410';
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.yemeniData) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'المجموع الاعتباري (من 410):';
      document.getElementById('receipt-year').textContent = (payload.equivalentTotal || 0).toFixed(2) + ' / 410';
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.bahrainiData) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'المجموع الاعتباري (من 410):';
      document.getElementById('receipt-year').textContent = (payload.equivalentTotal || 0).toFixed(2) + ' / 410';
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.palestinianData) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'المجموع الاعتباري (من 410):';
      document.getElementById('receipt-year').textContent = (payload.equivalentTotal || 0).toFixed(2) + ' / 410';
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.otherData) {
    if (programRow) {
      programRow.style.display = 'flex';
      const programLabel = document.getElementById('receipt-program-label');
      if (programLabel) programLabel.textContent = 'اسم الشهادة:';
      document.getElementById('receipt-program').textContent = payload.otherData.certificateName;
    }
    if (yearRow) yearRow.style.display = 'none';
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.egyptianData) {
    // This IS the target Egyptian certificate — no equivalent-total row, unlike every foreign cert.
    if (programRow) {
      programRow.style.display = 'flex';
      const programLabel = document.getElementById('receipt-program-label');
      if (programLabel) programLabel.textContent = 'نظام المواد:';
      document.getElementById('receipt-program').textContent = payload.egyptianData.subjectSystem;
    }
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'المجموع (من ' + (payload.denominator || 0) + '):';
      document.getElementById('receipt-year').textContent = (payload.finalTotal || 0).toFixed(2) + ' / ' + (payload.denominator || 0);
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.azharData) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'المجموع الاعتباري (من 410):';
      document.getElementById('receipt-year').textContent = (payload.equivalentTotal || 0).toFixed(2) + ' / 410';
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.emiratiData) {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'المجموع الاعتباري (من 410):';
      document.getElementById('receipt-year').textContent = (payload.equivalentTotal || 0).toFixed(2) + ' / 410';
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = (payload.percentage || 0).toFixed(2) + '%';
    }
  } else if (payload.americanDiplomaData) {
    // No single equivalent total for this certificate (§8) — show the 3 admission criteria instead.
    const am = payload.americanDiplomaData;
    if (programRow) {
      programRow.style.display = 'flex';
      const programLabel = document.getElementById('receipt-program-label');
      if (programLabel) programLabel.textContent = 'SAT I / SAT II:';
      document.getElementById('receipt-program').textContent = am.satII !== null && am.satII !== undefined
        ? `${am.satI} / ${am.satII}`
        : `${am.satI} (SAT II غير مطلوب)`;
    }
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'النسبة الأساسية (من 40):';
      document.getElementById('receipt-year').textContent = (payload.basePercentage || 0).toFixed(2) + ' / 40';
    }
    if (saudiGpaRow && saudiGpaVal) {
      saudiGpaRow.style.display = 'flex';
      saudiGpaVal.textContent = 'المعدل: ' + (payload.averageScore || 0).toFixed(2) + ' / 100';
    }
  } else {
    if (programRow) programRow.style.display = 'none';
    if (yearRow) {
      yearRow.style.display = 'flex';
      if (yearLabel) yearLabel.textContent = 'السنة الدراسية:';
      document.getElementById('receipt-year').textContent = payload.yearOfStudy;
    }
    if (saudiGpaRow) saudiGpaRow.style.display = 'none';
  }

  const modeBadge = document.getElementById('receipt-mode');
  if (mode === 'server') {
    modeBadge.textContent = 'تم الحفظ على الخادم بنجاح';
    modeBadge.style.color = 'var(--success-color)';
  } else {
    modeBadge.textContent = 'تم الحفظ محلياً في المتصفح (الخادم غير متصل)';
    modeBadge.style.color = 'var(--accent-color)';
  }

  // Setup Actions
  const btnDownloadJson = document.getElementById('btn-download-json');
  btnDownloadJson.onclick = () => {
    downloadReceiptFile(payload, 'json');
  };

  const btnDownloadCsv = document.getElementById('btn-download-csv');
  btnDownloadCsv.onclick = () => {
    downloadReceiptFile(payload, 'csv');
  };

  const btnNewForm = document.getElementById('btn-new-form');
  btnNewForm.onclick = () => {
    location.reload();
  };
}

// Download local JSON / CSV receipts
function downloadReceiptFile(payload, format) {
  let fileContent = '';
  let fileName = `receipt_${payload.nationalId}`;
  let mimeType = '';

  if (format === 'json') {
    fileContent = JSON.stringify(payload, null, 2);
    fileName += '.json';
    mimeType = 'application/json';
  } else {
    // Generate CSV
    const csvRows = [];
    csvRows.push('\uFEFF'); // UTF-8 BOM for Excel Arabic layout
    csvRows.push('حقل,القيمة');
    csvRows.push(`اسم الطالب (عربي),"${payload.studentNameAr || payload.studentName}"`);
    csvRows.push(`اسم الطالب (انجليزي),"${payload.studentNameEn || ''}"`);
    csvRows.push(`الرقم القومي,${payload.nationalId}`);
    csvRows.push(`الكلية (الرغبة),"${payload.wishCollege || ''}"`);
    csvRows.push(`البرنامج (الرغبة),"${payload.wishProgram || ''}"`);
    csvRows.push(`سنة التخرج,"${payload.graduationYear || ''}"`);
    csvRows.push(`النوع,"${payload.gender || ''}"`);
    csvRows.push(`رقم هاتف الطالب,${payload.studentPhone || ''}`);
    csvRows.push(`ايميل الشخصي للطالب,${payload.studentEmail || ''}`);
    csvRows.push(`اسم ولي الامر,"${payload.guardianName || ''}"`);
    csvRows.push(`الرقم القومي لولي الامر,${payload.guardianNationalId || ''}`);
    csvRows.push(`وظيفة ولي الامر,"${payload.guardianOccupation || ''}"`);
    csvRows.push(`رقم هاتف ولي الامر,${payload.guardianPhone || ''}`);
    csvRows.push(`صلة قرابة ولي الامر,"${payload.guardianRelation || ''}"`);
    csvRows.push(`المحافظه,"${payload.addressGov || ''}"`);
    csvRows.push(`المركز,"${payload.addressCenter || ''}"`);
    csvRows.push(`قرية/حي,"${payload.addressVillage || ''}"`);
    csvRows.push(`شارع,"${payload.addressStreet || ''}"`);
    csvRows.push(`رقم العماره,"${payload.addressBuilding || ''}"`);
    csvRows.push(`رقم الدور,"${payload.addressFloor || ''}"`);
    csvRows.push(`نوع الشهادة,"${payload.certification}"`);

    if (payload.yearsCount) {
      csvRows.push(`مسار الدراسة,"${payload.track}"`);
      csvRows.push(`عدد سنوات الدراسة,"${payload.yearsCount}"`);
      csvRows.push(`مجموع الدرجات المحرزة الكلي,${payload.overallTotals.totalAchieved}`);
      csvRows.push(`مجموع المعاملات الكلي,${payload.overallTotals.totalCoefficients}`);
      csvRows.push(`المجموع الموزون الكلي,${payload.overallTotals.totalWeighted}`);
      csvRows.push(`النسبة المئوية النهائية (GPA),${payload.overallTotals.finalPercentage}%`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');

      payload.years.forEach(yr => {
        csvRows.push(`-- ${yr.yearLabelAr} --`);
        csvRows.push('المادة,المعامل,الدرجة المحرزة,الدرجة الموزونة');
        yr.grades.forEach(g => {
          csvRows.push(`"${g.subjectName}",${g.coefficient},${g.achieved},${g.weighted}`);
        });
        csvRows.push(`مجموع درجات السنة,${yr.subtotal.totalAchieved}`);
        csvRows.push(`مجموع معاملات السنة,${yr.subtotal.totalCoefficients}`);
        csvRows.push(`مجموع موزون السنة,${yr.subtotal.totalWeighted}`);
        csvRows.push('');
      });
    } else if (payload.igProgram) {
      csvRows.push(`برنامج الـ IG,"${payload.track}"`);
      csvRows.push(`نوع البرنامج,"${payload.igProgram}"`);
      csvRows.push(`تطبيق المعامل النسبي,${payload.factor}`);
      csvRows.push(`الحافز الرياضي,${payload.sportsBonus}%`);
      csvRows.push(`النسبة المئوية المحسوبة,${payload.scorePercentage}%`);
      csvRows.push(`المجموع الحكومي المعادل,${payload.governmentScore}/410`);
      csvRows.push('');
      csvRows.push('التقدير,العدد');

      const activeSubkey = payload.igProgram === 'IGCSE' ? 'igcse' : (payload.igProgram === 'AS-Levels' ? 'as_level' : 'a_level');
      const gradesObj = payload.grades[activeSubkey] || {};
      Object.keys(gradesObj).forEach(gradeKey => {
        csvRows.push(`"${gradeKey}",${gradesObj[gradeKey]}`);
      });
    } else if (payload.kuwaitiData) {
      const kw = payload.kuwaitiData;
      csvRows.push(`المسار الأكاديمي,"${payload.track}"`);
      csvRows.push(`عدد سنوات الدراسة,${kuwaitiYearsCountLabel(kw.yearsCount)}`);
      csvRows.push(`هل يوجد مواد بنظام الدور الثاني؟,${kw.hasSecondAttempt ? 'نعم' : 'لا'}`);
      csvRows.push(`النسبة المئوية النهائية,${(payload.finalPercentage || 0)}%`);
      csvRows.push(`المجموع المعادل,${(payload.equivalentTotal || 0)}/410`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');

      const gradeLevels = [
        { level: 10, label: 'الصف العاشر', weight: kw.grade10Weight, subjects: kw.grade10Subjects },
        { level: 11, label: 'الصف الحادي عشر', weight: kw.grade11Weight, subjects: kw.grade11Subjects },
        { level: 12, label: 'الصف الثاني عشر', weight: kw.grade12Weight, subjects: kw.grade12Subjects }
      ];
      gradeLevels.forEach(gl => {
        if (!gl.subjects) return;
        csvRows.push(`-- ${gl.label} (نسبتها ${gl.weight}%) --`);
        csvRows.push('المادة,الدرجة المتحصلة');
        gl.subjects.forEach(s => {
          csvRows.push(`"${s.subjectName}",${s.obtained}`);
        });
        csvRows.push('');
      });
    } else if (payload.qatariData) {
      const qa = payload.qatariData;
      csvRows.push(`المسار الأكاديمي,"${payload.track}"`);
      csvRows.push(`المجموع (من 700),${(payload.finalTotal || 0)}`);
      csvRows.push(`النسبة المئوية,${(payload.percentage || 0)}%`);
      csvRows.push(`المجموع الاعتباري (المجموع المصري),${(payload.equivalentTotal || 0)}/410`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');
      csvRows.push('المادة,الدرجة');
      qa.subjects.forEach(s => {
        csvRows.push(`"${s.subjectName}",${s.mark}`);
      });
    } else if (payload.omaniData) {
      const om = payload.omaniData;
      csvRows.push(`المسار الأكاديمي,"${payload.track}"`);
      csvRows.push(`المجموع (من 700),${(payload.finalTotal || 0)}`);
      csvRows.push(`النسبة المئوية,${(payload.percentage || 0)}%`);
      csvRows.push(`المجموع الاعتباري (المجموع المصري),${(payload.equivalentTotal || 0)}/410`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');
      csvRows.push('المادة,الدرجة');
      om.subjects.forEach(s => {
        csvRows.push(`"${s.subjectName}",${s.mark}`);
      });
    } else if (payload.yemeniData) {
      const ye = payload.yemeniData;
      csvRows.push(`المسار الأكاديمي,"${payload.track}"`);
      csvRows.push(`المجموع (من 600),${(payload.finalTotal || 0)}`);
      csvRows.push(`النسبة المئوية,${(payload.percentage || 0)}%`);
      csvRows.push(`المجموع الاعتباري (المجموع المصري),${(payload.equivalentTotal || 0)}/410`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');
      csvRows.push('المادة,الدرجة');
      ye.subjects.forEach(s => {
        csvRows.push(`"${s.subjectName}",${s.mark}`);
      });
    } else if (payload.palestinianData) {
      csvRows.push(`الفرع,"${payload.palestinianData.branch}"`);
      csvRows.push(`النسبة المئوية,${(payload.percentage || 0)}%`);
      csvRows.push(`المجموع الاعتباري (المجموع المصري),${(payload.equivalentTotal || 0)}/410`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
    } else if (payload.otherData) {
      csvRows.push(`اسم الشهادة,"${payload.otherData.certificateName}"`);
      csvRows.push(`النسبة المئوية,${(payload.percentage || 0)}%`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
    } else if (payload.egyptianData) {
      const eg = payload.egyptianData;
      csvRows.push(`المسار,"${payload.track}"`);
      csvRows.push(`نظام المواد,"${eg.subjectSystem}"`);
      csvRows.push(`المجموع (من ${payload.denominator || 0}),${(payload.finalTotal || 0)}`);
      csvRows.push(`النسبة المئوية,${(payload.percentage || 0)}%`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');
      csvRows.push('المادة,الدرجة');
      eg.subjects.forEach(s => {
        csvRows.push(`"${s.subjectName}",${s.mark}`);
      });
    } else if (payload.azharData) {
      const az = payload.azharData;
      csvRows.push(`القسم,"${payload.track}"`);
      csvRows.push(`المجموع (من ${payload.denominator || 0}),${(payload.finalTotal || 0)}`);
      csvRows.push(`النسبة المئوية,${(payload.percentage || 0)}%`);
      csvRows.push(`المجموع الاعتباري (المجموع المصري),${(payload.equivalentTotal || 0)}/410`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');
      csvRows.push('المادة,الدرجة');
      az.subjects.forEach(s => {
        csvRows.push(`"${s.subjectName}",${s.mark}`);
      });
    } else if (payload.emiratiData) {
      const em = payload.emiratiData;
      csvRows.push(`المجموع (من ${payload.denominator || 0}),${(payload.finalTotal || 0)}`);
      csvRows.push(`النسبة المئوية,${(payload.percentage || 0)}%`);
      csvRows.push(`المجموع الاعتباري (المجموع المصري),${(payload.equivalentTotal || 0)}/410`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');
      csvRows.push('المادة,الدرجة');
      em.subjects.forEach(s => {
        csvRows.push(`"${s.subjectName}",${s.mark}`);
      });
    } else if (payload.americanDiplomaData) {
      const am = payload.americanDiplomaData;
      csvRows.push(`المعدل (من 100),${(payload.averageScore || 0)}`);
      csvRows.push(`النسبة الأساسية (من 40),${(payload.basePercentage || 0)}`);
      csvRows.push(`SAT I,${am.satI}`);
      if (am.satII !== null && am.satII !== undefined) {
        csvRows.push(`SAT II,${am.satII}`);
        csvRows.push(`مادة SAT II الأولى,"${am.satIISubject1 || ''}"`);
        csvRows.push(`مادة SAT II الثانية,"${am.satIISubject2 || ''}"`);
      }
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');
      csvRows.push('المادة,الدرجة');
      am.subjects.forEach(s => {
        csvRows.push(`"${s.subjectName}",${s.mark}`);
      });
    } else {
      csvRows.push(`المسار الأكاديمي,"${payload.track}"`);
      csvRows.push(`السنة الدراسية,"${payload.yearOfStudy}"`);
      csvRows.push(`تاريخ الإرسال,${payload.submittedAt}`);
      csvRows.push('');
      csvRows.push('المادة,الدرجة,النسبة الموزونة,الدرجة المتحصلة');

      payload.grades.forEach(g => {
        csvRows.push(`"${g.subjectName}",${g.grade},${g.weighted},${g.achieved}`);
      });
    }

    fileContent = csvRows.join('\n');
    fileName += '.csv';
    mimeType = 'text/csv;charset=utf-8;';
  }

  const blob = new Blob([fileContent], { type: mimeType });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', fileName);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

// Common Alerts
function showAlert(id, message, type) {
  const alertEl = document.getElementById(id);
  alertEl.textContent = message;
  alertEl.style.display = 'flex';
  if (type === 'danger') {
    alertEl.className = 'alert alert-danger';
  } else {
    alertEl.className = 'alert alert-success';
  }
}

function hideAlert(id) {
  const alertEl = document.getElementById(id);
  alertEl.style.display = 'none';
}
