using System;
using System.Collections.Generic;

namespace StudentRegistry.Domain.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNameEn { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string WishCollege { get; set; } = string.Empty;   // "الرغبة" — desired college, selection-only
        public string? WishProgram { get; set; }   // desired program, when applicable to the college
        public int GraduationYear { get; set; }   // "سنة التخرج" — selection-only, 2022-2026
        public string Gender { get; set; } = string.Empty;   // ذكر / أنثى
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
        public string GuardianNationalId { get; set; } = string.Empty;
        public string GuardianOccupation { get; set; } = string.Empty;
        public string GuardianPhone { get; set; } = string.Empty;
        public string GuardianRelation { get; set; } = string.Empty;
        public string AddressGov { get; set; } = string.Empty;
        public string AddressCenter { get; set; } = string.Empty;
        public string? AddressVillage { get; set; }
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressBuilding { get; set; } = string.Empty;
        public string? AddressFloor { get; set; }
        public string Certification { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual SaudiStudentTotals? SaudiTotals { get; set; }
        public virtual ICollection<SaudiStudentGrades> SaudiGrades { get; set; } = new List<SaudiStudentGrades>();
        public virtual IgStudentGrades? IgGrades { get; set; }
        public virtual ICollection<IgStudentGradeCounts> IgGradeCounts { get; set; } = new List<IgStudentGradeCounts>();
        public virtual ICollection<StandardStudentGrades> StandardGrades { get; set; } = new List<StandardStudentGrades>();
        public virtual KuwaitiStudentTotals? KuwaitiTotals { get; set; }
        public virtual QatariStudentTotals? QatariTotals { get; set; }
        public virtual OmaniStudentTotals? OmaniTotals { get; set; }
        public virtual YemeniStudentTotals? YemeniTotals { get; set; }
        public virtual BahrainiStudentTotals? BahrainiTotals { get; set; }
        public virtual PalestinianStudentTotals? PalestinianTotals { get; set; }
        public virtual OtherStudentTotals? OtherTotals { get; set; }
        public virtual EgyptianStudentTotals? EgyptianTotals { get; set; }
        public virtual AzharStudentTotals? AzharTotals { get; set; }
        public virtual EmiratiStudentTotals? EmiratiTotals { get; set; }
    }
}
