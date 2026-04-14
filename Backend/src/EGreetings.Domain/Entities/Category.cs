using EGreetings.Domain.Common;

namespace EGreetings.Domain.Entities;

/// <summary>
/// UC27 - Quản lý danh mục (Admin) - BR-31: không xóa danh mục có template đang hoạt động
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsSystem { get; set; } = false;  // BR-31: Danh mục hệ thống không xóa được
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public ICollection<GreetingTemplate> Templates { get; set; } = [];
}
