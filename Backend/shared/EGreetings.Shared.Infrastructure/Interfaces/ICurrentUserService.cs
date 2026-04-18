namespace EGreetings.Shared.Infrastructure.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Role { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
