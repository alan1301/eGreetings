using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Categories.Queries;

public class GetPublicCategoriesQueryHandler : IRequestHandler<GetPublicCategoriesQuery, List<CategoryDto>>
{
    private readonly IGreetingDbContext _context;

    public GetPublicCategoriesQueryHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetPublicCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        var result = new List<CategoryDto>();
        foreach (var category in categories)
        {
            var templateCount = await _context.GreetingTemplates
                .CountAsync(t => t.CategoryId == category.Id && t.IsActive, cancellationToken);

            result.Add(new CategoryDto(
                category.Id,
                category.Name,
                category.Description,
                category.IconUrl,
                category.IsActive,
                templateCount,
                category.CreatedAt
            ));
        }

        return result;
    }
}
