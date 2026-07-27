using AutoMapper;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    public class PendingReviewService : IPendingReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PendingReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PendingReviewResponseDto> FlagAsync(int studentId, string flaggedBy)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new KeyNotFoundException("الطالب غير موجود.");
            }

            var existingPending = await _unitOfWork.PendingReviews.GetPendingForStudentAsync(studentId);
            if (existingPending != null)
            {
                throw new InvalidOperationException("هذا الطالب موضوع بالفعل قيد المراجعة.");
            }

            var review = new PendingReview
            {
                StudentId = studentId,
                FlaggedBy = string.IsNullOrWhiteSpace(flaggedBy) ? "User" : flaggedBy,
                FlaggedAt = DateTime.UtcNow,
                Status = "pending"
            };

            await _unitOfWork.PendingReviews.AddAsync(review);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PendingReviewResponseDto>(review);
        }

        public async Task<PendingReviewResponseDto> ResolveAsync(int studentId, string resolvedBy)
        {
            var pending = await _unitOfWork.PendingReviews.GetPendingForStudentAsync(studentId);
            if (pending == null)
            {
                throw new KeyNotFoundException("لا يوجد لهذا الطالب حالة قيد المراجعة لإزالتها.");
            }

            pending.Status = "resolved";
            pending.ResolvedBy = string.IsNullOrWhiteSpace(resolvedBy) ? "Editor" : resolvedBy;
            pending.ResolvedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PendingReviewResponseDto>(pending);
        }

        public async Task<IEnumerable<PendingReviewResponseDto>> GetForStudentAsync(int studentId)
        {
            var reviews = await _unitOfWork.PendingReviews.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<PendingReviewResponseDto>>(reviews);
        }
    }
}
