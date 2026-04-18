using MediatR;
using EGreetings.Subscription.Application.DTOs;

namespace EGreetings.Subscription.Application.Features.Subscriptions.Queries;

public record GetMySubscriptionQuery(int UserId) : IRequest<SubscriptionDetailDto?>;
