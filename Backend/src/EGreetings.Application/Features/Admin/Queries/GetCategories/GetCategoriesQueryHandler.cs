using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IRepository<Category> _categoryRepo;

    public GetCategoriesQueryHandler(IRepository<Category> categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepo.Query()
            .Include(c => c.Templates)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Description, c.IconUrl,
                c.IsSystem, c.IsActive, c.DisplayOrder,
                c.Templates.Count(t => t.IsActive),
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return categories;
    }
}
