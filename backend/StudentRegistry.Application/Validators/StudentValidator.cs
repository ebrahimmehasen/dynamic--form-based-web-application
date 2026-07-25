using FluentValidation;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using System;
using System.Linq;

namespace StudentRegistry.Application.Validators
{
    public class StudentCreateDtoValidator : AbstractValidator<StudentCreateDto>
    {
        public StudentCreateDtoValidator()
        {
            RuleFor(x => x.StudentName)
                .NotEmpty().WithMessage("الرجاء إدخال اسم الطالب كاملاً.")
                .MaximumLength(100).WithMessage("يجب ألا يزيد اسم الطالب عن 100 حرف.")
                .Must(NotContainHtml).WithMessage("اسم الطالب غير صالح ولا يمكن أن يحتوي على رموز أو وسوم HTML.");

            RuleFor(x => x.StudentNameEn)
                .NotEmpty().WithMessage("الرجاء إدخال اسم الطالب بالإنجليزية.")
                .MaximumLength(100).WithMessage("يجب ألا يزيد الاسم بالإنجليزية عن 100 حرف.")
                .Must(NotContainHtml).WithMessage("الاسم بالإنجليزية غير صالح ولا يمكن أن يحتوي على رموز أو وسوم HTML.");

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("الرجاء إدخال الرقم القومي.")
                .Length(8, 20).WithMessage("الرجاء إدخال رقم قومي صحيح (بين 8 و 20 خانة).")
                .Must(NotContainHtml).WithMessage("الرقم القومي غير صالح ولا يمكن أن يحتوي على رموز أو وسوم HTML.");

            RuleFor(x => x.WishCollege)
                .Must(c => WishConstants.Colleges.Contains(c))
                .WithMessage("الرجاء اختيار الكلية.");

            RuleFor(x => x.WishProgram)
                .Must((dto, program) => BeValidWishProgram(dto.WishCollege, program))
                .WithMessage("الرجاء اختيار البرنامج المناسب للكلية المختارة.");

            RuleFor(x => x.GraduationYear)
                .InclusiveBetween(2022, 2026).WithMessage("الرجاء اختيار سنة تخرج صحيحة (بين 2022 و2026).");

            RuleFor(x => x.Gender)
                .Must(g => g == "ذكر" || g == "أنثى")
                .WithMessage("الرجاء اختيار النوع (ذكر أو أنثى).");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("الرجاء إدخال رقم هاتف الطالب.")
                .Matches(@"^[0-9+\s]{8,20}$").WithMessage("الرجاء إدخال رقم هاتف صحيح.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("الرجاء إدخال البريد الإلكتروني.")
                .EmailAddress().WithMessage("الرجاء إدخال بريد إلكتروني صحيح.");

            RuleFor(x => x.GuardianName)
                .NotEmpty().WithMessage("الرجاء إدخال اسم ولي الأمر.")
                .MaximumLength(100).WithMessage("يجب ألا يزيد اسم ولي الأمر عن 100 حرف.")
                .Must(NotContainHtml).WithMessage("اسم ولي الأمر غير صالح ولا يمكن أن يحتوي على رموز أو وسوم HTML.");

            RuleFor(x => x.GuardianNationalId)
                .NotEmpty().WithMessage("الرجاء إدخال الرقم القومي لولي الأمر.")
                .Length(8, 20).WithMessage("الرجاء إدخال رقم قومي صحيح لولي الأمر (بين 8 و 20 خانة).")
                .Must(NotContainHtml).WithMessage("الرقم القومي لولي الأمر غير صالح ولا يمكن أن يحتوي على رموز أو وسوم HTML.");

            RuleFor(x => x.GuardianOccupation)
                .NotEmpty().WithMessage("الرجاء إدخال وظيفة ولي الأمر.")
                .MaximumLength(100).WithMessage("يجب ألا تزيد وظيفة ولي الأمر عن 100 حرف.")
                .Must(NotContainHtml).WithMessage("وظيفة ولي الأمر غير صالحة ولا يمكن أن تحتوي على رموز أو وسوم HTML.");

            RuleFor(x => x.GuardianPhone)
                .NotEmpty().WithMessage("الرجاء إدخال رقم هاتف ولي الأمر.")
                .Matches(@"^[0-9+\s]{8,20}$").WithMessage("الرجاء إدخال رقم هاتف صحيح لولي الأمر.");

            RuleFor(x => x.GuardianRelation)
                .NotEmpty().WithMessage("الرجاء اختيار صلة القرابة بولي الأمر.");

            RuleFor(x => x.AddressGov)
                .NotEmpty().WithMessage("الرجاء اختيار المحافظة.");

            RuleFor(x => x.AddressCenter)
                .NotEmpty().WithMessage("الرجاء إدخال المركز/القسم.");

            RuleFor(x => x.AddressStreet)
                .NotEmpty().WithMessage("الرجاء إدخال اسم الشارع.");

            RuleFor(x => x.AddressBuilding)
                .NotEmpty().WithMessage("الرجاء إدخال رقم المبنى.");

            RuleFor(x => x.Certification)
                .NotEmpty().WithMessage("الرجاء اختيار نوع الشهادة المعادلة.");

            RuleFor(x => x.Track)
                .NotEmpty().WithMessage("الرجاء اختيار المسار الأكاديمي.");

            RuleFor(x => x.Photo)
                .NotEmpty().WithMessage("الرجاء إضافة الصورة الشخصية للطالب قبل الإرسال.")
                .Must(StartWithBase64Header).WithMessage("تنسيق الصورة غير صالح. يجب أن تكون بصيغة JPG أو PNG أو WebP.");

            // Conditionals based on Certification Type
            When(x => IsSaudiCert(x.Certification), () =>
            {
                RuleFor(x => x.YearsCount)
                    .NotEmpty().WithMessage("الرجاء اختيار عدد سنوات الدراسة التراكمية.");

                RuleFor(x => x.SaudiGrades)
                    .NotEmpty().WithMessage("الرجاء إدخال درجات المواد.")
                    .Must(grades => grades != null && grades.Count > 0).WithMessage("جدول درجات المواد فارغ.");

                RuleForEach(x => x.SaudiGrades).ChildRules(grade =>
                {
                    grade.RuleFor(g => g.YearLabel).NotEmpty();
                    grade.RuleFor(g => g.SubjectName)
                        .NotEmpty().WithMessage("اسم المادة مطلوب.")
                        .Must(NotBeDeniedSaudiSubject)
                        .WithMessage("لا يمكن إضافة هذه المادة لأنها غير داخلة في حساب التنسيق المصري.");
                    grade.RuleFor(g => g.Achieved).GreaterThan(0).WithMessage("الدرجة المتحصلة يجب أن تكون أكبر من الصفر.");
                    grade.RuleFor(g => g.Weighted).GreaterThan(0).WithMessage("الدرجة الموزونة يجب أن تكون أكبر من الصفر.");
                    grade.RuleFor(g => g)
                        .Must(HaveWholeNumberCoefficient)
                        .WithMessage(g => $"درجات مادة \"{g.SubjectName}\" غير صحيحة: الموزونة يجب أن تكون من مضاعفات المتحصلة (المعامل = الموزونة ÷ المتحصلة يجب أن يكون رقماً صحيحاً).");
                });

                RuleFor(x => x.AptitudeScore)
                    .NotNull().WithMessage("الرجاء إدخال درجة القدرات.");

                RuleFor(x => x.AptitudeScore!.Value)
                    .InclusiveBetween(0, 100).WithMessage("درجة القدرات يجب أن تكون بين 0 و 100.")
                    .When(x => x.AptitudeScore.HasValue);
            });

            When(x => IsIgCert(x.Certification), () =>
            {
                RuleFor(x => x.IgProgram)
                    .NotEmpty().WithMessage("الرجاء تحديد برنامج الـ IG.");

                RuleFor(x => x.IgGradeCounts)
                    .NotEmpty().WithMessage("الرجاء تحديد توزيع الدرجات الأكاديمية لحساب المجموع.")
                    .Must(counts => counts != null && counts.Count > 0).WithMessage("توزيع الدرجات فارغ.");

                RuleFor(x => x.Factor)
                    .NotNull().WithMessage("قيمة المعامل النسبي مطلوبة.")
                    .GreaterThan(0).WithMessage("قيمة المعامل النسبي يجب أن تكون أكبر من الصفر.");

                RuleFor(x => x.SportsBonus)
                    .NotNull().WithMessage("نسبة الحافز الرياضي مطلوبة.")
                    .InclusiveBetween(0, 30).WithMessage("نسبة الحافز الرياضي يجب أن تكون بين 0 و 30%.");

                RuleForEach(x => x.IgGradeCounts).ChildRules(count =>
                {
                    count.RuleFor(c => c.GradeType).NotEmpty();
                    count.RuleFor(c => c.Grade).NotEmpty();
                    count.RuleFor(c => c.Count).GreaterThanOrEqualTo(0);
                });
            });

            When(x => !IsSaudiCert(x.Certification) && !IsIgCert(x.Certification) && !IsKuwaitiCert(x.Certification) && !IsQatariCert(x.Certification) && !IsOmaniCert(x.Certification) && !IsYemeniCert(x.Certification) && !IsBahrainiCert(x.Certification) && !IsPalestinianCert(x.Certification) && !IsOtherCert(x.Certification) && !IsEgyptianCert(x.Certification) && !IsAzharCert(x.Certification) && !IsEmiratiCert(x.Certification), () =>
            {
                RuleFor(x => x.YearOfStudy)
                    .NotEmpty().WithMessage("الرجاء اختيار السنة الدراسية.");

                RuleFor(x => x.StandardGrades)
                    .NotEmpty().WithMessage("الرجاء إدخال درجات المواد.")
                    .Must(grades => grades != null && grades.Count > 0).WithMessage("جدول درجات المواد فارغ.");

                RuleForEach(x => x.StandardGrades).ChildRules(grade =>
                {
                    grade.RuleFor(g => g.SubjectName).NotEmpty().WithMessage("اسم المادة مطلوب.");
                    grade.RuleFor(g => g.Grade).InclusiveBetween(0, 100).WithMessage("الدرجة يجب أن تكون بين 0 و 100.");
                    grade.RuleFor(g => g.WeightedPercentage).InclusiveBetween(0, 100).WithMessage("النسبة الموزونة يجب أن تكون بين 0 و 100.");
                });
            });

            When(x => IsKuwaitiCert(x.Certification), () =>
            {
                RuleFor(x => x.KuwaitiData)
                    .NotNull().WithMessage("بيانات الشهادة الكويتية مطلوبة.");

                When(x => x.KuwaitiData != null, () =>
                {
                    RuleFor(x => x.KuwaitiData!.YearsCount)
                        .Must(y => y == KuwaitiConstants.OneYear || y == KuwaitiConstants.TwoYears || y == KuwaitiConstants.ThreeYears)
                        .WithMessage("الرجاء اختيار عدد سنوات الدراسة (سنة واحدة أو سنتان أو ثلاث سنوات).");

                    bool IsOneYear(StudentCreateDto x) => x.KuwaitiData!.YearsCount == KuwaitiConstants.OneYear;
                    bool IsThreeYears(StudentCreateDto x) => x.KuwaitiData!.YearsCount == KuwaitiConstants.ThreeYears;

                    // Grade 10 is only required/considered when the student studied three years.
                    When(IsThreeYears, () =>
                    {
                        RuleFor(x => x.KuwaitiData!.Grade10Weight)
                            .NotNull().WithMessage("الرجاء إدخال نسبة الصف العاشر من المعدل التراكمي كما هي مدونة بالشهادة.")
                            .GreaterThan(0).WithMessage("نسبة الصف العاشر يجب أن تكون أكبر من الصفر.")
                            .LessThanOrEqualTo(100).WithMessage("نسبة الصف العاشر يجب ألا تتجاوز 100.");

                        RuleFor(x => x.KuwaitiData!.Grade10Subjects)
                            .Must(subjects => MatchesExactKuwaitiSubjectSet(subjects, KuwaitiConstants.Grade10MaxMarks.Keys))
                            .WithMessage("قائمة مواد الصف العاشر يجب أن تطابق تماماً المواد المعتمدة لهذا الصف، بدون نقص أو زيادة.");

                        RuleForEach(x => x.KuwaitiData!.Grade10Subjects)
                            .ChildRules(subject => ValidateKuwaitiSubjectRow(subject, KuwaitiConstants.Grade10MaxMarks));
                    });

                    // Grade 11 is not applicable at all for the "One Year" (grade 12 only) case.
                    When(x => !IsOneYear(x), () =>
                    {
                        RuleFor(x => x.KuwaitiData!.Grade11Weight)
                            .NotNull().WithMessage("الرجاء إدخال نسبة الصف الحادي عشر من المعدل التراكمي كما هي مدونة بالشهادة.")
                            .GreaterThan(0).WithMessage("نسبة الصف الحادي عشر يجب أن تكون أكبر من الصفر.")
                            .LessThanOrEqualTo(100).WithMessage("نسبة الصف الحادي عشر يجب ألا تتجاوز 100.");

                        RuleFor(x => x.KuwaitiData!.Grade12Weight)
                            .NotNull().WithMessage("الرجاء إدخال نسبة الصف الثاني عشر من المعدل التراكمي كما هي مدونة بالشهادة.")
                            .GreaterThan(0).WithMessage("نسبة الصف الثاني عشر يجب أن تكون أكبر من الصفر.")
                            .LessThanOrEqualTo(100).WithMessage("نسبة الصف الثاني عشر يجب ألا تتجاوز 100.");

                        RuleFor(x => x.KuwaitiData)
                            .Must(WeightsSumToOneHundred)
                            .WithMessage("مجموع نسب السنوات المدخلة (كما هي مدونة بالشهادة) يجب أن يساوي 100%.");

                        RuleFor(x => x.KuwaitiData!.Grade11Subjects)
                            .Must(subjects => MatchesExactKuwaitiSubjectSet(subjects, KuwaitiConstants.Grade11MaxMarks.Keys))
                            .WithMessage("قائمة مواد الصف الحادي عشر يجب أن تطابق تماماً المواد المعتمدة لهذا الصف، بدون نقص أو زيادة.");

                        RuleForEach(x => x.KuwaitiData!.Grade11Subjects)
                            .ChildRules(subject => ValidateKuwaitiSubjectRow(subject, KuwaitiConstants.Grade11MaxMarks));
                    });

                    // Grade 12 is always required, regardless of years count.
                    RuleFor(x => x.KuwaitiData!.Grade12Subjects)
                        .Must(subjects => MatchesExactKuwaitiSubjectSet(subjects, KuwaitiConstants.Grade12MaxMarks.Keys))
                        .WithMessage("قائمة مواد الصف الثاني عشر يجب أن تطابق تماماً المواد المعتمدة لهذا الصف، بدون نقص أو زيادة.");

                    RuleForEach(x => x.KuwaitiData!.Grade12Subjects)
                        .ChildRules(subject => ValidateKuwaitiSubjectRow(subject, KuwaitiConstants.Grade12MaxMarks));
                });
            });

            When(x => IsQatariCert(x.Certification), () =>
            {
                RuleFor(x => x.QatariData)
                    .NotNull().WithMessage("بيانات الشهادة القطرية مطلوبة.");

                // §1.6 — only المسار العلمي has a defined subject list today; block everything else.
                RuleFor(x => x.Track)
                    .Must(t => t == QatariConstants.ScientificTrack)
                    .WithMessage(QatariConstants.NonScientificTrackError);

                When(x => x.QatariData != null && x.Track == QatariConstants.ScientificTrack, () =>
                {
                    RuleFor(x => x.QatariData!.Subjects)
                        .Must(subjects => MatchesExactSingleYearSubjectSet(subjects, QatariConstants.ScientificTrackSubjects))
                        .WithMessage("قائمة المواد يجب أن تطابق تماماً مواد المسار العلمي السبع، بدون نقص أو زيادة أو تكرار.");

                    RuleForEach(x => x.QatariData!.Subjects).ChildRules(subject =>
                        ValidateSingleYearSubjectRow(subject, SingleYearFixedTotalConstants.IslamicEducationSubject));
                });
            });

            When(x => IsOmaniCert(x.Certification), () =>
            {
                RuleFor(x => x.OmaniData)
                    .NotNull().WithMessage("بيانات الشهادة العمانية مطلوبة.");

                When(x => x.OmaniData != null, () =>
                {
                    RuleFor(x => x.OmaniData!.Subjects)
                        .Must(subjects => MatchesExactSingleYearSubjectSet(subjects, OmaniConstants.Subjects))
                        .WithMessage("قائمة المواد يجب أن تطابق تماماً المواد السبع المعتمدة، بدون نقص أو زيادة أو تكرار.");

                    RuleForEach(x => x.OmaniData!.Subjects).ChildRules(subject =>
                        ValidateSingleYearSubjectRow(subject, SingleYearFixedTotalConstants.IslamicEducationSubject));
                });
            });

            When(x => IsYemeniCert(x.Certification), () =>
            {
                RuleFor(x => x.YemeniData)
                    .NotNull().WithMessage("بيانات الشهادة اليمنية مطلوبة.");

                When(x => x.YemeniData != null, () =>
                {
                    RuleFor(x => x.YemeniData!.Subjects)
                        .Must(subjects => MatchesExactSingleYearSubjectSet(subjects, YemeniConstants.Subjects))
                        .WithMessage("قائمة المواد يجب أن تطابق تماماً المواد الست المعتمدة، بدون نقص أو زيادة أو تكرار.");

                    // §1.3 — no excluded subject for Yemeni: any name outside the 6 counted subjects
                    // (including التربية الإسلامية) simply fails the exact-set check above.
                    RuleForEach(x => x.YemeniData!.Subjects).ChildRules(subject =>
                        ValidateSingleYearSubjectRow(subject, excludedSubject: null));
                });
            });

            When(x => IsBahrainiCert(x.Certification), () =>
            {
                RuleFor(x => x.BahrainiData)
                    .NotNull().WithMessage("بيانات الشهادة البحرينية مطلوبة.");

                // مسار مهني/فني ليس له قائمة مواد معتمدة بعد — يُحظر بنفس أسلوب الشهادة القطرية.
                RuleFor(x => x.Track)
                    .Must(t => t == BahrainiConstants.ScientificTrack || t == BahrainiConstants.LiteraryTrack)
                    .WithMessage(BahrainiConstants.VocationalTrackError);

                When(x => x.BahrainiData != null && x.Track == BahrainiConstants.ScientificTrack, () =>
                {
                    // Multiset match (not the stricter Qatari/Omani/Yemeni set match): the real
                    // per-semester course-code table has intentional duplicate subject names
                    // (separate course codes for the same subject, e.g. ريض253/ريض261).
                    RuleFor(x => x.BahrainiData!.Subjects)
                        .Must(subjects => MatchesExactSingleYearSubjectMultiset(subjects, BahrainiConstants.ScientificTrackSubjects))
                        .WithMessage("قائمة المواد يجب أن تطابق تماماً مواد المسار العلمي (30 مادة موزعة على الفصول 3-6)، بدون نقص أو زيادة.");

                    RuleForEach(x => x.BahrainiData!.Subjects).ChildRules(subject =>
                        ValidateSingleYearSubjectRow(subject, excludedSubject: null));
                });

                When(x => x.BahrainiData != null && x.Track == BahrainiConstants.LiteraryTrack, () =>
                {
                    RuleFor(x => x.BahrainiData!.Subjects)
                        .Must(subjects => MatchesExactSingleYearSubjectSet(subjects, BahrainiConstants.LiteraryTrackSubjects))
                        .WithMessage("قائمة المواد يجب أن تطابق تماماً مواد المسار الأدبي الثمانية، بدون نقص أو زيادة أو تكرار.");

                    RuleForEach(x => x.BahrainiData!.Subjects).ChildRules(subject =>
                        ValidateSingleYearSubjectRow(subject, excludedSubject: null));
                });
            });

            // §1.1 — percentage-in only: no subjects, no grades grid, no excluded-subjects list.
            When(x => IsPalestinianCert(x.Certification), () =>
            {
                RuleFor(x => x.PalestinianData)
                    .NotNull().WithMessage("بيانات الشهادة الفلسطينية مطلوبة.");

                When(x => x.PalestinianData != null, () =>
                {
                    RuleFor(x => x.PalestinianData!.Percentage)
                        .InclusiveBetween(0, 100)
                        .WithMessage("النسبة المئوية يجب أن تكون بين 0 و100.");

                    RuleFor(x => x.PalestinianData!.Branch)
                        .Must(b => b == PalestinianConstants.ScientificBranch || b == PalestinianConstants.LiteraryBranch)
                        .WithMessage("الرجاء اختيار الفرع (علمي أو أدبي).");
                });
            });

            // §1.2 — percentage-in only, free-text certificate name, no track selector at all.
            When(x => IsOtherCert(x.Certification), () =>
            {
                RuleFor(x => x.OtherData)
                    .NotNull().WithMessage("بيانات الشهادة مطلوبة.");

                When(x => x.OtherData != null, () =>
                {
                    RuleFor(x => x.OtherData!.CertificateName)
                        .NotEmpty().WithMessage("الرجاء إدخال اسم الشهادة.")
                        .MaximumLength(200).WithMessage("يجب ألا يزيد اسم الشهادة عن 200 حرف.")
                        .Must(NotContainHtml).WithMessage("اسم الشهادة غير صالح ولا يمكن أن يحتوي على رموز أو وسوم HTML.");

                    RuleFor(x => x.OtherData!.Percentage)
                        .InclusiveBetween(0, 100)
                        .WithMessage("النسبة المئوية يجب أن تكون بين 0 و100.");
                });
            });

            // §Egyptian — this IS the target Egyptian certificate itself: Track (علمي علوم / علمي
            // رياضة / أدبي) + SubjectSystem (قديم / حديث) together determine the exact subject set
            // and each subject's fixed max mark (§EgyptianConstants). No equivalent-total conversion.
            When(x => IsEgyptianCert(x.Certification), () =>
            {
                RuleFor(x => x.Track)
                    .Must(t => EgyptianConstants.Tracks.Contains(t))
                    .WithMessage("الرجاء اختيار المسار (علمي علوم أو علمي رياضة أو أدبي).");

                // §5/§6 — the Wish section's college restricts which tracks are valid here; the
                // client-side dropdown restriction is never trusted on its own.
                RuleFor(x => x.Track)
                    .Must((dto, track) => EgyptianConstants.GetAllowedTracksForCollege(dto.WishCollege).Contains(track))
                    .WithMessage("المسار المختار غير متاح للكلية المحددة في قسم الرغبة.");

                RuleFor(x => x.EgyptianData)
                    .NotNull().WithMessage("بيانات الثانوية العامة المصرية مفقودة.");

                When(x => x.EgyptianData != null, () =>
                {
                    RuleFor(x => x.EgyptianData!.SubjectSystem)
                        .Must(s => s == EgyptianConstants.OldSystem || s == EgyptianConstants.NewSystem)
                        .WithMessage("الرجاء اختيار نظام المواد (قديم أو حديث).");

                    When(x => EgyptianConstants.Tracks.Contains(x.Track) &&
                        (x.EgyptianData!.SubjectSystem == EgyptianConstants.OldSystem || x.EgyptianData!.SubjectSystem == EgyptianConstants.NewSystem), () =>
                    {
                        RuleFor(x => x.EgyptianData!.Subjects)
                            .Must((dto, subjects) => MatchesExactEgyptianSubjectSet(subjects, EgyptianConstants.GetSubjectMaxMarks(dto.Track, dto.EgyptianData!.SubjectSystem)))
                            .WithMessage("قائمة المواد يجب أن تطابق تماماً المواد المطلوبة للمسار ونظام المواد المختارين، بدون نقص أو زيادة أو تكرار.");

                        RuleFor(x => x.EgyptianData!.Subjects)
                            .Must((dto, subjects) => AllEgyptianMarksWithinRange(dto.Track, dto.EgyptianData!.SubjectSystem, subjects))
                            .WithMessage("الرجاء إدخال درجة صحيحة (بين 0 والدرجة العظمى المحددة) لكل مادة.");
                    });
                });
            });

            // §Azhar — قسم (علمي/أدبي) determines the exact fixed subject set and each subject's
            // fixed max mark (§AzharConstants). المواد الشرعية are never modeled at all — there is
            // no field for them to exclude. المجموع الاعتباري = Percentage × 4.1.
            When(x => IsAzharCert(x.Certification), () =>
            {
                RuleFor(x => x.Track)
                    .Must(t => AzharConstants.Sections.Contains(t))
                    .WithMessage("الرجاء اختيار القسم (علمي أو أدبي).");

                // §5 — the Wish section's college restricts which قسم values are valid here; the
                // client-side dropdown restriction is never trusted on its own.
                RuleFor(x => x.Track)
                    .Must((dto, track) => AzharConstants.GetAllowedSectionsForCollege(dto.WishCollege).Contains(track))
                    .WithMessage("القسم المختار غير متاح للكلية المحددة في قسم الرغبة.");

                RuleFor(x => x.AzharData)
                    .NotNull().WithMessage("بيانات الثانوية الأزهرية مفقودة.");

                When(x => x.AzharData != null && AzharConstants.Sections.Contains(x.Track), () =>
                {
                    RuleFor(x => x.AzharData!.Subjects)
                        .Must((dto, subjects) => MatchesExactAzharSubjectSet(subjects, AzharConstants.GetSubjectMaxMarks(dto.Track)))
                        .WithMessage("قائمة المواد يجب أن تطابق تماماً المواد المطلوبة للقسم المختار، بدون نقص أو زيادة أو تكرار.");

                    RuleFor(x => x.AzharData!.Subjects)
                        .Must((dto, subjects) => AllAzharMarksWithinRange(dto.Track, subjects))
                        .WithMessage("الرجاء إدخال درجة صحيحة (بين 0 والدرجة العظمى المحددة) لكل مادة.");
                });
            });

            // §Emirati — single track today (no track-selection UI). Core subjects (5) are always
            // required; optional subjects (الكيمياء/العلوم الصحية/الأحياء) may each be included at
            // most once — they are never required, even for medical Wish colleges (§4, UI-only
            // warning). The denominator is derived from however many subjects are actually
            // submitted, so unlike Qatari/Omani/Yemeni/Bahraini this isn't a fixed-length exact match.
            When(x => IsEmiratiCert(x.Certification), () =>
            {
                RuleFor(x => x.EmiratiData)
                    .NotNull().WithMessage("بيانات الشهادة الإماراتية مطلوبة.");

                When(x => x.EmiratiData != null, () =>
                {
                    RuleFor(x => x.EmiratiData!.Subjects)
                        .Must(IsValidEmiratiSubjectSet)
                        .WithMessage("قائمة المواد غير صحيحة: يجب إدخال كل المواد الأساسية الخمس، مع إمكانية إضافة أي من المواد الاختيارية (الكيمياء/العلوم الصحية/الأحياء) دون تكرار.");

                    RuleForEach(x => x.EmiratiData!.Subjects).ChildRules(subject =>
                        ValidateSingleYearSubjectRow(subject, excludedSubject: null));
                });
            });
        }

        // §Emirati — every core subject must be present exactly once; anything beyond that must be
        // one of the optional subjects, at most once each (no duplicates, no unknown names).
        private bool IsValidEmiratiSubjectSet(System.Collections.Generic.List<SingleYearSubjectMarkCreateDto>? subjects)
        {
            if (subjects == null) return false;
            var names = subjects.Select(s => s.SubjectName).ToList();
            if (names.Distinct().Count() != names.Count) return false;

            if (!EmiratiConstants.CoreSubjects.All(names.Contains)) return false;

            var extras = names.Except(EmiratiConstants.CoreSubjects);
            return extras.All(EmiratiConstants.OptionalSubjects.Contains);
        }

        // §Egyptian — exact match against the track+system's required subject set (from
        // EgyptianConstants.GetSubjectMaxMarks), no missing/extra/duplicate subjects.
        private bool MatchesExactEgyptianSubjectSet(
            System.Collections.Generic.List<SingleYearSubjectMarkCreateDto>? subjects,
            System.Collections.Generic.Dictionary<string, decimal> required)
        {
            if (subjects == null) return false;
            var names = subjects.Select(s => s.SubjectName).ToList();
            if (names.Count != required.Count) return false;
            if (names.Distinct().Count() != names.Count) return false;
            return required.Keys.All(names.Contains);
        }

        // §Egyptian — each subject's mark must be within [0, its own fixed max mark], which varies
        // by subject (80/60/50/40), unlike the uniform-100 single-year-fixed-total family.
        private bool AllEgyptianMarksWithinRange(string track, string system, System.Collections.Generic.List<SingleYearSubjectMarkCreateDto>? subjects)
        {
            if (subjects == null) return false;
            var maxMarks = EgyptianConstants.GetSubjectMaxMarks(track, system);
            foreach (var subject in subjects)
            {
                if (!maxMarks.TryGetValue(subject.SubjectName ?? string.Empty, out var max)) return false;
                if (subject.Mark < 0 || subject.Mark > max) return false;
            }
            return true;
        }

        // §Azhar — exact match against the قسم's required subject set (from
        // AzharConstants.GetSubjectMaxMarks), no missing/extra/duplicate subjects.
        private bool MatchesExactAzharSubjectSet(
            System.Collections.Generic.List<SingleYearSubjectMarkCreateDto>? subjects,
            System.Collections.Generic.Dictionary<string, decimal> required)
        {
            if (subjects == null) return false;
            var names = subjects.Select(s => s.SubjectName).ToList();
            if (names.Count != required.Count) return false;
            if (names.Distinct().Count() != names.Count) return false;
            return required.Keys.All(names.Contains);
        }

        // §Azhar — each subject's mark must be within [0, its own fixed max mark], which varies by
        // subject (60/40/30).
        private bool AllAzharMarksWithinRange(string section, System.Collections.Generic.List<SingleYearSubjectMarkCreateDto>? subjects)
        {
            if (subjects == null) return false;
            var maxMarks = AzharConstants.GetSubjectMaxMarks(section);
            foreach (var subject in subjects)
            {
                if (!maxMarks.TryGetValue(subject.SubjectName ?? string.Empty, out var max)) return false;
                if (subject.Mark < 0 || subject.Mark > max) return false;
            }
            return true;
        }

        // Bahraini المسار العلمي only: same exact-count match as MatchesExactSingleYearSubjectSet but
        // WITHOUT the no-duplicates rule, since its real subject list has intentional duplicates.
        private bool MatchesExactSingleYearSubjectMultiset(
            System.Collections.Generic.List<SingleYearSubjectMarkCreateDto>? subjects, string[] required)
        {
            if (subjects == null) return false;
            var names = subjects.Select(s => s.SubjectName).OrderBy(n => n, StringComparer.Ordinal).ToList();
            var requiredSorted = required.OrderBy(n => n, StringComparer.Ordinal).ToList();
            return names.SequenceEqual(requiredSorted);
        }

        // Shared by Qatari, Omani, Yemeni and Bahraini's المسار الأدبي (all single-year, fixed-100-per-subject
        // certificates with no duplicate subject names).
        private bool MatchesExactSingleYearSubjectSet(
            System.Collections.Generic.List<SingleYearSubjectMarkCreateDto>? subjects, string[] required)
        {
            if (subjects == null) return false;
            var names = subjects.Select(s => s.SubjectName).ToList();
            if (names.Count != required.Length) return false;
            if (names.Distinct().Count() != names.Count) return false; // no duplicates
            return required.All(names.Contains);
        }

        // excludedSubject is null for certs with no carve-out (Yemeni) — the NotEqual rule is
        // skipped in that case rather than assuming every cert in this family excludes something.
        private void ValidateSingleYearSubjectRow(InlineValidator<SingleYearSubjectMarkCreateDto> subject, string? excludedSubject)
        {
            var nameRule = subject.RuleFor(g => g.SubjectName)
                .NotEmpty().WithMessage("اسم المادة مطلوب.");

            if (excludedSubject != null)
            {
                nameRule.NotEqual(excludedSubject)
                    .WithMessage("مادة التربية الإسلامية لا تدخل ضمن قائمة المواد المحتسبة — الرجاء إدخال درجتها في الحقل المخصص لها.");
            }

            subject.RuleFor(g => g.Mark)
                .InclusiveBetween(0, SingleYearFixedTotalConstants.MaxMarkPerSubject)
                .WithMessage("درجة المادة يجب أن تكون بين 0 و100.");
        }

        // Pharmacy: program must be the fixed auto-filled value. No-program colleges: program must
        // be empty. Colleges with a program list: program must be exactly one of that list.
        private bool BeValidWishProgram(string college, string? program)
        {
            if (WishConstants.NoProgramColleges.Contains(college))
                return string.IsNullOrEmpty(program);

            if (college == WishConstants.Pharmacy)
                return program == WishConstants.PharmacyProgram;

            if (WishConstants.ProgramsByCollege.TryGetValue(college, out var programs))
                return program != null && programs.Contains(program);

            return true; // invalid/empty college is already caught by the WishCollege rule
        }

        private bool IsQatariCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("قطرية") || cert.Equals("qatari", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsOmaniCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("عمانية") || cert.Equals("omani", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsYemeniCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("يمنية") || cert.Equals("yemeni", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsBahrainiCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("بحرينية") || cert.Equals("bahraini", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPalestinianCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("فلسطين") || cert.Equals("palestinian", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsOtherCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("أخرى") || cert.Equals("other", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsEgyptianCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("الثانوية العامة المصرية") || cert.Equals("egyptian", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsAzharCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("الثانوية الأزهرية") || cert.Equals("azhar", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsEmiratiCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("الشهادة الإماراتية") || cert.Equals("emirati", StringComparison.OrdinalIgnoreCase);
        }

        private bool WeightsSumToOneHundred(KuwaitiDataCreateDto? data)
        {
            if (data == null) return false;
            if (data.YearsCount == KuwaitiConstants.OneYear) return true; // grade 12 alone carries 100%
            decimal sum = data.Grade11Weight ?? 0;
            sum += data.Grade12Weight ?? 0;
            if (data.YearsCount == KuwaitiConstants.ThreeYears)
                sum += data.Grade10Weight ?? 0;
            return Math.Abs(sum - 100m) <= 0.01m;
        }

        private void ValidateKuwaitiSubjectRow(
            InlineValidator<KuwaitiSubjectGradeCreateDto> subject,
            System.Collections.Generic.Dictionary<string, decimal> maxMarks)
        {
            subject.RuleFor(g => g.SubjectName)
                .NotEmpty().WithMessage("اسم المادة مطلوب.")
                .Must(NotBeExcludedKuwaitiSubject).WithMessage("هذه المادة غير محتسبة في معادلة الشهادة الكويتية.");

            subject.RuleFor(g => g.Obtained)
                .GreaterThanOrEqualTo(0).WithMessage("الدرجة المتحصلة يجب أن تكون أكبر من أو تساوي الصفر.");

            subject.RuleFor(g => g)
                .Must(g => maxMarks.TryGetValue(g.SubjectName ?? string.Empty, out var max) && g.Obtained <= max)
                .WithMessage("الدرجة المتحصلة يجب ألا تتجاوز الدرجة العظمى الرسمية لهذه المادة.");
        }

        private bool MatchesExactKuwaitiSubjectSet(
            System.Collections.Generic.List<KuwaitiSubjectGradeCreateDto>? subjects,
            System.Collections.Generic.ICollection<string> required)
        {
            if (subjects == null) return false;
            var names = subjects.Select(s => s.SubjectName).ToList();
            if (names.Count != required.Count) return false;
            return required.All(r => names.Contains(r)) && names.All(n => required.Contains(n));
        }

        private bool NotBeExcludedKuwaitiSubject(string subjectName)
        {
            if (string.IsNullOrEmpty(subjectName)) return true;
            return !KuwaitiConstants.ExcludedSubjects.Contains(subjectName);
        }

        private bool IsKuwaitiCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("كويتية") || cert.Equals("kuwaiti", StringComparison.OrdinalIgnoreCase);
        }

        private bool StartWithBase64Header(string photo)
        {
            if (string.IsNullOrEmpty(photo)) return false;
            return photo.StartsWith("data:image/jpeg;base64,") ||
                   photo.StartsWith("data:image/jpg;base64,") ||
                   photo.StartsWith("data:image/png;base64,") ||
                   photo.StartsWith("data:image/webp;base64,");
        }

        private bool NotContainHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;
            return !text.Contains("<") && !text.Contains(">");
        }

        private bool IsSaudiCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("سعودية") || cert.Equals("Saudi Certificate", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsIgCert(string cert)
        {
            if (string.IsNullOrEmpty(cert)) return false;
            return cert.Contains("IG") || cert.Equals("ig", StringComparison.OrdinalIgnoreCase);
        }

        private bool HaveWholeNumberCoefficient(SaudiGradeCreateDto grade)
        {
            if (grade.Achieved <= 0) return false;
            var coefficient = grade.Weighted / grade.Achieved;
            return Math.Abs(coefficient - Math.Round(coefficient)) < 0.001m;
        }

        // Mirrors wwwroot/js/form-handler.js's normalizeArabicSubject/checkSaudiSubjectAllowed —
        // re-validated here since students can add arbitrary subject names client-side.
        private bool NotBeDeniedSaudiSubject(string subjectName)
        {
            var normalized = NormalizeArabicSubject(subjectName);
            if (string.IsNullOrEmpty(normalized)) return true; // caught by NotEmpty separately

            foreach (var denied in SaudiConstants.DeniedSubjectsExact)
            {
                if (NormalizeArabicSubject(denied) == normalized) return false;
            }

            foreach (var keyword in SaudiConstants.DeniedKeywords)
            {
                if (normalized.Contains(keyword)) return false;
            }

            return true;
        }

        private static string NormalizeArabicSubject(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var value = text.Trim();
            value = System.Text.RegularExpressions.Regex.Replace(value, @"\s+", " ");
            value = value
                .Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا')
                .Replace('ة', 'ه')
                .Replace('ى', 'ي');
            value = System.Text.RegularExpressions.Regex.Replace(value, @"[ً-ْـ]", "");
            return value.ToLowerInvariant();
        }
    }
}
