using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.Common.Models;
using EGreetings.Greeting.Application.DTOs;
using EGreetings.Greeting.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Admin;

public class GetAllGreetingsQueryHandler : IRequestHandler<GetAllGreetingsQuery, PagedResult<GreetingDto>>
{
    private readonly IGreetingDbContext _context;

    public GetAllGreetingsQueryHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GreetingDto>> Handle(GetAllGreetingsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Greeting> query = _context.Greetings
            .Include(g => g.Template);

        if (request.UserId.HasValue)
        {
            query = query.Where(g => g.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<GreetingStatus>(request.Status, ignoreCase: true, out var status))
            {
                query = query.Where(g => g.Status == status);
            }
        }

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
