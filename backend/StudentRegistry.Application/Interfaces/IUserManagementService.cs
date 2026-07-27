using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserResponseDto>> GetAllAsync();

        // Throws ArgumentException on validation failure (bad role, duplicate/empty username, weak password).
        Task<UserResponseDto> CreateAsync(UserCreateDto createDto);

        // Throws KeyNotFoundException if the user doesn't exist, InvalidOperationException if it's
        // the protected root admin, ArgumentException on validation failure.
        Task<UserResponseDto> UpdateAsync(int id, UserUpdateDto updateDto);

        // Throws KeyNotFoundException if the user doesn't exist, InvalidOperationException if it's
        // the protected root admin or the currently logged-in user themselves.
        Task DeleteAsync(int id, string currentUsername);

        // Throws KeyNotFoundException if the user doesn't exist, InvalidOperationException if it's
        // the protected root admin, ArgumentException on a weak password.
        Task ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto);
    }
}
