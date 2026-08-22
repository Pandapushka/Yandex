using WebApiEvent.Application.Interfaces;

namespace WebApiEvent.Infrastructure.Security
{
    /// <summary>
    /// Хэширование паролей на основе PBKDF2 (Microsoft.AspNetCore.Identity.PasswordHasher).
    /// Соль генерируется случайно для каждого пароля; формат хэша — "{iterations}.{salt}.{subkey}" в base64.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private static readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _identityHasher = new();

        public string Hash(string password)
            => _identityHasher.HashPassword(null!, password);

        public bool Verify(string password, string passwordHash)
            => _identityHasher.VerifyHashedPassword(null!, passwordHash, password)
                != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed;
    }
}
