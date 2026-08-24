using Users.Application.DTOs.Auth;
using Users.Domain.Enums;

namespace Users.Application.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request, UserRole role, CancellationToken cancellationToken = default);
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
