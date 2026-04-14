using EGreetings.Application.Features.Categories.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Categories.Queries.GetPublicCategories;

/// <summary>UC03 - Xem danh sách danh mục công khai với số lượng template</summary>
public record GetPublicCategoriesQuery : IRequest<List<CategoryDto>>;
