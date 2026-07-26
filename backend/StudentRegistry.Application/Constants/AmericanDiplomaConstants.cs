using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentRegistry.Application.Constants
{
    // "الدبلومة الأمريكية" — unlike every other certificate in this system, admission is NOT a
    // single equivalent-total number. It depends on three separate criteria together: the GPA-based
    // base percentage (out of 40), SAT I, and (for most colleges) SAT II — so this certificate has
    // no EquivalentTotal/410 conversion at all. SAT II's requirement, fixed first subject, and
    // second-subject options are all driven by the Wish section's college (§WishConstants) — never
    // a separate/duplicate selector.
    // Referenced by ConfigController (API), StudentCreateDtoValidator, and StudentService.
    public static class AmericanDiplomaConstants
    {
        public const int BestSubjectsCount = 8;
        public const decimal MaxMarkPerSubject = 100m;

        // §2 — النسبة المئوية الأساسية = (متوسط أفضل 8 مواد) × 40 ÷ 100، أي أساس المعادلة من 40 وليس 100.
        public const decimal BasePercentageWeight = 40m;

        public const int SatMin = 400;
        public const int SatMax = 1600;

        // §6 — هذه حدود دنيا استرشادية فقط (تنبيه، وليست حظرًا) — التحقق البنيوي (400-1600) وحده
        // يمنع الإرسال؛ الانخفاض عن هذه الحدود لا يمنع الحساب أو الإرسال أبدًا.
        public const int SatIMinimumThreshold = 1050;
        public const int SatIIMinimumThreshold = 1100;

        public const string BiologySubject = "الأحياء";
        public const string MathSubject = "الرياضيات";
        public const string PhysicsSubject = "الفيزياء";
        public const string ChemistrySubject = "الكيمياء";

        // مجموعتا الكليات اللتان تُظهران SAT II على الإطلاق — كل واحدة لها مادة أولى ثابتة + خيارات
        // للمادة الثانية. تجارة (الكلية السابعة في WishConstants.Colleges) ليس لها SAT II إطلاقًا
        // (يظل القسم مخفيًا تمامًا لها، كما كان).
        public static readonly string[] MedicalColleges =
        {
            WishConstants.HumanMedicine, WishConstants.Dentistry, WishConstants.Pharmacy, WishConstants.Nursing
        };

        public static readonly string[] EngineeringColleges =
        {
            WishConstants.Engineering, WishConstants.Computers
        };

        // كل الكليات الست اللي SAT II ظاهر لها (سواء إلزامي أو اختياري حسب الحالة) — وليس معناها
        // "إلزامي" بعد الآن؛ استخدم IsSatIIMandatory لتحديد الإلزامية الفعلية.
        public static readonly string[] SatIIApplicableColleges =
            MedicalColleges.Concat(EngineeringColleges).ToArray();

        public static readonly string[] MedicalSatIISecondSubjectOptions =
        {
            PhysicsSubject, ChemistrySubject, MathSubject
        };

        public static readonly string[] EngineeringSatIISecondSubjectOptions =
        {
            PhysicsSubject, ChemistrySubject, BiologySubject
        };

        public static bool IsSatIIApplicable(string college) => SatIIApplicableColleges.Contains(college);

        // المجموعة الطبية: SAT II اختياري دائمًا (لكن يظل ظاهرًا وقابلاً للتعبئة).
        // مجموعة الهندسة/الحاسبات: SAT II إلزامي، إلا إذا أكد الطالب أنه درس الرياضيات المتقدمة.
        // أي كلية أخرى (مثل تجارة): SAT II غير مطبق أصلاً (مخفي بالكامل).
        public static bool IsSatIIMandatory(string college, bool studiedAdvancedMath)
        {
            if (MedicalColleges.Contains(college)) return false;
            if (EngineeringColleges.Contains(college)) return !studiedAdvancedMath;
            return false;
        }

        public static string GetSatIIFixedFirstSubject(string college)
        {
            if (MedicalColleges.Contains(college)) return BiologySubject;
            if (EngineeringColleges.Contains(college)) return MathSubject;
            return string.Empty;
        }

        public static string[] GetSatIISecondSubjectOptions(string college)
        {
            if (MedicalColleges.Contains(college)) return MedicalSatIISecondSubjectOptions;
            if (EngineeringColleges.Contains(college)) return EngineeringSatIISecondSubjectOptions;
            return Array.Empty<string>();
        }

        public const string TestTypeSat = "SAT";
        public const string TestTypeAct = "ACT";

        public const int ActMin = 1;
        public const int ActMax = 36;

        // جدول تعادل ACT (الدرجة الكلية) → SAT — الجدول الرسمي الصادر عن College Board/ACT.
        public static readonly IReadOnlyDictionary<int, int> ActToSatConcordance = new Dictionary<int, int>
        {
            [36] = 1600, [35] = 1560, [34] = 1520, [33] = 1480, [32] = 1440, [31] = 1410, [30] = 1380,
            [29] = 1350, [28] = 1320, [27] = 1290, [26] = 1240, [25] = 1220, [24] = 1190, [23] = 1150,
            [22] = 1120, [21] = 1090, [20] = 1050, [19] = 1020, [18] = 980, [17] = 950, [16] = 910,
            [15] = 870, [14] = 820, [13] = 770, [12] = 720, [11] = 680, [10] = 640, [9] = 610
        };

        // جدول تعادل ACT Math (المادة الفرعية) → SAT Math — يُستخدم فقط لمجموعة الهندسة/الحاسبات
        // (المادة الثابتة الأولى لـ SAT II فيها هي الرياضيات)، حيث لا يوجد اختبار ACT منفصل لكل مادة.
        public static readonly IReadOnlyDictionary<int, int> ActMathToSatMathConcordance = new Dictionary<int, int>
        {
            [36] = 800, [35] = 780, [34] = 760, [33] = 740, [32] = 720, [31] = 710, [30] = 700,
            [29] = 680, [28] = 660, [27] = 640, [26] = 610, [25] = 590, [24] = 580, [23] = 560,
            [22] = 540, [21] = 530, [20] = 520, [19] = 510, [18] = 500, [17] = 470, [16] = 430,
            [15] = 400, [14] = 360, [13] = 330, [12] = 310, [11] = 280, [10] = 260
        };

        public static int ConvertActToSat(int actScore) => ConvertUsingConcordance(actScore, ActToSatConcordance);

        public static int ConvertActMathToSatMath(int actMathScore) => ConvertUsingConcordance(actMathScore, ActMathToSatMathConcordance);

        // بحث مباشر إذا كانت الدرجة موجودة في الجدول؛ وإلا استيفاء خطي بين أقرب قيمتين؛ مع
        // تثبيت (clamp) القيم خارج نطاق الجدول عند أقرب حد (الأدنى أو الأقصى).
        private static int ConvertUsingConcordance(int actScore, IReadOnlyDictionary<int, int> table)
        {
            int minKey = table.Keys.Min();
            int maxKey = table.Keys.Max();

            if (actScore <= minKey) return table[minKey];
            if (actScore >= maxKey) return table[maxKey];
            if (table.TryGetValue(actScore, out var exact)) return exact;

            int lowerKey = table.Keys.Where(k => k < actScore).Max();
            int upperKey = table.Keys.Where(k => k > actScore).Min();
            double t = (double)(actScore - lowerKey) / (upperKey - lowerKey);
            double interpolated = table[lowerKey] + t * (table[upperKey] - table[lowerKey]);
            return (int)Math.Round(interpolated, MidpointRounding.AwayFromZero);
        }

        public const string SatIIDateNote =
            "تاريخ اختبار SAT II يجب ألا يتعدى تاريخ الحصول على الدبلومة الأمريكية.";

        public const string AdmissionNote =
            "القبول النهائي بالكلية يعتمد على استيفاء الشروط الثلاثة معًا: المعدل الدراسي (أفضل 8 مواد) + درجة SAT I + درجة SAT II (حسب الكلية المختارة) — وليس على رقم نهائي واحد كما هو الحال في باقي الشهادات.";

        public const string ActLevel2UnavailableNote =
            "لا يوجد تحويل رسمي من ACT لهذه المادة — يُرجى استخدام درجة SAT لهذا المتطلب.";

        public const string Disclaimer =
            "هذه النتيجة تقديرية ويجب تأكيدها رسمياً من مكتب تنسيق القبول بالجامعات والمعاهد المصرية.";
    }
}
