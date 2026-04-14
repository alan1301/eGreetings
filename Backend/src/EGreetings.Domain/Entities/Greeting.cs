using EGreetings.Domain.Common;
using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC05 - Tùy chỉnh thiệp | UC06 - Gửi thiệp | UC07 - Xem thiệp đã gửi
/// UC09 - Xem thiệp (người nhận) | UC20 - Gửi thiệp tự động | UC24 - Xem chi tiết thiệp (Guest)
/// </summary>
public class Greeting : BaseEntity
{
    public int? UserId { get; set; }               // null nếu là Guest (UC08)
    public int TemplateId { get; set; }
    public string? GuestSenderEmail { get; set; }   // UC08: Guest checkout
    public string? GuestSenderName { get; set; }

    public string SenderMessage { get; set; } = string.Empty;
    public string CustomHtml { get; set; } = string.Empty;   // Sau khi tùy chỉnh

    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;

    public GreetingStatus Status { get; set; } = GreetingStatus.Draft;
    public DateTime? ScheduledAt { get; set; }    // UC06/UC20: lên lịch gửi
    public DateTime? SentAt { get; set; }

    // BR-28: Reply-To header = email người gửi
    public string? ReplyToEmail { get; set; }

    // Unique link để người nhận xem thiệp (UC09)
    public string UniqueToken { get; set; } = Guid.NewGuid().ToString("N");
    public int ViewCount { get; set; } = 0;

    // Navigation
    public User? User { get; set; }
    public GreetingTemplate Template { get; set; } = null!;
    public ICollection<GreetingTransaction> Transactions { get; set; } = [];
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
}
