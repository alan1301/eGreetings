using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.Common.Models;
using EGreetings.Greeting.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Greetings.Queries;

public class GetUserGreetingsQueryHandler : IRequestHandler<GetUserGreetingsQuery, PagedResult<GreetingDto>>
{
    private readonly IGreetingDbContext _context;

    public GetUserGreetingsQueryHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GreetingDto>> Handle(GetUserGreetingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Greetings
            .Where(g => g.UserId == request.UserId)
            .Include(g => g.Template);

        var totalCount = await query.CountAsync(cancellationToken);

        var greetings = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<GreetingDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            Items = greetings.Select(g => new GreetingDto(
                g.Id,
                g.TemplateId,
                g.Template?.Name ?? "",
                g.RecipientEmail,
                g.RecipientName,
                g.Status.ToString(),
                g.ScheduledAt,
                g.SentAt,
                g.ViewToken,
                g.CreatedAt
            )).ToList()
        };

        return result;
    }
}
