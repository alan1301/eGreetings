using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Subscriptions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Subscriptions.Queries.GetRecipients;

public class GetRecipientsQueryHandler : IRequestHandler<GetRecipientsQuery, List<SubscriptionRecipientDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecipientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriptionRecipientDto>> Handle(GetRecipientsQuery request, CancellationToken cancellationToken)
    {
        // Verify subscription belongs to user
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == request.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Gói Subscribe không tồn tại hoặc không có quyền truy cập.");

        var recipients = await _context.SubscriptionRecipients
            .Where(r => r.SubscriptionId == request.SubscriptionId)
            .Select(r => new SubscriptionRecipientDto(
                r.Id,
                r.Email,
                r.Name,
                r.Birthday,
                r.Occasion,
                r.IsActive
            ))
            .OrderByDescending(r => r.IsActive)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return recipients;
    }
}
