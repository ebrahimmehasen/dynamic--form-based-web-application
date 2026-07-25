using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.Constants;
using System.Collections.Generic;
using System.Linq;

namespace StudentRegistry.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        [HttpGet("subjects")]
        public IActionResult GetSubjectsConfig()
        {
            var config = new
            {
                certifications = new Dictionary<string, object>
                {
                    { "ig", new { name = "شهادات الـ IG (IGCSE/O-Level/A-Level)", tracks = new[] { "IGCSE (Early Years) - مواد عامة", "A-Levels (Advanced Years) - تخصص علمي أو أدبي", "AS-Levels (Intermediate Year) - انتقالى" } } },
                    { "saudi", new { name = "شهادة سعودية", tracks = new[] { "المسار العام", "مسار العلوم", "مسار الإدارة والأعمال", "مسار الهندسة والتكنولوجيا", "مسار العلوم الإنسانية" } } },
                    { "qatari", new { name = "شهادة قطرية", tracks = new[] { "المسار العلمي", "المسار الأدبي والإنسانيات", "مسار التكنولوجيا" } } },
                    { "bahraini", new { name = "شهادة بحرينية", tracks = new[] { BahrainiConstants.ScientificTrack, BahrainiConstants.LiteraryTrack, BahrainiConstants.VocationalTrack } } },
                    { "kuwaiti", new { name = "شهادة كويتية", tracks = new[] { "القسم العلمي", "القسم الأدبي" } } },
                    // §1.6 — Omani has one track only; the dropdown offers a single fixed value.
                    { "omani", new { name = "شهادة عمانية", tracks = new[] { OmaniConstants.SingleTrack } } },
                    // §1.1 — Yemeni has one track only; the dropdown offers a single fixed value.
                    { "yemeni", new { name = "شهادة يمنية", tracks = new[] { YemeniConstants.SingleTrack } } },
                    // §1.1 — percentage-in only: no subjects, no subjects-palestinian config endpoint.
                    // Track (علمي/أدبي) is recorded but never changes the calculation.
                    { "palestinian", new { name = "شهادة فلسطينية (توجيهي)", tracks = new[] { PalestinianConstants.ScientificBranch, PalestinianConstants.LiteraryBranch } } },
                    { "egyptian", new { name = "الثانوية العامة المصرية", tracks = EgyptianConstants.Tracks } },
                    { "azhar", new { name = "الثانوية الأزهرية", tracks = AzharConstants.Sections } },
                    // §Emirati — single track today; the array stays future-proof (a second track can
                    // be added to EmiratiConstants.Tracks later without touching this line).
                    { "emirati", new { name = "الشهادة الإماراتية", tracks = EmiratiConstants.Tracks } },
                    // §1.1 — must stay LAST in the list. Percentage-in only, free-text certificate
                    // name, no track selector at all (empty tracks array — the UI renders no track
                    // dropdown for this cert and never populates one).
                    { "other", new { name = "أخرى", tracks = System.Array.Empty<string>() } }
                },
                subjects = new Dictionary<string, string[]>
                {
                    { "year_1", new[] { "اللغة العربية", "اللغة الإنجليزية", "الرياضيات", "الكيمياء", "الفيزياء", "الأحياء", "الدراسات الإسلامية", "الدراسات الاجتماعية" } },
                    { "year_2", new[] { "اللغة العربية", "اللغة الإنجليزية", "الرياضيات (2)", "الكيمياء", "الفيزياء", "الأحياء", "الحاسب الآلي", "الدراسات الإسلامية" } },
                    { "year_3", new[] { "اللغة العربية", "اللغة الإنجليزية", "الرياضيات (3)", "الكيمياء", "الفيزياء", "الجيولوجيا والعلوم البيئية", "الدراسات الإسلامية", "التربية الوطنية" } }
                }
            };

            return Ok(config);
        }

        [HttpGet("subjects-saudi")]
        public IActionResult GetSaudiSubjectsConfig()
        {
            // Source of truth: official Saudi weighted-grade spreadsheet.
            // No fixed coefficients here anymore — the coefficient is derived per submission
            // from the student's own "Achieved" and "Weighted" entries (Weighted / Achieved,
            // must be a whole number). Duplicate subject names within a block are intentional —
            // they represent separate exams/terms for the same subject.
            var saudiConfig = new Dictionary<string, string[]>
            {
                {
                    "block_1", new[]
                    {
                        "اللغة الإنجليزية", "الأحياء", "الرياضيات", "الكفايات اللغوية",
                        "التفكير الناقد", "تقنية رقمية", "الفيزياء", "اللغة الإنجليزية",
                        "علم البيئة", "الرياضيات", "الكفايات اللغوية", "تربية مهنية",
                        "تقنية رقمية", "اللغة الإنجليزية", "دراسات اجتماعية", "الرياضيات",
                        "الكيمياء", "المعرفة المالية", "تقنية رقمية"
                    }
                },
                {
                    "block_2", new[]
                    {
                        "اللغة الإنجليزية", "الأحياء", "الرياضيات", "الكيمياء", "الفنون",
                        "اللغة الإنجليزية", "التاريخ", "الأحياء", "الرياضيات", "الكيمياء",
                        "الكفايات اللغوية", "تقنية رقمية", "الفيزياء", "اللغة الإنجليزية",
                        "الأحياء", "الرياضيات", "الكيمياء", "تقنية رقمية"
                    }
                },
                {
                    "block_3", new[]
                    {
                        "الفيزياء", "الرياضيات", "اللغة الإنجليزية", "البحث ومصادر المعلومات",
                        "النشاط", "علوم الارض والفضاء", "الكيمياء", "تقنية رقمية",
                        "التنمية المستدامه", "الفيزياء", "اللغة الإنجليزية", "الرياضيات",
                        "الجغرافيا", "النشاط", "الدراسات الادبية", "علوم الارض والفضاء",
                        "دراسات نفسية واجتماعية", "مواطنة رقمية", "مهارات حياتية 2",
                        "التنمية المستدامه"
                    }
                }
            };

            return Ok(saudiConfig);
        }

        [HttpGet("subjects-kuwaiti")]
        public IActionResult GetKuwaitiSubjectsConfig()
        {
            // Max marks are fixed (taken from an official Kuwaiti certificate sample) — the student
            // only enters the obtained mark; the weight per year is entered by the student themselves
            // since it is printed on their own certificate (see KuwaitiConstants for details).
            static object[] ToSubjectList(Dictionary<string, decimal> maxMarks) =>
                maxMarks.Select(kv => (object)new { name = kv.Key, maxMark = kv.Value }).ToArray();

            var kuwaitiConfig = new
            {
                grade_10 = ToSubjectList(KuwaitiConstants.Grade10MaxMarks),
                grade_11 = ToSubjectList(KuwaitiConstants.Grade11MaxMarks),
                grade_12 = ToSubjectList(KuwaitiConstants.Grade12MaxMarks),
                years_count_options = new[] { KuwaitiConstants.OneYear, KuwaitiConstants.TwoYears, KuwaitiConstants.ThreeYears }
            };

            return Ok(kuwaitiConfig);
        }

        [HttpGet("subjects-qatari")]
        public IActionResult GetQatariSubjectsConfig()
        {
            var qatariConfig = new
            {
                scientific = QatariConstants.ScientificTrackSubjects,
                max_mark_per_subject = SingleYearFixedTotalConstants.MaxMarkPerSubject,
                total_max = QatariConstants.ScientificTrackSubjects.Length * SingleYearFixedTotalConstants.MaxMarkPerSubject,
                excluded_subject = SingleYearFixedTotalConstants.IslamicEducationSubject,
                scientific_track_name = QatariConstants.ScientificTrack
            };

            return Ok(qatariConfig);
        }

        [HttpGet("subjects-omani")]
        public IActionResult GetOmaniSubjectsConfig()
        {
            var omaniConfig = new
            {
                subjects = OmaniConstants.Subjects,
                max_mark_per_subject = SingleYearFixedTotalConstants.MaxMarkPerSubject,
                total_max = OmaniConstants.Subjects.Length * SingleYearFixedTotalConstants.MaxMarkPerSubject,
                excluded_subject = SingleYearFixedTotalConstants.IslamicEducationSubject
            };

            return Ok(omaniConfig);
        }

        [HttpGet("subjects-yemeni")]
        public IActionResult GetYemeniSubjectsConfig()
        {
            var yemeniConfig = new
            {
                subjects = YemeniConstants.Subjects,
                max_mark_per_subject = SingleYearFixedTotalConstants.MaxMarkPerSubject,
                total_max = YemeniConstants.Subjects.Length * SingleYearFixedTotalConstants.MaxMarkPerSubject,
                excluded_subject = (string?)null
            };

            return Ok(yemeniConfig);
        }

        [HttpGet("subjects-bahraini")]
        public IActionResult GetBahrainiSubjectsConfig()
        {
            var bahrainiConfig = new
            {
                scientific = BahrainiConstants.ScientificTrackSubjects,
                // Grouped by semester (الفصل 3/4/5/6) for the UI — mirrors Saudi's block_1/2/3 pattern.
                // Duplicate subject names within a semester are intentional (separate course codes).
                scientific_semesters = BahrainiConstants.ScientificSemesters,
                literary = BahrainiConstants.LiteraryTrackSubjects,
                max_mark_per_subject = SingleYearFixedTotalConstants.MaxMarkPerSubject,
                scientific_track_name = BahrainiConstants.ScientificTrack,
                literary_track_name = BahrainiConstants.LiteraryTrack,
                vocational_track_name = BahrainiConstants.VocationalTrack,
                excluded_subjects = new[]
                {
                    "التربية الإسلامية والدينية", "التربية للمواطنة", "المقررات الإثرائية",
                    "التربية الرياضية", "التربية الفنية", "التربية الأسرية",
                    "القراءة عربي إنجليزي", "الثقافة الشعبية", "خدمة المجتمع"
                }
            };

            return Ok(bahrainiConfig);
        }

        [HttpGet("subjects-egyptian")]
        public IActionResult GetEgyptianSubjectsConfig()
        {
            // Nested by track then system so the client can look up the exact subject set (each
            // with its fixed max mark) for whatever combination the student picks. §3/§4.
            var subjectsByTrackAndSystem = new Dictionary<string, Dictionary<string, object[]>>();
            foreach (var track in EgyptianConstants.Tracks)
            {
                var perSystem = new Dictionary<string, object[]>();
                foreach (var system in EgyptianConstants.Systems)
                {
                    perSystem[system] = EgyptianConstants.GetSubjectMaxMarks(track, system)
                        .Select(kv => (object)new { name = kv.Key, maxMark = kv.Value })
                        .ToArray();
                }
                subjectsByTrackAndSystem[track] = perSystem;
            }

            var egyptianConfig = new
            {
                tracks = EgyptianConstants.Tracks,
                systems = EgyptianConstants.Systems,
                subjects_by_track_and_system = subjectsByTrackAndSystem,
                denominators = new
                {
                    new_system = EgyptianConstants.NewSystemDenominator,
                    old_system = EgyptianConstants.OldSystemDenominator
                },
                new_system_name = EgyptianConstants.NewSystem,
                old_system_name = EgyptianConstants.OldSystem
            };

            return Ok(egyptianConfig);
        }

        [HttpGet("subjects-azhar")]
        public IActionResult GetAzharSubjectsConfig()
        {
            var subjectsBySection = new Dictionary<string, object[]>();
            var denominators = new Dictionary<string, decimal>();
            foreach (var section in AzharConstants.Sections)
            {
                subjectsBySection[section] = AzharConstants.GetSubjectMaxMarks(section)
                    .Select(kv => (object)new { name = kv.Key, maxMark = kv.Value })
                    .ToArray();
                denominators[section] = AzharConstants.GetDenominator(section);
            }

            var azharConfig = new
            {
                sections = AzharConstants.Sections,
                subjects_by_section = subjectsBySection,
                denominators = denominators
            };

            return Ok(azharConfig);
        }

        [HttpGet("subjects-emirati")]
        public IActionResult GetEmiratiSubjectsConfig()
        {
            var emiratiConfig = new
            {
                core_subjects = EmiratiConstants.CoreSubjects,
                optional_subjects = EmiratiConstants.OptionalSubjects,
                max_mark_per_subject = SingleYearFixedTotalConstants.MaxMarkPerSubject,
                single_track_name = EmiratiConstants.SingleTrack,
                medical_colleges = EmiratiConstants.MedicalWishColleges,
                medical_wish_warning = EmiratiConstants.MedicalWishWarning
            };

            return Ok(emiratiConfig);
        }
    }
}
