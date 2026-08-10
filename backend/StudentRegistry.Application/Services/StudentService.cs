using AutoMapper;
using Microsoft.Extensions.Logging;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Editor;
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
        private readonly ILogger<StudentService> _logger;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService, ILogger<StudentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<StudentResponseDto?> GetStudentByIdAsync(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null)
            {
                return null;
            }

            var dto = _mapper.Map<StudentResponseDto>(student);
            var pendingReview = await _unitOfWork.PendingReviews.GetPendingForStudentAsync(id);
            dto.HasPendingReview = pendingReview != null;
            return dto;
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

        public async Task<IEnumerable<StudentListItemDto>> SearchStudentsAsync(string? query)
        {
            var students = await _unitOfWork.Students.SearchAsync(query);
            var mapped = _mapper.Map<List<StudentListItemDto>>(students);

            var studentIds = mapped.Select(m => m.Id).ToList();
            var notes = await _unitOfWork.ReviewNotes.GetByStudentIdsAsync(studentIds);
            // Field comments now cover the same "has an open review remark" signal as ReviewNotes
            // (Viewer's review page posts into FieldComments too, so Editor sees them) — a student
            // is flagged if either source has an entry, old ReviewNotes rows included.
            var fieldComments = await _unitOfWork.FieldComments.GetByStudentIdsAsync(studentIds);
            var notedIds = notes.Select(n => n.StudentId)
                .Concat(fieldComments.Select(c => c.StudentId))
                .ToHashSet();
            var pendingReviews = await _unitOfWork.PendingReviews.GetPendingByStudentIdsAsync(studentIds);
            var pendingIds = pendingReviews.Select(p => p.StudentId).ToHashSet();

            foreach (var item in mapped)
            {
                item.HasReviewNotes = notedIds.Contains(item.Id);
                item.HasPendingReview = pendingIds.Contains(item.Id);
            }

            return mapped;
        }

        public async Task<StudentResponseDto> RegisterStudentAsync(StudentCreateDto createDto)
        {
            _logger.LogInformation("بدء تسجيل استمارة جديدة. الرقم القومي: {NationalId}، الشهادة: {Certification}.", createDto.NationalId, createDto.Certification);

            // 0. Idempotency: if this exact submission attempt already succeeded (e.g. the client
            // retried after a dropped connection but the original request actually landed), return
            // the existing record instead of inserting a duplicate.
            if (!string.IsNullOrWhiteSpace(createDto.SubmissionToken))
            {
                var existingByToken = await _unitOfWork.Students.GetBySubmissionTokenAsync(createDto.SubmissionToken);
                if (existingByToken != null)
                {
                    _logger.LogInformation("طلب تسجيل مكرر (نفس رمز الإرسال) للرقم القومي {NationalId} — تم إرجاع السجل الموجود بدلاً من الإدخال مرة أخرى.", createDto.NationalId);
                    return _mapper.Map<StudentResponseDto>(existingByToken);
                }
            }

            // 1. Verify uniqueness of NationalId
            var existingStudent = await _unitOfWork.Students.GetByNationalIdAsync(createDto.NationalId);
            if (existingStudent != null)
            {
                _logger.LogWarning("محاولة تسجيل مرفوضة: الرقم القومي {NationalId} مسجل مسبقاً.", createDto.NationalId);
                throw new InvalidOperationException("رقم قومي مسجل مسبقاً. لا يمكن إدخال نفس الرقم القومي مرتين. تم تسجيل بياناتك بنجاح، ولتعديل أي بيانات يمكن تعديلها عند زيارة الجامعة.");
            }

            // 2. Save image to local wwwroot disk directory
            string relativePhotoPath = await _fileStorageService.SaveBase64ImageAsync(createDto.Photo, createDto.NationalId);

            // 3. Map base Student info
            var student = _mapper.Map<Student>(createDto);
            student.PhotoPath = relativePhotoPath;
            student.SubmittedAt = DateTime.UtcNow;
            student.SubmissionToken = string.IsNullOrWhiteSpace(createDto.SubmissionToken) ? null : createDto.SubmissionToken;

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
            try
            {
                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                // The photo was already written to disk at this point (step 2) — if the DB write
                // fails, that file is now orphaned on disk with no matching row. Logged so an
                // orphaned-uploads cleanup can find it later; the exception itself still propagates
                // to ExceptionMiddleware for the HTTP response as before.
                _logger.LogError(ex, "فشل حفظ بيانات الطالب في قاعدة البيانات. الرقم القومي: {NationalId}. الصورة المرفوعة (قد تكون يتيمة الآن): {PhotoPath}", createDto.NationalId, relativePhotoPath);
                throw;
            }

            _logger.LogInformation("تم تسجيل استمارة الطالب بنجاح. المعرف: {StudentId}، الرقم القومي: {NationalId}.", student.Id, createDto.NationalId);

            // 6. Return mapped response
            return _mapper.Map<StudentResponseDto>(student);
        }

        // Dispatches to the one recompute routine matching this student's certificate type, re-deriving
        // its totals from whatever raw grade rows are currently in the DB — the same rows the Editor's
        // per-cell inline edit (FieldEditService) may have just changed. Each changed total field is
        // recorded as its own FieldEdit row (Source = "recalculate") so the edits sheet shows exactly
        // what the recalculation changed, under the real editor's username.
        public async Task<StudentResponseDto> RecalculateStudentTotalsAsync(int studentId, string editorUsername)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new KeyNotFoundException("الطالب غير موجود.");
            }

            var changes = new List<(string Group, string Property, string? OldValue, string? NewValue)>();

            if (student.SaudiTotals != null)
            {
                RecalculateSaudiTotals(student, changes);
            }
            else if (student.IgGrades != null)
            {
                RecalculateIgTotals(student, changes);
            }
            else if (student.KuwaitiTotals != null)
            {
                RecalculateKuwaitiTotals(student, changes);
            }
            else if (student.QatariTotals != null)
            {
                var (finalTotal, percentage) = RecalculateFlatFixedSubjectTotal(student.StandardGrades);
                TrackChange(changes, "QatariTotals", "FinalTotal", student.QatariTotals.FinalTotal, finalTotal);
                TrackChange(changes, "QatariTotals", "Percentage", student.QatariTotals.Percentage, percentage);
                var equivalentTotal = CalculateEquivalentTotal(percentage);
                TrackChange(changes, "QatariTotals", "EquivalentTotal", student.QatariTotals.EquivalentTotal, equivalentTotal);
                student.QatariTotals.FinalTotal = finalTotal;
                student.QatariTotals.Percentage = percentage;
                student.QatariTotals.EquivalentTotal = equivalentTotal;
            }
            else if (student.OmaniTotals != null)
            {
                var (finalTotal, percentage) = RecalculateFlatFixedSubjectTotal(student.StandardGrades);
                TrackChange(changes, "OmaniTotals", "FinalTotal", student.OmaniTotals.FinalTotal, finalTotal);
                TrackChange(changes, "OmaniTotals", "Percentage", student.OmaniTotals.Percentage, percentage);
                var equivalentTotal = CalculateEquivalentTotal(percentage);
                TrackChange(changes, "OmaniTotals", "EquivalentTotal", student.OmaniTotals.EquivalentTotal, equivalentTotal);
                student.OmaniTotals.FinalTotal = finalTotal;
                student.OmaniTotals.Percentage = percentage;
                student.OmaniTotals.EquivalentTotal = equivalentTotal;
            }
            else if (student.YemeniTotals != null)
            {
                var (finalTotal, percentage) = RecalculateFlatFixedSubjectTotal(student.StandardGrades);
                TrackChange(changes, "YemeniTotals", "FinalTotal", student.YemeniTotals.FinalTotal, finalTotal);
                TrackChange(changes, "YemeniTotals", "Percentage", student.YemeniTotals.Percentage, percentage);
                var equivalentTotal = CalculateEquivalentTotal(percentage);
                TrackChange(changes, "YemeniTotals", "EquivalentTotal", student.YemeniTotals.EquivalentTotal, equivalentTotal);
                student.YemeniTotals.FinalTotal = finalTotal;
                student.YemeniTotals.Percentage = percentage;
                student.YemeniTotals.EquivalentTotal = equivalentTotal;
            }
            else if (student.BahrainiTotals != null)
            {
                var finalTotal = Math.Round(student.StandardGrades.Sum(g => g.Grade), 2);
                var percentage = student.BahrainiTotals.TotalMax > 0
                    ? Math.Round((finalTotal / student.BahrainiTotals.TotalMax) * 100, 2)
                    : 0;
                TrackChange(changes, "BahrainiTotals", "FinalTotal", student.BahrainiTotals.FinalTotal, finalTotal);
                TrackChange(changes, "BahrainiTotals", "Percentage", student.BahrainiTotals.Percentage, percentage);
                var equivalentTotal = CalculateEquivalentTotal(percentage);
                TrackChange(changes, "BahrainiTotals", "EquivalentTotal", student.BahrainiTotals.EquivalentTotal, equivalentTotal);
                student.BahrainiTotals.FinalTotal = finalTotal;
                student.BahrainiTotals.Percentage = percentage;
                student.BahrainiTotals.EquivalentTotal = equivalentTotal;
            }
            else if (student.EgyptianTotals != null)
            {
                // Denominator is a fixed constant by design (never derived from the rows' own max
                // marks — see ProcessEgyptianCertificate) so it's read back as-is, never recomputed.
                var finalTotal = Math.Round(student.StandardGrades.Sum(g => g.Grade), 2);
                var percentage = student.EgyptianTotals.Denominator > 0
                    ? Math.Round((finalTotal / student.EgyptianTotals.Denominator) * 100, 2)
                    : 0;
                TrackChange(changes, "EgyptianTotals", "FinalTotal", student.EgyptianTotals.FinalTotal, finalTotal);
                TrackChange(changes, "EgyptianTotals", "Percentage", student.EgyptianTotals.Percentage, percentage);
                student.EgyptianTotals.FinalTotal = finalTotal;
                student.EgyptianTotals.Percentage = percentage;
            }
            else if (student.AzharTotals != null)
            {
                var finalTotal = Math.Round(student.StandardGrades.Sum(g => g.Grade), 2);
                var percentage = student.AzharTotals.Denominator > 0
                    ? Math.Round((finalTotal / student.AzharTotals.Denominator) * 100, 2)
                    : 0;
                TrackChange(changes, "AzharTotals", "FinalTotal", student.AzharTotals.FinalTotal, finalTotal);
                TrackChange(changes, "AzharTotals", "Percentage", student.AzharTotals.Percentage, percentage);
                var equivalentTotal = CalculateEquivalentTotal(percentage);
                TrackChange(changes, "AzharTotals", "EquivalentTotal", student.AzharTotals.EquivalentTotal, equivalentTotal);
                student.AzharTotals.FinalTotal = finalTotal;
                student.AzharTotals.Percentage = percentage;
                student.AzharTotals.EquivalentTotal = equivalentTotal;
            }
            else if (student.EmiratiTotals != null)
            {
                var finalTotal = Math.Round(student.StandardGrades.Sum(g => g.Grade), 2);
                var percentage = student.EmiratiTotals.Denominator > 0
                    ? Math.Round((finalTotal / student.EmiratiTotals.Denominator) * 100, 2)
                    : 0;
                TrackChange(changes, "EmiratiTotals", "FinalTotal", student.EmiratiTotals.FinalTotal, finalTotal);
                TrackChange(changes, "EmiratiTotals", "Percentage", student.EmiratiTotals.Percentage, percentage);
                var equivalentTotal = CalculateEquivalentTotal(percentage);
                TrackChange(changes, "EmiratiTotals", "EquivalentTotal", student.EmiratiTotals.EquivalentTotal, equivalentTotal);
                student.EmiratiTotals.FinalTotal = finalTotal;
                student.EmiratiTotals.Percentage = percentage;
                student.EmiratiTotals.EquivalentTotal = equivalentTotal;
            }
            else if (student.AmericanDiplomaTotals != null)
            {
                RecalculateAmericanDiplomaTotals(student, changes);
            }
            else if (student.PalestinianTotals != null)
            {
                var percentage = Math.Round(student.PalestinianTotals.Percentage, 2);
                var equivalentTotal = CalculateEquivalentTotal(percentage);
                TrackChange(changes, "PalestinianTotals", "EquivalentTotal", student.PalestinianTotals.EquivalentTotal, equivalentTotal);
                student.PalestinianTotals.EquivalentTotal = equivalentTotal;
            }
            else
            {
                throw new ArgumentException("لا توجد درجات مواد يمكن إعادة حسابها لهذا النوع من الشهادات.");
            }

            if (changes.Count == 0)
            {
                return _mapper.Map<StudentResponseDto>(student);
            }

            var now = DateTime.UtcNow;
            foreach (var change in changes)
            {
                await _unitOfWork.FieldEdits.AddAsync(new FieldEdit
                {
                    StudentId = studentId,
                    FieldName = FieldPath.Format(change.Group, null, change.Property),
                    OldValue = change.OldValue,
                    NewValue = change.NewValue,
                    Editor = editorUsername,
                    Source = "recalculate",
                    EditedAt = now
                });
            }

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<StudentResponseDto>(student);
        }

        private static void TrackChange(
            List<(string Group, string Property, string? OldValue, string? NewValue)> changes,
            string group, string property, object? oldValue, object? newValue)
        {
            var oldStr = oldValue?.ToString();
            var newStr = newValue?.ToString();
            if (oldStr != newStr)
            {
                changes.Add((group, property, oldStr, newStr));
            }
        }

        // Mirrors ProcessSaudiCertificate's math exactly, but reads Achieved/Weighted back from the
        // already-persisted SaudiStudentGrades rows (possibly edited) instead of the registration DTO,
        // and re-derives Coefficient the same authoritative way rather than trusting any hand-edited
        // value — an edited Achieved/Weighted pair that no longer divides evenly is still rejected,
        // exactly as it would be at registration time.
        private void RecalculateSaudiTotals(Student student, List<(string, string, string?, string?)> changes)
        {
            var totals = student.SaudiTotals!;
            var yearWeights = GetSaudiYearWeights(totals.YearsCount);

            decimal overallAchieved = 0, overallWeighted = 0, schoolPercentage = 0;
            int overallCoefficients = 0;

            foreach (var yearGroup in student.SaudiGrades.GroupBy(g => g.YearLabel))
            {
                decimal yearWeightedSum = 0;
                int yearCoefficientSum = 0;

                foreach (var row in yearGroup)
                {
                    if (row.Achieved <= 0)
                        throw new ArgumentException($"الدرجة المتحصلة لمادة \"{row.SubjectName}\" يجب أن تكون أكبر من الصفر.");

                    decimal rawCoefficient = row.Weighted / row.Achieved;
                    int coefficient = (int)Math.Round(rawCoefficient, MidpointRounding.AwayFromZero);
                    if (Math.Abs(rawCoefficient - coefficient) > 0.001m)
                        throw new ArgumentException($"درجات مادة \"{row.SubjectName}\" غير صحيحة: المعامل الناتج (الموزونة ÷ المتحصلة) ليس رقماً صحيحاً.");

                    row.Coefficient = coefficient;

                    overallAchieved += row.Achieved;
                    overallWeighted += row.Weighted;
                    overallCoefficients += coefficient;
                    yearWeightedSum += row.Weighted;
                    yearCoefficientSum += coefficient;
                }

                if (yearCoefficientSum <= 0)
                    throw new ArgumentException($"لا يمكن حساب نسبة السنة \"{yearGroup.Key}\" — مجموع المعاملات صفر.");
                if (!yearWeights.TryGetValue(yearGroup.Key, out decimal weightPercent))
                    throw new ArgumentException($"لا يوجد وزن معرف للسنة \"{yearGroup.Key}\" مع عدد سنوات الدراسة \"{totals.YearsCount}\".");

                decimal yearPercentage = yearWeightedSum / yearCoefficientSum;
                schoolPercentage += yearPercentage * (weightPercent / 100m);
            }

            decimal finalPercentage = Math.Round((Math.Round(schoolPercentage, 2) + totals.AptitudeScore) / 2, 2);
            decimal equivalentTotal = Math.Round((finalPercentage / 100m) * EquivalencyConstants.EgyptianScientificTrackTotal, 2);

            TrackChange(changes, "SaudiTotals", "TotalAchieved", totals.TotalAchieved, Math.Round(overallAchieved, 2));
            TrackChange(changes, "SaudiTotals", "TotalWeighted", totals.TotalWeighted, Math.Round(overallWeighted, 2));
            TrackChange(changes, "SaudiTotals", "TotalCoefficients", totals.TotalCoefficients, overallCoefficients);
            TrackChange(changes, "SaudiTotals", "SchoolPercentage", totals.SchoolPercentage, Math.Round(schoolPercentage, 2));
            TrackChange(changes, "SaudiTotals", "FinalPercentage", totals.FinalPercentage, finalPercentage);
            TrackChange(changes, "SaudiTotals", "EquivalentTotal", totals.EquivalentTotal, equivalentTotal);

            totals.TotalAchieved = Math.Round(overallAchieved, 2);
            totals.TotalWeighted = Math.Round(overallWeighted, 2);
            totals.TotalCoefficients = overallCoefficients;
            totals.SchoolPercentage = Math.Round(schoolPercentage, 2);
            totals.FinalPercentage = finalPercentage;
            totals.EquivalentTotal = equivalentTotal;
        }

        // Mirrors ProcessIgCertificate's math, reading IgProgram/Factor/SportsBonus back from the
        // already-persisted IgStudentGrades row (possibly edited) and the subject counts from
        // IgStudentGradeCounts, instead of the registration DTO.
        private void RecalculateIgTotals(Student student, List<(string, string, string?, string?)> changes)
        {
            var totals = student.IgGrades!;
            int maxPointVal = totals.IgProgram switch
            {
                "IGCSE" => 8,
                "AS-Levels" => 5,
                "A-Levels" => 6,
                _ => 8
            };

            int totalPoints = 0, totalSubjects = 0;
            foreach (var count in student.IgGradeCounts)
            {
                totalPoints += count.Count * GetIgPoints(count.GradeType, count.Grade);
                totalSubjects += count.Count;
            }

            int maxPoints = totalSubjects * maxPointVal;
            decimal scorePercentage = maxPoints > 0 ? ((decimal)totalPoints / maxPoints) * 100 : 0m;

            if (totals.Factor > 0)
            {
                scorePercentage *= totals.Factor;
            }
            scorePercentage += totals.SportsBonus;
            scorePercentage = Math.Round(scorePercentage, 2);
            decimal governmentScore = Math.Round((scorePercentage / 100) * EquivalencyConstants.EgyptianScientificTrackTotal, 2);

            TrackChange(changes, "IgGrades", "ScorePercentage", totals.ScorePercentage, scorePercentage);
            TrackChange(changes, "IgGrades", "GovernmentScore", totals.GovernmentScore, governmentScore);

            totals.ScorePercentage = scorePercentage;
            totals.GovernmentScore = governmentScore;
        }

        // Mirrors ProcessKuwaitiCertificate's math, reading each grade-level's obtained/max marks back
        // from the already-persisted StandardStudentGrades rows (filtered by GradeLevel) and the
        // per-level weights back from KuwaitiStudentTotals (both possibly edited), instead of the
        // registration DTO.
        private void RecalculateKuwaitiTotals(Student student, List<(string, string, string?, string?)> changes)
        {
            var totals = student.KuwaitiTotals!;
            bool isOneYear = totals.YearsCount == KuwaitiConstants.OneYear;
            bool isThreeYears = totals.YearsCount == KuwaitiConstants.ThreeYears;

            decimal? grade10Percentage = isThreeYears ? CalculateGradeLevelPercentage(student, 10) : null;
            decimal? grade11Percentage = !isOneYear ? CalculateGradeLevelPercentage(student, 11) : null;
            decimal grade12Percentage = CalculateGradeLevelPercentage(student, 12) ?? 0;

            decimal grade12Weight = isOneYear ? 100m : totals.Grade12Weight;
            decimal finalPercentage;
            if (isOneYear)
            {
                finalPercentage = grade12Percentage;
            }
            else
            {
                finalPercentage = (grade11Percentage!.Value * (totals.Grade11Weight ?? 0) / 100)
                                 + (grade12Percentage * grade12Weight / 100);
                if (isThreeYears)
                    finalPercentage += grade10Percentage!.Value * (totals.Grade10Weight ?? 0) / 100;
            }

            finalPercentage = Math.Round(finalPercentage, 2);
            decimal equivalentTotal = Math.Round((finalPercentage / 100) * EquivalencyConstants.EgyptianScientificTrackTotal, 2);

            var newGrade10 = grade10Percentage.HasValue ? Math.Round(grade10Percentage.Value, 2) : (decimal?)null;
            var newGrade11 = grade11Percentage.HasValue ? Math.Round(grade11Percentage.Value, 2) : (decimal?)null;
            var newGrade12 = Math.Round(grade12Percentage, 2);

            TrackChange(changes, "KuwaitiTotals", "Grade10Percentage", totals.Grade10Percentage, newGrade10);
            TrackChange(changes, "KuwaitiTotals", "Grade11Percentage", totals.Grade11Percentage, newGrade11);
            TrackChange(changes, "KuwaitiTotals", "Grade12Percentage", totals.Grade12Percentage, newGrade12);
            TrackChange(changes, "KuwaitiTotals", "FinalPercentage", totals.FinalPercentage, finalPercentage);
            TrackChange(changes, "KuwaitiTotals", "EquivalentTotal", totals.EquivalentTotal, equivalentTotal);

            totals.Grade10Percentage = newGrade10;
            totals.Grade11Percentage = newGrade11;
            totals.Grade12Percentage = newGrade12;
            totals.FinalPercentage = finalPercentage;
            totals.EquivalentTotal = equivalentTotal;
        }

        private static decimal? CalculateGradeLevelPercentage(Student student, int gradeLevel)
        {
            var rows = student.StandardGrades.Where(g => g.GradeLevel == gradeLevel).ToList();
            if (rows.Count == 0) return null;

            decimal totalObtained = rows.Sum(r => r.Grade);
            decimal totalMax = rows.Sum(r => r.MaxMark ?? 0);
            return totalMax > 0 ? (totalObtained / totalMax) * 100 : 0;
        }

        // Shared by Qatari/Omani/Yemeni recalculation: unlike Bahraini/Egyptian/Azhar/Emirati, these
        // three don't persist their own denominator field, so it's re-derived from the sum of each row's
        // own (possibly-edited) MaxMark — which defaults to the certificate's fixed 100-per-subject at
        // registration time, matching ProcessSingleYearFixedTotalCertificate's fixed denominator unless
        // an editor has deliberately changed a row's MaxMark too.
        private static (decimal finalTotal, decimal percentage) RecalculateFlatFixedSubjectTotal(IEnumerable<StandardStudentGrades> rows)
        {
            var list = rows.ToList();
            decimal finalTotal = Math.Round(list.Sum(r => r.Grade), 2);
            decimal totalMax = list.Sum(r => r.MaxMark ?? SingleYearFixedTotalConstants.MaxMarkPerSubject);
            decimal percentage = totalMax > 0 ? Math.Round((finalTotal / totalMax) * 100, 2) : 0;
            return (finalTotal, percentage);
        }

        // Mirrors ProcessAmericanDiplomaCertificate's math, reading the 8 best-subject scores back from
        // the already-persisted StandardStudentGrades rows and the SAT/ACT test info back from
        // AmericanDiplomaStudentTotals (both possibly edited), instead of the registration DTO.
        private void RecalculateAmericanDiplomaTotals(Student student, List<(string, string, string?, string?)> changes)
        {
            var totals = student.AmericanDiplomaTotals!;
            var rows = student.StandardGrades.ToList();
            if (rows.Count == 0)
                throw new ArgumentException($"يجب وجود {AmericanDiplomaConstants.BestSubjectsCount} مواد على الأقل لإعادة الحساب.");

            decimal sum = rows.Sum(r => r.Grade);
            decimal average = sum / rows.Count;
            decimal basePercentage = Math.Round(average * AmericanDiplomaConstants.BasePercentageWeight / 100m, 2);

            bool isAct1 = totals.TestType1 == AmericanDiplomaConstants.TestTypeAct;
            int satI = isAct1 && totals.ActComposite.HasValue
                ? AmericanDiplomaConstants.ConvertActToSat(totals.ActComposite.Value)
                : totals.SatI;

            bool satIIApplicable = AmericanDiplomaConstants.IsSatIIApplicable(student.WishCollege);
            bool isAct2 = totals.TestType2 == AmericanDiplomaConstants.TestTypeAct;
            bool satIIProvided = satIIApplicable && (isAct2 ? totals.ActMath.HasValue : totals.SatII.HasValue);
            int? satII = satIIProvided
                ? (isAct2 ? AmericanDiplomaConstants.ConvertActMathToSatMath(totals.ActMath!.Value) : totals.SatII)
                : null;

            int equivalentWeight = satI >= AmericanDiplomaConstants.EquivalentFormulaBonusThreshold
                ? AmericanDiplomaConstants.EquivalentFormulaWeightWithBonus
                : AmericanDiplomaConstants.EquivalentFormulaWeightBase;

            decimal testIContribution = (decimal)satI / AmericanDiplomaConstants.SatMax * equivalentWeight;
            decimal testIIContribution = (satIIProvided && satII!.Value >= AmericanDiplomaConstants.EquivalentFormulaSatIICountThreshold)
                ? (decimal)satII.Value / AmericanDiplomaConstants.SatMax * AmericanDiplomaConstants.EquivalentFormulaSatIIWeight
                : 0m;

            decimal equivalentPercentage = Math.Round(testIContribution + testIIContribution + basePercentage, 2);
            bool satIBelowMinimum = satI < AmericanDiplomaConstants.SatIMinimumThreshold;
            bool satIIBelowMinimum = satIIProvided && satII!.Value < AmericanDiplomaConstants.SatIIMinimumThreshold;

            var newAverage = Math.Round(average, 2);

            TrackChange(changes, "AmericanDiplomaTotals", "AverageScore", totals.AverageScore, newAverage);
            TrackChange(changes, "AmericanDiplomaTotals", "BasePercentage", totals.BasePercentage, basePercentage);
            TrackChange(changes, "AmericanDiplomaTotals", "SatI", totals.SatI, satI);
            TrackChange(changes, "AmericanDiplomaTotals", "SatII", totals.SatII, satII);
            TrackChange(changes, "AmericanDiplomaTotals", "SatIBelowMinimum", totals.SatIBelowMinimum, satIBelowMinimum);
            TrackChange(changes, "AmericanDiplomaTotals", "SatIIBelowMinimum", totals.SatIIBelowMinimum, satIIBelowMinimum);

            totals.AverageScore = newAverage;
            totals.BasePercentage = basePercentage;
            totals.SatI = satI;
            totals.SatII = satII;
            totals.SatIBelowMinimum = satIBelowMinimum;
            totals.SatIIBelowMinimum = satIIBelowMinimum;
            // EquivalentPercentage isn't in EditableFieldRegistry (display-only downstream value), so
            // it's kept in sync here without a FieldEdit entry of its own — same treatment as every
            // other non-whitelisted derived field.
            totals.EquivalentPercentage = equivalentPercentage;
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
        // student types in their own best 8 subject names (no fixed list) — stored as-entered.
        // The 1050/1100 admission minimums (§6) are recorded as informational flags only — never
        // enforced as a rejection here (that already happened, correctly, nowhere: the validator
        // never blocks on them either).
        private void ProcessAmericanDiplomaCertificate(StudentCreateDto dto, Student student)
        {
            var am = dto.AmericanDiplomaData;
            if (am?.Subjects == null || am.Subjects.Count != AmericanDiplomaConstants.BestSubjectsCount)
                throw new ArgumentException($"يجب إدخال أفضل {AmericanDiplomaConstants.BestSubjectsCount} مواد.");

            decimal sum = 0;
            foreach (var subject in am.Subjects)
            {
                decimal score = subject.Mark;
                sum += score;

                student.StandardGrades.Add(new StandardStudentGrades
                {
                    Student = student,
                    YearOfStudy = "12",
                    SubjectName = subject.SubjectName,
                    Grade = score,
                    MaxMark = AmericanDiplomaConstants.MaxMarkPerSubject,
                    WeightedPercentage = Math.Round((score / AmericanDiplomaConstants.MaxMarkPerSubject) * 100, 2),
                    Achieved = score,
                    GradeLevel = 12
                });
            }

            decimal average = sum / AmericanDiplomaConstants.BestSubjectsCount;
            decimal basePercentage = Math.Round(average * AmericanDiplomaConstants.BasePercentageWeight / 100m, 2);

            // Level 1: resolve the SAT-equivalent value server-side, authoritatively — a native
            // SAT score passes through unchanged, an ACT composite is converted via the official
            // concordance table before it ever reaches SatI/SatIBelowMinimum below. The client's
            // own SatI value (if any) is never trusted when the test type is ACT.
            bool isAct1 = am.TestType1 == AmericanDiplomaConstants.TestTypeAct;
            int satI = isAct1 ? AmericanDiplomaConstants.ConvertActToSat(am.ActComposite!.Value) : am.SatI;

            // SAT II / Level 2 is only ever persisted when it was both applicable to this college
            // AND actually provided — mandatory (engineering, no checkbox) or optionally filled in
            // (medical, or engineering with the checkbox). The validator already enforced that
            // subjects only accompany an actual level-2 value, and that ACT is only chosen there
            // when the college's fixed subject is Math.
            bool satIIApplicable = AmericanDiplomaConstants.IsSatIIApplicable(dto.WishCollege);
            bool isAct2 = am.TestType2 == AmericanDiplomaConstants.TestTypeAct;
            bool satIIProvided = satIIApplicable && (isAct2 ? am.ActMath.HasValue : am.SatII.HasValue);
            int? satII = satIIProvided
                ? (isAct2 ? AmericanDiplomaConstants.ConvertActMathToSatMath(am.ActMath!.Value) : am.SatII)
                : null;

            // النسبة النهائية المعادلة (مكتب تنسيق الجامعات الحكومية والمعاهد العليا) — تُحسب دائمًا
            // من القيم المعادلة النهائية لـ SatI/SatII (بعد تحويل ACT إن وُجد، فلا فرق بين مصدري
            // الاختبار هنا)، وتُضاف كرقم جديد بجانب القيم الحالية دون استبدالها.
            int equivalentWeight = satI >= AmericanDiplomaConstants.EquivalentFormulaBonusThreshold
                ? AmericanDiplomaConstants.EquivalentFormulaWeightWithBonus
                : AmericanDiplomaConstants.EquivalentFormulaWeightBase;

            decimal testIContribution = (decimal)satI / AmericanDiplomaConstants.SatMax * equivalentWeight;

            decimal testIIContribution = (satIIProvided && satII!.Value >= AmericanDiplomaConstants.EquivalentFormulaSatIICountThreshold)
                ? (decimal)satII.Value / AmericanDiplomaConstants.SatMax * AmericanDiplomaConstants.EquivalentFormulaSatIIWeight
                : 0m;

            decimal equivalentPercentage = Math.Round(testIContribution + testIIContribution + basePercentage, 2);

            student.AmericanDiplomaTotals = new AmericanDiplomaStudentTotals
            {
                Student = student,
                AverageScore = Math.Round(average, 2),
                BasePercentage = basePercentage,
                TestType1 = am.TestType1,
                SatI = satI,
                ActComposite = isAct1 ? am.ActComposite : null,
                TestType2 = satIIProvided ? am.TestType2 : null,
                SatII = satII,
                ActMath = satIIProvided && isAct2 ? am.ActMath : null,
                SatIISubject1 = satIIProvided ? am.SatIISubject1 : null,
                SatIISubject2 = satIIProvided ? am.SatIISubject2 : null,
                StudiedAdvancedMath = am.StudiedAdvancedMath,
                SatIBelowMinimum = satI < AmericanDiplomaConstants.SatIMinimumThreshold,
                SatIIBelowMinimum = satIIProvided && satII!.Value < AmericanDiplomaConstants.SatIIMinimumThreshold,
                EquivalentPercentage = equivalentPercentage
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
