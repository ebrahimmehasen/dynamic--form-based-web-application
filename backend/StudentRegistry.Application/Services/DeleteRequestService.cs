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
    public class DeleteRequestService : IDeleteRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteRequestService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DeleteRequestResponseDto> CreateAsync(DeleteRequestCreateDto createDto)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(createDto.StudentId);
            if (student == null)
            {
                throw new KeyNotFoundException("الطالب غير موجود.");
            }

            var existingPending = await _unitOfWork.DeleteRequests.GetPendingForStudentAsync(createDto.StudentId);
            if (existingPending != null)
            {
                throw new InvalidOperationException("يوجد بالفعل طلب حذف قيد المراجعة لهذا الطالب.");
            }

            var request = new DeleteRequest
            {
                StudentId = createDto.StudentId,
                Reason = createDto.Reason,
                Status = "pending",
                RequestedAt = DateTime.UtcNow
            };

            await _unitOfWork.DeleteRequests.AddAsync(request);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DeleteRequestResponseDto>(request);
        }

        public async Task<IEnumerable<DeleteRequestResponseDto>> GetForStudentAsync(int studentId)
        {
            var requests = await _unitOfWork.DeleteRequests.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<DeleteRequestResponseDto>>(requests);
        }
    }
}
