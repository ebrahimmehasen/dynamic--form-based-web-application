using System;

namespace StudentRegistry.Domain.Entities
{
    public class ReviewNote
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValueSnapshot { get; set; }
        public string ReviewerNote { get; set; } = string.Empty;
        public string Author { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Student Student { get; set; } = null!;
    }
}
