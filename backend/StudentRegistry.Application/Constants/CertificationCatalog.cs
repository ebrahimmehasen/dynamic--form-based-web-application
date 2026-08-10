using System.Collections.Generic;

namespace StudentRegistry.Application.Constants
{
    // Canonical list of certification type keys + Arabic display labels, mirroring the
    // "certifications" dictionary keys in ConfigController.GetSubjectsConfig (cert-select dropdown
    // values) — kept as a separate small list here because Display Settings only needs key+label,
    // not the full tracks/config payload ConfigController builds per certification.
    public static class CertificationCatalog
    {
        public static readonly IReadOnlyList<(string Key, string Label)> All = new (string, string)[]
        {
            ("ig", "شهادات الـ IG (IGCSE/O-Level/A-Level)"),
            ("saudi", "شهادة سعودية"),
            ("qatari", "شهادة قطرية"),
            ("bahraini", "شهادة بحرينية"),
            ("kuwaiti", "شهادة كويتية"),
            ("omani", "شهادة عمانية"),
            ("yemeni", "شهادة يمنية"),
            ("palestinian", "شهادة فلسطينية (توجيهي)"),
            ("egyptian", "الثانوية العامة المصرية"),
            ("azhar", "الثانوية الأزهرية"),
            ("emirati", "الشهادة الإماراتية"),
            ("americanDiploma", "الدبلومة الأمريكية"),
            ("other", "أخرى"),
        };
    }
}
