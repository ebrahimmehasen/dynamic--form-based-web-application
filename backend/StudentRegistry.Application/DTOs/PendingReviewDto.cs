using System;

namespace StudentRegistry.Application.DTOs
{
    public class PendingReviewResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string FlaggedBy { get; set; } = string.Empty;
        public DateTime FlaggedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
