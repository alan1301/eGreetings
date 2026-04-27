namespace EGreetings.Application.Interfaces;

public interface IPasswordHasher
{
    /// <summary>BCrypt hash with work factor >= 12 (BR-01, BR-08.4)</summary>
    string Hash(string password);

    bool Verify(string password, string hash);
}
