using System;
using System.Collections.Generic;

namespace StudentRegistry.Application.Constants
{
    // "الثانوية الأزهرية" — fixed subject list per قسم (علمي/أدبي), no subject-system variant
    // (unlike Egyptian). المواد الشرعية are simply never modeled — the code never has a field for
    // them, so there is nothing to exclude at calculation time. Single source of truth referenced
    // by ConfigController, StudentCreateDtoValidator and StudentService.
    public static class AzharConstants
    {
        public const string ScientificSection = "علمي";
        public const string LiterarySection = "أدبي";

        public static readonly string[] Sections = { ScientificSection, LiterarySection };

        public const string ArabicGrammarMorphology = "اللغة العربية (نحو وصرف)";   // علمي only
        public const string Grammar = "النحو";                                       // أدبي only
        public const string Morphology = "الصرف";                                    // أدبي only
        public const string Rhetoric = "البلاغة";
        public const string LiteratureTextsReading = "الأدب والنصوص والمطالعة";
        public const string Composition = "الإنشاء";
        public const string FirstForeignLanguage = "اللغة الأجنبية الأولى";
        public const string Physics = "الفيزياء";
        public const string Chemistry = "الكيمياء";
        public const string Biology = "الأحياء";
        public const string PureMath = "الرياضيات (بحتة)";
        public const string AppliedMath = "الرياضيات (تطبيقية)";
        public const string History = "التاريخ";
        public const string Geography = "الجغرافيا";
        public const string Logic = "المنطق";
        public const string PsychologySociology = "علم النفس والاجتماع";

        // §4 — the total denominator is fixed per قسم; it happens to equal the sum of that
        // قسم's own subject max marks (440 علمي / 420 أدبي), but is expressed as its own constant
        // to match the pattern used across every other certificate in this system.
        public const decimal ScientificDenominator = 440m;
        public const decimal LiteraryDenominator = 420m;

        public static decimal GetDenominator(string section) =>
            section == ScientificSection ? ScientificDenominator : LiteraryDenominator;

        // §2/§3 — returns the exact required subject set and each subject's fixed max mark for a
        // given قسم.
        public static Dictionary<string, decimal> GetSubjectMaxMarks(string section)
        {
            if (section == ScientificSection)
            {
                return new Dictionary<string, decimal>
                {
                    { ArabicGrammarMorphology, 40m },
                    { Rhetoric, 40m },
                    { LiteratureTextsReading, 40m },
                    { Composition, 40m },
                    { FirstForeignLanguage, 40m },
                    { Physics, 60m },
                    { Chemistry, 60m },
                    { Biology, 60m },
                    { PureMath, 30m },
                    { AppliedMath, 30m }
                };
            }

            if (section == LiterarySection)
            {
                return new Dictionary<string, decimal>
                {
                    { Grammar, 40m },
                    { Morphology, 40m },
                    { Rhetoric, 40m },
                    { LiteratureTextsReading, 40m },
                    { Composition, 40m },
                    { FirstForeignLanguage, 60m },
                    { History, 40m },
                    { Geography, 40m },
                    { Logic, 40m },
                    { PsychologySociology, 40m }
                };
            }

            return new Dictionary<string, decimal>();
        }

        // §5 — the Wish section's college restricts which قسم values are valid here (قسم علمي
        // الأزهري covers both علوم ورياضة معًا، so every science-adjacent college maps to it).
        // Referenced by both StudentCreateDtoValidator and StudentService.
        private static readonly Dictionary<string, string[]> AllowedSectionsByCollege = new Dictionary<string, string[]>
        {
            { WishConstants.HumanMedicine, new[] { ScientificSection } },
            { WishConstants.Dentistry, new[] { ScientificSection } },
            { WishConstants.Pharmacy, new[] { ScientificSection } },
            { WishConstants.Nursing, new[] { ScientificSection } },
            { WishConstants.Engineering, new[] { ScientificSection } },
            { WishConstants.Computers, new[] { ScientificSection } },
            { WishConstants.Commerce, new[] { ScientificSection, LiterarySection } }
        };

        public static string[] GetAllowedSectionsForCollege(string college) =>
            AllowedSectionsByCollege.TryGetValue(college, out var sections) ? sections : Array.Empty<string>();
    }
}
