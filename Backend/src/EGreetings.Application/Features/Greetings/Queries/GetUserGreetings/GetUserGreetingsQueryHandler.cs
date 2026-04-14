using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Greetings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Greetings.Queries.GetUserGreetings;

public class GetUserGreetingsQueryHandler : IRequestHandler<GetUserGreetingsQuery, List<GreetingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserGreetingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GreetingDto>> Handle(GetUserGreetingsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Greetings
            .Include(g => g.Template)
            .Where(g => g.UserId == request.UserId)
            .OrderByDescending(g => g.CreatedAt)
            .ThenBy(g => g.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(g => new GreetingDto(
                g.Id, g.TemplateId, g.Template.Name,
                g.RecipientName, g.RecipientEmail, g.SenderMessage,
                g.Status.ToString(), g.ScheduledAt, g.SentAt,
                g.ViewCount, g.UniqueToken, g.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
