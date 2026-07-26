using System;

namespace StudentRegistry.Domain.Entities
{
    public class DeleteRequest
    {
        public int Id { get; set; }
        // Nullable + SET NULL on delete: once an Admin approves the request the student row is
        // actually removed, and this record must survive as proof the approval happened.
        public int? StudentId { get; set; }
        // TODO: replace hardcoded "Editor" with the logged-in username once per-user identity is wired
        // through these write paths (mirrors the same placeholder already used by ReviewNote.Author).
        public string RequestedBy { get; set; } = "Editor";
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
        public string Status { get; set; } = "pending"; // "pending" | "approved" | "rejected"
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public virtual Student? Student { get; set; }
    }
}
