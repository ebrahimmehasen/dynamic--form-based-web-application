namespace StudentRegistry.Domain.Entities
{
    public class OmaniStudentTotals
    {
        public int StudentId { get; set; }

        public decimal FinalTotal { get; set; }   // out of 700
        public decimal Percentage { get; set; }

        // المجموع الاعتباري (المجموع المصري) — (Percentage ÷ 100) × 410. Always derived from the
        // final percentage shown on the site, never recomputed from raw certificate totals.
        public decimal EquivalentTotal { get; set; }

        // Navigation property
        public virtual Student Student { get; set; } = null!;
    }
}
