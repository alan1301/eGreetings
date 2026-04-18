using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Greetings.Queries;

public class GetGreetingByTokenQueryHandler : IRequestHandler<GetGreetingByTokenQuery, GreetingDto>
{
    private readonly IGreetingDbContext _context;

    public GetGreetingByTokenQueryHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<GreetingDto> Handle(GetGreetingByTokenQuery request, CancellationToken cancellationToken)
    {
        var greeting = await _context.Greetings
            .Include(g => g.Template)
            .FirstOrDefaultAsync(g => g.ViewToken == request.Token, cancellationToken)
            ?? throw new KeyNotFoundException($"Greeting with token {request.Token} not found");

        return new GreetingDto(
            greeting.Id,
            greeting.TemplateId,
            greeting.Template?.Name ?? "",
            greeting.RecipientEmail,
            greeting.RecipientName,
            greeting.Status.ToString(),
            greeting.ScheduledAt,
            greeting.SentAt,
            greeting.ViewToken,
            greeting.CreatedAt
        );
    }
}
