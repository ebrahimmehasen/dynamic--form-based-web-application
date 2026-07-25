namespace StudentRegistry.Domain.Entities
{
    public class EmiratiStudentTotals
    {
        public int StudentId { get; set; }

        public decimal FinalTotal { get; set; }
        public decimal Denominator { get; set; }   // 500-800, depends on how many optional subjects were entered
        public decimal Percentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // out of 410

        public virtual Student Student { get; set; } = null!;
    }
}
