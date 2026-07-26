using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IFieldEditService
    {
        // Throws KeyNotFoundException when the student/row doesn't exist, ArgumentException on an
        // unknown/non-whitelisted field or an unconvertible value.
        Task<FieldEditResponseDto> ApplyEditAsync(FieldEditCreateDto createDto);
        Task<IEnumerable<FieldEditResponseDto>> GetForStudentAsync(int studentId);
    }
}
