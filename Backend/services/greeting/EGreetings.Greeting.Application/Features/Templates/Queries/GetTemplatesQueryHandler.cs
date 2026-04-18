using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.Common.Models;
using EGreetings.Greeting.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Templates.Queries;

public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, PagedResult<TemplateDto>>
{
    private readonly IGreetingDbContext _context;

    public GetTemplatesQueryHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.GreetingTemplates
            .Where(t => t.IsActive);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(t =>
                t.Name.Contains(request.Keyword) ||
                t.Description!.Contains(request.Keyword)
            );
        }

        if (request.IsFree.HasValue)
        {
            query = query.Where(t => t.IsFree == request.IsFree.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var templates = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<TemplateDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            Items = templates.Select(t => new TemplateDto(
                t.Id,
                t.CategoryId,
                "", // Will be populated below
                t.Name,
                t.ThumbnailUrl,
                t.IsFree,
                t.IsActive,
                t.UsageCount,
                t.CreatedAt
            )).ToList()
        };

        // Populate category names
        var categoryIds = templates.Select(t => t.CategoryId).Distinct().ToList();
        var categories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        result.Items = result.Items.Select((dto, index) =>
            new TemplateDto(
                dto.Id,
                dto.CategoryId,
                categoryDict.TryGetValue(dto.CategoryId, out var catName) ? catName : "",
                dto.Name,
                dto.ThumbnailUrl,
                dto.IsFree,
                dto.IsActive,
                dto.UsageCount,
                dto.CreatedAt
            )
        ).ToList();

        return result;
    }
}
