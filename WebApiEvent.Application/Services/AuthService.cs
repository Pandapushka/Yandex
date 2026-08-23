using WebApiEvent.Application.DTOs.Auth;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Domain.Entities;
using WebApiEvent.Domain.Enums;
using WebApiEvent.Domain.Exceptions;

namespace WebApiEvent.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task RegisterAsync(
            RegisterRequest request, UserRole role, CancellationToken cancellationToken = default)
        {
            ValidateRegisterRequest(request);

            if (await _userRepository.GetByLoginAsync(request.Login, cancellationToken) != null)
                throw new CustomValidationException("Пользователь с таким логином уже существует");

            var user = User.Create(request.Login, _passwordHasher.Hash(request.Password), role);

            _userRepository.Add(user);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByLoginAsync(request.Login, cancellationToken);

            // Одно сообщение и для неверного логина, и для неверного пароля.
            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new InvalidCredentialsException("Неверный логин или пароль");

            var token = _tokenGenerator.Generate(user.Id, user.Login, user.Role);
            return new LoginResponse(token);
        }

        private static void ValidateRegisterRequest(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login))
                throw new CustomValidationException("Логин обязателен");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new CustomValidationException("Пароль должен содержать минимум 6 символов");
        }
    }
}
