using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Greetings.DTOs;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetGreetings;

public class GetAllGreetingsQueryHandler : IRequestHandler<GetAllGreetingsQuery, PagedResult<GreetingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllGreetingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GreetingDto>> Handle(GetAllGreetingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Greetings
            .Include(g => g.Template)
            .AsQueryable();

        // Filter by user if provided
        if (request.UserId.HasValue)
            query = query.Where(g => g.UserId == request.UserId.Value);

        // Filter by status if provided
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<GreetingStatus>(request.Status, true, out var status))
                query = query.Where(g => g.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(g => new GreetingDto(
                g.Id,
                g.TemplateId,
                g.Template.Name,
                g.RecipientName,
                g.RecipientEmail,
                g.SenderMessage,
                g.Status.ToString(),
                g.ScheduledAt,
                g.SentAt,
                g.ViewCount,
                g.UniqueToken,
                g.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<GreetingDto>(items, total, request.Page, request.PageSize);
    }
}
