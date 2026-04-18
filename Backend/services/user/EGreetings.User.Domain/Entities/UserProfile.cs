using EGreetings.Shared.Domain;

namespace EGreetings.User.Domain.Entities;

public class UserProfile : BaseEntity
{
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
}
