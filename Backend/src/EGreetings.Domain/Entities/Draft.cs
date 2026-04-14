using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC17 - Lưu bản nháp thiệp
/// </summary>
public class Draft : BaseEntity
{
    public int UserId { get; set; }
    public int TemplateId { get; set; }
    public string Title { get; set; } = "Bản nháp chưa đặt tên";
    public string CustomHtml { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public string? RecipientName { get; set; }
    public string? SenderMessage { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public GreetingTemplate Template { get; set; } = null!;
}
