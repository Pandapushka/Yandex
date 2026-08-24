using FluentAssertions;
using Users.Infrastructure.Security;

namespace Users.Tests
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Fact]
        public void Hash_ReturnsNonEmptyHash()
        {
            var hash = _hasher.Hash("password123");

            hash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Hash_DoesNotContainPlainPassword()
        {
            var hash = _hasher.Hash("password123");

            hash.Should().NotContain("password123");
        }

        [Fact]
        public void Hash_SamePassword_ProducesDifferentHashes()
        {
            var hash1 = _hasher.Hash("password123");
            var hash2 = _hasher.Hash("password123");

            hash1.Should().NotBe(hash2);
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
