using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEvent.Domain.Enums;
using WebApiEvent.Domain.Exceptions;

namespace WebApiEvent.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Login { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public UserRole Role { get; private set; } = UserRole.User;
        public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
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
