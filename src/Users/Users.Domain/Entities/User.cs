using Users.Domain.Enums;
using Users.Domain.Exceptions;

namespace Users.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Login { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public UserRole Role { get; private set; } = UserRole.User;

        private User() { }

        public static User Create(string login, string passwordHash, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new DomainException("Логин обязателен");
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException("Хеш пароля обязателен");

            return new User
            {
                Id = Guid.NewGuid(),
                Login = login,
                PasswordHash = passwordHash,
                Role = role
            };
        }
    }
}
