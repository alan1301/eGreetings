using EGreetings.Application.Features.Admin.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Admin.Queries.GetCategories;

/// <summary>UC27 - Xem danh sách danh mục (Admin)</summary>
public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
