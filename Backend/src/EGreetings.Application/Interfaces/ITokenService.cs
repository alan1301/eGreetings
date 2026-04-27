using EGreetings.Domain.Entities;

namespace EGreetings.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, bool rememberMe = false);
    string GenerateRefreshToken();
    string HashToken(string token);
    Guid? GetUserIdFromToken(string token);
}
