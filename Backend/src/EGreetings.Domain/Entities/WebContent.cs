using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC28 - Quản lý nội dung website (Banner, Footer, About) - Admin
/// Hỗ trợ rollback 10 phiên bản gần nhất
/// </summary>
public class WebContent : BaseEntity
{
    public string Key { get; set; } = string.Empty;       // "banner", "footer", "about", ...
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;   // HTML content
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public int? PreviousVersionId { get; set; }           // Linked list cho rollback
    public int? UpdatedByUserId { get; set; }

    // Navigation
    public WebContent? PreviousVersion { get; set; }
    public User? UpdatedByUser { get; set; }
}
