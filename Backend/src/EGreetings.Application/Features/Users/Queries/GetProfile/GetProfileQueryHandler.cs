using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Users.DTOs;
using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Users.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto>
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Subscription> _subscriptionRepo;

    public GetProfileQueryHandler(IRepository<User> userRepo, IRepository<Subscription> subscriptionRepo)
    {
        _userRepo = userRepo;
        _subscriptionRepo = subscriptionRepo;
    }

    public async Task<UserProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

        // UC19 A3: Lấy trạng thái Subscribe hiện tại
        var activeSubscription = await _subscriptionRepo.Query()
            .Where(s => s.UserId == request.UserId
                && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Pending))
            .OrderByDescending(s => s.ExpiredAt)
            .FirstOrDefaultAsync(cancellationToken);

        string? subscribeStatus = activeSubscription?.Status.ToString();
        DateTime? subscribeExpiredAt = activeSubscription?.ExpiredAt;

        return new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.AvatarUrl,
            user.Role.ToString(),
            user.Status.ToString(),
            user.EmailVerifiedAt,
            user.CreatedAt,
            subscribeStatus,
            subscribeExpiredAt
        );
    }
}
