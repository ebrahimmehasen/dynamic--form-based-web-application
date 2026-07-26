using System;

namespace StudentRegistry.Domain.Entities
{
    public class FieldEdit
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        // TODO: replace hardcoded "Editor" with the logged-in username once per-user identity is wired
        // through these write paths (mirrors the same placeholder already used by ReviewNote.Author).
        public string Editor { get; set; } = "Editor";
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
        public string Source { get; set; } = "manual"; // "manual" | "from_comment"
        public int? SourceCommentId { get; set; }

        public virtual Student Student { get; set; } = null!;
        public virtual FieldComment? SourceComment { get; set; }
    }
}
