using System;

namespace StudentRegistry.Application.DTOs
{
    public class FieldEditCreateDto
    {
        public int StudentId { get; set; }
        public string EntityGroup { get; set; } = string.Empty;
        public int? EntityRowId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string? NewValue { get; set; }
        public string? Note { get; set; }
        public string Source { get; set; } = "manual"; // "manual" | "from_comment"

        // Present when this edit resolves an open comment (both the "Approve as Edit" and "Write own
        // edit" flows). When Source == "from_comment" this is also persisted as FieldEdit.SourceCommentId;
        // for "write own edit" the comment is still marked resolved but the edit itself isn't linked to it.
        public int? TriggeringCommentId { get; set; }
    }

    public class FieldEditResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? StudentName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Editor { get; set; } = string.Empty;
        public DateTime EditedAt { get; set; }
        public string? Note { get; set; }
        public string Source { get; set; } = string.Empty;
        public int? SourceCommentId { get; set; }
    }
}
