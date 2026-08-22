using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEvent.Domain.Enums;

namespace WebApiEvent.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string Generate(Guid userId, string login, UserRole role);
    }
}
