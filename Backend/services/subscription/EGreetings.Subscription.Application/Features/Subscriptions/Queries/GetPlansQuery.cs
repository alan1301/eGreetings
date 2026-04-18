using EGreetings.Subscription.Application.Common.Interfaces;
using EGreetings.Subscription.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Queries;

public record GetPlansQuery : IRequest<List<PlanDto>>;

public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, List<PlanDto>>
{
    private readonly ISubscriptionDbContext _context;

    public GetPlansQueryHandler(ISubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.PricePerYear)
            .Select(p => new PlanDto(
                p.Id,
                p.Name,
                p.Description,
                p.MaxRecipients,
                p.PricePerYear,
                p.IsActive
            ))
            .ToListAsync(cancellationToken);

        return plans;
    }
}
