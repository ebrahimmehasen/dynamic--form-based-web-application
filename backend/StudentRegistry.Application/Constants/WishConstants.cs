using System.Collections.Generic;

namespace StudentRegistry.Application.Constants
{
    // "الرغبة" (Wish) section — desired college + program. Selection-only, never used in any
    // equivalence calculation. Referenced by StudentCreateDtoValidator.
    public static class WishConstants
    {
        public const string HumanMedicine = "طب بشري";
        public const string Dentistry = "طب أسنان";
        public const string Pharmacy = "صيدلة";
        public const string Nursing = "تمريض";
        public const string Engineering = "هندسة";
        public const string Computers = "حاسبات";
        public const string Commerce = "تجارة";

        public static readonly string[] Colleges =
        {
            HumanMedicine, Dentistry, Pharmacy, Nursing, Engineering, Computers, Commerce
        };

        // Colleges with no sub-program at all — WishProgram must be empty for these.
        public static readonly string[] NoProgramColleges =
        {
            HumanMedicine, Dentistry, Nursing
        };

        // Pharmacy has exactly one fixed program, auto-filled and not user-editable.
        public const string PharmacyProgram = "إكلينيكية";

        public static readonly Dictionary<string, string[]> ProgramsByCollege = new Dictionary<string, string[]>
        {
            { Engineering, new[] { "تشييد", "ميكاترونكس" } },
            { Computers, new[] { "نظم معلومات طيران", "معلوماتية طبية", "ذكاء اصطناعي" } },
            { Commerce, new[] { "إدارة أعمال", "محاسبة" } }
        };
    }
}
