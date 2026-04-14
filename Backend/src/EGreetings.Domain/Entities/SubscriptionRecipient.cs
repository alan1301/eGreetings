using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// Danh sách người nhận trong gói Subscribe - UC11 Quản lý Subscribe
/// </summary>
public class SubscriptionRecipient : BaseEntity
{
    public int SubscriptionId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }     // Dùng cho gửi tự động (UC20)
    public string? Occasion { get; set; }       // Dịp gửi: Birthday, Anniversary, ...
    public bool IsActive { get; set; } = true;

    // Navigation
    public Subscription Subscription { get; set; } = null!;
}
