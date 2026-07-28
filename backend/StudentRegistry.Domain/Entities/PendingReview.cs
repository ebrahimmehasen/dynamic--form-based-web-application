using System;

namespace StudentRegistry.Domain.Entities
{
    // A Viewer flags a student "قيد المراجعة" (under review) from the Student Records Review page;
    // only an Editor can clear it, after actually reviewing/fixing the data. One row per flag cycle —
    // "currently pending" means the latest row for a student has Status == "pending".
    public class PendingReview
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FlaggedBy { get; set; } = "User";
        public DateTime FlaggedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "pending"; // "pending" | "resolved"
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public virtual Student Student { get; set; } = null!;
    }
}
