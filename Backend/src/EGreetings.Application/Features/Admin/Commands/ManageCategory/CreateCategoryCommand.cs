using EGreetings.Application.Features.Categories.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Commands.ManageCategory;

/// <summary>UC27 - Tạo danh mục mới</summary>
public record CreateCategoryCommand(
    string Name,
    string? Description,
    string? IconUrl
) : IRequest<CategoryDto>;
