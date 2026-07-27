using AutoMapper;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    public class EditorStudentService : IEditorStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStudentService _studentService;

        public EditorStudentService(IUnitOfWork unitOfWork, IMapper mapper, IStudentService studentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _studentService = studentService;
        }

        public async Task<PagedResultDto<EditorStudentListItemDto>> SearchAsync(string? query, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

            var (items, totalCount) = await _unitOfWork.Students.SearchPagedAsync(query, page, pageSize);
            var mapped = _mapper.Map<List<EditorStudentListItemDto>>(items);
            var studentIds = mapped.Select(m => m.Id).ToList();

            var edits = await _unitOfWork.FieldEdits.GetByStudentIdsAsync(studentIds);
            var comments = await _unitOfWork.FieldComments.GetByStudentIdsAsync(studentIds);
            var deleteRequests = await _unitOfWork.DeleteRequests.GetByStudentIdsAsync(studentIds);
            var pendingReviews = await _unitOfWork.PendingReviews.GetPendingByStudentIdsAsync(studentIds);

            var editedIds = edits.Select(e => e.StudentId).ToHashSet();
            var commentedIds = comments.Select(c => c.StudentId).ToHashSet();
            var pendingDeleteIds = deleteRequests
                .Where(d => d.Status == "pending" && d.StudentId.HasValue)
                .Select(d => d.StudentId!.Value)
                .ToHashSet();
            var pendingReviewIds = pendingReviews.Select(p => p.StudentId).ToHashSet();

            foreach (var item in mapped)
            {
                item.HasFieldEdits = editedIds.Contains(item.Id);
                item.HasFieldComments = commentedIds.Contains(item.Id);
                item.HasPendingDeleteRequest = pendingDeleteIds.Contains(item.Id);
                item.HasPendingReview = pendingReviewIds.Contains(item.Id);
            }

            return new PagedResultDto<EditorStudentListItemDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<StudentResponseDto?> GetByIdAsync(int id) => _studentService.GetStudentByIdAsync(id);

        public Task<StudentResponseDto> RecalculateAsync(int studentId, string editorUsername) =>
            _studentService.RecalculateStudentTotalsAsync(studentId, editorUsername);

        public async Task<StudentResponseDto> SetEligibilityAsync(int studentId, string status, string confirmedBy)
        {
            if (status != "Eligible" && status != "NotEligible")
            {
                throw new ArgumentException("حالة الاستيفاء غير صالحة.");
            }

            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new KeyNotFoundException("الطالب غير موجود.");
            }

            var oldStatus = student.EligibilityStatus;
            student.EligibilityStatus = status;
            student.EligibilityConfirmedBy = confirmedBy;
            student.EligibilityConfirmedAt = DateTime.UtcNow;

            // Logged in the same edits sheet as every other Editor write, so who confirmed
            // eligibility (and when/what it changed from) shows up in the audit log too.
            await _unitOfWork.FieldEdits.AddAsync(new FieldEdit
            {
                StudentId = studentId,
                FieldName = "Student.EligibilityStatus",
                OldValue = oldStatus,
                NewValue = status,
                Editor = confirmedBy,
                Source = "manual",
                EditedAt = DateTime.UtcNow
            });

            await _unitOfWork.CompleteAsync();

            return _mapper.Map<StudentResponseDto>(student);
        }

        public async Task<IEnumerable<AuditLogEntryDto>> GetAuditLogAsync(int studentId)
        {
            var edits = await _unitOfWork.FieldEdits.GetByStudentIdAsync(studentId);
            var comments = await _unitOfWork.FieldComments.GetByStudentIdAsync(studentId);
            var deleteRequests = await _unitOfWork.DeleteRequests.GetByStudentIdAsync(studentId);

            var entries = new List<AuditLogEntryDto>();

            entries.AddRange(edits.Select(e => new AuditLogEntryDto
            {
                Type = "Edit",
                FieldName = e.FieldName,
                OldValue = e.OldValue,
                NewValue = e.NewValue,
                Actor = e.Editor,
                Timestamp = e.EditedAt,
                Note = e.Note,
                Status = e.Source
            }));

            entries.AddRange(comments.Select(c => new AuditLogEntryDto
            {
                Type = "Comment",
                FieldName = c.FieldName,
                OldValue = null,
                NewValue = c.FieldSnapshot,
                Actor = c.Author,
                Timestamp = c.CreatedAt,
                Note = c.CommentText,
                Status = c.Status
            }));

            entries.AddRange(deleteRequests.Select(d => new AuditLogEntryDto
            {
                Type = "DeleteRequest",
                FieldName = string.Empty,
                OldValue = null,
                NewValue = null,
                Actor = d.RequestedBy,
                Timestamp = d.RequestedAt,
                Note = d.Reason,
                Status = d.Status
            }));

            return entries.OrderByDescending(e => e.Timestamp);
        }
    }
}
