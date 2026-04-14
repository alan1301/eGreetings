using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Greetings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Greetings.Queries.GetGreetingByToken;

public class GetGreetingByTokenQueryHandler : IRequestHandler<GetGreetingByTokenQuery, GreetingDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetGreetingByTokenQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GreetingDetailDto> Handle(GetGreetingByTokenQuery request, CancellationToken cancellationToken)
    {
        var greeting = await _context.Greetings
            .Include(g => g.Template)
            .FirstOrDefaultAsync(g => g.UniqueToken == request.Token, cancellationToken)
            ?? throw new KeyNotFoundException("Thiệp không tồn tại.");

        // Tăng view count
        greeting.ViewCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return new GreetingDetailDto(
            greeting.Id, greeting.TemplateId, greeting.Template.Name,
            greeting.Template.ThumbnailUrl, greeting.CustomHtml,
            greeting.RecipientName, greeting.RecipientEmail,
            greeting.SenderMessage, greeting.Status.ToString(),
            greeting.ScheduledAt, greeting.SentAt,
            greeting.ViewCount, greeting.UniqueToken, greeting.CreatedAt);
    }
}
