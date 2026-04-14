namespace EGreetings.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Inactive = 2,  // Chưa xác thực email
    Banned = 3     // Bị Admin ban
}
