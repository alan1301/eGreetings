using EGreetings.Application.Features.Categories.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.ManageCategory;

/// <summary>UC27 - Cập nhật danh mục</summary>
public record UpdateCategoryCommand(
    int Id,
    string? Name,
    string? Description,
    string? IconUrl,
    bool? IsActive
) : IRequest<CategoryDto>;
