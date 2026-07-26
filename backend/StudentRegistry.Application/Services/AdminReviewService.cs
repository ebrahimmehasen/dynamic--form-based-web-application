using AutoMapper;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    public class AdminReviewService : IAdminReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FieldEditResponseDto>> GetFieldEditsAsync(bool fromCommentOnly)
        {
            var edits = await _unitOfWork.FieldEdits.GetAllAsync(fromCommentOnly);
            return _mapper.Map<IEnumerable<FieldEditResponseDto>>(edits);
        }

        public async Task<IEnumerable<FieldCommentResponseDto>> GetFieldCommentsAsync()
        {
            var comments = await _unitOfWork.FieldComments.GetAllAsync();
            return _mapper.Map<IEnumerable<FieldCommentResponseDto>>(comments);
        }

        public async Task<IEnumerable<DeleteRequestResponseDto>> GetDeleteRequestsAsync(string? status)
        {
            var requests = await _unitOfWork.DeleteRequests.GetAllAsync(status);
            return _mapper.Map<IEnumerable<DeleteRequestResponseDto>>(requests);
        }

        public async Task<DeleteRequestResponseDto?> ApproveDeleteAsync(int id)
        {
            var request = await _unitOfWork.DeleteRequests.GetByIdAsync(id);
            if (request == null || request.Status != "pending")
            {
                return null;
            }

            if (request.StudentId.HasValue)
            {
                var student = await _unitOfWork.Students.GetByIdAsync(request.StudentId.Value);
                if (student != null)
                {
                    // Cascades through the existing cert-table FKs (SaudiStudentTotals, StandardStudentGrades,
                    // etc.) — no new deletion logic needed. FieldEdits/FieldComments cascade too; this
                    // DeleteRequests row survives via its own SetNull FK (see DeleteRequestConfiguration).
                    _unitOfWork.Students.Delete(student);
                }
            }

            // TODO: replace hardcoded "Admin" with the logged-in username once per-user identity is
            // wired through these write paths (mirrors the same placeholder already used elsewhere).
            request.Status = "approved";
            request.ReviewedBy = "Admin";
            request.ReviewedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<DeleteRequestResponseDto>(request);
        }

        public async Task<DeleteRequestResponseDto?> RejectDeleteAsync(int id)
        {
            var request = await _unitOfWork.DeleteRequests.GetByIdAsync(id);
            if (request == null || request.Status != "pending")
            {
                return null;
            }

            // TODO: replace hardcoded "Admin" with the logged-in username once per-user identity is
            // wired through these write paths.
            request.Status = "rejected";
            request.ReviewedBy = "Admin";
            request.ReviewedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<DeleteRequestResponseDto>(request);
        }
    }
}
