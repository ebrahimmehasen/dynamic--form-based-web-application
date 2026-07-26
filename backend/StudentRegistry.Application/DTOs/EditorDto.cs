using System;
using System.Collections.Generic;

namespace StudentRegistry.Application.DTOs
{
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    // Same flat columns as StudentListItemDto, plus the per-row activity flags the Editor table needs
    // for its "has edits/comments" left-border indicator and pending-deletion badge.
    public class EditorStudentListItemDto : StudentListItemDto
    {
        public bool HasFieldEdits { get; set; }
        public bool HasFieldComments { get; set; }
        public bool HasPendingDeleteRequest { get; set; }
    }

    // One unified row for the per-student (and global Admin) audit log — merges FieldEdits,
    // FieldComments and DeleteRequests into a single newest-first timeline.
    public class AuditLogEntryDto
    {
        public string Type { get; set; } = string.Empty; // "Edit" | "Comment" | "DeleteRequest"
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Actor { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
