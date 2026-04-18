using MediatR;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Queries;

public record GetRecipientsQuery(int UserId, int SubscriptionId) : IRequest<List<SubscriptionRecipientDto>>;
