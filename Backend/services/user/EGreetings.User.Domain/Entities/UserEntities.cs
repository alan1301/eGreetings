using EGreetings.Shared.Domain.Common;

namespace EGreetings.User.Domain.Entities;

public class UserProfile : BaseEntity
{
    public int IdentityUserId { get; set; }  // from Identity Service
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
}

public class Contact : BaseEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class Draft : BaseEntity
{
    public int UserId { get; set; }
    public int TemplateId { get; set; }
    public string? CustomHtml { get; set; }
    public string? SenderMessage { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
}
