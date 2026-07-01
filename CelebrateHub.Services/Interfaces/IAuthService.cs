using CelebrateHub.Services.DTOs;

namespace CelebrateHub.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<bool> ChangePasswordAsync(int employeeId, ChangePasswordDto dto);
    }
}