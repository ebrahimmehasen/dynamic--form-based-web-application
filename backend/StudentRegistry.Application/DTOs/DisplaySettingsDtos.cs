using System;

namespace StudentRegistry.Application.DTOs
{
    public class CertificationDisplaySettingDto
    {
        public string CertificationKey { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsResultVisible { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUsername { get; set; }
    }

    public class ToggleDisplaySettingDto
    {
        public bool IsVisible { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
