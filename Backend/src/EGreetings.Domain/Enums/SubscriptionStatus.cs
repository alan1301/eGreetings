namespace EGreetings.Domain.Enums;

/// <summary>
/// Trạng thái gói Subscribe - theo Section 2.1 (Subscription State Machine)
/// Pending → Active → Expired → Disabled
/// </summary>
public enum SubscriptionStatus
{
    Pending = 1,   // Chờ thanh toán xác nhận
    Active = 2,    // Đang hoạt động
    Expired = 3,   // Hết hạn (quá ngày ExpiredAt)
    Disabled = 4   // Bị vô hiệu hóa (Admin ban user hoặc hủy thủ công)
}
