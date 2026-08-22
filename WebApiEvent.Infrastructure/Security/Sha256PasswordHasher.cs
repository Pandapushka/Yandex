using System.Security.Cryptography;
using System.Text;
using WebApiEvent.Application.Interfaces;

namespace WebApiEvent.Infrastructure.Security
{
    public class Sha256PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public bool Verify(string password, string passwordHash)
        {
            var hashOfInput = Hash(password);
            return string.Equals(hashOfInput, passwordHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
