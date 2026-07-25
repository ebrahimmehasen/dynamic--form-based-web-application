using System;

namespace StudentRegistry.Application.DTOs
{
    // Author is intentionally not client-settable — the service always forces it to "User"
    // until authentication is implemented.
    public class ReviewNoteCreateDto
    {
        public int StudentId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValueSnapshot { get; set; }
        public string ReviewerNote { get; set; } = string.Empty;
    }

    public class ReviewNoteResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValueSnapshot { get; set; }
        public string ReviewerNote { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
