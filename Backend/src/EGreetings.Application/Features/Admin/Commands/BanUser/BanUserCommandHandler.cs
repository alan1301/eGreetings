using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Commands.BanUser;

public class BanUserCommandHandler : IRequestHandler<BanUserCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public BanUserCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<bool> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Không thể ban tài khoản Admin.");

        var oldStatus = user.Status;
        user.Status = request.IsBanning ? UserStatus.Banned : UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        // UC21 A4: Ban user → Disable tất cả Subscription đang Active
        if (request.IsBanning)
        {
            var activeSubscriptions = await _context.Subscriptions
                .Where(s => s.UserId == request.TargetUserId && s.Status == SubscriptionStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var sub in activeSubscriptions)
            {
                sub.Status = SubscriptionStatus.Disabled;
                sub.UpdatedAt = DateTime.UtcNow;

                await _auditService.LogAsync(AuditEventType.SubscriptionDisabled, "Subscription",
                    sub.Id.ToString(), $"Subscription bị Disable do User {user.Email} bị ban.",
                    cancellationToken: cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            request.IsBanning ? AuditEventType.UserBanned : AuditEventType.UserUnbanned,
            "User", user.Id.ToString(),
            $"User {user.Email} {(request.IsBanning ? "bị ban" : "được unban")}. Lý do: {request.Reason}",
            oldValue: oldStatus.ToString(), newValue: user.Status.ToString(),
            cancellationToken: cancellationToken);

        return true;
    }
}
