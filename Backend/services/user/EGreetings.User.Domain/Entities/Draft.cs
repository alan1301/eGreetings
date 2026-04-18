using EGreetings.Shared.Domain;

namespace EGreetings.User.Domain.Entities;

public class Draft : BaseEntity
{
    public int UserId { get; set; }
    public int TemplateId { get; set; }
    public string? Title { get; set; }
    public string? CustomHtml { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientName { get; set; }
    public string? SenderMessage { get; set; }
}
