using System;

namespace StudentRegistry.Application.DTOs
{
    public class DeleteRequestCreateDto
    {
        public int StudentId { get; set; }
        public string? Reason { get; set; }
    }

    public class DeleteRequestResponseDto
    {
        public int Id { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
