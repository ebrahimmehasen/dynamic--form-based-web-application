using AutoMapper;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        public async Task<StudentResponseDto?> GetStudentByIdAsync(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<StudentResponseDto?> GetStudentByNationalIdAsync(string nationalId)
        {
            var student = await _unitOfWork.Students.GetByNationalIdAsync(nationalId);
            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<IEnumerable<StudentResponseDto>> GetAllStudentsAsync()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            return _mapper.Map<IEnumerable<StudentResponseDto>>(students);
        }

        public async Task<StudentResponseDto> RegisterStudentAsync(StudentCreateDto createDto)
        {
            // 1. Verify uniqueness of NationalId
            var existingStudent = await _unitOfWork.Students.GetByNationalIdAsync(createDto.NationalId);
            if (existingStudent != null)
            {
                throw new InvalidOperationException("رقم قومي مسجل مسبقاً. لا يمكن إدخال نفس الرقم القومي مرتين.");
            }

            // 2. Save image to local wwwroot disk directory
            string relativePhotoPath = await _fileStorageService.SaveBase64ImageAsync(createDto.Photo, createDto.NationalId);

            // 3. Map base Student info
            var student = _mapper.Map<Student>(createDto);
            student.PhotoPath = relativePhotoPath;
            student.SubmittedAt = DateTime.UtcNow;

            // 4. Handle Sub-calculations and structures based on Cert Type
            string cert = createDto.Certification;
            if (cert.Contains("سعودية") || cert.Equals("Saudi Certificate", StringComparison.OrdinalIgnoreCase))
            {
                ProcessSaudiCertificate(createDto, student);
            }
            else if (cert.Contains("IG") || cert.Equals("ig", StringComparison.OrdinalIgnoreCase))
            {
                ProcessIgCertificate(createDto, student);
            }
            else if (cert.Contains("كويتية") || cert.Equals("kuwaiti", StringComparison.OrdinalIgnoreCase))
            {
                ProcessKuwaitiCertificate(createDto, student);
            }
            else if (cert.Contains("قطرية") || cert.Equals("qatari", StringComparison.OrdinalIgnoreCase))
            {
                ProcessQatariCertificate(createDto, student);
            }
            else if (cert.Contains("عمانية") || cert.Equals("omani", StringComparison.OrdinalIgnoreCase))
            {
                ProcessOmaniCertificate(createDto, student);
            }
            else if (cert.Contains("يمنية") || cert.Equals("yemeni", StringComparison.OrdinalIgnoreCase))
            {
                ProcessYemeniCertificate(createDto, student);
            }
            else if (cert.Contains("بحرينية") || cert.Equals("bahraini", StringComparison.OrdinalIgnoreCase))
            {
                ProcessBahrainiCertificate(createDto, student);
            }
            else if (cert.Contains("فلسطين") || cert.Equals("palestinian", StringComparison.OrdinalIgnoreCase))
            {
                ProcessPalestinianCertificate(createDto, student);
            }
            else if (cert.Contains("أخرى") || cert.Equals("other", StringComparison.OrdinalIgnoreCase))
            {
                ProcessOtherCertificate(createDto, student);
            }
            else if (cert.Contains("الثانوية العامة المصرية") || cert.Equals("egyptian", StringComparison.OrdinalIgnoreCase))
            {
                ProcessEgyptianCertificate(createDto, student);
            }
            else if (cert.Contains("الثانوية الأزهرية") || cert.Equals("azhar", StringComparison.OrdinalIgnoreCase))
            {
                ProcessAzharCertificate(createDto, student);
            }
            else if (cert.Contains("الشهادة الإماراتية") || cert.Equals("emirati", StringComparison.OrdinalIgnoreCase))
            {
                ProcessEmiratiCertificate(createDto, student);
            }
            else if (cert.Contains("الدبلومة الأمريكية") || cert.Equals("americanDiploma", StringComparison.OrdinalIgnoreCase))
            {
                ProcessAmericanDiplomaCertificate(createDto, student);
            }
            else
            {
                ProcessStandardCertificate(createDto, student);
            }

            // 5. Commit unit of work
            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.CompleteAsync();

            // 6. Return mapped response
            return _mapper.Map<StudentResponseDto>(student);
        }

        // Official Saudi weighted-grade formula (source: the registrar's spreadsheet).
        // Per subject the student enters BOTH Achieved and Weighted; the coefficient is derived
        // (Weighted / Achieved) and must be a whole number — re-validated here server-side even
        // though StudentCreateDtoValidator already checked it, since the server is authoritative.
        // Per year block: yearPercentage = (Σ Weighted) / (Σ Coefficient), then multiplied by a
        // year-weight that depends on YearsCount (see GetSaudiYearWeights). The sum of the
        // weighted year percentages is the "school percentage", which is then averaged with the
        // student's AptitudeScore (درجة القدرات) for the final grade.
        private void ProcessSaudiCertificate(StudentCreateDto dto, Student student)
        {
            if (dto.SaudiGrades == null || !dto.SaudiGrades.Any())
                throw new ArgumentException("بيانات المواد والدرجات للشهادة السعودية مفقودة.");
            if (!dto.AptitudeScore.HasValue)
                throw new ArgumentException("درجة القدرات مفقودة.");

            string yearsCount = dto.YearsCount ?? "Three Years";
            var yearWeights = GetSaudiYearWeights(yearsCount);

            decimal overallAchieved = 0;
            decimal overallWeighted = 0;
            int overallCoefficients = 0;
            decimal schoolPercentage = 0;

            foreach (var yearGroup in dto.SaudiGrades.GroupBy(g => g.YearLabel))
            {
                decimal yearWeightedSum = 0;
                int yearCoefficientSum = 0;

                foreach (var gradeDto in yearGroup)
                {
                    if (gradeDto.Achieved <= 0)
                        throw new ArgumentException($"الدرجة المتحصلة لمادة \"{gradeDto.SubjectName}\" يجب أن تكون أكبر من الصفر.");

                    decimal rawCoefficient = gradeDto.Weighted / gradeDto.Achieved;
                    int coefficient = (int)Math.Round(rawCoefficient, MidpointRounding.AwayFromZero);
                    if (Math.Abs(rawCoefficient - coefficient) > 0.001m)
                        throw new ArgumentException($"درجات مادة \"{gradeDto.SubjectName}\" غير صحيحة: المعامل الناتج (الموزونة ÷ المتحصلة) ليس رقماً صحيحاً.");

                    student.SaudiGrades.Add(new SaudiStudentGrades
                    {
                        Student = student,
                        YearLabel = gradeDto.YearLabel,
                        SubjectName = gradeDto.SubjectName,
                        Achieved = gradeDto.Achieved,
                        Weighted = gradeDto.Weighted,
                        Coefficient = coefficient
                    });

                    overallAchieved += gradeDto.Achieved;
                    overallWeighted += gradeDto.Weighted;
                    overallCoefficients += coefficient;
                    yearWeightedSum += gradeDto.Weighted;
                    yearCoefficientSum += coefficient;
                }

                if (yearCoefficientSum <= 0)
                    throw new ArgumentException($"لا يمكن حساب نسبة السنة \"{yearGroup.Key}\" — مجموع المعاملات صفر.");

                if (!yearWeights.TryGetValue(yearGroup.Key, out decimal weightPercent))
                    throw new ArgumentException($"لا يوجد وزن معرف للسنة \"{yearGroup.Key}\" مع عدد سنوات الدراسة \"{yearsCount}\".");

                decimal yearPercentage = yearWeightedSum / yearCoefficientSum;
                schoolPercentage += yearPercentage * (weightPercent / 100m);
            }

            // Rounded to 2dp FIRST because this is the exact value displayed on the site — the
            // equivalent total must be derived from what the student sees, never from the raw
            // unrounded intermediate value.
            decimal finalPercentage = Math.Round((schoolPercentage + dto.AptitudeScore.Value) / 2, 2);

            // المجموع الاعتباري (المجموع المصري) = (finalPercentage ÷ 100) × 410.
            decimal equivalentTotal = (finalPercentage / 100m) * EquivalencyConstants.EgyptianScientificTrackTotal;

            student.SaudiTotals = new SaudiStudentTotals
            {
                Student = student,
                YearsCount = yearsCount,
                TotalAchieved = Math.Round(overallAchieved, 2),
                TotalWeighted = Math.Round(overallWeighted, 2),
                TotalCoefficients = overallCoefficients,
                SchoolPercentage = Math.Round(schoolPercentage, 2),
                AptitudeScore = dto.AptitudeScore.Value,
                FinalPercentage = finalPercentage,
                EquivalentTotal = Math.Round(equivalentTotal, 2)
            };
        }

        private static Dictionary<string, decimal> GetSaudiYearWeights(string yearsCount) => yearsCount switch
        {
            "One Year" => new Dictionary<string, decimal> { ["Year 1"] = 100m },
            "Two Years" => new Dictionary<string, decimal> { ["Year 1"] = 50m, ["Year 2"] = 50m },
            _ => new Dictionary<string, decimal> { ["Year 1"] = 20m, ["Year 2"] = 40m, ["Year 3"] = 40m }
        };

        private void ProcessIgCertificate(StudentCreateDto dto, Student student)
        {
            if (dto.IgGradeCounts == null || !dto.IgGradeCounts.Any())
                throw new ArgumentException("توزيع درجات الـ IG مفقود.");

            string program = dto.IgProgram ?? "IGCSE";
            decimal factor = dto.Factor ?? 1.2m;
            decimal sportsBonus = dto.SportsBonus ?? 0m;
            
            int maxPointVal = program switch
            {
                "IGCSE" => 8,
                "AS-Levels" => 5,
                "A-Levels" => 6,
                _ => 8
            };

            int totalPoints = 0;
            int totalSubjects = 0;

            foreach (var countDto in dto.IgGradeCounts)
            {
                var countEntity = _mapper.Map<IgStudentGradeCounts>(countDto);
                countEntity.Student = student;
                student.IgGradeCounts.Add(countEntity);

                // Point aggregation based on IG standards
                int pointsPerSubject = GetIgPoints(countDto.GradeType, countDto.Grade);
                totalPoints += countDto.Count * pointsPerSubject;
                totalSubjects += countDto.Count;
            }

            int maxPoints = totalSubjects * maxPointVal;
            decimal scorePercentage = maxPoints > 0 
                ? ((decimal)totalPoints / maxPoints) * 100 
                : 0m;

            // Apply Coefficient Factor
            if (dto.Factor.HasValue && dto.Factor.Value > 0)
            {
                scorePercentage *= factor;
            }

            // Apply Sports Bonus
            scorePercentage += sportsBonus;

            // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the
            // equivalent total (المجموع الاعتباري / المجموع المصري) must be derived from it, never
            // from the raw unrounded intermediate value.
            scorePercentage = Math.Round(scorePercentage, 2);
            decimal governmentScore = (scorePercentage / 100) * EquivalencyConstants.EgyptianScientificTrackTotal;

            student.IgGrades = new IgStudentGrades
            {
                Student = student,
                IgProgram = program,
                Factor = factor,
                SportsBonus = sportsBonus,
                ScorePercentage = scorePercentage,
                GovernmentScore = Math.Round(governmentScore, 2)
            };
        }

        private void ProcessStandardCertificate(StudentCreateDto dto, Student student)
        {
            if (dto.StandardGrades == null || !dto.StandardGrades.Any())
                throw new ArgumentException("بيانات المواد والدرجات للشهادة المعادلة مفقودة.");

            foreach (var gradeDto in dto.StandardGrades)
            {
                var grade = _mapper.Map<StandardStudentGrades>(gradeDto);
                grade.Student = student;
                student.StandardGrades.Add(grade);
            }
        }

        private void ProcessKuwaitiCertificate(StudentCreateDto dto, Student student)
        {
            var kw = dto.KuwaitiData;
            if (kw == null)
                throw new ArgumentException("بيانات الشهادة الكويتية مفقودة.");

            bool isOneYear = kw.YearsCount == KuwaitiConstants.OneYear;
            bool isThreeYears = kw.YearsCount == KuwaitiConstants.ThreeYears;

            decimal? grade10Percentage = null;
            decimal? grade10Weight = null;
            if (isThreeYears)
            {
                grade10Percentage = CalculateKuwaitiGradeLevelPercentage(kw.Grade10Subjects, KuwaitiConstants.Grade10MaxMarks, 10, student);
                grade10Weight = kw.Grade10Weight ?? 0;
            }

            decimal? grade11Percentage = null;
            decimal? grade11Weight = null;
            if (!isOneYear)
            {
                grade11Percentage = CalculateKuwaitiGradeLevelPercentage(kw.Grade11Subjects, KuwaitiConstants.Grade11MaxMarks, 11, student);
                grade11Weight = kw.Grade11Weight ?? 0;
            }

            decimal grade12Percentage = CalculateKuwaitiGradeLevelPercentage(kw.Grade12Subjects, KuwaitiConstants.Grade12MaxMarks, 12, student);

            // §"One Year" — the student only has grade 12 on their certificate, so it alone carries 100%.
            // Weights for the multi-year cases are entered by the student from their own official
            // certificate (each year's contribution to the cumulative average is printed there),
            // rather than derived from a hardcoded graduation-year table.
            decimal grade12Weight = isOneYear ? 100m : (kw.Grade12Weight ?? 0);

            decimal finalPercentage;
            if (isOneYear)
            {
                finalPercentage = grade12Percentage;
            }
            else
            {
                finalPercentage = (grade11Percentage!.Value * grade11Weight!.Value / 100)
                                 + (grade12Percentage * grade12Weight / 100);
                if (isThreeYears)
                    finalPercentage += grade10Percentage!.Value * grade10Weight!.Value / 100;
            }

            // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the
            // equivalent total (المجموع الاعتباري / المجموع المصري) must be derived from it, never
            // from the raw unrounded intermediate value.
            finalPercentage = Math.Round(finalPercentage, 2);
            decimal equivalentTotal = (finalPercentage / 100) * EquivalencyConstants.EgyptianScientificTrackTotal;

            student.KuwaitiTotals = new KuwaitiStudentTotals
            {
                Student = student,
                YearsCount = kw.YearsCount,
                Grade10Percentage = grade10Percentage.HasValue ? Math.Round(grade10Percentage.Value, 2) : null,
                Grade11Percentage = grade11Percentage.HasValue ? Math.Round(grade11Percentage.Value, 2) : null,
                Grade12Percentage = Math.Round(grade12Percentage, 2),
                Grade10Weight = grade10Weight,
                Grade11Weight = grade11Weight,
                Grade12Weight = grade12Weight,
                FinalPercentage = finalPercentage,
                EquivalentTotal = Math.Round(equivalentTotal, 2),
                HasSecondAttempt = kw.HasSecondAttempt
            };
        }

        // §1.3 — gradePercentage = (Σ obtained ÷ Σ fixed maxMark of counted subjects) × 100.
        // Max marks come from the server-side KuwaitiConstants table — never accepted from the client.
        private decimal CalculateKuwaitiGradeLevelPercentage(
            List<KuwaitiSubjectGradeCreateDto>? subjects, Dictionary<string, decimal> maxMarks, int gradeLevel, Student student)
        {
            if (subjects == null || subjects.Count == 0)
                throw new ArgumentException($"بيانات مواد الصف {gradeLevel} مفقودة.");

            decimal totalObtained = 0;
            decimal totalMax = 0;

            foreach (var subject in subjects)
            {
                // Defence in depth: the validator already enforces an exact match against the
                // counted-subject list (§1.1) and rejects excluded subjects (§1.2).
                if (!maxMarks.TryGetValue(subject.SubjectName, out var maxMark))
                    continue;

                totalObtained += subject.Obtained;
                totalMax += maxMark;

                student.StandardGrades.Add(new StandardStudentGrades
                {
                    Student = student,
                    YearOfStudy = gradeLevel.ToString(),
                    SubjectName = subject.SubjectName,
                    Grade = subject.Obtained,
                    MaxMark = maxMark,
                    WeightedPercentage = maxMark > 0 ? Math.Round((subject.Obtained / maxMark) * 100, 2) : 0,
                    Achieved = subject.Obtained,
                    GradeLevel = gradeLevel
                });
            }

            return totalMax > 0 ? (totalObtained / totalMax) * 100 : 0;
        }

        private void ProcessQatariCertificate(StudentCreateDto dto, Student student)
        {
            var qa = dto.QatariData;
            if (qa == null)
                throw new ArgumentException("بيانات الشهادة القطرية مفقودة.");

            if (dto.Track != QatariConstants.ScientificTrack)
                throw new ArgumentException(QatariConstants.NonScientificTrackError);

            var (finalTotal, percentage) = ProcessSingleYearFixedTotalCertificate(
                qa.Subjects, QatariConstants.ScientificTrackSubjects, student,
                "بيانات المواد والدرجات للشهادة القطرية مفقودة.");

            // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the
            // equivalent total (المجموع الاعتباري / المجموع المصري) must be derived from it, never
            // from the raw unrounded intermediate value.
            percentage = Math.Round(percentage, 2);
            decimal equivalentTotal = (percentage / 100m) * EquivalencyConstants.EgyptianScientificTrackTotal;

            student.QatariTotals = new QatariStudentTotals
            {
                Student = student,
                FinalTotal = Math.Round(finalTotal, 2),
                Percentage = percentage,
                EquivalentTotal = Math.Round(equivalentTotal, 2)
            };
        }

        private void ProcessOmaniCertificate(StudentCreateDto dto, Student student)
        {
            var om = dto.OmaniData;
            if (om == null)
                throw new ArgumentException("بيانات الشهادة العمانية مفقودة.");

            var (finalTotal, percentage) = ProcessSingleYearFixedTotalCertificate(
                om.Subjects, OmaniConstants.Subjects, student,
                "بيانات المواد والدرجات للشهادة العمانية مفقودة.");

            // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the
            // equivalent total (المجموع الاعتباري / المجموع المصري) must be derived from it, never
            // from the raw unrounded intermediate value.
            percentage = Math.Round(percentage, 2);
            decimal equivalentTotal = (percentage / 100m) * EquivalencyConstants.EgyptianScientificTrackTotal;

            student.OmaniTotals = new OmaniStudentTotals
            {
                Student = student,
                FinalTotal = Math.Round(finalTotal, 2),
                Percentage = percentage,
                EquivalentTotal = Math.Round(equivalentTotal, 2)
            };
        }

        private void ProcessYemeniCertificate(StudentCreateDto dto, Student student)
        {
            var ye = dto.YemeniData;
            if (ye == null)
                throw new ArgumentException("بيانات الشهادة اليمنية مفقودة.");

            var (finalTotal, percentage) = ProcessSingleYearFixedTotalCertificate(
                ye.Subjects, YemeniConstants.Subjects, student,
                "بيانات المواد والدرجات للشهادة اليمنية مفقودة.");

            // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the
            // equivalent total (المجموع الاعتباري / المجموع المصري) must be derived from it, never
            // from the raw unrounded intermediate value.
            percentage = Math.Round(percentage, 2);
            decimal equivalentTotal = (percentage / 100m) * EquivalencyConstants.EgyptianScientificTrackTotal;

            student.YemeniTotals = new YemeniStudentTotals
            {
                Student = student,
                FinalTotal = Math.Round(finalTotal, 2),
                Percentage = percentage,
                EquivalentTotal = Math.Round(equivalentTotal, 2)
            };
        }

        // Egyptian-equivalency for the Bahraini certificate (last-two-years, track-dependent subject
        // list). Reuses the shared single-year fixed-total math, then scales the percentage to /410
        // exactly like the Kuwaiti/IG/Saudi/Qatari/Omani/Yemeni formulas.
        private void ProcessBahrainiCertificate(StudentCreateDto dto, Student student)
        {
            var ba = dto.BahrainiData;
            if (ba == null)
                throw new ArgumentException("بيانات الشهادة البحرينية مفقودة.");

            string[] subjectList = BahrainiConstants.GetTrackSubjects(dto.Track);
            if (subjectList.Length == 0)
                throw new ArgumentException(BahrainiConstants.VocationalTrackError);

            var (finalTotal, percentage) = ProcessSingleYearFixedTotalCertificate(
                ba.Subjects, subjectList, student,
                "بيانات المواد والدرجات للشهادة البحرينية مفقودة.");

            decimal totalMax = subjectList.Length * SingleYearFixedTotalConstants.MaxMarkPerSubject;

            // Rounded to 2dp FIRST — this is the exact percentage displayed on the site, and the
            // equivalent total (المجموع الاعتباري / المجموع المصري) must be derived from it, never
            // from the raw unrounded intermediate value.
            percentage = Math.Round(percentage, 2);
            decimal equivalentTotal = (percentage / 100m) * EquivalencyConstants.EgyptianScientificTrackTotal;

            student.BahrainiTotals = new BahrainiStudentTotals
            {
                Student = student,
                Track = dto.Track,
                FinalTotal = Math.Round(finalTotal, 2),
                TotalMax = totalMax,
                Percentage = percentage,
                EquivalentTotal = Math.Round(equivalentTotal, 2)
            };
        }

        // §1.1/1.2 — percentage-in only: no subjects, no max marks, no denominator, no
        // StandardStudentGrades rows. The student's typed percentage is echoed back (rounded to
        // 2dp) and converted via the shared CalculateEquivalentTotal helper. Track (علمي/أدبي) is
        // recorded as Branch but never forks the calculation.
        private void ProcessPalestinianCertificate(StudentCreateDto dto, Student student)
        {
            var pa = dto.PalestinianData;
            if (pa == null)
                throw new ArgumentException("بيانات الشهادة الفلسطينية مفقودة.");

            decimal percentage = Math.Round(pa.Percentage, 2);

            student.PalestinianTotals = new PalestinianStudentTotals
            {
                Student = student,
                Percentage = percentage,
                EquivalentTotal = CalculateEquivalentTotal(percentage),
                Branch = pa.Branch
            };
        }

        // Shared conversion: المجموع الاعتباري (المجموع المصري) = (displayedPercentage ÷ 100) × 410.
        // Takes the percentage as already displayed/rounded to 2dp — never a raw unrounded value —
        // so the number on screen and the number persisted always agree.
        private static decimal CalculateEquivalentTotal(decimal displayedPercentage) =>
            Math.Round((displayedPercentage / 100m) * EquivalencyConstants.EgyptianScientificTrackTotal, 2);

        // §1.2/1.3 — "أخرى": percentage-in only, free-text certificate name, no track selector at
        // all. No equivalent-total conversion for this certificate — percentage only, by explicit
        // product decision (unlike every other percentage-in certificate in this system).
        private void ProcessOtherCertificate(StudentCreateDto dto, Student student)
        {
            var ot = dto.OtherData;
            if (ot == null)
                throw new ArgumentException("بيانات الشهادة مفقودة.");

            student.OtherTotals = new OtherStudentTotals
            {
                Student = student,
                CertificateName = ot.CertificateName.Trim(),
                Percentage = Math.Round(ot.Percentage, 2)
            };
        }

        // §Egyptian — this IS the target Egyptian certificate itself, so there is no equivalent-total
        // conversion (unlike every foreign certificate above). The denominator is fixed by subject
        // system alone (320 حديث / 410 قديم) — it is NEVER derived from the sum of the visible
        // fields' own max marks, which is an intentional business rule (they don't add up to it).
        private void ProcessEgyptianCertificate(StudentCreateDto dto, Student student)
        {
            var eg = dto.EgyptianData;
            if (eg == null)
                throw new ArgumentException("بيانات الثانوية العامة المصرية مفقودة.");

            if (!EgyptianConstants.GetAllowedTracksForCollege(dto.WishCollege).Contains(dto.Track))
                throw new ArgumentException("المسار المختار غير متاح للكلية المحددة في قسم الرغبة.");

            var maxMarks = EgyptianConstants.GetSubjectMaxMarks(dto.Track, eg.SubjectSystem);

            if (eg.Subjects == null || eg.Subjects.Count == 0)
                throw new ArgumentException("بيانات المواد والدرجات للثانوية العامة المصرية مفقودة.");

            decimal finalTotal = 0;
            foreach (var subject in eg.Subjects)
            {
                // Defence in depth: the validator already enforces an exact match against the
                // track+system's required subject set and each mark's own max-mark range.
                if (!maxMarks.TryGetValue(subject.SubjectName, out var maxMark))
                    continue;

                finalTotal += subject.Mark;

                student.StandardGrades.Add(new StandardStudentGrades
                {
                    Student = student,
                    YearOfStudy = "12",
                    SubjectName = subject.SubjectName,
                    Grade = subject.Mark,
                    MaxMark = maxMark,
                    WeightedPercentage = Math.Round((subject.Mark / maxMark) * 100, 2),
                    Achieved = subject.Mark,
                    GradeLevel = 12
                });
            }

            decimal denominator = EgyptianConstants.GetDenominator(eg.SubjectSystem);
            decimal percentage = denominator > 0 ? Math.Round((finalTotal / denominator) * 100, 2) : 0;

            student.EgyptianTotals = new EgyptianStudentTotals
            {
                Student = student,
                Track = dto.Track,
                SubjectSystem = eg.SubjectSystem,
                FinalTotal = Math.Round(finalTotal, 2),
                Denominator = denominator,
                Percentage = percentage
            };
        }

        // §Azhar — fixed subject list per قسم (no subject-system variant, unlike Egyptian). المواد
        // الشرعية are never modeled at all, so there is nothing to exclude here. المجموع الاعتباري
        // (المجموع المصري) uses the same shared CalculateEquivalentTotal helper as every other
        // foreign certificate (Percentage × 4.1 = (Percentage / 100) × 410).
        private void ProcessAzharCertificate(StudentCreateDto dto, Student student)
        {
            var az = dto.AzharData;
            if (az == null)
                throw new ArgumentException("بيانات الثانوية الأزهرية مفقودة.");

            if (!AzharConstants.GetAllowedSectionsForCollege(dto.WishCollege).Contains(dto.Track))
                throw new ArgumentException("القسم المختار غير متاح للكلية المحددة في قسم الرغبة.");

            var maxMarks = AzharConstants.GetSubjectMaxMarks(dto.Track);

            if (az.Subjects == null || az.Subjects.Count == 0)
                throw new ArgumentException("بيانات المواد والدرجات للثانوية الأزهرية مفقودة.");

            decimal finalTotal = 0;
            foreach (var subject in az.Subjects)
            {
                // Defence in depth: the validator already enforces an exact match against the
                // قسم's required subject set and each mark's own max-mark range.
                if (!maxMarks.TryGetValue(subject.SubjectName, out var maxMark))
                    continue;

                finalTotal += subject.Mark;

                student.StandardGrades.Add(new StandardStudentGrades
                {
                    Student = student,
                    YearOfStudy = "12",
                    SubjectName = subject.SubjectName,
                    Grade = subject.Mark,
                    MaxMark = maxMark,
                    WeightedPercentage = Math.Round((subject.Mark / maxMark) * 100, 2),
                    Achieved = subject.Mark,
                    GradeLevel = 12
                });
            }

            decimal denominator = AzharConstants.GetDenominator(dto.Track);
            decimal percentage = denominator > 0 ? Math.Round((finalTotal / denominator) * 100, 2) : 0;

            student.AzharTotals = new AzharStudentTotals
            {
                Student = student,
                Section = dto.Track,
                FinalTotal = Math.Round(finalTotal, 2),
                Denominator = denominator,
                Percentage = percentage,
                EquivalentTotal = CalculateEquivalentTotal(percentage)
            };
        }

        // §Emirati — core subjects (5) are always required; optional subjects (Chemistry/Health
        // Sciences/Biology) are counted — added to BOTH the numerator and the denominator — only if
        // the student actually submits a mark for them. That variable-length denominator is the one
        // difference from ProcessSingleYearFixedTotalCertificate (which assumes a fixed-length exact
        // subject match), so Emirati gets its own small loop instead of reusing it. المجموع الاعتباري
        // (المجموع المصري) uses the same shared CalculateEquivalentTotal helper as every other
        // foreign certificate.
        private void ProcessEmiratiCertificate(StudentCreateDto dto, Student student)
        {
            var em = dto.EmiratiData;
            if (em?.Subjects == null || em.Subjects.Count == 0)
                throw new ArgumentException("بيانات المواد والدرجات للشهادة الإماراتية مفقودة.");

            decimal finalTotal = 0;
            int countedSubjects = 0;

            foreach (var subject in em.Subjects)
            {
                // Defence in depth: the validator already enforces the core+optional subject rules.
                bool isCounted = EmiratiConstants.CoreSubjects.Contains(subject.SubjectName)
                    || EmiratiConstants.OptionalSubjects.Contains(subject.SubjectName);
                if (!isCounted)
                    continue;

                finalTotal += subject.Mark;
                countedSubjects++;

                student.StandardGrades.Add(new StandardStudentGrades
                {
                    Student = student,
                    YearOfStudy = "12",
                    SubjectName = subject.SubjectName,
                    Grade = subject.Mark,
                    MaxMark = SingleYearFixedTotalConstants.MaxMarkPerSubject,
                    WeightedPercentage = Math.Round((subject.Mark / SingleYearFixedTotalConstants.MaxMarkPerSubject) * 100, 2),
                    Achieved = subject.Mark,
                    GradeLevel = 12
                });
            }

            decimal denominator = countedSubjects * SingleYearFixedTotalConstants.MaxMarkPerSubject;
            decimal percentage = denominator > 0 ? Math.Round((finalTotal / denominator) * 100, 2) : 0;

            student.EmiratiTotals = new EmiratiStudentTotals
            {
                Student = student,
                FinalTotal = Math.Round(finalTotal, 2),
                Denominator = denominator,
                Percentage = percentage,
                EquivalentTotal = CalculateEquivalentTotal(percentage)
            };
        }

        // §American Diploma — no EquivalentTotal at all (unlike every certificate above): admission
        // depends on BasePercentage + SatI + SatII together, not one combined number (§8). The
        // student's 8 best subjects have no fixed names, so they're stored as "المادة N" rows.
        // The 1050/1100 admission minimums (§6) are recorded as informational flags only — never
        // enforced as a rejection here (that already happened, correctly, nowhere: the validator
        // never blocks on them either).
        private void ProcessAmericanDiplomaCertificate(StudentCreateDto dto, Student student)
        {
            var am = dto.AmericanDiplomaData;
            if (am?.BestEightScores == null || am.BestEightScores.Count != AmericanDiplomaConstants.BestSubjectsCount)
                throw new ArgumentException("يجب إدخال درجات أفضل 8 مواد.");

            decimal sum = 0;
            for (int i = 0; i < am.BestEightScores.Count; i++)
            {
                decimal score = am.BestEightScores[i];
                sum += score;

                student.StandardGrades.Add(new StandardStudentGrades
                {
                    Student = student,
                    YearOfStudy = "12",
                    SubjectName = $"المادة {i + 1}",
                    Grade = score,
                    MaxMark = AmericanDiplomaConstants.MaxMarkPerSubject,
                    WeightedPercentage = Math.Round((score / AmericanDiplomaConstants.MaxMarkPerSubject) * 100, 2),
                    Achieved = score,
                    GradeLevel = 12
                });
            }

            decimal average = sum / AmericanDiplomaConstants.BestSubjectsCount;
            decimal basePercentage = Math.Round(average * AmericanDiplomaConstants.BasePercentageWeight / 100m, 2);

            bool requiresSatII = AmericanDiplomaConstants.RequiresSatII(dto.WishCollege);

            student.AmericanDiplomaTotals = new AmericanDiplomaStudentTotals
            {
                Student = student,
                AverageScore = Math.Round(average, 2),
                BasePercentage = basePercentage,
                SatI = am.SatI,
                SatII = requiresSatII ? am.SatII : null,
                SatIISubject1 = requiresSatII ? am.SatIISubject1 : null,
                SatIISubject2 = requiresSatII ? am.SatIISubject2 : null,
                SatIBelowMinimum = am.SatI < AmericanDiplomaConstants.SatIMinimumThreshold,
                SatIIBelowMinimum = requiresSatII && am.SatII.HasValue && am.SatII.Value < AmericanDiplomaConstants.SatIIMinimumThreshold
            };
        }

        // Shared by Qatari, Omani, Yemeni and Bahraini: single grade level, every subject fixed at 100 each,
        // no weights. §1.4 — finalTotal/percentage recomputed entirely from raw marks against the
        // certificate's fixed subject list; the client-sent percentage is never trusted. The
        // denominator is derived from the certificate's own subject count (never from the submitted
        // rows) so it varies per cert — 700 for Qatari/Omani's 7 subjects, 600 for Yemeni's 6.
        private (decimal finalTotal, decimal percentage) ProcessSingleYearFixedTotalCertificate(
            List<SingleYearSubjectMarkCreateDto>? subjects, string[] subjectList, Student student, string missingDataMessage)
        {
            if (subjects == null || subjects.Count == 0)
                throw new ArgumentException(missingDataMessage);

            decimal finalTotal = 0;
            foreach (var subject in subjects)
            {
                // Defence in depth: the validator already enforces an exact match against the
                // certificate's subject list (and rejects any excluded subject, where applicable).
                if (!subjectList.Contains(subject.SubjectName))
                    continue;

                finalTotal += subject.Mark;

                student.StandardGrades.Add(new StandardStudentGrades
                {
                    Student = student,
                    YearOfStudy = "12",
                    SubjectName = subject.SubjectName,
                    Grade = subject.Mark,
                    MaxMark = SingleYearFixedTotalConstants.MaxMarkPerSubject,
                    WeightedPercentage = Math.Round((subject.Mark / SingleYearFixedTotalConstants.MaxMarkPerSubject) * 100, 2),
                    Achieved = subject.Mark,
                    GradeLevel = 12
                });
            }

            decimal totalMaxMark = subjectList.Length * SingleYearFixedTotalConstants.MaxMarkPerSubject;
            decimal percentage = (finalTotal / totalMaxMark) * 100;
            return (finalTotal, percentage);
        }

        private int GetIgPoints(string gradeType, string grade)
        {
            // Point system translation matching frontend logic
            return gradeType switch
            {
                "igcse-legacy" => grade switch
                {
                    "A_STAR" => 8,
                    "A" => 7,
                    "B" => 6,
                    "C" => 5,
                    _ => 0
                },
                "igcse-numeric" => grade switch
                {
                    "9" => 8,
                    "8" => 7,
                    "7" => 6,
                    "6" => 5,
                    "5" => 4,
                    "4" => 3,
                    _ => 0
                },
                "as-level" => grade switch
                {
                    "A" => 5,
                    "B" => 4,
                    "C" => 3,
                    "D" => 2,
                    _ => 0
                },
                "a-level" => grade switch
                {
                    "A_STAR" => 6,
                    "A" => 5,
                    "B" => 4,
                    "C" => 3,
                    "D" => 2,
                    _ => 0
                },
                _ => 0
            };
        }
    }
}
