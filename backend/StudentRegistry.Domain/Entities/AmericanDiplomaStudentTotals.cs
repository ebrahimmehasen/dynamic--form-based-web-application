namespace StudentRegistry.Domain.Entities
{
    public class AmericanDiplomaStudentTotals
    {
        public int StudentId { get; set; }

        public decimal AverageScore { get; set; }      // متوسط أفضل 8 مواد، من 100
        public decimal BasePercentage { get; set; }     // متوسط × 40 ÷ 100، من 40

        public int SatI { get; set; }                     // قيمة معادلة SAT النهائية دائمًا (تحويل تلقائي إذا كان الاختبار الأصلي ACT)
        public int? SatII { get; set; }                  // null إذا لم تكن مطبقة أو كانت اختيارية ولم تُدخل
        public string? SatIISubject1 { get; set; }
        public string? SatIISubject2 { get; set; }
        public bool StudiedAdvancedMath { get; set; }     // ذو معنى فقط لمجموعة الهندسة/الحاسبات

        public string TestType1 { get; set; } = "SAT";    // "SAT" أو "ACT" — نوع اختبار المستوى الأول
        public int? ActComposite { get; set; }             // الدرجة الكلية الخام لـ ACT (1-36)، فقط عندما TestType1 = ACT
        public string? TestType2 { get; set; }             // "SAT" أو "ACT" — نوع اختبار المستوى الثاني، فقط عندما SAT II مطبقة
        public int? ActMath { get; set; }                  // درجة ACT الفرعية للرياضيات الخام (1-36)، فقط عندما TestType2 = ACT

        // تنبيهات استرشادية فقط — لا تمنع الحساب أو الحفظ.
        public bool SatIBelowMinimum { get; set; }
        public bool SatIIBelowMinimum { get; set; }

        // النسبة النهائية المعادلة وفق معادلة مكتب التنسيق للجامعات الحكومية:
        // (SatI/1600 × 69 أو 60) + (SatII/1600 × 15 إذا SatII >= 1100) + AverageScore
        public decimal EquivalentPercentage { get; set; }

        public virtual Student Student { get; set; } = null!;
    }
}
