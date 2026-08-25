using Users.Application.Interfaces;

namespace Users.Infrastructure.Security
{
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
