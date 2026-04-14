using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC03 - Xem danh sách mẫu thiệp | UC04 - Xem chi tiết mẫu thiệp
/// UC12 - Quản lý template (Admin)
/// </summary>
public class GreetingTemplate : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;   // Nội dung HTML thiệp
    public string CssStyle { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsFree { get; set; } = false;
    public int UsageCount { get; set; } = 0;

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<Greeting> Greetings { get; set; } = [];
}
