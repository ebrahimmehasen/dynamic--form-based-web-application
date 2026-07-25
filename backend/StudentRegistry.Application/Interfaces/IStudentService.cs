using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IStudentService
    {
        Task<StudentResponseDto?> GetStudentByIdAsync(int id);
        Task<StudentResponseDto?> GetStudentByNationalIdAsync(string nationalId);
        Task<IEnumerable<StudentResponseDto>> GetAllStudentsAsync();
        Task<IEnumerable<StudentListItemDto>> SearchStudentsAsync(string? query);
        Task<StudentResponseDto> RegisterStudentAsync(StudentCreateDto createDto);
    }
}
