using EGreetings.Shared.Domain;

namespace EGreetings.User.Domain.Entities;

public class Contact : BaseEntity
{
    public int UserId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Note { get; set; }
    public string? Group { get; set; }
}
