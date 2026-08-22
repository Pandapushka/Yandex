using FluentAssertions;
using WebApiEvent.Infrastructure.Security;

namespace WebApiEvent.Tests
{
    public class PasswordHasherTests
    {
        private readonly Sha256PasswordHasher _hasher = new();

        [Fact]
        public void Hash_ReturnsHexString()
        {
            var hash = _hasher.Hash("password123");

            hash.Should().NotBeNullOrEmpty();
            hash.Should().HaveLength(64);
        }

        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            var hash = _hasher.Hash("password123");

            _hasher.Verify("password123", hash).Should().BeTrue();
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            var hash = _hasher.Hash("password123");

            _hasher.Verify("wrong-password", hash).Should().BeFalse();
        }
    }
}
