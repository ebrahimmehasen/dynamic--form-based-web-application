using StudentRegistry.Application.DTOs;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IAuthService
    {
        // Real credential verification, session/token issuance, and DB lookups belong in whatever
        // implementation replaces AuthService — the controller and frontend never change.
        Task<LoginResultDto> LoginAsync(LoginRequestDto request);
    }
}
