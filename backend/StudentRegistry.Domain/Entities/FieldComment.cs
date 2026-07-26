using System;

namespace StudentRegistry.Domain.Entities
{
    public class FieldComment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldSnapshot { get; set; }
        public string CommentText { get; set; } = string.Empty;
        // TODO: replace hardcoded "Editor" with the logged-in username once per-user identity is wired
        // through these write paths (mirrors the same placeholder already used by ReviewNote.Author).
        public string Author { get; set; } = "Editor";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = "unreviewed"; // "unreviewed" | "approved_as_edit" | "dismissed"

        public virtual Student Student { get; set; } = null!;
    }
}
