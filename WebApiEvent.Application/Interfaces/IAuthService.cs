using WebApiEvent.Application.DTOs.Auth;
using WebApiEvent.Domain.Enums;

namespace WebApiEvent.Application.Interfaces
{
    /// <summary>Сценарии регистрации и входа.</summary>
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request, UserRole role, CancellationToken cancellationToken = default);
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
