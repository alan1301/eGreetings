using EGreetings.Domain.Common;
using EGreetings.Domain.Enums;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC08 - Thanh toán (Guest + User) | UC25 - Lịch sử thanh toán | UC26 - Gia hạn Subscribe
/// </summary>
public class Payment : BaseEntity
{
    public int? UserId { get; set; }            // null nếu là Guest
    public int? SubscriptionId { get; set; }    // null nếu thanh toán lẻ (Guest/UC08)
    public string? GuestEmail { get; set; }     // UC08: Guest checkout

    public string TransactionCode { get; set; } = Guid.NewGuid().ToString("N")[..12].ToUpper();
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? PaymentMethod { get; set; }  // VNPay, Momo, Card, ...
    public string? PaymentGatewayRef { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Note { get; set; }

    // Navigation
    public User? User { get; set; }
    public Subscription? Subscription { get; set; }
}
