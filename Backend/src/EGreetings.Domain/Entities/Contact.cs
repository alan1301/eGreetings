using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC16 - Quản lý danh bạ người nhận
/// </summary>
public class Contact : BaseEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Note { get; set; }

    /// <summary>UC16: Nhóm liên hệ — Gia đình / Bạn bè / Đồng nghiệp</summary>
    public string? Group { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
