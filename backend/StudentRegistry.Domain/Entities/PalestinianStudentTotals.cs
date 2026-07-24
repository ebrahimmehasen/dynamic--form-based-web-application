namespace StudentRegistry.Domain.Entities
{
    public class PalestinianStudentTotals
    {
        public int StudentId { get; set; }

        // The percentage the student typed, echoed back rounded to 2dp — never derived from
        // subjects, since Tawjihi has none in this system.
        public decimal Percentage { get; set; }

        // المجموع الاعتباري (المجموع المصري) — (Percentage ÷ 100) × 410.
        public decimal EquivalentTotal { get; set; }

        // علمي / أدبي — recorded for the record only, never forks the calculation.
        public string Branch { get; set; } = string.Empty;

        public virtual Student Student { get; set; } = null!;
    }
}
