namespace StudentRegistry.Domain.Entities
{
    public class AmericanDiplomaStudentTotals
    {
        public int StudentId { get; set; }

        public decimal AverageScore { get; set; }      // متوسط أفضل 8 مواد، من 100
        public decimal BasePercentage { get; set; }     // متوسط × 40 ÷ 100، من 40

        public int SatI { get; set; }
        public int? SatII { get; set; }                  // null إذا لم تكن مطبقة أو كانت اختيارية ولم تُدخل
        public string? SatIISubject1 { get; set; }
        public string? SatIISubject2 { get; set; }
        public bool StudiedAdvancedMath { get; set; }     // ذو معنى فقط لمجموعة الهندسة/الحاسبات

        // تنبيهات استرشادية فقط — لا تمنع الحساب أو الحفظ.
        public bool SatIBelowMinimum { get; set; }
        public bool SatIIBelowMinimum { get; set; }

        public virtual Student Student { get; set; } = null!;
    }
}
