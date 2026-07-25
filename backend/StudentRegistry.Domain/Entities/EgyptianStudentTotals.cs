namespace StudentRegistry.Domain.Entities
{
    public class EgyptianStudentTotals
    {
        public int StudentId { get; set; }

        public string Track { get; set; } = string.Empty;
        public string SubjectSystem { get; set; } = string.Empty;   // قديم / حديث

        public decimal FinalTotal { get; set; }     // sum of entered subject scores
        public decimal Denominator { get; set; }    // fixed 320 (حديث) or 410 (قديم) — never derived
        public decimal Percentage { get; set; }

        // Navigation property
        public virtual Student Student { get; set; } = null!;
    }
}
