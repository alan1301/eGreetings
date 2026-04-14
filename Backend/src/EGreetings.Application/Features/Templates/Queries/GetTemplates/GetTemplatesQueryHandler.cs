using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Templates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Templates.Queries.GetTemplates;

public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, List<TemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.GreetingTemplates
            .Include(t => t.Category)
            .Where(t => t.IsActive);

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            query = query.Where(t => t.Name.Contains(request.SearchKeyword) ||
                                     t.Description.Contains(request.SearchKeyword));

        if (request.IsFree.HasValue)
            query = query.Where(t => t.IsFree == request.IsFree.Value);

        return await query
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TemplateDto(
                t.Id, t.CategoryId, t.Category.Name, t.Name, t.Description,
                t.ThumbnailUrl, t.IsFree, t.IsActive, t.UsageCount, t.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
