using System;

namespace StudentRegistry.Domain.Entities
{
    // One row per certification type (key matches ConfigController's certifications dictionary /
    // the cert-select dropdown value, e.g. "saudi", "americanDiploma"). Controls whether the public
    // registration success screen shows the computed final score/total for that certification —
    // toggled only from the admin "إعدادات العرض" tab, restricted to the protected root admin.
    public class CertificationDisplaySetting
    {
        public int Id { get; set; }
        public string CertificationKey { get; set; } = string.Empty;
        public bool IsResultVisible { get; set; } = true;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUsername { get; set; }
    }
}
