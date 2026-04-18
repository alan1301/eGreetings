using EGreetings.Identity.Application.Common.Interfaces;
using EGreetings.Identity.Domain.Enums;
using EGreetings.Shared.Contracts.Events.Identity;
using MassTransit;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.BanUser;

public class BanUserCommandHandler : IRequestHandler<BanUserCommand, Unit>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public BanUserCommandHandler(IIdentityDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Unit> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Id == request.TargetUserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (request.IsBanning)
        {
            user.Status = UserStatus.Banned;
            user.BanReason = request.Reason;
        }
        else
        {
            user.Status = UserStatus.Active;
            user.BanReason = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new UserBannedEvent(
                user.Id,
                user.Email,
                request.IsBanning,
                request.Reason,
                DateTime.UtcNow),
            cancellationToken);

        return Unit.Value;
    }
}
