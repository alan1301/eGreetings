using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Categories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Categories.Queries.GetPublicCategories;

public class GetPublicCategoriesQueryHandler : IRequestHandler<GetPublicCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetPublicCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.IconUrl,
                c.IsActive,
                c.Templates.Count(t => t.IsActive),
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return categories;
    }
}
