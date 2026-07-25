using System;
using System.Collections.Generic;

namespace StudentRegistry.Application.Constants
{
    // "الثانوية العامة المصرية" — this IS the Egyptian target certificate itself, not a foreign
    // certificate being converted, so it has no equivalent-total (المجموع الاعتباري) conversion.
    // The subject list per track+system, and the fixed denominator per system, are the single
    // source of truth referenced by ConfigController, StudentCreateDtoValidator and StudentService.
    public static class EgyptianConstants
    {
        public const string ScientificScienceTrack = "علمي علوم";
        public const string ScientificMathTrack = "علمي رياضة";
        public const string LiteraryTrack = "أدبي";

        public static readonly string[] Tracks =
        {
            ScientificScienceTrack, ScientificMathTrack, LiteraryTrack
        };

        public const string OldSystem = "قديم";
        public const string NewSystem = "حديث";

        public static readonly string[] Systems = { OldSystem, NewSystem };

        public const string Arabic = "اللغة العربية";
        public const string English = "اللغة الإنجليزية";
        public const string History = "التاريخ";
        public const string Geography = "الجغرافيا";
        public const string PhilosophyLogic = "الفلسفة والمنطق";
        public const string PsychologySociology = "علم النفس والاجتماع";
        public const string Biology = "الأحياء";
        public const string Geology = "جيولوجيا وعلوم البيئية";
        public const string Chemistry = "الكيمياء";
        public const string Physics = "الفيزياء";
        public const string Mathematics = "رياضيات";
        public const string PureMath = "رياضيات بحتة";
        public const string AppliedMath = "رياضيات تطبيقية";
        public const string SecondForeignLanguage = "اللغة الأجنبية الثانية";

        // §4 — the total denominator is fixed by subject system alone; it is NEVER derived from
        // the sum of the visible fields' own max marks (those don't add up to it, by design).
        public const decimal NewSystemDenominator = 320m;
        public const decimal OldSystemDenominator = 410m;

        public static decimal GetDenominator(string system) =>
            system == NewSystem ? NewSystemDenominator : OldSystemDenominator;

        // §5/§6 — the Wish section's college restricts which Track values are valid for this
        // certificate. Referenced by both StudentCreateDtoValidator and StudentService so the
        // client-side dropdown restriction is never trusted on its own.
        private static readonly Dictionary<string, string[]> AllowedTracksByCollege = new Dictionary<string, string[]>
        {
            { WishConstants.HumanMedicine, new[] { ScientificScienceTrack } },
            { WishConstants.Dentistry, new[] { ScientificScienceTrack } },
            { WishConstants.Pharmacy, new[] { ScientificScienceTrack } },
            { WishConstants.Nursing, new[] { ScientificScienceTrack } },
            { WishConstants.Computers, new[] { ScientificScienceTrack, ScientificMathTrack } },
            { WishConstants.Engineering, new[] { ScientificMathTrack } },
            { WishConstants.Commerce, new[] { ScientificScienceTrack, ScientificMathTrack, LiteraryTrack } }
        };

        public static string[] GetAllowedTracksForCollege(string college) =>
            AllowedTracksByCollege.TryGetValue(college, out var tracks) ? tracks : Array.Empty<string>();

        // §3 — returns the exact required subject set and each subject's fixed max mark for a
        // given track+system combination.
        public static Dictionary<string, decimal> GetSubjectMaxMarks(string track, string system)
        {
            var result = new Dictionary<string, decimal>
            {
                { Arabic, 80m },
                { English, system == NewSystem ? 60m : 50m }
            };

            if (track == LiteraryTrack)
            {
                result[History] = 60m;
                result[Geography] = 60m;
                result[PhilosophyLogic] = 60m;
                result[PsychologySociology] = 60m;
            }
            else if (track == ScientificScienceTrack)
            {
                result[Biology] = 60m;
                result[Chemistry] = 60m;
                result[Physics] = 60m;

                // §Egyptian correction — جيولوجيا وعلوم البيئية only exists in النظام القديم for
                // مسار علمي علوم; النظام الحديث has no such subject for this track.
                if (system == OldSystem)
                {
                    result[Geology] = 60m;
                }
            }
            else if (track == ScientificMathTrack)
            {
                result[Chemistry] = 60m;
                result[Physics] = 60m;

                if (system == NewSystem)
                {
                    result[Mathematics] = 60m;
                }
                else
                {
                    result[PureMath] = 60m;
                    result[AppliedMath] = 60m;
                }
            }

            if (system == OldSystem)
            {
                result[SecondForeignLanguage] = 40m;
            }

            return result;
        }
    }
}
