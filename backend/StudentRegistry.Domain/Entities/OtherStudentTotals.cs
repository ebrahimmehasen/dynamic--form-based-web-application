namespace StudentRegistry.Domain.Entities
{
    public class OtherStudentTotals
    {
        public int StudentId { get; set; }

        // Free text typed by the student — persisted verbatim, no validation beyond non-empty.
        public string CertificateName { get; set; } = string.Empty;

        // The percentage the student typed, echoed back rounded to 2dp — never derived from
        // subjects, since this certificate type has none in this system. No equivalent-total
        // conversion for this certificate — percentage only, by explicit product decision.
        public decimal Percentage { get; set; }

        public virtual Student Student { get; set; } = null!;
    }
}
