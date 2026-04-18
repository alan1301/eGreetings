using EGreetings.Identity.Domain.Entities;

namespace EGreetings.Identity.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
