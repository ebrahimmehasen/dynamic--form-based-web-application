using System;
using System.Linq;

namespace StudentRegistry.Application.Constants
{
    // "الدبلومة الأمريكية" — unlike every other certificate in this system, admission is NOT a
    // single equivalent-total number. It depends on three separate criteria together: the GPA-based
    // base percentage (out of 40), SAT I, and (for most colleges) SAT II — so this certificate has
    // no EquivalentTotal/410 conversion at all. SAT II's requirement, fixed first subject, and
    // second-subject options are all driven by the Wish section's college (§WishConstants),
    // mirroring the Egyptian/Azhar Track-by-college pattern.
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

        // §5 — مجموعتا الكليات اللتان تتطلبان SAT II، وكل واحدة لها مادة أولى ثابتة + خيارات للمادة
        // الثانية. تجارة (الكلية السابعة في WishConstants.Colleges) ليس لها SAT II إطلاقًا.
        public static readonly string[] MedicalColleges =
        {
            WishConstants.HumanMedicine, WishConstants.Dentistry, WishConstants.Pharmacy, WishConstants.Nursing
        };

        public static readonly string[] EngineeringColleges =
        {
            WishConstants.Engineering, WishConstants.Computers
        };

        public static readonly string[] SatIIRequiredColleges =
            MedicalColleges.Concat(EngineeringColleges).ToArray();

        public static readonly string[] MedicalSatIISecondSubjectOptions =
        {
            PhysicsSubject, ChemistrySubject, MathSubject
        };

        public static readonly string[] EngineeringSatIISecondSubjectOptions =
        {
            PhysicsSubject, ChemistrySubject, BiologySubject
        };

        public static bool RequiresSatII(string college) => SatIIRequiredColleges.Contains(college);

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

        public const string SatIIDateNote =
            "تاريخ اختبار SAT II يجب ألا يتعدى تاريخ الحصول على الدبلومة الأمريكية.";

        public const string AdmissionNote =
            "القبول النهائي بالكلية يعتمد على استيفاء الشروط الثلاثة معًا: المعدل الدراسي (أفضل 8 مواد) + درجة SAT I + درجة SAT II (حسب الكلية المختارة) — وليس على رقم نهائي واحد كما هو الحال في باقي الشهادات.";

        public const string Disclaimer =
            "هذه النتيجة تقديرية ويجب تأكيدها رسمياً من مكتب تنسيق القبول بالجامعات والمعاهد المصرية.";
    }
}
