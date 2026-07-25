namespace StudentRegistry.Domain.Entities
{
    public class AzharStudentTotals
    {
        public int StudentId { get; set; }

        public string Section { get; set; } = string.Empty;   // علمي / أدبي

        public decimal FinalTotal { get; set; }     // sum of entered subject scores
        public decimal Denominator { get; set; }    // fixed 440 (علمي) or 420 (أدبي)
        public decimal Percentage { get; set; }

        // المجموع الاعتباري (المجموع المصري) = Percentage × 4.1 = (Percentage / 100) × 410.
        public decimal EquivalentTotal { get; set; }

        // Navigation property
        public virtual Student Student { get; set; } = null!;
    }
}
