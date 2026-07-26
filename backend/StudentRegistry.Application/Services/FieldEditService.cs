using AutoMapper;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Editor;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    public class FieldEditService : IFieldEditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FieldEditService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FieldEditResponseDto> ApplyEditAsync(FieldEditCreateDto createDto)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(createDto.StudentId);
            if (student == null)
            {
                throw new KeyNotFoundException("الطالب غير موجود.");
            }

            if (!EditableFieldRegistry.Groups.TryGetValue(createDto.EntityGroup, out var descriptor))
            {
                throw new ArgumentException("مجموعة الحقل غير معروفة.");
            }

            if (!descriptor.EditableProperties.Contains(createDto.PropertyName))
            {
                throw new ArgumentException("هذا الحقل غير قابل للتعديل.");
            }

            object target;
            if (descriptor.IsCollection)
            {
                if (!createDto.EntityRowId.HasValue)
                {
                    throw new ArgumentException("معرف الصف مطلوب لهذا النوع من الحقول.");
                }

                var row = descriptor.GetCollection!(student).FirstOrDefault(r => GetRowId(r) == createDto.EntityRowId.Value);
                if (row == null)
                {
                    throw new KeyNotFoundException("صف الدرجة غير موجود لهذا الطالب.");
                }
                target = row;
            }
            else
            {
                var single = descriptor.GetSingle!(student);
                if (single == null)
                {
                    throw new ArgumentException("هذا الحقل غير موجود لهذا الطالب (نوع شهادة مختلف).");
                }
                target = single;
            }

            var property = descriptor.EntityType.GetProperty(createDto.PropertyName)
                ?? throw new ArgumentException("حقل غير معروف.");

            var oldValue = property.GetValue(target)?.ToString();
            var newValueObj = EditableFieldRegistry.ConvertValue(createDto.NewValue, property.PropertyType);
            property.SetValue(target, newValueObj);

            var fieldName = FieldPath.Format(createDto.EntityGroup, createDto.EntityRowId, createDto.PropertyName);
            var isFromComment = createDto.Source == "from_comment";

            var edit = new FieldEdit
            {
                StudentId = createDto.StudentId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValueObj?.ToString(),
                Note = createDto.Note,
                Source = isFromComment ? "from_comment" : "manual",
                SourceCommentId = isFromComment ? createDto.TriggeringCommentId : null,
                EditedAt = DateTime.UtcNow
            };
            await _unitOfWork.FieldEdits.AddAsync(edit);

            // "Write own edit" (Source == manual) still resolves the comment that triggered it, even
            // though the edit itself isn't linked as having "come from" the comment.
            if (createDto.TriggeringCommentId.HasValue)
            {
                var comment = await _unitOfWork.FieldComments.GetByIdAsync(createDto.TriggeringCommentId.Value);
                if (comment != null)
                {
                    comment.Status = "approved_as_edit";
                    comment.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _unitOfWork.CompleteAsync();

            return _mapper.Map<FieldEditResponseDto>(edit);
        }

        public async Task<IEnumerable<FieldEditResponseDto>> GetForStudentAsync(int studentId)
        {
            var edits = await _unitOfWork.FieldEdits.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<FieldEditResponseDto>>(edits);
        }

        private static int GetRowId(object row) =>
            (int)(row.GetType().GetProperty("Id")!.GetValue(row) ?? 0);
    }
}
