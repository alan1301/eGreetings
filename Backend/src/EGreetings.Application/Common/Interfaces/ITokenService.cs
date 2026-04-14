using EGreetings.Domain.Entities;

namespace EGreetings.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    int? ValidateToken(string token);
}
