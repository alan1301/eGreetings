using EGreetings.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Subscriptions.Queries.GetRecipients;

/// <summary>UC11 - Xem danh sách người nhận</summary>
public record GetRecipientsQuery(int UserId, int SubscriptionId) : IRequest<List<SubscriptionRecipientDto>>;
