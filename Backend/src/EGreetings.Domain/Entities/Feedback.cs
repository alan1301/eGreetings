using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC13 - Gửi phản hồi / báo cáo
/// </summary>
public class Feedback : BaseEntity
{
    public int? UserId { get; set; }     // null nếu ẩn danh
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }

    /// <summary>UC07: Đánh giá sao 1-5 (tùy chọn) — BR-13</summary>
    public int? StarRating { get; set; }   // 1–5, nullable = chưa đánh giá

    public bool IsRead { get; set; } = false;
    public string? AdminReply { get; set; }
    public DateTime? RepliedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}
