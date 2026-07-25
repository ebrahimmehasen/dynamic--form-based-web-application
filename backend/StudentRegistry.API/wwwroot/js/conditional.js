let appConfig = null;
let saudiConfig = null;
let kuwaitiConfig = null;
let qatariConfig = null;
let omaniConfig = null;
let yemeniConfig = null;
let bahrainiConfig = null;
let egyptianConfig = null;
let azharConfig = null;
let emiratiConfig = null;

// Fetch config from the ConfigController API (single source of truth)
async function loadSubjectsConfig() {
  try {
    const response = await fetch('/api/config/subjects');
    if (response.ok) {
      appConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load certifications/subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات الشهادات والمواد من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-saudi');
    if (response.ok) {
      saudiConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-saudi: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Saudi subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الشهادة السعودية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-kuwaiti');
    if (response.ok) {
      kuwaitiConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-kuwaiti: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Kuwaiti subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الشهادة الكويتية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-qatari');
    if (response.ok) {
      qatariConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-qatari: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Qatari subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الشهادة القطرية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-omani');
    if (response.ok) {
      omaniConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-omani: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Omani subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الشهادة العمانية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-yemeni');
    if (response.ok) {
      yemeniConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-yemeni: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Yemeni subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الشهادة اليمنية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-bahraini');
    if (response.ok) {
      bahrainiConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-bahraini: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Bahraini subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الشهادة البحرينية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-egyptian');
    if (response.ok) {
      egyptianConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-egyptian: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Egyptian subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الثانوية العامة المصرية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-azhar');
    if (response.ok) {
      azharConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-azhar: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Azhar subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الثانوية الأزهرية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }

  try {
    const response = await fetch('/api/config/subjects-emirati');
    if (response.ok) {
      emiratiConfig = await response.json();
    } else {
      throw new Error('Failed to load /api/config/subjects-emirati: ' + response.status);
    }
  } catch (error) {
    console.error('Could not load Emirati subjects configuration.', error);
    showAlert('form-alert', 'تعذر تحميل بيانات مواد الشهادة الإماراتية من الخادم. الرجاء تحديث الصفحة.', 'danger');
  }
}

// Adjust Section E (Year Selection) options and labels based on certKey
function adjustYearSelect(certKey) {
  const yearSelect = document.getElementById('year-select');
  const titleEl = document.querySelector('#section-year .section-title');
  const descEl = document.querySelector('#section-year .section-desc');

  if (certKey === 'saudi') {
    yearSelect.innerHTML = `
      <option value="">-- اختر --</option>
      <option value="One Year">سنة واحدة (24 مادة)</option>
      <option value="Two Years">سنتان (45 مادة)</option>
      <option value="Three Years">ثلاث سنوات (68 مادة)</option>
    `;
    if (titleEl) titleEl.textContent = 'عدد سنوات الدراسة التراكمية';
    if (descEl) descEl.textContent = 'الرجاء اختيار عدد سنوات الدراسة التراكمية التي تريد احتسابها للمعدل.';
  } else {
    yearSelect.innerHTML = `
      <option value="">-- اختر --</option>
      <option value="أولى ثانوي">أولى ثانوي</option>
      <option value="تانية ثانوي">تانية ثانوي</option>
      <option value="تالتة ثانوي">تالتة ثانوي</option>
    `;
    if (titleEl) titleEl.textContent = 'ما هي السنة الدراسية؟';
    if (descEl) descEl.textContent = 'الرجاء اختيار السنة الدراسية الحالية للطالب.';
  }
}

// Get compiled Saudi blocks based on selected year (returns array of year-by-year blocks)
// Each block's subjects are plain subject-name strings (no fixed coefficient anymore —
// the coefficient is derived per submission from the student's own Achieved/Weighted entries).
function getSaudiBlocks(yearVal) {
  const toSubjectList = (names) => (names || []).map(name => ({ name }));
  const b1 = toSubjectList(saudiConfig.block_1);
  const b2 = toSubjectList(saudiConfig.block_2);
  const b3 = toSubjectList(saudiConfig.block_3);

  // block_1/2/3 are fixed to real grade levels (الأول/الثاني/الثالث الثانوي) — they are NOT
  // interchangeable placeholders. "One Year" always means the student's single year was Third
  // Secondary, so it must use block_3 (not block_1); "Two Years" means Second + Third Secondary,
  // so it uses block_2 + block_3 (not block_1 + block_2). Matches the official spreadsheet exactly.
  const blocks = [];
  if (yearVal === 'One Year') {
    blocks.push({
      label: 'الصف الثالث الثانوي (Third Secondary Grade)',
      key: 'Year 1',
      subjects: JSON.parse(JSON.stringify(b3))
    });
  } else if (yearVal === 'Two Years') {
    blocks.push({
      label: 'الصف الثاني الثانوي (Second Secondary Grade)',
      key: 'Year 1',
      subjects: JSON.parse(JSON.stringify(b2))
    });
    blocks.push({
      label: 'الصف الثالث الثانوي (Third Secondary Grade)',
      key: 'Year 2',
      subjects: JSON.parse(JSON.stringify(b3))
    });
  } else if (yearVal === 'Three Years') {
    blocks.push({
      label: 'الصف الأول الثانوي (First Secondary Grade)',
      key: 'Year 1',
      subjects: JSON.parse(JSON.stringify(b1))
    });
    blocks.push({
      label: 'الصف الثاني الثانوي (Second Secondary Grade)',
      key: 'Year 2',
      subjects: JSON.parse(JSON.stringify(b2))
    });
    blocks.push({
      label: 'الصف الثالث الثانوي (Third Secondary Grade)',
      key: 'Year 3',
      subjects: JSON.parse(JSON.stringify(b3))
    });
  }
  return blocks;
}

// Official Saudi year-weight table (mirrors StudentService.GetSaudiYearWeights on the backend).
// Keyed by the block's position ("Year 1"/"Year 2"/"Year 3"), not by real grade level.
function getSaudiYearWeights(yearsCount) {
  if (yearsCount === 'One Year') return { 'Year 1': 100 };
  if (yearsCount === 'Two Years') return { 'Year 1': 50, 'Year 2': 50 };
  return { 'Year 1': 20, 'Year 2': 40, 'Year 3': 40 };
}

// Get compiled subjects based on certKey and selected year
function getActiveSubjects(certKey, yearVal) {
  if (certKey === 'saudi') {
    const blocks = getSaudiBlocks(yearVal);
    let allSubjects = [];
    blocks.forEach(b => {
      allSubjects = allSubjects.concat(b.subjects);
    });
    return allSubjects;
  } else {
    let subjectsList = [];
    if (yearVal === 'أولى ثانوي') {
      subjectsList = appConfig.subjects.year_1 || [];
    } else if (yearVal === 'تانية ثانوي') {
      subjectsList = appConfig.subjects.year_2 || [];
    } else if (yearVal === 'تالتة ثانوي') {
      subjectsList = appConfig.subjects.year_3 || [];
    }
    return subjectsList.map(name => ({ name: name, coefficient: null }));
  }
}

// "الرغبة" (Wish) section — desired college + program. Selection-only, never used in any
// equivalence calculation. Mirrors backend WishConstants.
const WISH_PROGRAMS_BY_COLLEGE = {
  'هندسة': ['تشييد', 'ميكاترونكس'],
  'حاسبات': ['نظم معلومات طيران', 'معلوماتية طبية', 'ذكاء اصطناعي'],
  'تجارة': ['إدارة أعمال', 'محاسبة']
};
const WISH_NO_PROGRAM_COLLEGES = ['طب بشري', 'طب أسنان', 'تمريض'];
const WISH_PHARMACY_COLLEGE = 'صيدلة';
const WISH_PHARMACY_PROGRAM = 'إكلينيكية';

// Egyptian Thanaweya Amma — the Wish section's college restricts which Track options are offered
// (only for this certification; mirrors backend EgyptianConstants.GetAllowedTracksForCollege).
const EGYPTIAN_TRACKS_BY_COLLEGE = {
  'طب بشري': ['علمي علوم'],
  'طب أسنان': ['علمي علوم'],
  'صيدلة': ['علمي علوم'],
  'تمريض': ['علمي علوم'],
  'حاسبات': ['علمي علوم', 'علمي رياضة'],
  'هندسة': ['علمي رياضة'],
  'تجارة': ['علمي علوم', 'علمي رياضة', 'أدبي']
};

function getEgyptianAllowedTracks(collegeVal) {
  return EGYPTIAN_TRACKS_BY_COLLEGE[collegeVal] || [];
}

// Rebuilds the shared Track select's options for the Egyptian cert based on the currently selected
// Wish college. If the previously chosen track is no longer allowed, resets it and forces the
// student to choose again from the new allowed set (§5/§6) — a no-op for every other certification.
function refreshEgyptianTrackOptions() {
  const certSelect = document.getElementById('cert-select');
  if (!certSelect || certSelect.value !== 'egyptian') return;

  const trackSelect = document.getElementById('track-select');
  const trackLockedIndicator = document.getElementById('track-locked-msg');
  const collegeVal = document.getElementById('wish-college').value;
  const allowedTracks = getEgyptianAllowedTracks(collegeVal);
  const previousValue = trackSelect.value;

  trackSelect.innerHTML = '<option value="">-- اختر --</option>';
  allowedTracks.forEach(track => {
    const option = document.createElement('option');
    option.value = track;
    option.textContent = track;
    trackSelect.appendChild(option);
  });

  if (allowedTracks.includes(previousValue)) {
    trackSelect.value = previousValue;
    return;
  }

  // Previously selected track (if any) is no longer valid for the new college — reset it.
  trackSelect.value = '';
  trackSelect.disabled = false;
  if (trackLockedIndicator) trackLockedIndicator.style.display = 'none';
  deactivateSection('section-year');
  deactivateSection('section-grades');
  if (typeof generateEgyptianTrackUI === 'function') {
    generateEgyptianTrackUI('');
  }
  updateProgressIndicator();
}

// Rebuild Section C (Wish) program dropdown based on the selected college.
function initWishSection() {
  const collegeSelect = document.getElementById('wish-college');
  const programSelect = document.getElementById('wish-program');
  const programGroup = document.getElementById('wish-program-group');

  collegeSelect.addEventListener('change', function () {
    const college = this.value;

    programSelect.innerHTML = '<option value="">-- اختر --</option>';
    programGroup.style.display = 'block';
    programSelect.disabled = true;
    programSelect.value = '';

    if (WISH_PROGRAMS_BY_COLLEGE[college]) {
      WISH_PROGRAMS_BY_COLLEGE[college].forEach(program => {
        const option = document.createElement('option');
        option.value = program;
        option.textContent = program;
        programSelect.appendChild(option);
      });
      programSelect.disabled = false;
    } else if (college === WISH_PHARMACY_COLLEGE) {
      const option = document.createElement('option');
      option.value = WISH_PHARMACY_PROGRAM;
      option.textContent = WISH_PHARMACY_PROGRAM;
      programSelect.appendChild(option);
      programSelect.value = WISH_PHARMACY_PROGRAM;
      programSelect.disabled = true;
    } else if (WISH_NO_PROGRAM_COLLEGES.includes(college)) {
      programGroup.style.display = 'none';
    }

    refreshEgyptianTrackOptions();
    refreshAzharTrackOptions();
    updateEmiratiMedicalWarning();
  });
}

// Emirati §4 — a static warning shown when the Wish college is one of the medical colleges.
// Display-only: it never makes the optional subjects (الكيمياء/العلوم الصحية/الأحياء) required.
const EMIRATI_MEDICAL_COLLEGES_FALLBACK = ['طب بشري', 'طب أسنان', 'صيدلة', 'تمريض'];

function updateEmiratiMedicalWarning() {
  const certSelect = document.getElementById('cert-select');
  const warningEl = document.getElementById('emirati-medical-warning');
  if (!certSelect || !warningEl || certSelect.value !== 'emirati') return;

  const collegeVal = document.getElementById('wish-college').value;
  const medicalColleges = (emiratiConfig && emiratiConfig.medical_colleges) || EMIRATI_MEDICAL_COLLEGES_FALLBACK;
  warningEl.style.display = medicalColleges.includes(collegeVal) ? 'flex' : 'none';
}

// Azhar Thanaweya — the Wish section's college restricts which قسم options are offered (only for
// this certification; mirrors backend AzharConstants.GetAllowedSectionsForCollege). قسم علمي
// الأزهري covers both علوم ورياضة معًا، so every science-adjacent college maps to it.
const AZHAR_SECTIONS_BY_COLLEGE = {
  'طب بشري': ['علمي'],
  'طب أسنان': ['علمي'],
  'صيدلة': ['علمي'],
  'تمريض': ['علمي'],
  'هندسة': ['علمي'],
  'حاسبات': ['علمي'],
  'تجارة': ['علمي', 'أدبي']
};

function getAzharAllowedSections(collegeVal) {
  return AZHAR_SECTIONS_BY_COLLEGE[collegeVal] || [];
}

// Rebuilds the shared Track select's options for the Azhar cert based on the currently selected
// Wish college. If the previously chosen قسم is no longer allowed, resets it and forces the
// student to choose again — a no-op for every other certification.
function refreshAzharTrackOptions() {
  const certSelect = document.getElementById('cert-select');
  if (!certSelect || certSelect.value !== 'azhar') return;

  const trackSelect = document.getElementById('track-select');
  const trackLockedIndicator = document.getElementById('track-locked-msg');
  const collegeVal = document.getElementById('wish-college').value;
  const allowedSections = getAzharAllowedSections(collegeVal);
  const previousValue = trackSelect.value;

  trackSelect.innerHTML = '<option value="">-- اختر --</option>';
  allowedSections.forEach(section => {
    const option = document.createElement('option');
    option.value = section;
    option.textContent = section;
    trackSelect.appendChild(option);
  });

  if (allowedSections.includes(previousValue)) {
    trackSelect.value = previousValue;
    return;
  }

  // Previously selected قسم is no longer valid for the new college — reset it.
  trackSelect.value = '';
  trackSelect.disabled = false;
  if (trackLockedIndicator) trackLockedIndicator.style.display = 'none';
  deactivateSection('section-year');
  deactivateSection('section-grades');
  if (typeof generateAzharGradesUI === 'function') {
    generateAzharGradesUI('');
  }
  updateProgressIndicator();
}

// Initialise Conditional Handlers
function initConditionals() {
  const certSelect = document.getElementById('cert-select');
  const trackSelect = document.getElementById('track-select');
  const yearSelect = document.getElementById('year-select');
  const trackLockedIndicator = document.getElementById('track-locked-msg');

  initWishSection();

  // Load configuration
  loadSubjectsConfig().then(() => {
    if (appConfig) {
      populateCertifications();
    }
  });

  function populateCertifications() {
    certSelect.innerHTML = '<option value="">-- اختر --</option>';
    Object.keys(appConfig.certifications).forEach(key => {
      const option = document.createElement('option');
      option.value = key;
      option.textContent = appConfig.certifications[key].name;
      certSelect.appendChild(option);
    });
  }

  // Master Trigger (Certifications Select)
  certSelect.addEventListener('change', function () {
    const certKey = this.value;

    // Reset subsequent fields
    trackSelect.innerHTML = '<option value="">-- اختر --</option>';
    trackSelect.disabled = !certKey;
    trackSelect.value = '';
    trackLockedIndicator.style.display = 'none';

    adjustYearSelect(certKey);
    yearSelect.value = '';
    yearSelect.disabled = !certKey;

    // Reset Egyptian's nested system-select + subjects table whenever the certification changes.
    const egyptianSystemSelect = document.getElementById('egyptian-system-select');
    const egyptianSystemGroup = document.getElementById('egyptian-system-group');
    const egyptianSubjectsBlock = document.getElementById('egyptian-subjects-block');
    if (egyptianSystemSelect) egyptianSystemSelect.value = '';
    if (egyptianSystemGroup) egyptianSystemGroup.style.display = 'none';
    if (egyptianSubjectsBlock) egyptianSubjectsBlock.style.display = 'none';

    // Hide following sections
    deactivateSection('section-track');
    deactivateSection('section-year');
    deactivateSection('section-grades');

    // "أخرى" has no track selector at all — hide Section E entirely rather than just leaving it
    // deactivated (an inactive .form-section is still fully visible, just unhighlighted).
    document.getElementById('section-track').style.display = (certKey === 'other' || certKey === 'emirati') ? 'none' : 'block';

    // Reset IG UI & standard table UI
    document.getElementById('non-ig-grades-container').style.display = 'block';
    document.getElementById('ig-grades-container').style.display = 'none';
    document.getElementById('kuwaiti-grades-container').style.display = 'none';
    document.getElementById('qatari-grades-container').style.display = 'none';
    document.getElementById('omani-grades-container').style.display = 'none';
    document.getElementById('yemeni-grades-container').style.display = 'none';
    document.getElementById('bahraini-grades-container').style.display = 'none';
    document.getElementById('palestinian-grades-container').style.display = 'none';
    document.getElementById('other-grades-container').style.display = 'none';
    document.getElementById('egyptian-grades-container').style.display = 'none';
    document.getElementById('azhar-grades-container').style.display = 'none';
    document.getElementById('emirati-grades-container').style.display = 'none';
    document.getElementById('section-year').style.display = 'block';
    document.getElementById('section-grades-title').textContent = 'جدول إدخال الدرجات';
    document.getElementById('section-grades-desc').textContent = 'أدخل الدرجة والنسبة الموزونة لكل مادة أدناه. سيتم احتساب الدرجة المتحصلة تلقائياً.';

    if (certKey) {
      // Check if IG Cert is selected
      if (certKey === 'ig') {
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('ig-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة درجات الـ IG';
        document.getElementById('section-grades-desc').textContent = 'الرجاء اختيار عدد المواد لكل تقدير للبرنامج المختار لحساب النسبة المئوية والمجموع الحكومي تلقائياً.';
        if (typeof resetIGCalculator === 'function') {
          resetIGCalculator();
        }
      } else if (certKey === 'kuwaiti') {
        // Kuwaiti cert does not use the year-select section at all (§ ARCHITECTURE.md 4.A step order)
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('kuwaiti-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الشهادة الكويتية';
        document.getElementById('section-grades-desc').textContent = 'اختر عدد سنوات الدراسة، ثم أدخل الدرجة المتحصلة لكل مادة ونسبة كل سنة من معدلك التراكمي كما هي مدونة في شهادتك.';
      } else if (certKey === 'qatari') {
        // Qatari cert is grade-12-only — no year-select section at all.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('qatari-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الشهادة القطرية';
        document.getElementById('section-grades-desc').textContent = 'أدخل درجة كل مادة من مواد المسار العلمي للصف الثاني عشر. المسارات الأخرى غير مدعومة حالياً.';
      } else if (certKey === 'omani') {
        // Omani cert is grade-12-only, single track — no year-select section at all.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('omani-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الشهادة العمانية';
        document.getElementById('section-grades-desc').textContent = 'أدخل درجة كل مادة من المواد السبع المعتمدة للصف الثاني عشر.';
      } else if (certKey === 'yemeni') {
        // Yemeni cert is grade-12-only, single track — no year-select section at all.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('yemeni-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الشهادة اليمنية';
        document.getElementById('section-grades-desc').textContent = 'أدخل درجة كل مادة من المواد الست المعتمدة للصف الثاني عشر.';
      } else if (certKey === 'bahraini') {
        // Bahraini cert is grade-11+12-only, track-dependent subject list — no year-select section.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('bahraini-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الشهادة البحرينية';
        document.getElementById('section-grades-desc').textContent = 'أدخل درجة كل مادة من مواد المسار المختار (آخر سنتين دراسيتين فقط). المسار المهني/الفني غير مدعوم حالياً.';
      } else if (certKey === 'palestinian') {
        // Palestinian Tawjihi is percentage-in only — no subjects, no grades grid, no year-select.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('palestinian-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الشهادة الفلسطينية (توجيهي)';
        document.getElementById('section-grades-desc').textContent = 'أدخل النسبة المئوية النهائية كما هي مدونة بشهادة التوجيهي.';
      } else if (certKey === 'other') {
        // "أخرى" has NO track selector at all — percentage-in only, free-text certificate name.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('other-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة شهادة أخرى';
        document.getElementById('section-grades-desc').textContent = 'أدخل اسم الشهادة والنسبة المئوية النهائية.';
      } else if (certKey === 'egyptian') {
        // Egyptian Thanaweya Amma — track selects the subject set; a nested "نظام المواد"
        // select (rendered inside the container itself, mirroring Kuwaiti's years-count) then
        // selects قديم/حديث, which fixes each subject's max mark and the overall denominator.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('egyptian-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الثانوية العامة المصرية';
        document.getElementById('section-grades-desc').textContent = 'اختر نظام المواد، ثم أدخل درجة كل مادة.';
      } else if (certKey === 'azhar') {
        // Azhar Thanaweya — القسم (from the shared track-select) selects a fixed subject list
        // directly — no secondary system select, unlike Egyptian.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('azhar-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الثانوية الأزهرية';
        document.getElementById('section-grades-desc').textContent = 'أدخل درجة كل مادة من مواد القسم المختار.';
      } else if (certKey === 'emirati') {
        // Emirati has a single track today (no track-selection UI) — core subjects (5) are
        // required, optional subjects (الكيمياء/العلوم الصحية/الأحياء) are not.
        document.getElementById('section-year').style.display = 'none';
        document.getElementById('non-ig-grades-container').style.display = 'none';
        document.getElementById('emirati-grades-container').style.display = 'block';
        document.getElementById('section-grades-title').textContent = '🧮 حاسبة الشهادة الإماراتية';
        document.getElementById('section-grades-desc').textContent = 'أدخل درجة كل مادة أساسية (إلزامية)، والمواد الاختيارية إن وُجدت في شهادتك.';
      }

      if (certKey === 'other' || certKey === 'emirati') {
        // Skip the track step entirely — go straight to section-grades.
        activateSection('section-grades');
        if (certKey === 'other' && typeof recalculateOther === 'function') {
          recalculateOther();
        }
        if (certKey === 'emirati' && typeof generateEmiratiGradesUI === 'function') {
          generateEmiratiGradesUI();
        }
      } else {
        // Populate track options — Egyptian's and Azhar's options are restricted by the Wish
        // college (§5).
        let tracks = appConfig.certifications[certKey].tracks;
        if (certKey === 'egyptian') {
          tracks = getEgyptianAllowedTracks(document.getElementById('wish-college').value);
        } else if (certKey === 'azhar') {
          tracks = getAzharAllowedSections(document.getElementById('wish-college').value);
        }
        tracks.forEach(track => {
          const option = document.createElement('option');
          option.value = track;
          option.textContent = track;
          trackSelect.appendChild(option);
        });
        activateSection('section-track');
      }
    }

    // Alerts are mutually exclusive: "أخرى" gets its own alert, الشهادة البحرينية keeps its own
    // embedded alert (untouched), and every other certification gets the shared general alert.
    document.getElementById('general-cert-alert').style.display =
      (certKey && certKey !== 'other' && certKey !== 'bahraini') ? 'block' : 'none';
    document.getElementById('other-cert-alert').style.display =
      (certKey === 'other') ? 'block' : 'none';

    updateProgressIndicator();
  });

  // Track Select Trigger
  trackSelect.addEventListener('change', function () {
    const trackVal = this.value;
    const certKey = certSelect.value;

    if (trackVal) {
      trackSelect.disabled = true;
      trackLockedIndicator.style.display = 'flex';

      if (certKey === 'ig') {
        activateSection('section-grades');

        document.getElementById('ig-sub-igcse').style.display = 'none';
        document.getElementById('ig-sub-as').style.display = 'none';
        document.getElementById('ig-sub-a').style.display = 'none';

        if (trackVal.includes('IGCSE')) {
          document.getElementById('ig-sub-igcse').style.display = 'block';
        } else if (trackVal.includes('AS-Levels')) {
          document.getElementById('ig-sub-as').style.display = 'block';
        } else if (trackVal.includes('A-Levels')) {
          document.getElementById('ig-sub-a').style.display = 'block';
        }

        if (typeof calculateIGScore === 'function') {
          calculateIGScore();
        }
      } else if (certKey === 'kuwaiti') {
        activateSection('section-grades');
      } else if (certKey === 'qatari') {
        activateSection('section-grades');
        if (typeof generateQatariGradesUI === 'function') {
          generateQatariGradesUI(trackVal);
        }
      } else if (certKey === 'omani') {
        activateSection('section-grades');
        if (typeof generateOmaniGradesUI === 'function') {
          generateOmaniGradesUI();
        }
      } else if (certKey === 'yemeni') {
        activateSection('section-grades');
        if (typeof generateYemeniGradesUI === 'function') {
          generateYemeniGradesUI();
        }
      } else if (certKey === 'bahraini') {
        activateSection('section-grades');
        if (typeof generateBahrainiGradesUI === 'function') {
          generateBahrainiGradesUI(trackVal);
        }
      } else if (certKey === 'palestinian') {
        activateSection('section-grades');
        if (typeof recalculatePalestinian === 'function') {
          recalculatePalestinian();
        }
      } else if (certKey === 'egyptian') {
        activateSection('section-grades');
        if (typeof generateEgyptianTrackUI === 'function') {
          generateEgyptianTrackUI(trackVal);
        }
      } else if (certKey === 'azhar') {
        activateSection('section-grades');
        if (typeof generateAzharGradesUI === 'function') {
          generateAzharGradesUI(trackVal);
        }
      } else {
        yearSelect.value = '';
        activateSection('section-year');
      }
    } else {
      deactivateSection('section-year');
      deactivateSection('section-grades');
    }

    updateProgressIndicator();
  });

  // Year Select Trigger
  yearSelect.addEventListener('change', function () {
    const yearVal = this.value;

    if (yearVal) {
      generateGradesTable(yearVal);
      activateSection('section-grades');
    } else {
      deactivateSection('section-grades');
    }

    updateProgressIndicator();
  });
}

// Helper to activate a form section
function activateSection(sectionId) {
  const section = document.getElementById(sectionId);
  if (section) {
    section.classList.add('active');
  }
}

// Helper to deactivate a form section
function deactivateSection(sectionId) {
  const section = document.getElementById(sectionId);
  if (section) {
    section.classList.remove('active');
  }
}
