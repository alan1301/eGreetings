using MediatR;

namespace EGreetings.Application.Commands.Auth.Login;

public record LoginCommand(
    string Email,
    string Password,
    bool RememberMe = false
) : IRequest<LoginResult>;

public record LoginResult(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,       // Frontend uses this to compute expiresIn
    int FailedLoginCount      // for CAPTCHA trigger at >= 3 (BR-04)
)
{
    // Alias so frontend can access as { token } directly
    public string Token => AccessToken;
};
