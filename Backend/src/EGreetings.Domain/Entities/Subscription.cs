using EGreetings.Domain.Common;
using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC10 - Đăng ký Subscribe | UC11 - Quản lý Subscribe | UC21 - Quản lý User/Subscribe (Admin)
/// UC26 - Gia hạn Subscribe | BR-30: Renew = +30 ngày từ ngày hết hạn
/// State Machine: Pending → Active → Expired → Disabled
/// </summary>
public class Subscription : BaseEntity
{
    public int UserId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpiredAt { get; set; }
    public int MaxRecipients { get; set; } = 10;  // Số người nhận tối đa trong gói

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<SubscriptionRecipient> Recipients { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
