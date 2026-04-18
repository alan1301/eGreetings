using EGreetings.Identity.Application.Common.Interfaces;
using MassTransit;
using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public ForgotPasswordCommandHandler(IIdentityDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            // Silently succeed for security reasons (don't reveal if email exists)
            return Unit.Value;
        }

        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new SendPasswordResetEmailCommand
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            ResetToken = user.PasswordResetToken,
            CorrelationId = Guid.NewGuid()
        }, cancellationToken);

        return Unit.Value;
    }
}

public class SendPasswordResetEmailCommand
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
}
