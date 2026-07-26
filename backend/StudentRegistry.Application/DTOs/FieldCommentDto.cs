using System;

namespace StudentRegistry.Application.DTOs
{
    public class FieldCommentCreateDto
    {
        public int StudentId { get; set; }
        public string EntityGroup { get; set; } = string.Empty;
        public int? EntityRowId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string? FieldSnapshot { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }

    public class FieldCommentResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        // Populated on the unreviewed/global lists (Notifications page, Admin log) so the entry can
        // link straight to the student without a second round-trip; left null on per-student lists.
        public string? StudentName { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldSnapshot { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
