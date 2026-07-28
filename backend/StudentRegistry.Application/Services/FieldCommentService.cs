using AutoMapper;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Editor;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    public class FieldCommentService : IFieldCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FieldCommentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FieldCommentResponseDto> AddCommentAsync(FieldCommentCreateDto createDto, string authorUsername)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(createDto.StudentId);
            if (student == null)
            {
                throw new KeyNotFoundException("الطالب غير موجود.");
            }

            if (!EditableFieldRegistry.Groups.ContainsKey(createDto.EntityGroup))
            {
                throw new ArgumentException("مجموعة الحقل غير معروفة.");
            }

            var fieldName = FieldPath.Format(createDto.EntityGroup, createDto.EntityRowId, createDto.PropertyName);

            var comment = new FieldComment
            {
                StudentId = createDto.StudentId,
                FieldName = fieldName,
                FieldSnapshot = createDto.FieldSnapshot,
                CommentText = createDto.CommentText,
                Author = string.IsNullOrWhiteSpace(authorUsername) ? "Editor" : authorUsername,
                Status = "unreviewed",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.FieldComments.AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<FieldCommentResponseDto>(comment);
        }

        public async Task<IEnumerable<FieldCommentResponseDto>> GetForStudentAsync(int studentId)
        {
            var comments = await _unitOfWork.FieldComments.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<FieldCommentResponseDto>>(comments);
        }

        public async Task<IEnumerable<FieldCommentResponseDto>> GetUnreviewedAsync()
        {
            var comments = await _unitOfWork.FieldComments.GetUnreviewedAsync();
            return _mapper.Map<IEnumerable<FieldCommentResponseDto>>(comments);
        }

        public async Task<IEnumerable<FieldCommentResponseDto>> GetResolvedAsync()
        {
            var comments = await _unitOfWork.FieldComments.GetResolvedAsync();
            return _mapper.Map<IEnumerable<FieldCommentResponseDto>>(comments);
        }

        public Task<int> GetUnreviewedCountAsync() => _unitOfWork.FieldComments.GetUnreviewedCountAsync();

        public async Task<FieldCommentResponseDto?> DismissAsync(int id)
        {
            var comment = await _unitOfWork.FieldComments.GetByIdAsync(id);
            if (comment == null)
            {
                return null;
            }

            comment.Status = "dismissed";
            comment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<FieldCommentResponseDto>(comment);
        }
    }
}
