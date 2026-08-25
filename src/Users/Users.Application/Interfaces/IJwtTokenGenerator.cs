using Users.Domain.Enums;

namespace Users.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string Generate(Guid userId, string login, UserRole role);
    }
}
