using EGreetings.Application.Interfaces;

namespace EGreetings.Infrastructure.Services;

/// <summary>
/// BCrypt password hasher (BR-01: work factor >= 12).
/// </summary>
public class PasswordHasherService : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
